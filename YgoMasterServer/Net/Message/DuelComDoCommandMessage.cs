using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelComDoCommandMessage : DuelComMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelComDoCommand; }
        }

        public int Player;
        public int Position;
        public int Index;
        public int CommandId;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Player = reader.ReadInt32();
            Position = reader.ReadInt32();
            Index = reader.ReadInt32();
            CommandId = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Player);
            writer.Write(Position);
            writer.Write(Index);
            writer.Write(CommandId);
        }
    }
}
