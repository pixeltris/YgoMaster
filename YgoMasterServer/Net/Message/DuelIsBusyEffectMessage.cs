using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelIsBusyEffectMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelIsBusyEffect; }
        }

        public ulong RunEffectSeq;
        public DuelViewType ViewType;

        public override void Read(BinaryReader reader)
        {
            RunEffectSeq = reader.ReadUInt64();
            ViewType = (DuelViewType)reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(RunEffectSeq);
            writer.Write((int)ViewType);
        }
    }
}
