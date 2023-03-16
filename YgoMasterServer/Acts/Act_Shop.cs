using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        const int shopExtraGuaranteePackCount = 10;

        // TODO: Put this somewhere else (called by Act_ShopPurchase / SoloUpdateChapterStatus / GiveDuelReward)
        bool GiveStructureDeck(GameServerWebRequest request, int structureDeckId)
        {
            /*if (request.Player.Items.Contains(structureDeckId))
            {
                return false;
            }*/
            request.Player.Items.Add(structureDeckId);
            DeckInfo deck;
            if (!StructureDecks.TryGetValue(structureDeckId, out deck))
            {
                return false;
            }
            List<int> cards = deck.GetAllCards();
            HashSet<int> uniqueCardIds = new HashSet<int>();
            foreach (int cardId in cards)
            {
                uniqueCardIds.Add(cardId);
                request.Player.Cards.Add(cardId, 1, DisableNoDismantle ? PlayerCardKind.Dismantle : PlayerCardKind.NoDismantle, CardStyleRarity.Normal);
            }
            Dictionary<string, object> itemsData = request.GetOrCreateDictionary("Item");
            Dictionary<string, object> itemsHave = Utils.GetOrCreateDictionary(itemsData, "have");
            itemsHave[structureDeckId.ToString()] = 1;
            int[] accessoryItemIds =
            {
                deck.Accessory.Box,
                deck.Accessory.Sleeve
            };
            foreach (int itemId in accessoryItemIds)
            {
                if (itemId != 0 && request.Player.Items.Add(itemId))
                {
                    itemsHave[itemId.ToString()] = 1;
                }
            }
            WriteCards_have(request, uniqueCardIds);
            return true;
        }

        void Act_ShopGetList(GameServerWebRequest request, int targetShopId = 0)
        {
            Dictionary<string, object> packShop = new Dictionary<string, object>();
            Dictionary<string, object> structureShop = new Dictionary<string, object>();
            Dictionary<string, object> accessoryShop = new Dictionary<string, object>();
            Dictionary<string, object> specialShop = new Dictionary<string, object>();
            ShopItemInfo targetShop = null;
            foreach (ShopItemInfo shopItem in Shop.AllShops.Values)
            {
                if (targetShopId != 0)
                {
                    if (shopItem.ShopId == targetShopId)
                    {
                        targetShop = shopItem;
                    }
                    else
                    {
                        continue;
                    }
                }
                long expireTime;
                long packBuyLimit;
                bool isNew;
                if (request.Player.ShopState.GetAvailability(Shop, shopItem, out packBuyLimit, out expireTime, out isNew) == PlayerShopItemAvailability.Hidden)
                {
                    continue;
                }
                int buyLimit = 0;
                int have = 0;
                Dictionary<string, object> data = new Dictionary<string, object>();
                switch (shopItem.Category)
                {
                    case ShopCategory.Pack:
                        if (packBuyLimit == 0 && shopItem.Buylimit > 0)
                        {
                            buyLimit = 1;
                            have = 1;
                        }
                        int normalCardListId = shopItem.ShopId;
                        int pickupCardListId = 0;
                        ShopOddsInfo odds = shopItem.GetOdds(Shop);
                        if (odds != null)
                        {
                            if (odds.CardRateList.FirstOrDefault(x => x.Standard) != null)
                            {
                                normalCardListId = Shop.StandardPack.ShopId;
                                pickupCardListId = shopItem.ShopId;
                            }
                        }
                        
                        int numCardsObtained;
                        double percentComplete = shopItem.GetPercentComplete(request.Player, out numCardsObtained);
                        double percentToNextPack = shopItem.UnlockSecretsAtPercent - percentComplete;
                        Func<string, string> updateShopText = (string str) =>
                        {
                            if (string.IsNullOrEmpty(str))
                            {
                                return str;
                            }
                            str = str.Replace("{BUYS_REMAIN}", packBuyLimit.ToString());
                            str = str.Replace("{CARDS_OBTAINED}", numCardsObtained.ToString());
                            str = str.Replace("{CARDS}", shopItem.Cards.Keys.Count.ToString());
                            str = str.Replace("{PERCENT_COMPLETE}", percentComplete.ToString("N0"));
                            str = str.Replace("{PERCENT_COMPLETE_N1}", percentComplete.ToString("N1"));
                            str = str.Replace("{PERCENT_COMPLETE_N2}", percentComplete.ToString("N2"));
                            str = str.Replace("{PERCENT_TO_PACK}", percentToNextPack.ToString("N0"));
                            str = str.Replace("{PERCENT_TO_PACK_N1}", percentToNextPack.ToString("N1"));
                            str = str.Replace("{PERCENT_TO_PACK_N2}", percentToNextPack.ToString("N2"));
                            return Utils.FixIdString(str);
                        };
                        
                        packShop[shopItem.ShopId.ToString()] = data;
                        data["packId"] = shopItem.Id;
                        data["packType"] = (int)shopItem.PackType;
                        data["nameTextId"] = updateShopText(shopItem.NameText);
                        if (shopItem.DescTextGenerated)
                        {
                            List<string> desc = new List<string>();
                            if (shopItem.ReleaseDate != default(DateTime))
                            {
                                desc.Add(shopItem.ReleaseDate.ToString("MMMM dd yyyy"));
                            }
                            //desc.Add(shopItem.Cards.Count + " cards");
                            //desc.Add(numCardsObtained + " owned");
                            desc.Add(numCardsObtained + " / " + shopItem.Cards.Count + " owned");
                            if (shopItem.UnlockSecrets.Count > 0 && percentComplete < shopItem.UnlockSecretsAtPercent &&
                                !shopItem.HasUnlockedAllSecrets(request.Player, Shop))
                            {
                                desc.Add((shopItem.UnlockSecretsAtPercent - percentComplete).ToString("N1") + "% left until the next pack");
                            }
                            string descStr = Utils.FixIdString(string.Join("\n", desc));
                            data["descShortTextId"] = descStr;
                            data["descFullTextId"] = descStr;
                        }
                        else
                        {
                            data["descShortTextId"] = updateShopText(shopItem.DescShortText);
                            data["descFullTextId"] = updateShopText(shopItem.DescFullText);
                        }
                        data["power"] = shopItem.Power;
                        data["flexibility"] = shopItem.Flexibility;
                        data["difficulty"] = shopItem.Difficulty;
                        //data["foundTime"]data["limitTime"] <--- these might potentially change the sort order, but no other impact
                        data["pack_card_num"] = shopItem.CardNum;
                        data["normalCardListId"] = normalCardListId;
                        data["pickupCardListId"] = pickupCardListId;
                        data["isFinalizedUR"] = request.Player.ShopState.IsUltraRareGuaranteed(shopItem.Id);
                        data["isSpecialTime"] = shopItem.IsSpecialTime;
                        break;
                    case ShopCategory.Structure:
                        structureShop[shopItem.ShopId.ToString()] = data;
                        buyLimit = shopItem.Buylimit;
                        have = Math.Min(shopItem.Buylimit, (int)request.Player.ShopState.GetPurchasedCount(shopItem));
                        DeckInfo deck = StructureDecks[shopItem.Id];
                        data["structure_id"] = deck.Id;
                        data["accessory"] = deck.Accessory.ToDictionary();
                        data["focus"] = deck.DisplayCards.ToDictionary();
                        data["contents"] = deck.ToDictionary();
                        break;
                    case ShopCategory.Accessory:
                        accessoryShop[shopItem.ShopId.ToString()] = data;
                        buyLimit = 1;
                        have = request.Player.Items.Contains(shopItem.Id) ? 1 : 0;
                        data["itemId"] = shopItem.Id;
                        data["item_id"] = shopItem.Id;
                        data["item_category"] = (int)ItemID.GetCategoryFromID(shopItem.Id);
                        data["is_period"] = false;//?
                        data["max"] = buyLimit;
                        data["have"] = have;
                        break;
                    case ShopCategory.Special:
                        specialShop[shopItem.ShopId.ToString()] = data;
                        // TODO (same as Pack but also has "setItems" / "isSPProb")
                        continue;
                }
                data["shopId"] = shopItem.ShopId;
                data["category"] = (int)shopItem.Category;
                data["subCategory"] = shopItem.SubCategory;
                data["iconMrk"] = shopItem.IconMrk;
                data["iconType"] = (int)shopItem.IconType;
                if (!string.IsNullOrEmpty(shopItem.IconData))
                {
                    data["iconData"] = shopItem.IconData;
                }
                if (!string.IsNullOrEmpty(shopItem.Preview))
                {
                    // Prior to v1.4.1 this was a string in many cases. It's no longer a string (shop fails to load)
                    data["preview"] = MiniJSON.Json.Deserialize(shopItem.Preview);
                }
                data["searchCategory"] = shopItem.SearchCategory.ToArray();
                data["limitdate_ts"] = expireTime;
                data["limit_buy_count"] = buyLimit;
                data["now_buy_count"] = have;
                data["list_button_type"] = (int)ShopItemListButtonType.Default;
                data["confirm_text_id"] = new string[0];// Custom text for the "Purchase Confirmation" popup (entries are line breaked)
                data["isNew"] = isNew;

                Dictionary<string, object> prices = new Dictionary<string,object>();
                foreach (ShopItemPrice price in shopItem.Prices)
                {
                    string textId = null;
                    string pop = null;
                    ShopItemPriceButtonType buttonType = ShopItemPriceButtonType.Blue;
                    if (shopItem.PackType != ShopPackType.None)
                    {
                        textId = price.ItemAmount <= 1 ? "IDS_SHOP_BUY_BUTTON_CARDPACK_SINGLE" : "IDS_SHOP_BUY_BUTTON_CARDPACK_MULTI";
                        if (price.ItemAmount > 1)
                        {
                            if (packBuyLimit > 0 && price.ItemAmount > packBuyLimit)
                            {
                                // Not enough funds to buy the pack
                                // TODO: Find how to disable the button instead of hiding it?
                                // TODO: Support downgrading a 10x purchase to a smaller pack size (with correctly adjusted pricing)
                                continue;
                            }
                            if (price.ItemAmount == shopExtraGuaranteePackCount)
                            {
                                pop = "IDS_SHOP_BUY_BUTTON_ADS_CARDPACK_10_SR";// "At least 1 Super Rare guaranteed in 10 Packs"
                                buttonType = ShopItemPriceButtonType.Yellow;
                                if (request.Player.ShopState.IsUltraRareGuaranteed(shopItem.Id))
                                {
                                    pop = "IDS_SHOP_BUY_BUTTON_ADS_CARDPACK_10_UR";// "Ultra Rare guaranteed in 10 Packs"
                                    buttonType = ShopItemPriceButtonType.Pink;
                                }
                            }
                        }
                        ShopOddsInfo odds = shopItem.GetOdds(Shop);
                        if ((price.ItemAmount > 1 || shopItem.Prices.Count == 1) &&
                            odds != null && shopItem.GetFinalCardRarityGuarantee(odds) == CardRarity.SuperRare)
                        {
                            // This is when a single pack contains a SR (and there's no multi buy option)
                            pop = "IDS_SHOP_BUY_BUTTON_ADS_CARDPACK_10_SR_SET";// "At least 1 SR guaranteed"
                        }
                    }
                    // NOTE: If you send "IDS_SHOP_BUY_BUTTON_CARDPACK_MULTI" with no "textArgs" the shops will break
                    Dictionary<string, object> priceData = new Dictionary<string, object>()
                    {
                        { "price_id", price.Id },
                        { "item_category", 1 },
                        { "item_id", 1 },
                        { "button_type", (int)buttonType },
                        { "use_item_num", price.Price },
                        { "buy_count", 1 },
                        { "sort", price.Id },
                        { "confirm_reg_id", 0 },// Added v1.1.1
                        //"free_num":1 <--- used for "1 Free Pull" (also need "use_item_num" set to 0)
                    };
                    if (!string.IsNullOrEmpty(textId))
                    {
                        priceData["textId"] = textId;
                        priceData["textArgs"] = price.ItemAmount > 1 ? new int[] { price.ItemAmount } : new int[0];
                    }
                    if (!string.IsNullOrEmpty(pop))
                    {
                        priceData["POP"] = Utils.FixIdString(pop);
                    }
                    prices[price.Id.ToString()] = priceData;
                }
                data["prices"] = prices;

                Dictionary<string, object> cardList = Utils.GetOrCreateDictionary(request.GetOrCreateDictionary("Gacha"), "cardList");
                if (!Shop.PerPackRarities)
                {
                    switch (shopItem.PackType)
                    {
                        case ShopPackType.Standard:
                        case ShopPackType.Bonus:
                            // These are fetched via "Gacha.get_card_list"
                            break;
                        default:
                            cardList[shopItem.ShopId.ToString()] = shopItem.Cards.Keys.ToArray();
                            break;
                    }
                }
            }
            if (targetShopId > 0)
            {
                if (targetShop == null)
                {
                    return;
                }
                Dictionary<string, object> shopData = request.GetOrCreateDictionary("Shop");
                string categoryStr = null;
                Dictionary<string, object> itemData = null;
                switch (targetShop.Category)
                {
                    case ShopCategory.Pack:
                        categoryStr = "PackShop";
                        itemData = packShop;
                        break;
                    case ShopCategory.Structure:
                        categoryStr = "StructureShop";
                        itemData = structureShop;
                        break;
                    case ShopCategory.Accessory:
                        categoryStr = "AccessoryShop";
                        itemData = accessoryShop;
                        break;
                    case ShopCategory.Special:
                        categoryStr = "specialShop";
                        itemData = specialShop;
                        break;
                }
                if (itemData != null)
                {
                    Utils.GetOrCreateDictionary(shopData, categoryStr)[itemData.First().Key] = itemData.First().Value;
                }
            }
            else
            {
                request.Response["Shop"] = new Dictionary<string, object>()
                {
                    { "PackShop", packShop },
                    { "StructureShop", structureShop },
                    { "AccessoryShop", accessoryShop },
                    { "SpecialShop", specialShop },
                };
                request.Remove("Shop");
                if (request.Player.ShopState.ClearNew())
                {
                    SavePlayer(request.Player);
                }
            }
        }

        void Act_ShopPurchase(GameServerWebRequest request)
        {
            request.Player.ShopState.ClearNew();
            int shopId = Utils.GetValue<int>(request.ActParams, "shop_id");
            int priceId = Utils.GetValue<int>(request.ActParams, "price_id");
            int count = Utils.GetValue<int>(request.ActParams, "count");
            bool success = false;
            ShopItemInfo shopItem = null;
            ShopItemPrice price;
            if (shopId > 0 && priceId > 0 && count == 1 && Shop.AllShops.TryGetValue(shopId, out shopItem) &&
                (price = shopItem.Prices.FirstOrDefault(x => x.Id == priceId)) != null)
            {
                if (request.Player.ShopState.GetAvailability(Shop, shopItem) != PlayerShopItemAvailability.Available)
                {
                    request.ResultCode = (int)ResultCodes.ShopCode.OUT_OF_TERM;
                    request.Remove("Shop.PackShop." + shopItem.ShopId.ToString());
                    request.Remove("Gacha.drawPackInfo", "Gacha.effects", "Gacha.info", "Operation.Dialog");
                    return;
                }
                if (request.Player.Gems < price.Price)
                {
                    Utils.LogWarning("Lacking funds to purchase item " + shopItem.Id);
                }
                else
                {
                    switch (shopItem.Category)
                    {
                        case ShopCategory.Pack:
                            success = Act_ShopPurchase_Pack(request, price, shopItem);
                            break;
                        case ShopCategory.Structure:
                            if ((int)request.Player.ShopState.GetPurchasedCount(shopItem) >= shopItem.Buylimit)
                            {
                                Utils.LogWarning("Tried to re-purchase owned structure deck " + shopItem.Id);
                            }
                            else
                            {
                                DeckInfo deck;
                                if (!StructureDecks.TryGetValue(shopItem.Id, out deck))
                                {
                                    Utils.LogWarning("Failed to find structure deck " + shopItem.Id);
                                }
                                else
                                {
                                    success = true;
                                    request.Player.Gems -= price.Price;
                                    WriteItem(request, (int)ItemID.Value.Gem);
                                    GiveStructureDeck(request, shopItem.Id);
                                    request.Response["Shop"] = new Dictionary<string, object>()
                                    {
                                        { "StructureShop", new Dictionary<string, object>() {
                                            { shopItem.ShopId.ToString(), new Dictionary<string, object>() {
                                                { "now_buy_count", (int)request.Player.ShopState.GetPurchasedCount(shopItem) + 1 }
                                            }}
                                        }}
                                    };
                                }
                            }
                            break;
                        case ShopCategory.Accessory:
                            if (request.Player.Items.Contains(shopItem.Id))
                            {
                                Utils.LogWarning("Tried to re-purchase owned item " + shopItem.Id);
                            }
                            else
                            {
                                success = true;
                                request.Player.Gems -= price.Price;
                                request.Player.Items.Add(shopItem.Id);
                                Dictionary<string, object> have = new Dictionary<string, object>();
                                have[shopItem.Id.ToString()] = 1;
                                have[((int)ItemID.Value.Gem).ToString()] = request.Player.Gems;
                                foreach (int fieldPartItemId in ItemID.GetDuelFieldParts(shopItem.Id))
                                {
                                    request.Player.Items.Add(fieldPartItemId);
                                    have[fieldPartItemId.ToString()] = 1;
                                }
                                request.Response["Item"] = new Dictionary<string, object>()
                                {
                                    { "have", have }
                                };
                                request.Response["Shop"] = new Dictionary<string, object>()
                                {
                                    { "AccessoryShop", new Dictionary<string, object>() {
                                        { shopItem.ShopId.ToString(), new Dictionary<string, object>() {
                                            { "now_buy_count", 1 }
                                        }}
                                    }}
                                };
                            }
                            break;
                        case ShopCategory.Special:
                            Utils.LogInfo("TODO: Handle special shop category");
                            break;
                    }
                }
            }
            if (success)
            {
                if (shopItem != null)
                {
                    request.Player.ShopState.Purchased(shopItem);
                    if (shopItem.SetPurchased.Count > 0)
                    {
                        foreach (int otherShopItemId in shopItem.SetPurchased)
                        {
                            ShopItemInfo otherShopItem;
                            if (Shop.AllShops.TryGetValue(otherShopItemId, out otherShopItem))
                            {
                                request.Player.ShopState.Purchased(otherShopItem);
                                if (request.Player.ShopState.GetAvailability(Shop, otherShopItem) == PlayerShopItemAvailability.Hidden)
                                {
                                    request.Remove("Shop." + shopItem.Category + "Shop." + shopItem.ShopId.ToString());
                                }
                                else
                                {
                                    Act_ShopGetList(request, otherShopItemId);
                                }
                            }
                        }
                    }
                    if (request.Player.ShopState.GetAvailability(Shop, shopItem) == PlayerShopItemAvailability.Hidden)
                    {
                        request.Remove("Shop." + shopItem.Category + "Shop." + shopItem.ShopId.ToString());
                    }
                    else if (shopItem.Buylimit > 0 || shopItem.SecretBuyLimit > 0)
                    {
                        Act_ShopGetList(request, shopItem.ShopId);
                    }
                }
                request.Player.ShopState.ClearNew();// Some are set to new temporarily above, it can be cleared now
                SavePlayer(request.Player);
            }
            else
            {
                request.ResultCode = (int)ResultCodes.ShopCode.PROCESSING_FAILED;
                Utils.LogWarning("Shop purchase failed!");
            }
            request.Remove("Gacha.drawPackInfo", "Gacha.effects", "Gacha.info");
        }

        bool Act_ShopPurchase_Pack(GameServerWebRequest request, ShopItemPrice price, ShopItemInfo shopItem)
        {
            // NOTE: Anything above 10 packs makes the client bug out
            int packCount = Math.Min(10, Math.Max(1, price.ItemAmount));
            ShopOddsInfo odds = shopItem.GetOdds(Shop);
            if (odds == null)
            {
                Utils.LogWarning("Failed to find pack odds for " + shopItem.Id);
                return false;
            }
            else
            {
                request.Player.Gems -= price.Price;

                Dictionary<int, int> standardPackCardRare = GetCardRarities(request.Player, Shop.StandardPack);
                Dictionary<int, int> packCardRare = GetCardRarities(request.Player, shopItem);
                if (Shop.PerPackRarities)
                {
                    // TODO: Also need to write standard pack if this pack is a secret pack (pickup)
                    WritePerPackRarities(request, packCardRare);
                }

                // 1 = Blue smoke
                // 2 = Orange smoke
                // 3 = A lot of orange smoke
                int smokeType = rand.Next(100) < 15 ? 2 : 1;
                if (packCount == shopExtraGuaranteePackCount)
                {
                    smokeType = 3;
                }

                HashSet<int> foundSecrets = new HashSet<int>();// Hashset of shop ids
                HashSet<int> cardIdsToUpdate = new HashSet<int>();
                HashSet<int> newCardIds = new HashSet<int>();
                Dictionary<ShopItemInfo, List<Dictionary<string, object>>> packInfos = new Dictionary<ShopItemInfo, List<Dictionary<string, object>>>();
                HashSet<ShopItemInfo> pickupPacks = new HashSet<ShopItemInfo>();
                bool showSecretFoundResult = false;
                bool pulledUltraRare = false;
                bool isUltraRareGuaranteed = request.Player.ShopState.IsUltraRareGuaranteed(shopItem.Id);
                Dictionary<CardRarity, int> numCardsByRarity = shopItem.GetNumCardsByRarity(packCardRare);
                for (int packIndex = 0; packIndex < packCount; packIndex++)
                {
                    HashSet<int> seenCardIdsThisPack = new HashSet<int>();
                    CardRarity highestCardBackRarity = CardRarity.Normal;
                    CardRarity highestCardRarity = CardRarity.Normal;
                    List<object> cardInfos = new List<object>();
                    List<ShopOddsRarity> matches = new List<ShopOddsRarity>();
                    CardRarity rarityGuarantee = CardRarity.UltraRare;
                    // Find the lowest possible rarity of the pack
                    foreach (int cardId in shopItem.Cards.Keys)
                    {
                        CardRarity rarity;
                        if (TryGetCardRarity(cardId, packCardRare, out rarity))
                        {
                            if (rarity < rarityGuarantee)
                            {
                                rarityGuarantee = rarity;
                                if (rarityGuarantee == CardRarity.Normal)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    for (int cardIndex = 0; cardIndex < shopItem.CardNum; cardIndex++)
                    {
                        // TODO: Improve the code which determines which odds to use as it currently isn't super flexible
                        if (cardIndex == shopItem.CardNum - 1)
                        {
                            if (CardRarity.Rare > rarityGuarantee)
                            {
                                rarityGuarantee = CardRarity.Rare;
                            }
                            if ((packIndex + 1) == shopExtraGuaranteePackCount)
                            {
                                CardRarity newRarityGuarantee = isUltraRareGuaranteed ? CardRarity.UltraRare : CardRarity.SuperRare;
                                if (newRarityGuarantee > rarityGuarantee)
                                {
                                    rarityGuarantee = newRarityGuarantee;
                                }
                            }
                        }
                        matches.Clear();
                        int num = cardIndex + 1;
                        foreach (ShopOddsRarity item in odds.CardRateList)
                        {
                            if (num >= item.StartNum && num <= item.EndNum)
                            {
                                if (item.GuaranteeRareMin > 0 || item.GuaranteeRareMax > 0)
                                {
                                    if (rarityGuarantee >= item.GuaranteeRareMin && rarityGuarantee <= item.GuaranteeRareMax)
                                    {
                                        matches.Add(item);
                                    }
                                }
                                else
                                {
                                    matches.Add(item);
                                }
                            }
                        }
                        ShopOddsRarity match = null;
                        if (matches.Count > 0)
                        {
                            match = matches.OrderByDescending(x => x.GuaranteeRareMin).First();
                        }
                        else if (odds.CardRateList.Count > 0)
                        {
                            match = odds.CardRateList.OrderBy(x => x.GuaranteeRareMin).FirstOrDefault(x => num >= x.StartNum && num <= x.EndNum);
                            if (match != null)
                            {
                                // NOTE: Removing this log for now as it'll always occur for a default odds guarantee of SuperRare or higher (see TODO above)
                                //LogInfo("Using fallback case for rarity odds on card index " + cardIndex);
                            }
                        }
                        if (match == null)
                        {
                            Utils.LogWarning("Failed to determine odds for card index " + cardIndex + " on pack id " + shopItem.Id);
                            //continue;
                            match = odds.CardRateList[0];
                        }
                        Dictionary<CardRarity, double> rarityAccumaltiveRate = new Dictionary<CardRarity, double>();
                        double totalRarityPercent = 0;
                        double subtractPercent = 0;
                        foreach (KeyValuePair<CardRarity, double> rate in match.Rate.OrderBy(x => x.Key))
                        {
                            if (rate.Value <= 0 || numCardsByRarity[rate.Key] == 0)
                            {
                                // Remove the percentage from the calculation as otherwise it'll produce skewed results
                                subtractPercent += Math.Max(0, rate.Value);
                                continue;
                            }
                            totalRarityPercent += rate.Value;
                            rarityAccumaltiveRate[rate.Key] = totalRarityPercent;
                        }
                        if (rarityAccumaltiveRate.Count == 0)
                        {
                            Utils.LogWarning("Missing odds for card index " + cardIndex + " on pack id " + shopItem.Id);
                        }
                        double rarityPercent = rand.NextDouble() * (100 - subtractPercent);
                        CardRarity rarity = CardRarity.None;
                        foreach (KeyValuePair<CardRarity, double> rate in rarityAccumaltiveRate.OrderBy(x => x.Key))
                        {
                            if (rarityPercent < rate.Value)
                            {
                                rarity = rate.Key;
                                break;
                            }
                        }
                        //LogInfo("rarityPercent: " + rarityPercent + " value: " + rarity);
                        if (rarity == CardRarity.None)
                        {
                            if (rarityAccumaltiveRate.Count == 0)
                            {
                                Utils.LogWarning("Skip as all rarities cleared out for index " + cardIndex);
                                continue;
                            }
                            rarity = rarityAccumaltiveRate.Keys.OrderByDescending(x => x).First();
                        }
                        if (match.Standard)
                        {
                            pickupPacks.Add(shopItem);
                        }
                        int foundCardId = 0;
                        List<int> shuffledCards = Utils.Shuffle(rand, new List<int>(match.Standard ? Shop.StandardPack.Cards.Keys : shopItem.Cards.Keys));
                        List<CardRarity> raritiesToTry = new List<CardRarity>();
                        {
                            CardRarity tempRarity = rarity;
                            while (tempRarity >= CardRarity.Normal)
                            {
                                raritiesToTry.Add(tempRarity);
                                tempRarity--;
                            }
                            if (Shop.UpgradeRarityWhenNotFound)
                            {
                                tempRarity = rarity + 1;
                                while (tempRarity <= CardRarity.UltraRare)
                                {
                                    raritiesToTry.Add(tempRarity);
                                    tempRarity++;
                                }
                            }
                        }
                        CardRarity originalRarity = rarity;
                        while (raritiesToTry.Count > 0)
                        {
                            rarity = raritiesToTry[0];
                            foreach (int cardId in shuffledCards)
                            {
                                CardRarity cardRarity;
                                if (TryGetCardRarity(cardId, match.Standard ? standardPackCardRare : packCardRare, out cardRarity) && cardRarity == rarity)
                                {
                                    if (Shop.NoDuplicatesPerPack && seenCardIdsThisPack.Contains(cardId) && cardRarity < CardRarity.SuperRare)
                                    {
                                        continue;
                                    }
                                    seenCardIdsThisPack.Add(cardId);
                                    foundCardId = cardId;
                                    break;
                                }
                            }
                            if (foundCardId != 0)
                            {
                                break;
                            }
                            raritiesToTry.RemoveAt(0);
                        }
                        if (foundCardId == 0)
                        {
                            Utils.LogWarning("Failed to find card id for card index " + cardIndex + " on pack id " + shopItem.Id);
                            continue;
                        }
                        if (rarity > originalRarity)
                        {
                            Utils.LogWarning("Upgraded rarity from " + originalRarity + " to " + rarity + ". Consider using a different shop odds for this pack");
                        }
                        if (rarity == CardRarity.UltraRare)
                        {
                            pulledUltraRare = true;
                        }
                        CardStyleRarity styleRarity = CardStyleRarity.Normal;
                        if (!Shop.DisableCardStyleRarity)
                        {
                            foreach (ShopOddsStyleRarity item in odds.CardStyleRarityRateList)
                            {
                                if (item.Rarities.Contains(rarity))
                                {
                                    Dictionary<CardStyleRarity, double> styleRarityAccumaltiveRate = new Dictionary<CardStyleRarity, double>();
                                    double styleRarityTotalPercent = 0;
                                    foreach (KeyValuePair<CardStyleRarity, double> rate in item.Rate.OrderBy(x => x.Key))
                                    {
                                        if (rate.Value <= 0)
                                        {
                                            continue;
                                        }
                                        styleRarityTotalPercent += rate.Value;
                                        styleRarityAccumaltiveRate[rate.Key] = styleRarityTotalPercent;
                                    }
                                    if (styleRarityAccumaltiveRate.Count == 0)
                                    {
                                        Utils.LogWarning("Invalid style rarity odds for card index " + cardIndex + " on pack id " + shopItem.Id);
                                    }
                                    else
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
                                        //LogInfo("styleRarityPercent: " + styleRarityPercent + " value: " + styleRarity);
                                    }
                                    break;
                                }
                            }
                        }
                        bool isNewCard = false;
                        if (request.Player.Cards.GetCount(foundCardId) == 0 || newCardIds.Contains(foundCardId))
                        {
                            isNewCard = true;
                            newCardIds.Add(foundCardId);
                        }
                        request.Player.Cards.Add(foundCardId, 1, PlayerCardKind.Dismantle, styleRarity);
                        cardIdsToUpdate.Add(foundCardId);
                        HashSet<int> thisCardFoundSecrets = new HashSet<int>();
                        HashSet<int> thisCardFoundSecretsExtend = new HashSet<int>();
                        List<ShopItemInfo> secretPacks;
                        if (rarity >= CardRarity.SuperRare && Shop.SecretPacksByCardId.TryGetValue(foundCardId, out secretPacks))
                        {
                            foreach (ShopItemInfo secretPack in secretPacks)
                            {
                                switch (secretPack.SecretType)
                                {
                                    case ShopItemSecretType.FindOrCraft:
                                    case ShopItemSecretType.Find:
                                        if (request.Player.ShopState.GetAvailability(Shop, secretPack) == PlayerShopItemAvailability.Available)
                                        {
                                            thisCardFoundSecretsExtend.Add(secretPack.ShopId);
                                        }
                                        else
                                        {
                                            request.Player.ShopState.New(secretPack);
                                            foundSecrets.Add(secretPack.ShopId);
                                            thisCardFoundSecrets.Add(secretPack.ShopId);
                                        }
                                        request.Player.ShopState.Unlock(secretPack);
                                        showSecretFoundResult = true;
                                        break;
                                }
                            }
                        }
                        List<ShopItemInfo> additionalUnlockedSecrets = shopItem.DoUnlockSecrets(request.Player, Shop);
                        if (additionalUnlockedSecrets != null && additionalUnlockedSecrets.Count > 0)
                        {
                            foreach (ShopItemInfo additionalUnlock in additionalUnlockedSecrets)
                            {
                                foundSecrets.Add(additionalUnlock.ShopId);
                                thisCardFoundSecrets.Add(additionalUnlock.ShopId);
                                showSecretFoundResult = true;
                            }
                        }
                        CardRarity backSideRarity = rarity;
                        if (Shop.PackOddsVisuals.RarityJebait && Shop.PackOddsVisuals.RarityOnCardBack &&
                            rarity > CardRarity.Rare && rand.Next(100) < 20)
                        {
                            backSideRarity--;
                        }
                        if (rarity > highestCardRarity)
                        {
                            highestCardRarity = rarity;
                        }
                        if (backSideRarity > highestCardBackRarity)
                        {
                            highestCardBackRarity = backSideRarity;
                        }
                        cardInfos.Add(new Dictionary<string, object>()
                        {
                            { "mrk", foundCardId },
                            { "rarity", (int)rarity },
                            { "backSideRarity", Shop.PackOddsVisuals.RarityOnCardBack ? (int)backSideRarity : 1 },
                            { "foundSecrets", thisCardFoundSecrets.ToArray() },
                            { "extendSecrets", thisCardFoundSecretsExtend.ToArray() },
                            { "new", isNewCard },
                            { "premiumType", (int)styleRarity }
                        });
                    }
                    if (cardInfos.Count > 0)
                    {
                        bool showRarityOnPack = Shop.PackOddsVisuals.RarityOnPack;
                        bool doCutin = highestCardBackRarity >= CardRarity.SuperRare && rand.Next(100) < 5;
                        bool jebaitRarity = Shop.PackOddsVisuals.RarityJebait && (packCount == 1 || packIndex < packCount - 1) &&
                            highestCardBackRarity >= CardRarity.SuperRare && rand.Next(100) < 20;
                        bool jebaitRarityBg = jebaitRarity && rand.Next(2) == 0;

                        // 1 = Blue lighting bolt to the left
                        // 2 = Blue lighting bolt to left / right
                        // 3 = Blue lighting bolt to left / right and purple middle left
                        int thunderType = 1;
                        if (highestCardBackRarity >= CardRarity.SuperRare)
                        {
                            thunderType = rand.Next(2, 4);
                        }
                        else
                        {
                            thunderType = rand.Next(1, 3);
                        }
                        // 1 = Normal
                        // 2 = Monster cutting animation
                        int cutType = doCutin ? 2 : 1;
                        // 1 = Normal
                        // 2 = White flash
                        // 3 = White flash followed by yellow background
                        // 4 = White flash followed by rainbow background
                        int rarityUpBgType = jebaitRarity && jebaitRarityBg ? (int)highestCardBackRarity : 1;
                        // 1 = Normal
                        // 2 = White flash over the card pack
                        // 3 = White flash over the card pack (x2), gets big and emits a yellow effect
                        // 4 = White flash over the card pack (x2), gets big and emits a rainbow effect
                        int rarityUpType = jebaitRarity && !jebaitRarityBg ? (int)highestCardBackRarity : 1;
                        Dictionary<string, object> effects = new Dictionary<string, object>()
                        {
                            { "thunder", showRarityOnPack ? thunderType : rand.Next(1, 4) },
                            { "rarityup", showRarityOnPack ? rarityUpType : 1 },
                            { "cut", cutType },
                            { "rarityupBg", showRarityOnPack ? rarityUpBgType : 1 },
                            { "rarity", (jebaitRarity || !showRarityOnPack ? 1 : (int)highestCardBackRarity) },
                        };
                        List<Dictionary<string, object>> packList;
                        if (!packInfos.TryGetValue(shopItem, out packList))
                        {
                            packInfos[shopItem] = packList = new List<Dictionary<string, object>>();
                        }
                        packList.Add(new Dictionary<string, object>()
                        {
                            { "effects", effects },
                            { "cardInfo", cardInfos },
                        });
                    }
                }
                bool isNextPackUR = packCount == shopExtraGuaranteePackCount && !pulledUltraRare && !Shop.DisableUltraRareGuarantee;
                if (packInfos.Count > 0)
                {
                    Dictionary<string, object> gacha = request.GetOrCreateDictionary("Gacha");

                    // TODO: Add support for more than 10 per pack type (would need to split this up based on set of 10 per pack type)
                    List<object> packs = new List<object>();
                    foreach (KeyValuePair<ShopItemInfo, List<Dictionary<string, object>>> pack in packInfos)
                    {
                        packs.Add(new Dictionary<string, object>()
                        {
                            { "packInfo", pack.Value },
                            { "effects", new Dictionary<string, object>()
                            {
                                { "isPickup", pickupPacks.Contains(pack.Key) },
                                { "imageName", shopItem.PackImageName },
                                { "smokeType", smokeType }
                            } },
                        });
                    }

                    Dictionary<string, object> drawInfo = Utils.GetOrCreateDictionary(gacha, "drawInfo");
                    drawInfo["packs"] = packs;
                    drawInfo["options"] = new Dictionary<string, object>()
                    {
                        { "skippable", true },
                    };

                    gacha["resultInfo"] = new Dictionary<string, object>()
                    {
                        { "isSendGift", false },
                        { "showSecretFoundResult", showSecretFoundResult },
                        { "isNextFinalizedUR", isNextPackUR },
                        { "NextFinalizedURNameTextId", "IDS_CARDPACK_ID0001_NAME" },
                    };
                }
                WriteCards_have(request, cardIdsToUpdate);
                request.Response["Item"] = new Dictionary<string, object>()
                {
                    { "have", new Dictionary<string, object>() {
                        { ((int)ItemID.Value.Gem).ToString(), request.Player.Gems },
                    }}
                };
                HashSet<int> shopIdsToUpdate = new HashSet<int>();
                if (packCount == shopExtraGuaranteePackCount)
                {
                    request.Player.ShopState.SetUltraRareGaurantee(shopItem.Id, isNextPackUR);
                    shopIdsToUpdate.Add(shopItem.ShopId);// Update for either state change (SR->UR / UR->SR)
                }
                if (shopItem.Buylimit > 0 || (shopItem.SecretBuyLimit > 0 && shopItem.SecretType != ShopItemSecretType.None))
                {
                    // To update indicators for the buy limit
                    shopIdsToUpdate.Add(shopItem.ShopId);
                }
                if (shopItem.UnlockSecrets.Count > 0 || shopItem.DescTextGenerated)
                {
                    // To update indicators for the next pack unlock
                    shopIdsToUpdate.Add(shopItem.ShopId);
                }
                foreach (int secretShopId in foundSecrets)
                {
                    shopIdsToUpdate.Add(secretShopId);
                }
                foreach (int shopIdToUpdate in shopIdsToUpdate)
                {
                    Act_ShopGetList(request, shopIdToUpdate);
                }
            }
            return true;
        }

        void Act_GachaGetCardList(GameServerWebRequest request)
        {
            int cardListId = Utils.GetValue<int>(request.ActParams, "card_list_id");
            if (cardListId > 0)
            {
                List<int> cardList = null;
                ShopItemInfo shopItem;
                if (Shop.AllShops.TryGetValue(cardListId, out shopItem))
                {
                    cardList = new List<int>(shopItem.Cards.Keys);
                }
                else
                {
                    cardList = new List<int>();
                    Utils.LogWarning("Failed to find shop " + cardListId);
                }
                request.Response["Gacha"] = new Dictionary<string, object>()
                {
                    { "cardList", new Dictionary<string, object>() {
                        { cardListId.ToString(), cardList }
                    }}
                };
                if (Shop.PerPackRarities)
                {
                    WritePerPackRarities(request, GetCardRarities(request.Player, shopItem));
                }
            }
        }

        void Act_GachaGetProbability(GameServerWebRequest request)
        {
            int packId = Utils.GetValue<int>(request.ActParams, "gacha_id");
            ShopItemInfo item;
            if (Shop.PacksByPackId.TryGetValue(packId, out item) && item.Cards.Count > 0)
            {
                ShopOddsInfo odds = item.GetOdds(Shop);
                if (odds != null)
                {
                    Dictionary<CardRarity, int> cardRarityCount = new Dictionary<CardRarity, int>();
                    Dictionary<int, int> packCardRare = GetCardRarities(request.Player, item);
                    foreach (KeyValuePair<int, int> card in packCardRare)
                    {
                        if (!cardRarityCount.ContainsKey((CardRarity)card.Value))
                        {
                            cardRarityCount[(CardRarity)card.Value] = 1;
                        }
                        else
                        {
                            cardRarityCount[(CardRarity)card.Value]++;
                        }
                    }
                    List<object> cardRateList = new List<object>();
                    List<object> premiereRateList = new List<object>();
                    foreach (ShopOddsRarity cardRate in odds.CardRateList)
                    {
                        Dictionary<string, object> rateDict = new Dictionary<string, object>();
                        foreach (KeyValuePair<CardRarity, double> rate in cardRate.Rate)
                        {
                            int num;
                            cardRarityCount.TryGetValue(rate.Key, out num);
                            rateDict[((int)rate.Key).ToString()] = new Dictionary<string, object>()
                            {
                                { "num", num },
                                { "rate", string.Format("{0:0.00}", rate.Value) }
                            };
                        }
                        Dictionary<string, object> info = new Dictionary<string, object>()
                        {
                            { "start_num", cardRate.StartNum },
                            { "end_num", cardRate.EndNum },
                            { "standard", cardRate.Standard },
                            { "rate", rateDict }
                        };
                        if (cardRate.GuaranteeRareMin != CardRarity.None || cardRate.GuaranteeRareMax != CardRarity.None)
                        {
                            info["settle_rare_min"] = (int)cardRate.GuaranteeRareMin;
                            info["settle_rare_max"] = (int)cardRate.GuaranteeRareMax;
                        }
                        cardRateList.Add(info);
                    }
                    foreach (ShopOddsStyleRarity premiereRate in odds.CardStyleRarityRateList)
                    {
                        List<object> rareList = new List<object>();
                        Dictionary<string, object> rateDict = new Dictionary<string,object>();
                        foreach (CardRarity rarity in premiereRate.Rarities)
                        {
                            rareList.Add((int)rarity);
                        }
                        foreach (KeyValuePair<CardStyleRarity, double> rate in premiereRate.Rate)
                        {
                            rateDict[((int)rate.Key).ToString()] = string.Format("{0:0.00}", rate.Value);
                        }
                        premiereRateList.Add(new Dictionary<string, object>()
                            {
                                { "rare", rareList },
                                { "rate", rateDict }
                            });
                    }
                    request.Response["Gacha"] = new Dictionary<string, object>()
                    {
                        { "rateData", new Dictionary<string, object>() {
                            { "gachaType", (int)item.PackType },
                            { "cardRateList", cardRateList },
                            { "premiereRateList", premiereRateList },
                        }}
                    };
                }
            }
            request.Remove("Gacha.rateData");
        }
    }
}
