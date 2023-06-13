using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelSpectatorEnterMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelSpectatorEnter; }
        }

        public uint PlayerCode;
        public int SpectatorCount;

        public override void Read(BinaryReader reader)
        {
            PlayerCode = reader.ReadUInt32();
            SpectatorCount = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(PlayerCode);
            writer.Write(SpectatorCount);
        }
    }
}
