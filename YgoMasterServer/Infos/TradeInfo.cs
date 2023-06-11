using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    class TradeInfo
    {
        public Player Player1;
        public Player Player2;
        public bool Player1HasPressedTrade { get; private set; }
        public bool Player2HasPressedTrade { get; private set; }
        public bool HaveBothPressedTrade
        {
            get { return Player1 != null && Player2 != null && Player1HasPressedTrade && Player2HasPressedTrade; }
        }
        public bool IsEmpty
        {
            get { return Player1 == null && Player2 == null; }
        }

        /// <summary>
        /// A fake deck used to hold the trade state
        /// </summary>
        public DeckInfo State;

        public TradeInfo()
        {
            State = new DeckInfo();
            State.Id = 99999999;
            State.RegulationId = DeckInfo.DefaultRegulationId;
            State.Name = "";
        }

        public bool Add(Player player)
        {
            if (player == Player1 || player == Player2)
            {
                return true;
            }
            if (Player1 == null)
            {
                Player1 = player;
                ClearHasPressedTrade();
                player.ActiveTrade = this;
                return true;
            }
            if (Player2 == null)
            {
                Player2 = player;
                ClearHasPressedTrade();
                player.ActiveTrade = this;
                return true;
            }
            return false;
        }

        public bool Remove(Player player)
        {
            if (player == Player1)
            {
                Player1 = null;
                ClearHasPressedTrade();
                player.ActiveTrade = null;
                return true;
            }
            else if (player == Player2)
            {
                Player2 = null;
                ClearHasPressedTrade();
                player.ActiveTrade = null;
                return true;
            }
            else if (player.ActiveTrade != null)
            {
                TradeInfo activeTrade = player.ActiveTrade;
                player.ActiveTrade = null;
                activeTrade.Remove(player);
            }
            return false;
        }

        public Player GetOtherPlayer(Player player)
        {
            if (player == Player1)
            {
                return Player2;
            }
            else if (player == Player2)
            {
                return Player1;
            }
            return null;
        }

        public void SetHasPressedTrade(Player player, bool value)
        {
            if (player == Player1)
            {
                Player1HasPressedTrade = value;
            }
            else if (player == Player2)
            {
                Player2HasPressedTrade = value;
            }
        }

        void ClearHasPressedTrade()
        {
            Player1HasPressedTrade = false;
            Player2HasPressedTrade = false;
        }
    }
}
