using ZipkinTracer.Models;

namespace ZipkinTracer
{
    internal interface ISpanCollector
    {
        void Collect(Span span);
        bool TryTake(out Span span);
    }
}