using Solution.Net.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Solution.Net
{
    public sealed class OperationToken
    {
        public int ReceivedLength { get; set; } = 0;
        public byte OpCode { get; set; } = 0;
        public bool IsHeaderReceived { get; set; } = false;

        public void Reset()
        {
            ReceivedLength = 0;
            OpCode = 0;
            IsHeaderReceived = false;
        }
    }

    public sealed class Session
    {
        public readonly int Id;
        public readonly Socket Socket;
        public readonly Queue<IncomingOperation> PendingOperations = new Queue<IncomingOperation>();

        public bool Disconnected { get; private set; }

        public Session(int id, Socket socket)
        {
            Id = id;
            Socket = socket;
        }

        private byte[] HeaderBuffer;
        private byte[] LengthBuffer;
        private SocketAsyncEventArgs ReceiveSocketAsyncEventArgs = new SocketAsyncEventArgs();

        public void Start()
        {
            //Socket.NoDelay = true;
            //Socket.UseOnlyOverlappedIO = true;

            HeaderBuffer = new byte[5];

            ReceiveSocketAsyncEventArgs.Completed += ProcessReceive;
            ReceiveSocketAsyncEventArgs.SetBuffer(HeaderBuffer, 0, HeaderBuffer.Length);
            ReceiveSocketAsyncEventArgs.UserToken = new OperationToken();
            ReceiveSocketAsyncEventArgs.AcceptSocket = Socket;

            if (!ReceiveSocketAsyncEventArgs.AcceptSocket.ReceiveAsync(ReceiveSocketAsyncEventArgs))
                ProcessReceive(null, ReceiveSocketAsyncEventArgs);
        }

        public Dictionary<byte, int> PacketCounts = new Dictionary<byte, int>();

        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (Disconnected)
                return;

            if (!e.AcceptSocket.Connected)
            {
                Disconnect("ProcessReceive: !Socket.Connected");
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Disconnect($"ProcessReceive: e.SocketError != SocketError.Success");
                return;
            }

            var token = (OperationToken)e.UserToken;


            if (!token.IsHeaderReceived)
            {
                if (e.BytesTransferred < 5)
                {
                    Disconnect($"ProcessReceive: e.BytesTransferred < 5: {e.BytesTransferred}");
                    return;
                }

                var len = BitConverter.ToInt32(e.Buffer, 1);

                if (len > 512)
                {
                    Disconnect("Too large of a packet size");
                    return;
                }

                token.IsHeaderReceived = true;
                token.OpCode = e.Buffer[0];
                token.ReceivedLength = len;

                if (LengthBuffer == null)
                    LengthBuffer = new byte[len];
                else
                {
                    Array.Clear(LengthBuffer, 0, LengthBuffer.Length);
                    Array.Resize(ref LengthBuffer, len);
                }

                e.SetBuffer(LengthBuffer, 0, LengthBuffer.Length);
                try
                {
                    if (!e.AcceptSocket.ReceiveAsync(e))
                        ProcessReceive(sender, e);
                }
                catch (StackOverflowException ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect("Stack overflow");
                }
                return;
            }

            if (e.BytesTransferred < token.ReceivedLength)
            {
                Disconnect($"ProcessReceive: e.BytesTransferred < {token.ReceivedLength}");
                return;
            }

            var buffer = new byte[e.Buffer.Length];
            Array.Copy(e.Buffer, buffer, e.Buffer.Length);

            var incomingOperation = new IncomingOperation(this, token.OpCode, buffer);
            PendingOperations.Enqueue(incomingOperation);

            if (!Disconnected)
            {
                Array.Clear(HeaderBuffer, 0, HeaderBuffer.Length);
                e.SetBuffer(HeaderBuffer, 0, HeaderBuffer.Length);
                token.Reset();

                try
                {
                    if (!e.AcceptSocket.ReceiveAsync(e))
                        ProcessReceive(sender, e);
                }
                catch (StackOverflowException ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect("Stack overflow");
                }
            }
        }

        public void Disconnect(string reason)
        {
            if (Disconnected)
                return;
            Disconnected = true;

            ReceiveSocketAsyncEventArgs.Completed -= ProcessReceive;

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
    }
}