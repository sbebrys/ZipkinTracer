using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ZipkinTracer.Internal;
using ZipkinTracer.Http;
using ZipkinTracer.Helpers;

namespace ZipkinTracer.Owin
{
    internal class ZipkinMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ZipkinConfig _zipkinConfig;
        //private readonly ITraceInfoAccessor _traceInfoAccessor;

        //public const string TraceIdHeaderName = "X-B3-TraceId";
        //public const string SpanIdHeaderName = "X-B3-SpanId";
        //public const string ParentSpanIdHeaderName = "X-B3-ParentSpanId";
        //public const string SampledHeaderName = "X-B3-Sampled";

        public ZipkinMiddleware(RequestDelegate next, ISpanProcessor spanProcessor, ZipkinConfig zipkinConfig
            //, ITraceInfoAccessor traceInfoAccessor
            )
        {
            if (spanProcessor == null) throw new ArgumentNullException(nameof(spanProcessor));
            if (zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));
            //if (traceInfoAccessor == null) throw new ArgumentNullException(nameof(traceInfoAccessor));

            _next = next;
            _zipkinConfig = zipkinConfig;
            //_traceInfoAccessor = traceInfoAccessor;

            spanProcessor.Start();
        }

        public async Task Invoke(HttpContext context)
        {
            if (_zipkinConfig.Bypass != null && _zipkinConfig.Bypass(context.Request))
            {
                await _next(context);
                return;
            }

            //SetTraceInfoProperties(context);

            var traceClient = context.RequestServices.GetRequiredService<IZipkinTracer>();
            var span = await traceClient.StartServerTrace(new Uri(context.Request.GetEncodedUrl()), context.Request.Method);
            try
            {
                await _next(context);
            }
            finally
            {
                traceClient.EndServerTrace(span);
            }
        }

        //private void SetTraceInfoProperties(HttpContext context)
        //{
        //    string headerTraceId = context.Request.Headers[TraceIdHeaderName];
        //    string headerSpanId = context.Request.Headers[SpanIdHeaderName];
        //    string headerParentSpanId = context.Request.Headers[ParentSpanIdHeaderName];
        //    string headerSampled = context.Request.Headers[SampledHeaderName];

        //    string requestPath = context.Request.Path.ToString();

        //    _traceInfoAccessor.TraceInfo.TraceId = headerTraceId.IsParsableTo128Or64Bit() ? headerTraceId : GenerateNewTraceId(_zipkinConfig.Create128BitTraceId);
        //    _traceInfoAccessor.TraceInfo.SpanId = headerSpanId.IsParsableToLong() ? headerSpanId : GenerateHexEncodedInt64Id();
        //    _traceInfoAccessor.TraceInfo.ParentSpanId = headerParentSpanId.IsParsableToLong() ? headerParentSpanId : string.Empty;
        //    _traceInfoAccessor.TraceInfo.IsSampled = _zipkinConfig.ShouldBeSampled(headerSampled, requestPath);

        //    if (_traceInfoAccessor.TraceInfo.SpanId == _traceInfoAccessor.TraceInfo.ParentSpanId)
        //    {
        //        throw new ArgumentException("x-b3-SpanId and x-b3-ParentSpanId must not be the same value.");
        //    }
        //}

        //private string GenerateNewTraceId(bool create128Bit)
        //{
        //    return create128Bit ? Guid.NewGuid().ToString("N") : GenerateHexEncodedInt64Id();
        //}

        //private string GenerateHexEncodedInt64Id()
        //{
        //    return Convert.ToString(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0), 16);
        //}
    }

    public static class AppBuilderExtensions
    {
        public static void UseZipkinTracer(this IApplicationBuilder app)
        {
            app.UseMiddleware<ZipkinMiddleware>();
        }
    }
}