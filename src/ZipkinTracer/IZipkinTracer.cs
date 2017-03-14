using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ZipkinTracer.Models;

namespace ZipkinTracer
{
    public interface IZipkinTracer
    {
        Task<Span> StartServerTrace(Uri requestUri, string methodName);

        Task<Span> StartClientTrace(Uri remoteUri, string methodName, TraceInfo traceInfo);

        void EndServerTrace(Span serverSpan);

        void EndClientTrace(Span clientSpan, int statusCode);

        Task Record(Span span, [CallerMemberName] string value = null);

        Task RecordBinary<T>(Span span, string key, T value);

        Task RecordLocalComponent(Span span, string value);

        TraceInfo CreateInnerSpan();
    }
}
