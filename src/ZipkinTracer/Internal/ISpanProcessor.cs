using System.Threading.Tasks;

namespace ZipkinTracer.Internal
{
    internal interface ISpanProcessor
    {
        Task Start();
        Task Stop();
    }
}