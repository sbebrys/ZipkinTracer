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
            byte[] valueArray;

            switch (type)
            {
                case AnnotationType.Boolean:
                    valueArray = BitConverter.GetBytes((bool)value);
                    break;
                case AnnotationType.ByteArray:
                    valueArray = value as byte[];
                    break;
                case AnnotationType.Int16:
                    valueArray = ConvertBigEndian(BitConverter.GetBytes((double)value));
                    break;
                case AnnotationType.Int32:
                    valueArray = ConvertBigEndian(BitConverter.GetBytes((double)value));
                    break;
                case AnnotationType.Int64:
                    valueArray = ConvertBigEndian(BitConverter.GetBytes((double)value));
                    break;
                case AnnotationType.Double:
                    valueArray = ConvertBigEndian(BitConverter.GetBytes((double)value));
                    break;
                case AnnotationType.String:
                    return value as string;
                default:
                    throw new ArgumentException("Unsupported object type for binary annotation.");
            }

            return Encoding.UTF8.GetString(valueArray);
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

            var bytes = ConvertBigEndian(address.GetAddressBytes());
            return BitConverter.ToUInt32(bytes, 0).ToString();
        }

        public static string ToIPV6Bytes(this IPAddress address)
        {
            if (address == null || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                return null;

            var bytes = ConvertBigEndian(address.GetAddressBytes());
            return Encoding.UTF8.GetString(bytes);
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