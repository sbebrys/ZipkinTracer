﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZipkinTracer.Models;
using ZipkinTracer.Models.References;

namespace ZipkinTracer.Internal
{
    internal sealed class SpanTracer : ISpanTracer
    {
        private readonly ISpanCollector _spanCollector;
        private readonly IServiceEndpoint _zipkinEndpoint;
        private readonly ZipkinConfig _zipkinConfig;

        public SpanTracer(ZipkinConfig zipkinConfig, ISpanCollector spanCollector, IServiceEndpoint zipkinEndpoint)
        {
            if (zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));
            if (spanCollector == null) throw new ArgumentNullException(nameof(spanCollector));
            if (zipkinEndpoint == null) throw new ArgumentNullException(nameof(zipkinEndpoint));

            _spanCollector = spanCollector;
            _zipkinEndpoint = zipkinEndpoint;
            _zipkinConfig = zipkinConfig;
        }

        public async Task<Span> ReceiveServerSpan(string spanName, TraceInfo traceInfo, Uri requestUri)
        {
            var serviceName = CleanServiceName(traceInfo.Domain.Host);
            var newSpan = new Span(spanName, traceInfo.SpanId, traceInfo.ParentSpanId, traceInfo.TraceId, traceInfo.Domain);
            var serviceEndpoint = await _zipkinEndpoint.GetLocalEndpoint(serviceName, (ushort)requestUri.Port);

            var annotation = new Annotation
            {
                Host = serviceEndpoint,
                Value = ZipkinConstants.ServerReceive
            };

            newSpan.Annotations.Add(annotation);

            AddBinaryAnnotation("http.path", requestUri.AbsolutePath, newSpan, serviceEndpoint);

            return newSpan;
        }

        public void SendServerSpan(Span span)
        {
            if (span == null)
            {
                throw new ArgumentNullException(nameof(span));
            }

            if (span.Annotations == null || !span.Annotations.Any())
            {
                throw new ArgumentException("Invalid server span: Annotations list is invalid.");
            }

            var annotation = new Annotation
            {
                Host = span.Annotations.First().Host,
                Value = ZipkinConstants.ServerSend
            };

            span.Annotations.Add(annotation);

            _spanCollector.Collect(span);
        }

        public async Task<Span> SendClientSpan(string spanName, TraceInfo traceInfo, Uri remoteUri)
        {
            var serviceName = CleanServiceName(traceInfo.Domain.Host);
            var newSpan = new Span(spanName, traceInfo.SpanId, traceInfo.ParentSpanId, traceInfo.TraceId, traceInfo.Domain);
            var serviceEndpoint = await _zipkinEndpoint.GetLocalEndpoint(serviceName, (ushort)remoteUri.Port);
            var clientServiceName = CleanServiceName(remoteUri.Host);

            var annotation = new Annotation
            {
                Host = serviceEndpoint,
                Value = ZipkinConstants.ClientSend
            };

            newSpan.Annotations.Add(annotation);
            AddBinaryAnnotation("http.path", remoteUri.AbsolutePath, newSpan, serviceEndpoint);
            AddBinaryAnnotation("sa", "1", newSpan, await _zipkinEndpoint.GetRemoteEndpoint(remoteUri, clientServiceName));

            return newSpan;
        }

        public void ReceiveClientSpan(Span span, int statusCode)
        {
            if (span == null)
            {
                throw new ArgumentNullException(nameof(span));
            }

            if (span.Annotations == null || !span.Annotations.Any())
            {
                throw new ArgumentException("Invalid client span: Annotations list is invalid.");
            }

            var annotation = new Annotation
            {
                Host = span.Annotations.First().Host,
                Value = ZipkinConstants.ClientReceive
            };

            span.Annotations.Add(annotation);
            AddBinaryAnnotation("http.status", statusCode.ToString(), span, span.Annotations.First().Host);

            _spanCollector.Collect(span);
        }

        public async Task Record(Span span, string value)
        {
            var serviceName = CleanServiceName(span.Domain.Host);
            var servicePort = (ushort)span.Domain.Port;

            if (span == null)
                throw new ArgumentNullException(nameof(span), "In order to record an annotation, the span must be not null.");

            span.Annotations.Add(new Annotation
            {
                Host = await _zipkinEndpoint.GetLocalEndpoint(serviceName, servicePort),
                Value = value
            });
        }

        public async Task RecordBinary(Span span, string key, object value)
        { 
            var serviceName = CleanServiceName(span.Domain.Host);
            var servicePort = (ushort)span.Domain.Port;

            if (span == null)
                throw new ArgumentNullException(nameof(span), "In order to record a binary annotation, the span must be not null.");

            var host = await _zipkinEndpoint.GetLocalEndpoint(serviceName, servicePort);

            span.Annotations.Add(new BinaryAnnotation
            {
                Host = host,
                Key = key,
                Value = value
            });
        }

        private string CleanServiceName(string host)
        {
            foreach (var domain in _zipkinConfig.NotToBeDisplayedDomainList)
            {
                if (host.Contains(domain))
                {
                    return host.Replace(domain, string.Empty);
                }
            }

            return host;
        }

        private void AddBinaryAnnotation(string key, object value, Span span, Endpoint endpoint)
        {
            var binaryAnnotation = new BinaryAnnotation
            {
                Host = endpoint,
                Key = key,
                Value = value
            };

            span.Annotations.Add(binaryAnnotation);
        }
    }
}
