using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Graphite.Policy
{
	public static class TracingPolicy
	{
		public static ExceptionPolicy Default
		{
			get
			{
				// from Send method
				return ExceptionPolicy.InCaseOf<SocketException, ObjectDisposedException, InvalidOperationException>()
					.Retry(2)
					.Finally(ex =>
						{
							// poor man's logging...
							Trace.WriteLine(ex.ToString());
							return true; // handled.
						});
			}
		}
	}
}