using System.Collections.Generic;

namespace Syslog.Framework.Logging
{
	/// <summary>
	/// Allows sending of structured data in RFC 5424.
	/// </summary>
	public class SyslogStructuredData
	{
		/// <summary>
		/// Creates an instance of SyslogStructuredData.
		/// </summary>
		public SyslogStructuredData()
		{
		}

		/// <summary>
		/// Creates an instance of SyslogStructuredData.
		/// </summary>
		/// <param name="id"></param>
		public SyslogStructuredData(string id)
		{
			Id = id;
		}

		/// <summary>
		/// Gets the ID for the structured data.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets the list of structured data elements.
		/// </summary>
		public IEnumerable<SylogStructuredDataElement> Elements { get; set; }
	}

	/// <summary>
	/// A named value for structured data.
	/// </summary>
	public class SylogStructuredDataElement
	{
		/// <summary>
		/// Creates an instance of SylogStructuredDataElement.
		/// </summary>
		public SylogStructuredDataElement()
		{
		}

		/// <summary>
		/// Creates an instance of SylogStructuredDataElement.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public SylogStructuredDataElement(string name, string value)
		{
			Name = name;
			Value = value;
		}

		/// <summary>
		/// Gets the name of the element.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the value of the element.
		/// </summary>
		public string Value { get; set; }
	}
}