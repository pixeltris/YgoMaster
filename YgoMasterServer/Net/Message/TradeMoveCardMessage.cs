using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class TradeMoveCardMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.TradeMoveCard; }
        }

        public bool RemoveCard;
        public int CardId;
        public CardStyleRarity StyleRarity;
        public bool OtherPlayer;
        public uint OtherPlayerCode;

        public override void Read(BinaryReader reader)
        {
            RemoveCard = reader.ReadBoolean();
            CardId = reader.ReadInt32();
            StyleRarity = (CardStyleRarity)reader.ReadInt32();
            OtherPlayer = reader.ReadBoolean();
            OtherPlayerCode = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(RemoveCard);
            writer.Write(CardId);
            writer.Write((int)StyleRarity);
            writer.Write(OtherPlayer);
            writer.Write(OtherPlayerCode);
        }
    }
}
