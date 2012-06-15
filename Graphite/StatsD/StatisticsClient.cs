namespace Graphite.StatsD
{
	public interface StatisticsClient
	{
		void Timing(string key, long value, double sampleRate = 1.0);

		void Decrement(string key, int magnitude = -1, double sampleRate = 1.0);
		void Decrement(params string[] keys);
		void Decrement(int magnitude, params string[] keys);
		void Decrement(int magnitude, double sampleRate, params string[] keys);

		void Increment(string key, int magnitude = 1, double sampleRate = 1.0);
		void Increment(int magnitude, double sampleRate, params string[] keys);
	}
}