using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZipkinTracer.Models;
using ZipkinTracer.Models.References;

namespace ZipkinTracer
{
    /// <summary>
    /// Zipkin Tracer client
    /// </summary>
    public class ZipkinClient : ITracerClient
    {
        private readonly ISpanTracer _spanTracer;
        private readonly ITraceProvider _traceProvider;
        private readonly ILogger<ZipkinClient> _logger;

        public bool IsTraceOn => !string.IsNullOrEmpty(_traceProvider.TraceId) && _traceProvider.IsSampled;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="traceProvider"></param>
        /// <param name="spanTracer"></param>
        /// <param name="logger"></param>
        public ZipkinClient(ITraceProvider traceProvider, ISpanTracer spanTracer, ILogger<ZipkinClient> logger)
        {
            if (traceProvider == null) throw new ArgumentNullException(nameof(traceProvider));
            if (spanTracer == null) throw new ArgumentNullException(nameof(spanTracer));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            _spanTracer = spanTracer;
            _traceProvider = traceProvider;
        }

        /// <summary>
        /// Start client trace
        /// </summary>
        /// <param name="remoteUri"></param>
        /// <param name="methodName"></param>
        /// <param name="traceProvider"></param>
        /// <returns></returns>
        public async Task<Span> StartClientTrace(Uri remoteUri, string methodName, ITraceProvider traceProvider)
        {
            if (!IsTraceOn || traceProvider == null || string.IsNullOrEmpty(methodName))
                return null;

            try
            {
                return await _spanTracer.SendClientSpan(
                    methodName.ToLower(),
                    traceProvider.TraceId,
                    traceProvider.ParentSpanId,
                    traceProvider.SpanId,
                    remoteUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, "Error Starting Client Trace");
                return null;
            }
        }

        /// <summary>
        /// Start server trace
        /// </summary>
        /// <param name="clientSpan"></param>
        /// <param name="statusCode"></param>
        public void EndClientTrace(Span clientSpan, int statusCode)
        {
            if (!IsTraceOn)
                return;

            try
            {
                _spanTracer.ReceiveClientSpan(clientSpan, statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, "Error Ending Client Trace");
            }
        }

        /// <summary>
        /// Start server trace
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public async Task<Span> StartServerTrace(Uri requestUri, string methodName)
        {
            if (!IsTraceOn || string.IsNullOrEmpty(methodName))
                return null;

            try
            {
                return await _spanTracer.ReceiveServerSpan(
                    methodName.ToLower(),
                    _traceProvider.TraceId,
                    _traceProvider.ParentSpanId,
                    _traceProvider.SpanId,
                    requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, "Error Starting Server Trace");
                return null;
            }
        }

        /// <summary>
        /// End server trace
        /// </summary>
        /// <param name="serverSpan"></param>
        public void EndServerTrace(Span serverSpan)
        {
            if (!IsTraceOn)
                return;

            try
            {
                _spanTracer.SendServerSpan(serverSpan);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, "Error Ending Server Trace");
            }
        }

        /// <summary>
        /// Records an annotation with the current timestamp and the provided value in the span.
        /// </summary>
        /// <param name="span">The span where the annotation will be recorded.</param>
        /// <param name="value">The value of the annotation to be recorded. If this parameter is omitted
        /// (or its value set to null), the method caller member name will be automatically passed.</param>
        public async Task Record(Span span, [CallerMemberName] string value = null)
        {
            if (!IsTraceOn)
                return;

            try
            {
                await _spanTracer.Record(span, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, "Error recording the annotation");
            }
        }

        /// <summary>
        /// Records a key-value pair as a binary annotiation in the span.
        /// </summary>
        /// <typeparam name="T">The type of the value to be recorded. See remarks for the currently supported types.</typeparam>
        /// <param name="span">The span where the annotation will be recorded.</param>
        /// <param name="key">The key which is a reference to the recorded value.</param>
        /// <param name="value">The value of the annotation to be recorded.</param>
        /// <remarks>The RecordBinary will record a key-value pair which can be used to tag some additional information
        /// in the trace without any timestamps. The currently supported value types are <see cref="bool"/>,
        /// <see cref="T:byte[]"/>, <see cref="short"/>, <see cref="int"/>, <see cref="long"/>, <see cref="double"/> and
        /// <see cref="string"/>. Any other types will be passed as string annotation types.
        /// 
        /// Please note, that although the values have types, they will be recorded and sent by calling their
        /// respective ToString() method.</remarks>
        public async Task RecordBinary<T>(Span span, string key, T value)
        {
            if (!IsTraceOn)
                return;

            try
            {
                await _spanTracer.RecordBinary(span, key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, $"Error recording a binary annotation (key: {key})");
            }
        }

        /// <summary>
        /// Records a local component annotation in the span.
        /// </summary>
        /// <param name="span">The span where the annotation will be recorded.</param>
        /// <param name="value">The value of the local trace to be recorder.</param>
        public async Task RecordLocalComponent(Span span, string value)
        {
            if (!IsTraceOn)
                return;

            try
            {
                await _spanTracer.RecordBinary(span, ZipkinConstants.LocalComponent, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, $"Error recording local trace (value: {value})");
            }
        }

        public ITraceProvider GetNextTrace()
        {
            return _traceProvider.GetNext();
        }
    }
}
