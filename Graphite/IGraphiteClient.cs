using System;

namespace Graphite
{
    public interface IGraphiteClient
    {
        void Send(string path, int value);
        void Send(string path, int value, DateTime timeStamp);
    }
}