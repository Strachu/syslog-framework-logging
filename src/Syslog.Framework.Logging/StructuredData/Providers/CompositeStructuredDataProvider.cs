using System.Collections.Generic;
using System.Linq;

namespace Syslog.Framework.Logging.StructuredData.Providers
{
	/// <summary>
	/// A provider which merges results from multiple structured data providers.
	/// </summary>
	internal class CompositeStructuredDataProvider : IStructuredDataProvider
	{
		private readonly IReadOnlyList<IStructuredDataProvider> _providers;

		public CompositeStructuredDataProvider(IReadOnlyList<IStructuredDataProvider> providers)
		{
			_providers = providers;
		}

		public IEnumerable<SyslogStructuredData> GetStructuredDataForLogRequest<TLogData>(StructuredDataProviderContext<TLogData> context)
		{
			var uniqueStructuredDataEntries = new Dictionary<string, SyslogStructuredData>(capacity: _providers.Count);
			var allEntries = _providers.SelectMany(x => x.GetStructuredDataForLogRequest(context));

			foreach (var dataEntry in allEntries)
			{
				uniqueStructuredDataEntries[dataEntry.Id] = dataEntry;
			}

			return uniqueStructuredDataEntries.Values;
		}
	}
}