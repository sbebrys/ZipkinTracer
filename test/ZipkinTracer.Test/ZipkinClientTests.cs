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
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(null, spanTracer, logger), "traceProvider");
            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(traceProvider, null, logger), "spanTracer");
            Assert.Catch<ArgumentNullException>(() => new ZipkinClient(traceProvider, spanTracer, null), "logger");
        }

        [Test]
        public async Task StartServerTrace_WithoutTraceId_ReturnNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => null);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartServerTrace_IsNotSampled_ReturnNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.IsSampled.Returns(ci => false);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartServerTrace_WithoutMethodName_ReturnNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartServerTrace(null, null));
            Assert.IsNull(await zipkinClient.StartServerTrace(new Uri("http://localhost"), null));
        }

        [Test]
        public async Task StartServerTrace_WithTraceOn_ReceiveSpan()
        {
            var span = new Span();
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            spanTracer.ReceiveServerSpan(null, null, null, null, null).ReturnsForAnyArgs(span);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.AreSame(await zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"), span);
        }

        [Test]
        public void StartServerTrace_WithSpanTracerException_DoesntThrow()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            spanTracer.WhenForAnyArgs(x => x.ReceiveServerSpan(null, null, null, null, null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.DoesNotThrowAsync(() => zipkinClient.StartServerTrace(new Uri("http://localhost"), "POST"));
        }

        [Test]
        public async Task StartClientTrace_WithoutTraceId_ReturnNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => null);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(null, null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceProvider));
        }

        [Test]
        public async Task StartClientTrace_IsNotSampled_ReturnNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => null);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(null, null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceProvider));
        }

        [Test]
        public async Task StartClientTrace_WithTraceOn_ReceiveSpan()
        {
            var span = new Span();
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            spanTracer.SendClientSpan(null, null, null, null, null).ReturnsForAnyArgs(span);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.AreSame(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceProvider), span);
        }

        [Test]
        public async Task StartClientTrace_WithoutTraceProvider_ReceiveNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", null));
        }

        [Test]
        public async Task StartClientTrace_WithoutMethodName_ReceiveNullSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, null));
            Assert.IsNull(await zipkinClient.StartClientTrace(new Uri("http://localhost"), null, traceProvider));
        }

        [Test]
        public void StartClientTrace_WithSpanTracerException_DoesntThrow()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            spanTracer.WhenForAnyArgs(x => x.SendClientSpan(null, null, null, null, null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.DoesNotThrowAsync(() => zipkinClient.StartClientTrace(new Uri("http://localhost"), "POST", traceProvider));
        }

        [Test]
        public void EndServerTrace_WithSpanTracerException_DoesntThrow()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            spanTracer.WhenForAnyArgs(x => x.SendServerSpan(null)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.DoesNotThrow(() => zipkinClient.EndServerTrace(new Span()));
        }

        [Test]
        public void EndServerTrace_WithoutTraceId_NotCallSendServerSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.IsSampled.Returns(ci => false);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            zipkinClient.EndServerTrace(null);
            zipkinClient.EndServerTrace(new Span());

            spanTracer.DidNotReceiveWithAnyArgs().SendServerSpan(null);
        }

        [Test]
        public void EndServerTrace_IsNotSampled_NotCallSendServerSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.IsSampled.Returns(ci => false);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            zipkinClient.EndServerTrace(null);
            zipkinClient.EndServerTrace(new Span());

            spanTracer.DidNotReceiveWithAnyArgs().SendServerSpan(null);
        }

        [Test]
        public void EndServerTrace_WithTraceOn_CallSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);
            zipkinClient.EndServerTrace(null);
            zipkinClient.EndServerTrace(new Span());

            spanTracer.ReceivedWithAnyArgs(2).SendServerSpan(null);
        }

        [Test]
        public void EndClientTrace_WithoutTraceId_NotCallReceiveClientSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            zipkinClient.EndClientTrace(null, 0);
            zipkinClient.EndClientTrace(new Span(), 500);

            spanTracer.DidNotReceiveWithAnyArgs().ReceiveClientSpan(null, 0);
        }

        [Test]
        public void EndClientTrace_IsNotSampled_NotCallReceiveClientSpan()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            zipkinClient.EndClientTrace(null, 0);
            zipkinClient.EndClientTrace(new Span(), 500);

            spanTracer.DidNotReceiveWithAnyArgs().ReceiveClientSpan(null, 0);
        }

        [Test]
        public void EndClientTrace_WithSpanTracerException_DoesntThrow()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            spanTracer.WhenForAnyArgs(x => x.ReceiveClientSpan(null, 0)).Throw<Exception>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            Assert.DoesNotThrow(() => zipkinClient.EndClientTrace(new Span(), 0));
        }

        [Test]
        public void EndClientTrace_WithTraceOn_CallSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.TraceId.Returns(ci => "traceId");
            traceProvider.IsSampled.Returns(ci => true);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);
            zipkinClient.EndClientTrace(null, 0);
            zipkinClient.EndClientTrace(new Span(), 0);

            spanTracer.ReceivedWithAnyArgs(2).ReceiveClientSpan(null, 0);
        }

        [Test]
        public async Task Record_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            await zipkinClient.Record(null);
            await zipkinClient.Record(new Span());
            await zipkinClient.Record(null, "value");
            await zipkinClient.Record(new Span(), "value");

            await spanTracer.DidNotReceiveWithAnyArgs().Record(null, string.Empty);
        }

        [Test]
        public async Task Record_IsNotSampled_NotCallRecordOnSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            await zipkinClient.Record(null);
            await zipkinClient.Record(new Span());
            await zipkinClient.Record(null, "value");
            await zipkinClient.Record(new Span(), "value");
        }

        [Test]
        public async Task RecordLocalComponent_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            await zipkinClient.RecordLocalComponent(null, null);
            await zipkinClient.RecordLocalComponent(null, "value");
            await zipkinClient.RecordLocalComponent(new Span(), "value");

            await spanTracer.DidNotReceiveWithAnyArgs().Record(null, string.Empty);
        }

        [Test]
        public async Task RecordLocalComponent_IsNotSampled_NotCallRecordOnSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            await zipkinClient.RecordLocalComponent(null, null);
            await zipkinClient.RecordLocalComponent(null, "value");
            await zipkinClient.RecordLocalComponent(new Span(), "value");
        }

        [Test]
        public async Task RecordBinary_WithoutTraceId_NotCallRecordOnSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            await zipkinClient.RecordBinary<object>(null, null, null);
            await zipkinClient.RecordBinary<object>(null, "value", null);
            await zipkinClient.RecordBinary<object>(null, "value", new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(new Span(), null, null);
            await zipkinClient.RecordBinary<object>(new Span(), null, new object());
            await zipkinClient.RecordBinary<object>(new Span(), "value", null);
            await zipkinClient.RecordBinary<object>(new Span(), "value", new object());

            await spanTracer.DidNotReceiveWithAnyArgs().RecordBinary(null, null, null);
        }

        [Test]
        public async Task RecordBinary_IsNotSampled_NotCallRecordOnSpanTracer()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);

            await zipkinClient.RecordBinary<object>(null, null, null);
            await zipkinClient.RecordBinary<object>(null, "value", null);
            await zipkinClient.RecordBinary<object>(null, "value", new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(null, null, new object());
            await zipkinClient.RecordBinary<object>(new Span(), null, null);
            await zipkinClient.RecordBinary<object>(new Span(), null, new object());
            await zipkinClient.RecordBinary<object>(new Span(), "value", null);
            await zipkinClient.RecordBinary<object>(new Span(), "value", new object());

            await spanTracer.DidNotReceiveWithAnyArgs().RecordBinary(null, null, null);
        }

        [Test]
        public void GetNextTrace_Return_NextTrace()
        {
            var traceProvider = Substitute.For<ITraceProvider>();
            var nextTraceProvider = Substitute.For<ITraceProvider>();
            var spanTracer = Substitute.For<ISpanTracer>();
            var logger = Substitute.For<ILogger<ZipkinClient>>();

            traceProvider.GetNext().Returns(x => nextTraceProvider);

            var zipkinClient = new ZipkinClient(traceProvider, spanTracer, logger);
            var retTaceProvider = zipkinClient.GetNextTrace();

            Assert.AreSame(retTaceProvider, nextTraceProvider);
        }
    }
}
