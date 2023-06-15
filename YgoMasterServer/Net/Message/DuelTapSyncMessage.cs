using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelTapSyncMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelTapSync; }
        }

        public int AnimationId;
        public bool Character;
        public bool Near;

        public override void Read(BinaryReader reader)
        {
            AnimationId = reader.ReadInt32();
            Character = reader.ReadBoolean();
            Near = reader.ReadBoolean();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(AnimationId);
            writer.Write(Character);
            writer.Write(Near);
        }
    }
}
