using System;
using System.Net.Sockets;
using System.Text;

namespace Solution
{
    public struct IncomingOperation
    {
        public byte OperationCode;
        public byte[] Buffer;
        public int Position;
        public Session Session;

        public IncomingOperation(Session session, byte opCode, byte[] buffer)
        {
            OperationCode = opCode;
            Buffer = buffer;
            Position = 0;
            Session = session;
        }

        public void Log(int bytes)
        {
            Console.WriteLine(OperationCode + " | " + SizeSuffix(Position) + " / " + SizeSuffix(Buffer.Length) + " - " + $"{SizeSuffix(bytes, 0)}");
        }

        public byte ReadByte()
        {
            if (Buffer.Length > Position)
            {
                var value = Buffer[Position];
                Position += 1;
                //Log(1);
                return value;
            }
            else
                throw new Exception("ReadByte Length Overflow");
        }

        public bool ReadBoolean()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToBoolean(Buffer, Position);
                Position += 1;
                //Log(1);
                return value;
            }
            else
                throw new Exception("ReadBoolean Length Overflow");
        }

        public float ReadFloat()
        {
            if (Buffer.Length > Position)
            {
                Array.Reverse(Buffer, Position, 4);
                var value = BitConverter.ToSingle(Buffer, Position);
                Position += 4;
                //Log(4);
                return value;
            }
            else
                throw new Exception("ReadFloat Length Overflow");
        }

        public double ReadDouble()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToDouble(Buffer, Position);
                Position += 8;
                //Log(8);
                return value;
            }
            else
                throw new Exception("ReadDouble Length Overflow");
        }

        public short ReadInt16()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToInt16(Buffer, Position);
                Position += 2;
                //Log(2);
                return value;
            }
            else
                throw new Exception("ReadInt16 Length Overflow");
        }

        public short[] ReadInt16PackedInt32()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToInt32(Buffer, Position);
                var ret = new short[2];
                ret[0] = (short)(value >> 16);
                ret[1] = (short)(value & 0xFFFF);
                Position += 4;
                //Log(4);
                return ret;
            }
            else
                throw new Exception("ReadInt16 Length Overflow");
        }

        public int ReadInt32()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToInt32(Buffer, Position);
                Position += 4;
                //Log(4);
                return value;
            }
            else
                throw new Exception("ReadInt32 Length Overflow");
        }

        public long ReadInt64()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToInt64(Buffer, Position);
                Position += 8;
                //Log(8);
                return value;
            }
            else
                throw new Exception("ReadInt64 Length Overflow");
        }
        public ushort ReadUInt16()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToUInt16(Buffer, Position);
                Position += 2;
                //Log(2);
                return value;
            }
            else
                throw new Exception("ReadUInt16 Length Overflow");
        }

        public uint ReadUInt32()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToUInt32(Buffer, Position);
                Position += 4;
                //Log(4);
                return value;
            }
            else
                throw new Exception("ReadUInt32 Length Overflow");
        }

        public ulong ReadUInt64()
        {
            if (Buffer.Length > Position)
            {
                var value = BitConverter.ToUInt64(Buffer, Position);
                Position += 8;
                //Log(8);
                return value;
            }
            else
                throw new Exception("ReadInt32 Length Overflow");
        }

        public string ReadUTF16()
        {
            var size = ReadInt16();
            if (Buffer.Length > Position)
            {
                var value = Encoding.UTF8.GetString(Buffer, Position, size);
                Position += size;
                //Log(size);
                return value;
            }
            else
                throw new Exception("ReadInt32 Length Overflow");
        }

        //https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException("decimalPlaces");
            
            if (value < 0)
                return "-" + SizeSuffix(-value, decimalPlaces);
            
            if (value == 0)
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            var mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            var adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}