using System;
using System.Collections.Generic;
using System.Linq;
using ZipkinTracer.Models;
using ZipkinTracer.Models.References;

namespace ZipkinTracer.Extensions
{
    public static class ZipkinTracerExtensions
    {
        private const long TicksPerMicrosecond = 10;

        private static readonly Dictionary<Type, AnnotationType> AnnotationTypeMappings =
            new Dictionary<Type, AnnotationType>()
            {
                {typeof(bool), AnnotationType.Boolean},
                {typeof(byte[]), AnnotationType.ByteArray},
                {typeof(short), AnnotationType.Int16},
                {typeof(int), AnnotationType.Int32},
                {typeof(long), AnnotationType.Int64},
                {typeof(double), AnnotationType.Double},
                {typeof(string), AnnotationType.String}
            };

        public static AnnotationType AsAnnotationType(this Type type)
        {
            return AnnotationTypeMappings.ContainsKey(type) ? AnnotationTypeMappings[type] : AnnotationType.String;
        }

        public static IEnumerable<TAnnotation> GetAnnotationsByType<TAnnotation>(this Span span)
            where TAnnotation : AnnotationBase
        {
            return span.Annotations.OfType<TAnnotation>();
        }

        public static long ToUnixTimeMicroseconds(this DateTime value)
        {
            return Convert.ToInt64(
                (value - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).Ticks / TicksPerMicrosecond
            );
        }
    }
}