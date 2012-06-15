using System.Configuration;

namespace Graphite
{
	/// <summary>
	/// Configuration section for StatsD
	/// </summary>
	public class StatsDConfigurationSection
		: ConfigurationSection
	{
		const string HostKey = "host";
		const string PortKey = "port";
		const string KeyPrefixKey = "keyPrefix";

		private T Get<T>(string key)
		{
			return (T) this[key];
		}

		[ConfigurationProperty(HostKey, DefaultValue = "localhost", IsRequired = false)]
		public string Host
		{
			get { return Get<string>(HostKey); }
		}

		[ConfigurationProperty(PortKey, DefaultValue = "8125", IsRequired = false)]
		public uint Port
		{
			get { return Get<uint>(PortKey); }
		}

		[ConfigurationProperty(KeyPrefixKey, DefaultValue = "", IsRequired = false)]
		public string KeyPrefix
		{
			get { return Get<string>(KeyPrefixKey); }
		}
	}
}