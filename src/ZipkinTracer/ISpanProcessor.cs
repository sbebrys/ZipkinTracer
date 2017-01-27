using System.Threading.Tasks;

namespace ZipkinTracer
{
    internal interface ISpanProcessor
    {
        Task Start();
        Task Stop();
    }
}