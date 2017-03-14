using System.Threading;
using ZipkinTracer.Models;

namespace ZipkinTracer.Http
{
    public class TraceInfoAccessor : ITraceInfoAccessor
    {
        private static AsyncLocal<TraceInfo> _traceInfoCurrent = new AsyncLocal<TraceInfo>();

        public TraceInfo TraceInfo
        {
            get
            {
                return _traceInfoCurrent.Value;
            }

            set
            {
                _traceInfoCurrent.Value = value;
            }
        }
    }
}
