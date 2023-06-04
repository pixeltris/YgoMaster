using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster.Net
{
    class NetPacketBuffer
    {
        public List<byte> Buffer { get; private set; }
        public int Length { get { return Buffer.Count; } }

        public NetPacketBuffer()
        {
            Buffer = new List<byte>();
        }

        public NetPacketBuffer(byte[] data)
        {
            Buffer = new List<byte>(data);
        }

        public bool IsComplete()
        {
            byte[] data = Buffer.ToArray();

            if (data.Length < 4)
                return false;

            return Length >= BitConverter.ToInt32(data, 0) + 4;
        }

        public void Append(byte[] data)
        {
            Buffer.AddRange(data);
        }

        public void Clear()
        {
            Buffer.Clear();
        }

        public byte[] GetBytes(int max)
        {
            byte[] data = null;
            try
            {
                if (Length > 0)
                {
                    if (max >= Length)
                        data = new byte[Length];
                    else
                        data = new byte[max];
                    System.Buffer.BlockCopy(Buffer.ToArray(), 0, data, 0, data.Length);
                    Buffer.RemoveRange(0, data.Length);
                }
            }
            catch
            {
                Console.WriteLine("throw GetBytes max:" + max + " length: " + Length + " datalen: " + data.Length + " Datalen " + Buffer.Count);
                throw new Exception();
            }
            return data;
        }

        public byte[] GetPacket()
        {
            if (!IsComplete())
                return null;

            byte[] data = Buffer.ToArray();
            int length = BitConverter.ToInt32(data, 0);
            data = new byte[length];
            System.Buffer.BlockCopy(Buffer.ToArray(), 4, data, 0, length);
            Buffer.RemoveRange(0, length + 4);
            return data;
        }
    }
}