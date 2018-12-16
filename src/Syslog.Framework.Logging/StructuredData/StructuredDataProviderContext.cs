namespace Syslog.Framework.Logging.StructuredData
{
	/// <summary>
	/// An object which contains data potentially helpful to a <see cref="IStructuredDataProvider"/>.
	/// </summary>
	public class StructuredDataProviderContext<TLogData>
	{
		internal StructuredDataProviderContext(LogRequest<TLogData> originalLogData)
		{
			OriginalLogData = originalLogData;
		}

		/// <summary>
		/// Original log request as passed to a logger.
		/// </summary>
		public LogRequest<TLogData> OriginalLogData { get; }
	}
}