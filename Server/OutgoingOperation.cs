using System;
using System.Collections.Generic;
using System.Text;

namespace Solution
{
    public struct OutgoingOperation
    {
        public byte Id;
        public List<byte> Buffer;
        public int Length;

        public OutgoingOperation(byte id)
        {
            Id = id;
            Buffer = new List<byte>();
            Length = 0;
        }

        public void WriteByte(byte value)
        {
            Buffer.Add(value);
            Length += 1;
        }

        public void WriteBoolean(bool value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 1;
        }

        public void WriteFloat(float value)
        {
            var val = BitConverter.GetBytes(value);
            Array.Reverse(val);
            Buffer.AddRange(val);
            Length += 4;
        }

        public void WriteDouble(double value)
        {
            var val = BitConverter.GetBytes(value);
            Array.Reverse(val);
            Buffer.AddRange(val);
            Length += 8;
        }

        public void WriteInt16(short value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 2;
        }

        public void WriteInt16PackedInt32(short left, short right)
        {
            var value = (left << 16) | (right & 0xFFFF);
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 4;
        }

        public void WriteInt32(int value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 4;
        }

        public void WriteInt64(long value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 8;
        }

        public void WriteUInt16(ushort value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 2;
        }

        public void WriteUInt32(uint value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 4;
        }

        public void WriteUInt64(ulong value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Length += 8;
        }

        public void WriteUTF16(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteUInt16((ushort)bytes.Length);
            Buffer.AddRange(bytes);
            Length += bytes.Length;
        }

        public void WriteUTF32(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteInt32(bytes.Length);
            Buffer.AddRange(bytes);
            Length += bytes.Length;
        }

        public byte[] GetBuffer()
        {
            // 4 bytes
            // 1 byte
            // N bytes

            Buffer.InsertRange(0, BitConverter.GetBytes(Length + 1));
            Buffer.Insert(4, Id);
            return Buffer.ToArray();
        }

        public void Reset()
        {
            Id = 0;
            Buffer.Clear();
            Length = 0;
        }
    }
}