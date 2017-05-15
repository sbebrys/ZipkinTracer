using ZipkinTracer.Models;

namespace ZipkinTracer.Internal
{
    internal interface ISpanCollector
    {
        void Collect(Span span);
        bool TryTake(out Span span);
    }
}