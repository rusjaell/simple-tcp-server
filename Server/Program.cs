using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Solution
{

    public struct Session
    {
        public readonly int Id;
        public readonly Socket Socket;
        public bool Disconnected { get; private set; }

        public Queue<IncomingOperation> PendingOperations;

        public Session(int id, Socket socket)
        {
            Id = id;
            Socket = socket;
            Disconnected = false;
            PendingOperations = new Queue<IncomingOperation>();
        }

        public void Receive()
        {
            try
            {
                if (Socket.Available >= 4) 
                {
                    var incomingOperation = new IncomingOperation(this);
                    incomingOperation.ReceiveFromSocket(Socket);

                    PendingOperations.Enqueue(incomingOperation);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Disconnect("Receive Error");
            }
        }

        public void HandleOperations()
        {
            while(PendingOperations.Count > 0)
            {
                var operation = PendingOperations.Dequeue();

                switch (operation.OperationCode)
                {
                    case 0:
                        Console.WriteLine("Operation 0 | " + operation.ReadByte() + " " + operation.ReadByte());
                        break;
                    case 1:
                        Console.WriteLine("Operation 1 | " + operation.ReadFloat() + " " + operation.ReadByte());
                        break;
                    case 2:
                        Console.WriteLine("Operation 2 | " + operation.ReadUTF16() + " " + operation.ReadByte());
                        break;
                }
            }
        }

        public void Disconnect(string reason)
        {
            if (Disconnected)
                return;
            Disconnected = true;

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Socket.Close();

            Console.WriteLine($"Session {Id} disconnected: {reason}");
        }
    }

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

    public sealed class TCPClient
    {
        private Socket Socket;

        public TCPClient(byte id, int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            Socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Socket.Connect("127.0.0.1", 2050);

            var packet = new OutgoingOperation(0);
            packet.WriteByte(32);
            packet.WriteByte(96);
            _ = Socket.Send(packet.GetBuffer());

            packet = new OutgoingOperation(1);
            packet.WriteFloat(5.0f);
            packet.WriteByte(32);
            _ = Socket.Send(packet.GetBuffer());

            packet = new OutgoingOperation(2);
            packet.WriteUTF16("Test Message");
            packet.WriteByte(5);
            _ = Socket.Send(packet.GetBuffer());

            Socket.Close();
        }
    }

    public sealed class Program
    {
        public static void Main()
        {
            var server = new TCPServer(2050, 0xFF);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    for (byte i = 0; i < 1; i++)
                    {
                        var client = new TCPClient(i, 2050);
                    }
                    Thread.Sleep(1000);
                }
            });

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