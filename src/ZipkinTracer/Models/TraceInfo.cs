using System;
using System.Net;
using ZipkinTracer.Helpers;

namespace ZipkinTracer.Models
{
    public class TraceInfo
    {
        public const string TraceIdHeaderName = "X-B3-TraceId";
        public const string SpanIdHeaderName = "X-B3-SpanId";
        public const string ParentSpanIdHeaderName = "X-B3-ParentSpanId";
        public const string SampledHeaderName = "X-B3-Sampled";
        public const string SpanNameHeaderName = "X-Span-Name";
        public const string TraceInfoKey = "TraceInfo";

        public string TraceId { get; }
        public string SpanId { get; }
        public string ParentSpanId => ParentTraceInfo?.SpanId;
        public Uri Domain { get; }
        public IPAddress LocalIP { get; }
        public bool IsSampled { get; }
        public TraceInfo ParentTraceInfo { get; }

        public bool IsTraceOn => !string.IsNullOrEmpty(TraceId) && IsSampled;

        public TraceInfo(string traceId, string spanId, bool isSampled, Uri domain, IPAddress localIP = null, string parentSpanId = null)
        {
            TraceId = traceId;
            SpanId = spanId;
            IsSampled = isSampled;
            Domain = domain;
            LocalIP = localIP;

            if (!string.IsNullOrEmpty(parentSpanId))
            {
                ParentTraceInfo = new TraceInfo(traceId, parentSpanId, isSampled, domain, localIP);
            }
        }

        public TraceInfo(TraceInfo parentTraceInfo)
        {
            ParentTraceInfo = parentTraceInfo;
            TraceId = parentTraceInfo?.TraceId;
            SpanId = TraceIdHelper.GenerateHexEncodedInt64Id();
            IsSampled = parentTraceInfo?.IsSampled ?? false;
            Domain = parentTraceInfo?.Domain;
            LocalIP = parentTraceInfo?.LocalIP;
        }
    }
}