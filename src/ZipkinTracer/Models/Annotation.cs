using System;

namespace ZipkinTracer.Models
{
    public class Annotation : AnnotationBase
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Value { get; set; }
    }
}
