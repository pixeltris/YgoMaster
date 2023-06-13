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

        public List<DuelRoomReplay> Replays { get; private set; }
        public Dictionary<long, DuelRoomReplay> ReplaysByDid { get; private set; }
        long nextReplayDid = 1;

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
            Replays = new List<DuelRoomReplay>();
            ReplaysByDid = new Dictionary<long, DuelRoomReplay>();
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

        public DuelRoomTable GetTableAsSpectator(Player player)
        {
            foreach (DuelRoomTable table in Tables)
            {
                lock (table.Spectators)
                {
                    if (table.Spectators.Contains(player))
                    {
                        return table;
                    }
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
                            Console.WriteLine("ResetTableStateIfMatchingOrDueling (" + table.State + ") on table " + Id +
                                " requester pcode " + player.Code + " name '" + player.Name);
                            table.ClearMatching();
                            break;
                    }
                }
            }
        }

        public void AddReplay(DuelRoomReplay replay)
        {
            lock (Replays)
            {
                if (replay.Player1 == null || replay.Player2 == null)
                {
                    return;
                }
                replay.Did = nextReplayDid++;
                replay.Player1.did = nextReplayDid++;
                replay.Player2.did = nextReplayDid++;
                Replays.Add(replay);
                ReplaysByDid[replay.Did] = replay;
            }
        }
    }

    class DuelRoomReplay
    {
        public long Did;
        public DuelSettings Player1;
        public DuelSettings Player2;
        public uint Player1Code;
        public uint Player2Code;
        public bool IsComplete
        {
            get { return Player1 != null && Player2 != null; }
        }

        public void AddReplay(Player player)
        {
            if (player.ActiveDuelSettings.MyID == 0)
            {
                Player1 = new DuelSettings();
                Player1.CopyFrom(player.ActiveDuelSettings);
                Player1Code = player.Code;
            }
            else
            {
                Player2 = new DuelSettings();
                Player2.CopyFrom(player.ActiveDuelSettings);
                Player2Code = player.Code;
            }
        }
    }

    class DuelRoomTable
    {
        public DuelRoomTableEntry[] Entries;
        public DuelRoomTableState State = DuelRoomTableState.Joinable;
        public bool IsDuelComplete
        {
            get { return State == DuelRoomTableState.Dueling && (Rewards.Player1Rewards != null || Rewards.Player2Rewards != null); }
        }
        public DateTime MatchedTime;

        public DuelRoomReplay Replay;
        public uint Seed;
        public int FirstPlayer;
        public int CoinFlipCounter;
        public int CoinFlipPlayerIndex;
        public string TableHash;
        public string TableTicket;
        public DuelRoomTableRewards Rewards = new DuelRoomTableRewards();

        public HashSet<Player> Spectators = new HashSet<Player>();
        public List<byte> SpectatorData = new List<byte>();
        public bool SpectatorFieldGuideNear;

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
        public bool HasBeginDuel
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
                    if (!entry.HasBeginDuel)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

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
                        Entries[i].HasBeginDuel = false;
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
                        Entries[i].HasBeginDuel = false;
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

        public int GetEntryIndex(Player player)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Player == player)
                {
                    return i;
                }
            }
            return -1;
        }

        public Player GetOpponent(Player player)
        {
            DuelRoomTableEntry opponent = GetOpponentEntry(player);
            return opponent != null ? opponent.Player : null;
        }

        public DuelRoomTableEntry GetOpponentEntry(Player player)
        {
            if (Entries[0].Player == player)
            {
                return Entries[1];
            }
            else if (Entries[1].Player == player)
            {
                return Entries[0];
            }
            return null;
        }

        public void ClearMatching()
        {
            lock (Entries)
            {
                foreach (DuelRoomTableEntry entry in Entries)
                {
                    entry.IsMatchingOrInDuel = false;
                    entry.HasBeginDuel = false;
                }
                State = DuelRoomTableState.Joinable;
            }
            ClearSpectators();
        }

        public bool InitDuel(int firstPlayer)
        {
            ClearSpectators();
            lock (Entries)
            {
                Player p1 = Player1;
                Player p2 = Player2;
                if (p1 == null || p2 == null || p1 == p2)
                {
                    ClearMatching();
                    return false;
                }

                State = DuelRoomTableState.Dueling;
                FirstPlayer = firstPlayer;
                return true;
            }
        }

        public void ClearSpectators()
        {
            lock (Spectators)
            {
                foreach (Player spectator in Spectators)
                {
                    spectator.SpectatingPlayerCode = 0;
                }
                SpectatorData.Clear();
                Spectators.Clear();
            }
        }
    }

    class DuelRoomTableRewards
    {
        public Player Player1;
        public Player Player2;
        public Dictionary<string, object> Player1Rewards;
        public Dictionary<string, object> Player2Rewards;

        public void Clear()
        {
            lock (this)
            {
                Player1 = null;
                Player2 = null;
                Player1Rewards = null;
                Player2Rewards = null;
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
        public bool HasBeginDuel;
        public DateTime CommentTime;
        public int Comment;
    }

    class DuelRoomRecord
    {
        public int Win;
        public int Loss;
        public int Draw;
    }
}
