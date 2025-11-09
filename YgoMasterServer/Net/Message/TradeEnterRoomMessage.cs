using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class TradeEnterRoomMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.TradeEnterRoom; }
        }

        public bool IsRemotePlayerAlreadyTrading;
        public bool IsRemotePlayerAlreadyHere;
        public uint PlayerCode;
        public string Name;

        public bool OwnsMainDeck;
        public string DeckJson;

        public string MyEntireCollectionJson;
        public string MyTradableCollectionJson;
        public string TheirEntireCollectionJson;
        public string TheirTradableCollectionJson;

        public override void Read(BinaryReader reader)
        {
            IsRemotePlayerAlreadyTrading = reader.ReadBoolean();
            IsRemotePlayerAlreadyHere = reader.ReadBoolean();
            PlayerCode = reader.ReadUInt32();
            Name = reader.ReadString();

            OwnsMainDeck = reader.ReadBoolean();
            DeckJson = ReadCompressedString(reader);

            MyEntireCollectionJson = ReadCompressedString(reader);
            MyTradableCollectionJson = ReadCompressedString(reader);
            TheirEntireCollectionJson = ReadCompressedString(reader);
            TheirTradableCollectionJson = ReadCompressedString(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IsRemotePlayerAlreadyTrading);
            writer.Write(IsRemotePlayerAlreadyHere);
            writer.Write(PlayerCode);
            writer.Write(Name != null ? Name : string.Empty);

            writer.Write(OwnsMainDeck);
            WriteCompressedString(writer, DeckJson != null ? DeckJson : string.Empty);

            WriteCompressedString(writer, MyEntireCollectionJson != null ? MyEntireCollectionJson: string.Empty);
            WriteCompressedString(writer, MyTradableCollectionJson != null ? MyTradableCollectionJson : string.Empty);
            WriteCompressedString(writer, TheirEntireCollectionJson != null ? TheirEntireCollectionJson : string.Empty);
            WriteCompressedString(writer, TheirTradableCollectionJson != null ? TheirTradableCollectionJson : string.Empty);
        }
    }
}
