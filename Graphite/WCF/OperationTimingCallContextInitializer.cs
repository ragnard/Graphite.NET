using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Graphite.WCF
{
	public class OperationTimingCallContextInitializer : ICallContextInitializer
	{
		readonly string _contractName;
		readonly IInvocationReporter _invocationReporter;
		readonly string _operationName;

		Stopwatch _stopwatch;

		public OperationTimingCallContextInitializer(IInvocationReporter invocationReporter, string operationName,
		                                             string contractName)
		{
			_invocationReporter = invocationReporter;
			_operationName = operationName;
			_contractName = contractName;
		}

		#region ICallContextInitializer Members

		public object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message)
		{
			_stopwatch = Stopwatch.StartNew();
			return null;
		}

		public void AfterInvoke(object correlationState)
		{
			_invocationReporter.Report(string.Format("{0}.{1}", _contractName, _operationName),
			                           _stopwatch.ElapsedMilliseconds);
		}

		#endregion
	}
}