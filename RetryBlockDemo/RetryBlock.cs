using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetryBlockDemo
{
    public static class RetryBlock
    {
        public static async Task ExecuteAsync<T>(Func<Task> action, int count, TimeSpan delay, Action<T> handler, CancellationToken ct) where T : Exception
        {
            var retry = 0;
            while (true)
            {
                try
                {
                    if (retry > 0)
                        await Task.Delay(delay, ct);
                    await action();
                    break;
                }
                catch (T ex)
                {
                    retry++;
                    if (ex is OperationCanceledException)
                        throw;
                    if (handler != null)
                        handler(ex);
                    if (count >= 0 && retry >= count)
                        throw;
                }
            }
        }

        public static async Task<TResult> ExecuteAsync<T, TResult>(Func<Task<TResult>> func, int count, TimeSpan delay, Action<T> handler, CancellationToken ct) where T : Exception
        {
            var retry = 0;
            while (true)
            {
                try
                {
                    if (retry > 0)
                        await Task.Delay(delay, ct);
                    return await func();
                }
                catch (T ex)
                {
                    retry++;
                    if (ex is OperationCanceledException)
                        throw;
                    if (handler != null)
                        handler(ex);
                    if (count >= 0 && retry >= count)
                        throw;
                }
            }
        }
    }
}