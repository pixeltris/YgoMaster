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
        public long DefaultSecretDuration;
        public bool PutAllCardsInStandrdPack;

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
            get { return (int)Category * 10000000; }
        }
        /// <summary>
        /// The shop tab
        /// </summary>
        public ShopCategory Category;
        /// <summary>
        /// The shop sub tab (on left of screen)
        /// </summary>
        public int SubCategory;

        public string NameText;
        public string DescShortText;
        public string DescFullText;
        public int IconMrk;
        public int IconType;
        public string IconData;
        public string Preview;
        public HashSet<int> SearchCategory { get; private set; }
        public List<ShopItemPrice> Prices { get; private set; }

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

        /// <summary>
        /// A fixed date/time for the shop item to expire
        /// </summary>
        public long ExpireDateTime;

        // Card pack info
        public ShopPackType PackType;
        public HashSet<int> Cards { get; private set; }
        public int CardNum;
        public int Power;
        public int Flexibility;
        public int Difficulty;
        /// <summary>
        /// The name of the odds to use. If null it will fall back to the pack type odds.
        /// </summary>
        public string OddsName;

        public ShopItemInfo()
        {
            Prices = new List<ShopItemPrice>();
            Cards = new HashSet<int>();
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
    }

    class ShopItemPrice
    {
        public int Id;
        public int Price;
        public string TextId;
        public List<int> TextArgs;
        public int ButtonType;
        /// <summary>
        /// Additional note (i.e. "At least 1 Super Rare guaranteed in 10 Packs")
        /// </summary>
        public string POP;

        public ShopItemPrice()
        {
            TextArgs = new List<int>();
        }
    }

    enum ShopItemSecretType
    {
        None,
        /// <summary>
        /// Unlocked by finding (or crafting) a card from the pack
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
        /// Unlocked in other ways (TODO)
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
    /// YgomGame.Shop.AccessorySubCategory
    /// </summary>
    enum ShopSubCategoryAccessory
    {
        None,
        Mate,
        Field,
        Protector,
        Icon
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
