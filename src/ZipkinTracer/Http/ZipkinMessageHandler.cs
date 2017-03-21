using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ZipkinTracer.Internal;
using ZipkinTracer.Models;

namespace ZipkinTracer.Http
{
    public class ZipkinMessageHandler : DelegatingHandler
    {
        private readonly IZipkinTracer _client;

        public ZipkinMessageHandler(IZipkinTracer client)
			: this(client, new HttpClientHandler())
        {
        }

        public ZipkinMessageHandler(IZipkinTracer client, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _client = client;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // open new span
            var traceInfo = _client.CreateInnerSpan();

            // start record span
            var span = await _client.StartClientTrace(request.RequestUri, $"{request.Method} {request.RequestUri.AbsolutePath}", traceInfo);
            
            // rewrite traceInfo to request headers
            request.Headers.Add(TraceInfo.TraceIdHeaderName, traceInfo.TraceId);
            request.Headers.Add(TraceInfo.SpanIdHeaderName, traceInfo.SpanId);
            request.Headers.Add(TraceInfo.ParentSpanIdHeaderName, traceInfo.ParentSpanId);
            request.Headers.Add(TraceInfo.SampledHeaderName, traceInfo.IsSampled.ToString());

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // end record span
            _client.EndClientTrace(span, (int)response.StatusCode, IsErrorStatusCode(response.StatusCode) ? response.StatusCode.ToString() : null);

            return response;
        }

		private static bool IsErrorStatusCode(HttpStatusCode statusCode)
		{
			return (int)statusCode >= 400 && (int)statusCode <= 599;
		}
	}
}