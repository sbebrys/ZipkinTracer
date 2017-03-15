using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;
using ZipkinTracer.Internal;
using ZipkinTracer.Models;
using ZipkinTracer.Models.References;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class SpanTracerTests
    {
        [Test]
        public void Ctor_NullArguments_ThrowException()
        {
            var spanCollector = Substitute.For<ISpanCollector>();
            var zipkinEndpoint = Substitute.For<IServiceEndpoint>();
            var zipkinConfig = Substitute.For<ZipkinConfig>(new Uri("http://localhost"), null, null);

            Assert.Throws<ArgumentNullException>(() => new SpanTracer(null, spanCollector, zipkinEndpoint), "spanCollector");
            Assert.Throws<ArgumentNullException>(() => new SpanTracer(zipkinConfig, null, zipkinEndpoint), "zipkinEndpoint");
            Assert.Throws<ArgumentNullException>(() => new SpanTracer(zipkinConfig, spanCollector, null), "zipkinConfig");
        }

        [Test]
        public async Task ReceiveServerSpan_InCorrectRequest_ReturnTraceSpan()
        {
            var requestUri = new Uri("http://server.com:999/api");
            var requestName = "request";
            var traceId = "traceId";
            var parentSpanId = "parentSpanId";
            var spanId = "spanId";

            var spanCollector = Substitute.For<ISpanCollector>();
            var zipkinEndpoint = Substitute.For<IServiceEndpoint>();
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var localEndpoint = new Endpoint {ServiceName = "server.com", Port = 999};
            zipkinEndpoint.GetLocalEndpoint(null, 0).ReturnsForAnyArgs(localEndpoint);

            var st = new SpanTracer(zipkinConfig, spanCollector, zipkinEndpoint);
			var trInfo = new TraceInfo(traceId, spanId, parentSpanId, true, new Uri("http://localhost"));
            var resultSpan = await st.ReceiveServerSpan(requestName, trInfo, requestUri);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(traceId, resultSpan.TraceId);
            Assert.AreEqual(parentSpanId, resultSpan.ParentId);
            Assert.AreEqual(spanId, resultSpan.Id);

            Assert.AreEqual(1, resultSpan.GetAnnotationsByType<Annotation>().Count());

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(ZipkinConstants.ServerReceive, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.IsNotNull(annotation.Host);

            Assert.AreEqual(localEndpoint, annotation.Host);

            var binaryAnnotations = resultSpan.GetAnnotationsByType<BinaryAnnotation>();

            Assert.AreEqual(1, binaryAnnotations.Count());
            AssertBinaryAnnotations(binaryAnnotations, "http.path", requestUri.AbsolutePath);
        }

        [Test]
        public async Task ReceiveServerSpan_InCorrectRequest_ReturnTraceSpan1()
        {
            var requestUri = new Uri("http://server.com:999/api");
            var requestName = "request";
            var traceId = "traceId";
            var parentSpanId = "parentSpanId";
            var spanId = "spanId";

            var spanCollector = Substitute.For<ISpanCollector>();
            var zipkinEndpoint = Substitute.For<IServiceEndpoint>();
            var zipkinConfig = new ZipkinConfig(new Uri("http://localhost"));
            var localEndpoint = new Endpoint { ServiceName = "server.com", Port = 999 };
            zipkinEndpoint.GetLocalEndpoint(null, 0).ReturnsForAnyArgs(localEndpoint);

            var st = new SpanTracer(zipkinConfig, spanCollector, zipkinEndpoint);
			var trInfo = new TraceInfo(traceId, parentSpanId, spanId, true, new Uri("http://localhost"));
			var resultSpan = await st.ReceiveServerSpan(requestName, trInfo, requestUri);
            var annotation = resultSpan.Annotations[0] as Annotation;

            Assert.AreEqual(localEndpoint, annotation.Host);
        }

        //[Test]
        //public async Task ReceiveServerSpan_UsingAlreadyCleanedDomainName()
        //{
        //    var domain = new Uri("https://server.com");
        //    var requestName = "request";
        //    var traceId = Guid.NewGuid().ToString("N");
        //    var parentSpanId = "parentSpanId";
        //    var spanId = "123123";
        //    var serverUri = new Uri("https://" + _clientServiceName + ":" + _port + _api);

        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);

        //    var localEndpoint = new Endpoint { ServiceName = domain.Host, Port = _port };
        //    _zipkinEndpointStub.GetLocalEndpoint(Arg.Is(domain.Host), Arg.Is(_port)).Returns(localEndpoint);

        //    var resultSpan = await spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

        //    var annotation = resultSpan.Annotations[0] as Annotation;
        //    Assert.AreEqual(localEndpoint, annotation.Host);
        //}

        //[Test]
        //public void SendServerSpan()
        //{
        //    var domain = new Uri("https://server.com");
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);

        //    var endpoint = new Endpoint() { ServiceName = domain.Host, Port = (ushort)domain.Port };
        //    var expectedSpan = new Span();
        //    expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = ZipkinConstants.ServerReceive, Timestamp = DateTimeOffset.UtcNow });

        //    _zipkinEndpointStub.GetLocalEndpoint(Arg.Is(domain.Host), (ushort)Arg.Is(domain.Port)).Returns(new Endpoint() { ServiceName = domain.Host });

        //    spanTracer.SendServerSpan(expectedSpan);

        //    //assert
        //    _spanCollectorStub.Received().Collect(Arg.Is<Span>(y => ValidateSendServerSpan(y, domain.Host)));
        //}

        //[Test]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void SendServerSpan_NullSpan()
        //{
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, new Uri("http://server.com"));

        //    spanTracer.SendServerSpan(null);
        //}

        //[Test]
        //[ExpectedException(typeof(ArgumentException))]
        //public void SendServerSpan_NullAnnotation()
        //{
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, new Uri("http://server.com"));

        //    var expectedSpan = new Span();

        //    spanTracer.SendServerSpan(expectedSpan);
        //}

        //[Test]
        //[ExpectedException(typeof(ArgumentException))]
        //public void SendServerSpan_InvalidAnnotation()
        //{
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, new Uri("http://server.com"));

        //    var expectedSpan = new Span();

        //    spanTracer.SendServerSpan(expectedSpan);
        //}

        //[Test]
        //public async Task SendClientSpan()
        //{
        //    var domain = new Uri("https://server.com");
        //    var requestName = "request";
        //    var traceId = Guid.NewGuid().ToString("N");
        //    var parentSpanId = "123123";
        //    var spanId = "321321";
        //    var serverUri = new Uri("https://" + _clientServiceName + ":" + _port + _api);

        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);

        //    var localEndpoint = new Endpoint { ServiceName = _serverServiceName };
        //    _zipkinEndpointStub.GetLocalEndpoint(Arg.Is(domain.Host), Arg.Any<ushort>()).Returns(localEndpoint);
        //    var remoteEndpoint = new Endpoint { ServiceName = _clientServiceName, Port = _port };
        //    _zipkinEndpointStub.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(_clientServiceName)).Returns(remoteEndpoint);

        //    var resultSpan = await spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

        //    Assert.AreEqual(requestName, resultSpan.Name);
        //    Assert.AreEqual(traceId, resultSpan.TraceId);
        //    Assert.AreEqual(parentSpanId, resultSpan.ParentId);
        //    Assert.AreEqual(spanId, resultSpan.Id);

        //    Assert.AreEqual(1, resultSpan.GetAnnotationsByType<Annotation>().Count());

        //    var annotation = resultSpan.Annotations[0] as Annotation;

        //    Assert.IsNotNull(annotation);
        //    Assert.AreEqual(ZipkinConstants.ClientSend, annotation.Value);
        //    Assert.IsNotNull(annotation.Timestamp);
        //    Assert.AreEqual(localEndpoint, annotation.Host);

        //    var binaryAnnotations = resultSpan.GetAnnotationsByType<BinaryAnnotation>();

        //    Assert.AreEqual(2, binaryAnnotations.Count());
        //    AssertBinaryAnnotations(binaryAnnotations, "http.path", serverUri.AbsolutePath);
        //    AssertBinaryAnnotations(binaryAnnotations, "sa", "1");

        //    var endpoint = binaryAnnotations.ToArray()[1].Host as Endpoint;

        //    Assert.IsNotNull(endpoint);
        //    Assert.AreEqual(_clientServiceName, endpoint.ServiceName);
        //    Assert.AreEqual(_port, endpoint.Port);
        //}

        //[Test]
        //public async Task SendClientSpanWithDomainUnderFilterList()
        //{
        //    var domain = new Uri("https://server.com");
        //    var requestName = "request";
        //    var traceId = Guid.NewGuid().ToString("N");
        //    var parentSpanId = "parentSpanID";
        //    var spanId = "12";
        //    var serverUri = new Uri("https://" + _clientServiceName + _zipkinNotToBeDisplayedDomainList.First() + ":" + _port + _api);

        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);

        //    var localEndpoint = new Endpoint { ServiceName = _serverServiceName };
        //    _zipkinEndpointStub.GetLocalEndpoint(Arg.Is(domain.Host), Arg.Any<ushort>()).Returns(localEndpoint);
        //    var remoteEndpoint = new Endpoint { ServiceName = _clientServiceName, Port = _port };
        //    _zipkinEndpointStub.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(_clientServiceName)).Returns(remoteEndpoint);

        //    var resultSpan = await spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

        //    var endpoint = resultSpan.GetAnnotationsByType<BinaryAnnotation>().ToArray()[1].Host as Endpoint;

        //    Assert.IsNotNull(endpoint);
        //    Assert.AreEqual(_clientServiceName, endpoint.ServiceName);
        //    Assert.AreEqual(_port, endpoint.Port);
        //}

        //[Test]
        //public void ReceiveClientSpan()
        //{
        //    var domain = new Uri("http://server.com");
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);
        //    var endpoint = new Endpoint() { ServiceName = _clientServiceName, Port = _port };
        //    var serverUri = new Uri("https://" + _clientServiceName + ":" + _port + _api);
        //    ushort returnCode = 132;
        //    var expectedSpan = new Span();

        //    expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = ZipkinConstants.ClientSend, Timestamp = DateTimeOffset.UtcNow });

        //    _zipkinEndpointStub.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(domain.Host)).Returns(endpoint);

        //    spanTracer.ReceiveClientSpan(expectedSpan, returnCode);

        //    //assert
        //    _spanCollectorStub.Received().Collect(Arg.Is<Span>(y => ValidateReceiveClientSpan(y, _clientServiceName, _port)));
        //}

        //[Test]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ReceiveClientSpan_EmptyAnnotationsList()
        //{
        //    var domain = new Uri("http://server.com");
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);
        //    var endpoint = new Endpoint() { ServiceName = _clientServiceName };
        //    var serverUri = new Uri("https://" + _clientServiceName + ":" + _port + _api);
        //    short returnCode = 123;
        //    var expectedSpan = new Span();

        //    _zipkinEndpointStub.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(domain.Host)).Returns(endpoint);

        //    spanTracer.ReceiveClientSpan(expectedSpan, returnCode);
        //}

        //[Test]
        //[TestCategory("TraceRecordTests")]
        //public async Task Record_WithSpanAndValue_AddsNewAnnotation()
        //{
        //    // Arrange
        //    var expectedDescription = "Description";
        //    var expectedSpan = new Span();
        //    var domain = new Uri("http://server.com");
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);

        //    // Act
        //    await spanTracer.Record(expectedSpan, expectedDescription);

        //    // Assert
        //    Assert.IsNotNull(
        //        expectedSpan.GetAnnotationsByType<Annotation>().SingleOrDefault(a => (string)a.Value == expectedDescription),
        //        "The record is not found in the Annotations."
        //    );
        //}

        //[Test]
        //[TestCategory("TraceRecordTests")]
        //public void RecordBinary_WithSpanAndValue_AddsNewTypeCorrectBinaryAnnotation()
        //{
        //    // Arrange
        //    var keyName = "TestKey";
        //    var testValues = new dynamic[]
        //    {
        //        new { Value = true, ExpectedResult = true, Type = AnnotationType.Boolean },
        //        new { Value = short.MaxValue, ExpectedResult = short.MaxValue, Type = AnnotationType.Int16 },
        //        new { Value = int.MaxValue, ExpectedResult = int.MaxValue, Type = AnnotationType.Int32 },
        //        new { Value = long.MaxValue, ExpectedResult = long.MaxValue, Type = AnnotationType.Int64 },
        //        new { Value = double.MaxValue, ExpectedResult = double.MaxValue, Type = AnnotationType.Double },
        //        new { Value = "String", ExpectedResult = "String", Type = AnnotationType.String },
        //        new { Value = DateTime.MaxValue, ExpectedResult = DateTime.MaxValue, Type = AnnotationType.String }
        //    };

        //    var domain = new Uri("http://server.com");
        //    var spanTracer = new SpanTracer(_spanCollectorStub, _zipkinEndpointStub, _zipkinNotToBeDisplayedDomainList, domain);

        //    foreach (var testValue in testValues)
        //    {
        //        var expectedSpan = new Span();

        //        // Act
        //        spanTracer.RecordBinary(expectedSpan, keyName, testValue.Value);

        //        // Assert
        //        var actualAnnotation = expectedSpan
        //            .GetAnnotationsByType<BinaryAnnotation>()?
        //            .SingleOrDefault(a => a.Key == keyName);

        //        var result = actualAnnotation?.Value;
        //        var annotationType = actualAnnotation?.AnnotationType;
        //        Assert.AreEqual(testValue.ExpectedResult, result, "The recorded value in the annotation is wrong.");
        //        Assert.AreEqual(testValue.Type, annotationType, "The Annotation Type is wrong.");
        //    }
        //}

        //private bool ValidateReceiveClientSpan(Span y, string serviceName, ushort port)
        //{
        //    var firstannotation = (Annotation)y.Annotations[0];
        //    var firstEndpoint = (Endpoint)firstannotation.Host;

        //    Assert.AreEqual(serviceName, firstEndpoint.ServiceName);
        //    Assert.AreEqual(port, firstEndpoint.Port);
        //    Assert.AreEqual(ZipkinConstants.ClientSend, firstannotation.Value);
        //    Assert.IsNotNull(firstannotation.Timestamp);

        //    var secondAnnotation = (Annotation)y.Annotations[1];
        //    var secondEndpoint = (Endpoint)secondAnnotation.Host;

        //    Assert.AreEqual(serviceName, secondEndpoint.ServiceName);
        //    Assert.AreEqual(port, secondEndpoint.Port);
        //    Assert.AreEqual(ZipkinConstants.ClientReceive, secondAnnotation.Value);
        //    Assert.IsNotNull(secondAnnotation.Timestamp);

        //    return true;
        //}

        //private bool ValidateSendServerSpan(Span y, string serviceName)
        //{
        //    var firstAnnotation = (Annotation)y.Annotations[0];
        //    var firstEndpoint = (Endpoint)firstAnnotation.Host;

        //    Assert.AreEqual(serviceName, firstEndpoint.ServiceName);
        //    Assert.AreEqual(ZipkinConstants.ServerReceive, firstAnnotation.Value);
        //    Assert.IsNotNull(firstAnnotation.Timestamp);

        //    var secondAnnotation = (Annotation)y.Annotations[1];
        //    var secondEndpoint = (Endpoint)secondAnnotation.Host;

        //    Assert.AreEqual(serviceName, secondEndpoint.ServiceName);
        //    Assert.AreEqual(ZipkinConstants.ServerSend, secondAnnotation.Value);
        //    Assert.IsNotNull(secondAnnotation.Timestamp);

        //    return true;
        //}

        private void AssertBinaryAnnotations(IEnumerable<BinaryAnnotation> list, string key, string value)
        {
            Assert.AreEqual(value, list.Where(x => x.Key.Equals(key)).Select(x => x.Value).First());
        }

        private IHttpContextAccessor GenerateContextAccessor()
        {
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = Substitute.ForPartsOf<HttpContext>();
            var request = Substitute.ForPartsOf<HttpRequest>();

            request.Scheme.Returns("http");
            request.Host.Returns(new HostString("server.com"));
            context.Request.Returns(request);
            httpContextAccessor.HttpContext.Returns(context);

            return httpContextAccessor;
        }
    }
}