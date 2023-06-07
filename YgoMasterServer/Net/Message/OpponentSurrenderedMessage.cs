using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class OpponentSurrenderedMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.OpponentSurrendered; }
        }
    }
}
