using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net
{
    abstract class NetMessage
    {
        public abstract NetMessageType Type { get; }

        public virtual void Read(BinaryReader reader)
        {
        }

        public virtual void Write(BinaryWriter writer)
        {
        }

        protected string ReadCompressedString(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len > 0)
            {
                byte[] compressedBuffer = reader.ReadBytes(len);
                byte[] buffer = LZ4.Decompress(compressedBuffer);
                return Encoding.UTF8.GetString(buffer);
            }
            return string.Empty;
        }

        protected void WriteCompressedString(BinaryWriter writer, string value)
        {
            if (value != null && value.Length > 0)
            {
                byte[] buffer;
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Encoding.UTF8.GetBytes(value));
                    bw.Flush();
                    buffer = ms.ToArray();
                }
                byte[] compressedBuffer = LZ4.Compress(buffer);
                writer.Write((int)compressedBuffer.Length);
                writer.Write(compressedBuffer);
            }
            else
            {
                writer.Write((int)0);
            }
        }
    }
}
