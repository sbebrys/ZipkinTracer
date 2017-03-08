using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;
using ZipkinTracer.Internal;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class TraceProviderTests
    {
        private const string Regex128BitPattern = @"^[a-f0-9]{32}$";
        private const string Regex64BitPattern = @"^[a-f0-9]{16}$";

        [Test]
        public void Constructor_GeneratingNew64BitTraceId()
        {
            // Arrange
            var context = Substitute.For<IHttpContextAccessor>();
            var config = new ZipkinConfig(new Uri("http://localhost"))
            {
                Create128BitTraceId = false
            };

            // Arrange & Act
            var traceProvider = new TraceProvider(config, context);

            // Assert
            Assert.IsTrue(Regex.IsMatch(traceProvider.TraceId, Regex64BitPattern));
            Assert.IsTrue(Regex.IsMatch(traceProvider.SpanId, Regex64BitPattern));
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(false, traceProvider.IsSampled);
        }

        [Test]
        public void Constructor_GeneratingNew128BitTraceId()
        {
            // Arrange
            var context = Substitute.For<IHttpContextAccessor>();
            var config = new ZipkinConfig(new Uri("http://localhost"))
            {
                Create128BitTraceId = true
            };

            // Arrange & Act
            var traceProvider = new TraceProvider(config, context);

            // Assert
            Assert.IsTrue(Regex.IsMatch(traceProvider.TraceId, Regex128BitPattern));
            Assert.IsTrue(Regex.IsMatch(traceProvider.SpanId, Regex64BitPattern));
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(false, traceProvider.IsSampled);
        }

        [Test]
        public void Constructor_HavingTraceProviderInContext()
        {
            // Arrange
            var context = Substitute.For<IHttpContextAccessor>();
            var providerInContext = Substitute.For<ITraceProvider>();
            var httpContext = Substitute.For<HttpContext>();
            var environment = new Dictionary<object, object>
            {
                {
                    "ZipkinTracer.TraceProvider", providerInContext 
                }
            };

            httpContext.Items.Returns(environment);
            context.HttpContext.Returns(httpContext);

            // Act
            var sut = new TraceProvider(new ZipkinConfig(new Uri("http://localhost")), context);

            // Assert
            Assert.AreEqual(providerInContext.TraceId, sut.TraceId);
            Assert.AreEqual(providerInContext.SpanId, sut.SpanId);
            Assert.AreEqual(providerInContext.ParentSpanId, sut.ParentSpanId);
            Assert.AreEqual(providerInContext.IsSampled, sut.IsSampled);
        }

        [Test]
        public void Constructor_AcceptingHeadersWith64BitTraceId()
        {
            // Arrange
            var traceId = Convert.ToString((long)123123123, 16);
            var spanId = Convert.ToString((long)123132123, 16);
            var parentSpanId = Convert.ToString((long)123123, 16);
            var isSampled = false;

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            var sut = new TraceProvider(new ZipkinConfig(new Uri("http://localhost")), context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(isSampled, sut.IsSampled);
        }

        [Test]
        public void Constructor_AcceptingHeadersWithLessThan16HexCharacters()
        {
            // Arrange
            var traceId = Convert.ToString((long)12231, 16).Substring(1);
            var spanId = Convert.ToString((long)123212, 16);
            var parentSpanId = Convert.ToString((long)123213213, 16);
            var isSampled = false;

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            var sut = new TraceProvider(new ZipkinConfig(new Uri("http://localhost")), context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(isSampled, sut.IsSampled);
        }

        [Test]
        public void Constructor_AcceptingHeadersWith128BitTraceId()
        {
            // Arrange
            var traceId = Guid.NewGuid().ToString("N");
            var spanId = Convert.ToString((long)231231, 16);
            var parentSpanId = Convert.ToString((long)123123, 16);
            var isSampled = false;

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            var sut = new TraceProvider(new ZipkinConfig(new Uri("http://localhost")), context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(isSampled, sut.IsSampled);
        }

        [Test]
        public void Constructor_AcceptingHeadersWithOutIsSampled()
        {
            // Arrange
            var traceId = Convert.ToString((long)12123, 16);
            var spanId = Convert.ToString((long)12213, 16);
            var parentSpanId = Convert.ToString((long)213213213, 16);
            var context = GenerateContext(traceId, spanId, parentSpanId);
            var configuration = new ZipkinConfig(new Uri("http://localhost"));

            // Act
            var sut = new TraceProvider(configuration, context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
        }

        [Test]
        public void Constructor_AcceptingHeadersWithInvalidIdValues()
        {
            // Arrange
            var traceId = Guid.NewGuid().ToString("N").Substring(1);
            var spanId = "spanId";
            var parentSpanId = "parentId";
            var isSampled = "false";

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled);

            var configuration = new ZipkinConfig(new Uri("http://localhost"));

            // Act
            var sut = new TraceProvider(configuration, context);

            // Assert
            Assert.AreNotEqual(traceId, sut.TraceId);
            Assert.AreNotEqual(spanId, sut.SpanId);
            Assert.AreEqual(string.Empty, sut.ParentSpanId);
            Assert.IsFalse(sut.IsSampled);
        }

        [Test]
        public void Constructor_AcceptingHeadersWithSpanAndParentSpan_ThrowException()
        {
            // Arrange
            var traceId = Convert.ToString((long)213213, 16);
            var spanId = Convert.ToString((long)21213231, 16);
            var parentSpanId = spanId;

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                "false");

            // Act
           Assert.Throws<ArgumentException>(() => new TraceProvider(new ZipkinConfig(new Uri("http://localhost")), context));
        }

        [Test]
        public void GetNext()
        {
            // Arrange
            var traceId = Convert.ToString((long)213213, 16);
            var spanId = Convert.ToString((long)2121, 16);
            var parentSpanId = Convert.ToString((long)212112, 16);

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                "false");

            var sut = new TraceProvider(new ZipkinConfig(new Uri("http://localhost")), context);

            // Act
            var nextTraceProvider = sut.GetNext();

            // Assert
            Assert.AreEqual(sut.TraceId, nextTraceProvider.TraceId);
            Assert.IsTrue(Regex.IsMatch(nextTraceProvider.SpanId, Regex64BitPattern));
            Assert.AreEqual(sut.SpanId, nextTraceProvider.ParentSpanId);
            Assert.AreEqual(sut.IsSampled, nextTraceProvider.IsSampled);
        }

        private IHttpContextAccessor GenerateContext(string traceId, string spanId, string parentSpanId, string isSampled = null)
        {
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = Substitute.ForPartsOf<HttpContext>();
            var request = Substitute.ForPartsOf<HttpRequest>();
            var environment = new Dictionary<object, object>();
            var headers = new HeaderDictionary(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { TraceProvider.TraceIdHeaderName, new [] { traceId } },
                { TraceProvider.SpanIdHeaderName, new [] { spanId } },
                { TraceProvider.ParentSpanIdHeaderName, new [] { parentSpanId } }
            });

            if (isSampled != null)
            {
                headers[TraceProvider.SampledHeaderName] = new[] { isSampled };
            }

            request.Headers.Returns(headers);
            context.Request.Returns(request);
            context.Items.Returns(environment);
            httpContextAccessor.HttpContext.Returns(context);

            return httpContextAccessor;
        }
    }
}
