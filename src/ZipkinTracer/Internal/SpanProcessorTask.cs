using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZipkinTracer.Helpers;

namespace ZipkinTracer.Internal
{
    internal class SpanProcessorTask : ISpanProcessorTask
    {
        private Task _taskInstance;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<SpanProcessorTask> _logger;
        private readonly ZipkinConfig _zipkinConfig;

        private readonly object _sync = new object();

        public bool IsCancelled => _cancellationTokenSource.IsCancellationRequested;

        public bool IsRunning => _taskInstance != null;

        public SpanProcessorTask(ZipkinConfig zipkinConfig, ILogger<SpanProcessorTask> logger)
        {
            if (zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            _zipkinConfig = zipkinConfig;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start(Func<Task> asyncAction)
        {
            if (asyncAction != null)
            {
                SyncHelper.ExecuteSafely(_sync,
                    () => _taskInstance == null || _taskInstance.Status == TaskStatus.Faulted,
                    () =>
                    {
                        _taskInstance = Task.Factory.StartNew(() => TaskExecuteLoop(asyncAction),
                            _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    });
            }
        }

        public void Stop()
        {
            SyncHelper.ExecuteSafely(_sync, () => _cancellationTokenSource.Token.CanBeCanceled,
                () => _cancellationTokenSource.Cancel());
        }

        private async Task TaskExecuteLoop(Func<Task> asyncAction)
        {
            while (!IsCancelled)
            {
                var delayTime = _zipkinConfig.SendDelayTime;
                try
                {
                    await asyncAction();
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(0), ex, "Error in SpanProcessorTask");
                    delayTime = _zipkinConfig.EncounteredAnErrorDelayTime;
                }

                // stop loop if task is cancelled while delay is in process
                try
                {
                    if (delayTime > TimeSpan.Zero)
                    {
                        await Task.Delay(delayTime, _cancellationTokenSource.Token);
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            _taskInstance = null;
        }
    }
}