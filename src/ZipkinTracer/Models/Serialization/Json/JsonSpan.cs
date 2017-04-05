using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ZipkinTracer.Extensions;

namespace ZipkinTracer.Models.Serialization.Json
{
    internal class JsonSpan
    {
        private readonly Span _span;

        [JsonProperty("traceId")]
        public string TraceId => _span.TraceId;

        [JsonProperty("name")]
        public string Name => _span.Name;

        [JsonProperty("id")]
        public string Id => _span.Id;

        [JsonProperty("parentId", NullValueHandling = NullValueHandling.Ignore)]
        public string ParentId => string.IsNullOrWhiteSpace(_span.ParentId) ? null : _span.ParentId;

        [JsonProperty("annotations")]
        public IEnumerable<JsonAnnotation> Annotations =>
            _span.GetAnnotationsByType<Annotation>().Select(annotation => new JsonAnnotation(annotation));

        [JsonProperty("binaryAnnotations")]
        public IEnumerable<JsonBinaryAnnotation> BinaryAnnotations =>
            _span.GetAnnotationsByType<BinaryAnnotation>().Select(annotation => new JsonBinaryAnnotation(annotation));

        public JsonSpan(Span span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            _span = span;
        }
    }
}