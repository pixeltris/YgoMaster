using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    class ShopInfo
    {
        public Dictionary<string, ShopOddsInfo> PackOddsByName { get; private set; }
        public Dictionary<ShopPackType, ShopOddsInfo> PackOddsByPackType { get; private set; }
        public ShopOddsVisualsSettings PackOddsVisuals { get; private set; }
        public Dictionary<int, string> PackShopImagesByCardId { get; private set; }
        public Dictionary<int, ShopItemInfo> PackShop { get; private set; }
        public Dictionary<int, ShopItemInfo> StructureShop { get; private set; }
        public Dictionary<int, ShopItemInfo> AccessoryShop { get; private set; }
        public Dictionary<int, ShopItemInfo> SpecialShop { get; private set; }
        public Dictionary<int, ShopItemInfo> AllShops { get; private set; }
        public Dictionary<int, ShopItemInfo> PacksByPackId { get; private set; }
        public Dictionary<int, List<ShopItemInfo>> SecretPacksByCardId { get; private set; }
        
        public ShopItemInfo StandardPack;

        /// <summary>
        /// Put all cards in the main standard pack
        /// </summary>
        public bool PutAllCardsInStandardPack;

        /// <summary>
        /// Use rarities as defined on individual packs (or the default rarities if not defined)
        /// This only applies to rarities in the shop. When you leave the shop the regular rarities will be used.
        /// </summary>
        public bool PerPackRarities;

        /// <summary>
        /// Disable card style rarity (shine / royal) on cards obtained from the shop
        /// </summary>
        public bool DisableCardStyleRarity;

        /// <summary>
        /// Unlock all secret packs by default
        /// </summary>
        public bool UnlockAllSecrets;

        /// <summary>
        /// Disable the ultra rare guarantee (which is determined based on whether the previous 10 pack didn't have an UR)
        /// </summary>
        public bool DisableUltraRareGuarantee;

        /// <summary>
        /// Upgrade card rarity if the given card rarity is not found (and no lower rarities are found).
        /// If this occurs and this is false no card will be given.
        /// </summary>
        public bool UpgradeRarityWhenNotFound;

        /// <summary>
        /// Don't show duplicate cards in an individual pack
        /// TODO: Add another option to greatly reduce the chance of duplicates instead
        /// </summary>
        public bool NoDuplicatesPerPack;

        // Default = the price if prices aren't defined
        // Override = an override to replace all existing prices

        public long DefaultSecretDuration;
        public long OverrideSecretDuration;

        public int DefaultPackPrice;
        public int DefaultPackPriceX10;
        public int OverridePackPrice;
        public int OverridePackPriceX10;
        public int DefaultPackCardNum;
        public int OverridePackCardNum;

        public int DefaultStructureDeckPrice;
        public int OverrideStructureDeckPrice;

        public int DefaultUnlockSecretsAtPercent;
        public int OverrideUnlockSecretsAtPercent;

        public ShopInfo()
        {
            PackOddsByName = new Dictionary<string, ShopOddsInfo>();
            PackOddsByPackType = new Dictionary<ShopPackType, ShopOddsInfo>();
            PackOddsVisuals = new ShopOddsVisualsSettings();
            PackShopImagesByCardId = new Dictionary<int,string>();
            PackShop = new Dictionary<int, ShopItemInfo>();
            StructureShop = new Dictionary<int, ShopItemInfo>();
            AccessoryShop = new Dictionary<int, ShopItemInfo>();
            SpecialShop = new Dictionary<int, ShopItemInfo>();
            AllShops = new Dictionary<int, ShopItemInfo>();
            PacksByPackId = new Dictionary<int, ShopItemInfo>();
            SecretPacksByCardId = new Dictionary<int, List<ShopItemInfo>>();
        }

        public static int GetBaseShopId(ShopCategory category)
        {
            return (int)category * 10000000;
        }
    }

    class ShopItemInfo
    {
        public int Id;
        public int ShopId
        {
            get { return BaseShopId + Id; }
        }
        public int BaseShopId
        {
            get { return ShopInfo.GetBaseShopId(Category); }
        }
        /// <summary>
        /// The shop tab
        /// </summary>
        public ShopCategory Category;
        /// <summary>
        /// The shop sub tab (on left of screen)
        /// See YgomGame.Shop.ShopDef - AccessorySubCategory, PackSubCategory, ProductCategory, SpecialSubCategory, StructureSubCategory
        /// </summary>
        public int SubCategory;

        public string NameText;
        public string DescShortText;
        public string DescFullText;
        public bool DescTextGenerated;// If true generate DescShortText/DescFullText based on pack info (card count, cards obtained)
        public int IconMrk;
        public ShopItemIconType IconType;
        public string IconData;
        public string Preview;
        public HashSet<int> SearchCategory { get; private set; }
        public List<ShopItemPrice> Prices { get; private set; }

        /// <summary>
        /// Release date used for IRL sets
        /// </summary>
        public DateTime ReleaseDate;

        /// <summary>
        /// Unlockable shop item which is hidden by default
        /// </summary>
        public ShopItemSecretType SecretType;
        /// <summary>
        /// The duration of the unlockable shop item
        /// </summary>
        public long SecretDurationInSeconds;
        /// <summary>
        /// Number of times you can buy it, after being depleted it'll become locked again (applicable only to card packs, 0 means unlimited)
        /// </summary>
        public int SecretBuyLimit;
        /// <summary>
        /// Number of times this can be bought (applicable only to card packs, 0 means unlimited)
        /// </summary>
        public int Buylimit;

        // Card pack info
        public ShopPackType PackType;
        public Dictionary<int, CardRarity> Cards { get; private set; }// CardRarity is only used on official sets (to support custom rarities per-pack)
        public int CardNum;
        public int Power;
        public int Flexibility;
        public int Difficulty;
        /// <summary>
        /// The name of the odds to use. If null it will fall back to the pack type odds.
        /// </summary>
        public string OddsName;

        /// <summary>
        /// Special time secret packs like "Alba Abyss"
        /// </summary>
        public bool IsSpecialTime;

        /// <summary>
        /// The image to use when opening the pack (e.g. "CardPackTex01_0000" / "CardPackTex03_4041")
        /// This is usually "CardPackTexXX_YYYY" where XX is is the pack type (standard/selection/secret/bonus) and YYYY is the card id on the pack)
        /// </summary>
        public string PackImageName;

        /// <summary>
        /// The secret packs to unlock once a certain percent of this pack has been obtained (a list of pack ids)
        /// </summary>
        public List<int> UnlockSecrets { get; private set; }
        /// <summary>
        /// The percent of completion at which to unlock linked secret packs
        /// </summary>
        public double UnlockSecretsAtPercent;

        /// <summary>
        /// A list of shop other ids which should have an incremented purchase count when this shop item is purchased
        /// </summary>
        public List<int> SetPurchased { get; private set; }

        public ShopItemInfo()
        {
            SearchCategory = new HashSet<int>();
            Prices = new List<ShopItemPrice>();
            Cards = new Dictionary<int, CardRarity>();
            UnlockSecrets = new List<int>();
            SetPurchased = new List<int>();
        }

        public ShopOddsInfo GetOdds(ShopInfo shop)
        {
            ShopOddsInfo odds = null;
            if (!string.IsNullOrEmpty(OddsName))
            {
                shop.PackOddsByName.TryGetValue(OddsName, out odds);
            }
            if (odds == null)
            {
                shop.PackOddsByPackType.TryGetValue(PackType, out odds);
            }
            return odds;
        }

        /// <summary>
        /// Returns the card rarity guarantee of the final card based on the pack odds
        /// (this doesn't take into account any additional player guarantee such as the UR one)
        /// </summary>
        public CardRarity GetFinalCardRarityGuarantee(ShopOddsInfo odds)
        {
            int num = CardNum;
            foreach (ShopOddsRarity item in odds.CardRateList.OrderBy(x => x.GuaranteeRareMin))
            {
                if (num >= item.StartNum && num <= item.EndNum)
                {
                    if (item.GuaranteeRareMin > 0 || item.GuaranteeRareMax > 0)
                    {
                        return (CardRarity)item.GuaranteeRareMin;
                    }
                }
            }
            return CardRarity.Normal;
        }

        public double GetPercentComplete(Player player)
        {
            int numObtained;
            return GetPercentComplete(player, out numObtained);
        }

        public double GetPercentComplete(Player player, out int numObtained)
        {
            numObtained = 0;
            foreach (int cardId in Cards.Keys)
            {
                if (player.Cards.GetCount(cardId) > 0)
                {
                    numObtained++;
                }
            }
            return ((double)numObtained / (double)Cards.Count) * 100.0;
        }

        public List<ShopItemInfo> DoUnlockSecrets(Player player, ShopInfo shop)
        {
            List<ShopItemInfo> result = null;
            if (UnlockSecrets.Count > 0 && UnlockSecretsAtPercent > 0)
            {
                bool isComplete = false;
                foreach (int id in UnlockSecrets)
                {
                    ShopItemInfo shopItem;
                    if (shop.PacksByPackId.TryGetValue(id, out shopItem))
                    {
                        if (player.ShopState.GetAvailability(shop, shopItem) == PlayerShopItemAvailability.Hidden)
                        {
                            if (!isComplete)
                            {
                                if (GetPercentComplete(player) < UnlockSecretsAtPercent)
                                {
                                    return null;
                                }
                                result = new List<ShopItemInfo>();
                                isComplete = true;
                            }
                            result.Add(shopItem);
                            player.ShopState.Unlock(shopItem);
                            player.ShopState.New(shopItem);
                        }
                    }
                }
            }
            if (result != null)
            {
                // Handle shop unlock chains. NOTE: This will result in a stackoverflow if you have a circular reference
                foreach (ShopItemInfo item in new List<ShopItemInfo>(result))
                {
                    List<ShopItemInfo> additionalItems = item.DoUnlockSecrets(player, shop);
                    if (additionalItems != null)
                    {
                        result.AddRange(additionalItems);
                    }
                }
            }
            return result;
        }

        public bool HasUnlockedAllSecrets(Player player, ShopInfo shop)
        {
            foreach (int id in UnlockSecrets)
            {
                ShopItemInfo shopItem;
                if (shop.PacksByPackId.TryGetValue(id, out shopItem))
                {
                    if (player.ShopState.GetAvailability(shop, shopItem) == PlayerShopItemAvailability.Hidden)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Dictionary<CardRarity, int> GetNumCardsByRarity(Dictionary<int, int> cardRare)
        {
            Dictionary<CardRarity, int> result = new Dictionary<CardRarity, int>();
            result[CardRarity.Normal] = 0;
            result[CardRarity.Rare] = 0;
            result[CardRarity.SuperRare] = 0;
            result[CardRarity.UltraRare] = 0;
            foreach (int cardId in Cards.Keys)
            {
                // Assumes cardRare has the correct rarities filled out
                int rarity;
                if (cardRare.TryGetValue(cardId, out rarity))
                {
                    result[(CardRarity)rarity]++;
                }
            }
            return result;
        }
    }

    class ShopItemPrice
    {
        public int Id;
        public int Price;
        public int ItemAmount;

        public ShopItemPrice()
        {
            ItemAmount = 1;
        }
    }

    /// <summary>
    /// YgomGame.Shop.ShopDef.ListButtonType
    /// 
    /// This is the background style of the item price in the main store (e.g. "1 Pack X 100" has a background of gray (Default))
    /// </summary>
    enum ShopItemListButtonType
    {
        Default,
        Highlight,

        // Everything at this point and beyond is invalid (sets the background to none)
        Invalid
    }

    /// <summary>
    /// The button style of the given price
    /// </summary>
    enum ShopItemPriceButtonType
    {
        None,// (actually blue, but most values in MD start from 1)

        /// <summary>
        /// Normal
        /// </summary>
        Blue,
        /// <summary>
        /// Super rare guaranteed
        /// </summary>
        Yellow,
        /// <summary>
        /// Ultra rare guaranteed
        /// </summary>
        Pink,// Pink/purple-ish
    }

    /// <summary>
    /// YgomGame.Shop.ShopDef.HighlightType
    /// </summary>
    enum ShopItemIconType
    {
        None,
        CardThumb,
        ItemThumb,
        WideThumb,
        PlayThumb
    }

    enum ShopItemSecretType
    {
        None,
        /// <summary>
        /// Unlocked by finding or crafting a card from the pack
        /// </summary>
        FindOrCraft,
        /// <summary>
        /// Unlocked by finding a card from the pack
        /// </summary>
        Find,
        /// <summary>
        /// Unlocked by crafting a card from the pack
        /// </summary>
        Craft,
        /// <summary>
        /// Unlocked in other ways. For example:
        /// - Obtaining a certain percentage of unique cards from another pack
        /// - Completing specific solo chapters (one issue is there wouldn't be a visual indication unless using the gate clear stuff)
        /// NOTE: This is implemented on a case by case basis (the requirements for the unlock don't exist on the given pack itself)
        /// NOTE: A pack which is "Find", "Craft", "FindOrCraft" can also implicitly be "Other" (i.e. have multiple ways to unlock)
        /// </summary>
        Other
    }

    class ShopOddsInfo
    {
        public string Name;
        public HashSet<ShopPackType> PackTypes { get; private set; }
        public List<ShopOddsRarity> CardRateList { get; private set; }
        /// <summary>
        /// premiereRateList
        /// </summary>
        public List<ShopOddsStyleRarity> CardStyleRarityRateList { get; private set; }

        public ShopOddsInfo()
        {
            PackTypes = new HashSet<ShopPackType>();
            CardRateList = new List<ShopOddsRarity>();
            CardStyleRarityRateList = new List<ShopOddsStyleRarity>();
        }

        public ShopOddsStyleRarity GetCardStyleRarity(CardRarity rarity)
        {
            foreach (ShopOddsStyleRarity item in CardStyleRarityRateList)
            {
                if (item.Rarities.Contains(rarity))
                {
                    return item;
                }
            }
            return null;
        }
    }

    class ShopOddsRarity
    {
        public int StartNum;
        public int EndNum;
        public bool Standard;
        public CardRarity GuaranteeRareMin;
        public CardRarity GuaranteeRareMax;
        public Dictionary<CardRarity, double> Rate { get; private set; }

        public ShopOddsRarity()
        {
            Rate = new Dictionary<CardRarity, double>();
        }
    }

    class ShopOddsStyleRarity
    {
        /// <summary>
        /// The rarities these rates are applicable to
        /// </summary>
        public List<CardRarity> Rarities { get; private set; }
        /// <summary>
        /// The rates for each given CardStyleRarity
        /// </summary>
        public Dictionary<CardStyleRarity, double> Rate { get; private set; }

        public ShopOddsStyleRarity()
        {
            Rarities = new List<CardRarity>();
            Rate = new Dictionary<CardStyleRarity, double>();
        }
    }

    class ShopOddsVisualsSettings
    {
        // TODO: Add more configuration options (ability to configure everything and randomness of things)
        public bool RarityJebait;
        public bool RarityOnCardBack;
        public bool RarityOnPack;
    }

    /// <summary>
    /// YgomGame.Shop.ProductCategory
    /// </summary>
    enum ShopCategory
    {
        None,
        Pack,
        Structure,
        Accessory,
        Special
    }

    /// <summary>
    /// YgomGame.Shop.AccessorySubCategory - removed v1.3.1?
    /// </summary>
    enum ShopSubCategoryAccessory
    {
        None,
        Mate,
        Field,
        Protector,
        Icon,
        Wallpaper,
        DeckCase
    }

    /// <summary>
    /// YgomGame.CardPack.CardPackDef.PackType
    /// </summary>
    enum ShopPackType
    {
        None,
        /// <summary>
        /// Master packs contain a large pool of cards and aren't time limited.
        /// These packs are listed with the lowest UI order priorty.
        /// </summary>
        Standard,
        /// <summary>
        /// Selection packs are time limited (available for 60 days).
        /// They have a special "Selection Pack" banner.
        /// </summary>
        Selection,
        /// <summary>
        /// Secret packs are unlocked after opening key cards from other packs.
        /// They are available for 24-hours, but can be unlocked again after that time period.
        /// They have additional rating info "Power", "Technical", "Hold the Line".
        /// </summary>
        Secret,
        /// <summary>
        /// Additional packs (to be used under the Bonus Pack sub category)
        /// </summary>
        Bonus
    }
}
