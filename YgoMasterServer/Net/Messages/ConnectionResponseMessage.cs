using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Messages
{
    class ConnectionResponseMessage : NetMessage
    {
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
