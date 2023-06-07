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
        OpponentSurrendered,
        OpponentDuelEnded,

        // Duel com messages
        DuelComMovePhase,
        DuelComDoCommand,
        DuelComCancelCommand,
        DuelComCancelCommand2,
        DuelDlgSetResult,
        DuelListSetCardExData,
        DuelListSetIndex,
        DuelListInitString
    }
}
