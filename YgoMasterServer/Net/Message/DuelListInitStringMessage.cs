using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    internal class DuelListInitStringMessage : DuelComMessage
    {
        public override NetMessageType Type
        {
            get { return NetMessageType.DuelListInitString; }
        }
    }
}
