using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace YgoMaster
{
    class DuelRoom
    {
        public uint Id;
        public uint SpectatorRoomId;
        public string HashKey;
        public Player Owner;

        public bool Disbanded;

        public int MemberCount
        {
            get
            {
                lock (MembersLocker)
                {
                    return Members.Count;
                }
            }
        }

        public int SpectatorCount
        {
            get
            {
                lock (MembersLocker)
                {
                    return Spectators.Count;
                }
            }
        }

        public int MemberLimit;
        public Dictionary<Player, DuelRoomRecord> Members { get; private set; }
        public HashSet<Player> Spectators { get; private set; }
        public object MembersLocker = new object();

        public DuelRoomTable[] Tables { get; private set; }

        public int Comment;

        /// <summary>
        /// 1 = 8000
        /// 2 = 4000
        /// </summary>
        public int LifePoints;

        /// <summary>
        /// IDS_ROOM_TIME_NORMAL
        /// IDS_ROOM_TIME_LONG
        /// IDS_ROOM_TIME_SHORT
        /// IDS_ROOM_TIME_VERY_LONG
        /// </summary>
        public int DuelTime;

        /// <summary>
        /// Regulation?
        /// </summary>
        public int Rule;

        public DateTime TimeCreated;
        public DateTime TimeUpdated;
        public DateTime TimeExpire;

        public bool ShowInRoomList;
        public bool AllowSpectators;
        public bool ShowInSpectatorList;

        public bool ViewReplays;

        public string ClientVersion;

        public DuelRoom()
        {
            Members = new Dictionary<Player, DuelRoomRecord>();
            Spectators = new HashSet<Player>();
        }

        public bool IsSpectator(Player player)
        {
            lock (Spectators)
            {
                return Spectators.Contains(player);
            }
        }

        public bool IsMember(Player player)
        {
            lock (Members)
            {
                return Members.ContainsKey(player);
            }
        }

        public bool IsMember(uint playerId)
        {
            lock (Members)
            {
                foreach (KeyValuePair<Player, DuelRoomRecord> member in Members)
                {
                    if (member.Key.Code == playerId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Contains(Player player)
        {
            return IsSpectator(player) || IsMember(player);
        }

        public void RemovePlayer(Player player)
        {
            lock (MembersLocker)
            {
                if (IsSpectator(player))
                {
                    Spectators.Remove(player);
                }
                else if (IsMember(player))
                {
                    Members.Remove(player);
                }
                player.DuelRoom = null;
            }
            foreach (DuelRoomTable table in Tables)
            {
                table.RemovePlayer(player);
            }
        }

        public void InitTables()
        {
            Tables = new DuelRoomTable[(MemberLimit + (MemberLimit % 2)) / 2];
            for (int i = 0; i < Tables.Length; i++)
            {
                Tables[i] = new DuelRoomTable();
            }
        }

        public DuelRoomTable GetTable(Player player)
        {
            foreach (DuelRoomTable table in Tables)
            {
                if (table.ContainsPlayer(player))
                {
                    return table;
                }
            }
            return null;
        }

        public void ResetTableStateIfMatchingOrDueling(Player player)
        {
            DuelRoomTable table = GetTable(player);
            if (table != null)
            {
                DuelRoomTableEntry tableEntry = table.GetEntry(player);
                if (tableEntry != null)
                {
                    switch (table.State)
                    {
                        case DuelRoomTableState.Matched:
                        case DuelRoomTableState.Dueling:
                            table.State = DuelRoomTableState.Joinable;
                            foreach (DuelRoomTableEntry entry in table.Entries)
                            {
                                entry.IsMatchingOrInDuel = false;
                            }
                            break;
                    }
                }
            }
        }
    }

    class DuelRoomTable
    {
        public DuelRoomTableEntry[] Entries;
        public DuelRoomTableState State = DuelRoomTableState.Joinable;
        public DateTime MatchedTime;

        static Random rand = new Random();

        public Player Player1
        {
            get { return Entries[0].Player; }
            set { Entries[0].Player = value; }
        }
        public Player Player2
        {
            get { return Entries[1].Player; }
            set { Entries[1].Player = value; }
        }
        public Player Player3
        {
            get { return Entries[2].Player; }
            set { Entries[2].Player = value; }
        }
        public Player Player4
        {
            get { return Entries[3].Player; }
            set { Entries[3].Player = value; }
        }
        public bool IsFull
        {
            get
            {
                Player[] players = { Player1, Player2 };
                foreach (Player player in players)
                {
                    if (player == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool IsMatched
        {
            get
            {
                Player[] players = { Player1, Player2 };
                foreach (Player player in players)
                {
                    if (player == null)
                    {
                        return false;
                    }
                    DuelRoomTableEntry entry = GetEntry(player);
                    if (entry == null)
                    {
                        return false;
                    }
                    if (!entry.IsMatchingOrInDuel)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public uint Seed;
        public int FirstPlayer;
        public int CoinFlipCounter;
        public int CoinFlipPlayerIndex;
        public string TableHash;
        public string TableTicket;

        public DuelRoomTable()
        {
            Entries = new DuelRoomTableEntry[4];
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i] = new DuelRoomTableEntry();
            }
        }

        public bool AddPlayer(Player player)
        {
            lock (Entries)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Entries[i].Player == null)
                    {
                        Entries[i].Player = player;
                        Entries[i].IsMatchingOrInDuel = false;
                        return true;
                    }
                }
                return false;
            }
        }

        public void RemovePlayer(Player player)
        {
            lock (Entries)
            {
                for (int i = 0; i < Entries.Length; i++)
                {
                    if (Entries[i].Player == player)
                    {
                        Entries[i].Player = null;
                        Entries[i].IsMatchingOrInDuel = false;
                    }
                }
            }
        }

        public bool ContainsPlayer(Player player)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Player == player)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetPlayerComment(Player player, int comment)
        {
            lock (Entries)
            {
                for (int i = 0; i < Entries.Length; i++)
                {
                    if (Entries[i].Player == player)
                    {
                        Entries[i].Comment = comment;
                        Entries[i].CommentTime = DateTime.UtcNow;
                    }
                }
            }
        }

        public DuelRoomTableEntry GetEntry(Player player)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Player == player)
                {
                    return Entries[i];
                }
            }
            return null;
        }

        public void Clear()
        {
            lock (Entries)
            {
                foreach (DuelRoomTableEntry entry in Entries)
                {
                    entry.Player = null;
                    entry.IsMatchingOrInDuel = false;
                    entry.Comment = 0;
                    entry.CommentTime = default(DateTime);
                }
                State = DuelRoomTableState.Joinable;
            }
        }
    }

    class DuelRoomTableEntry
    {
        private Player player;
        public Player Player
        {
            get { return player; }
            set
            {
                player = value;
                CommentTime = DateTime.MinValue;
                Comment = 0;
            }
        }
        public bool IsMatchingOrInDuel;
        public DateTime CommentTime;
        public int Comment;
    }

    /// <summary>
    /// NOTE:
    /// This isn't quite right...
    /// 2 = response to clicking "Begin Duel" where opponent hasn't yet clicked it
    /// 3 = what the opponent sees in response to "room_table_polling" after doing the above
    /// 4 = when you do "Begin Duel" after your opponent
    /// 5 = matched? or after coin flip?
    /// </summary>
    enum DuelRoomTableState
    {
        None,
        Joinable = 1,
        Matching = 3,
        Matched = 4,
        Dueling = 5
    }

    class DuelRoomRecord
    {
        public int Win;
        public int Loss;
        public int Draw;
    }
}
