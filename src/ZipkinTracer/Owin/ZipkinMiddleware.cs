using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ZipkinTracer.Internal;

namespace ZipkinTracer.Owin
{
    internal class ZipkinMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ZipkinConfig _zipkinConfig;

        public ZipkinMiddleware(RequestDelegate next, ISpanProcessor spanProcessor, ZipkinConfig zipkinConfig)
        {
            if(spanProcessor == null) throw new ArgumentNullException(nameof(spanProcessor));
            if(zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));

            _next = next;
            _zipkinConfig = zipkinConfig;

            spanProcessor.Start();
        }

        public async Task Invoke(HttpContext context)
        {
            if (_zipkinConfig.Bypass != null && _zipkinConfig.Bypass(context.Request))
            {
                await _next(context);
                return;
            }

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
    }

    public static class AppBuilderExtensions
    {
        public static void UseZipkinTracer(this IApplicationBuilder app)
        {
            app.UseMiddleware<ZipkinMiddleware>();
        }
    }
}