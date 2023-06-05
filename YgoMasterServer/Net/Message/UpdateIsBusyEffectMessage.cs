using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class UpdateIsBusyEffectMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.UpdateIsBusyEffect; }
        }

        public ulong Seq;
        public DuelViewType Id;
        public int Value;

        public override void Read(BinaryReader reader)
        {
            Seq = reader.ReadUInt64();
            Id = (DuelViewType)reader.ReadInt32();
            Value = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Seq);
            writer.Write((int)Id);
            writer.Write(Value);
        }
    }
}
