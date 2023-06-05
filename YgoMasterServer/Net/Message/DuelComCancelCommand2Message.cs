using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelComCancelCommand2Message : DuelComMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelComCancelCommand2; }
        }

        public bool Decide;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Decide = reader.ReadBoolean();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Decide);
        }
    }
}
