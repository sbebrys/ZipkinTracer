using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

        public static string AsAnnotationValue(this object value)
        {
            var type = value.GetType().AsAnnotationType();

            switch (type)
            {
                case AnnotationType.ByteArray:
                    return Convert.ToBase64String(value as byte[]);
                case AnnotationType.Boolean:
                case AnnotationType.Int16:
                case AnnotationType.Int32:
                case AnnotationType.Int64:
                case AnnotationType.Double:
                case AnnotationType.String:
                    return value.ToString();
                default:
                    throw new ArgumentException("Unsupported object type for binary annotation.");
            }
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

        public static string ToIPV4Integer(this IPAddress address)
        {
            if (address == null || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return null;

            return address.ToString();
        }

        public static string ToIPV6Bytes(this IPAddress address)
        {
            if (address == null || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                return null;

            return address.ToString();
        }

        private static byte[] ConvertBigEndian(byte[] input)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return input;
        }
    }
}