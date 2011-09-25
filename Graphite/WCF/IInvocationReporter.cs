namespace Graphite.WCF
{
    public interface IInvocationReporter
    {
        void Report(string path, long duration);
    }
}