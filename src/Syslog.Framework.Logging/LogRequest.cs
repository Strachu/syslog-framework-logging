using System;
using Microsoft.Extensions.Logging;

namespace Syslog.Framework.Logging
{
	/// <summary>
	/// Original log request as received by logger.
	/// </summary>
	public class LogRequest<TData>
	{
		public LogRequest(EventId eventId, string categoryName, LogLevel logLevel, TData data, Exception exception)
		{
			EventId = eventId;
			CategoryName = categoryName;
			LogLevel = logLevel;
			Data = data;
			Exception = exception;
		}

		public EventId EventId { get; }
		
		public string CategoryName { get; }
		
		public LogLevel LogLevel { get; }
		
		public TData Data { get; }
		
		public Exception Exception { get; }
	}
}