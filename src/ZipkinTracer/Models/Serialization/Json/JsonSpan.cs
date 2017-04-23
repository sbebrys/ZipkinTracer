using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ZipkinTracer.Extensions;
using ZipkinTracer.Internal;

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

        [JsonProperty("timestamp")]
        public long? Timestamp => _span.IsJoinedSpan ? null : ResolveBeginTimeStamp();

        [JsonProperty("duration")]
        public long? Duration => _span.IsJoinedSpan ? null : ResolveDuration();

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

        private long? ResolveBeginTimeStamp()
        {
            return _span.GetAnnotationsByType<Annotation>().
                    Where(x => x.Value == TraceKeys.ServerRecv || x.Value == TraceKeys.ClientSend).
                    Select(x => (long?)x.Timestamp.ToUnixTimeMicroseconds()).FirstOrDefault();
        }

        private long? ResolveEndTimeStamp()
        {
            return _span.GetAnnotationsByType<Annotation>().
                    Where(x => x.Value == TraceKeys.ServerSend || x.Value == TraceKeys.ClientRecv).
                    Select(x => (long?)x.Timestamp.ToUnixTimeMicroseconds()).FirstOrDefault();
        }

        private long? ResolveDuration()
        {
            var begin = ResolveBeginTimeStamp();
            var end = ResolveEndTimeStamp();

            if (!begin.HasValue || !end.HasValue)
                return null;

            return end - begin;
        }
    }
}