using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class PvpServerConnectionRequestMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.PvpServerConnectionRequest; }
        }

        public string Key;
        public uint DuelRoomId;
        public uint PlayerId1;
        public uint PlayerId2;

        public override void Read(BinaryReader reader)
        {
            Key = reader.ReadString();
            DuelRoomId = reader.ReadUInt32();
            PlayerId1 = reader.ReadUInt32();
            PlayerId2 = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Key != null ? Key : string.Empty);
            writer.Write(DuelRoomId);
            writer.Write(PlayerId1);
            writer.Write(PlayerId2);
        }
    }
}
