using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class ConnectionRequestMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.ConnectionRequest; }
        }

        public string Token;

        public override void Read(BinaryReader reader)
        {
            Token = reader.ReadString();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Token != null ? Token : string.Empty);
        }
    }
}
