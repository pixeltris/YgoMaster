using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    /// <summary>
    /// This message is to avoid potentially getting a ping timeout / network error in some situations when the duel ends
    /// NOTE: This might not be needed. Remove this message if it isn't an issue
    /// </summary>
    class OpponentDuelEndedMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.OpponentDuelEnded; }
        }
    }
}
