using Solution.Net;
using System;
using System.Diagnostics;
using System.Threading;

namespace Server.Core
{
    public sealed class Core
    {
        private readonly TCPServer TCPServer;

        public Core(int port, int backLog)
        {
            TCPServer = new TCPServer(port, backLog);
        }

        public void Run(int clients = 0)
        {
            TCPServer.Start();

            RunClients(clients);
            RunLoop();
        }

        private void RunClients(int clients)
        {
            // todo make this multi threaded

            for (var i = 0; i < clients; i++)
            {
                var process = new Process();
                process.StartInfo.FileName = "../../Client/Debug/Client.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                _ = process.Start();
            }
        }

        private void RunLoop()
        {
            var watch = Stopwatch.StartNew();

            var millisecondsPerTick = 50;
            var sleep = millisecondsPerTick;

            var lastMS = 0L;
            var mre = new ManualResetEvent(false);

            while (true)
            {
                if (sleep > 0)
                    _ = mre.WaitOne(sleep);

                var currentMS = watch.ElapsedMilliseconds;

                var dt = (int)Math.Max(currentMS - lastMS, millisecondsPerTick);

                var logicStartTime = watch.ElapsedMilliseconds;

                // server logic start

                TCPServer.ProcessMetrics();
                TCPServer.ProcessOperations();
                TCPServer.ProcessDisconnected();
                TCPServer.ProcessLogic(dt);

                // server logic end

                sleep = millisecondsPerTick - (int)(watch.ElapsedMilliseconds - logicStartTime);

                lastMS = currentMS;
            }
        }

        public void Shutdown()
        {
            // todo
        }
    }
}
