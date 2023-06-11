using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class FriendDuelInviteMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.FriendDuelInvite; }
        }

        public uint PlayerCode;
        public string Name;

        public override void Read(BinaryReader reader)
        {
            PlayerCode = reader.ReadUInt32();
            Name = reader.ReadString();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(PlayerCode);
            writer.Write(Name != null ? Name : string.Empty);
        }
    }
}
