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

            while (true)
            {
                server.Metrics();
                server.ProcessAccepting(); // this takes12ms its really slow the longest time because of the way accepting works
                server.ProcessReceive();
                server.ProcessOperations();
                server.ProcessDisconnected();
            }
        }
    }
}