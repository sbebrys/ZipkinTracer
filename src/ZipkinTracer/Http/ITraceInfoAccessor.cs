using ZipkinTracer.Models;

namespace ZipkinTracer.Http
{
    interface ITraceInfoAccessor
    {
        TraceInfo TraceInfo { get; set; }
    }
}
