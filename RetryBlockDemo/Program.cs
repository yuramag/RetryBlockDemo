using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetryBlockDemo
{
    class Program
    {
        private enum MessageLevel
        {
            None,
            Info,
            Warning,
            Error
        }

        private static void Main(string[] args)
        {
            var e = new ManualResetEventSlim();
            RunDemosAsync().ContinueWith(x => e.Set());
            e.Wait();

            Log();
            Log("Press <Enter> to exit...");
            Console.ReadLine();
        }

        public static async Task Demo1Async()
        {
            var ct = CancellationToken.None;
            await RetryBlock.ExecuteAsync<InvalidOperationException>(DemoActionAsync, 3, TimeSpan.FromSeconds(1), ErrorHandler, ct);
        }

        static async Task RunDemosAsync()
        {
            await RunDemoAsync(Demo1Async);
        }

        public static Task RunDemoAsync(Func<Task> demo)
        {
            var task = demo();
            task.ContinueWith(x => Log("Result: Success", MessageLevel.Info), TaskContinuationOptions.OnlyOnRanToCompletion);
            task.ContinueWith(x => Log("Result: Error => " + x.Exception.InnerException.Message, MessageLevel.Error), TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        static async Task DemoActionAsync()
        {
            await Task.Delay(10); // simulate some work
            if (new Random().Next(5) != 0)
                throw new InvalidOperationException("Exception has been thrown");
            //Log("Success", MessageLevel.Info);
        }

        static void ErrorHandler<T>(T exception) where T : Exception
        {
            Log(exception.Message, MessageLevel.Error);
        }

        private static readonly object s_lock = new object();

        static void Log(string message = null, MessageLevel level = MessageLevel.None)
        {
            lock (s_lock)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = level == MessageLevel.Info ? ConsoleColor.Green : level == MessageLevel.Warning ? ConsoleColor.Yellow : level == MessageLevel.Error ? ConsoleColor.Red : Console.ForegroundColor;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
