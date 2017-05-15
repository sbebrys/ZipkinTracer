using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using ZipkinTracer.Internal;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class SpanProcessorTests
    {
        [Test]
        public void Ctor_NullArguments_ThrowException()
        {
            var logger = Substitute.For<ILogger<SpanProcessor>>();
            var spanProcessorTask = Substitute.For<ISpanProcessorTask>();
            var spanCollector = Substitute.For<ISpanCollector>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            Assert.Catch<ArgumentNullException>(() => new SpanProcessor(null, spanCollector, config, logger), "spanProcessorTask");
            Assert.Catch<ArgumentNullException>(() => new SpanProcessor(spanProcessorTask, null, config, logger), "spanCollector");
            Assert.Catch<ArgumentNullException>(() => new SpanProcessor(spanProcessorTask, spanCollector, null, logger), "zipkinConfig");
            Assert.Catch<ArgumentNullException>(() => new SpanProcessor(spanProcessorTask, spanCollector, config, null), "logger");
        }

        [Test]
        public void Ctor_DefaultProcessor_NotStarted()
        {
            var logger = Substitute.For<ILogger<SpanProcessor>>();
            var spanProcessorTask = Substitute.For<ISpanProcessorTask>();
            var spanCollector = Substitute.For<ISpanCollector>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessor(spanProcessorTask, spanCollector, config, logger);

            Assert.IsFalse(sp.IsStarted);
        }

        [Test]
        public async Task Start_PropertyIsStarted_IsTrue()
        {
            var logger = Substitute.For<ILogger<SpanProcessor>>();
            var spanProcessorTask = Substitute.For<ISpanProcessorTask>();
            var spanCollector = Substitute.For<ISpanCollector>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessor(spanProcessorTask, spanCollector, config, logger);

            await sp.Start();

            Assert.IsTrue(sp.IsStarted);
        }

        [Test]
        public async Task Stop_PropertyIsStarted_IsFalse()
        {
            var logger = Substitute.For<ILogger<SpanProcessor>>();
            var spanProcessorTask = Substitute.For<ISpanProcessorTask>();
            var spanCollector = Substitute.For<ISpanCollector>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessor(spanProcessorTask, spanCollector, config, logger);

            await sp.Start();
            await sp.Stop();

            Assert.IsFalse(sp.IsStarted);
        }

        [Test]
        public async Task Start_SpanProcessorTask_IsStarted()
        {
            var started = false;
            var logger = Substitute.For<ILogger<SpanProcessor>>();
            var spanProcessorTask = Substitute.For<ISpanProcessorTask>();
            var spanCollector = Substitute.For<ISpanCollector>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            spanProcessorTask.WhenForAnyArgs(t => t.Start(null)).Do(ci => started = true);

            var sp = new SpanProcessor(spanProcessorTask, spanCollector, config, logger);

            await sp.Start();

            Assert.IsTrue(started);
        }

        [Test]
        public async Task Stop_SpanProcessorTask_IsStopped()
        {
            var stopped = false;
            var logger = Substitute.For<ILogger<SpanProcessor>>();
            var spanProcessorTask = Substitute.For<ISpanProcessorTask>();
            var spanCollector = Substitute.For<ISpanCollector>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            spanProcessorTask.WhenForAnyArgs(t => t.Stop()).Do(ci => stopped = true);

            var sp = new SpanProcessor(spanProcessorTask, spanCollector, config, logger);

            await sp.Start();
            await sp.Stop();

            Assert.IsTrue(stopped);
        }
    }
}
