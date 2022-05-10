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
            Dictionary<string, object> cardList = Utils.GetDictionary(request.ActParams, "card_list");
            if (cardList != null)
            {
                Dictionary<int, int> cardRare = GetCardRarities(request.Player);
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
                            { CardStyleRarity.Normal, Utils.GetValue<int>(numObj, "num") },
                            { CardStyleRarity.Shine, Utils.GetValue<int>(numObj, "p1_num") },
                            { CardStyleRarity.Royal, Utils.GetValue<int>(numObj, "p2_num") }
                        };
                        if (isCraft && (cards[cardId][CardStyleRarity.Shine] > 0 || cards[cardId][CardStyleRarity.Royal] > 0))
                        {
                            Utils.LogWarning("Craft request with specific style (shine:" +
                                cards[cardId][CardStyleRarity.Shine] + ", royal:" + cards[cardId][CardStyleRarity.Royal] + ")");
                            cards[cardId][CardStyleRarity.Shine] = 0;
                            cards[cardId][CardStyleRarity.Royal] = 0;
                        }
                        
                        foreach (KeyValuePair<CardStyleRarity, int> styleRarityNum in cards[cardId])
                        {
                            if (!isCraft && styleRarityNum.Value > request.Player.Cards.GetCount(cardId, PlayerCardKind.Dismantle, styleRarityNum.Key))
                            {
                                Utils.LogWarning("Lacking the desired amount of cards for dismantle");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
                            CardRarity rarity;
                            if (!TryGetCardRarity(cardId, cardRare, out rarity) && !TryGetCardRarity(cardId, CardRare, out rarity))
                            {
                                Utils.LogWarning("Couldn't find card " + cardId + " rarity for craft/dismantle");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
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
                                Utils.LogWarning("Overflow of craft points when crafting");
                                request.ResultCode = (int)ResultCodes.CraftCode.ERROR_UPDATE_FAILED;
                                return;
                            }
                            else if (!isCraft && !totalPoints.CanAdd(rarity, styleRarityNum.Value * pointCount))
                            {
                                Utils.LogWarning("Overflow of craft points when dismantling");
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
                                                    if (request.Player.ShopState.GetAvailability(Shop, secretPack) == PlayerShopItemAvailability.Available)
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
                            // The original code didn't take into account upgrading of card style rarity. This is hacked in after the fact
                            int rarity;
                            if (styleCount.Key == CardStyleRarity.Normal && cardRare.TryGetValue(card.Key, out rarity) &&
                                Craft.CraftStyleRarityRates.ContainsKey((CardRarity)rarity))
                            {
                                for (int i = 0; i < styleCount.Value; i++)
                                {
                                    Dictionary<CardStyleRarity, double> styleRarityAccumaltiveRate = new Dictionary<CardStyleRarity, double>();
                                    double styleRarityTotalPercent = 0;
                                    foreach (KeyValuePair<CardStyleRarity, double> rate in Craft.CraftStyleRarityRates[(CardRarity)rarity].OrderBy(x => x.Key))
                                    {
                                        if (rate.Value <= 0)
                                        {
                                            continue;
                                        }
                                        styleRarityTotalPercent += rate.Value;
                                        styleRarityAccumaltiveRate[rate.Key] = styleRarityTotalPercent;
                                    }
                                    CardStyleRarity styleRarity = styleCount.Key;
                                    if (styleRarityAccumaltiveRate.Count > 0)
                                    {
                                        double styleRarityPercent = rand.NextDouble() * 100;
                                        foreach (KeyValuePair<CardStyleRarity, double> rate in styleRarityAccumaltiveRate.OrderBy(x => x.Key))
                                        {
                                            if (styleRarityPercent < rate.Value)
                                            {
                                                styleRarity = rate.Key;
                                                break;
                                            }
                                        }
                                    }
                                    request.Player.Cards.Add(card.Key, 1, PlayerCardKind.Dismantle, styleRarity);
                                }
                            }
                            else
                            {
                                request.Player.Cards.Add(card.Key, styleCount.Value, PlayerCardKind.Dismantle, styleCount.Key);
                            }
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
                                        { "nameTextId", Utils.FixIdString(secretPack.NameText) },
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
                foreach (CardRarity rarity in Enum.GetValues(typeof(CardRarity)))
                {
                    CraftPointRollover rollover;
                    if (Craft.Rollover.TryGetValue(rarity, out rollover) && rollover.At > 0 && rollover.Take > 0 && rollover.Give > 0)
                    {
                        // TODO: Maybe do this without the loop?
                        while (true)
                        {
                            int num = request.Player.CraftPoints.Get(rarity);
                            if (num >= rollover.At)
                            {
                                request.Player.CraftPoints.Subtract(rarity, rollover.Take);
                                request.Player.CraftPoints.Add(rarity + 1, rollover.Give);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
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

        void Act_CraftGetCardRoute(GameServerWebRequest request)
        {
            // Only taking into account shop structure decks and packs for now
            // TODO: solo rewards / starting decks
            int cardId;
            if (Utils.TryGetValue(request.ActParams, "card_id", out cardId))
            {
                List<object> routes = new List<object>();
                for (int i = 0; i < 2; i++)
                {
                    IEnumerable<ShopItemInfo> shopItems = (i == 0 ? Shop.PackShop.Values : Shop.StructureShop.Values);
                    HowToObtainCard howToObtain = i == 0 ? HowToObtainCard.Pack : HowToObtainCard.SalesStructure;
                    foreach (ShopItemInfo shopItem in shopItems)
                    {
                        if (!shopItem.Cards.ContainsKey(cardId))
                        {
                            continue;
                        }
                        bool isRouteOpen = true;
                        switch (shopItem.SecretType)
                        {
                            case ShopItemSecretType.Find:
                            case ShopItemSecretType.FindOrCraft:
                            case ShopItemSecretType.Other:
                                isRouteOpen = request.Player.ShopState.GetAvailability(Shop, shopItem) != PlayerShopItemAvailability.Hidden;
                                break;
                            case ShopItemSecretType.None:
                                break;
                            default:
                                continue;
                        }
                        routes.Add(new Dictionary<string, object>()
                        {
                            { "route_category", (int)howToObtain },
                            { "route_param", shopItem.ShopId },
                            { "route_open", isRouteOpen },
                            { "route_name_id", Utils.FixIdString(shopItem.NameText) },
                            { "route_icon_type", (int)shopItem.IconType },
                            { "route_icon_data", shopItem.IconData },
                            { "route_icon_mrk", shopItem.IconMrk }
                        });
                    }
                }
                foreach (int itemId in DefaultItems)
                {
                    ItemID.Category category = ItemID.GetCategoryFromID(itemId);
                    if (category == ItemID.Category.STRUCTURE)
                    {
                        Act_CraftGetCardRoute_TryAddStructureDeck(request, routes, cardId, itemId, HowToObtainCard.InitialDistributionStructure);
                    }
                }
                Dictionary<int, Dictionary<string, object>> soloAllRewardData = Utils.GetIntDictDict(SoloData, "reward");
                if (soloAllRewardData != null)
                {
                    foreach (KeyValuePair<int, Dictionary<string, object>> soloRewardData in soloAllRewardData)
                    {
                        foreach (KeyValuePair<string, object> reward in soloRewardData.Value)
                        {
                            Dictionary<string, object> rewardItems = reward.Value as Dictionary<string, object>;
                            foreach (KeyValuePair<string, object> item in rewardItems)
                            {
                                int itemId;
                                int count = (int)Convert.ChangeType(item.Value, typeof(int));
                                if (int.TryParse(item.Key, out itemId))
                                {
                                    ItemID.Category category = ItemID.GetCategoryFromID(itemId);
                                    if (category == ItemID.Category.CARD)
                                    {
                                        if (itemId == cardId)
                                        {
                                            // TODO: Fetch which gates contain this card (will be messy getting that info here...)
                                            routes.Add(new Dictionary<string, object>()
                                            {
                                                { "route_category", (int)HowToObtainCard.Solo },
                                                { "route_param", 1 },// Gate id (required) determines which image to display
                                                { "route_open", true },// TODO: Check unlocked status
                                                { "route_name_id", string.Empty },//, "IDS_SOLO_GATE001" }, <--- removed to display no text
                                                { "route_icon_type", 0 },
                                                { "route_icon_data", string.Empty },
                                                { "route_icon_mrk", 0 }
                                            });
                                        }
                                    }
                                    else if (category == ItemID.Category.STRUCTURE)
                                    {
                                        Act_CraftGetCardRoute_TryAddStructureDeck(request, routes, cardId, itemId, HowToObtainCard.Solo);
                                    }
                                }
                            }
                        }
                    }
                }
                if (routes.Count > 0)
                {
                    request.Response["Route"] = routes;
                }
            }
        }

        void Act_CraftGetCardRoute_TryAddStructureDeck(GameServerWebRequest request, List<object> routes, int cardId, int structureId, HowToObtainCard route)
        {
            DeckInfo deck;
            if (StructureDecks.TryGetValue(structureId, out deck))
            {
                if (deck.GetAllCards().Contains(cardId))
                {
                    routes.Add(new Dictionary<string, object>()
                    {
                        { "route_category", (int)route },
                        { "route_param", structureId },
                        { "route_open", true },
                        { "route_name_id", "IDS_ITEM_ID" + structureId },
                        { "route_icon_type", 0 },
                        { "route_icon_data", string.Empty },
                        { "route_icon_mrk", 0 }
                    });
                }
            }
        }
    }
}
