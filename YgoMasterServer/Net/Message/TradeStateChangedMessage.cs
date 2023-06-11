using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class TradeStateChangeMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.TradeStateChange; }
        }

        public TradeStateChange State;
        public string CardListJson;

        public TradeStateChangeMessage()
        {
        }

        public TradeStateChangeMessage(TradeStateChange state)
        {
            State = state;
        }

        public TradeStateChangeMessage(TradeStateChange state, string cardListJson)
        {
            State = state;
            CardListJson = cardListJson;
        }

        public override void Read(BinaryReader reader)
        {
            State = (TradeStateChange)reader.ReadInt32();
            CardListJson = reader.ReadString();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((int)State);
            writer.Write(CardListJson != null ? CardListJson : string.Empty);
        }
    }

    enum TradeStateChange
    {
        Wait,
        Complete,
        Error,
        PressedTrade,
        PressedCancel
    }
}
