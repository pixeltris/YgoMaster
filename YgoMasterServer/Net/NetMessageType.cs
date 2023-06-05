using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster.Net
{
    enum NetMessageType
    {
        ConnectionRequest,
        ConnectionResponse,
        Ping,
        Pong,
        DuelError,
        UpdateIsBusyEffect,

        // Duel com messages
        DuelComMovePhase,
        DuelComDoCommand,
        DuelDlgSetResult,
        DuelListSetCardExData,
        DuelListSetIndex
        //TODO: DuelListInitString
    }
}
