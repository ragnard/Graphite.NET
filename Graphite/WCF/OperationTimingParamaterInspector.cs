using System.Diagnostics;
using System.ServiceModel.Dispatcher;

namespace Graphite.WCF
{
	public class OperationTimingParamaterInspector : IParameterInspector
	{
		readonly string _contractName;
		readonly IInvocationReporter _invocationReporter;

		Stopwatch _stopwatch;

		public OperationTimingParamaterInspector(IInvocationReporter invocationReporter, string contractName)
		{
			_invocationReporter = invocationReporter;
			_contractName = contractName;
		}

		#region IParameterInspector Members

		public object BeforeCall(string operationName, object[] inputs)
		{
			_stopwatch = Stopwatch.StartNew();
			return null;
		}

		public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
		{
			_stopwatch.Stop();

			_invocationReporter.Report(string.Format("{0}.{1}", _contractName, operationName),
			                           _stopwatch.ElapsedMilliseconds);
		}

		#endregion
	}
}