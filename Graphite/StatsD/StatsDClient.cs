using System;
using System.Net.Sockets;
using System.Text;
using Graphite.Policy;

namespace Graphite.StatsD
{
	public class StatsDClient : IDisposable, StatisticsClient
	{
		readonly UdpClient _client;
		readonly string _keyPrefix;
		readonly ExceptionPolicy _policy;
		readonly Random _random;

		public StatsDClient(string hostname, uint port = 8125u, string keyPrefix = null, ExceptionPolicy policy = null)
		{
			_keyPrefix = keyPrefix;
			_policy = policy ?? TracingPolicy.Default;
			_client = new UdpClient {ExclusiveAddressUse = false};
			_client.Connect(hostname, (int)port);
			_random = new Random();
		}

		public void Timing(string key, long value, double sampleRate = 1.0)
		{
			MaybeSend(sampleRate, string.Format("{0}:{1}|ms", key, value));
		}

		public void Decrement(string key, int magnitude = -1, double sampleRate = 1.0)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			Increment(key, magnitude, sampleRate);
		}

		public void Decrement(params string[] keys)
		{
			Increment(-1, 1.0, keys);
		}

		public void Decrement(int magnitude, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			
			Increment(magnitude, 1.0, keys);
		}

		public void Decrement(int magnitude, double sampleRate, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			
			Increment(magnitude, sampleRate, keys);
		}

		public void Increment(string key, int magnitude = 1, double sampleRate = 1.0)
		{
			string stat = string.Format("{0}:{1}|c", key, magnitude);
			
			MaybeSend(stat, sampleRate);
		}

		public void Increment(int magnitude, double sampleRate, params string[] keys)
		{
			var stats = new string[keys.Length];

			for (int i = 0; i < keys.Length; i++)
			{
				stats[i] = string.Format("{0}:{1}|c", keys[i], magnitude);
			}
			
			MaybeSend(sampleRate, stats);
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
			return _policy.Do(() =>
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
				});
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