﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_RoomGetList(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            Dictionary<string, object> searchOptions = Utils.GetDictionary(request.ActParams, "search_options");
            if (searchOptions == null)
            {
                return;
            }

            bool isSpectator = Utils.GetValue<bool>(searchOptions, "is_spectator");

            Dictionary<string, object> allRoomData = request.GetOrCreateDictionary("Room");
            List<object> roomList = new List<object>();
            allRoomData["room_list"] = roomList;

            Dictionary<uint, DuelRoom> duelRooms = GetDuelRoomsByRoomId();
            foreach (DuelRoom duelRoom in duelRooms.Values)
            {
                if (duelRoom.Disbanded)
                {
                    continue;
                }
                if (isSpectator && (!duelRoom.AllowSpectators || !duelRoom.ShowInSpectatorList))
                {
                    continue;
                }
                if (!isSpectator && !duelRoom.ShowInRoomList)
                {
                    continue;
                }

                Dictionary<string, object> roomData = new Dictionary<string, object>();
                Dictionary<string, object> roomMasterData = Utils.GetOrCreateDictionary(roomData, "room_master");
                roomMasterData["name"] = duelRoom.Owner.Name;
                roomMasterData["icon_id"] = duelRoom.Owner.IconId;
                roomMasterData["icon_frame_id"] = duelRoom.Owner.IconFrameId;
                roomData["room_id"] = duelRoom.Id;
                lock (duelRoom.Members)
                {
                    roomData["room_member"] = duelRoom.Members.Count;
                }
                roomData["test"] = "get";//?
                roomData["hash_key"] = duelRoom.HashKey;
                roomData["create_time"] = duelRoom.TimeCreated;
                roomData["limit_time"] = duelRoom.TimeExpire;
                roomData["room_comment"] = duelRoom.Comment;
                roomData["member_max"] = duelRoom.MemberLimit;
                roomData["is_public"] = duelRoom.ShowInRoomList;
                roomData["is_specter"] = duelRoom.ShowInSpectatorList;
                roomData["is_spectral"] = duelRoom.AllowSpectators;
                roomData["specter_id"] = duelRoom.SpectatorRoomId;
                roomData["is_replay"] = duelRoom.ViewReplays;
                roomData["rule"] = duelRoom.Rule;
                roomData["lp"] = duelRoom.LifePoints;
                roomData["time"] = duelRoom.DuelTime;
                roomData["xPlatformLimit"] = 0;
                roomData["client_version"] = duelRoom.ClientVersion;
                roomData["update_time"] = Utils.GetEpochTime(duelRoom.TimeUpdated);

                roomList.Add(roomData);
                if (roomList.Count >= DuelRoomMaxSearchResults)
                {
                    break;
                }
            }
        }

        void Act_RoomEntry(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            request.Remove("Room.room_info");

            uint roomId = Utils.GetValue<uint>(request.ActParams, "id");
            bool isSpectator = Utils.GetValue<bool>(request.ActParams, "is_specter");
            
            if (roomId == 0)
            {
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_ROOM;
                return;
            }

            DuelRoom duelRoom;

            lock (duelRoomsLocker)
            {
                Dictionary<uint, DuelRoom> duelRoomsDict = isSpectator ? duelRoomsBySpectatorRoomId : duelRoomsByRoomId;

                if (!duelRoomsDict.TryGetValue(roomId, out duelRoom) || duelRoom.Disbanded)
                {
                    request.ResultCode = (int)ResultCodes.RoomCode.ERR_ENTRY_FAILED;
                    return;
                }

                DuelRoom currentDuelRoom = request.Player.DuelRoom;
                if (currentDuelRoom != null && currentDuelRoom.Disbanded)
                {
                    request.Player.DuelRoom = null;
                }
                else if (currentDuelRoom != null && currentDuelRoom != duelRoom)
                {
                    request.ResultCode = (int)ResultCodes.RoomCode.ERR_ALREADY_ENTRY_ROOM;
                    return;
                }
            }

            lock (duelRoom.MembersLocker)
            {
                if ((isSpectator && duelRoom.Members.ContainsKey(request.Player)) ||
                    (!isSpectator && duelRoom.Spectators.Contains(request.Player)))
                {
                    request.ResultCode = (int)ResultCodes.RoomCode.ERR_ALREADY_ENTRY_ROOM;
                    return;
                }

                if ((!isSpectator && !duelRoom.Members.ContainsKey(request.Player)) ||
                    (isSpectator && !duelRoom.Spectators.Contains(request.Player)))
                {
                    if (isSpectator)
                    {
                        if (duelRoom.Spectators.Count >= DuelRoomMaxSpectators)
                        {
                            request.ResultCode = (int)ResultCodes.RoomCode.ERR_ROOM_MEMBER_MAX;
                            return;
                        }
                        duelRoom.Spectators.Add(request.Player);
                        duelRoom.TimeUpdated = DateTime.UtcNow;
                        request.Player.DuelRoom = duelRoom;
                    }
                    else
                    {
                        if (duelRoom.Members.Count >= duelRoom.MemberLimit)
                        {
                            request.ResultCode = (int)ResultCodes.RoomCode.ERR_ROOM_MEMBER_MAX;
                            return;
                        }
                        duelRoom.Members.Add(request.Player, new DuelRoomRecord());
                        duelRoom.TimeUpdated = DateTime.UtcNow;
                        request.Player.DuelRoom = duelRoom;
                    }
                }
            }

            if (!isSpectator)
            {
                lock (request.Player.DuelRoomInvitesByFriendId)
                {
                    foreach (KeyValuePair<uint, uint> invite in GetDuelRoomInvites(request.Player))
                    {
                        if (invite.Value == duelRoom.Id)
                        {
                            request.Player.DuelRoomInvitesByFriendId.Remove(invite.Key);
                        }
                    }
                }
            }

            DuelRoomTable table = duelRoom.GetTable(request.Player);
            if (table != null)
            {
                table.RemovePlayer(request.Player);
            }

            WriteRoomInfo(request, duelRoom);
        }

        void Act_RoomExit(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom currentDuelRoom = request.Player.DuelRoom;
            if (currentDuelRoom != null)
            {
                if (currentDuelRoom.Owner == request.Player)
                {
                    DisbandRoom(currentDuelRoom);
                }
                else
                {
                    currentDuelRoom.RemovePlayer(request.Player);
                }
                currentDuelRoom.TimeUpdated = DateTime.UtcNow;

                foreach (KeyValuePair<Player, FriendState> friend in GetFriendsObj(request.Player))
                {
                    lock (friend.Key.DuelRoomInvitesByFriendId)
                    {
                        friend.Key.DuelRoomInvitesByFriendId.Remove(request.Player.Code);
                    }
                }

                request.Remove("Room.room_info");
            }
        }

        void Act_RoomCreate(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            Dictionary<string, object> roomSettings = Utils.GetDictionary(request.ActParams, "room_settings");

            if (roomSettings == null)
            {
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_ROOM_CREATE_FAILED;
                return;
            }

            if (request.Player.DuelRoom != null)
            {
                Utils.LogWarning("Player " + request.Player + " is trying to create a duel room but is already in a duel room");
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_ALREADY_ENTRY_ROOM;
                return;
            }

            DuelRoom duelRoom = new DuelRoom();
            duelRoom.Id = GetNextDuelRoomId(duelRoomsByRoomId, duelRoomIdRng);
            duelRoom.SpectatorRoomId = GetNextDuelRoomId(duelRoomsBySpectatorRoomId, duelRoomSpectatorRoomIdRng);
            duelRoom.Owner = request.Player;
            duelRoom.Comment = Utils.GetValue<int>(roomSettings, "room_comment");
            duelRoom.MemberLimit = Utils.GetValue<int>(roomSettings, "member_max");
            duelRoom.Rule = Utils.GetValue<int>(roomSettings, "battle_rule");
            duelRoom.DuelTime = Utils.GetValue<int>(roomSettings, "battle_time");
            duelRoom.LifePoints = Utils.GetValue<int>(roomSettings, "battle_lp");
            duelRoom.ShowInRoomList = Utils.GetValue<bool>(roomSettings, "is_public");
            duelRoom.ShowInSpectatorList = Utils.GetValue<bool>(roomSettings, "is_specter");
            duelRoom.AllowSpectators = Utils.GetValue<bool>(roomSettings, "is_spectral");
            duelRoom.ViewReplays = Utils.GetValue<bool>(roomSettings, "is_replay");
            duelRoom.TimeCreated = DateTime.UtcNow;
            duelRoom.TimeUpdated = DateTime.UtcNow;
            duelRoom.TimeExpire = DateTime.UtcNow + TimeSpan.FromDays(1);
            duelRoom.ClientVersion = request.ClientVersion;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(duelRoom.Id + " - " + duelRoom.TimeCreated));
                duelRoom.HashKey = BitConverter.ToString(hash).Replace("-", string.Empty);
            }

            lock (duelRoom.MembersLocker)
            {
                duelRoom.Members.Add(request.Player, new DuelRoomRecord());
                request.Player.DuelRoom = duelRoom;
            }

            lock (duelRoomsLocker)
            {
                duelRoomsByRoomId[duelRoom.Id] = duelRoom;
                duelRoomsBySpectatorRoomId[duelRoom.SpectatorRoomId] = duelRoom;
            }

            duelRoom.InitTables();

            WriteRoomInfo(request, duelRoom);
        }

        void Act_RoomTablePolling(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_ROOM;
            }
            else
            {
                WriteRoomInfo(request, duelRoom);
            }
            request.Remove("Room.room_info");
        }

        void Act_RoomTableArrive(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_ROOM;
            }
            else
            {
                duelRoom.ResetTableStateIfMatchingOrDueling(request.Player);

                int tableIndex = Utils.GetValue<int>(request.ActParams, "table_no");
                if (tableIndex >= 0 && tableIndex < duelRoom.Tables.Length)
                {
                    DeckInfo deck = request.Player.Duel.GetDeck(GameMode.Room);
                    if (deck == null || (!DisableDeckValidation && !deck.IsValid(request.Player, duelRoom.Rule, Regulation)))
                    {
                        request.ResultCode = (int)ResultCodes.RoomCode.ERR_DECK_REG;
                    }
                    else if (duelRoom.Tables[tableIndex].AddPlayer(request.Player))
                    {
                        duelRoom.TimeUpdated = DateTime.UtcNow;
                        WriteRoomInfo(request, duelRoom);
                        request.Remove("Room.room_info");
                    }
                    else
                    {
                        request.ResultCode = (int)ResultCodes.RoomCode.ERR_NO_VACANT_TABLES;
                    }
                }
                else
                {
                    request.ResultCode = (int)ResultCodes.RoomCode.ERR_NO_VACANT_TABLES;
                }
            }
        }

        void Act_RoomTableLeave(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_ROOM;
            }
            else
            {
                duelRoom.ResetTableStateIfMatchingOrDueling(request.Player);

                DuelRoomTable table = duelRoom.GetTable(request.Player);
                if (table != null)
                {
                    table.RemovePlayer(request.Player);
                    duelRoom.TimeUpdated = DateTime.UtcNow;
                }
                WriteRoomInfo(request, duelRoom);
            }
            request.Remove("Room.room_info");
        }

        void Act_RoomSetUserComment(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            int comment = Utils.GetValue<int>(request.ActParams, "comment_id");

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_ROOM;
            }
            else
            {
                DuelRoomTable table = duelRoom.GetTable(request.Player);
                if (table != null)
                {
                    duelRoom.TimeUpdated = DateTime.UtcNow;
                    table.SetPlayerComment(request.Player, comment);
                }
                WriteRoomInfo(request, duelRoom);
            }
            request.Remove("Room.room_info");
        }

        void Act_RoomFriendInvite(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;

            List<uint> inviteRequests = Utils.GetValueTypeList<uint>(request.ActParams, "invite_list");
            if (inviteRequests == null || duelRoom == null)
            {
                return;
            }

            List<string> invitedList = new List<string>();

            Dictionary<string, object> roomData = request.GetOrCreateDictionary("Room");
            Dictionary<string, object> roomInfo = Utils.GetOrCreateDictionary(roomData, "room_info");
            Dictionary<string, object> invitedListInfo = Utils.GetOrCreateDictionary(roomInfo, "invited_list");
            invitedListInfo["user_list"] = invitedList;

            Dictionary<uint, FriendState> friends = GetFriends(request.Player);
            foreach (uint playerId in inviteRequests)
            {
                FriendState friendState;
                if (friends.TryGetValue(playerId, out friendState) && friendState.HasFlag(FriendState.Following) && friendState.HasFlag(FriendState.Follower))
                {
                    Player friend;
                    lock (playersLock)
                    {
                        if (!playersById.TryGetValue(playerId, out friend))
                        {
                            continue;
                        }
                    }
                    invitedList.Add(playerId.ToString());
                    lock (friend.DuelRoomInvitesByFriendId)
                    {
                        friend.DuelRoomInvitesByFriendId[request.Player.Code] = duelRoom.Id;
                    }
                }
            }

            invitedListInfo["num"] = inviteRequests.Count;
            invitedListInfo["success"] = invitedList.Count;
        }

        void Act_RoomBattleReady(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            bool isBattleReady = Utils.GetValue<bool>(request.ActParams, "isBattleReady");
            //int opponentCode = Utils.GetValue<int>(request.ActParams, "opp_code");

            if (isBattleReady)
            {
                if (!sessionServer.HasClientWithToken(request.Player.Token))
                {
                    Utils.LogWarning("[Act_RoomBattleReady] Player '" + request.Player.Name + "' can't enter ready state as they aren't connected to the session server");
                    request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_DATA;
                    return;
                }
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                Utils.LogWarning("[Act_RoomBattleReady] duelRoom");
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_ROOM;
            }
            else
            {
                DuelRoomTable table = duelRoom.GetTable(request.Player);
                if (table == null || !table.IsFull)
                {
                    Utils.LogWarning("[Act_RoomBattleReady] table == null || !table.IsFull");
                    request.ResultCode = (int)ResultCodes.RoomCode.ERR_RIVAL_LEAVE_TABLE;
                }
                else
                {
                    DuelRoomTableEntry playerTableEntry = table.GetEntry(request.Player);
                    if (playerTableEntry != null)
                    {
                        bool anyBattleReady = false;
                        foreach (DuelRoomTableEntry entry in table.Entries)
                        {
                            if (isBattleReady || entry.IsMatchingOrInDuel)
                            {
                                anyBattleReady = true;
                                if (table.State == DuelRoomTableState.Joinable)
                                {
                                    table.State = DuelRoomTableState.Matching;
                                    table.Seed = MultiplayerSeed != -1 ? (uint)MultiplayerSeed : (uint)rand.Next();
                                    table.FirstPlayer = -1;
                                    table.CoinFlipPlayerIndex = MultiplayerCoinFlipPlayerIndex != -1 ? MultiplayerCoinFlipPlayerIndex : rand.Next(2);
                                    table.CoinFlipCounter = MultiplayerCoinFlipCounter;
                                    using (SHA1 sha1 = SHA1.Create())
                                    {
                                        table.TableHash = BitConverter.ToString(sha1.ComputeHash(Guid.NewGuid().ToByteArray())).Replace("-", string.Empty);
                                    }
                                    using (MD5 md5 = MD5.Create())
                                    {
                                        table.TableTicket = BitConverter.ToString(md5.ComputeHash(Guid.NewGuid().ToByteArray())).Replace("-", string.Empty);
                                    }
                                }
                                break;
                            }
                        }
                        if (!anyBattleReady)
                        {
                            table.State = DuelRoomTableState.Joinable;
                        }

                        playerTableEntry.IsMatchingOrInDuel = isBattleReady;
                    }

                    WriteRoomInfo(request, duelRoom);
                }
            }

            request.Remove("Duel");
        }

        void Act_DuelMenuDeckCheck(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            PlayMode playMode = Utils.GetValue<PlayMode>(request.ActParams, "kind");
            //int tid = Utils.GetValue<int>(request.ActParams, "tid");//?
            //int regulation = Utils.GetValue<int>(request.ActParams, "regulation_id");

            if (playMode != PlayMode.ROOM)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null || !Regulation.ContainsKey(duelRoom.Rule.ToString()))
            {
                return;
            }

            DeckInfo deck = request.Player.Duel.GetDeck(GameMode.Room);

            Dictionary<string, object> roomData = request.GetOrCreateDictionary("Room");
            Dictionary<string, object> deckInfoData = Utils.GetOrCreateDictionary(roomData, "deck_info");
            if (deck != null)
            {
                deckInfoData["deck_id"] = deck.Id;
                deckInfoData["deck"] = deck.ToDictionary();
                deckInfoData["valid"] = DisableDeckValidation || deck.IsValid(request.Player, duelRoom.Rule, Regulation);
                deckInfoData["possession"] = true;
                deckInfoData["regulation"] = deck.RegulationId.ToString();
            }
            else
            {
                deckInfoData["valid"] = false;
                deckInfoData["possession"] = false;
            }

            request.Remove("Duel");
        }

        void WriteRoomInfo(GameServerWebRequest request, DuelRoom duelRoom)
        {
            if (duelRoom == null)
            {
                return;
            }

            Dictionary<string, object> roomData = request.GetOrCreateDictionary("Room");
            Dictionary<string, object> roomInfo = Utils.GetOrCreateDictionary(roomData, "room_info");
            roomInfo["room_id"] = duelRoom.Id;
            roomInfo["room_master"] = duelRoom.Owner.Code;
            lock (duelRoom.MembersLocker)
            {
                roomInfo["is_join_player"] = duelRoom.Members.ContainsKey(request.Player);
            }
            roomInfo["member_num"] = duelRoom.MemberCount;
            roomInfo["xPlatformLimit"] = 0;
            roomInfo["room_specter_id"] = duelRoom.SpectatorRoomId;
            roomInfo["specter_num"] = duelRoom.SpectatorCount;
            roomInfo["limit_time"] = duelRoom.TimeExpire.ToString("yyyy/MM/dd HH:mm:ss") + " (UTC)";
            Dictionary<string, object> membersData = Utils.GetOrCreateDictionary(roomInfo, "room_member");
            lock (duelRoom.MembersLocker)
            {
                foreach (KeyValuePair<Player, DuelRoomRecord> player in duelRoom.Members)
                {
                    Dictionary<string, object> memberData = new Dictionary<string, object>();
                    memberData["record"] = new Dictionary<string, object>()
                    {
                        { "win", player.Value.Win },
                        { "loss", player.Value.Loss },
                        { "draw", player.Value.Draw },
                    };
                    if (player.Key != request.Player)
                    {
                        memberData["same_platform"] = true;
                    }
                    memberData["platform_name"] = "";
                    memberData["platform"] = 0;
                    //memberData["comment"] = 0;// Doesn't seem to do anything? Only new_comment works
                    memberData["name"] = player.Key.Name;
                    memberData["icon_id"] = player.Key.IconId;
                    memberData["icon_frame_id"] = player.Key.IconFrameId;
                    membersData[player.Key.Code.ToString()] = memberData;
                }
            }
            roomInfo["is_public"] = duelRoom.ShowInRoomList;
            roomInfo["member_max"] = duelRoom.MemberLimit;
            roomInfo["room_comment"] = duelRoom.Comment;
            roomInfo["is_specter"] = duelRoom.ShowInSpectatorList;
            roomInfo["is_spectral"] = duelRoom.AllowSpectators;
            roomInfo["is_replay"] = duelRoom.ViewReplays;
            roomInfo["battle_setting"] = new Dictionary<string, object>()
            {
                { "rule", duelRoom.Rule },
                { "lp", duelRoom.LifePoints },
                { "time", duelRoom.DuelTime }
            };
            List<object> tableInfos = new List<object>();
            foreach (DuelRoomTable table in duelRoom.Tables)
            {
                Player player1 = table.Player1;
                Player player2 = table.Player2;

                DuelRoomTableState state = table.State;
                if ((request.Player == player1 || request.Player == player2) && state == DuelRoomTableState.Dueling)
                {
                    state = DuelRoomTableState.Matched;
                }

                Dictionary<string, object> tableInfo = new Dictionary<string, object>();
                tableInfo["player1"] = player1 != null ? player1.Code : 0;
                tableInfo["player2"] = player2 != null ? player2.Code : 0;
                tableInfo["stats"] = (int)state;
                tableInfos.Add(tableInfo);
            }
            roomInfo["table_info"] = tableInfos;

            Dictionary<string, object> playerComments = new Dictionary<string, object>();

            foreach (DuelRoomTable table in duelRoom.Tables)
            {
                foreach (DuelRoomTableEntry entry in table.Entries)
                {
                    Player player = entry.Player;
                    int comment = entry.Comment;
                    if (player != null && comment > 0 && entry.CommentTime > DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomCommentTimeoutInSeconds))
                    {
                        playerComments[player.Code.ToString()] = entry.Comment;
                    }
                }
            }

            if (playerComments.Count > 0)
            {
                roomInfo["new_comment"] = playerComments;
            }
            else
            {
                roomInfo["new_comment"] = new int[0];
            }
        }

        void Act_DuelMatching(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            //Dictionary<string, object> rule = Utils.GetDictionary(request.ActParams, "rule");
            //Utils.GetValue<int>(rule, "mode");// 5 for room duel?

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                Utils.LogWarning("[Act_DuelMatching] duelRoom == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTable table = duelRoom.GetTable(request.Player);
            if (table == null)
            {
                Utils.LogWarning("[Act_DuelMatching] table == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                Player p1 = table.Player1;
                Player p2 = table.Player2;
                if (p1 == null || p2 == null)
                {
                    Utils.LogWarning("[Act_DuelMatching] p1 == null || p2 == null");
                    request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                    return;
                }
                DeckInfo d1 = p1.Duel.GetDeck(GameMode.Room);
                DeckInfo d2 = p2.Duel.GetDeck(GameMode.Room);
                if (d1 == null || d2 == null)
                {
                    Utils.LogWarning("[Act_DuelMatching] d1 == null || d2 == null");
                    request.ResultCode = (int)ResultCodes.PvPCode.INVALID_DECK;
                    return;
                }

                DuelRoomTableEntry tableEntry = table.GetEntry(request.Player);
                if (tableEntry == null || !tableEntry.IsMatchingOrInDuel)
                {
                    Utils.LogWarning("[Act_DuelMatching] tableEntry == null || !tableEntry.IsMatchingOrInDuel");
                    request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                    return;
                }

                DuelRoomTableState state = table.State;
                if (state != DuelRoomTableState.Matching && state != DuelRoomTableState.Matched)
                {
                    Utils.LogWarning("[Act_DuelMatching] state != DuelRoomTableState.Matching && state != DuelRoomTableState.Matched");
                    request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                    return;
                }

                if (table.IsMatched)
                {
                    if (state != DuelRoomTableState.Matched)
                    {
                        table.MatchedTime = DateTime.UtcNow;
                        table.State = DuelRoomTableState.Matched;
                    }

                    Dictionary<string, object> duelData = request.GetOrCreateDictionary("Duel");
                    /*duelData["match"] = new Dictionary<string, object>()
                    {
                        { "url", serverPollUrl + "/pvp00/" + table.TableHash },// Setting the url results in polling (even in solo)
                        { "ticket", table.TableTicket },
                        { "stat", true }
                    };*/
                    List<object> playersData = new List<object>();
                    Tuple<Player, DeckInfo>[] players =
                    {
                        new Tuple<Player, DeckInfo>(p1, d1),
                        new Tuple<Player, DeckInfo>(p2, d2),
                    };
                    for (int i = 0; i < players.Length; i++)
                    {
                        Player player = players[i].Item1;
                        DeckInfo deck = players[i].Item2;
                        Dictionary<uint, FriendState> friends = GetFriends(player);
                        Dictionary<string, object> playerData = new Dictionary<string, object>();
                        playerData["pcode"] = player.Code;
                        playerData["name"] = player.Name;
                        playerData["mat"] = deck.Accessory.Field;
                        playerData["sleeve"] = deck.Accessory.Sleeve;
                        playerData["rank"] = new Dictionary<string, object>()
                        {
                            { "rank", player.Rank },
                            { "rate", player.Rate }
                        };
                        playerData["icon"] = player.IconId;
                        playerData["icon_frame"] = player.IconFrameId;
                        playerData["avatar"] = player.AvatarId;
                        playerData["avatar_home"] = deck.Accessory.AvBase;
                        playerData["duel_object"] = deck.Accessory.FieldObj;
                        playerData["deck_case"] = deck.Accessory.Box;
                        playerData["level"] = player.Level;
                        playerData["wallpaper"] = player.Wallpaper;
                        playerData["profile_tag"] = player.TitleTags.ToArray();
                        playerData["follow_num"] = friends.Count(x => x.Value.HasFlag(FriendState.Following));
                        playerData["follower_num"] = friends.Count(x => x.Value.HasFlag(FriendState.Follower));
                        playerData["one_more_effect"] = 0;//?
                        playerData["official"] = 0;//?
                        playerData["os"] = (int)PlatformID.Steam;
                        playerData["is_same_os"] = null;
                        playerData["online_id"] = null;
                        playerData["dlv_effect"] = null;
                        playersData.Add(playerData);
                    }
                    duelData["player"] = playersData;
                    duelData["myid"] = request.Player == p1 ? 0 : 1;
                    duelData["MyID"] = request.Player == p1 ? 0 : 1;
                    duelData["choice"] = table.CoinFlipPlayerIndex;
                    duelData["mat"] = new int[2] { d1.Accessory.Field, d2.Accessory.Field };
                    duelData["sleeve"] = new int[2] { d1.Accessory.Sleeve, d2.Accessory.Sleeve };
                    duelData["icon"] = new int[2] { p1.IconId, p2.IconId };
                    duelData["icon_frame"] = new int[2] { p1.IconFrameId, p2.IconFrameId };
                    duelData["avatar"] = new int[2] { p1.AvatarId, p2.AvatarId };
                    duelData["avatar_home"] = new int[2] { d1.Accessory.AvBase, d2.Accessory.AvBase };
                    duelData["duel_object"] = new int[2] { d1.Accessory.FieldObj, d2.Accessory.FieldObj };
                    duelData["deck_case"] = new int[2] { d1.Accessory.Box, d2.Accessory.Box };
                    duelData["os"] = new int[2] { (int)PlatformID.Steam, (int)PlatformID.Steam };
                    duelData["pcode"] = new uint[2] { p1.Code, p2.Code };
                    duelData["official"] = new int[2] { 0, 0 };
                    request.Remove("Duel");
                    return;
                }
                Thread.Sleep(500);
            }

            request.ResultCode = (int)ResultCodes.PvPCode.TIMEOUT;
        }

        void Act_DuelMatchingCancel(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                Utils.LogWarning("[Act_DuelMatchingCancel] duelRoom == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            duelRoom.ResetTableStateIfMatchingOrDueling(request.Player);
            WriteRoomInfo(request, duelRoom);
        }

        void Act_DuelStartWating(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                Utils.LogWarning("[Act_DuelStartWating] duelRoom == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTable table = duelRoom.GetTable(request.Player);
            if (table == null)
            {
                Utils.LogWarning("[Act_DuelStartWating] table == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTableEntry tableEntry = table.GetEntry(request.Player);
            if (tableEntry == null || !tableEntry.IsMatchingOrInDuel)
            {
                Utils.LogWarning("[Act_DuelStartWating] tableEntry == null || !tableEntry.IsMatchingOrInDuel");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            Player p1 = table.Player1;
            Player p2 = table.Player2;
            if (p1 == null || p2 == null)
            {
                Utils.LogWarning("[Act_DuelStartWating] p1 == null || p2 == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            DuelRoomTableState state = table.State;
            if (state != DuelRoomTableState.Matched && state != DuelRoomTableState.Dueling)
            {
                Utils.LogWarning("[Act_DuelStartWating] state != DuelRoomTableState.Matched && state != DuelRoomTableState.Dueling");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            if (table.MatchedTime < DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomTableMatchingTimeoutInSeconds))
            {
                table.ClearMatching();
                Utils.LogWarning("[Act_DuelStartWating] table.MatchedTime < DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomTableMatchingTimeoutInSeconds)");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            Thread.Sleep(1000);

            Dictionary<string, object> duelData = request.GetOrCreateDictionary("Duel");
            duelData["pvp_choice"] = new Dictionary<string, object>()
            {
                { "choice", table.CoinFlipPlayerIndex },
                { "sha", table.TableHash },
                { "cnt", Math.Max(0, table.CoinFlipCounter) }
            };
            duelData["myid"] = request.Player == p1 ? 0 : 1;
            int firstPlayer = table.FirstPlayer;
            if (firstPlayer >= 0 || table.CoinFlipCounter <= -1)
            {
                if (firstPlayer == -1 && table.Entries[table.CoinFlipPlayerIndex].Player == request.Player)
                {
                    state = table.State;
                    if (state != DuelRoomTableState.Matched && state != DuelRoomTableState.Dueling)
                    {
                        Utils.LogWarning("[Act_DuelStartWating] state != DuelRoomTableState.Matched && state != DuelRoomTableState.Dueling");
                        request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                        return;
                    }
                    table.State = DuelRoomTableState.Dueling;
                    table.FirstPlayer = firstPlayer = rand.Next(2);
                }
                if (firstPlayer >= 0)
                {
                    duelData["first"] = firstPlayer;
                    return;
                }
            }
            if (table.Entries[table.CoinFlipPlayerIndex].Player == request.Player)
            {
                if (table.CoinFlipCounter >= 0)
                {
                    table.CoinFlipCounter--;
                }
            }

            request.ResultCode = (int)ResultCodes.PvPCode.TIMEOUT;
        }

        void Act_DuelStartSelecting(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                Utils.LogWarning("[Act_DuelStartSelecting] duelRoom == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTable table = duelRoom.GetTable(request.Player);
            if (table == null)
            {
                Utils.LogWarning("[Act_DuelStartSelecting] table == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTableEntry tableEntry = table.GetEntry(request.Player);
            if (tableEntry == null || !tableEntry.IsMatchingOrInDuel)
            {
                Utils.LogWarning("[Act_DuelStartSelecting] tableEntry == null || !tableEntry.IsMatchingOrInDuel");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            DuelRoomTableState state = table.State;
            if (state != DuelRoomTableState.Matched && state != DuelRoomTableState.Dueling)
            {
                Utils.LogWarning("[Act_DuelStartSelecting] state != DuelRoomTableState.Matched && state != DuelRoomTableState.Dueling");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            if (table.MatchedTime < DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomTableMatchingTimeoutInSeconds))
            {
                table.ClearMatching();
                Utils.LogWarning("[Act_DuelStartSelecting] table.MatchedTime < DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomTableMatchingTimeoutInSeconds)");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            Player p1 = table.Player1;
            Player p2 = table.Player2;
            if (p1 == null || p2 == null)
            {
                Utils.LogWarning("[Act_DuelStartSelecting] p1 == null || p2 == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            int select = Utils.GetValue<int>(request.ActParams, "select");
            if (select >= 0 && select <= 1)
            {
                if (request.Player != p1)
                {
                    select = 1 - select;
                }
                table.State = DuelRoomTableState.Dueling;
                table.FirstPlayer = select;
            }
            else
            {
                Utils.LogWarning("[Act_DuelStartSelecting] select >= 0 && select <= 1");
                request.ResultCode = (int)ResultCodes.PvPCode.INVALID_PARAM;
            }
        }

        void Act_DuelBeginPvp(GameServerWebRequest request)
        {
            if (!MultiplayerEnabled)
            {
                return;
            }

            DuelRoom duelRoom = request.Player.DuelRoom;
            if (duelRoom == null)
            {
                Utils.LogWarning("[Act_DuelBeginPvp] duelRoom == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTable table = duelRoom.GetTable(request.Player);
            if (table == null)
            {
                Utils.LogWarning("[Act_DuelBeginPvp] table == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_EXIST_ROOM;
                return;
            }

            DuelRoomTableEntry tableEntry = table.GetEntry(request.Player);
            if (tableEntry == null || !tableEntry.IsMatchingOrInDuel)
            {
                Utils.LogWarning("[Act_DuelBeginPvp] tableEntry == null || !tableEntry.IsMatchingOrInDuel");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            if (table.MatchedTime < DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomTableMatchingTimeoutInSeconds))
            {
                table.ClearMatching();
                Utils.LogWarning("[Act_DuelBeginPvp] table.MatchedTime < DateTime.UtcNow - TimeSpan.FromSeconds(DuelRoomTableMatchingTimeoutInSeconds)");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            Player p1 = table.Player1;
            Player p2 = table.Player2;
            if (p1 == null || p2 == null)
            {
                Utils.LogWarning("[Act_DuelBeginPvp] p1 == null || p2 == null");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            DeckInfo d1 = p1.Duel.GetDeck(GameMode.Room);
            DeckInfo d2 = p2.Duel.GetDeck(GameMode.Room);
            if (d1 == null || d2 == null)
            {
                Utils.LogWarning("[Act_DuelBeginPvp] d1 == null || d2 == null");
                request.ResultCode = (int)ResultCodes.PvPCode.INVALID_DECK;
                return;
            }

            if (table.State != DuelRoomTableState.Dueling)
            {
                Utils.LogWarning("[Act_DuelBeginPvp] table.State != DuelRoomTableState.Dueling");
                request.ResultCode = (int)ResultCodes.PvPCode.NOT_FIND_OPPONENT;
                return;
            }

            if (!sessionServer.HasClientWithToken(request.Player.Token))
            {
                Utils.LogWarning("[Act_DuelBeginPvp] Player '" + request.Player.Name + "' can't enter duel as they aren't connected to the session server");
                request.ResultCode = (int)ResultCodes.RoomCode.ERR_INVALID_DATA;
                duelRoom.ResetTableStateIfMatchingOrDueling(request.Player);
                return;
            }

            Dictionary<uint, FriendState> p1Friends = GetFriends(p1);
            Dictionary<uint, FriendState> p2Friends = GetFriends(p2);

            Player[] players = { p1, p2 };
            DeckInfo[] decks = { d1, d2 };
            Dictionary<uint, FriendState>[] friends = { p1Friends, p2Friends };

            request.Player.Duel.Mode = GameMode.Room;

            DuelSettings duelSettings = new DuelSettings();
            duelSettings.RandSeed = table.Seed;
            duelSettings.FirstPlayer = table.FirstPlayer;
            duelSettings.noshuffle = MultiplayerNoShuffle;
            duelSettings.tag = false;
            duelSettings.dlginfo = false;
            duelSettings.MyID = request.Player == p1 ? 0 : 1;
            duelSettings.MyType = 0;
            duelSettings.Type = 0;
            duelSettings.MyPartnerType = 0;
            duelSettings.PlayableTagPartner = 0;
            duelSettings.regulation_id = 0;
            duelSettings.duel_start_timestamp = 0;
            duelSettings.surrender = true;
            duelSettings.Limit = 16;//0 (16 = NoRandom)
            duelSettings.GameMode = (int)GameMode.Room;//GameMode.Normal
            duelSettings.cpu = 100;
            duelSettings.cpuflag = null;
            duelSettings.LeftTimeMax = 300;//0
            duelSettings.TurnTimeMax = 300;//0
            duelSettings.TotalTimeMax = 300;//0
            duelSettings.Auto = -1;
            duelSettings.rec = false;
            duelSettings.recf = false;
            duelSettings.did = 0;
            duelSettings.duel = null;
            duelSettings.is_pvp = true;//false
            duelSettings.chapter = 0;
            duelSettings.SetRandomBgm(rand);
            for (int i = 0; i < players.Length; i++)
            {
                duelSettings.Deck[i] = decks[i];
                duelSettings.life[i] = duelRoom.LifePoints == 1 ? 8000 : 4000;
                duelSettings.level[i] = players[i].Level;
                duelSettings.follow_num[i] = friends[i].Count(x => x.Value.HasFlag(FriendState.Following));
                duelSettings.pcode[i] = (int)players[i].Code;
                duelSettings.rank[i] = players[i].Rank;
                duelSettings.DuelistLv[i] = players[i].Rate;
                duelSettings.name[i] = players[i].Name;
                duelSettings.avatar[i] = players[i].AvatarId;
                duelSettings.avatar_home[i] = decks[i].Accessory.AvBase;
                duelSettings.icon[i] = players[i].IconId;
                duelSettings.icon_frame[i] = players[i].IconFrameId;
                duelSettings.sleeve[i] = decks[i].Accessory.Sleeve;
                duelSettings.mat[i] = decks[i].Accessory.Field;
                duelSettings.duel_object[i] = decks[i].Accessory.FieldObj;
                duelSettings.wallpaper[i] = players[i].Wallpaper;
                duelSettings.profile_tag[i] = players[i].TitleTags.ToList();
            }
            duelSettings.SetRequiredDefaults();
            request.Response["Duel"] = duelSettings.ToDictionary();

            request.Remove("Duel", "DuelResult", "Result");
        }

        public string GetDuelingOpponentToken(string playerToken)
        {
            Player player;
            lock (playersLock)
            {
                playersByToken.TryGetValue(playerToken, out player);
            }

            if (player == null)
            {
                return null;
            }

            DuelRoom duelRoom = player.DuelRoom;
            if (duelRoom == null)
            {
                return null;
            }

            DuelRoomTable table = duelRoom.GetTable(player);
            if (table == null || table.State != DuelRoomTableState.Dueling)
            {
                return null;
            }

            Player p1 = table.Player1;
            Player p2 = table.Player2;
            if (p1 == null || p2 == null)
            {
                return null;
            }

            if (p1 != player && p2 != player)
            {
                return null;
            }

            return p1 == player ? p2.Token : p1.Token;
        }
    }
}
