using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelEngineStateMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelEngineState; }
        }

        public ulong RunEffectSeq;
        public DuelViewType ViewType;
        public int Param1;
        public int Param2;
        public int Param3;
        public byte DoCommandUser;
        public byte RunDialogUser;
        public byte[] CompressedBuffer;

        public override void Read(BinaryReader reader)
        {
            RunEffectSeq = reader.ReadUInt64();
            ViewType = (DuelViewType)reader.ReadInt32();
            Param1 = reader.ReadInt32();
            Param2 = reader.ReadInt32();
            Param3 = reader.ReadInt32();
            DoCommandUser = reader.ReadByte();
            RunDialogUser = reader.ReadByte();
            int len = reader.ReadInt32();
            CompressedBuffer = len == 0 ? new byte[0] : reader.ReadBytes(len);
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(RunEffectSeq);
            writer.Write((int)ViewType);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
            writer.Write(DoCommandUser);
            writer.Write(RunDialogUser);
            if (CompressedBuffer != null)
            {
                writer.Write(CompressedBuffer.Length);
                writer.Write(CompressedBuffer);
            }
            else
            {
                writer.Write((int)0);
            }
        }
    }
}
