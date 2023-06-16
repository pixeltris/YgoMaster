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

        // Misc duel messages
        DuelError,
        UpdateIsBusyEffect,
        OpponentDuelEnded,
        DuelTapSync,
        DuelEmote,

        // Duel spectator messages
        DuelSpectatorEnter,
        DuelSpectatorCount,
        DuelSpectatorData,
        DuelSpectatorFieldGuide,

        // Duel com messages
        DuelComMovePhase,
        DuelComDoCommand,
        DuelComCancelCommand,
        DuelComCancelCommand2,
        DuelDlgSetResult,
        DuelListSetCardExData,
        DuelListSetIndex,
        DuelListInitString,

        // Trade messages
        TradeEnterRoom,
        TradeLeaveRoom,
        TradeMoveCard,
        TradeStateChange,

        // Friend messages
        FriendDuelInvite
    }
}
