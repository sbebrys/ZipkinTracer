using System;
using System.Threading.Tasks;

namespace ZipkinTracer
{
    internal interface ISpanProcessorTask
    {
        void Stop();

        void Start(Func<Task> asyncAction);

        bool IsCancelled { get; }
    }
}