using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        Dictionary<uint, FriendState> GetFriends(Player player)
        {
            lock (player.Friends)
            {
                return new Dictionary<uint, FriendState>(player.Friends);
            }
        }

        Dictionary<Player, FriendState> GetFriendsObj(Player player)
        {
            Dictionary<uint, FriendState> friendStates = GetFriends(player);
            Dictionary<Player, FriendState> friends = new Dictionary<Player, FriendState>();
            lock (playersLock)
            {
                foreach (KeyValuePair<uint, FriendState> friendState in friendStates)
                {
                    Player friendPlayer;
                    if (playersById.TryGetValue(friendState.Key, out friendPlayer))
                    {
                        friends[friendPlayer] = friendState.Value;
                    }
                }
            }
            return friends;
        }

        Dictionary<uint, uint> GetDuelRoomInvites(Player player)
        {
            Dictionary<uint, DuelRoom> duelRooms = GetDuelRoomsByRoomId();

            Dictionary<uint, uint> invites;
            lock (player.DuelRoomInvitesByFriendId)
            {
                invites = new Dictionary<uint, uint>(player.DuelRoomInvitesByFriendId);
            }

            foreach (KeyValuePair<uint, uint> duelRoomInvite in new Dictionary<uint, uint>(player.DuelRoomInvitesByFriendId))
            {
                DuelRoom duelRoom;
                if (!duelRooms.TryGetValue(duelRoomInvite.Value, out duelRoom) || duelRoom.Disbanded || !duelRoom.IsMember(duelRoomInvite.Key))
                {
                    lock (player.DuelRoomInvitesByFriendId)
                    {
                        player.DuelRoomInvitesByFriendId.Remove(duelRoomInvite.Key);
                    }
                }
            }
            return new Dictionary<uint, uint>(player.DuelRoomInvitesByFriendId);
        }

        Dictionary<uint, DuelRoom> GetDuelRoomsByRoomId()
        {
            lock (duelRoomsLocker)
            {
                return new Dictionary<uint, DuelRoom>(duelRoomsByRoomId);
            }
        }

        uint GetNextDuelRoomId(Dictionary<uint, DuelRoom> duelRooms, URNG.LinearCongruentialGenerator rng)
        {
            lock (duelRoomsLocker)
            {
                for (int i = 0; i < DuelRoomMaxId; i++)
                {
                    uint value = duelRoomIdRng.Next();
                    if (!duelRoomsByRoomId.ContainsKey(value))
                    {
                        return value;
                    }
                }
            }
            throw new Exception("Failed to get next duel room id");
        }

        void DisbandRoom(DuelRoom duelRoom)
        {
            List<Player> removedMembers;

            lock (duelRoom.MembersLocker)
            {
                foreach (Player player in duelRoom.Members.Keys)
                {
                    player.DuelRoom = null;
                }
                foreach (Player player in duelRoom.Spectators)
                {
                    player.DuelRoom = null;
                }

                removedMembers = new List<Player>(duelRoom.Members.Keys);

                duelRoom.Members.Clear();
                duelRoom.Spectators.Clear();
            }

            foreach (Player player in removedMembers)
            {
                foreach (KeyValuePair<Player, FriendState> friend in GetFriendsObj(player))
                {
                    lock (friend.Key.DuelRoomInvitesByFriendId)
                    {
                        friend.Key.DuelRoomInvitesByFriendId.Remove(player.Code);
                    }
                }
            }

            lock (duelRoomsLocker)
            {
                duelRoomsByRoomId.Remove(duelRoom.Id);
                duelRoomsBySpectatorRoomId.Remove(duelRoom.SpectatorRoomId);
            }

            duelRoom.Disbanded = true;
            duelRoom.TimeUpdated = DateTime.UtcNow;
        }
    }
}
