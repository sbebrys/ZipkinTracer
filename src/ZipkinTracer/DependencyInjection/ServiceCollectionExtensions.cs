using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ZipkinTracer.Internal;
using ZipkinTracer.Models;
using ZipkinTracer.Http;

namespace ZipkinTracer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const int MaxQueueSize = 100;

        public static void AddZipkinTracer(this IServiceCollection services, ZipkinConfig config)
        {
            if(config == null) throw new ArgumentNullException(nameof(config));

            services.AddSingleton(config);
            services.AddSingleton(new BlockingCollection<Span>(MaxQueueSize));
            services.AddSingleton<IServiceEndpoint, ServiceEndpoint>();
            services.AddSingleton<ISpanProcessorTask, SpanProcessorTask>();
            services.AddSingleton<ISpanProcessor, SpanProcessor>();
            services.AddSingleton<ITraceInfoAccessor, TraceInfoAccessor>();

            services.AddScoped<ISpanCollector, SpanCollector>();
            services.AddScoped<IZipkinTracer, ZipkinClient>();
            services.AddScoped<ISpanTracer, SpanTracer>();
        }
    }
}