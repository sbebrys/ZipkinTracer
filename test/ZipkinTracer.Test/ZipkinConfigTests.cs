using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class ZipkinConfigTests
    {
        [Test]
        public void Ctor_NullArguments_ThrowException()
        {
            Func<HttpRequest, Uri> domainResolverFunc = null;

            Assert.Catch<ArgumentNullException>(() => new ZipkinConfig(null), "zipkinBaseUri");
            Assert.Catch<ArgumentNullException>(() => new ZipkinConfig(null, domainResolverFunc), "zipkinBaseUri");
            Assert.Catch<ArgumentNullException>(() => new ZipkinConfig(new Uri("http://localhost"), domainResolverFunc), "domainResolverFunc");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(0.5)]
        [TestCase(1)]
        [TestCase(3.0)]
        public void SetSampleRate_AlwaysInRange(double sampleRate)
        {
            var config = new ZipkinConfig(new Uri("http://localhost"));

            config.SampleRate = sampleRate;
            config.SampleRate.Should().BeInRange(0, 1);

            if (sampleRate > 0 && sampleRate < 1)
                Assert.AreEqual(sampleRate, config.SampleRate);
        }

        [Test]
        [TestCase("0", null, 0, null, ExpectedResult = false)]
        [TestCase("1", null, 0, null, ExpectedResult = true)]
        [TestCase("false", null, 0, null, ExpectedResult = false)]
        [TestCase("true", null, 0, null, ExpectedResult = true)]
        [TestCase("FALSE", null, 0, null, ExpectedResult = false)]
        [TestCase("TRUE", null, 0, null, ExpectedResult = true)]
        [TestCase("FalSe", null, 0, null, ExpectedResult = false)]
        [TestCase("TrUe", null, 0, null, ExpectedResult = true)]
        [TestCase(null, "/x", 0, "/x", ExpectedResult = false)]
        [TestCase("", "/x", 0, "/x", ExpectedResult = false)]
        [TestCase("invalidValue", "/x", 0,"/x", ExpectedResult = false)]
        [TestCase(null, null, 0, null, ExpectedResult = false)]
        [TestCase(null, "/x", 0, null, ExpectedResult = false)]
        [TestCase(null, null, 1, null, ExpectedResult = true)]
        public bool ShouldBeSampled(string sampledFlag, string requestPath, double sampleRate, string excludedPath)
        {
            var config = new ZipkinConfig(new Uri("http://localhost")) { SampleRate = sampleRate };

            if (excludedPath != null)
                config.ExcludedPathList.Add(excludedPath);

            return config.ShouldBeSampled(sampledFlag, requestPath);
        }
    }
}