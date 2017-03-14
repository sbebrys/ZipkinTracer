namespace ZipkinTracer.Models
{
    public class TraceInfo
    {
        public TraceInfo(string traceId, string spanId, string parentSpanId, bool isSampled)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            IsSampled = isSampled;
        }

        public string TraceId { get; set; }
        public string SpanId { get; set; }
        public string ParentSpanId { get; set; }
        public bool IsSampled { get; set; }
    }
}
