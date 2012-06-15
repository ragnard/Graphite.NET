using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Graphite.WCF
{
	public class OperationTimingEndpointBehavior : IEndpointBehavior
	{
		readonly IInvocationReporter _invocationReporter;

		public OperationTimingEndpointBehavior(IInvocationReporter invocationReporter)
		{
			_invocationReporter = invocationReporter;
		}

		#region IEndpointBehavior Members

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			foreach (ClientOperation operation in clientRuntime.Operations)
			{
				operation.ParameterInspectors.Add(
					new OperationTimingParamaterInspector(_invocationReporter, endpoint.Contract.Name));
			}
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			foreach (DispatchOperation operation in endpointDispatcher.DispatchRuntime.Operations)
			{
				operation.CallContextInitializers.Add(
					new OperationTimingCallContextInitializer(_invocationReporter, operation.Name, endpoint.Contract.Name));
			}
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}

		#endregion
	}
}