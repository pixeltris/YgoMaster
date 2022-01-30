using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        // NOTE: ResultCode doesn't seem to be working here...

        void Act_CraftExchangeMulti(GameServerWebRequest request, bool isCraft = false)
        {
            Dictionary<string, object> cardList = GetDictionary(request.ActParams, "card_list");
            if (cardList != null)
            {
                Dictionary<int, Dictionary<CardStyleRarity, int>> cards = new Dictionary<int, Dictionary<CardStyleRarity, int>>();
                PlayerCraftPoints points = new PlayerCraftPoints();
                PlayerCraftPoints totalPoints = new PlayerCraftPoints();
                totalPoints.Add(request.Player.CraftPoints);
                HashSet<int> foundSecrets = new HashSet<int>();// Hashset of shop ids
                HashSet<int> foundSecretsExtend = new HashSet<int>();
                foreach (KeyValuePair<string, object> item in cardList)
                {
                    Dictionary<string, object> numObj = item.Value as Dictionary<string, object>;
                    int cardId;
                    if (int.TryParse(item.Key, out cardId) && numObj != null)
                    {
                        cards[cardId] = new Dictionary<CardStyleRarity, int>()
                        {
                            { CardStyleRarity.Normal, GetValue<int>(numObj, "num") },
                            { CardStyleRarity.Shine, GetValue<int>(numObj, "p1_num") },
                            { CardStyleRarity.Royal, GetValue<int>(numObj, "p2_num") }
                        };
                        foreach (KeyValuePair<CardStyleRarity, int> styleRarityNum in cards[cardId])
                        {
                            if (!isCraft && styleRarityNum.Value > request.Player.Cards.GetCount(cardId, PlayerCardKind.Dismantle, styleRarityNum.Key))
                            {
                                LogWarning("Lacking the desired amount of cards for dismantle");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
                            int rarityVal;
                            if (!CardRare.TryGetValue(cardId, out rarityVal))
                            {
                                LogWarning("Couldn't find card rarity for craft/dismantle");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
                            CardRarity rarity = (CardRarity)rarityVal;
                            int pointCount = 0;
                            if (isCraft)
                            {
                                pointCount = Craft.GetCraftRequirement(rarity, styleRarityNum.Key);
                            }
                            else
                            {
                                pointCount = Craft.GetDismantleReward(rarity, styleRarityNum.Key);
                            }
                            if (isCraft && !totalPoints.CanSubtract(rarity, styleRarityNum.Value * pointCount))
                            {
                                LogWarning("Overflow of craft points when crafting");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
                            else if (!isCraft && !totalPoints.CanAdd(rarity, styleRarityNum.Value * pointCount))
                            {
                                LogWarning("Overflow of craft points when dismantling");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
                            else
                            {
                                if (isCraft)
                                {
                                    totalPoints.Subtract(rarity, styleRarityNum.Value * pointCount);
                                    List<ShopItemInfo> secretPacks;
                                    if (rarity >= CardRarity.SuperRare && Shop.SecretPacksByCardId.TryGetValue(cardId, out secretPacks))
                                    {
                                        foreach (ShopItemInfo secretPack in secretPacks)
                                        {
                                            switch (secretPack.SecretType)
                                            {
                                                case ShopItemSecretType.FindOrCraft:
                                                case ShopItemSecretType.Craft:
                                                    if (request.Player.ShopState.GetAvailability(secretPack) == PlayerShopItemAvailability.Available)
                                                    {
                                                        foundSecretsExtend.Add(secretPack.ShopId);
                                                    }
                                                    else
                                                    {
                                                        foundSecrets.Add(secretPack.ShopId);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    totalPoints.Add(rarity, styleRarityNum.Value * pointCount);
                                }
                                points.Add(rarity, styleRarityNum.Value * pointCount);
                            }
                        }
                    }
                }
                foreach (KeyValuePair<int, Dictionary<CardStyleRarity, int>> card in cards)
                {
                    foreach (KeyValuePair<CardStyleRarity, int> styleCount in card.Value)
                    {
                        if (isCraft)
                        {
                            request.Player.Cards.Add(card.Key, styleCount.Value, PlayerCardKind.Dismantle, styleCount.Key);
                        }
                        else
                        {
                            request.Player.Cards.Subtract(card.Key, styleCount.Value, PlayerCardKind.Dismantle, styleCount.Key);
                        }
                    }
                }
                List<Dictionary<string, object>> secretPackList = new List<Dictionary<string, object>>();
                if (isCraft)
                {
                    request.Player.CraftPoints.Subtract(points);
                    if (foundSecrets.Count > 0 || foundSecretsExtend.Count > 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            bool isNewSecret = i == 0;
                            HashSet<int> shopIds = isNewSecret ? foundSecrets : foundSecretsExtend;
                            foreach (int shopId in shopIds)
                            {
                                ShopItemInfo secretPack;
                                if (Shop.AllShops.TryGetValue(shopId, out secretPack))
                                {
                                    if (isNewSecret)
                                    {
                                        request.Player.ShopState.New(secretPack);
                                    }
                                    request.Player.ShopState.Unlock(secretPack);
                                    secretPackList.Add(new Dictionary<string, object>()
                                    {
                                        { "nameTextId", FixIdString(secretPack.NameText) },
                                        { "shopId", secretPack.ShopId },
                                        { "iconMrk", secretPack.IconMrk },
                                        { "is_extend", !isNewSecret },
                                        //{ "free_num", 1 }// "1 Free Pull" (this is also the only way to show a "!" mark)
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    request.Player.CraftPoints.Add(points);
                }
                SavePlayer(request.Player);
                request.Response["Item"] = new Dictionary<string, object>()
                {
                    { "have", request.Player.CraftPoints.ToDictionary() },
                };
                request.Response["Craft"] = new Dictionary<string, object>()
                {
                    { "point", request.Player.CraftPoints.ToDictionary() },
                    { "secret_pack_list", secretPackList },
                    { "is_send_present_box", false }
                };
                WriteCards_have(request, new HashSet<int>(cards.Keys));
            }
        }

        void Act_CraftGenerateMulti(GameServerWebRequest request)
        {
            Act_CraftExchangeMulti(request, true);
        }
    }
}
