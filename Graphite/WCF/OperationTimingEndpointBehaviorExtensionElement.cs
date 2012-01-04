using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using Graphite.StatsD;

namespace Graphite.WCF
{
	public class OperationTimingEndpointBehaviorExtensionElement : BehaviorExtensionElement
	{
		[ConfigurationProperty("hostname", IsRequired = true)]
		public string Hostname
		{
			get { return (string) base["hostname"]; }
		}

		[ConfigurationProperty("port", DefaultValue = 8125)]
		public int Port
		{
			get { return (int) base["port"]; }
		}

		[ConfigurationProperty("keyPrefix")]
		public string KeyPrefix
		{
			get { return (string) base["keyPrefix"]; }
		}


		public override Type BehaviorType
		{
			get { return typeof (OperationTimingEndpointBehavior); }
		}

		protected override object CreateBehavior()
		{
			var client = new StatsDClient(Hostname, Port, KeyPrefix);

			return new OperationTimingEndpointBehavior(new InvocationReporter(client));
		}
	}
}