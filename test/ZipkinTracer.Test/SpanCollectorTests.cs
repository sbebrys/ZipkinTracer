using System;
using System.Collections.Concurrent;
using System.Net;
using NUnit.Framework;
using ZipkinTracer.Internal;
using ZipkinTracer.Models;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class SpanCollectorTests
    {
        [Test]
        public void Ctor_NullQueue_ThrowException()
        {
			Assert.Catch<ArgumentNullException>(() => new SpanCollector(null), "spanQueue");
        }

        [Test]
        public void Collect_NullSpan_NotCollect()
        {
            var collection = new BlockingCollection<Span>();
            var collector = new SpanCollector(collection);

            collector.Collect(null);

            Assert.AreEqual(collection.Count, 0);
        }

        [Test]
        public void Collect_NewSpan_AddedToCollection()
        {
            var collection = new BlockingCollection<Span>();
            var collector = new SpanCollector(collection);

            var traceInfo = new TraceInfo(string.Empty, string.Empty, true, false, null, IPAddress.Loopback);
            collector.Collect(new Span(string.Empty, traceInfo));

            Assert.AreEqual(collection.Count, 1);
        }

        [Test]
        public void TryTake_EmptyCollection_NotReturnSpan()
        {
            var collection = new BlockingCollection<Span>();
            var collector = new SpanCollector(collection);

            Span span;
            var ret = collector.TryTake(out span);

            Assert.IsFalse(ret);
            Assert.IsNull(span);
        }

        [Test]
        public void TryTake_NotEmptyCollection_ReturnSpan()
        {
            var traceInfo = new TraceInfo(string.Empty, "TestSpanId", true, false, null, IPAddress.Loopback);
            var span = new Span(string.Empty, traceInfo);

			var collection = new BlockingCollection<Span>();
            var collector = new SpanCollector(collection);

            collection.Add(span);

            Span spanTaken;
            var ret = collector.TryTake(out spanTaken);

            Assert.IsTrue(ret);
            Assert.AreSame(spanTaken, span);
        }

        [Test]
        public void TryTake_NotEmptyCollection_ClearCollection()
        {
            var traceInfo = new TraceInfo(string.Empty, "TestSpanId", true, false, null, IPAddress.Loopback);
            var span = new Span(string.Empty, traceInfo);
            var collection = new BlockingCollection<Span>();
            var collector = new SpanCollector(collection);

            collection.Add(span);

            Assert.AreEqual(collection.Count, 1);

            Span spanTaken;
            collector.TryTake(out spanTaken);

            Assert.AreEqual(collection.Count, 0);
        }
    }
}
