using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelListSetCardExDataMessage : DuelComMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelListSetCardExData; }
        }

        public int Index;
        public int Data;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Index = reader.ReadInt32();
            Data = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Index);
            writer.Write(Data);
        }
    }
}
