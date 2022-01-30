using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace YgoMaster
{
    partial class GameServer
    {
        Player thePlayer;
        Random rand = new Random();

        int NumDeckSlots;
        bool UnlockAllCards;
        bool UnlockAllItems;
        bool SoloRemoveDuelTutorials;
        bool SoloDisableNoShuffle;
        HashSet<int> DefaultItems;
        int DefaultGems;
        CraftInfo Craft;
        ShopInfo Shop;
        Dictionary<int, DeckInfo> StructureDecks;// <structureid, DeckInfo>
        Dictionary<int, int> CardRare;
        List<int> CardCraftable;
        Dictionary<string, object> Regulation;
        Dictionary<string, object> SoloData;
        Dictionary<int, DuelSettings> SoloDuels;// <chapterid, DuelSettings>

        void LoadSettings()
        {
            //MergeShopDumps();

            if (!File.Exists(settingsFile))
            {
                LogWarning("Failed to load settings file");
            }

            string text = File.ReadAllText(settingsFile);
            Dictionary<string, object> values = MiniJSON.Json.DeserializeStripped(text) as Dictionary<string, object>;
            if (values == null)
            {
                throw new Exception("Failed to parse settings json");
            }

            NumDeckSlots = GetValue<int>(values, "DeckSlots", 20);
            GetIntHashSet(values, "DefaultItems", DefaultItems = new HashSet<int>(), ignoreZero: true);
            DefaultGems = GetValue<int>(values, "DefaultGems");
            UnlockAllCards = GetValue<bool>(values, "UnlockAllCards");
            UnlockAllItems = GetValue<bool>(values, "UnlockAllItems");
            SoloRemoveDuelTutorials = GetValue<bool>(values, "SoloRemoveDuelTutorials");
            SoloDisableNoShuffle = GetValue<bool>(values, "SoloDisableNoShuffle");

            CardRare = new Dictionary<int, int>();
            Dictionary<string, object> cardRareDict = GetValue<Dictionary<string, object>>(values, "CardRare");
            if (cardRareDict != null)
            {
                foreach (KeyValuePair<string, object> item in cardRareDict)
                {
                    CardRare[int.Parse(item.Key)] = (int)Convert.ChangeType(item.Value, typeof(int));
                }
            }

            CardCraftable = new List<int>();
            List<object> cardCrList = GetValue<List<object>>(values, "CardCr");
            if (cardCrList != null)
            {
                foreach (object id in cardCrList)
                {
                    CardCraftable.Add((int)Convert.ChangeType(id, typeof(int)));
                }
            }
            if (GetValue<bool>(values, "CardCratableAll"))
            {
                CardCraftable.Clear();
                foreach (int cardId in CardRare.Keys)
                {
                    CardCraftable.Add(cardId);
                }
            }

            Regulation = GetValue<Dictionary<string, object>>(values, "Regulation");

            StructureDecks = new Dictionary<int, DeckInfo>();
            Dictionary<string, object> structureDecksData = GetValue<Dictionary<string, object>>(values, "Structure");
            if (structureDecksData != null)
            {
                foreach (object obj in structureDecksData.Values)
                {
                    DeckInfo deck = new DeckInfo();
                    Dictionary<string, object> deckData = obj as Dictionary<string, object>;
                    deck.Id = GetValue<int>(deckData, "structure_id");
                    deck.Accessory.FromDictionary(GetDictionary(deckData, "accessory"));
                    deck.DisplayCards.FromDictionary(GetDictionary(deckData, "focus"));
                    deck.FromDictionary(GetDictionary(deckData, "contents"));
                    StructureDecks[deck.Id] = deck;
                }
            }

            Craft = new CraftInfo();
            Craft.FromDictionary(GetValue(values, "Craft", default(Dictionary<string, object>)));

            LoadShop();
            LoadSolo();
        }

        void SavePlayer(Player player)
        {
            //LogInfo("Save (player)");
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Code"] = player.Code;
            data["Name"] = player.Name;
            data["Rank"] = player.Rank;
            data["Rate"] = player.Rate;
            data["Level"] = player.Level;
            data["Exp"] = player.Exp;
            data["Gems"] = player.Gems;
            data["IconId"] = player.IconId;
            data["IconFrameId"] = player.IconFrameId;
            data["AvatarId"] = player.AvatarId;
            data["Wallpaper"] = player.Wallpaper;
            data["CardFavorites"] = player.CardFavorites.ToDictionary();
            data["TitleTags"] = player.TitleTags.ToArray();
            if (!UnlockAllItems)
            {
                data["Items"] = player.Items.ToArray();
            }
            data["SoloChapters"] = player.SoloChaptersToDictionary();
            data["CraftPoints"] = player.CraftPoints.ToDictionary();
            data["OrbPoints"] = player.OrbPoints.ToDictionary();
            data["ShopState"] = player.ShopState.ToDictionary();
            if (!UnlockAllCards)
            {
                data["Cards"] = player.Cards.ToDictionary(CardRare);
            }
            string jsonFormatted = FormatJson(MiniJSON.Json.Serialize(data));
            File.WriteAllText(playerSettingsFile, jsonFormatted);
        }

        void LoadPlayer(Player player)
        {
            Dictionary<string, object> data = null;
            if (File.Exists(GameServer.playerSettingsFile))
            {
                data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(GameServer.playerSettingsFile)) as Dictionary<string, object>;
            }
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            uint code;
            if (TryGetValue(data, "Code", out code) && code != 0)
            {
                player.Code = code;
            }
            player.Name = GetValue<string>(data, "Name", "Duelist");
            player.Rank = GetValue<int>(data, "Rank", (int)StandardRank.ROOKIE);
            player.Rate = GetValue<int>(data, "Rate");
            player.Level = GetValue<int>(data, "Level", 1);
            player.Exp = GetValue<long>(data, "Exp");
            player.Gems = GetValue<int>(data, "Gems", DefaultGems);
            player.IconId = GetValue<int>(data, "IconId");
            player.IconFrameId = GetValue<int>(data, "IconFrameId");
            player.AvatarId = GetValue<int>(data, "AvatarId");
            player.Wallpaper = GetValue<int>(data, "Wallpaper");

            player.SoloChaptersFromDictionary(GetDictionary(data, "SoloChapters"));
            player.CraftPoints.FromDictionary(GetDictionary(data, "CraftPoints"));
            player.OrbPoints.FromDictionary(GetDictionary(data, "OrbPoints"));
            player.ShopState.FromDictionary(GetDictionary(data, "ShopState"));
            player.CardFavorites.FromDictionary(GetDictionary(data, "CardFavorites"));
            List<object> titleTags;
            if (TryGetValue(data, "TitleTags", out titleTags))
            {
                foreach (object tag in titleTags)
                {
                    player.TitleTags.Add((int)Convert.ChangeType(tag, typeof(int)));
                }
            }
            if (UnlockAllItems)
            {
                Type[] enumTypes =
                {
                    typeof(ItemID.AVATAR),
                    typeof(ItemID.ICON),
                    typeof(ItemID.ICON_FRAME),
                    typeof(ItemID.PROTECTOR),
                    typeof(ItemID.DECK_CASE),
                    typeof(ItemID.FIELD),
                    typeof(ItemID.FIELD_OBJ),
                    typeof(ItemID.AVATAR_HOME),
                    typeof(ItemID.WALLPAPER),
                };
                foreach (Type enumType in enumTypes)
                {
                    foreach (int value in Enum.GetValues(enumType))
                    {
                        player.Items.Add(value);
                    }
                }
            }
            else
            {
                GetIntHashSet(data, "Items", player.Items, ignoreZero: true);
                foreach (int item in DefaultItems)
                {
                    if (item != 0)
                    {
                        player.Items.Add(item);
                        switch (ItemID.GetCategoryFromID(item))
                        {
                            case ItemID.Category.ICON: if (!player.Items.Contains(player.IconId)) player.IconId = item; break;
                            case ItemID.Category.ICON_FRAME: if (!player.Items.Contains(player.IconFrameId)) player.IconFrameId = item; break;
                            case ItemID.Category.AVATAR: if (!player.Items.Contains(player.AvatarId)) player.AvatarId = item; break;
                            case ItemID.Category.WALLPAPER: if (!player.Items.Contains(player.Wallpaper)) player.Wallpaper = item; break;
                            case ItemID.Category.FIELD:
                                foreach (int fieldPartItemId in ItemID.GetDuelFieldParts(item))
                                {
                                    player.Items.Add(fieldPartItemId);
                                }
                                break;
                        }
                    }
                }
            }
            if (UnlockAllCards)
            {
                foreach (int cardId in CardRare.Keys)
                {
                    player.Cards.SetCount(cardId, 3, PlayerCardKind.Dismantle, CardStyleRarity.Normal);
                }
            }
            else
            {
                Dictionary<string, object> cards = GetDictionary(data, "Cards");
                if (cards != null)
                {
                    player.Cards.FromDictionary(cards);
                }
            }
            if (player.Cards.Count == 0)
            {
                // Only look for default starting decks if the player has no cards
                foreach (int itemId in player.Items)
                {
                    if (ItemID.GetCategoryFromID(itemId) == ItemID.Category.STRUCTURE)
                    {
                        DeckInfo deck;
                        if (StructureDecks.TryGetValue(itemId, out deck))
                        {
                            foreach (int cardId in deck.GetAllCards())
                            {
                                player.Cards.Add(cardId, 1, PlayerCardKind.NoDismantle, CardStyleRarity.Normal);
                            }
                        }
                    }
                }
            }

            if (Directory.Exists(decksDirectory))
            {
                foreach (string file in Directory.GetFiles(decksDirectory, "*json"))
                {
                    try
                    {
                        DeckInfo deck = new DeckInfo();
                        deck.File = file;
                        LoadDeck(deck);
                        deck.Id = player.NextDeckUId++;
                        player.Decks[deck.Id] = deck;
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to load deck " + file);
                    }
                }
            }
        }

        void SaveDeck(DeckInfo deck)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Name"] = deck.Name;
            data["Status"] = deck.Status;
            data["TimeCreated"] = deck.TimeCreated;
            data["TimeEdited"] = deck.TimeEdited;
            data["Accessory"] = deck.Accessory.ToDictionary();
            data["DisplayCards"] = deck.DisplayCards.ToIndexDictionary();
            data["MainDeckCards"] = deck.MainDeckCards.ToDictionary();
            data["ExtraDeckCards"] = deck.ExtraDeckCards.ToDictionary();
            data["SideDeckCards"] = deck.SideDeckCards.ToDictionary();
            File.WriteAllText(deck.File, MiniJSON.Json.Serialize(data));
        }

        void LoadDeck(DeckInfo deck)
        {
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(deck.File)) as Dictionary<string, object>;
            deck.Name = GetValue<string>(data, "Name");
            deck.Status = GetValue<int>(data, "Status");
            deck.TimeCreated = GetValue<uint>(data, "TimeCreated");
            deck.TimeEdited = GetValue<uint>(data, "TimeEdited");
            deck.Accessory.FromDictionary(GetDictionary(data, "Accessory"));
            deck.DisplayCards.FromIndexedDictionary(GetDictionary(data, "DisplayCards"));
            deck.MainDeckCards.FromDictionary(GetDictionary(data, "MainDeckCards"));
            deck.ExtraDeckCards.FromDictionary(GetDictionary(data, "ExtraDeckCards"));
            deck.SideDeckCards.FromDictionary(GetDictionary(data, "SideDeckCards"));
        }

        void DeleteDeck(DeckInfo deck)
        {
            try
            {
                if (!string.IsNullOrEmpty(deck.File) && File.Exists(deck.File))
                {
                    File.Delete(deck.File);
                }
            }
            catch
            {
            }
        }

        void LoadShop()
        {
            Shop = new ShopInfo();
            LoadShop(Path.Combine(dataDirectory, "Shop.json"), true);
            string dir = Path.Combine(dataDirectory, "Shop");
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles(dir, "*.json"))
                {
                    LoadShop(file, false);
                }
            }
            Dictionary<int, ShopItemInfo>[] allShops = { Shop.PackShop, Shop.StructureShop, Shop.AccessoryShop, Shop.SpecialShop };
            foreach (Dictionary<int, ShopItemInfo> shopTab in allShops)
            {
                foreach (KeyValuePair<int, ShopItemInfo> shopItem in shopTab)
                {
                    if (Shop.AllShops.ContainsKey(shopItem.Key))
                    {
                        LogWarning("Duplicate shop id " + shopItem.Key);
                    }
                    else
                    {
                        Shop.AllShops[shopItem.Key] = shopItem.Value;
                        switch (shopItem.Value.Category)
                        {
                            case ShopCategory.Pack:
                                Shop.PacksByPackId[shopItem.Value.Id] = shopItem.Value;
                                break;
                        }
                        switch (shopItem.Value.PackType)
                        {
                            case ShopPackType.Secret:
                                if (shopItem.Value.SecretType == ShopItemSecretType.None)
                                {
                                    shopItem.Value.SecretType = ShopItemSecretType.FindOrCraft;
                                }
                                foreach (int cardId in shopItem.Value.Cards)
                                {
                                    if (!Shop.SecretPacksByCardId.ContainsKey(cardId))
                                    {
                                        Shop.SecretPacksByCardId[cardId] = new List<ShopItemInfo>();
                                    }
                                    Shop.SecretPacksByCardId[cardId].Add(shopItem.Value);
                                }
                                break;
                        }
                    }
                }
            }
            int numStandardPacks = 0;
            foreach (ShopItemInfo item in Shop.AllShops.Values)
            {
                if (item.PackType == ShopPackType.Standard)
                {
                    numStandardPacks++;
                    Shop.StandardPack = item;
                }
            }
            if (numStandardPacks != 1 || Shop.StandardPack == null)
            {
                // NOTE: If this changes make sure to update the pack opener code to include the additional packs
                LogWarning("Expected to find 1 standard card pack in shop data but found " + numStandardPacks);
            }
            if (Shop.PutAllCardsInStandrdPack && Shop.StandardPack != null)
            {
                Shop.StandardPack.Cards.Clear();
                foreach (int cardId in CardRare.Keys.OrderBy(x => x))
                {
                    Shop.StandardPack.Cards.Add(cardId);
                }
            }

            LoadShopPackOdds(Path.Combine(dataDirectory, "ShopPackOdds.json"));
            dir = Path.Combine(dataDirectory, "ShopPackOdds");
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles("*.json"))
                {
                    LoadShopPackOdds(file);
                }
            }
            LoadShopPackOddsVisuals(Path.Combine(dataDirectory, "ShopPackOddsVisuals.json"));// TODO: Maybe just merge into Shop.json
        }

        void LoadShop(string file, bool isMainShop)
        {
            if (!File.Exists(file))
            {
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            if (data == null)
            {
                return;
            }
            long secretPackDuration;
            if (!TryGetValue<long>(data, "SecretPackDuration", out secretPackDuration))
            {
                secretPackDuration = -1;// Special value to state that it's not defined (only checked in LoadShopItems)
            }
            if (isMainShop)
            {
                Shop.DefaultSecretDuration = GetValue<long>(data, "DefaultSecretDuration");
                Shop.PutAllCardsInStandrdPack = GetValue<bool>(data, "PutAllCardsInStandrdPack");
            }
            LoadShopItems(Shop.PackShop, "PackShop", data, secretPackDuration);
            LoadShopItems(Shop.StructureShop, "StructureShop", data, secretPackDuration);
            LoadShopItems(Shop.AccessoryShop, "AccessoryShop", data, secretPackDuration);
            LoadShopItems(Shop.SpecialShop, "SpecialShop", data, secretPackDuration);

            List<object> packShopImageStrings = GetValue(data, "PackShopImages", default(List<object>));
            if (packShopImageStrings != null)
            {
                foreach (object obj in packShopImageStrings)
                {
                    string packShopImage = obj as string;
                    if (!string.IsNullOrEmpty(packShopImage))
                    {
                        int cardId = int.Parse(packShopImage.Split('_')[1]);
                        Shop.PackShopImagesByCardId[cardId] = packShopImage;
                    }
                }
            }
        }

        void LoadShopItems(Dictionary<int, ShopItemInfo> shopItems, string type, Dictionary<string, object> shopData, long secretPackDuration)
        {
            foreach (Dictionary<string, object> data in GetDictionaryCollection(shopData, type))
            {
                ShopItemInfo info = new ShopItemInfo();
                info.Buylimit = GetValue<int>(data, "buyLimit");// custom
                info.SecretBuyLimit = GetValue<int>(data, "secretBuyLimit");// custom
                info.SecretType = (ShopItemSecretType)GetValue<int>(data, "secretType");// custom
                bool isSecretDurationDefined = TryGetValue<long>(data, "secretDuration", out info.SecretDurationInSeconds);// custom
                info.ExpireDateTime = GetValue<long>(data, "expireTime");// custom
                info.IconType = GetValue<int>(data, "iconType");
                info.IconData = GetValue<string>(data, "iconData");
                info.SubCategory = GetValue<int>(data, "subCategory");
                object previewObj = GetValue<object>(data, "preview");
                if (previewObj is string)
                {
                    info.Preview = previewObj as string;
                }
                else if (previewObj is Dictionary<string, object>)
                {
                    info.Preview = MiniJSON.Json.Serialize(previewObj);
                }
                switch (type)
                {
                    case "PackShop":
                        info.Category = ShopCategory.Pack;
                        info.Id = GetValue<int>(data, "packId");
                        if (info.Id == 0)
                        {
                            LogWarning("Invalid pack id id " + info.Id + " in shop data");
                            return;
                        }
                        info.PackType = (ShopPackType)GetValue<int>(data, "packType", 1);
                        info.CardNum = GetValue<int>(data, "pack_card_num", 1);
                        GetIntHashSet(data, "cardList", info.Cards);// custom
                        if (info.Cards.Count == 0)
                        {
                            LogWarning("Card pack " + info.Id + " doesn't have any cards");
                        }
                        switch (info.PackType)
                        {
                            case ShopPackType.Secret:
                                if (info.SecretType == ShopItemSecretType.None)
                                {
                                    info.SecretType = ShopItemSecretType.FindOrCraft;
                                }
                                break;
                        }
                        break;
                    case "StructureShop":
                        info.Category = ShopCategory.Structure;
                        info.Id = GetValue<int>(data, "structure_id");
                        if (!StructureDecks.ContainsKey(info.Id))
                        {
                            // Unknown structure deck
                            LogWarning("Unknown structure deck id " + info.Id + " in shop data");
                            return;
                        }
                        break;
                    case "AccessoryShop":
                        info.Category = ShopCategory.Accessory;
                        info.Id = GetValue<int>(data, "itemId");// There's also "item_id"
                        ItemID.Category itemCategory = ItemID.GetCategoryFromID(info.Id);
                        switch (itemCategory)
                        {
                            case ItemID.Category.AVATAR:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Mate;
                                break;
                            case ItemID.Category.FIELD:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Field;
                                break;
                            case ItemID.Category.PROTECTOR:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Protector;
                                break;
                            case ItemID.Category.ICON:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Icon;
                                break;
                            default:
                                LogWarning("Unhandled shop accessory type " + itemCategory + " for item id " + info.Id);
                                return;
                        }
                        break;
                    case "SpecialShop":
                        info.Category = ShopCategory.Special;
                        break;
                }
                switch (type)
                {
                    case "PackShop":
                    case "StructureShop":
                        info.NameText = GetValue<string>(data, "nameTextId");
                        info.DescShortText = GetValue<string>(data, "descShortTextId");
                        info.DescFullText = GetValue<string>(data, "descFullTextId");
                        info.IconMrk = GetValue<int>(data, "iconMrk");
                        info.Power = GetValue<int>(data, "power");
                        info.Flexibility = GetValue<int>(data, "flexibility");
                        info.Difficulty = GetValue<int>(data, "difficulty");
                        info.OddsName = GetValue<string>(data, "oddsName");
                        break;
                }
                foreach (Dictionary<string, object> priceData in GetDictionaryCollection(data, "prices"))
                {
                    int itemId = GetValue<int>(priceData, "item_id");
                    if (itemId != 1)
                    {
                        LogWarning("Unsupported item id " + itemId + " for shop");
                        return;
                    }
                    ShopItemPrice price = new ShopItemPrice();
                    price.Id = (info.Prices.Count + 1);
                    price.ButtonType = GetValue(priceData, "button_type", 1);
                    price.Price = GetValue<int>(priceData, "use_item_num");
                    price.POP = GetValue<string>(priceData, "POP");
                    price.TextId = GetValue<string>(priceData, "textId");
                    GetIntList(priceData, "textArgs", price.TextArgs);
                    info.Prices.Add(price);
                }
                if (shopItems.ContainsKey(info.ShopId))
                {
                    LogWarning("Duplicate shop id " + info.ShopId);
                    return;
                }
                if (info.SecretType != ShopItemSecretType.None && !isSecretDurationDefined)
                {
                    info.SecretDurationInSeconds = secretPackDuration >= 0 ? secretPackDuration : Shop.DefaultSecretDuration;
                }
                shopItems[info.ShopId] = info;
            }
        }

        void LoadShopPackOdds(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            List<object> oddsList = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as List<object>;
            if (oddsList == null)
            {
                return;
            }
            foreach (object dataObj in oddsList)
            {
                Dictionary<string, object> data = dataObj as Dictionary<string, object>;
                if (data == null)
                {
                    continue;
                }
                ShopOddsInfo shopOdds = new ShopOddsInfo();
                shopOdds.Name = GetValue<string>(data, "name");
                List<object> packTypesList = GetValue(data, "packTypes", default(List<object>));
                if (packTypesList != null)
                {
                    foreach (object packType in packTypesList)
                    {
                        shopOdds.PackTypes.Add((ShopPackType)(int)Convert.ChangeType(packType, typeof(int)));
                    }
                }
                List<object> cardRateList = GetValue(data, "cardRateList", default(List<object>));
                if (cardRateList != null)
                {
                    foreach (object cardRateDataObj in cardRateList)
                    {
                        Dictionary<string, object> cardRateData = cardRateDataObj as Dictionary<string, object>;
                        ShopOddsRarity rarityInfo = new ShopOddsRarity();
                        rarityInfo.StartNum = GetValue<int>(cardRateData, "start_num");
                        rarityInfo.EndNum = GetValue<int>(cardRateData, "end_num");
                        rarityInfo.Standard = GetValue<bool>(cardRateData, "standard");
                        rarityInfo.GuaranteeRareMin = (CardRarity)GetValue<int>(cardRateData, "settle_rare_min");
                        rarityInfo.GuaranteeRareMax = (CardRarity)GetValue<int>(cardRateData, "settle_rare_max");
                        Dictionary<string, object> rateData = GetDictionary(cardRateData, "rate");
                        foreach (KeyValuePair<string, object> item in rateData)
                        {
                            CardRarity rarity = (CardRarity)int.Parse(item.Key);
                            if (rarity == CardRarity.None)
                            {
                                LogWarning("Invalid card rarity in shop odds");
                            }
                            else
                            {
                                rarityInfo.Rate[rarity] = GetValue<double>(item.Value as Dictionary<string, object>, "rate");
                            }
                        }
                        shopOdds.CardRateList.Add(rarityInfo);
                    }
                }
                List<object> premiereRateList = GetValue(data, "premiereRateList", default(List<object>));
                if (premiereRateList != null)
                {
                    foreach (object premRateDataObj in premiereRateList)
                    {
                        Dictionary<string, object> premRateData = premRateDataObj as Dictionary<string, object>;
                        List<object> rareData = GetValue(premRateData, "rare", default(List<object>));
                        Dictionary<string, object> rateData = GetDictionary(premRateData, "rate");
                        if (rareData != null && rateData != null)
                        {
                            ShopOddsStyleRarity rarityInfo = new ShopOddsStyleRarity();
                            foreach (object rareObj in rareData)
                            {
                                CardRarity rarity = (CardRarity)(int)Convert.ChangeType(rareObj, typeof(int));
                                if (rarity != CardRarity.None)
                                {
                                    rarityInfo.Rarities.Add(rarity);
                                }
                            }
                            foreach (KeyValuePair<string, object> rate in rateData)
                            {
                                CardStyleRarity value = (CardStyleRarity)(int)Convert.ChangeType(rate.Key, typeof(int));
                                rarityInfo.Rate[value] = (double)Convert.ChangeType(rate.Value, typeof(double));
                            }
                            shopOdds.CardStyleRarityRateList.Add(rarityInfo);
                        }
                    }
                }
                foreach (ShopPackType packType in shopOdds.PackTypes)
                {
                    if (Shop.PackOddsByPackType.ContainsKey(packType))
                    {
                        LogWarning("Duplicate shop odds for pack type " + packType);
                    }
                    else
                    {
                        Shop.PackOddsByPackType[packType] = shopOdds;
                    }
                }
                if (!string.IsNullOrEmpty(shopOdds.Name))
                {
                    if (Shop.PackOddsByName.ContainsKey(shopOdds.Name))
                    {
                        LogWarning("Duplicate shop odds name '" + shopOdds.Name + "'");
                    }
                    else
                    {
                        Shop.PackOddsByName[shopOdds.Name] = shopOdds;
                    }
                }
            }
        }

        void LoadShopPackOddsVisuals(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            ShopOddsVisualsSettings settings = Shop.PackOddsVisuals;
            settings.RarityJebait = GetValue<bool>(data, "RarityJebait", true);
            settings.RarityOnCardBack = GetValue<bool>(data, "RarityOnCardBack", true);
            settings.RarityOnPack = GetValue<bool>(data, "RarityOnPack", true);
        }

        Dictionary<string, object> GetResData(Dictionary<string, object> data)
        {
            if (data != null && data.ContainsKey("code") && data.ContainsKey("res"))
            {
                List<object> resList = GetValue(data, "res", default(List<object>));
                if (resList != null && resList.Count > 0)
                {
                    List<object> resData = resList[0] as List<object>;
                    data = resData[1] as Dictionary<string, object>;
                }
            }
            return data;
        }

        /// <summary>
        /// Helper to merge packet logs of visting the shop to extract out desired information (card packs)
        /// </summary>
        void MergeShopDumps()
        {
            string dir = Path.Combine(dataDirectory, "ShopDumps");
            if (!Directory.Exists(dir))
            {
                return;
            }
            Dictionary<int, Dictionary<string, object>> packShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> structureShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> accessoryShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> specialShop = new Dictionary<int, Dictionary<string, object>>();
            List<Dictionary<string, object>> gachaDatas = new List<Dictionary<string, object>>();
            foreach (string file in Directory.GetFiles(dir))
            {
                Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                data = GetResData(data);
                if (data != null)
                {
                    Dictionary<string, object> shopData = GetValue(data, "Shop", default(Dictionary<string, object>));
                    if (shopData != null)
                    {
                        Action<string, Dictionary<int, Dictionary<string, object>>> fetchShop = (string shopName, Dictionary<int, Dictionary<string, object>> shop) =>
                            {
                                Dictionary<string, object> items = GetValue(shopData, shopName, default(Dictionary<string, object>));
                                foreach (KeyValuePair<string, object> item in items)
                                {
                                    int shopId;
                                    if (int.TryParse(item.Key, out shopId))
                                    {
                                        Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;
                                        if (itemData != null)
                                        {
                                            shop[shopId] = itemData;
                                        }
                                    }
                                }
                            };
                        fetchShop("PackShop", packShop);
                        fetchShop("StructureShop", structureShop);
                        fetchShop("AccessoryShop", accessoryShop);
                        fetchShop("SpecialShop", specialShop);
                    }
                    Dictionary<string, object> gachaData = GetValue(data, "Gacha", default(Dictionary<string, object>));
                    if (gachaData != null)
                    {
                        gachaDatas.Add(gachaData);
                    }
                }
            }
            foreach (Dictionary<string, object> gachaData in gachaDatas)
            {
                Dictionary<string, object> cardListData = GetValue(gachaData, "cardList", default(Dictionary<string, object>));
                if (cardListData != null)
                {
                    foreach (KeyValuePair<int, Dictionary<string, object>> packShopItem in packShop)
                    {
                        int normalCardListId = GetValue<int>(packShopItem.Value, "normalCardListId");
                        int pickupCardListId = GetValue<int>(packShopItem.Value, "pickupCardListId");
                        object cardIdsObj = null;
                        if (pickupCardListId != 0)
                        {
                            TryGetValue(cardListData, pickupCardListId.ToString(), out cardIdsObj);
                        }
                        else if (normalCardListId != 0)
                        {
                            TryGetValue(cardListData, normalCardListId.ToString(), out cardIdsObj);
                        }
                        if (cardIdsObj != null)
                        {
                            packShopItem.Value["cardList"] = cardIdsObj;
                        }
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            Dictionary<string, Dictionary<int, Dictionary<string, object>>> allShops = new Dictionary<string,Dictionary<int,Dictionary<string,object>>>()
            {
                { "PackShop", packShop },
                { "StructureShop", structureShop },
                { "AccessoryShop", accessoryShop },
                //{ "SpecialShop", specialShop }// TODO
            };
            foreach (KeyValuePair<string, Dictionary<int, Dictionary<string, object>>> shop in allShops)
            {
                sb.AppendLine("    \"" + shop.Key + "\": {");
                foreach (KeyValuePair<int, Dictionary<string, object>> shopItem in shop.Value)
                {
                    sb.AppendLine("        \"" + shopItem.Key + "\":" + MiniJSON.Json.Serialize(shopItem.Value) + ",");
                }
                sb.AppendLine("    },");
            }
            File.WriteAllText(Path.Combine(dataDirectory, "AllShopsMerged.json"), sb.ToString());
        }

        void LoadSolo()
        {
            string file = Path.Combine(dataDirectory, "Solo.json");
            if (!File.Exists(file))
            {
                LogWarning("Failed to load solo file");
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            data = GetResData(data);
            Dictionary<string, object> masterData;
            if (data != null && TryGetValue(data, "Master", out masterData))
            {
                data = masterData;
            }
            if (data != null)
            {
                TryGetValue(data, "Solo", out SoloData);
            }
            LoadSoloDuels();
        }

        void LoadSoloDuels()
        {
            SoloDuels = new Dictionary<int, DuelSettings>();
            int nextStoryDeckId = 1;
            string dir = Path.Combine(dataDirectory, "SoloDuels");
            if (!Directory.Exists(dir))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                data = GetResData(data);
                Dictionary<string, object> duelData;
                if (!TryGetValue(data, "Duel", out duelData))
                {
                    continue;
                }
                int chapterId;
                if (!TryGetValue(duelData, "chapter", out chapterId))
                {
                    continue;
                }
                DuelSettings duel = new DuelSettings();
                duel.FromDictionary(duelData);
                for (int i = 0; i < duel.Deck.Length; i++)
                {
                    if (duel.Deck[i].MainDeckCards.Count > 0)
                    {
                        duel.Deck[i].Id = nextStoryDeckId++;
                    }
                }
                duel.SetRequiredDefaults();
                if (SoloDuels.ContainsKey(chapterId))
                {
                    LogWarning("Duplicate chapter " + chapterId);
                    continue;
                }
                SoloDuels[chapterId] = duel;
            }
        }
    }
}
