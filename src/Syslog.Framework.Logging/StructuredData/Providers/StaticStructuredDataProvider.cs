using System.Collections.Generic;

namespace Syslog.Framework.Logging.StructuredData.Providers
{
	/// <summary>
	/// A provider which returns static structured data.
	/// </summary>
	internal class StaticStructuredDataProvider : IStructuredDataProvider
	{
		private readonly IReadOnlyList<SyslogStructuredData> _structuredData;

		public StaticStructuredDataProvider(IReadOnlyList<SyslogStructuredData> structuredData)
		{
			_structuredData = structuredData ?? new List<SyslogStructuredData>();
		}

		public IEnumerable<SyslogStructuredData> GetStructuredDataForLogRequest<TLogData>(StructuredDataProviderContext<TLogData> context)
		{
			return _structuredData;
		}
	}
}