using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelSpectatorDataMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelSpectatorData; }
        }

        public byte[] Buffer;

        public override void Read(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len > 0)
            {
                Buffer = reader.ReadBytes(len);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            if (Buffer != null && Buffer.Length > 0)
            {
                writer.Write((int)Buffer.Length);
                writer.Write(Buffer);
            }
            else
            {
                writer.Write((int)0);
            }
        }
    }
}
