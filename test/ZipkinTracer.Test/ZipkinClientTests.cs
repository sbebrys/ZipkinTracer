using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ZipkinTracer.Models;
using ZipkinTracer.Models.References;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class ZipkinClientTests
    {
        [Test]
        public void Ctor_NullArguments_ThrowException()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(null, traceInfoAccessor, spanTracer, logger), "zipkinConfig");
            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(zipkinConfig, null, spanTracer, logger), "traceInfoAccessor");
            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(zipkinConfig, traceInfoAccessor, null, logger), "spanTracer");
            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, null), "logger");
        }

        [Test]
        public async Task StartServerTrace_WithoutTraceId_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo(string.Empty, string.Empty, string.Empty, true, null));

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartServerTrace_IsNotSampled_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo(string.Empty, string.Empty, string.Empty, false, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartServerTrace_WithoutMethodName_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null));
        }

        [Test]
        public async Task StartServerTrace_WithTraceOn_ReceiveSpan()
        {
            var span = new Span("span", string.Empty, string.Empty, string.Empty, new Uri("http://localhost"));
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

            spanTracer.ReceiveServerSpan(null, null, null).ReturnsForAnyArgs(span);

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.AreSame(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"), span);
        }

        [Test]
        public void StartServerTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			spanTracer.WhenForAnyArgs(x => x.ReceiveServerSpan(null, null, null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrowAsync(() => zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartClientTrace_WithoutTraceId_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo(null, string.Empty, string.Empty, true, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(null, null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceInfoAccessor.TraceInfo));
        }

        [Test]
        public async Task StartClientTrace_IsNotSampled_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo(null, string.Empty, string.Empty, false, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(null, null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceInfoAccessor.TraceInfo));
        }

        [Test]
        public async Task StartClientTrace_WithTraceOn_ReceiveSpan()
        {
            var span = new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost"));
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			spanTracer.SendClientSpan(null, null, null).ReturnsForAnyArgs(span);

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.AreSame(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceInfoAccessor.TraceInfo), span);
        }

        [Test]
        public async Task StartClientTrace_WithouttraceInfoAccessor_ReceiveNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", null));
        }

        [Test]
        public async Task StartClientTrace_WithoutMethodName_ReceiveNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, traceInfoAccessor.TraceInfo));
        }

        [Test]
        public void StartClientTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			spanTracer.WhenForAnyArgs(x => x.SendClientSpan(null, null, null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrowAsync(() => zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceInfoAccessor.TraceInfo));
        }

        [Test]
        public void EndServerTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			spanTracer.WhenForAnyArgs(x => x.SendServerSpan(null, 0)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrow(() => zipkinClient.EndServerTrace(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), 200));
        }

        [Test]
        public void EndServerTrace_WithoutTraceId_NotCallSendServerSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, false, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            zipkinClient.EndServerTrace(null, 200);
            zipkinClient.EndServerTrace(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), 200);

            spanTracer.DidNotReceiveWithAnyArgs().SendServerSpan(null, 200);
        }

        [Test]
        public void EndServerTrace_ForNullSpan_NotCallSendServerSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, false, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            zipkinClient.EndServerTrace(null, 200);

            spanTracer.DidNotReceiveWithAnyArgs().SendServerSpan(null, 200);
        }

        [Test]
        public void EndServerTrace_WithTraceOn_CallSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);
            zipkinClient.EndServerTrace(null, 200);
            zipkinClient.EndServerTrace(new Span("span", "id", string.Empty, "traceId", new Uri("http://localhost")), 200);

            spanTracer.ReceivedWithAnyArgs(1).SendServerSpan(null, 200);
        }

        [Test]
        public void EndClientTrace_WithoutTraceId_NotCallReceiveClientSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            zipkinClient.EndClientTrace(null, 0);
            zipkinClient.EndClientTrace(new Span("span", null, string.Empty, string.Empty, new Uri("http://localhost")), 500);

            spanTracer.DidNotReceiveWithAnyArgs().ReceiveClientSpan(null, 0);
        }

        [Test]
        public void EndClientTrace_ForNullSpan_NotCallReceiveClientSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            zipkinClient.EndClientTrace(null, 0);

            spanTracer.DidNotReceiveWithAnyArgs().ReceiveClientSpan(null, 0);
        }

        [Test]
        public void EndClientTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			spanTracer.WhenForAnyArgs(x => x.ReceiveClientSpan(null, 0)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrow(() => zipkinClient.EndClientTrace(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), 0));
        }

        [Test]
        public void EndClientTrace_WithTraceOn_CallSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, string.Empty, true, null));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);
            zipkinClient.EndClientTrace(null, 0);
            zipkinClient.EndClientTrace(new Span("span", "id", string.Empty, "traceId", new Uri("http://localhost")), 0);

            spanTracer.ReceivedWithAnyArgs(1).ReceiveClientSpan(null, 0);
        }

        [Test]
        public async Task Record_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            await zipkinClient.Record(null);
            await zipkinClient.Record(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")));
            await zipkinClient.Record(null, "value");
            await zipkinClient.Record(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value");

            await spanTracer.DidNotReceiveWithAnyArgs().Record(null, string.Empty);
        }

        [Test]
        public async Task Record_IsNotSampled_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            await zipkinClient.Record(null);
            await zipkinClient.Record(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")));
            await zipkinClient.Record(null, "value");
            await zipkinClient.Record(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value");
        }

        [Test]
        public async Task RecordLocalComponent_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            await zipkinClient.RecordLocalComponent(null, null);
            await zipkinClient.RecordLocalComponent(null, "value");
            await zipkinClient.RecordLocalComponent(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value");

            await spanTracer.DidNotReceiveWithAnyArgs().Record(null, string.Empty);
        }

        [Test]
        public async Task RecordLocalComponent_IsNotSampled_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            await zipkinClient.RecordLocalComponent(null, null);
            await zipkinClient.RecordLocalComponent(null, "value");
            await zipkinClient.RecordLocalComponent(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value");
        }

        [Test]
        public async Task RecordBinary_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            await zipkinClient.RecordBinary<object>(null, null, null);
            await zipkinClient.RecordBinary<object>(null, "value", null);
            await zipkinClient.RecordBinary<object>(null, "value", new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), null, null);
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), null, new object());
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value", null);
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value", new object());

            await spanTracer.DidNotReceiveWithAnyArgs().RecordBinary(null, null, null);
        }

        [Test]
        public async Task RecordBinary_IsNotSampled_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            await zipkinClient.RecordBinary<object>(null, null, null);
            await zipkinClient.RecordBinary<object>(null, "value", null);
            await zipkinClient.RecordBinary<object>(null, "value", new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), null, null);
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), null, new object());
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value", null);
            await zipkinClient.RecordBinary<object>(new Span("span", "traceId", string.Empty, string.Empty, new Uri("http://localhost")), "value", new object());

            await spanTracer.DidNotReceiveWithAnyArgs().RecordBinary(null, null, null);
        }

        [Test]
        public void GetNextTrace_Return_NextTrace()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

	        var traceInfo = new TraceInfo("traceId", "spanId", "parentSpanId", true, new Uri("http://localhost"));
			traceInfoAccessor.TraceInfo.Returns(ci => traceInfo);

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);
            var innerSpanTraceInfo = zipkinClient.CreateInnerSpan();

            Assert.AreEqual(traceInfo.TraceId, innerSpanTraceInfo.TraceId);
            Assert.AreEqual(traceInfo.SpanId, innerSpanTraceInfo.ParentSpanId);
            Assert.AreEqual(traceInfo.Domain, innerSpanTraceInfo.Domain);
            Assert.AreEqual(traceInfo.IsSampled, innerSpanTraceInfo.IsSampled);
			Assert.IsNotEmpty(innerSpanTraceInfo.SpanId);
			Assert.AreNotEqual(traceInfo.SpanId, innerSpanTraceInfo.SpanId);
		}
    }
}
