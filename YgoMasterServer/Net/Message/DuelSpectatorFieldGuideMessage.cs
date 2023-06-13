using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelSpectatorFieldGuideMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelSpectatorFieldGuide; }
        }

        public bool Near;

        public override void Read(BinaryReader reader)
        {
            Near = reader.ReadBoolean();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Near);
        }
    }
}
