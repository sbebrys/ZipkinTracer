using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZipkinTracer.Helpers
{
    internal static class SyncHelper
    {
        public static void ExecuteSafely(object sync, Func<bool> canExecute, Action actiontoExecuteSafely)
        {
            if (canExecute())
            {
                lock (sync)
                {
                    if (canExecute())
                    {
                        actiontoExecuteSafely();
                    }
                }
            }
        }

        public static async Task ExecuteSafelyAsync(object sync, Func<bool> canExecute, Func<Task> actiontoExecuteSafely)
        {
            if (canExecute())
            {
                if (Monitor.TryEnter(sync))
                {
                    if (canExecute())
                    {
                        await actiontoExecuteSafely();
                    }
                }
            }
        }
    }
}
