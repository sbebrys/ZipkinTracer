using System;
using System.Collections.Generic;

namespace ZipkinTracer.Models
{
    public class Span
    {
        public string Name { get; }
        public string Id { get; }
        public string ParentId { get; }
        public string TraceId { get; }
        public Uri Domain { get; }
        public IList<AnnotationBase> Annotations { get; } = new List<AnnotationBase>();

        public Span(string name, string id, string parentId, string traceId, Uri domain)
        {
            Name = name;
            Id = id;
            ParentId = parentId;
            TraceId = traceId;
            Domain = domain;
        }
    }
}