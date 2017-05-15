using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZipkinTracer.Helpers;
using ZipkinTracer.Models;
using ZipkinTracer.Models.Serialization.Json;

namespace ZipkinTracer.Internal
{
    internal class SpanProcessor : ISpanProcessor
    {
        //send contents of queue if it has pending items but less than max batch size after doing max number of polls
        private const int MaxNumberOfPolls = 5;
        private const string ZipkinSpanPostPath = "/api/v1/spans";

        private readonly ISpanCollector _spanCollector;
        private readonly ZipkinConfig _zipkinConfig;
        private readonly ILogger<SpanProcessor> _logger;
        private readonly ISpanProcessorTask _spanProcessorTask;
        private readonly object _syncObj = new object();

        //using a queue because even as we pop items to send to zipkin, another 
        //thread can be adding spans if someone shares the span processor accross threads
        private readonly ConcurrentQueue<JsonSpan> _serializableSpans;

        private int _subsequentPollCount;

        public bool IsStarted { get; private set; }

        public SpanProcessor(ISpanProcessorTask spanProcessorTask, ISpanCollector spanCollector,
            ZipkinConfig zipkinConfig, ILogger<SpanProcessor> logger)
        {
            if (spanProcessorTask == null) throw new ArgumentNullException(nameof(spanProcessorTask));
            if (spanCollector == null) throw new ArgumentNullException(nameof(spanCollector));
            if (zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _zipkinConfig = zipkinConfig;
            _spanCollector = spanCollector;
            _serializableSpans = new ConcurrentQueue<JsonSpan>();
            _logger = logger;
            _spanProcessorTask = spanProcessorTask;
        }

        public Task Stop()
        {
            return SyncHelper.ExecuteSafelyAsync(_syncObj, () => IsStarted, () =>
            {
                _spanProcessorTask.Stop();
                IsStarted = false;
                return LogSubmittedSpans();
            });
        }

        public Task Start()
        {
            SyncHelper.ExecuteSafely(_syncObj, () => !IsStarted, () =>
            {
                _spanProcessorTask.Start(LogSubmittedSpans);
                IsStarted = true;
            });
            return Task.CompletedTask;
        }

        private async Task LogSubmittedSpans()
        {
            var anyNewSpans = ProcessQueuedSpans();

            if (anyNewSpans)
                _subsequentPollCount = 0;
            else if (_serializableSpans.Count > 0)
                _subsequentPollCount++;

            if (ShouldSendQueuedSpansOverWire())
            {
                await SendSpansOverHttp();
            }
        }

        private async Task SendSpansToZipkin(string spans)
        {
            if (spans == null)
                throw new ArgumentNullException(nameof(spans));

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = _zipkinConfig.ZipkinBaseUri;
                    var response = await client.PostAsync(ZipkinSpanPostPath, new StringContent(spans));

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError(
                            "Failed to send spans to Zipkin server (HTTP status code returned: {0}). Response from server: {1}",
                            response.StatusCode, await response.Content.ReadAsStringAsync());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(0), ex, ex.Message);
                    throw;
                }
            }
        }

        private bool ShouldSendQueuedSpansOverWire()
        {
            return _serializableSpans.Any() &&
                   (_serializableSpans.Count >= _zipkinConfig.SpanProcessorBatchSize
                    || _spanProcessorTask.IsCancelled
                    || _subsequentPollCount > MaxNumberOfPolls);
        }

        private bool ProcessQueuedSpans()
        {
            Span span;
            var anyNewSpansQueued = false;
            while (_spanCollector.TryTake(out span))
            {
                _serializableSpans.Enqueue(new JsonSpan(span));
                anyNewSpansQueued = true;
            }
            return anyNewSpansQueued;
        }

        private async Task SendSpansOverHttp()
        {
            var spansJsonRepresentation = GetSpansJsonRepresentation();
            await SendSpansToZipkin(spansJsonRepresentation);
            _subsequentPollCount = 0;
        }

        private string GetSpansJsonRepresentation()
        {
            JsonSpan span;
            var spanList = new List<JsonSpan>();
            //using Dequeue into a list so that the span is removed from the queue as we add it to list
            while (_serializableSpans.TryDequeue(out span))
            {
                spanList.Add(span);
            }
            var spansJsonRepresentation = JsonConvert.SerializeObject(spanList);
            return spansJsonRepresentation;
        }
    }
}