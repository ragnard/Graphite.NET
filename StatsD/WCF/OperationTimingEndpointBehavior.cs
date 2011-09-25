using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace StatsD.WCF
{
    public class OperationTimingEndpointBehavior : IEndpointBehavior
    {
        private readonly StatsClient _client;

        public OperationTimingEndpointBehavior(StatsClient client)
        {
            _client = client;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            foreach(var operation in clientRuntime.Operations)
            {
                operation.ParameterInspectors.Add(
                    new OperationTimingParamaterInspector(_client, endpoint.Contract.Name));
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (var operation in endpointDispatcher.DispatchRuntime.Operations)
            {
                operation.CallContextInitializers.Add(
                    new OperationTimingCallContextInitializer(_client, operation.Name, endpoint.Contract.Name));
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

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