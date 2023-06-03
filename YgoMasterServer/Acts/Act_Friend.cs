using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_FriendGetList(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            // All (true) = clicking friends button
            // All (false) = following/unfollowing someone
            bool isAll = Utils.GetValue<bool>(request.ActParams, "all");

            Dictionary<uint, FriendState> friends = GetFriends(request.Player);
            Dictionary<uint, uint> duelRoomInvitesByFriendId = GetDuelRoomInvites(request.Player);

            Dictionary<uint, Player> friendsById = new Dictionary<uint, Player>();
            lock (playersLock)
            {
                foreach (KeyValuePair<uint, FriendState> friend in friends)
                {
                    Player friendPlayer;
                    if (playersById.TryGetValue(friend.Key, out friendPlayer))
                    {
                        friendsById[friend.Key] = friendPlayer;
                    }
                }
            }

            Dictionary<string, object> friendData = request.GetOrCreateDictionary("Friend");

            Dictionary<string, object> allFollowData = Utils.GetOrCreateDictionary(friendData, "follow");
            Dictionary<string, object> allFollowerData = Utils.GetOrCreateDictionary(friendData, "follower");

            int nextFollowerId = 0;

            foreach (KeyValuePair<uint, FriendState> friendInfo in friends)
            {
                long followedDate = friendInfo.Key;// TODO: Change FriendState to class and provide extra detail
                Player friend;
                if (friendsById.TryGetValue(friendInfo.Key, out friend))
                {
                    if (friendInfo.Value.HasFlag(FriendState.Following))
                    {
                        Dictionary<string, object> followData = new Dictionary<string, object>();
                        followData["name"] = friend.Name;
                        followData["pcode"] = friend.Code;
                        followData["icon_id"] = friend.IconId;
                        followData["icon_frame_id"] = friend.IconFrameId;
                        followData["wallpaper"] = friend.Wallpaper;
                        followData["mutual"] = friendInfo.Value.HasFlag(FriendState.Follower);
                        followData["pin"] = friendInfo.Value.HasFlag(FriendState.Pinned);
                        followData["watch"] = friend.IsDuelingPVP;

                        uint roomId;
                        if (duelRoomInvitesByFriendId.TryGetValue(friendInfo.Key, out roomId))
                        {
                            followData["room_id"] = roomId;
                        }

                        if (friend.LastRequestTime != default(DateTime) &&
                            friend.LastRequestTime > DateTime.UtcNow - TimeSpan.FromSeconds(FriendOfflineInSeconds))
                        {
                            followData["online_time"] = Utils.GetEpochTime(friend.LastRequestTime);
                        }
                        if (friend.LoginTime != default(DateTime))
                        {
                            followData["login_time"] = Utils.GetEpochTime(friend.LoginTime);
                        }

                        allFollowData[friendInfo.Key.ToString()] = followData;
                    }

                    if (friendInfo.Value.HasFlag(FriendState.Follower))
                    {
                        Dictionary<string, object> followerData = new Dictionary<string, object>();
                        followerData["name"] = friend.Name;
                        followerData["pcode"] = friend.Code;
                        followerData["icon_id"] = friend.IconId;
                        followerData["icon_frame_id"] = friend.IconFrameId;
                        followerData["wallpaper"] = friend.Wallpaper;
                        followerData["mutual"] = friendInfo.Value.HasFlag(FriendState.Following);
                        followerData["watch"] = friend.IsDuelingPVP;
                        if (friend.LastRequestTime != default(DateTime) &&
                            friend.LastRequestTime > DateTime.UtcNow - TimeSpan.FromSeconds(FriendOfflineInSeconds))
                        {
                            followerData["online_time"] = Utils.GetEpochTime(friend.LastRequestTime);
                        }
                        if (friend.LoginTime != default(DateTime))
                        {
                            followerData["login_time"] = Utils.GetEpochTime(friend.LoginTime);
                        }

                        allFollowerData[nextFollowerId.ToString()] = followerData;
                        nextFollowerId++;
                    }
                }
            }

            allFollowerData["is_terminal"] = true;//?

            friendData["block"] = new uint[0];
            friendData["tag"] = new uint[0];
            friendData["refresh_sec"] = FriendsRefreshInSeconds;

            if (isAll)
            {
                lock (request.Player.DuelRoomInvitesByFriendId)
                {
                    request.Player.DuelRoomInvitesByFriendId.Clear();
                }
            }

            request.Remove("Friend.follow", "Friend.follower", "Friend.block", "Friend.tag", "Friend.refresh_sec", "Friend.refresh");
        }

        void Act_FriendRefreshInfo(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            /*List<uint> pcodeList = Utils.GetValueTypeList<uint>(request.ActParams, "pcode_list");
            Dictionary<string, object> friendData = request.GetOrCreateDictionary("Friend");
            Dictionary<string, object> refreshData = Utils.GetOrCreateDictionary(friendData, "refresh");*/
        }

        void Act_FriendFollow(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");
            bool delete = Utils.GetValue<bool>(request.ActParams, "delete");

            bool mutual = false;

            UpdateFriendState(request.Player, pcode, FriendState.Following, delete, false);

            Player friendPlayer;
            lock (playersLock)
            {
                playersById.TryGetValue(pcode, out friendPlayer);
            }
            if (friendPlayer != null)
            {
                UpdateFriendState(friendPlayer, request.Player.Code, FriendState.Follower, delete, true);
            }

            Dictionary<uint, FriendState> friends = GetFriends(request.Player);
            FriendState friendState;
            if (friends.TryGetValue(pcode, out friendState) && friendState.HasFlag(FriendState.Following) && friendState.HasFlag(FriendState.Following))
            {
                mutual = true;
            }

            if (friendPlayer != null && !friendState.HasFlag(FriendState.Following))
            {
                lock (friendPlayer.DuelRoomInvitesByFriendId)
                {
                    friendPlayer.DuelRoomInvitesByFriendId.Remove(request.Player.Code);
                }
            }

            Dictionary<string, object> friendData = request.GetOrCreateDictionary("Friend");
            Dictionary<string, object> updatedData = Utils.GetOrCreateDictionary(friendData, "updated");
            updatedData["pcode"] = pcode;
            updatedData["mutal"] = mutual;
        }

        void Act_FriendIdSearch(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");

            Player player;
            lock (playersLock)
            {
                playersById.TryGetValue(pcode, out player);
            }

            if (player != null && player == request.Player)
            {
                request.ResultCode = (int)ResultCodes.FriendCode.ACCOUNT_OWN;
                return;
            }

            Dictionary<string, object> friendData = request.GetOrCreateDictionary("Friend");
            Dictionary<string, object> searchData = Utils.GetOrCreateDictionary(friendData, "search");
            List<object> searchDataList = new List<object>();
            searchData["excess"] = false;
            searchData["list"] = searchDataList;

            if (player != null)
            {
                Dictionary<uint, FriendState> friends = GetFriends(request.Player);
                FriendState friendState;
                friends.TryGetValue(player.Code, out friendState);
                searchDataList.Add(GetSearchFriendData(player, friendState));
            }

            request.Remove("Friend.search");
        }

        void Act_FriendTagSearch(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            List<int> tags = Utils.GetIntList(request.ActParams, "tag");

            Dictionary<string, object> friendData = request.GetOrCreateDictionary("Friend");
            Dictionary<string, object> searchData = Utils.GetOrCreateDictionary(friendData, "search");
            List<object> searchDataList = new List<object>();

            Dictionary<uint, FriendState> friends = GetFriends(request.Player);
            List<Player> players = new List<Player>();

            lock (playersLock)
            {
                foreach (Player player in playersById.Values)
                {
                    if (player.TitleTags.Intersect(tags).Count() == tags.Count && player != request.Player)
                    {
                        players.Add(player);
                    }
                }
            }

            for (int i = 0; i < players.Count && i < FriendSearchLimit; i++)
            {
                Player player = players[i];

                FriendState friendState;
                friends.TryGetValue(player.Code, out friendState);
                searchDataList.Add(GetSearchFriendData(player, friendState));
            }

            searchData["excess"] = players.Count > FriendSearchLimit;
        }

        Dictionary<string, object> GetSearchFriendData(Player friend, FriendState friendState)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["name"] = friend.Name;
            result["pcode"] = friend.Code;
            result["icon_id"] = friend.IconId;
            result["icon_frame_id"] = friend.IconFrameId;
            result["wallpaper"] = friend.Wallpaper;
            result["friend"] = (int)(friendState & ~FriendState.Pinned);
            result["pin"] = friendState.HasFlag(FriendState.Pinned);
            result["watch"] = friend.IsDuelingPVP;
            if (friend.LastRequestTime != default(DateTime) &&
                friend.LastRequestTime > DateTime.UtcNow - TimeSpan.FromSeconds(FriendOfflineInSeconds))
            {
                result["online_time"] = Utils.GetEpochTime(friend.LastRequestTime);
            }
            if (friend.LoginTime != default(DateTime))
            {
                result["login_time"] = Utils.GetEpochTime(friend.LoginTime);
            }
            return result;
        }

        void UpdateFriendState(Player player, uint friendId, FriendState stateToChange, bool delete, bool saveNow)
        {
            if (player.Code == friendId)
            {
                // You can't friend yourself...
                return;
            }

            lock (player.Friends)
            {
                FriendState friendState;
                bool hasFriend = player.Friends.TryGetValue(friendId, out friendState);
                if (delete)
                {
                    friendState &= ~stateToChange;
                }
                else
                {
                    friendState |= stateToChange;
                }
                if ((friendState & ~FriendState.Pinned) == FriendState.None)
                {
                    player.Friends.Remove(friendId);
                }
                else
                {
                    player.Friends[friendId] = friendState;
                }
            }
            if (saveNow)
            {
                SavePlayerNow(player);
            }
            else
            {
                SavePlayer(player);
            }
        }

        void Act_FriendSetPin(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");
            bool delete = Utils.GetValue<bool>(request.ActParams, "delete");
            bool updateWork = Utils.GetValue<bool>(request.ActParams, "update_work");

            Player friendPlayer;
            lock (playersLock)
            {
                playersById.TryGetValue(pcode, out friendPlayer);
            }

            FriendState friendState;
            lock (request.Player.Friends)
            {
                if (request.Player.Friends.TryGetValue(pcode, out friendState))
                {
                    if (friendState.HasFlag(FriendState.Following) && !delete)
                    {
                        friendState |= FriendState.Pinned;
                    }
                    else
                    {
                        friendState &= ~FriendState.Pinned;
                    }
                    request.Player.Friends[pcode] = friendState;
                }
            }
            SavePlayer(request.Player);

            if (updateWork)
            {
                Dictionary<string, object> friendData = request.GetOrCreateDictionary("Friend");
                Dictionary<string, object> allFollowData = Utils.GetOrCreateDictionary(friendData, "follow");
                Dictionary<string, object> followData = Utils.GetOrCreateDictionary(allFollowData, pcode.ToString());
                followData["pin"] = friendState.HasFlag(FriendState.Pinned);
            }
        }
    }
}
