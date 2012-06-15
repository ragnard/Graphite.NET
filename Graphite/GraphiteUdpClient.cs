using System;
using System.Net.Sockets;
using Graphite.Policy;

namespace Graphite
{
	/// <summary>
	/// Graphite UDP client. Have a look at
	/// http://graphite.wikidot.com/getting-your-data-into-graphite
	/// for details
	/// </summary>
	public class GraphiteUdpClient : IGraphiteClient, IDisposable
	{
		readonly ExceptionPolicy _policy;
		readonly UdpClient _udpClient;

		public GraphiteUdpClient(string hostname, int port = 2003, string keyPrefix = null, ExceptionPolicy policy = null)
		{
			_policy = policy ?? TracingPolicy.Default;

			Hostname = hostname;
			Port = port;
			KeyPrefix = keyPrefix;

			_udpClient = new UdpClient(Hostname, Port);
		}

		public string Hostname { get; private set; }
		public int Port { get; private set; }
		public string KeyPrefix { get; private set; }

		public void Send(string path, int value, DateTime timeStamp)
		{
			_policy.Do(() =>
				{
#if NET35
					if (!string.IsNullOrEmpty(KeyPrefix))
#else
					if (!string.IsNullOrWhiteSpace(KeyPrefix))
#endif
					{
						path = KeyPrefix + "." + path;
					}

					byte[] message = new PlaintextMessage(path, value, timeStamp).ToByteArray();

					_udpClient.Send(message, message.Length);
				});
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;

			if (_udpClient != null)
			{
				_udpClient.Close();
			}
		}
	}
}