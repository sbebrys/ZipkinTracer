using System.Linq;
using NSubstitute;
using NUnit.Framework;
using ZipkinTracer.Models;
using ZipkinTracer.Models.Serialization.Json;

namespace ZipkinTracer.Test.Models.Serialization.Json
{
    [TestFixture]
    public class JsonSpanTests
    {
        [Test]
        public void JsonSpan()
        {
            // Arrange
            var span = new Span
            {
                Id = "15",
                Name = "cnk",
                ParentId = "05",
                TraceId = "123",
            };

            var annotation = Substitute.For<Annotation>();
            var binaryAnnotation = Substitute.For<BinaryAnnotation>();

            span.Annotations.Add(annotation);
            span.Annotations.Add(annotation);
            span.Annotations.Add(binaryAnnotation);
            span.Annotations.Add(binaryAnnotation);

            // Act
            var result = new JsonSpan(span);

            // Assert
            Assert.AreEqual(span.TraceId, result.TraceId);
            Assert.AreEqual(span.Name, result.Name);
            Assert.AreEqual(span.Id, result.Id);
            Assert.AreEqual(span.ParentId, result.ParentId);
            Assert.AreEqual(2, result.Annotations.Count());
            Assert.AreEqual(2, result.BinaryAnnotations.Count());
        }

        [Test]
        public void JsonSpan_ParentIdIsWhiteSpace()
        {
            // Arrange
            var span = new Span
            {
                ParentId = string.Empty
            };

            // Act
            var result = new JsonSpan(span);

            // Assert
            Assert.IsNull(result.ParentId);
        }
    }
}