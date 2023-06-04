using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class ConnectionResponseMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.ConnectionResponse; }
        }

        public bool Success;

        public override void Read(BinaryReader reader)
        {
            Success = reader.ReadBoolean();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Success);
        }
    }
}
