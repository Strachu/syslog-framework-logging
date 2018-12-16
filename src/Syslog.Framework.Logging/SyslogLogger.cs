using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Syslog.Framework.Logging.StructuredData;

namespace Syslog.Framework.Logging
{
	public abstract class SyslogLogger : ILogger
	{
		private readonly string _name;
		private readonly string _host;
		private readonly LogLevel _lvl;
		private readonly SyslogLoggerSettings _settings;

		public SyslogLogger(string name, SyslogLoggerSettings settings, string host, LogLevel lvl)
		{
			_name = name;
			_settings = settings;
			_host = host;
			_lvl = lvl;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel != LogLevel.None && logLevel >= _lvl;
		}

		public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));

			if (!IsEnabled(logLevel))
                return;

			string message = formatter(state, exception);

			if (String.IsNullOrEmpty(message))
                return;

			var originalRequest = new LogRequest<TState>(eventId, _name, logLevel, state, exception);
			
			// Defined in RFC 5424, section 6.2.1, and RFC 3164, section 4.1.1.
			// If a different value is needed, then this code should probably move into the specific loggers.
			var severity = MapToSeverityType(logLevel);
			var priority = ((int)_settings.FacilityType * 8) + (int)severity;
			var procid = GetProcID();
			var now = _settings.UseUtc ? DateTime.UtcNow : DateTime.Now;
			var msg = FormatMessage(originalRequest, priority, now, _host, _name, procid, eventId.Id, message);
			var raw = Encoding.ASCII.GetBytes(msg);

			using (var udp = new UdpClient())
			{
				udp.Send(raw, raw.Length, _settings.ServerHost, _settings.ServerPort);
			}
		}

		[Obsolete("Remains for backward compatibility only. Will be removed in future. Override the other method overload")]
		protected virtual string FormatMessage(int priority, DateTime now, string host, string name, int procid, int msgid, string message)
		{
			throw new NotImplementedException($"You have to provide implementation for a {nameof(FormatMessage)} method.");
		}

		protected virtual string FormatMessage<TLogData>(LogRequest<TLogData> request, int priority, DateTime now, string host, string name, int procid, int msgid, string message)
		{
#pragma warning disable 618
			return FormatMessage(priority, now, host, name, procid, msgid, message);
#pragma warning restore 618
		}
		
		private int? _procID;
		private int GetProcID()
		{
			if (_procID == null)
			{
				try
				{
					// Attempt to get the process ID. This might not work on all platforms.
					_procID = Process.GetCurrentProcess().Id;
				}
				catch
				{
					// If we can't get it, just default to 0.
					_procID = 0;
				}
			}

			return _procID.Value;
		}

		internal virtual SeverityType MapToSeverityType(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Information:
					return SeverityType.Informational;
				case LogLevel.Warning:
					return SeverityType.Warning;
				case LogLevel.Error:
					return SeverityType.Error;
				case LogLevel.Critical:
					return SeverityType.Critical;
				default:
					return SeverityType.Debug;
			}
		}
	}

	/// <summary>
	/// Based on RFC 3164: https://tools.ietf.org/html/rfc3164
	/// </summary>
	public class Syslog3164Logger : SyslogLogger
	{
		public Syslog3164Logger(string name, SyslogLoggerSettings settings, string host, LogLevel lvl)
			: base(name, settings, host, lvl)
		{
		}

		protected override string FormatMessage<TLogData>(LogRequest<TLogData> request, int priority, DateTime now, string host, string name, int procid, int msgid, string message)
		{
            var tag = name.Replace(".", String.Empty).Replace("_", String.Empty); // Alphanumeric
            tag = tag.Substring(0, Math.Min(32, tag.Length)); // Max length is 32 according to spec
            return $"<{priority}>{now:MMM dd HH:mm:ss} {host} {tag} {message}";
		}
	}

	/// <summary>
	/// Based on RFC 5424: https://tools.ietf.org/html/rfc5424
	/// </summary>
	public class Syslog5424v1Logger : SyslogLogger
	{
		private readonly IStructuredDataProvider _structuredDataProvider;
		
		public Syslog5424v1Logger(string name, SyslogLoggerSettings settings, string host, LogLevel lvl, IStructuredDataProvider structuredDataProvider)
			: base(name, settings, host, lvl)
		{
			_structuredDataProvider = structuredDataProvider;
		}

		protected override string FormatMessage<TLogData>(LogRequest<TLogData> request, int priority, DateTime now, string host, string name, int procid, int msgid, string message)
		{
			var providerContext = new StructuredDataProviderContext<TLogData>(request);
			var structuredData = _structuredDataProvider?.GetStructuredDataForLogRequest(providerContext)?.ToList() ?? new List<SyslogStructuredData>();

			var formattedStructuredData = FormatStructuredData(structuredData) ?? String.Empty;
			return $"<{priority}>1 {now:o} {host} {name} {procid} {msgid} {formattedStructuredData} {message}";
		}

		private string FormatStructuredData(IReadOnlyCollection<SyslogStructuredData> structuredData)
		{
			if (structuredData == null)
				return null;

			if (!structuredData.Any())
				return null;
			
			var sb = new StringBuilder();

			foreach (var data in structuredData)
			{
				if (!IsValidPrintAscii(data.Id, '=', ' ', ']', '"'))
					throw new InvalidOperationException($"ID for structured data {data.Id} is not valid. US Ascii 33-126 only, except '=', ' ', ']', '\"'");

				sb.Append($"[{data.Id}");

				if (data.Elements != null)
				{
					foreach (var element in data.Elements)
					{
						if (!IsValidPrintAscii(element.Name, '=', ' ', ']', '"'))
							throw new InvalidOperationException($"Element {element.Name} in structured data {data.Id} is not valid. US Ascii 33-126 only, except '=', ' ', ']', '\"'");

						// According to spec, need to escape these characters.
						var val = element.Value
							.Replace("\\", "\\\\")
							.Replace("\"", "\\\"")
							.Replace("]", "\\]");
						sb.Append($" {element.Name}=\"{val}\"");
					}
				}

				sb.Append("]");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Based on spec, section 6.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private bool IsValidPrintAscii(string name, params char[] invalid)
		{
			if (String.IsNullOrEmpty(name))
				return false;

			foreach (var ch in name)
			{
				if (ch < 33)
					return false;
				if (ch > 126)
					return false;
				if (invalid.Contains(ch))
					return false;
			}

			return true;
		}
	}
}
