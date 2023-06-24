using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    class DuelSysActFinishedMessage : NetMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelSysActFinished; }
        }
    }
}
