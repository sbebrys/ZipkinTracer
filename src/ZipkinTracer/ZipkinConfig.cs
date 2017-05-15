using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ZipkinTracer
{
    /// <summary>
    /// Zipkin tracer configuration
    /// </summary>
    public class ZipkinConfig
    {
        private double _sampleRate;

        private readonly Random _random = new Random();

        /// <summary>
        /// Zipkin server uri
        /// </summary>
        public Uri ZipkinBaseUri { get; }

        /// <summary>
        /// Predicate to omit request in tracing
        /// </summary>
        public Predicate<HttpRequest> Bypass { get; set; } = request => false;

        /// <summary>
        /// Domain name resolver
        /// </summary>
        public Func<HttpRequest, Uri> Domain { get; } = request => new Uri($"{request.Scheme}://{request.Host.Value}");

        /// <summary>
        /// Batch size of spans use in report process
        /// </summary>
        public uint SpanProcessorBatchSize { get; set; }

        /// <summary>
        /// List of service path which will omit in tracing
        /// </summary>
        public IList<string> ExcludedPathList { get; }

        /// <summary>
        /// List of domain which will be cut in reported annotations
        /// </summary>
        public IList<string> NotToBeDisplayedDomainList { get; }

        /// <summary>
        /// Sample rate form 0 to 1.0
        /// </summary>
        public double SampleRate
        {
            get { return _sampleRate; }
            set
            {
                if (value < 0)
                    _sampleRate = 0;
                else if (value > 1)
                    _sampleRate = 1;
                else
                    _sampleRate = value;
            }
        }

        /// <summary>
        /// If true treace id will be 128bit number
        /// </summary>
        public bool Create128BitTraceId { get; set; }

        /// <summary>
        /// Delay time between sending collected spans
        /// </summary>
        public TimeSpan SendDelayTime { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Delay time after error in sending collected spans
        /// </summary>
        public TimeSpan EncounteredAnErrorDelayTime { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Is trace is global enabled 
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="zipkinBaseUri">Zipkin serwer uri</param>
        /// <param name="excludedPathList">List of service path which will omit in tracing</param>
        /// <param name="notToBeDisplayedDomainList">List of domain which will be cut in reported annotations</param>
        public ZipkinConfig(Uri zipkinBaseUri, IList<string> excludedPathList = null,
            IList<string> notToBeDisplayedDomainList = null)
        {
            if (zipkinBaseUri == null) throw new ArgumentNullException(nameof(zipkinBaseUri));

            ZipkinBaseUri = zipkinBaseUri;
            ExcludedPathList = excludedPathList ?? new List<string>();
            NotToBeDisplayedDomainList = notToBeDisplayedDomainList ?? new List<string>();
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="zipkinBaseUri">Zipkin serwer uri</param>
        /// <param name="domainResolverFunc">Domain name resolver</param>
        /// <param name="excludedPathList">List of service path which will omit in tracing</param>
        /// <param name="notToBeDisplayedDomainList">List of domain which will be cut in reported annotations</param>
        public ZipkinConfig(Uri zipkinBaseUri, Func<HttpRequest, Uri> domainResolverFunc,
            IList<string> excludedPathList = null, IList<string> notToBeDisplayedDomainList = null)
        {
            if (zipkinBaseUri == null) throw new ArgumentNullException(nameof(zipkinBaseUri));
            if (domainResolverFunc == null) throw new ArgumentNullException(nameof(domainResolverFunc));

            ZipkinBaseUri = zipkinBaseUri;
            Domain = domainResolverFunc;
            ExcludedPathList = excludedPathList ?? new List<string>();
            NotToBeDisplayedDomainList = notToBeDisplayedDomainList ?? new List<string>();
        }

        /// <summary>
        /// Checks if sampled flag from headers has value if not decide if need to sample or not using sample rate
        /// </summary>
        /// <param name="sampledFlag"></param>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        public bool ShouldBeSampled(string sampledFlag, string requestPath)
        {
            bool result;
            if (TryParseSampledFlagToBool(sampledFlag, out result)) return result;

            if (IsInDontSampleList(requestPath)) return false;

            return _random.NextDouble() <= SampleRate;
        }

        private bool IsInDontSampleList(string path)
        {
            return path != null &&
                   ExcludedPathList.Any(uri => path.StartsWith(uri, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool TryParseSampledFlagToBool(string sampledFlag, out bool booleanValue)
        {
            if (string.IsNullOrWhiteSpace(sampledFlag))
            {
                booleanValue = false;
                return false;
            }

            switch (sampledFlag)
            {
                case "0":
                    booleanValue = false;
                    return true;
                case "1":
                    booleanValue = true;
                    return true;
                default:
                    return bool.TryParse(sampledFlag, out booleanValue);
            }
        }
    }
}