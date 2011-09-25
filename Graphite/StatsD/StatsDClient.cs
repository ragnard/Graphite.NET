using System;
using System.Net.Sockets;
using System.Text;

namespace Graphite.StatsD
{
    public class StatsDClient : IDisposable
    {
        private readonly string _keyPrefix;
        private readonly UdpClient _client;
        private readonly Random _random;

        public StatsDClient(string hostname, int port, string keyPrefix = null)
        {
            _keyPrefix = keyPrefix;
            _client = new UdpClient { ExclusiveAddressUse = false };
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
            var stat = string.Format("{0}:{1}|c", key, magnitude);
            return MaybeSend(stat, sampleRate);
        }

        public bool Increment(int magnitude, double sampleRate, params string[] keys)
        {
            var stats = new string[keys.Length];

            for (var i = 0; i < keys.Length; i++)
            {
                stats[i] = string.Format("{0}:{1}|c", keys[i], magnitude);
            }
            return MaybeSend(sampleRate, stats);
        }

        private bool MaybeSend(string stat, double sampleRate)
        {
            return MaybeSend(sampleRate, stat);
        }

        private bool MaybeSend(double sampleRate, params string[] stats)
        {
            // only return true if we sent something
            var retval = false; 

            if (sampleRate < 1.0)
            {
                foreach (var stat in stats)
                {
                    if (_random.NextDouble() <= sampleRate)
                    {
                        var sampledStat = string.Format("{0}|@{1}", stat, sampleRate);
                        
                        if (Send(sampledStat))
                        {
                            retval = true;
                        }
                    }
                }
            }
            else
            {
                foreach (var stat in stats)
                {
                    if (Send(stat))
                    {
                        retval = true;
                    }
                }
            }

            return retval;
        }

        private bool Send(string message)
        {
            try
            {
                if(!string.IsNullOrWhiteSpace(_keyPrefix))
                {
                    message = _keyPrefix + "." + message;
                }

                var data = Encoding.UTF8.GetBytes(message);

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