using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;

namespace YgoMaster
{
    /// <summary>
    /// Holds set information (packs, boosters, decks, etc) obtained from the official DB
    /// </summary>
    class YgoDbSet
    {
        /// <summary>
        /// The id of the set as defined in the yugioh database website
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The name of the set
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The release date of the set
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// The cards in the set
        /// </summary>
        public Dictionary<int, CardRarity> Cards { get; private set; }

        /// <summary>
        /// The type of set
        /// </summary>
        public YgoDbSetType Type { get; set; }

        public YgoDbSet()
        {
            Cards = new Dictionary<int, CardRarity>();
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<int> ids = new List<int>();
            List<int> rarities = new List<int>();
            foreach (KeyValuePair<int, CardRarity> card in Cards)
            {
                ids.Add(card.Key);
                rarities.Add((int)card.Value);
            }
            result["id"] = Id;
            result["name"] = Name;
            result["date"] = Utils.GetEpochTime(ReleaseDate);
            result["type"] = (int)Type;
            result["ids"] = ids;
            result["r"] = rarities;
            return result;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            Cards.Clear();
            Id = Utils.GetValue<long>(data, "id");
            Name = Utils.GetValue<string>(data, "name");
            ReleaseDate = Utils.ConvertEpochTime(Utils.GetValue<long>(data, "date"));
            Type = Utils.GetValue<YgoDbSetType>(data, "type");
            List<object> ids = Utils.GetValue<List<object>>(data, "ids");
            List<object> rarities = Utils.GetValue<List<object>>(data, "r");
            for (int i = 0; i < ids.Count && i < rarities.Count; i++)
            {
                Cards[(int)Convert.ChangeType(ids[i], typeof(int))] = (CardRarity)(int)Convert.ChangeType(rarities[i], typeof(int));
            }
        }

        public static YgoMaster.CardRarity ConvertRarity(YgoDbSet.CardRarity rarity)
        {
            switch (rarity)
            {
                default:
                case CardRarity.Common:
                    return YgoMaster.CardRarity.Normal;
                case CardRarity.Rare:
                case CardRarity.HobbyCommon:
                    return YgoMaster.CardRarity.Rare;
                case CardRarity.SuperRare:
                    return YgoMaster.CardRarity.SuperRare;
                case CardRarity.UltraRare:
                case CardRarity.SecretRare:
                case CardRarity.GoldRare:
                case CardRarity.GoldSecret:
                case CardRarity.UltimateRare:
                case CardRarity.GhostRare:
                case CardRarity.PlatinumSecretRare:
                case CardRarity.PharaohUltraRare:
                case CardRarity.PrismaticSecretRare:
                case CardRarity.SecretRare10k:
                case CardRarity.PremiumGoldRare:
                    return YgoMaster.CardRarity.UltraRare;
            }
        }

        public enum CardRarity
        {
            Unknown,
            Common,
            Rare,
            SuperRare,
            UltraRare,
            SecretRare,
            GoldRare,
            GoldSecret,
            UltimateRare,
            GhostRare,// HolographicRare on OCG
            PlatinumSecretRare,
            Shatterfoil,
            Starfoil,

            HobbyCommon,//? same as HobbyRare?
            // These don't appear on the EN version of the site (ja only)
            HobbyRare,
            HobbySuperRare,
            HobbyUltraRare,

            // These don't appear on the EN version of the site (ja only)
            HolographicRare,// GhostRare on TCG
            CollectorsRare,
            ExtraSecretRare,
            ParallelRare,
            ParallelSuperRare,
            ParallelSecretRare,
            ParallelExtraSecretRare,
            ParallelUltraRare,
            MillenniumCommon,//? same as MillenniumRare?
            MillenniumRare,
            MillenniumSuperRare,
            MillenniumSecretRare,
            MillenniumUltraRare,
            MillenniumGoldRare,
            KaibaCorporationCommon,
            KaibaCorporationRare,
            KaibaCorporationSuperRare,
            KaibaCorporationSecretRare,
            KaibaCorporationUltraRare,

            // New (relative to when this code was written)
            PharaohUltraRare,// "Ultra Rare (Pharaoh's Rare)" - King's Court (2000001121000)
            PrismaticSecretRare,// Lightning Overdrive (2000001115000)
            SecretRare10k,// Battles of Legend: Armageddon (11119005)
            PremiumGoldRare,// Maximum Gold: El Dorado (2000001137000)
        }
    }

    /// <summary>
    /// The type of set (NOTE: Anything with duplicates will only include 1 of that card)
    /// </summary>
    public enum YgoDbSetType
    {
        Unknown,

        // Boosters / packs
        BoosterPack,
        DuelistPack,

        // Decks
        StructureDeck,
        StarterDeck,

        // Tins / boxes
        SpecialEditionBox,
        Tin,

        // Misc
        Misc,
        DuelTerminal,
        VideoGameBundle,
        Tournament,
        Promotional,
        MagazineBooksComics,
        SpeedDuel
    }

    /// <summary>
    /// Manages a collection of sets (packs, boosters, decks, etc)
    /// </summary>
    class YgoDbSetCollection
    {
        public Dictionary<long, YgoDbSet> Sets { get; private set; }

        public YgoDbSet this[long id]
        {
            get
            {
                YgoDbSet result;
                Sets.TryGetValue(id, out result);
                return result;
            }
            set
            {
                Sets[id] = value;
            }
        }

        public YgoDbSetCollection()
        {
            Sets = new Dictionary<long, YgoDbSet>();
        }

        static YgoDbSetCollection()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;//Tls12
        }

        public void Load(string file)
        {
            Sets.Clear();
            if (!File.Exists(file))
            {
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            List<object> sets = Utils.GetValue(data, "sets", default(List<object>));
            if (sets != null)
            {
                foreach (object setObj in sets)
                {
                    Dictionary<string, object> setData = setObj as Dictionary<string, object>;
                    if (setData == null)
                    {
                        continue;
                    }
                    YgoDbSet set = new YgoDbSet();
                    set.FromDictionary(setData);
                    Sets[set.Id] = set;
                }
            }
        }

        public void Save(string file)
        {
            List<object> sets = new List<object>();
            foreach (YgoDbSet set in Sets.Values)
            {
                sets.Add(set.ToDictionary());
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sets"] = sets;
            File.WriteAllText(file, MiniJSON.Json.Serialize(data));
        }

        public void DownloadSets(string outputDir, string locale = "en")
        {
            string baseUrl = "https://www.db.yugioh-card.com/yugiohdb";

            // Get the sets page
            string setsListFile = Path.Combine(outputDir, locale + "_sets");
            if (!File.Exists(setsListFile))
            {
                string url = baseUrl + "/card_list.action?request_locale=" + locale;
                string html = Utils.DownloadString(url);
                if (string.IsNullOrEmpty(html))
                {
                    throw new Exception("Failed to load sets url " + url);
                }
                if (!html.Contains("class=\"list_title"))
                {
                    throw new Exception("Invalid sets page");
                }
                File.WriteAllText(setsListFile, html);
            }

            Dictionary<long, string> setIdNames = new Dictionary<long, string>();
            Dictionary<long, YgoDbSetType> setIdTypes = new Dictionary<long, YgoDbSetType>();

            // Get the sets pages
            if (File.Exists(setsListFile))
            {
                string html = File.ReadAllText(setsListFile);
                List<int> listIndexes = new List<int>();
                int listIndex = 0;
                while ((listIndex = html.IndexOf("class=\"list_title", listIndex)) >= 0)
                {
                    int nextListIndex = html.IndexOf("class=\"list_title", listIndex + 1);

                    string setTypeStr = Utils.FindFirstContentBetween(html, listIndex, nextListIndex, "<tr class=\"pack_l", "</tr>");
                    setTypeStr = Utils.FindAllContentBetween(setTypeStr, 0, int.MaxValue, "<th>", "</th>")[1];
                    YgoDbSetType setType = YgoDbSetType.Unknown;

                    switch (setTypeStr.ToLower())
                    {
                        case "booster packs":
                            setType = YgoDbSetType.BoosterPack;
                            break;
                        case "special edition boxes":
                            setType = YgoDbSetType.SpecialEditionBox;
                            break;
                        case "starter decks":
                            setType = YgoDbSetType.StarterDeck;
                            break;
                        case "structure decks":
                            setType = YgoDbSetType.StructureDeck;
                            break;
                        case "tins":
                            setType = YgoDbSetType.Tin;
                            break;
                        case "duelist packs":
                            setType = YgoDbSetType.DuelistPack;
                            break;
                        case "duel terminal cards":
                        case "duelist terminal cards":
                            setType = YgoDbSetType.DuelTerminal;
                            break;
                        case "others":
                            setType = YgoDbSetType.Misc;
                            break;
                        case "magazines, books, comics":
                            setType = YgoDbSetType.MagazineBooksComics;
                            break;
                        case "tournaments":
                            setType = YgoDbSetType.Tournament;
                            break;
                        case "promotional cards":
                            setType = YgoDbSetType.Promotional;
                            break;
                        case "video game bundles":
                            setType = YgoDbSetType.VideoGameBundle;
                            break;
                        case "speed duel":
                            setType = YgoDbSetType.SpeedDuel;
                            break;
                        default:
                            throw new NotImplementedException("Unhandled set type " + setTypeStr);
                    }

                    string[] sets = Utils.FindAllContentBetween(html, listIndex, nextListIndex, "<div class=\"pack", "</div>");

                    for (int i = 0; i < sets.Length; i++)
                    {
                        if (sets[i].StartsWith("_m"))
                        {
                            // Skip "pack_m" class
                            continue;
                        }

                        string setName = Utils.FindFirstContentBetween(sets[i], 0, int.MaxValue, "<strong>", "</strong>");
                        int pidIndex = sets[i].IndexOf("&pid=") + 5;
                        int pidEndIndex = sets[i].IndexOfAny(new char[] { ' ', '&', '\"' }, pidIndex);
                        long setId = long.Parse(sets[i].Substring(pidIndex, pidEndIndex - pidIndex));

                        setIdNames[setId] = setName;
                        setIdTypes[setId] = setType;

                        string filename = Path.Combine(outputDir, locale + "_" + setId.ToString());

                        if (!File.Exists(filename))
                        {
                            string url = baseUrl + "/card_search.action?ope=1&rp=99999&pid=" + setId + "&request_locale=" + locale;
                            string setHtml = Utils.DownloadString(url);
                            if (string.IsNullOrEmpty(setHtml))
                            {
                                throw new Exception("Failed to load sets url " + url);
                            }
                            if (!setHtml.Contains("box_list"))
                            {
                                throw new Exception("Invalid sets page '" + url + "'");
                            }
                            File.WriteAllText(filename, setHtml);
                        }
                    }

                    listIndex++;
                }
            }

            // Parse the sets pages
            foreach (KeyValuePair<long, string> setIdName in setIdNames)
            {
                string setFile = Path.Combine(outputDir, locale + "_" + setIdName.Key);
                if (File.Exists(setFile))
                {
                    string html = File.ReadAllText(setFile);

                    YgoDbSet setInfo = new YgoDbSet();
                    setInfo.Id = setIdName.Key;
                    setInfo.Type = setIdTypes[setIdName.Key];
                    setInfo.Name = setIdName.Value;
                    Sets[setInfo.Id] = setInfo;

                    // Get the release date
                    string releaseDateStr = Utils.FindFirstContentBetween(html, 0, int.MaxValue, "id=\"previewed\">", "</p>");
                    if (string.IsNullOrEmpty(releaseDateStr))
                    {
                        switch (setInfo.Type)
                        {
                            case YgoDbSetType.MagazineBooksComics:
                                // Magazine / books / comics uses varying date formats
                                if (setInfo.Name.TrimEnd().EndsWith("-"))
                                {
                                    // Magazine / books / comics includes the date in the title
                                    //"SHONEN JUMP Promotional Card - April 2012 -"
                                    string nameTrimmed = setInfo.Name.TrimEnd('-', ' ');
                                    int lastDashIndex = nameTrimmed.LastIndexOf('-');
                                    int lastSlashIndex = nameTrimmed.LastIndexOf('/');
                                    if (lastSlashIndex > lastDashIndex)
                                    {
                                        lastDashIndex = lastSlashIndex;
                                    }
                                    if (lastDashIndex > 0)
                                    {
                                        setInfo.Name = nameTrimmed.Substring(0, lastDashIndex).Trim();
                                        setInfo.ReleaseDate = DateTime.Parse(nameTrimmed.Substring(lastDashIndex + 1));
                                    }
                                }
                                break;
                            case YgoDbSetType.Tournament:
                                // Some tournament packs don't have a release date specified
                                break;
                            default:
                                Console.WriteLine("Release date null for " + setIdName.Key + " type:" + setInfo.Type);
                                break;
                        }
                    }
                    else
                    {
                        string[] releaseDateNumbers = new string[3];
                        int releaseDateNumberIndex = -1;
                        bool hasReleaseDateNumber = false;
                        for (int i = 0; i < releaseDateStr.Length; i++)
                        {
                            if (char.IsNumber(releaseDateStr[i]))
                            {
                                if (!hasReleaseDateNumber)
                                {
                                    releaseDateNumberIndex++;
                                    hasReleaseDateNumber = true;
                                    releaseDateNumbers[releaseDateNumberIndex] = string.Empty;
                                }
                                releaseDateNumbers[releaseDateNumberIndex] += releaseDateStr[i];
                            }
                            else
                            {
                                hasReleaseDateNumber = false;
                            }
                        }
                        switch (locale)
                        {
                            case "ja":
                                setInfo.ReleaseDate = new DateTime(
                                    int.Parse(releaseDateNumbers[0]),//year
                                    int.Parse(releaseDateNumbers[1]),//month
                                    int.Parse(releaseDateNumbers[2]));//day
                                break;
                            case "en":
                                setInfo.ReleaseDate = new DateTime(
                                    int.Parse(releaseDateNumbers[2]),//year
                                    int.Parse(releaseDateNumbers[0]),//month
                                    int.Parse(releaseDateNumbers[1]));//day
                                break;
                            case "de":
                            case "fr":
                            case "it":
                            case "es":
                                setInfo.ReleaseDate = new DateTime(
                                    int.Parse(releaseDateNumbers[2]),//year
                                    int.Parse(releaseDateNumbers[1]),//month
                                    int.Parse(releaseDateNumbers[0]));//day
                                break;
                            default:
                                throw new NotImplementedException("Unhandled date format for locale '" + locale + "'");
                        }
                    }

                    // Get the card list html
                    html = Utils.FindFirstContentBetween(html, 0, int.MaxValue, "class=\"box_list\"", "<!--.box_list-->");

                    string mainCardHeader = "card_status";
                    int idx = 0;
                    while ((idx = html.IndexOf(mainCardHeader, idx)) >= 0)
                    {
                        int nextIdx = html.IndexOf(mainCardHeader, idx + 1);

                        int cardId = int.Parse(Utils.FindFirstContentBetween(html, idx, nextIdx, "cid=", "\""));
                        YgoDbSet.CardRarity rarity = YgoDbSet.CardRarity.Common;

                        string rarityStr = Utils.FindFirstContentBetween(html, idx, nextIdx, "/rarity/icon_", ".png");
                        if (rarityStr != null)
                        {
                            switch (rarityStr)
                            {
                                case "r":
                                    rarity = YgoDbSet.CardRarity.Rare;
                                    break;
                                case "ur":
                                    rarity = YgoDbSet.CardRarity.UltraRare;
                                    break;
                                case "ul":
                                    rarity = YgoDbSet.CardRarity.UltimateRare;
                                    break;
                                case "sr":
                                    rarity = YgoDbSet.CardRarity.SuperRare;
                                    break;
                                case "se":
                                    rarity = YgoDbSet.CardRarity.SecretRare;
                                    break;
                                case "gr":
                                    rarity = YgoDbSet.CardRarity.GoldRare;
                                    break;
                                case "gs":
                                    rarity = YgoDbSet.CardRarity.GoldSecret;
                                    break;
                                case "gh":
                                    rarity = YgoDbSet.CardRarity.GhostRare;
                                    break;
                                case "ps":
                                    rarity = YgoDbSet.CardRarity.PlatinumSecretRare;
                                    break;
                                case "sh":
                                    rarity = YgoDbSet.CardRarity.Shatterfoil;
                                    break;
                                case "st":
                                    rarity = YgoDbSet.CardRarity.Starfoil;
                                    break;

                                case "h_n":
                                    rarity = YgoDbSet.CardRarity.HobbyCommon;
                                    break;
                                // ja only
                                case "h_r":
                                    rarity = YgoDbSet.CardRarity.HobbyRare;
                                    break;
                                case "h_sr":
                                    rarity = YgoDbSet.CardRarity.HobbySuperRare;
                                    break;
                                case "h_ur":
                                    rarity = YgoDbSet.CardRarity.HobbyUltraRare;
                                    break;

                                // ja only
                                case "hr":
                                    rarity = YgoDbSet.CardRarity.HolographicRare;
                                    break;
                                case "cr":
                                    rarity = YgoDbSet.CardRarity.CollectorsRare;
                                    break;
                                case "es":
                                    rarity = YgoDbSet.CardRarity.ExtraSecretRare;
                                    break;
                                case "p":
                                    rarity = YgoDbSet.CardRarity.ParallelRare;
                                    break;
                                case "p_sr":
                                    rarity = YgoDbSet.CardRarity.ParallelSuperRare;
                                    break;
                                case "p_se":
                                    rarity = YgoDbSet.CardRarity.ParallelSecretRare;
                                    break;
                                case "p_es":
                                    rarity = YgoDbSet.CardRarity.ParallelExtraSecretRare;
                                    break;
                                case "p_ur":
                                    rarity = YgoDbSet.CardRarity.ParallelUltraRare;
                                    break;
                                case "m_n":
                                    rarity = YgoDbSet.CardRarity.MillenniumCommon;
                                    break;
                                case "m_r":
                                    rarity = YgoDbSet.CardRarity.MillenniumRare;
                                    break;
                                case "m_sr":
                                    rarity = YgoDbSet.CardRarity.MillenniumSuperRare;
                                    break;
                                case "m_se":
                                    rarity = YgoDbSet.CardRarity.MillenniumSecretRare;
                                    break;
                                case "m_ur":
                                    rarity = YgoDbSet.CardRarity.MillenniumUltraRare;
                                    break;
                                case "m_gr":
                                    rarity = YgoDbSet.CardRarity.MillenniumGoldRare;
                                    break;
                                case "kc_n":
                                    rarity = YgoDbSet.CardRarity.KaibaCorporationCommon;
                                    break;
                                case "kc_r":
                                    rarity = YgoDbSet.CardRarity.KaibaCorporationRare;
                                    break;
                                case "kc_sr":
                                    rarity = YgoDbSet.CardRarity.KaibaCorporationSuperRare;
                                    break;
                                case "kc_se":
                                    rarity = YgoDbSet.CardRarity.KaibaCorporationSecretRare;
                                    break;
                                case "kc_ur":
                                    rarity = YgoDbSet.CardRarity.KaibaCorporationUltraRare;
                                    break;

                                // New (relative to when this code was written)
                                case "ur_pr":
                                    rarity = YgoDbSet.CardRarity.PharaohUltraRare;
                                    break;
                                case "pse":
                                    rarity = YgoDbSet.CardRarity.PrismaticSecretRare;
                                    break;
                                case "se_10000":
                                    rarity = YgoDbSet.CardRarity.SecretRare10k;
                                    break;
                                case "pgr":
                                    rarity = YgoDbSet.CardRarity.PremiumGoldRare;
                                    break;

                                default:
                                    throw new NotImplementedException("Unhandled card rarity " + rarityStr);
                            }
                        }

                        // Add the card rarity info to the set
                        if (!setInfo.Cards.ContainsKey(cardId))
                        {
                            setInfo.Cards.Add(cardId, rarity);
                        }

                        idx++;
                    }
                }
            }
        }
    }
}
