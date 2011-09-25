using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Graphite.WCF
{
    public class OperationTimingEndpointBehaviorExtensionElement : BehaviorExtensionElement
    {
        [ConfigurationProperty("hostname", IsRequired = true)]
        public string Hostname
        {
            get
            {
                return (string)base["hostname"];
            }
        }

        [ConfigurationProperty("port", DefaultValue = 8125)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }
        }

        [ConfigurationProperty("keyPrefix")]
        public string KeyPrefix
        {
            get
            {
                return (string)base["keyPrefix"];
            }
        }


        protected override object CreateBehavior()
        {
            return new OperationTimingEndpointBehavior(new StatsClient(Hostname, Port, KeyPrefix));
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(OperationTimingEndpointBehavior);
            }
        }
    }
}