using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Solution
{
    public sealed class TCPServer
    {
        private int NextSessionId;
        private Dictionary<int, Session> Sessions = new Dictionary<int, Session>();
        private object SessionLock = new object();

        private Socket Socket;
        
        private int Handled;

        public void Metrics()
        {
            Console.WriteLine($"Handling: {Handled} last tick");
            Handled = 0;
        }

        public TCPServer(int port, int backlog)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Blocking = false;

            Socket.Bind(endpoint);
            Socket.Listen(backlog);

            Socket.BeginAccept(ProcessAccepting, null);
        }

        public void ProcessAccepting(IAsyncResult ar)
        {
            try
            {
                var socket = Socket.EndAccept(ar);
                if (socket == null)
                {
                    Console.WriteLine("Invalid Socket");
                    return;
                }

                NextSessionId++;
                lock (SessionLock)
                {
                    Sessions.Add(NextSessionId, new Session(NextSessionId, socket));
                }
            }
            catch
            {
            }
            Socket.BeginAccept(ProcessAccepting, null);
        }

        public void ProcessReceive()
        {
            lock (SessionLock)
            {
                foreach (var session in Sessions.Values)
                {
                    session.Receive();
                }
            }
        }

        public void ProcessOperations()
        {
            lock (SessionLock)
            {
                foreach (var session in Sessions.Values)
                {
                    session.HandleOperations(ref Handled);
                }
            }
        }

        public void ProcessDisconnected()
        {
            lock (SessionLock)
            {
                var sessionsToRemove = new List<int>();
                foreach (var session in Sessions.Values)
                    if (session.Disconnected)
                        sessionsToRemove.Add(session.Id);
                foreach (var id in sessionsToRemove)
                    _ = Sessions.Remove(id);
            }
        }
    }
}