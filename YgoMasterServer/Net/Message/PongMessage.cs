using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class PongMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.Pong; }
        }

        public long ServerToClientLatency;
        public DateTime ResponseTime;

        public override void Read(BinaryReader reader)
        {
            ServerToClientLatency = reader.ReadInt64();
            ResponseTime = Utils.ConvertEpochTime(reader.ReadInt64());
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(ServerToClientLatency);
            writer.Write(Utils.GetEpochTime(ResponseTime));
        }
    }
}
