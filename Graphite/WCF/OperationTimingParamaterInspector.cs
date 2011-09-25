using System.Diagnostics;
using System.ServiceModel.Dispatcher;

namespace Graphite.WCF
{
    public class OperationTimingParamaterInspector : IParameterInspector
    {
        private readonly StatsClient _statsClient;
        private readonly string _contractName;

        private Stopwatch _stopwatch;

        public OperationTimingParamaterInspector(StatsClient statsClient, string contractName)
        {
            _statsClient = statsClient;
            _contractName = contractName;
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            _stopwatch = Stopwatch.StartNew();
            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            _stopwatch.Stop();

            _statsClient.Timing(string.Format("{0}.{1}", _contractName, operationName), 
                                _stopwatch.ElapsedMilliseconds);
        }
    }
}