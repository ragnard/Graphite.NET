using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Graphite.WCF
{
    public class OperationTimingCallContextInitializer : ICallContextInitializer
    {
        private readonly StatsClient _client;
        private readonly string _operationName;
        private readonly string _contractName;
        
        private Stopwatch _stopwatch;

        public OperationTimingCallContextInitializer(StatsClient client, string operationName, string contractName)
        {
            _client = client;
            _operationName = operationName;
            _contractName = contractName;
        }

        public object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message)
        {
            _stopwatch = Stopwatch.StartNew();
            return null;
        }

        public void AfterInvoke(object correlationState)
        {
            _client.Timing(string.Format("{0}.{1}", _contractName, _operationName), (int)_stopwatch.ElapsedMilliseconds);
        }
    }
}