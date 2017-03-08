using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ZipkinTracer.Internal;

namespace ZipkinTracer.Http
{
    public class ZipkinMessageHandler : DelegatingHandler
    {
        private readonly IZipkinTracer _client;

        public ZipkinMessageHandler(IZipkinTracer client)
			:this(client, new HttpClientHandler())
        {
        }

        public ZipkinMessageHandler(IZipkinTracer client, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _client = client;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_client.IsTraceOn)
            {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            var nextTrace = _client.GetNextTrace();

            request.Headers.Add(TraceProvider.TraceIdHeaderName, nextTrace.TraceId);
            request.Headers.Add(TraceProvider.SpanIdHeaderName, nextTrace.SpanId);
            request.Headers.Add(TraceProvider.ParentSpanIdHeaderName, nextTrace.ParentSpanId);
            request.Headers.Add(TraceProvider.SampledHeaderName, nextTrace.ParentSpanId);

            var span = await _client.StartClientTrace(request.RequestUri, request.Method.ToString(), nextTrace);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            _client.EndClientTrace(span, (int)response.StatusCode);

            return response;
        }
    }
}