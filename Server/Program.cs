using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Solution
{
    public sealed class TimedProfiler : IDisposable
    {
        private string Message { get; }
        private Stopwatch Stopwatch { get; }

        public TimedProfiler(string message)
        {
            Message = message;
            Stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            var time = Stopwatch.Elapsed;
            var ms = Stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"{Message} - Elapsed: {time} ({ms}ms)");
        }
    }

    public sealed class Program
    {
        public static void Main()
        {
            var server = new TCPServer(2050, 0xFF);

            // todo make this multi threaded

            while (true)
            {
                Console.WriteLine("============================================================================");
                using (var timedProfiler = new TimedProfiler("Metrics"))
                    server.Metrics();
                using (var timedProfiler = new TimedProfiler("ProcessReceive"))
                    server.ProcessReceive();
                using (var timedProfiler = new TimedProfiler("ProcessOperations"))
                    server.ProcessOperations();
                using (var timedProfiler = new TimedProfiler("ProcessDisconnected"))
                    server.ProcessDisconnected();
            }
        }
    }
}