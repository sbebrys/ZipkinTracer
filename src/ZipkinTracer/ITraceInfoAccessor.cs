using ZipkinTracer.Models;

namespace ZipkinTracer
{
    public interface ITraceInfoAccessor
    {
        TraceInfo TraceInfo { get; set; }
    }
}