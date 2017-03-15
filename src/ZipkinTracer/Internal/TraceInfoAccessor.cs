using System.Threading;
using ZipkinTracer.Models;

namespace ZipkinTracer.Internal
{
    internal class TraceInfoAccessor : ITraceInfoAccessor
    {
        private static readonly AsyncLocal<TraceInfo> TraceInfoCurrent = new AsyncLocal<TraceInfo>();

        public TraceInfo TraceInfo
        {
            get
            {
                return TraceInfoCurrent.Value;
            }

            set
            {
				TraceInfoCurrent.Value = value;
            }
        }
    }
}
