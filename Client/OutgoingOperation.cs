using System;
using System.Collections.Generic;
using System.Text;

namespace Solution
{
    public struct OutgoingOperation
    {
        public byte Id;
        private List<byte> Buffer;
        private int Position;

        public OutgoingOperation(byte id)
        {
            Id = id;
            Buffer = new List<byte>();
            Position = 0;

            Buffer.Add(Id);
        }

        public void WriteByte(byte value)
        {
            Buffer.Add(value);
            Position += 1;
        }

        public void WriteBoolean(bool value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 1;
        }

        public void WriteFloat(float value)
        {
            var val = BitConverter.GetBytes(value);
            Array.Reverse(val);
            Buffer.AddRange(val);
            Position += 4;
        }

        public void WriteDouble(double value)
        {
            var val = BitConverter.GetBytes(value);
            Array.Reverse(val);
            Buffer.AddRange(val);
            Position += 8;
        }

        public void WriteInt16(short value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 2;
        }

        public void WriteInt16PackedInt32(short left, short right)
        {
            var value = (left << 16) | (right & 0xFFFF);
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 4;
        }

        public void WriteInt32(int value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 4;
        }

        public void WriteInt64(long value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 8;
        }

        public void WriteUInt16(ushort value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 2;
        }

        public void WriteUInt32(uint value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 4;
        }

        public void WriteUInt64(ulong value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Position += 8;
        }

        public void WriteUTF16(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteUInt16((ushort)bytes.Length);
            Buffer.AddRange(bytes);
            Position += bytes.Length;
        }

        public void WriteUTF32(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteInt32(bytes.Length);
            Buffer.AddRange(bytes);
            Position += bytes.Length;
        }

        public byte[] GetBuffer()
        {
            // 1 byte
            // 4 bytes
            // N bytes

            Buffer.InsertRange(1, BitConverter.GetBytes(Position));
            return Buffer.ToArray();
        }

        public void Reset()
        {
            Id = 0;
            Buffer.Clear();
            Position = 0;
        }
    }
}