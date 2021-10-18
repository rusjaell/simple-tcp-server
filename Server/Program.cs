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
           
            while (true)
            {
                server.ProcessAccepting();
                server.ProcessReceive();
                server.ProcessOperations();
                server.ProcessDisconnected();
            }
        }
    }
}