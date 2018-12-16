using System.Collections.Generic;

namespace Syslog.Framework.Logging.StructuredData
{
	/// <summary>
	/// A provider for structured data which can change during application lifetime such as executed task sequence id or logged in user login.
	/// </summary>
	public interface IStructuredDataProvider
	{
		IEnumerable<SyslogStructuredData> GetStructuredDataForLogRequest<TLogData>(StructuredDataProviderContext<TLogData> context);
	}
}