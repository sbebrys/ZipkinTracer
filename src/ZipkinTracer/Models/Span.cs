using System;
using System.Collections.Generic;
using System.Net;

namespace ZipkinTracer.Models
{
    public class Span
    {
        public string Name { get; }
        public string Id => TraceInfo.SpanId;
        public string ParentId => TraceInfo.ParentSpanId;
        public string TraceId => TraceInfo.TraceId;
        public Uri Domain => TraceInfo.Domain;
        public IPAddress LocalIP => TraceInfo.LocalIP;
        public bool IsJoinedSpan => TraceInfo.IsJoinedSpan;
        public TraceInfo TraceInfo { get; }
        public IList<AnnotationBase> Annotations { get; } = new List<AnnotationBase>();

        public Span(string name, TraceInfo traceInfo)
        {
            Name = name;
            TraceInfo = traceInfo;
        }
    }
}