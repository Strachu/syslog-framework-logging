﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Syslog.Framework.Logging.TransportProtocols;
using Syslog.Framework.Logging.StructuredData;
using Syslog.Framework.Logging.StructuredData.Providers;

namespace Syslog.Framework.Logging
{
	public class SyslogLoggerProvider : ILoggerProvider
	{
		private readonly SyslogLoggerSettings _settings;
		private readonly string _hostName;
		private readonly LogLevel _logLevel;
		private readonly IDictionary<string, ILogger> _loggers;
		private readonly IMessageSender _messageSender;
		private readonly IStructuredDataProvider _structuredDataProvider;

		public SyslogLoggerProvider(SyslogLoggerSettings settings, string hostName, LogLevel logLevel)
		{
			_settings = settings;
			_hostName = hostName;
			_logLevel = logLevel;
			_messageSender = settings.CustomMessageSender ?? MessageSenderFactory.CreateFromSettings(settings);
			_loggers = new Dictionary<string, ILogger>();

			var staticStructuredDataProvider = new StaticStructuredDataProvider(settings.StructuredData?.ToList());
			var allProviders = new[] {staticStructuredDataProvider}.Concat(settings.StructuredDataProviders).ToList();
			
			_structuredDataProvider = new CompositeStructuredDataProvider(allProviders);
		}
		
		public ILogger CreateLogger(string name)
		{
			if (!_loggers.ContainsKey(name))
				_loggers[name] = CreateLoggerInstance(name);

			return _loggers[name];
		}

		private ILogger CreateLoggerInstance(string name)
		{
			switch (_settings.HeaderType)
			{
				case SyslogHeaderType.Rfc3164:
					return new Syslog3164Logger(name, _settings, _hostName, _logLevel, _messageSender);
				case SyslogHeaderType.Rfc5424v1:
					return new Syslog5424v1Logger(name, _settings, _hostName, _logLevel, _messageSender, _structuredDataProvider);
				default:
					throw new InvalidOperationException($"SyslogHeaderType '{_settings.HeaderType.ToString()}' is not recognized.");
			}
		}

		public ILogger CreateLogger<T>()
		{
			return CreateLogger(typeof(T).FullName);
		}

		public void Dispose()
		{
			_loggers.Clear();
		}
	}
}
