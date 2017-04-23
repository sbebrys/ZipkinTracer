using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            var traceInfo = new TraceInfo(string.Empty, string.Empty, true, false, null, null, string.Empty);

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null, traceInfo));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null, traceInfo));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST", traceInfo));
        }

        [Test]
        public async Task StartServerTrace_IsNotSampled_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			var traceInfo = new TraceInfo(string.Empty, string.Empty, false, false, null, null, string.Empty);

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null, traceInfo));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null, traceInfo));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST", traceInfo));
        }

        [Test]
        public async Task StartServerTrace_WithoutMethodName_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null, traceInfo));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null, traceInfo));
        }

        [Test]
        public async Task StartServerTrace_WithTraceOn_ReceiveSpan()
        {
            var traceInfo = new TraceInfo(string.Empty, string.Empty, true, false, new Uri("http://localhost"), null);
            var span = new Span("span", traceInfo);
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			var traceInfoServer = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);

            spanTracer.ReceiveServerSpan(null, null, null).ReturnsForAnyArgs(span);

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.AreSame(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST", traceInfoServer), span);
        }

        [Test]
        public void StartServerTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);

			spanTracer.WhenForAnyArgs(x => x.ReceiveServerSpan(null, null, null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrowAsync(() => zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST", traceInfo));
        }

        [Test]
        public async Task StartClientTrace_WithoutTraceId_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo(null, string.Empty, true, false, null, null, string.Empty));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartClientTrace_IsNotSampled_ReturnNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo(null, string.Empty, false, false, null, null, string.Empty));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartClientTrace_WithTraceOn_ReceiveSpan()
        {
            var traceInfo = new TraceInfo("traceId", string.Empty, true, false, new Uri("http://localhost"), null);
            var span = new Span("span", traceInfo);
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty));

			spanTracer.SendClientSpan(null, null, null).ReturnsForAnyArgs(span);

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.AreSame(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST"), span);
        }

        [Test]
        public async Task StartClientTrace_WithouttraceInfoAccessor_ReceiveNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartClientTrace_WithoutMethodName_ReceiveNullSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty));

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

	        Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null));
        }

        [Test]
        public void StartClientTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty));

			spanTracer.WhenForAnyArgs(x => x.SendClientSpan(null, null, null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrowAsync(() => zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public void EndServerTrace_WithSpanTracerException_DoesntThrow()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);

            traceInfoAccessor.TraceInfo.Returns(ci => traceInfo);

			spanTracer.WhenForAnyArgs(x => x.SendServerSpan(null, 0)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrow(() => zipkinClient.EndServerTrace(new Span("span", traceInfo), 200));
        }

        [Test]
        public void EndServerTrace_WithoutTraceId_NotCallSendServerSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var traceInfo = new TraceInfo("traceId", string.Empty, false, false, null, null, string.Empty);
            traceInfoAccessor.TraceInfo.Returns(ci => traceInfo);

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            zipkinClient.EndServerTrace(null, 200);
            zipkinClient.EndServerTrace(new Span("span", null), 200);

            spanTracer.DidNotReceiveWithAnyArgs().SendServerSpan(null, 200);
        }

        [Test]
        public void EndServerTrace_ForNullSpan_NotCallSendServerSpan()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

			traceInfoAccessor.TraceInfo.Returns(ci => new TraceInfo("traceId", string.Empty, false, false, null, null, string.Empty));

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

            var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);
            traceInfoAccessor.TraceInfo.Returns(ci => traceInfo);

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);
            zipkinClient.EndServerTrace(null, 200);
            zipkinClient.EndServerTrace(new Span("span", traceInfo), 200);

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
            zipkinClient.EndClientTrace(new Span("span", null), 500);

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

            var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);
            traceInfoAccessor.TraceInfo.Returns(ci => traceInfo);

			spanTracer.WhenForAnyArgs(x => x.ReceiveClientSpan(null, 0)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);

            Assert.DoesNotThrow(() => zipkinClient.EndClientTrace(new Span("span", traceInfo), 0));
        }

        [Test]
        public void EndClientTrace_WithTraceOn_CallSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);
            traceInfoAccessor.TraceInfo.Returns(ci => traceInfo);

			var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);
            zipkinClient.EndClientTrace(null, 0);
            zipkinClient.EndClientTrace(new Span("span", traceInfo), 0);

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
            await zipkinClient.Record(new Span("span", null));
            await zipkinClient.Record(null, "value");
            await zipkinClient.Record(new Span("span", null), "value");

            await spanTracer.DidNotReceiveWithAnyArgs().Record(null, string.Empty);
        }

        [Test]
        public async Task RecordLocalComponent_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var traceInfoAccessor = Substitute.For<ITraceInfoAccessor>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(zipkinConfig, traceInfoAccessor, spanTracer, logger);
            var traceInfo = new TraceInfo("traceId", string.Empty, true, false, null, null, string.Empty);

            await zipkinClient.RecordLocalComponent(null, null);
            await zipkinClient.RecordLocalComponent(null, "value");
            await zipkinClient.RecordLocalComponent(new Span("span", traceInfo), "value");

            await spanTracer.DidNotReceiveWithAnyArgs().Record(null, string.Empty);
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
            await zipkinClient.RecordBinary<object>(new Span("span", null), null, null);
            await zipkinClient.RecordBinary<object>(new Span("span", null), null, new object());
            await zipkinClient.RecordBinary<object>(new Span("span", null), "value", null);
            await zipkinClient.RecordBinary<object>(new Span("span", null), "value", new object());

            await spanTracer.DidNotReceiveWithAnyArgs().RecordBinary(null, null, null);
        }
    }
}
