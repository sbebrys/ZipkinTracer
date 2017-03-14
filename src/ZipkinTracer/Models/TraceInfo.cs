using System;

namespace ZipkinTracer.Models
{
    public class TraceInfo
    {
        public const string TraceIdHeaderName = "X-B3-TraceId";
        public const string SpanIdHeaderName = "X-B3-SpanId";
        public const string ParentSpanIdHeaderName = "X-B3-ParentSpanId";
        public const string SampledHeaderName = "X-B3-Sampled";

        public string TraceId { get; }
        public string SpanId { get; }
        public string ParentSpanId { get; }
        public Uri Domain { get; }
        public bool IsSampled { get; }

        public bool IsTraceOn => !string.IsNullOrEmpty(TraceId) && IsSampled;

        public TraceInfo(string traceId, string spanId, string parentSpanId, bool isSampled, Uri domain)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            IsSampled = isSampled;
            Domain = domain;
        }
    }
}
