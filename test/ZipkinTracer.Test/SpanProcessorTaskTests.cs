using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class SpanProcessorTaskTests
    {
        [Test]
        public void Ctor_NullArguments_ThrowException()
        {
            var logger = Substitute.For<ILogger<SpanProcessorTask>>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            Assert.Catch<ArgumentNullException>(() => new SpanProcessorTask(null, logger), "zipkinConfig");
            Assert.Catch<ArgumentNullException>(() => new SpanProcessorTask(config, null), "logger");
        }

        [Test]
        public void Start_NullAction_IsNotRunning()
        {
            var logger = Substitute.For<ILogger<SpanProcessorTask>>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessorTask(config, logger);
            sp.Start(null);

            Assert.IsFalse(sp.IsRunning);
        }

        [Test]
        public void Start_Action_IsExecuting()
        {
            var isExecute = false;
            var logger = Substitute.For<ILogger<SpanProcessorTask>>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessorTask(config, logger);
            sp.Start(() => Task.Run(() => isExecute = true));

            Thread.Sleep(200);

            sp.Stop();

            Assert.IsTrue(isExecute);
        }

        [Test]
        public void Start_ActionWithException_IsNotRethrow()
        {
            var logger = Substitute.For<ILogger<SpanProcessorTask>>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessorTask(config, logger);
            Assert.DoesNotThrow(() => sp.Start(() => Task.Run(() => { throw new Exception(); })));
            sp.Stop();
        }

        [Test]
        public void Start_StoppedProcessor_ActionIsNotCalled()
        {
            var isExecute = false;
            var logger = Substitute.For<ILogger<SpanProcessorTask>>();
            var config = new ZipkinConfig(new Uri("http://localhost")) { SpanProcessorBatchSize = 10 };

            var sp = new SpanProcessorTask(config, logger);
            sp.Stop();
            sp.Start(() => Task.Run(() => isExecute = true));

            Assert.IsFalse(isExecute);
        }
    }
}
