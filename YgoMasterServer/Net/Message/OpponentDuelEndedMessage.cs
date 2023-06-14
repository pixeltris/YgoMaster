using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class OpponentDuelEndedMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.OpponentDuelEnded; }
        }

        public DuelResultType Result;
        public DuelFinishType Finish;

        public override void Read(BinaryReader reader)
        {
            Result = (DuelResultType)reader.ReadInt32();
            Finish = (DuelFinishType)reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((int)Result);
            writer.Write((int)Finish);
        }
    }
}
