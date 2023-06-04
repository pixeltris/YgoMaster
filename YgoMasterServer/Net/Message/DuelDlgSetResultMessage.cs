using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelDlgSetResultMessage : DuelComMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelDlgSetResult; }
        }

        public uint Result;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Result = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Result);
        }
    }
}
