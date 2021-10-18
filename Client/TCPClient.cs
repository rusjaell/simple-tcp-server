using System.Net;
using System.Net.Sockets;

namespace Solution
{
    public sealed class TCPClient
    {
        public readonly Socket Socket;

        public TCPClient(int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Socket.Connect(endpoint);
        }

        public void Send(ref OutgoingOperation outgoingOperation)
        {
            var buffer = outgoingOperation.GetBuffer();
            _ = Socket.Send(buffer);
        }

        public void Close()
        {
            if (!Socket.Connected)
                return;
            
            Socket.Close();
        }
    }
}