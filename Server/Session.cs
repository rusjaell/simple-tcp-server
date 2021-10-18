using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Solution
{
    public sealed class Session
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

        public bool IsConnected()
        {
            try
            {
                return !(Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void Receive()
        {
            if (!IsConnected())
            {
                Disconnect("Socket not connected");
                return;
            }

            try
            {
                while (Socket.Available >= 4)
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

        public void HandleOperations(ref int handled)
        {
            while(PendingOperations.Count > 0)
            {
                var operation = PendingOperations.Dequeue();

                //switch (operation.OperationCode)
                //{
                //    case 0:
                //        Console.WriteLine("Operation 0 | " + operation.ReadByte() + " " + operation.ReadByte());
                //        break;
                //    case 1:
                //        Console.WriteLine("Operation 1 | " + operation.ReadFloat() + " " + operation.ReadByte());
                //        break;
                //    case 2:
                //        Console.WriteLine("Operation 2 | " + operation.ReadUTF16() + " " + operation.ReadByte());
                //        break;
                //}
                handled++;
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
}