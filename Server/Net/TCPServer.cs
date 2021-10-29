using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Solution.Net
{
    public sealed class TCPServer
    {
        private int NextSessionId;
        private Dictionary<int, Session> Sessions = new Dictionary<int, Session>();
        private object SessionLock = new object();

        private Socket Socket;
        
        private int Handled;

        public void ProcessMetrics()
        {
            Console.Title = $"Handling: {Handled} every tick - {Sessions.Count} Sessions";
            Handled = 0;
        }

        public TCPServer(int port, int backlog)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Blocking = false;

            Socket.Bind(endpoint);
            Socket.Listen(backlog);
        }

        public void Start()
        {
            _ = Socket.BeginAccept(ProcessAccepting, null);
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
                    var session = new Session(NextSessionId, socket);
                    session.Start();
                    Sessions.Add(NextSessionId, session);
                }
            }
            catch
            {
            }

            Start();
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

        public void ProcessLogic(double dt)
        {
            // todo whatver server needs
        }
    }
}