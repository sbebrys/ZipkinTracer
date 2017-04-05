using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZipkinTracer.Helpers
{
    internal static class TraceIdHelper
    {
        /// <summary>
        /// Generate a traceId.
        /// </summary>
        /// <param name="create128Bit">true for 128bit, false for 64 bit</param>
        /// <returns></returns>
        internal static string GenerateNewTraceId(bool create128Bit)
        {
            return create128Bit ? Guid.NewGuid().ToString("N") : GenerateHexEncodedInt64Id();
        }

        /// <summary>
        /// Generate a hex encoded Int64 from new Guid.
        /// </summary>
        /// <returns>The hex encoded int64</returns>
        internal static string GenerateHexEncodedInt64Id()
        {
            return Convert.ToString(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0), 16);
        }
    }
}