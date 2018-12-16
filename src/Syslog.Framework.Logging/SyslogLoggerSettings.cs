using System.Collections.Generic;
using Syslog.Framework.Logging.StructuredData;

namespace Syslog.Framework.Logging
{
	public class SyslogLoggerSettings
	{
		#region Fields and Methods

		/// <summary>
		/// Gets or sets the host for the Syslog server.
		/// </summary>
		public string ServerHost { get; set; } = "127.0.0.1";

		/// <summary>
		/// Gets or sets the port for the Syslog server.
		/// </summary>
		public int ServerPort { get; set; } = 514;

		/// <summary>
		/// Gets or sets the facility type.
		/// </summary>
		public FacilityType FacilityType { get; set; } = FacilityType.Local0;

		/// <summary>
		/// Gets or sets the header type. Set this instead of HeaderFormat.
		/// </summary>
		public SyslogHeaderType HeaderType { get; set; } = SyslogHeaderType.Rfc3164; // Default to 3164 to be backwards compatible with v1.

		/// <summary>
		/// Structured data that is sent with every request. Only for RFC 5424.
		/// </summary>
		/// <seealso cref="StructuredDataProvider"/>
		public IEnumerable<SyslogStructuredData> StructuredData { get; set; }

		/// <summary>
		/// Gets or sets whether to log messages using UTC or local time. Defaults to false (use local time).
		/// </summary>
		public bool UseUtc { get; set; } = false; // Default to false to be backwards compatible with v1.

		/// <summary>
		/// A provider for dynamic structured data which can change per log message such as correlation id or logged in user login.
		/// </summary>
		/// <seealso cref="StructuredData"/>
		public IStructuredDataProvider StructuredDataProvider { get; set; }
		
		#endregion
	}
}
