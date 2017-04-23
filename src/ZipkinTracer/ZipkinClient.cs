using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZipkinTracer.Models;
using ZipkinTracer.Models.References;
using ZipkinTracer.Helpers;
using ZipkinTracer.Internal;

namespace ZipkinTracer
{
    /// <summary>
    /// Zipkin Tracer client
    /// </summary>
    public class ZipkinClient : IZipkinTracer
    {
        private readonly ISpanTracer _spanTracer;
        private readonly ITraceInfoAccessor _traceInfoAccessor;
        private readonly ZipkinConfig _zipkinConfig;
        private readonly ILogger<ZipkinClient> _logger;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="zipkinConfig"></param>
        /// <param name="traceInfoAccessor"></param>
        /// <param name="spanTracer"></param>
        /// <param name="logger"></param>
        public ZipkinClient(ZipkinConfig zipkinConfig, ITraceInfoAccessor traceInfoAccessor, ISpanTracer spanTracer,
            ILogger<ZipkinClient> logger)
        {
            if (zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));
            if (traceInfoAccessor == null) throw new ArgumentNullException(nameof(traceInfoAccessor));
            if (spanTracer == null) throw new ArgumentNullException(nameof(spanTracer));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _zipkinConfig = zipkinConfig;
            _logger = logger;
            _spanTracer = spanTracer;
            _traceInfoAccessor = traceInfoAccessor;
        }

        /// <summary>
        /// Start client trace
        /// </summary>
        /// <param name="remoteUri"></param>
        /// <param name="methodName"></param>
        /// <returns>Span of client trace</returns>
        public async Task<Span> StartClientTrace(Uri remoteUri, string methodName)
        {
            if (_traceInfoAccessor.TraceInfo == null)
                return null;

            // new trace info
            var traceInfo = new TraceInfo(_traceInfoAccessor.TraceInfo);

            // set in current context
            _traceInfoAccessor.TraceInfo = traceInfo;

            if (!traceInfo.IsTraceOn || !_zipkinConfig.Enabled || string.IsNullOrEmpty(methodName))
                return null;

            try
            {
                return await _spanTracer.SendClientSpan(methodName.ToLower(), traceInfo, remoteUri);
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
        /// <param name="errorMessage"></param>
        public void EndClientTrace(Span clientSpan, int statusCode, string errorMessage = null)
        {
            try
            {
                if (clientSpan?.TraceInfo == null)
                    return;

                _spanTracer.ReceiveClientSpan(clientSpan, statusCode, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, "Error Ending Client Trace");
            }
            finally
            {
                _traceInfoAccessor.TraceInfo = clientSpan?.TraceInfo?.ParentTraceInfo;
            }
        }

        /// <summary>
        /// Start server trace
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="spanName"></param>
        /// <param name="traceInfo"></param>
        /// <returns></returns>
        public async Task<Span> StartServerTrace(Uri requestUri, string spanName, TraceInfo traceInfo)
        {
            try
            {
                _traceInfoAccessor.TraceInfo = traceInfo;

                if (traceInfo == null || !traceInfo.IsTraceOn || !_zipkinConfig.Enabled || string.IsNullOrEmpty(spanName))
                    return null;

                return await _spanTracer.ReceiveServerSpan(spanName.ToLower(), traceInfo, requestUri);
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
        /// <param name="statusCode"></param>
        /// <param name="errorMessage"></param>
        public void EndServerTrace(Span serverSpan, int statusCode, string errorMessage = null)
        {
            if (serverSpan?.TraceInfo == null)
                return;

            try
            {
                _spanTracer.SendServerSpan(serverSpan, statusCode, errorMessage);
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
            if (span?.TraceInfo == null)
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
            if (span?.TraceInfo == null)
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
            if (string.IsNullOrEmpty(span?.TraceId))
                return;

            try
            {
                await _spanTracer.RecordBinary(span, TraceKeys.LocalComponent, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(0), ex, $"Error recording local trace (value: {value})");
            }
        }
    }
}