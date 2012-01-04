using Graphite.WCF;

namespace Graphite.StatsD
{
	public class InvocationReporter : IInvocationReporter
	{
		readonly StatsDClient _statsDClient;

		public InvocationReporter(StatsDClient statsDClient)
		{
			_statsDClient = statsDClient;
		}

		public void Report(string path, long duration)
		{
			_statsDClient.Timing(path, duration);
		}
	}
}