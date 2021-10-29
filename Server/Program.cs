using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Solution
{
    public sealed class Program
    {
        public static void Main()
        {
            var server = new TCPServer(2050, 0xFF);

            // todo make this multi threaded

            for (var i = 0; i < 1; i++)
            {
                var process = new Process();
                process.StartInfo.FileName = "../../Client/Debug/Client.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }

            var watch = Stopwatch.StartNew();

            var tps = 50;
            var sleep = tps;

            var lastMS = 0L;
            var mre = new ManualResetEvent(false);

            while (true)
            {
                if (sleep > 0)
                    _ = mre.WaitOne(sleep);

                var currentMS = watch.ElapsedMilliseconds;

                var dt = (int)Math.Max(currentMS - lastMS, tps);

                var logicStartTime = watch.ElapsedMilliseconds;

                // server logic start

                server.ProcessMetrics();
                server.ProcessOperations();
                server.ProcessDisconnected();
                server.ProcessLogic(dt);
                
                // server logic end


                sleep = tps - (int)(watch.ElapsedMilliseconds - logicStartTime);

                lastMS = currentMS;
            }
        }
    }
}