using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Graphite.StatsD;

namespace Graphite.WCF
{
    public class OperationTimingEndpointBehavior : IEndpointBehavior
    {
        private readonly IInvocationReporter _invocationReporter;

        public OperationTimingEndpointBehavior(IInvocationReporter invocationReporter)
        {
            _invocationReporter = invocationReporter;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            foreach(var operation in clientRuntime.Operations)
            {
                operation.ParameterInspectors.Add(
                    new OperationTimingParamaterInspector(_invocationReporter, endpoint.Contract.Name));
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (var operation in endpointDispatcher.DispatchRuntime.Operations)
            {
                operation.CallContextInitializers.Add(
                    new OperationTimingCallContextInitializer(_invocationReporter, operation.Name, endpoint.Contract.Name));
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}