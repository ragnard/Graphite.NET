using System;
using System.Net.Sockets;
using System.Text;
using Graphite.Policy;

namespace Graphite.StatsD
{
	public class StatsDClient : IDisposable
	{
		readonly UdpClient _client;
		readonly string _keyPrefix;
		readonly ExceptionPolicy _policy;
		readonly Random _random;

		public StatsDClient(string hostname, int port, string keyPrefix = null, ExceptionPolicy policy = null)
		{
			_keyPrefix = keyPrefix;
			_policy = policy ?? TracingPolicy.Default;
			_client = new UdpClient {ExclusiveAddressUse = false};
			_client.Connect(hostname, port);
			_random = new Random();
		}

		public bool Timing(string key, long value, double sampleRate = 1.0)
		{
			return MaybeSend(sampleRate, string.Format("{0}:{1}|ms", key, value));
		}

		public bool Decrement(string key, int magnitude = -1, double sampleRate = 1.0)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(key, magnitude, sampleRate);
		}

		public bool Decrement(params string[] keys)
		{
			return Increment(-1, 1.0, keys);
		}

		public bool Decrement(int magnitude, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, 1.0, keys);
		}

		public bool Decrement(int magnitude, double sampleRate, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, sampleRate, keys);
		}

		public bool Increment(string key, int magnitude = 1, double sampleRate = 1.0)
		{
			string stat = string.Format("{0}:{1}|c", key, magnitude);
			return MaybeSend(stat, sampleRate);
		}

		public bool Increment(int magnitude, double sampleRate, params string[] keys)
		{
			var stats = new string[keys.Length];

			for (int i = 0; i < keys.Length; i++)
			{
				stats[i] = string.Format("{0}:{1}|c", keys[i], magnitude);
			}
			return MaybeSend(sampleRate, stats);
		}

		bool MaybeSend(string stat, double sampleRate)
		{
			return MaybeSend(sampleRate, stat);
		}

		bool MaybeSend(double sampleRate, params string[] stats)
		{
			// only return true if we sent something
			bool retval = false;

			if (sampleRate < 1.0)
			{
				foreach (string stat in stats)
				{
					if (_random.NextDouble() <= sampleRate)
					{
						string sampledStat = string.Format("{0}|@{1}", stat, sampleRate);

						if (Send(sampledStat))
						{
							retval = true;
						}
					}
				}
			}
			else
			{
				foreach (string stat in stats)
				{
					if (Send(stat))
					{
						retval = true;
					}
				}
			}

			return retval;
		}

		bool Send(string message)
		{
			try
			{
#if NET35
				if (!string.IsNullOrEmpty(_keyPrefix))
#else
					if (!string.IsNullOrWhiteSpace(_keyPrefix))
#endif
				{
					message = _keyPrefix + "." + message;
				}

				byte[] data = Encoding.UTF8.GetBytes(message);

				_client.Send(data, data.Length);

				return true;
			}
			catch
			{
				// Suppress all exceptions for now
				return false;
			}
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;

			if (_client != null)
			{
				_client.Close();
			}
		}

		#endregion
	}
}