using System;

namespace Graphite
{
    public interface IGraphiteClient
    {
        void Send(string path, double value, DateTime timeStamp);
    }

    public static class IGraphiteClientExtensions
    {
        public static void Send(this IGraphiteClient self, string path, int value)
        {
            self.Send(path, value, DateTime.Now);
        }
    }
}