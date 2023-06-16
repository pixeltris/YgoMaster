using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace YgoMaster.Net.Message
{
    class DuelEmoteMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelEmote; }
        }

        public bool Near;
        public string Text;

        public override void Read(BinaryReader reader)
        {
            Near = reader.ReadBoolean();
            Text = reader.ReadString();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Near);
            writer.Write(Text != null ? Text : string.Empty);
        }
    }
}
