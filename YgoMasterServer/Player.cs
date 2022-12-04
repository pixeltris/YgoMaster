using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    class Player
    {
        public bool RequiresSaving;
        public string Token;
        public uint Code;
        public string Name;
        public int Rank;
        public int Rate;
        public int Level;
        public long Exp;
        public int Gems;
        public int IconId;
        public int IconFrameId;
        public int AvatarId;
        public int Wallpaper;
        public HashSet<int> Items { get; private set; }
        public HashSet<int> TitleTags { get; private set; }
        public CardCollection CardFavorites { get; private set; }
        public Dictionary<int, DeckInfo> Decks { get; private set; }
        public PlayerCards Cards { get; private set; }
        public PlayerShopState ShopState { get; private set; }
        public Dictionary<int, ChapterStatus> SoloChapters { get; private set; }
        public PlayerCraftPoints CraftPoints { get; private set; }
        public PlayerOrbPoints OrbPoints { get; private set; }
        public PlayerDuelState Duel { get; private set; }

        public string Lang;// Temporary / not saved. Used to display topic text in "User.home"
        public int NextDeckUId = 1;

        public Player(uint code)
        {
            Code = code;
            Items = new HashSet<int>();
            TitleTags = new HashSet<int>();
            CardFavorites = new CardCollection();
            Decks = new Dictionary<int, DeckInfo>();
            Cards = new PlayerCards();
            ShopState = new PlayerShopState();
            SoloChapters = new Dictionary<int, ChapterStatus>();
            CraftPoints = new PlayerCraftPoints();
            OrbPoints = new PlayerOrbPoints();
            Duel = new PlayerDuelState(this);
        }

        public Dictionary<string, object> SoloChaptersToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<int, Dictionary<int, int>> gates = new Dictionary<int, Dictionary<int, int>>();
            foreach (KeyValuePair<int, ChapterStatus> state in SoloChapters)
            {
                if (state.Value > ChapterStatus.OPEN)
                {
                    int gateId = GameServer.GetChapterGateId(state.Key);
                    Dictionary<int, int> gateInfo;
                    if (!gates.TryGetValue(gateId, out gateInfo))
                    {
                        result[gateId.ToString()] = gates[gateId] = new Dictionary<int, int>();
                    }
                    gates[gateId][state.Key] = (int)state.Value;
                }
            }
            return result;
        }

        public void SoloChaptersFromDictionary(Dictionary<string, object> data)
        {
            SoloChapters.Clear();
            if (data == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> gate in data)
            {
                Dictionary<string, object> gateChapters = gate.Value as Dictionary<string, object>;
                int gateId;
                if (int.TryParse(gate.Key, out gateId) && gateChapters != null)
                {
                    foreach (KeyValuePair<string, object> chapter in gateChapters)
                    {
                        int chapterId;
                        if (int.TryParse(chapter.Key, out chapterId))
                        {
                            SoloChapters[chapterId] = (ChapterStatus)(int)Convert.ChangeType(chapter.Value, typeof(int));
                        }
                    }
                }
            }
        }

        public void AddItem(int itemId, int amount)
        {
            ItemID.Category category = ItemID.GetCategoryFromID(itemId);
            switch (category)
            {
                case ItemID.Category.CONSUME:
                    switch ((ItemID.Value)itemId)
                    {
                        case ItemID.Value.Gem:
                        case ItemID.Value.GemAlt:
                            Gems += amount;
                            break;
                        case ItemID.Value.CpN: CraftPoints.Add(CardRarity.Normal, amount); break;
                        case ItemID.Value.CpR: CraftPoints.Add(CardRarity.Rare, amount); break;
                        case ItemID.Value.CpSR: CraftPoints.Add(CardRarity.SuperRare, amount); break;
                        case ItemID.Value.CpUR: CraftPoints.Add(CardRarity.UltraRare, amount); break;
                        case ItemID.Value.OrbDark: OrbPoints.Add(OrbType.Dark, amount); break;
                        case ItemID.Value.OrbLight: OrbPoints.Add(OrbType.Light, amount); break;
                        case ItemID.Value.OrbEarth: OrbPoints.Add(OrbType.Earth, amount); break;
                        case ItemID.Value.OrbWater: OrbPoints.Add(OrbType.Water, amount); break;
                        case ItemID.Value.OrbFire: OrbPoints.Add(OrbType.Fire, amount); break;
                        case ItemID.Value.OrbWind: OrbPoints.Add(OrbType.Wind, amount); break;
                    }
                    break;
                case ItemID.Category.STRUCTURE:
                    throw new Exception("Use GiveStructureDeck to add structure decks");
                case ItemID.Category.CARD:
                    Cards.Add(itemId, amount, PlayerCardKind.Dismantle, CardStyleRarity.Normal);
                    break;
                default:
                    Items.Add(itemId);
                    break;
            }
        }
    }

    class PlayerCraftPoints : PlayerPoints<CardRarity>
    {
        public PlayerCraftPoints()
        {
            Limit = 2000000000;
            AddId(ItemID.Value.CpN, CardRarity.Normal);
            AddId(ItemID.Value.CpR, CardRarity.Rare);
            AddId(ItemID.Value.CpSR, CardRarity.SuperRare);
            AddId(ItemID.Value.CpUR, CardRarity.UltraRare);
        }
    }

    class PlayerOrbPoints : PlayerPoints<OrbType>
    {
        public PlayerOrbPoints()
        {
            Limit = 2000000000;
            AddId(ItemID.Value.OrbDark, OrbType.Dark);
            AddId(ItemID.Value.OrbLight, OrbType.Light);
            AddId(ItemID.Value.OrbEarth, OrbType.Earth);
            AddId(ItemID.Value.OrbWater, OrbType.Water);
            AddId(ItemID.Value.OrbFire, OrbType.Fire);
            AddId(ItemID.Value.OrbWind, OrbType.Wind);
        }
    }

    class PlayerPoints<T>
    {
        Dictionary<T, int> points = new Dictionary<T, int>();
        Dictionary<ItemID.Value, T> idmap = new Dictionary<ItemID.Value, T>();
        public int Limit { get; protected set; }

        protected void AddId(ItemID.Value id, T tid)
        {
            points[tid] = 0;
            idmap[id] = tid;
        }

        public int Get(T tid)
        {
            int value;
            points.TryGetValue(tid, out value);
            return value;
        }

        public void Set(T tid, int value)
        {
            points[tid] = Math.Max(0, Math.Min(value, Limit));
        }

        public void Add(PlayerPoints<T> other)
        {
            foreach (KeyValuePair<T, int> point in other.points)
            {
                Add(point.Key, point.Value);
            }
        }

        public void Add(T tid, int count)
        {
            Set(tid, Get(tid) + count);
        }

        public bool CanAdd(T tid, int count)
        {
            int total = points[tid] + count;
            return total >= 0 && total <= Limit;
        }

        public void Subtract(PlayerPoints<T> other)
        {
            foreach (KeyValuePair<T, int> point in other.points)
            {
                Subtract(point.Key, point.Value);
            }
        }

        public void Subtract(T tid, int count)
        {
            Set(tid, Get(tid) - count);
        }

        public bool CanSubtract(T tid, int count)
        {
            int total = points[tid] - count;
            return total >= 0 && total <= Limit;
        }

        public void Clear()
        {
            foreach (KeyValuePair<T, int> point in points)
            {
                points[point.Key] = 0;
            }
        }

        public void ToDictionary(Dictionary<string, object> data)
        {
            foreach (KeyValuePair<ItemID.Value, T> id in idmap)
            {
                data[((int)id.Key).ToString()] = points[id.Value];
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            ToDictionary(result);
            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            if (data == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> item in data)
            {
                int id;
                T tid;
                if (int.TryParse(item.Key, out id) && idmap.TryGetValue((ItemID.Value)id, out tid))
                {
                    points[tid] = (int)Convert.ChangeType(item.Value, typeof(int));
                }
            }
        }
    }

    class PlayerCards
    {
        Dictionary<int, State> cards = new Dictionary<int, State>();

        public int Count
        {
            get { return cards.Count; }
        }

        public int GetCount(int cardId)
        {
            return GetCount(cardId, PlayerCardKind.All);
        }

        public int GetCount(int cardId, PlayerCardKind kind)
        {
            State state;
            if (cards.TryGetValue(cardId, out state))
            {
                return state.Get(kind);
            }
            return 0;
        }

        public int GetCount(int cardId, PlayerCardKind kind, CardStyleRarity styleRarity)
        {
            State state;
            if (cards.TryGetValue(cardId, out state))
            {
                return state.Get(kind, styleRarity);
            }
            return 0;
        }

        public void Add(int cardId, int count, PlayerCardKind kind, CardStyleRarity styleRarity)
        {
            State state;
            cards.TryGetValue(cardId, out state);
            switch (kind)
            {
                case PlayerCardKind.All:
                    state.Set(state.Get(PlayerCardKind.Dismantle, styleRarity) + count, PlayerCardKind.Dismantle, styleRarity);
                    state.Set(state.Get(PlayerCardKind.NoDismantle, styleRarity) + count, PlayerCardKind.NoDismantle, styleRarity);
                    break;
                default:
                    state.Set(state.Get(kind, styleRarity) + count, kind, styleRarity);
                    break;
            }
            if (state.IsZero)
            {
                cards.Remove(cardId);
            }
            else
            {
                cards[cardId] = state;
            }
        }

        public void Subtract(int cardId, int count, PlayerCardKind kind, CardStyleRarity styleRarity)
        {
            State state;
            if (cards.TryGetValue(cardId, out state))
            {
                long time = state.Time;
                Add(cardId, -count, kind, styleRarity);
                if (cards.TryGetValue(cardId, out state))
                {
                    // For subtract we to keep the old time (the offical server updates time on craft but not on dismantle)
                    state.Time = time;
                    cards[cardId] = state;
                }
            }
        }

        public void SetCount(int cardId, int count, PlayerCardKind kind, CardStyleRarity styleRarity)
        {
            State state;
            cards.TryGetValue(cardId, out state);
            state.Set(count, kind, styleRarity);
            if (state.IsZero)
            {
                cards.Remove(cardId);
            }
            else
            {
                cards[cardId] = state;
            }
        }

        public void Remove(int cardId)
        {
            cards.Remove(cardId);
        }

        public void Remove(int cardId, PlayerCardKind kind, CardStyleRarity styleRarity)
        {
            SetCount(cardId, 0, kind, styleRarity);
        }

        public void Clear()
        {
            cards.Clear();
        }

        public bool Contains(int cardId)
        {
            return cards.ContainsKey(cardId);
        }

        public IEnumerable<int> GetIDs()
        {
            return cards.Keys;
        }

        public Dictionary<string, object> CardToDictionary(int cardId, Dictionary<int, int> cardRare = null)
        {
            int rarity;
            if (cardRare == null || !cardRare.TryGetValue(cardId, out rarity))
            {
                rarity = 1;
            }
            return CardToDictionary(cardId, rarity);
        }

        public Dictionary<string, object> CardToDictionary(int cardId, int rarity)
        {
            State state;
            if (cards.TryGetValue(cardId, out state))
            {
                return new Dictionary<string, object>()
                {
                    { "st", state.Time },
                    { "r", rarity },// The card rarity. I don't think this impacts anything so the current code path sets this to 1
                    { "n", state.Get(PlayerCardKind.Dismantle, CardStyleRarity.Normal) },
                    { "p1n", state.Get(PlayerCardKind.Dismantle, CardStyleRarity.Shine) },
                    { "p2n", state.Get(PlayerCardKind.Dismantle, CardStyleRarity.Royal) },
                    { "p_n", state.Get(PlayerCardKind.NoDismantle, CardStyleRarity.Normal) },
                    { "p_p1n", state.Get(PlayerCardKind.NoDismantle, CardStyleRarity.Shine) },
                    { "p_p2n", state.Get(PlayerCardKind.NoDismantle, CardStyleRarity.Royal) },
                    { "tn", state.Get(PlayerCardKind.All) }
                };
            }
            return new Dictionary<string, object>()
            {
                { "st", state.Time },
                { "r", rarity },
                { "n", 0 },
                { "p1n", 0 },
                { "p2n", 0 },
                { "p_n", 0 },
                { "p_p1n", 0 },
                { "p_p2n", 0 },
                { "tn", 0 }
            };
        }

        public Dictionary<string, object> ToDictionary(Dictionary<int, int> cardRare = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (int cardId in GetIDs())
            {
                Dictionary<string, object> cardData = CardToDictionary(cardId, cardRare);
                if (cardData != null)
                {
                    result[cardId.ToString()] = cardData;
                }
            }
            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            Clear();
            foreach (KeyValuePair<string, object> entry in data)
            {
                Dictionary<string, object> cardCountData = entry.Value as Dictionary<string, object>;
                int cardId;
                if (int.TryParse(entry.Key, out cardId) && cardCountData != null)
                {
                    State state = new State();
                    state.Time = Utils.GetValue<long>(cardCountData, "st");
                    state.NumNormal = Utils.GetValue<byte>(cardCountData, "n");
                    state.NumShine = Utils.GetValue<byte>(cardCountData, "p1n");
                    state.NumRoyal = Utils.GetValue<byte>(cardCountData, "p2n");
                    state.NumNormal_NoDismantle = Utils.GetValue<byte>(cardCountData, "p_n");
                    state.NumShine_NoDismantle = Utils.GetValue<byte>(cardCountData, "p_p1n");
                    state.NumRoyal_NoDismantle = Utils.GetValue<byte>(cardCountData, "p_p2n");
                    cards[cardId] = state;
                }
            }
        }

        struct State
        {
            public byte NumNormal;
            public byte NumShine;
            public byte NumRoyal;
            public byte NumNormal_NoDismantle;
            public byte NumShine_NoDismantle;
            public byte NumRoyal_NoDismantle;
            public long Time;

            public bool IsZero
            {
                get { return Get(PlayerCardKind.All) == 0; }
            }

            public int Get(PlayerCardKind kind)
            {
                switch (kind)
                {
                    case PlayerCardKind.All: return Get(PlayerCardKind.Dismantle) + Get(PlayerCardKind.NoDismantle);
                    case PlayerCardKind.Dismantle: return NumNormal + NumShine + NumRoyal;
                    case PlayerCardKind.NoDismantle: return NumNormal_NoDismantle + NumShine_NoDismantle + NumRoyal_NoDismantle;
                }
                return 0;
            }

            public int Get(PlayerCardKind kind, CardStyleRarity styleRarity)
            {
                switch (kind)
                {
                    case PlayerCardKind.All:
                        switch (styleRarity)
                        {
                            case CardStyleRarity.Normal: return NumNormal + NumNormal_NoDismantle;
                            case CardStyleRarity.Shine: return NumShine + NumShine_NoDismantle;
                            case CardStyleRarity.Royal: return NumRoyal + NumRoyal_NoDismantle;
                        }
                        break;
                    case PlayerCardKind.Dismantle:
                        switch (styleRarity)
                        {
                            case CardStyleRarity.Normal: return NumNormal;
                            case CardStyleRarity.Shine: return NumShine;
                            case CardStyleRarity.Royal: return NumRoyal;
                        }
                        break;
                    case PlayerCardKind.NoDismantle:
                        switch (styleRarity)
                        {
                            case CardStyleRarity.Normal: return NumNormal_NoDismantle;
                            case CardStyleRarity.Shine: return NumShine_NoDismantle;
                            case CardStyleRarity.Royal: return NumRoyal_NoDismantle;
                        }
                        break;
                }
                return 0;
            }

            public void Set(int count, PlayerCardKind kind)
            {
                Time = Utils.GetEpochTime();
                byte countByte = IntToByte(count);
                switch (kind)
                {
                    case PlayerCardKind.All:
                        NumNormal = NumShine = NumRoyal = countByte;
                        NumNormal_NoDismantle = NumShine_NoDismantle = NumRoyal_NoDismantle = countByte;
                        break;
                    case PlayerCardKind.Dismantle:
                        NumNormal = NumShine = NumRoyal = countByte;
                        break;
                    case PlayerCardKind.NoDismantle:
                        NumNormal_NoDismantle = NumShine_NoDismantle = NumRoyal_NoDismantle = countByte;
                        break;
                }
            }

            public void Set(int count, PlayerCardKind kind, CardStyleRarity styleRarity)
            {
                Time = Utils.GetEpochTime();
                byte countByte = IntToByte(count);
                switch (kind)
                {
                    case PlayerCardKind.All:
                        switch (styleRarity)
                        {
                            case CardStyleRarity.Normal: NumNormal = NumNormal_NoDismantle = countByte; break;
                            case CardStyleRarity.Shine: NumShine = NumShine_NoDismantle = countByte; break;
                            case CardStyleRarity.Royal: NumRoyal = NumRoyal_NoDismantle = countByte; break;
                        }
                        break;
                    case PlayerCardKind.Dismantle:
                        switch (styleRarity)
                        {
                            case CardStyleRarity.Normal: NumNormal = countByte; break;
                            case CardStyleRarity.Shine: NumShine = countByte; break;
                            case CardStyleRarity.Royal: NumRoyal = countByte; break;
                        }
                        break;
                    case PlayerCardKind.NoDismantle:
                        switch (styleRarity)
                        {
                            case CardStyleRarity.Normal: NumNormal_NoDismantle = countByte; break;
                            case CardStyleRarity.Shine: NumShine_NoDismantle = countByte; break;
                            case CardStyleRarity.Royal: NumRoyal_NoDismantle = countByte; break;
                        }
                        break;
                }
            }

            byte IntToByte(int count)
            {
                if (count < byte.MinValue)
                {
                    return 0;
                }
                if (count > byte.MaxValue)
                {
                    return byte.MaxValue;
                }
                return (byte)count;
            }
        }
    }

    enum PlayerCardKind
    {
        All,
        Dismantle,
        NoDismantle
    }

    class PlayerShopState
    {
        public Dictionary<int, PlayerShopItemState> ShopItems { get; private set; }// <shopid, PlayerShopItemState>
        public HashSet<int> UltraRareGuaranteedPacks { get; private set; }// <packid>

        public PlayerShopState()
        {
            ShopItems = new Dictionary<int, PlayerShopItemState>();
            UltraRareGuaranteedPacks = new HashSet<int>();
        }

        public bool IsUltraRareGuaranteed(int packId)
        {
            return UltraRareGuaranteedPacks.Contains(packId);
        }

        public void SetUltraRareGaurantee(int packId, bool newValue)
        {
            if (newValue)
            {
                UltraRareGuaranteedPacks.Add(packId);
            }
            else
            {
                UltraRareGuaranteedPacks.Remove(packId);
            }
        }

        PlayerShopItemState GetItemState(ShopItemInfo shopItem)
        {
            PlayerShopItemState state;
            if (!ShopItems.TryGetValue(shopItem.ShopId, out state))
            {
                ShopItems[shopItem.ShopId] = state = new PlayerShopItemState();
            }
            return state;
        }

        public bool ClearNew()
        {
            bool hadNew = false;
            foreach (PlayerShopItemState shopItemState in ShopItems.Values)
            {
                if (shopItemState.IsNew)
                {
                    hadNew = true;
                    shopItemState.IsNew = false;
                }
            }
            return hadNew;
        }

        public void New(ShopItemInfo shopItem)
        {
            PlayerShopItemState state = GetItemState(shopItem);
            state.IsNew = true;
        }

        public bool HasNew()
        {
            foreach (PlayerShopItemState state in ShopItems.Values)
            {
                if (state.IsNew)
                {
                    return true;
                }
            }
            return false;
        }

        public void Unlock(ShopItemInfo shopItem)
        {
            PlayerShopItemState state = GetItemState(shopItem);
            state.Unlocked = true;
            state.UnlockedTime = Utils.GetEpochTime();
            state.UnlockedPurchaseCount = 0;
        }

        public void Lock(ShopItemInfo shopItem)
        {
            PlayerShopItemState state = GetItemState(shopItem);
            state.Unlocked = false;
            state.UnlockedTime = 0;
            state.UnlockedPurchaseCount = 0;
        }

        public void Purchased(ShopItemInfo shopItem)
        {
            PlayerShopItemState state = GetItemState(shopItem);
            state.PurchaseCount++;
            if (shopItem.SecretType != ShopItemSecretType.None)
            {
                state.UnlockedPurchaseCount++;
            }
            if (shopItem.SecretBuyLimit > 0 && state.UnlockedPurchaseCount >= shopItem.SecretBuyLimit)
            {
                Lock(shopItem);
            }
        }

        public long GetPurchasedCount(ShopItemInfo shopItem)
        {
            PlayerShopItemState state = GetItemState(shopItem);
            return state != null ? state.PurchaseCount : 0;
        }

        public PlayerShopItemAvailability GetAvailability(ShopInfo shop, ShopItemInfo shopItem)
        {
            long buyLimit, expireTime;
            bool isNew;
            return GetAvailability(shop, shopItem, out buyLimit, out expireTime, out isNew);
        }

        public PlayerShopItemAvailability GetAvailability(ShopInfo shop, ShopItemInfo shopItem, out long buyLimit, out long expireTime, out bool isNew)
        {
            buyLimit = 0;
            expireTime = 0;
            isNew = false;
            if (shop.UnlockAllSecrets || shopItem.IsSpecialTime)
            {
                return PlayerShopItemAvailability.Available;
            }
            PlayerShopItemState state = GetItemState(shopItem);
            isNew = state.IsNew;
            if (shopItem.SecretType != ShopItemSecretType.None)
            {
                if (!state.Unlocked)
                {
                    return PlayerShopItemAvailability.Hidden;
                }
                if (shopItem.SecretDurationInSeconds > 0)
                {
                    expireTime = state.UnlockedTime + shopItem.SecretDurationInSeconds;
                    if (expireTime < Utils.GetEpochTime())
                    {
                        Lock(shopItem);
                        return PlayerShopItemAvailability.Hidden;
                    }
                }
            }
            if (shopItem.SecretType != ShopItemSecretType.None && shopItem.SecretBuyLimit > 0)
            {
                if (state.UnlockedPurchaseCount >= shopItem.SecretBuyLimit)
                {
                    return PlayerShopItemAvailability.Hidden;
                }
                buyLimit = shopItem.SecretBuyLimit - state.UnlockedPurchaseCount;
            }
            if (shopItem.Buylimit > 0)
            {
                if (state.PurchaseCount >= shopItem.Buylimit)
                {
                    return PlayerShopItemAvailability.Purchased;
                }
                if (buyLimit == 0 || shopItem.Buylimit - state.PurchaseCount < buyLimit)
                {
                    buyLimit = shopItem.Buylimit - state.PurchaseCount;
                }
            }
            return PlayerShopItemAvailability.Available;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["UltraRareGuaranteedPacks"] = UltraRareGuaranteedPacks.ToArray();
            Dictionary<string, object> shopItemsData = new Dictionary<string, object>();
            foreach (KeyValuePair<int, PlayerShopItemState> shopItemState in ShopItems)
            {
                if (shopItemState.Value.IsDefault)
                {
                    continue;
                }
                shopItemsData[shopItemState.Key.ToString()] = new Dictionary<string, object>()
                {
                    { "Unlocked", shopItemState.Value.Unlocked },
                    { "UnlockedTime", shopItemState.Value.UnlockedTime },
                    { "IsNew", shopItemState.Value.IsNew },
                    { "PurchaseCount", shopItemState.Value.PurchaseCount },
                    { "UnlockedPurchaseCount", shopItemState.Value.UnlockedPurchaseCount },
                };
            }
            result["ShopItems"] = shopItemsData;
            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            ShopItems.Clear();
            UltraRareGuaranteedPacks.Clear();
            if (data == null)
            {
                return;
            }
            List<object> guaranteesObj = Utils.GetValue(data, "UltraRareGuaranteedPacks", default(List<object>));
            if (guaranteesObj != null)
            {
                foreach (object obj in guaranteesObj)
                {
                    UltraRareGuaranteedPacks.Add((int)Convert.ChangeType(obj, typeof(int)));
                }
            }
            Dictionary<string, object> shops = Utils.GetValue(data, "ShopItems", default(Dictionary<string, object>));
            if (shops != null)
            {
                foreach (KeyValuePair<string, object> shop in shops)
                {
                    int shopId;
                    Dictionary<string, object> stateData = shop.Value as Dictionary<string, object>;
                    if (int.TryParse(shop.Key, out shopId) && stateData != null)
                    {
                        PlayerShopItemState state = new PlayerShopItemState();
                        state.Unlocked = Utils.GetValue<bool>(stateData, "Unlocked");
                        state.UnlockedTime = Utils.GetValue<long>(stateData, "UnlockedTime");
                        state.IsNew = Utils.GetValue<bool>(stateData, "IsNew");
                        state.PurchaseCount = Utils.GetValue<long>(stateData, "PurchaseCount");
                        state.UnlockedPurchaseCount = Utils.GetValue<long>(stateData, "UnlockedPurchaseCount");
                        ShopItems[shopId] = state;
                    }
                }
            }
        }
    }

    enum PlayerShopItemAvailability
    {
        Available,
        Purchased,
        Hidden,
    }

    class PlayerShopItemState
    {
        public bool Unlocked;
        public long UnlockedTime;
        public bool IsNew;
        /// <summary>
        /// Total number of times this item has been purchased
        /// </summary>
        public long PurchaseCount;
        /// <summary>
        /// Number of times this item has been purchased after being unlocked (resets every time it's unlocked)
        /// </summary>
        public long UnlockedPurchaseCount;

        public bool IsDefault
        {
            get { return !Unlocked && UnlockedTime == 0 && !IsNew && PurchaseCount == 0 && UnlockedPurchaseCount == 0; }
        }
    }

    class PlayerDuelState
    {
        Player player;
        public GameMode Mode;
        public Dictionary<GameMode, int> SelectedDeck { get; private set; }// <GameMode, deckid>

        // For solo/story...
        public int ChapterId;
        public bool IsMyDeck;

        public PlayerDuelState(Player player)
        {
            this.player = player;
            SelectedDeck = new Dictionary<GameMode, int>();
        }

        public DeckInfo GetDeck(GameMode mode)
        {
            int deckId;
            if (SelectedDeck.TryGetValue(mode, out deckId))
            {
                DeckInfo deck;
                if (player.Decks.TryGetValue(deckId, out deck))
                {
                    return deck;
                }
            }
            return null;
        }

        public int GetDeckId(GameMode mode)
        {
            int result;
            SelectedDeck.TryGetValue(mode, out result);
            return result;
        }

        public void SetDeckId(GameMode mode, int deckId)
        {
            SelectedDeck[mode] = deckId;
        }

        public void SelectedDeckFromDictionary(Dictionary<string, object> data)
        {
            SelectedDeck.Clear();
            if (data == null)
            {
                return;
            }
            Dictionary<string, DeckInfo> decksByFileName = new Dictionary<string, DeckInfo>();
            foreach (DeckInfo deck in player.Decks.Values)
            {
                if (!string.IsNullOrEmpty(deck.File))
                {
                    string name = Path.GetFileName(deck.File);
                    decksByFileName[name] = deck;
                }
            }
            foreach (KeyValuePair<string, object> selectedDeck in data)
            {
                GameMode mode;
                DeckInfo deck;
                string file = selectedDeck.Value as string;
                if (Enum.TryParse(selectedDeck.Key, out mode) && !string.IsNullOrEmpty(file) && decksByFileName.TryGetValue(file, out deck))
                {
                    SelectedDeck[mode] = deck.Id;
                }
            }
        }

        public Dictionary<string, object> SelectedDeckToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<GameMode, int> selectedDeck in SelectedDeck)
            {
                DeckInfo deck = GetDeck(selectedDeck.Key);
                if (deck != null && !string.IsNullOrEmpty(deck.File))
                {
                    result[selectedDeck.Key.ToString()] = Path.GetFileName(deck.File);
                }
            }
            return result;
        }
    }
}
