using System;
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
			// start record span
			var span = await _client.StartClientTrace(request.RequestUri, $"{request.Method} {request.RequestUri.AbsolutePath}");
			var traceInfo = _client.GetCurrentTraceInfo();

			try
			{
				// rewrite traceInfo to request headers
				request.Headers.Add(TraceInfo.TraceIdHeaderName, traceInfo.TraceId);
				request.Headers.Add(TraceInfo.SpanIdHeaderName, traceInfo.SpanId);
				request.Headers.Add(TraceInfo.ParentSpanIdHeaderName, traceInfo.ParentSpanId);
				request.Headers.Add(TraceInfo.SampledHeaderName, traceInfo.IsSampled.ToString());

				request.Properties[TraceInfo.TraceInfoKey] = traceInfo;

				var response = await base.SendAsync(request, cancellationToken);

				// end record span
				_client.EndClientTrace(span, (int)response.StatusCode, IsErrorStatusCode(response.StatusCode) ? response.StatusCode.ToString() : null);

				return response;
			}
			catch (Exception ex)
			{
				// end record span
				_client.EndClientTrace(span, 500, ex.Message?.Substring(0, Math.Min(ex.Message.Length, 128)) ?? HttpStatusCode.InternalServerError.ToString());

				// rethrow
				throw;
			}
		}

		private static bool IsErrorStatusCode(HttpStatusCode statusCode)
		{
			return (int)statusCode >= 400 && (int)statusCode <= 599;
		}
	}
}