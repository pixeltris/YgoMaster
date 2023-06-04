using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelComMovePhaseMessage : DuelComMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelComMovePhase; }
        }

        public int Phase;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Phase = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Phase);
        }
    }
}
