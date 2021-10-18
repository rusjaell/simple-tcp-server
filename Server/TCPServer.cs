using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Solution
{
    public sealed class TCPServer
    {
        private int NextSessionId;
        private Dictionary<int, Session> Sessions = new Dictionary<int, Session>();

        private Socket Socket;

        public TCPServer(int port, int backlog)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Blocking = false;

            Socket.Bind(endpoint);
            Socket.Listen(backlog);
        }

        public void ProcessAccepting()
        {
            try
            {
                var socket = Socket.Accept();
                if (socket == null)
                {
                    Console.WriteLine("Invalid Socket");
                    return;
                }

                NextSessionId++;
                Sessions.Add(NextSessionId, new Session(NextSessionId, socket));
            }
            catch
            {
            }
        }

        public void ProcessReceive()
        {
            foreach (var session in Sessions.Values)
            {
                session.Receive();
            }
        }

        public void ProcessOperations()
        {
            foreach (var session in Sessions.Values)
            {
                session.HandleOperations();
            }
        }

        public void ProcessDisconnected()
        {
            var sessionsToRemove = Sessions.Where(_ => _.Value.Disconnected).Select(_ => _.Key).ToList();
            foreach (var id in sessionsToRemove)
                _ = Sessions.Remove(id);
        }
    }
}