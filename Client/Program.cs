using System;
using System.Threading;

namespace Solution
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var client = new TCPClient(2050);

            while (true)
            {
                var packet = new OutgoingOperation(0);
                packet.WriteByte(32);
                packet.WriteByte(96);
                client.Send(ref packet);

                packet = new OutgoingOperation(1);
                packet.WriteFloat(5.0f);
                packet.WriteByte(32);
                client.Send(ref packet);

                packet = new OutgoingOperation(2);
                packet.WriteUTF16("Test Message");
                packet.WriteByte(5);
                client.Send(ref packet);
             
                Thread.Sleep(16); // 60 client fps (if it was a game)
            }
            client.Close();
        }
    }
}
