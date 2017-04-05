using System;
using System.Collections.Concurrent;
using ZipkinTracer.Models;

namespace ZipkinTracer.Internal
{
    internal class SpanCollector : ISpanCollector
    {
        private readonly BlockingCollection<Span> _spanQueue;

        public SpanCollector(BlockingCollection<Span> spanQueue)
        {
            if (spanQueue == null) throw new ArgumentNullException(nameof(spanQueue));
            _spanQueue = spanQueue;
        }

        public void Collect(Span span)
        {
            if (span != null)
            {
                _spanQueue.Add(span);
            }
        }

        public bool TryTake(out Span span)
        {
            return _spanQueue.TryTake(out span);
        }
    }
}