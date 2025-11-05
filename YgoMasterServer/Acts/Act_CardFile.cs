using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_CardFileGetList(GameServerWebRequest request)
        {
            Player player;
            if (MultiplayerEnabled)
            {
                uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");
                lock (playersLock)
                {
                    playersById.TryGetValue(pcode == 0 ? request.Player.Code : pcode, out player);
                }
            }
            else
            {
                player = request.Player;
            }
            if (player == null)
            {
                return;
            }

            string playerKind = player == request.Player ? "User" : "Friend";
            Dictionary<string, object> cardFileData = request.GetOrCreateDictionary("CardFile");
            Dictionary<string, object> cardFileUserDataDict = Utils.GetOrCreateDictionary(cardFileData, playerKind);
            foreach (UserCardFileStatus cardFileStatus in player.CardFiles.Files.Values)
            {
                cardFileUserDataDict[cardFileStatus.Id.ToString()] = new Dictionary<string, object>()
                {
                    { "collect_card_cnt", cardFileStatus.Cards.Count(x => x.Value.Have && !x.Value.Dismantled) },
                    { "complete_status", cardFileStatus.IsComplete() ? 1 : 0 }
                };
            }
            WriteUserProfileCardFile(request, player);
            request.Remove("CardFile." + playerKind);
        }

        void Act_CardFileDetail(GameServerWebRequest request)
        {
            Player player;
            if (MultiplayerEnabled)
            {
                uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");
                lock (playersLock)
                {
                    playersById.TryGetValue(pcode == 0 ? request.Player.Code : pcode, out player);
                }
            }
            else
            {
                player = request.Player;
            }
            if (player == null)
            {
                return;
            }

            string playerKind = player == request.Player ? "User" : "Friend";
            int itemId = Utils.GetValue<int>(request.ActParams, "item_id");
            //bool isOpenProfile = Utils.GetValue<bool>(request.ActParams, "is_open_profile");// Purpose?
            UserCardFileStatus cardFileStatus;
            CardFileData cardFile;
            if (player.CardFiles.Files.TryGetValue(itemId, out cardFileStatus) && CardFiles.TryGetValue(itemId, out cardFile))
            {
                bool haveAnyCardsBeenDismantled = false;
                bool isNewlyComplete = false;
                if (player == request.Player)
                {
                    bool wasComplete = cardFileStatus.IsComplete();
                    foreach (UserCardFileStatusCard card in cardFileStatus.Cards.Values)
                    {
                        if (card.Have != card.RealHave)
                        {
                            if (card.Dismantled)
                            {
                                haveAnyCardsBeenDismantled = true;
                            }
                            card.Have = card.RealHave;
                            if (card.Have)
                            {
                                card.IsNew = true;
                            }
                            request.Player.RequiresSaving = true;
                        }
                    }
                    isNewlyComplete = !wasComplete && cardFileStatus.IsComplete();
                }

                Dictionary<string, object> cardFileData = request.GetOrCreateDictionary("CardFile");
                Dictionary<string, object> cardFileUserDataDict = Utils.GetOrCreateDictionary(cardFileData, playerKind);
                Dictionary<string, object> cardFileUserData = new Dictionary<string, object>();
                cardFileUserDataDict[cardFileStatus.Id.ToString()] = cardFileUserData;
                cardFileUserData["collect_card_cnt"] = cardFileStatus.Cards.Count(x => x.Value.Have && !x.Value.Dismantled);
                cardFileUserData["complete_status"] = cardFileStatus.IsComplete() ? 1 : 0;

                Dictionary<string, object> cardListData = new Dictionary<string, object>();
                foreach (UserCardFileStatusCard card in cardFileStatus.Cards.Values)
                {
                    cardListData[card.CardId.ToString()] = new Dictionary<string, object>()
                    {
                        { "page", card.Page },
                        { "card_pos", card.Pos },
                        { "premium", Math.Max(1, (int)card.Style) },
                        { "have", card.Have ? 1 : 0 },// NOTE: If "have" is more than 1 the client only shows the first card in the list
                        { "is_new", player == request.Player && card.IsNew ? 1 : 0 }
                    };
                }
                cardFileUserData["card_list"] = cardListData;

                if (player == request.Player)
                {
                    cardFileUserData["play_comp_effect"] = isNewlyComplete ? 1 : 0;// Animation showing that the card collection is completed
                    cardFileUserData["exchange_status"] = haveAnyCardsBeenDismantled;// Popup "Dismantled cards have been removed."
                    cardFileUserData["update_ms_file_status"] = cardFileStatus.HasCollectionBeenExpanded ? 1 : 0;// Popup "A new card that can be filed has been added."
                }
                cardFileUserData["sort_type"] = cardFileStatus.SortType;
                cardFileUserData["refresh_sort_flag"] = false;// Makes the client sends "CardFile.update_card_order", why would we want to do this?

                if (player == request.Player)
                {
                    Dictionary<string, object> notCollectListDataDict = Utils.GetOrCreateDictionary(cardFileData, "NotCollectList");
                    Dictionary<string, object> notCollectListData = Utils.GetOrCreateDictionary(notCollectListDataDict, cardFileStatus.Id.ToString());
                    foreach (UserCardFileStatusCard card in cardFileStatus.Cards.Values)
                    {
                        CardFileDataCard cardData;
                        if (!card.Have && cardFile.CardList.TryGetValue(card.CardId, out cardData))
                        {
                            notCollectListData[card.CardId.ToString()] = new Dictionary<string, object>()
                            {
                                { "card_id", card.CardId },
                                { "assign_premium_type", cardData.AssignPremiumType },
                            };
                        }
                    }
                }

                WriteUserProfileCardFile(request, player);

                request.Remove("CardFile." + playerKind + "." + itemId);
                if (player == request.Player)
                {
                    request.Remove("CardFile.NotCollectList." + itemId);
                    request.Remove("Craft.secret_pack_list");
                    request.Remove("Craft.open_secret_pack");
                    ClearCardFileNewStatus(player, cardFileStatus);
                }
            }
        }

        void Act_CardFileUpdateCardOrder(GameServerWebRequest request)
        {
            int itemId = Utils.GetValue<int>(request.ActParams, "item_id");
            int sortType = Utils.GetValue<int>(request.ActParams, "sort_type");
            Dictionary<string, object> cardList = Utils.GetDictionary(request.ActParams, "card_list");

            bool hasError = false;
            UserCardFileStatus cardFileStatus;
            if (request.Player.CardFiles.Files.TryGetValue(itemId, out cardFileStatus))
            {
                foreach (string cardIdStr in cardList.Keys)
                {
                    int cardId;
                    UserCardFileStatusCard card;
                    List<int> pagePos = Utils.GetIntList(cardList, cardIdStr);
                    if (pagePos.Count >= 2 && int.TryParse(cardIdStr, out cardId) && cardFileStatus.Cards.TryGetValue(cardId, out card))
                    {
                        card.Page = pagePos[0];
                        card.Pos = pagePos[1];
                    }
                    else
                    {
                        Utils.LogWarning("Failed to find " + cardIdStr + " in Collector's File " + itemId);
                        hasError = true;
                        break;
                    }
                }
                if (hasError)
                {
                    cardFileStatus.SortType = 0;
                    CardFileData cardFile;
                    if (CardFiles.TryGetValue(itemId, out cardFile))
                    {
                        foreach (CardFileDataCard card in cardFile.CardList.Values)
                        {
                            cardFileStatus.Cards[card.CardId].Pos = card.CardPos;
                            cardFileStatus.Cards[card.CardId].Page = card.Page;
                        }
                    }
                }
                else
                {
                    cardFileStatus.SortType = sortType;
                }
                request.Player.RequiresSaving = true;
            }
        }

        void WriteCardFileHave(GameServerWebRequest request)
        {
            Dictionary<string, object> cardFileStatusData = request.GetOrCreateDictionary("CardFile");
            cardFileStatusData["HaveFile"] = request.Player.CardFiles.Files.Keys.ToList();
            Dictionary<string, object> cardFileStatusNotCollectListData = Utils.GetOrCreateDictionary(cardFileStatusData, "NotCollectList");
            foreach (UserCardFileStatus cardFileStatus in request.Player.CardFiles.Files.Values)
            {
                CardFileData cardFile;
                if (CardFiles.TryGetValue(cardFileStatus.Id, out cardFile))
                {
                    Dictionary<string, object> notCollect = new Dictionary<string, object>();
                    cardFileStatusNotCollectListData[cardFileStatus.Id.ToString()] = notCollect;
                    foreach (UserCardFileStatusCard card in cardFileStatus.Cards.Values)
                    {
                        CardFileDataCard cardData;
                        if (!card.Have && cardFile.CardList.TryGetValue(card.CardId, out cardData))
                        {
                            notCollect[card.CardId.ToString()] = new Dictionary<string, object>()
                            {
                                { "card_id", card.CardId },
                                { "assign_premium_type", cardData.AssignPremiumType }
                            };
                        }
                    }
                }
            }
        }

        void WriteUserProfileCardFile(GameServerWebRequest request, Player player)
        {
            Dictionary<string, object> userData = request.GetOrCreateDictionary("User");
            Dictionary<string, object> profileData = Utils.GetOrCreateDictionary(userData, "profile");
            profileData["card_file"] = GetCardFileStatusForProfile(player);
        }

        Dictionary<string, object> GetCardFileStatusForProfile(Player player)
        {
            UserCardFileStatus cardFileStatus = null;
            if (player.CardFiles.ActiveCardFileId != 0)
            {
                player.CardFiles.Files.TryGetValue(player.CardFiles.ActiveCardFileId, out cardFileStatus);
            }
            return new Dictionary<string, object>() {
                { "item_id", cardFileStatus == null ? 0 : cardFileStatus.Id },
                { "complete_status", cardFileStatus == null || !cardFileStatus.IsComplete() ? 0 : 1 }
            };
        }

        void UpdateCardFilesStatus(Player player)
        {
            foreach (UserCardFileStatus cardFile in player.CardFiles.Files.Values)
            {
                UpdateCardFileStatus(player, cardFile);
            }
        }

        void UpdateCardFileStatus(Player player, UserCardFileStatus cardFileStatus)
        {
            CardFileData cardFile;
            if (!CardFiles.TryGetValue(cardFileStatus.Id, out cardFile))
            {
                return;
            }
            bool listModified = false;
            foreach (int cardId in new List<int>(cardFileStatus.Cards.Keys))
            {
                if (!cardFile.CardList.ContainsKey(cardId))
                {
                    cardFileStatus.Cards.Remove(cardId);
                    listModified = true;
                }
            }
            if (cardFile.CardList.Count != cardFileStatus.Cards.Count)
            {
                int oldUserStatusCount = cardFileStatus.Cards.Count;
                foreach (int cardId in cardFile.CardList.Keys)
                {
                    if (!cardFileStatus.Cards.ContainsKey(cardId))
                    {
                        cardFileStatus.Cards[cardId] = new UserCardFileStatusCard(cardId);
                    }
                }
                cardFileStatus.HasCollectionBeenExpanded = oldUserStatusCount > 0;
                listModified = true;
            }
            if (listModified)
            {
                // Need to reset the sorting order due to changes in the card list
                cardFileStatus.SortType = 0;
                foreach (CardFileDataCard card in cardFile.CardList.Values)
                {
                    cardFileStatus.Cards[card.CardId].Pos = card.CardPos;
                    cardFileStatus.Cards[card.CardId].Page = card.Page;
                }
                player.RequiresSaving = true;
            }
            foreach (int cardId in cardFile.CardList.Keys)
            {
                player.UpdateCardFileStatusForCardId(cardId);
            }
        }

        void ClearCardFileNewStatus(Player player, UserCardFileStatus cardFileStatus)
        {
            foreach (UserCardFileStatusCard card in cardFileStatus.Cards.Values)
            {
                if (card.Dismantled)
                {
                    card.Dismantled = false;
                    player.RequiresSaving = true;
                }
                if (card.IsNew)
                {
                    card.IsNew = false;
                    player.RequiresSaving = true;
                }
            }
            if (cardFileStatus.HasCollectionBeenExpanded)
            {
                cardFileStatus.HasCollectionBeenExpanded = false;
                player.RequiresSaving = true;
            }
        }
    }
}
