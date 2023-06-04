using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Messages
{
    class ConnectionRequestMessage : NetMessage
    {
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
