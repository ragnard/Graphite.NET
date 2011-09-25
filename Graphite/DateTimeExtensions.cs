using System;

namespace Graphite
{
    public static class DateTimeExtensions
    {
        //private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        private static readonly long EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().Ticks;

        public static long ToUnixTime(this DateTime self)
        {
            return (self.ToUniversalTime().Ticks - EPOCH) / 10000000;
        }
    }
}