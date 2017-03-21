using System;
using System.Threading.Tasks;
using ZipkinTracer.Models;

namespace ZipkinTracer
{
    public interface ISpanTracer
    {
        Task<Span> SendClientSpan(string methodName, TraceInfo traceInfo, Uri remoteUri);
        Task<Span> ReceiveServerSpan(string methodName, TraceInfo traceInfo, Uri requestUri);
        void ReceiveClientSpan(Span clientSpan, int statusCode, string errorMessage = null);
        void SendServerSpan(Span serverSpan, int statusCode, string errorMessage = null);
        Task Record(Span span, string value);
        Task RecordBinary(Span span, string key, object value);
    }
}