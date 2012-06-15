using System;
using System.Net.Sockets;
using Graphite.Policy;

namespace Graphite
{
	/// <summary>
	/// Typed TCP client for logging into graphite. Have a look at
	/// http://graphite.wikidot.com/getting-your-data-into-graphite
	/// for details
	/// </summary>
	public class GraphiteTcpClient : IGraphiteClient, IDisposable
	{
		readonly ExceptionPolicy _policy;
		readonly TcpClient _tcpClient;

		public GraphiteTcpClient(string hostname, int port = 2003, string keyPrefix = null, ExceptionPolicy policy = null)
		{
			_policy = policy ?? TracingPolicy.Default;
			Hostname = hostname;
			Port = port;
			KeyPrefix = keyPrefix;

			_tcpClient = new TcpClient(Hostname, Port);
		}

		public string Hostname { get; private set; }
		public int Port { get; private set; }
		public string KeyPrefix { get; private set; }

		#region IGraphiteClient Members

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

					_tcpClient.GetStream().Write(message, 0, message.Length);
				});
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;

			if (_tcpClient != null)
			{
				_tcpClient.Close();
			}
		}

		#endregion
	}
}