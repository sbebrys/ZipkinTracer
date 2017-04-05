using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ZipkinTracer.Models;

namespace ZipkinTracer
{
    public interface IZipkinTracer
    {
        Task<Span> StartServerTrace(Uri requestUri, string spanName);

        Task<Span> StartClientTrace(Uri remoteUri, string spanName);

        void EndServerTrace(Span serverSpan, int statusCode, string errorMessage = null);

        void EndClientTrace(Span clientSpan, int statusCode, string errorMessage = null);

        Task Record(Span span, [CallerMemberName] string value = null);

        Task RecordBinary<T>(Span span, string key, T value);

        Task RecordLocalComponent(Span span, string value);

	    TraceInfo GetCurrentTraceInfo();
    }
}
