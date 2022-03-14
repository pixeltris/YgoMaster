#if WITH_WIKI_DUMPER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlAgilityPack;
using System.Diagnostics;

namespace YgoMaster
{
    /// <summary>
    /// yugipedia scraper for video game decks, starter decks, and structure decks
    /// </summary>
    static class YgoWikiDecks
    {
        const string baseUrl = "https://yugipedia.com";
        static Dictionary<int, YdkHelper.GameCardInfo> allCards;
        static Dictionary<string, YdkHelper.GameCardInfo> allCardsByName;
        static Dictionary<int, int> cardRare;
        static string dataDir;
        static string decksOutputDir;

        class VideoGameInfo
        {
            public string ReleaseDate;
            public string Name;
            public string[] AltNames;
            public string[] CharactersUrls;

            public string OutputDir
            {
                get { return Path.Combine(decksOutputDir, "Video Game", Name); }
            }

            public VideoGameInfo(string releaseDate, string name, string[] altNames, string[] charactersUrls)
            {
                ReleaseDate = releaseDate;
                Name = name;
                HashSet<string> allAltNames = new HashSet<string>();
                allAltNames.Add(name.ToLowerInvariant());
                if (altNames != null)
                {
                    foreach (string altName in altNames)
                    {
                        allAltNames.Add(altName.ToLowerInvariant());
                    }
                }
                AltNames = allAltNames.ToArray();
                CharactersUrls = charactersUrls;
            }
        }

        static string MD5(string text)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
            }
        }

        static string GetPath(string url)
        {
            string dir = Path.Combine(dataDir, "WikiDecks");
            Utils.TryCreateDirectory(dir);
            return Path.Combine(dir, MD5(url));
        }

        public static void Dump(string dataDir, Dictionary<int, int> cardRare)
        {
            YgoWikiDecks.cardRare = cardRare;
            YgoWikiDecks.dataDir = dataDir;
            decksOutputDir = Path.Combine(dataDir, "VG & TCG Decks");
            allCards = YdkHelper.LoadCardDataFromGame(dataDir);
            allCardsByName = new Dictionary<string, YdkHelper.GameCardInfo>();
            foreach (YdkHelper.GameCardInfo cardInfo in allCards.Values)
            {
                allCardsByName[cardInfo.Name.ToLowerInvariant()] = cardInfo;
            }

            DumpTcgDecks();
            DumpVideoGameDecks();
            RegenLinkEvolutionDecks();
        }

        static bool TryDownloadFile(string path, string url)
        {
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                return true;
            }
            return Utils.DownloadFile(url, path);
        }

        static List<int> GetPickCards(List<int> cardIds)
        {
            // YgomGame.DeckEditViewController2.GetRepCards
            //
            // First card:
            // - Highest level main deck monster
            // - Fallback: First card in main deck (if only extra deck, pick highest level in the extra deck)
            // 
            // Second card:
            // - Highest level extra deck monster (if tied pick highest rarity)
            // - Fallback: Highest rarity card
            // 
            // Third card:
            // - Highest rarity card (if monster and rarity is tied; pick highest level monster)
            //
            // Deck order:
            // Monsters: By card frame, then by monster level, then by card id
            // Spells: By spell type, then card id
            // Traps: By trap type, then card id
            List<int> pickCards = new List<int>();
            List<YdkHelper.GameCardInfo> mainDeckCards = new List<YdkHelper.GameCardInfo>();
            List<YdkHelper.GameCardInfo> extraDeckCards = new List<YdkHelper.GameCardInfo>();
            foreach (int id in cardIds)
            {
                YdkHelper.GameCardInfo cardInfo;
                if (cardRare.ContainsKey(id) && allCards.TryGetValue(id, out cardInfo))
                {
                    if (cardInfo.IsMainDeck)
                    {
                        mainDeckCards.Add(cardInfo);
                    }
                    else
                    {
                        extraDeckCards.Add(cardInfo);
                    }
                }
            }
            // TODO: Could probably just do two sorts (first/second + third) for main/extra
            // Sort order for first/second cards: level, rare, frame, icon, id
            // Sort order for third card: rare, level, frame, icon, id
            YdkHelper.GameCardInfo first = mainDeckCards.FindAll(x => !pickCards.Contains(x.Id) && x.IsMonster).OrderByDescending(x => x.Level).ThenByDescending(x => cardRare[x.Id]).ThenBy(x => x.Frame).ThenBy(x => x.Id).FirstOrDefault();
            YdkHelper.GameCardInfo second = null;
            YdkHelper.GameCardInfo third = null;
            if (first == null)
            {
                first = mainDeckCards.FindAll(x => !pickCards.Contains(x.Id) && !x.IsMonster).OrderByDescending(x => cardRare[x.Id]).ThenBy(x => x.Frame).ThenBy(x => x.Icon).ThenBy(x => x.Id).FirstOrDefault();
                if (first == null)
                {
                    first = extraDeckCards.FindAll(x => !pickCards.Contains(x.Id)).OrderByDescending(x => x.Level).ThenByDescending(x => cardRare[x.Id]).ThenBy(x => x.Frame).ThenBy(x => x.Id).FirstOrDefault();
                }
            }
            if (first != null)
            {
                pickCards.Add(first.Id);
                second = extraDeckCards.FindAll(x => !pickCards.Contains(x.Id)).OrderByDescending(x => x.Level).ThenByDescending(x => cardRare[x.Id]).ThenBy(x => x.Frame).ThenBy(x => x.Id).FirstOrDefault();
                if (second == null)
                {
                    second = mainDeckCards.FindAll(x => !pickCards.Contains(x.Id)).OrderByDescending(x => cardRare[x.Id]).ThenByDescending(x => x.Level).ThenBy(x => x.Frame).ThenBy(x => x.Icon).ThenBy(x => x.Id).FirstOrDefault();
                }
            }
            if (second != null)
            {
                pickCards.Add(second.Id);
                List<YdkHelper.GameCardInfo> allDeckCards = new List<YdkHelper.GameCardInfo>();
                allDeckCards.AddRange(mainDeckCards);
                allDeckCards.AddRange(extraDeckCards);
                third = allDeckCards.FindAll(x => !pickCards.Contains(x.Id)).OrderByDescending(x => cardRare[x.Id]).ThenByDescending(x => x.Level).ThenBy(x => x.Frame).ThenBy(x => x.Icon).ThenBy(x => x.Id).FirstOrDefault();
            }
            if (third != null)
            {
                pickCards.Add(third.Id);
            }
            return pickCards;
        }

        static void RegenLinkEvolutionDecks()
        {
            // The data from link evolution didn't include the picked cards / accessory. Add those in
            string leDir = Path.Combine(dataDir, "LeData");
            string outputDir = Path.Combine(decksOutputDir, "Video Game", "Link Evolution");
            RegenLinkEvolutionDecks(new DirectoryInfo(leDir), new DirectoryInfo(outputDir));
        }

        static void RegenLinkEvolutionDecks(DirectoryInfo srcDir, DirectoryInfo dstDir)
        {
            dstDir.Create();
            foreach (FileInfo file in srcDir.GetFiles())
            {
                if (file.Extension.ToLowerInvariant() == ".json")
                {
                    DeckInfo deckInfo = new DeckInfo();
                    deckInfo.FromDictionaryEx(MiniJSON.Json.Deserialize(File.ReadAllText(file.FullName)) as Dictionary<string, object>);
                    List<int> deckCardIds = deckInfo.GetAllCards();
                    string deckName = string.IsNullOrEmpty(deckInfo.Name) ? Path.GetFileNameWithoutExtension(file.Name) : deckInfo.Name;
                    SaveDeck(deckCardIds, 0, dstDir.FullName, deckName, null, null, true);
                }
            }
            foreach (DirectoryInfo subDir in srcDir.GetDirectories())
            {
                RegenLinkEvolutionDecks(subDir, new DirectoryInfo(Path.Combine(dstDir.FullName, subDir.Name)));
            }
        }

        static void DumpVideoGameDecks()
        {
            // Online, Power of Chaos

            List<VideoGameInfo> gameInfos = new List<VideoGameInfo>()
            {
                // DM decks need manual curation due to non-standard dueling rules and card name issues
                //new VideoGameInfo("1998-12-16", "DM 1", baseUrl + "/wiki/Template:Yu-Gi-Oh!_Duel_Monsters_(video_game)_characters"),
                //new VideoGameInfo("1999-07-08", "DM 2 (Dark duel Stories)", baseUrl + "/wiki/Template:Yu-Gi-Oh!_Duel_Monsters_II:_Dark_duel_Stories_characters"),
                //new VideoGameInfo("2000-07-13", "DM 3 (Dark duel Stories)", baseUrl + "/wiki/Template:Yu-Gi-Oh!_Dark_Duel_Stories_characters"),
                //new VideoGameInfo("2000-12-07", "DM 4 (Battle of Great Duelist)", baseUrl + "/wiki/Template:Yu-Gi-Oh!_Duel_Monsters_4:_Battle_of_Great_Duelist_characters"),
                //new VideoGameInfo("2002-10-15", "DM 5 (The Eternal Duelist Soul)", baseUrl + "/wiki/Template:Yu-Gi-Oh!_The_Eternal_Duelist_Soul_characters"),
                //new VideoGameInfo("2003-11-04", "DM 7 (The Sacred Cards)", baseUrl + "/wiki/Template:Yu-Gi-Oh!_The_Sacred_Cards_characters"),
                //new VideoGameInfo("2004-06-29", "DM 8 (Reshef of Destruction)", baseUrl + "/wiki/Template:Yu-Gi-Oh!_Reshef_of_Destruction_characters"),

                new VideoGameInfo("2005-08-30", "Nightmare Troubadour", null, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_Nightmare_Troubadour_characters" }),
                //new VideoGameInfo("2006-01-10", "GX Duel Academy", null, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_GX_Duel_Academy_characters" }),// not enough characters
                new VideoGameInfo("2007-01-02", "GX Spirit Caller", new string[] { "Spirit Caller" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_GX_Spirit_Caller_characters" }),
                //new VideoGameInfo("2010-11-03", "5D's Decade Duels", null, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_Decade_Duels_characters"),// not enough characters
                new VideoGameInfo("2010-12-07", "5D's Duel Transer", new string[] { "Duel Transer" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_Duel_Transer_characters" }),
                new VideoGameInfo("2014-03-16", "Millennium Duels", null, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_Millennium_Duels_characters" }),
                new VideoGameInfo("2014-06-26", "Duel Arena", null, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_Duel_Arena_characters" }),
                new VideoGameInfo("2014-10-30", "Duel Generation", null, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_Duel_Generation_characters" }),

                new VideoGameInfo("2004-02-10", "WC 2004", new string[] { "World Championship 2004" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_World_Championship_Tournament_2004_characters" }),
                new VideoGameInfo("2006-03-14", "WC 2006 (Ultimate Masters)", new string[] { "World Champsionship 2006", "World Championship 2006" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_Ultimate_Masters:_World_Championship_Tournament_2006_characters" }),
                new VideoGameInfo("2007-03-20", "WC 2007", new string[] { "World Championship 2007" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_World_Championship_2007_characters" }),
                new VideoGameInfo("2007-12-04", "WC 2008", new string[] { "World Championship 2008" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_World_Championship_2008_characters" }),
                new VideoGameInfo("2009-05-19", "WC 2009 (5D's Stardust Accelerator)", new string[] { "World Championship 2009", "World Chamiponship 2009" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_World_Championship_2009:_Stardust_Accelerator_characters" }),
                new VideoGameInfo("2010-02-23", "WC 2010 (5D's Reverse of Arcadia)", new string[] { "World Championship 2010" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_World_Championship_2010:_Reverse_of_Arcadia_characters",
                    baseUrl + "/wiki/Template:Teams/WC10" }),
                new VideoGameInfo("2011-05-10", "WC 2011 (5D's Over the Nexus)", new string[] { "World Championship 2011" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_World_Championship_2011:_Over_the_Nexus_characters",
                    baseUrl + "/wiki/Template:Teams/WC11" }),

                new VideoGameInfo("2006-11-14", "Tag Force (GX)", new string[] { "Tag Force" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_GX_Tag_Force_characters" }),
                new VideoGameInfo("2007-09-18", "Tag Force 2 (GX)", new string[] { "Tag Force 2" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_GX_Tag_Force_2_characters" }),
                new VideoGameInfo("2008-11-28", "Tag Force 3 (GX)", new string[] { "Tag Force 3" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_GX_Tag_Force_3_characters" }),
                new VideoGameInfo("2009-11-17", "Tag Force 4 (5D's)", new string[] { "Tag Force 4" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_Tag_Force_4_characters" }),
                new VideoGameInfo("2010-10-26", "Tag Force 5 (5D's)", new string[] { "Tag Force 5" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_Tag_Force_5_characters" }),
                new VideoGameInfo("2011-08-22", "Tag Force 6 (5D's)", new string[] { "Tag Force 6" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_5D%27s_Tag_Force_6_characters" }),
                new VideoGameInfo("2015-01-22", "Tag Force Special (ARC-V)", new string[] { "Tag Force Special" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_ARC-V_Tag_Force_Special_characters" }),

                new VideoGameInfo("2014-09-25", "ZEXAL World Duel Carnival", new string[] { "World Duel Carnival" }, new string[] { baseUrl + "/wiki/Template:Yu-Gi-Oh!_ZEXAL_World_Duel_Carnival_characters" }),
            };

            Dictionary<string, VideoGameInfo> gameInfosByName = new Dictionary<string, VideoGameInfo>();
            foreach (VideoGameInfo gameInfo in gameInfos)
            {
                foreach (string name in gameInfo.AltNames)
                {
                    gameInfosByName[name] = gameInfo;
                }
            }
            gameInfosByName["v jump"] = null;
            gameInfosByName["v-jump"] = null;
            gameInfosByName["yu-gi-oh! duel links"] = null;
            gameInfosByName["duel links"] = null;

            foreach (VideoGameInfo gameInfo in gameInfos)
            {
                Utils.TryCreateDirectory(gameInfo.OutputDir);
                foreach (string characterTemplatesUrl in gameInfo.CharactersUrls)
                {
                    string characterTemplatesPath = GetPath(characterTemplatesUrl);
                    if (!TryDownloadFile(characterTemplatesPath, characterTemplatesUrl))
                    {
                        continue;
                    }
                    HtmlDocument doc = new HtmlDocument();
                    doc.OptionDefaultStreamEncoding = Encoding.UTF8;
                    doc.Load(characterTemplatesPath);
                    HtmlNode[] tables = doc.DocumentNode.Descendants().Where(x => x.GetAttributeValue("class", "").Contains("navbar-mini")).ToArray();//"navbox-title").ToArray();
                    if (tables.Length == 1)
                    {
                        HtmlNode table = tables[0];
                        while (table != null && table.Name != "table")
                        {
                            table = table.ParentNode;
                        }
                        HtmlNode[] elements = table.Descendants().Where(x => x.Name == "li").ToArray();
                        foreach (HtmlNode element in elements)
                        {
                            bool isHeader = false;
                            HtmlNode temp = element;
                            while (temp != null && temp != table)
                            {
                                string className = temp.GetAttributeValue("class", "");
                                if (className.Contains("navbox-abovebelow") || className.Contains("navbox-group") || className.Contains("navbox-title"))
                                {
                                    isHeader = true;
                                    break;
                                }
                                temp = temp.ParentNode;
                            }
                            if (isHeader)
                            {
                                continue;
                            }
                            foreach (HtmlNode link in element.Descendants("a"))
                            {
                                string characterUrl = baseUrl + link.GetAttributeValue("href", string.Empty);
                                if (characterUrl.Contains("redlink=1"))
                                {
                                    continue;
                                }
                                string characterName = link.InnerText;
                                int openBraceIndex = characterName.IndexOf('(');
                                if (openBraceIndex > 0)
                                {
                                    characterName = characterName.Substring(0, openBraceIndex).Trim();
                                }
                                DumpVideoGameCharacterDecks(characterUrl, characterName, gameInfo, gameInfosByName);
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("TODO: Handle multiple / no tables for video game '" + characterTemplatesUrl + "'");
                    }
                }
            }
        }

        static void DumpVideoGameCharacterDecks(string characterUrl, string characterName, VideoGameInfo gameInfo, Dictionary<string, VideoGameInfo> gameInfosByName)
        {
            string characterPath = GetPath(characterUrl);
            if (!TryDownloadFile(characterPath, characterUrl))
            {
                return;
            }
            HtmlDocument doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.Load(characterPath);

            HtmlNode deckNode = doc.GetElementbyId("Deck");
            if (deckNode == null)
            {
                deckNode = doc.GetElementbyId("Decks");
            }
            if (deckNode == null)
            {
                deckNode = doc.DocumentNode.Descendants().Where(x => x.GetAttributeValue("class", string.Empty).Contains("decklist")).FirstOrDefault();
                if (deckNode == null)
                {
                    Debug.WriteLine("No decks found for character '" + characterName + "' at url '" + characterUrl + "'");
                    return;
                }
                else
                {
                    deckNode = deckNode.PreviousSibling;
                }
            }
            else
            {
                while (deckNode.Name != "h2" && deckNode.Name != "h3" && deckNode.Name != "h4")
                {
                    deckNode = deckNode.ParentNode;
                }
            }

            bool isAnime = false;
            VideoGameInfo activeGameInfo = gameInfo;
            string deckName = null;
            HtmlNode nextNode = deckNode.NextSibling;
            while (nextNode != null)
            {
                if (nextNode.Name == "h2" || nextNode.GetAttributeValue("class", string.Empty) == "navbox")
                {
                    break;
                }
                if (nextNode.Name == "h3" || nextNode.Name == "h4")
                {
                    // Target game change
                    string gameName = SanitizeText(nextNode.FirstChild.InnerText).ToLowerInvariant();
                    switch (gameName)
                    {
                        case "anime":
                        case "ground":
                        case "turbo":
                            isAnime = true;
                            break;
                        case "deck recipe":
                            isAnime = false;
                            break;
                        default:
                            isAnime = false;
                            if (gameInfosByName.ContainsKey(gameName))
                            {
                                activeGameInfo = gameInfosByName[gameName];
                            }
                            break;
                    }
                }
                /*if (nextNode.Name == "table" && nextNode.GetAttributeValue("class", string.Empty).Contains("card-list"))
                {
                    // TODO
                    System.Diagnostics.Debugger.Break();
                    if (isCurrentGame && !isAnime)
                    {
                    }
                    deckName = null;
                }*/
                if (nextNode.Name == "div" && nextNode.GetAttributeValue("class", string.Empty).Contains("decklist"))
                {
                    if (activeGameInfo == gameInfo && !isAnime)
                    {
                        HtmlNode tableBody = nextNode.Descendants().Where(x => x.Name == "tbody").FirstOrDefault();
                        HtmlNode[] rows = tableBody.ChildNodes.Where(x => x.Name == "tr").ToArray();
                        if (rows.Length > 2)
                        {
                            Debug.WriteLine("TODO: Handle character deck table with more than 2 entries for url '" + characterUrl + "'");
                        }
                        else
                        {
                            if (rows.Length == 2)
                            {
                                // Get the deck name from the header
                                deckName = SanitizeText(rows[0].InnerText);
                                deckName = Utils.RemoveAllBracePairs(deckName);// TODO: Handle this better (this is to remove japanese text which is braced)
                                deckName = deckName.Trim();
                            }
                            bool validDeckName = true;
                            switch (deckName.ToLowerInvariant())
                            {
                                case "trunk":
                                case "side deck":
                                case "anime deck":
                                case "turbo deck":
                                case "secret treasures":
                                    validDeckName = false;
                                    break;
                            }
                            if (validDeckName)
                            {
                                string text = SanitizeText(rows[rows.Length == 2 ? 1 : 0].InnerText, false);
                                Dictionary<string, int> cardNames = Utils.GetCardNamesLowerAndCount(text);
                                int numCardsMissing = 0;
                                List<int> deckCardIds = new List<int>();
                                foreach (KeyValuePair<string, int> cardName in cardNames)
                                {
                                    string name = RemapCardName(cardName.Key);
                                    if (string.IsNullOrEmpty(name))
                                    {
                                        continue;
                                    }
                                    int count = cardName.Value;
                                    YdkHelper.GameCardInfo cardInfo;
                                    if (allCardsByName.TryGetValue(name, out cardInfo))
                                    {
                                        if (cardRare.ContainsKey(cardInfo.Id))
                                        {
                                            for (int i = 0; i < count; i++)
                                            {
                                                deckCardIds.Add(cardInfo.Id);
                                            }
                                        }
                                        else
                                        {
                                            numCardsMissing += count;
                                        }
                                    }
                                    else if (IsMissingCard(name))
                                    {
                                        numCardsMissing += count;
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Failed to find card '" + name + "' x" + count + " for deck '" + deckName + "' on url '" + characterUrl + "'");
                                    }
                                }
                                SaveDeck(deckCardIds, numCardsMissing, gameInfo.OutputDir, deckName, characterName, characterUrl);
                            }
                        }
                    }
                    deckName = null;
                }
                nextNode = nextNode.NextSibling;
            }
        }

        static bool SaveDeck(List<int> deckCardIds, int numCardsMissing, string ouputDir, string deckName, string characterName, string sourceUrl, bool ignoreMissing = false)
        {
            DeckInfo deckInfo = new DeckInfo();
            foreach (int cardId in deckCardIds)
            {
                YdkHelper.GameCardInfo cardInfo;
                if (allCards.TryGetValue(cardId, out cardInfo) && cardRare.ContainsKey(cardInfo.Id))
                {
                    if (cardInfo.IsMainDeck)
                    {
                        deckInfo.MainDeckCards.Add(cardInfo.Id);
                    }
                    else
                    {
                        deckInfo.ExtraDeckCards.Add(cardInfo.Id);
                    }
                }
            }
            foreach (int id in GetPickCards(deckCardIds))
            {
                deckInfo.DisplayCards.Add(id);
            }
            if (ignoreMissing || (deckInfo.MainDeckCards.Count >= 38 || (numCardsMissing == 0 && deckInfo.MainDeckCards.Count >= 36)))
            {
                deckInfo.TimeCreated = deckInfo.TimeEdited = Utils.GetEpochTime();
                deckInfo.Accessory.SetDefault();
                if (string.IsNullOrEmpty(deckName))
                {
                    deckName = "Deck";
                }
                deckName = deckName.Trim();
                deckInfo.Name = deckName;
                if (!string.IsNullOrEmpty(characterName))
                {
                    deckName = characterName + " - " + deckName;
                }
                for (int i = 0; i < 2; i++)
                {
                    deckName = deckName.Replace("&#91;sic&#93;" + (i == 0 ? " " : ""), "");
                }
                deckName = deckName.Replace("&amp;", "&");
                deckName = deckName.Trim();
                if (deckName.Length <= DeckInfo.NameMaxLength)
                {
                    deckInfo.Name = deckName;
                }
                string deckFileName = String.Join("", deckName.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.') + ".json";
                if (string.IsNullOrEmpty(deckFileName))
                {
                    deckFileName = "Deck";
                }
                File.WriteAllText(Path.Combine(ouputDir, deckFileName),
                    MiniJSON.Json.Serialize(deckInfo.ToDictionaryEx()) + (string.IsNullOrEmpty(sourceUrl) ? "" : (Environment.NewLine + "//" + sourceUrl)));
                return true;
            }
            return false;
        }

        static string SanitizeText(string text, bool removeControlChars = true)
        {
            return new string(text.Where(c => (!removeControlChars || !char.IsControl(c)) && c != '\u200E').ToArray())
                .Replace("&#32;", " ")
                .Replace("&amp;", "&")
                .Replace("’", "'");
        }

        static void DumpTcgDecks()
        {
            string allDecksUrl = baseUrl + "/wiki/Template:Decks";
            string allDecksPath = GetPath(allDecksUrl);
            if (!TryDownloadFile(allDecksPath, allDecksUrl))
            {
                return;
            }
            HtmlDocument doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.Load(allDecksPath);

            // Starter decks
            {
                List<HtmlNode> deckListsOfInterest = new List<HtmlNode>();
                HtmlNode nodeDecks = doc.GetElementbyId("Starter_Decks");
                while (nodeDecks != null && nodeDecks.Name != "table")
                {
                    nodeDecks = nodeDecks.ParentNode;
                }
                
                deckListsOfInterest.Add(nodeDecks.Descendants().Where(x => x.Name == "th" && x.InnerText == "Character").ToArray()[0].NextSibling);

                HtmlNode nodeAnnual = nodeDecks.Descendants().Where(x => x.Name == "th" && x.InnerText == "Annual").ToArray()[0].NextSibling;
                HtmlNode nodeAnnualTcg = nodeAnnual.Descendants().Where(x => x.Name == "th" && x.InnerText == "TCG").ToArray()[0].NextSibling;
                Debug.Assert(nodeAnnualTcg.FirstChild.Name == "div");
                deckListsOfInterest.Add(nodeAnnualTcg.FirstChild);

                DumpTcgDecks("Starter", deckListsOfInterest,
                    "2-Player: Yuya & Declan"// 21 card decks, not too useful
                    );
            }

            // Structure decks
            {
                List<HtmlNode> deckListsOfInterest = new List<HtmlNode>();
                HtmlNode nodeDecks = doc.GetElementbyId("Structure_Decks");
                while (nodeDecks != null && nodeDecks.Name != "table")
                {
                    nodeDecks = nodeDecks.ParentNode;
                }

                deckListsOfInterest.Add(nodeDecks.Descendants().Where(x => x.Name == "th" && x.InnerText == "Worldwide").ToArray()[0].NextSibling);
                deckListsOfInterest.Add(nodeDecks.Descendants().Where(x => x.Name == "th" && x.InnerText == "TCG").ToArray()[0].NextSibling);

                DumpTcgDecks("Structure", deckListsOfInterest);
            }
        }

        static void DumpTcgDecks(string groupName, List<HtmlNode> deckListsOfInterest, params string[] skipDeckNames)
        {
            foreach (HtmlNode deckListContainer in deckListsOfInterest)
            {
                foreach (HtmlNode originalEntry in deckListContainer.Descendants().Where(x => x.Name == "li"))
                {
                    HtmlNode[] links = originalEntry.Descendants("a").ToArray();
                    if (links.Length == 1)
                    {
                        string deckName = links[0].InnerText;
                        if (skipDeckNames.Contains(deckName))
                        {
                            continue;
                        }
                        string deckUrl = baseUrl + links[0].GetAttributeValue("href", string.Empty);
                        string deckPath = GetPath(deckUrl);
                        if (!TryDownloadFile(deckPath, deckUrl))
                        {
                            Debug.WriteLine("Failed to fetch '" + deckUrl + "'");
                        }
                        else
                        {
                            DumpTcgDeck(deckPath, deckUrl, deckName, groupName);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("TODO: Handle multiple / no links for: " + string.Join(", ", links.Select(x => x.InnerText)));
                    }
                }
            }
        }

        static void DumpTcgDeck(string path, string url, string deckName, string groupName)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.Load(path);

            DateTime releaseDate = default(DateTime);
            HtmlNode[] infoBoxes = doc.DocumentNode.Descendants().Where(x => x.GetAttributeValue("class", "").Contains("infobox-yugipedia")).ToArray();
            foreach (HtmlNode infoBox in infoBoxes)
            {
                if (releaseDate != default(DateTime))
                {
                    break;
                }
                HtmlNode[] releaseDatesHeader = infoBox.Descendants().Where(x => x.Name == "tr" && SanitizeText(x.InnerText).ToLowerInvariant().Contains("release date")).ToArray();
                if (releaseDatesHeader.Length > 0)
                {
                    HtmlNode dateNode = releaseDatesHeader[0].NextSibling;
                    while (dateNode != null)
                    {
                        HtmlNode[] englishDateNodes = dateNode.Descendants().Where(x => x.Name == "th" && SanitizeText(x.InnerText).ToLowerInvariant().Contains("english")).ToArray();
                        if (englishDateNodes.Length > 0 && !englishDateNodes[0].InnerText.ToLowerInvariant().Contains("asian"))
                        {
                            if (DateTime.TryParse(englishDateNodes[0].NextSibling.InnerText, out releaseDate))
                            {
                                break;
                            }
                        }
                        dateNode = dateNode.NextSibling;
                    }
                }
            }
            if (releaseDate != default(DateTime))
            {
                deckName = releaseDate.ToString("yyy-MM") + " - " + deckName;
            }

            HtmlNode[] cardLists = doc.DocumentNode.Descendants().Where(x => x.GetAttributeValue("class", "").Contains("card-list")).ToArray();
            foreach (HtmlNode cardList in cardLists)
            {
                string cardListDeckName = deckName;
                if (cardLists.Length > 1)
                {
                    if (cardList.ParentNode.PreviousSibling.PreviousSibling.Name != "h2")
                    {
                        Debug.WriteLine("Multi card list without a deck name at url '" + url + "'");
                        continue;
                    }
                    cardListDeckName = cardList.ParentNode.PreviousSibling.PreviousSibling.Descendants().Where(
                        x => x.GetAttributeValue("class", "").Contains("mw-headline")).ToArray()[0].InnerText.Trim();
                }
                List<int> deckCardIds = new List<int>();
                int numCardsMissing = 0;
                int nameColumnIndex = -1;
                int quantityColumnIndex = -1;
                bool isFirstRow = true;
                foreach (HtmlNode row in cardList.Descendants("tr"))
                {
                    Debug.Assert(row.ParentNode.ParentNode == cardList);// TODO: If fail, handle situations where there are sub tables
                    if (isFirstRow)
                    {
                        int columnIndex = 0;
                        foreach (HtmlNode column in row.ChildNodes)
                        {
                            switch (column.InnerText.ToLowerInvariant())
                            {
                                case "english name":
                                case "name":
                                case "card name":
                                    nameColumnIndex = columnIndex;
                                    break;
                                case "qty":
                                case "quantity":
                                case "count":
                                    quantityColumnIndex = columnIndex;
                                    break;
                            }
                            columnIndex++;
                        }
                        if (nameColumnIndex == -1)
                        {
                            Debug.WriteLine("Failed to find card name column for '" + url + "'");
                            return;
                        }
                        isFirstRow = false;
                    }
                    else
                    {
                        int quantity;
                        if (quantityColumnIndex < 0 || !int.TryParse(row.ChildNodes[quantityColumnIndex].InnerText, out quantity))
                        {
                            quantity = 1;
                        }
                        string name = row.ChildNodes[nameColumnIndex].SelectSingleNode("a").InnerText.ToLowerInvariant();
                        name = SanitizeText(name);
                        name = RemapCardName(name);
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        YdkHelper.GameCardInfo cardInfo;
                        if (allCardsByName.TryGetValue(name, out cardInfo))
                        {
                            if (cardRare.ContainsKey(cardInfo.Id))
                            {
                                for (int i = 0; i < quantity; i++)
                                {
                                    deckCardIds.Add(cardInfo.Id);
                                }
                            }
                            else
                            {
                                numCardsMissing += quantity;
                            }
                        }
                        else if (IsMissingCard(name))
                        {
                            numCardsMissing += quantity;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to find card '" + name + "' x" + quantity + " for deck '" + deckName + "' on url '" + url + "'");
                        }
                    }
                }
                string dir = Path.Combine(decksOutputDir, groupName);
                Utils.TryCreateDirectory(dir);
                SaveDeck(deckCardIds, numCardsMissing, dir, deckName, null, url);
            }
        }
        
        // TODO: Maybe move this into Utils.cs so that DeckEditorUtils.cs can take advantage of rhis for copying decks from yugipedia manually?
        static string RemapCardName(string name)
        {
            // Remap cards (cards not in the game / card name changes)
            name = name.TrimEnd('∞', ' ');
            switch (name)
            {
                // These aren't in master duel. Remap them to similar cards
                case "mooyan curry": name = "blue medicine"; break;
                case "spellbinding circle": name = "shadow spell"; break;
                case "bait doll": name = "de-spell"; break;// TODO: Think of a better replacement (bait doll is stronger than de-spell)

                // Cards which have changed name
                case "oscillo hero #2": name = "wattkid"; break;
                case "excavation of mage stones": name = "magical stone excavation"; break;
                case "dark magician's tome of black magic": name = "magic formula"; break;
                case "d. d. assailant": name = "d.d. assailant"; break;
                case "d. d. warrior lady": name = "d.d. warrior lady"; break;
                case "amazon archer": name = "amazoness archer"; break;
                case "marie the fallen one": name = "darklord marie"; break;
                case "collapse": name = "shrink"; break;
                case "red-eyes b. dragon": name = "red-eyes black dragon"; break;
                case "cathedral of nobles": name = "temple of the kings"; break;
                case "corpse of yata-garasu": name = "legacy of yata-garasu"; break;
                case "reversal of graves": name = "exchange of the spirit"; break;
                case "winged dragon,guardian of the fortress #1": name = "winged dragon, guardian of the fortress #1"; break;
                case "dark magic ritual": name = "black magic ritual"; break;
                case "enchanted arrow": name = "spell shattering arrow"; break;
                case "magic-arm shield": name = "magical arm shield"; break;
                case "rope of spirit": name = "soul rope"; break;
                case "makiu": name = "makiu, the magical mist"; break;
                case "b. skull dragon": name = "black skull dragon"; break;
                case "dimensional warrior": name = "d.d. warrior"; break;
                case "silent fiend": name = "silent doom"; break;
                case "hidden book of spell": name = "hidden spellbook"; break;
                case "shield break":
                case "shield crash": name = "shield crush"; break;
                case "hero flash !!": name = "hero flash!!"; break;
                case "elemental hero madballman": name = "elemental hero mudballman"; break;
                case "elemental hero erikshieler": name = "elemental hero electrum"; break;
                case "elemental hero airman": name = "elemental hero stratos"; break;
                case "sealed gold coffer": name = "gold sarcophagus"; break;
                case "kinetic soldier": name = "cipher soldier"; break;
                case "forest guard green baboon": name = "green baboon, defender of the forest"; break;
                case "master of dragon soldier": name = "dragon master knight"; break;
                case "ancient gear artillery": name = "ancient gear tank"; break;
                case "flying kamakiri 1":
                case "flying kamakiri": name = "flying kamakiri #1"; break;
                case "mystic wok": name = "mystik wok"; break;
                case "helios tris megistus": name = "helios trice megistus"; break;
                case "red-eyes b. chick": name = "black dragon's chick"; break;
                case "cliff the trap remover": name = "dark scorpion - cliff the trap remover"; break;
                case "stronghold": name = "stronghold the moving fortress"; break;
                case "hand collapse": name = "hand destruction"; break;
                case "monster relief": name = "relieve monster"; break;
                case "tricky's magic 4": name = "tricky spell 4"; break;
                case "skull descovery knight": name = "doomcaliber knight"; break;
                case "spell reactor・re": name = "spell reactor ・re"; break;
                case "summon reactor・sk": name = "summon reactor ・sk"; break;
                case "trap reactor・y fi": name = "trap reactor ・y fi"; break;
                case "cu chulainn the awakened": name = "cú chulainn the awakened"; break;
                case "dryad": name = "doriado"; break;
                case "pigeonholing books of spell": name = "spellbook organization"; break;
                case "sacred knight joan": name = "noble knight joan"; break;
                case "coat of justice": name = "court of justice"; break;
                case "harpie's brother": name = "sky scout"; break;
                case "big core": name = "b.e.s. big core"; break;
                case "nightmare's steel cage": name = "nightmare's steelcage"; break;
                case "morphing jar 2": name = "morphing jar #2"; break;
                case "falchionß": name = "falchionβ"; break;
                case "harpie's hunting ground": name = "harpies' hunting ground"; break;
                case "judgement of anubis": name = "judgment of anubis"; break;
                case "thunder-end dragon": name = "thunder end dragon"; break;
                case "injection angel lily": name = "injection fairy lily"; break;
                case "white stone of legend": name = "the white stone of legend"; break;
                case "double-edge sword technique": name = "double-edged sword technique"; break;
                case "fire formation - gyokko": name = "fire formation - gyokkou"; break;
                case "super vehicroid - jumbo drill": name = "super vehicroid jumbo drill"; break;
                case "bazoo the soul eater": name = "bazoo the soul-eater"; break;
                case "nurse reficule the fallen one": name = "darklord nurse reficule"; break;
                case "doppel warrior": name = "doppelwarrior"; break;
                case "gagaga bolt": name = "gagagabolt"; break;
                case "triple-star trion": name = "triple star trion"; break;
                case "oh f!sh": name = "oh f!sh!"; break;
                case "blackwing - sirocco of dawn": name = "blackwing - sirocco the dawn"; break;
                case "freya, guide to victory": name = "freya, spirit of victory"; break;
                case "chronomaly cabrera trebelchet": name = "chronomaly cabrera trebuchet"; break;
                case "sargasso's lighthouse": name = "sargasso lighthouse"; break;
                case "luster dragon 2": name = "luster dragon #2"; break;
                case "meteor b. dragon": name = "meteor black dragon"; break;
                case "red-eyes b. metal dragon": name = "red-eyes black metal dragon"; break;
                case "a rival appears": name = "a rival appears!"; break;
                case "d.d.r - different dimension reincarnation": name = "d.d.r. - different dimension reincarnation"; break;
                case "stardust dragon*": name = "stardust dragon"; break;
                case "submersible aero shark": name = "submersible carrier aero shark"; break;
                case "metaion, the time lord": name = "metaion, the timelord"; break;
                case "malefic red-eyes b. dragon": name = "malefic red-eyes black dragon"; break;
                case "metaphysical regeneration": name = "supernatural regeneration"; break;
                case "madolche cruffsant": name = "madolche cruffssant"; break;
                case "ojama delta hurricane": name = "ojama delta hurricane!!"; break;
                case "necrolancer the timelord": name = "necrolancer the time-lord"; break;
                case "o - over soul": name = "o - oversoul"; break;
                case "flamvell magical": name = "flamvell magician"; break;
                case "laval judgement lord": name = "laval judgment lord"; break;
                case "flamvell urquizas": name = "flamvell uruquizas"; break;
                case "xyz xtreme!!": name = "xyz xtreme !!"; break;
                case "harpie lady #1": name = "harpie lady 1"; break;
                case "harpie lady #2": name = "harpie lady 2"; break;
                case "harpie lady #3": name = "harpie lady 3"; break;
                case "air marmot of nefariousness": name = "archfiend marmot of nefariousness"; break;
                case "stone d.": name = "stone dragon"; break;
                case "kaminarikozou": name = "thunder kid"; break;
                case "wicked mirror": name = "archfiend mirror"; break;
                case "call of the dark": name = "call of darkness"; break;
                case "magic thorn": name = "magical thorn"; break;
                case "kuwagata alpha": name = "kuwagata α"; break;
                case "bright castle": name = "shine palace"; break;
                case "curse of vampire": name = "vampire's curse"; break;
                case "mistobody": name = "mist body"; break;
                case "magician's valkyrie": name = "magician's valkyria"; break;
                case "banisher of light": name = "banisher of the light"; break;
                case "helios duo megiste": name = "helios duo megistus"; break;
                case "helios tris megiste": name = "helios trice megistus"; break;
                case "homonculus gold": name = "golden homunculus"; break;
                case "the ancient sun helios": name = "helios - the primordial sun"; break;
                case "guardian exode": name = "exxod, master of the guard"; break;
                case "fault zone": name = "canyon"; break;
                case "hero heyro": name = "hero ring"; break;
                case "level up": name = "level up!"; break;
                case "ally of justice - catastor": name = "ally of justice catastor"; break;
                case "calamity of the wicked": name = "malevolent catastrophe"; break;
                case "summon priest": name = "summoner monk"; break;
                case "skull zoma": name = "zoma the spirit"; break;
                case "f.g.d.": name = "five-headed dragon"; break;
                case "alkana knight joker": name = "arcana knight joker"; break;
                case "dark dreadroute": name = "the wicked dreadroot"; break;
                case "rigras leever": name = "rigorous reaver"; break;
                case "white horns d.": name = "white-horned dragon"; break;
                case "dark ruler vandalgyon": name = "van'dalgyon the dark dragon lord"; break;
                case "crush card": name = "crush card virus"; break;
                case "meklord emperor wisel ∞": name = "meklord emperor wisel"; break;
                case "magician's unite": name = "magicians unite"; break;
                case "gateway of dark world": name = "gateway to dark world"; break;
                case "rancer dragonute": name = "lancer dragonute"; break;
                case "acolyte of the ice barrier": name = "pilgrim of the ice barrier"; break;
                case "ice blast user rice": name = "reese the ice mistress"; break;
                case "red-moon baby": name = "vampire baby"; break;
                case "vampire orchis": name = "vampiric orchis"; break;
                case "altar of tribute": name = "altar for tribute"; break;
                case "victory d.": name = "victory dragon"; break;
                case "powerbond": name = "power bond"; break;
                case "spell absorbtion": name = "spell absorption"; break;
                case "indomitable fighter lee": name = "indomitable fighter lei lei"; break;
                case "winged rhino": name = "winged rhynos"; break;
                case "jurak protapus": name = "jurrac protops"; break;
                case "jurak tyranus": name = "jurrac tyrannus"; break;
                case "jurak vello": name = "jurrac velo"; break;
                case "jurak monolov": name = "jurrac monoloph"; break;
                case "jurak giganot": name = "jurrac giganoto"; break;
                case "levia-dragon daedalus": name = "levia-dragon - daedalus"; break;
                case "destiny hero - bloo-d": name = "destiny hero - plasma"; break;
                case "dark avatar": name = "the wicked avatar"; break;
                case "dark eraser": name = "the wicked eraser"; break;
                case "naturia gaodrake": name = "naturia leodrake"; break;
                case "reinforcements of the army": name = "reinforcement of the army"; break;
                case "kaiser seahorse": name = "kaiser sea horse"; break;
                case "bell of destruction": name = "ring of destruction"; break;
                case "mechanical spider": name = "karakuri spider"; break;
                case "swordsman from a foreign land": name = "swordsman from a distant land"; break;
                case "gilgarth": name = "gil garth"; break;
                case "elemnetal hero avian": name = "elemental hero avian"; break;
                case "elmental hero sparkman": name = "elemental hero sparkman"; break;
                case "naturia nerve": name = "naturia vein"; break;
                case "the splendid venus": name = "splendid venus"; break;
                case "fiend roar deity raven": name = "fabled raven"; break;
                case "fiend roar deity valkiris": name = "fabled valkyrus"; break;
                case "ally of justice - field marshall": name = "ally of justice field marshal"; break;
                case "ally of justice - light gazer": name = "ally of justice light gazer"; break;
                case "exodius the ultimate forbidden one": name = "exodius the ultimate forbidden lord"; break;
                case "arcana force extra - the light ruler": name = "arcana force ex - the light ruler"; break;
                case "x-saber urz": name = "x-saber uruz"; break;
                case "armityle the chaos phantom": name = "armityle the chaos phantasm"; break;
                case "vortex kong": name = "voltic kong"; break;
                case "elemental hero the heat": name = "elemental hero heat"; break;
                case "counter crystal": name = "counter gem"; break;
                case "crystal set": name = "crystal pair"; break;
                case "arms hole": name = "hidden armory"; break;
                case "naturia cherry": name = "naturia cherries"; break;
                case "divine fowl king alector": name = "alector, sovereign of birds"; break;
                case "stardust dragon/assault modex2": name = "stardust dragon/assault mode"; break;// TODO: Fix their broken "x2"
                case "soldier of mist valley": name = "mist valley soldier"; break;
                case "blizzed, guard of the ice barrier": name = "blizzed, defender of the ice barrier"; break;
                case "great monk of the ice barrier": name = "dai-sojo of the ice barrier"; break;
                case "intoxicated bug of the ice barrier": name = "numbing grub in the ice barrier"; break;
                case "feng shui master of the ice barrier": name = "geomancer of the ice barrier"; break;
                case "practitioner of the ice barrier": name = "cryomancer of the ice barrier"; break;
                case "dewloren, tiger prince of the ice barrier": name = "dewloren, tiger king of the ice barrier"; break;
                case "worm gurus": name = "worm gulse"; break;
                case "worm jeetrikups": name = "worm jetelikpse"; break;
                case "worm links": name = "worm linx"; break;
                case "worm milidith": name = "worm millidith"; break;
                case "worm requie": name = "worm rakuyeh"; break;
                case "ally bomb": name = "ally salvo"; break;
                case "ally of justice - blind sucker": name = "ally of justice nullfier"; break;
                case "ally of justice - cosmic closer": name = "ally of justice cosmic gateway"; break;
                case "ally of justice - researcher": name = "ally of justice searcher"; break;
                case "ally of justice - thousand arms": name = "ally of justice thousand arms"; break;
                case "ally of justice - unknown crusher": name = "ally of justice unknown crusher"; break;
                case "ally of justice - unlimiter": name = "ally of justice unlimiter"; break;
                case "ally of justice - cyclone creator": name = "ally of justice cyclone creator"; break;
                case "ally of justice - field marshal": name = "ally of justice field marshal"; break;
                case "flamvell gurnika": name = "flamvell grunika"; break;
                case "flamvell paun": name = "flamvell poun"; break;
                case "x-saber anapelera": name = "x-saber anu piranha"; break;
                case "allsword commander gatmuz": name = "commander gottoms, swordmaster"; break;
                case "x-saber palomlo": name = "x-saber palomuro"; break;
                case "x-saber passiul": name = "x-saber pashuul"; break;
                case "naturia cosmos beet": name = "naturia cosmobeet"; break;
                case "army genex": name = "genex army"; break;
                case "genex blast": name = "genex blastfan"; break;
                case "genex heat": name = "genex furnace"; break;
                case "solar genex": name = "genex solar"; break;
                case "turbine genex": name = "genex turbine"; break;
                case "wind farm genex": name = "windmill genex"; break;
                case "watchkeeper of mist valley": name = "mist valley watcher"; break;
                case "executioner of mist valley": name = "mist valley executor"; break;
                case "falcon of mist valley": name = "mist valley falcon"; break;
                case "mist valley bird of prey": name = "mist valley apex avian"; break;
                case "thunder bird of mist valley": name = "mist valley thunderbird"; break;
                case "shaman of mist valley": name = "mist valley shaman"; break;
                case "strange bird of mist valley": name = "mist valley baby roc"; break;
                case "wind user of mist valley": name = "mist valley windmaster"; break;
                case "thunder lord of mist valley": name = "mist valley thunder lord"; break;
                case "fiend roar deity luri": name = "fabled lurrie"; break;
                case "saurobeast brachion": name = "sauropod brachion"; break;
                case "guard of flamvell": name = "flamvell guard"; break;
                case "machiners defender": name = "machina defender"; break;
                case "machiners force": name = "machina force"; break;
                case "machiners soldier": name = "machina soldier"; break;
                case "machiners sniper": name = "machina sniper"; break;
                case "ally of justice - enemy catcher": name = "ally of justice enemy catcher"; break;
                case "ally of justice - garadholg": name = "ally of justice garadholg"; break;
                case "ally of justice - rudra": name = "ally of justice rudra"; break;
                case "ally of justice - thunder armor": name = "ally of justice thunder armor"; break;
                case "spare genex": name = "genex spare"; break;
                case "fiend roar deity galbas": name = "fabled gallabas"; break;
                case "fiend roar deity grimlo": name = "fabled grimro"; break;
                case "fiend roar deity kushano": name = "fabled kushano"; break;
                case "blackwing - zephyrus the elite": name = "blackwing - zephyros the elite"; break;
                case "carrieroid": name = "carrierroid"; break;
                case "hot ride": name = "kasha"; break;
                case "forbidden graveyard": name = "silent graveyard"; break;
                case "cemetery bomb": name = "cemetary bomb"; break;
                case "hundred-eyes dragon": name = "hundred eyes dragon"; break;
                case "worm lynx": name = "worm linx"; break;
                case "snap dragon": name = "snapdragon"; break;
                case "dragunity knight - gadearg": name = "dragunity knight - gae dearg"; break;
                case "dragunity knight - vajuranda": name = "dragunity knight - vajrayana"; break;
                case "trident dragon": name = "trident dragion"; break;
                case "vampire koala": name = "vampiric koala"; break;
                case "gishki erial": name = "gishki ariel"; break;
                case "gishki ceremonial mirror": name = "gishki aquamirror"; break;
                case "ceremonial mirror meditation": name = "aquamirror meditation"; break;
                case "archfiend zombie skull": name = "archfiend zombie-skull"; break;
                case "laval cannoneer": name = "laval cannon"; break;
                case "tender of the laval volcano": name = "laval volcano handmaiden"; break;
                case "laval stannon": name = "laval stennon"; break;
                case "gem-knight prism aura": name = "gem-knight prismaura"; break;
                case "daigusto sphreeze": name = "daigusto sphreez"; break;
                case "vylon hapt": name = "vylon hept"; break;
                case "vylon tetrah": name = "vylon tetra"; break;
                case "steelswarm genesoid": name = "steelswarm genome"; break;
                case "steelswarm needle": name = "steelswarm sting"; break;
                case "gishki eater": name = "gishki reliever"; break;
                case "gishki noelia": name = "gishki noellia"; break;
                case "double-fin shark": name = "double fin shark"; break;
                case "reborn puzzle": name = "puzzle reborn"; break;
                case "ghostrick panic": name = "ghostrick scare"; break;
                case "toad slime": name = "slime toad"; break;
                case "overpowering eyes": name = "overpowering eye"; break;
                case "earth giant gaia plate": name = "gaia plate the earth giant"; break;
                case "cyber angel - benten": name = "cyber angel benten"; break;
                case "cyber angel - dakini": name = "cyber angel dakini"; break;
                case "philosopher's stone - sabatier": name = "sabatiel - the philosopher's stone"; break;
                case "ayer's rock sunrise": name = "ayers rock sunrise"; break;
                case "cyber angel - idaten": name = "cyber angel idaten"; break;
                case "ice blizzard master": name = "ice master"; break;
                case "swordsman of doom lithmus": name = "litmus doom swordsman"; break;
                case "ritual of lithmus": name = "litmus doom ritual"; break;
                case "declaration of rebirth": name = "rebirth judgment"; break;
                case "clone duplication": name = "cloning"; break;
                case "dragonroar": name = "dragoroar"; break;
                case "lord of dark summons": name = "dark summoning beast"; break;
                case "holy knight ishzark": name = "divine knight ishzark"; break;
                case "black magic revival casket": name = "dark renewal"; break;
                case "saber slasher": name = "sword slasher"; break;
                case "mystical beast serket": name = "mystical beast of serket"; break;
                case "beast machine king barbaros ur": name = "beast machine king barbaros ür"; break;
                case "naturia balkion": name = "naturia barkion"; break;
                case "dark master zorc": name = "dark master - zorc"; break;
                case "alligator sword dragon": name = "alligator's sword dragon"; break;
                case "arkana knight joker": name = "arcana knight joker"; break;
                case "ally of justice - clausolas": name = "ally of justice clausolas"; break;
                case "dark trap hole": name = "darkfall"; break;
                case "shield and sword": name = "shield & sword"; break;
                case "frog the jam": name = "slime toad"; break;
                case "real genex kurokishian": name = "locomotion r-genex"; break;
                case "jurak gwyver": name = "jurrac guaiba"; break;
                case "jurak iguanon": name = "jurrac iguanon"; break;
                case "jurak staurico": name = "jurrac stauriko"; break;
                case "jurak titan": name = "jurrac titano"; break;
                case "jurak brachis": name = "jurrac brachis"; break;
                case "jurak velhipto": name = "jurrac velphito"; break;
                case "over the rainbow": name = "rainbow refraction"; break;
                case "efflorescent knight": name = "weathering soldier"; break;
                case "cenozoic fossil knight - skullger": name = "fossil warrior skull bone"; break;
                case "mesozoic fossil knight - skullknight": name = "fossil warrior skull knight"; break;
                case "paleozoic fossil dragon - skullgeoth": name = "fossil dragon skullgios"; break;
                case "paleozoic fossil knight - skullking": name = "fossil warrior skull king"; break;
                case "ally of justice decisive arms": name = "ally of justice decisive armor"; break;
                case "cyril, monk of dark world": name = "ceruli, guru of dark world"; break;
                case "mid piece golem": name = "medium piece golem"; break;
                case "ivy shackle": name = "ivy shackles"; break;
                case "solemn judgement": name = "solemn judgment"; break;
                case "luck loan": name = "lucky loan"; break;
                case "fortune future": name = "fortune's future"; break;
                case "one hundred-eyed dragon": name = "hundred eyes dragon"; break;
                case "roar of the earthbound": name = "roar of the earthbound immortal"; break;
                case "spider's lair": name = "spiders' lair"; break;
                case "chaos end master": name = "chaos-end master"; break;
                case "fiend roar deity legion": name = "fabled ragin"; break;
                case "fiend roar deity levuathan": name = "fabled leviathan"; break;
                case "crystal counter": name = "counter gem"; break;
                case "mist valley watchkeeper": name = "mist valley watcher"; break;
                case "mist valley executioner": name = "mist valley executor"; break;
                case "mist valley strange bird": name = "mist valley baby roc"; break;
                case "mist valley wind user": name = "mist valley windmaster"; break;
                case "dragunity tribul": name = "dragunity tribus"; break;
                case "dragunity black spear": name = "dragunity darkspear"; break;
                case "dragunity knight - gáebolg": name = "dragunity knight - gae bulg"; break;
                case "dragunity arma - laevateinn": name = "dragunity arma leyvaten"; break;
                case "dragunity arma - mistilteinn": name = "dragunity arma mystletainn"; break;
                case "water reflection of the ice barrier": name = "dewdark of the ice barrier"; break;
                case "bushi of the ice barrier": name = "samurai of the ice barrier"; break;
                case "defying troops of the ice barrier": name = "shock troops of the ice barrier"; break;
                case "forbidden spell group of the ice barrier": name = "spellbreaker of the ice barrier"; break;
                case "grunard, tiger commander of the ice barrier": name = "general grunard of the ice barrier"; break;
                case "fiend roar deity ashenvale": name = "fabled ashenveil"; break;
                case "fiend roar deity cruz": name = "fabled krus"; break;
                case "fiend roar deity dipu": name = "fabled dyf"; break;
                case "fiend roar deity solcius": name = "fabled soulkius"; break;
                case "fiend roar deity topy": name = "fabled topi"; break;
                case "fiend roar deity urstos": name = "fabled urustos"; break;
                case "fiend roar deity mihztorji": name = "fabled miztoji"; break;
                case "fiend roar deity ordoro": name = "fabled oltro"; break;
                case "naturia flightfly": name = "naturia fruitfly"; break;
                case "naturia hostneedle": name = "naturia horneedle"; break;
                case "naturia triumph": name = "naturia tulip"; break;
                case "liar wire": name = "lair wire"; break;
                case "alligator sword": name = "alligator's sword"; break;
                case "ally of justice d.d. checker": name = "ally of justice quarantine"; break;
                case "ally of justice lethal weapon": name = "ally of justice omni-weapon"; break;
                case "real genex coordinator": name = "r-genex overseer"; break;
                case "real genex vindicate": name = "vindikite r-genex"; break;
                case "ally of justice blind sucker": name = "ally of justice nullfier"; break;
                case "ally of justice reverse brake": name = "ally of justice reverse break"; break;
                case "ally of justice cycle leader": name = "ally of justice cycle reader"; break;
                case "real genex accelerator": name = "r-genex accelerator"; break;
                case "recycle genex": name = "genex recycled"; break;
                case "morphing jar#2": name = "morphing jar #2"; break;
                case "tech genus rush rhino be-04": name = "t.g. rush rhino"; break;
                case "tech genus werewolf bw-03": name = "t.g. warwolf"; break;
                case "tech genus cyber magician sc-01": name = "t.g. cyber magician"; break;
                case "tech genus striker wa-01": name = "t.g. striker"; break;
                case "tech genus blade gunner maxx-10000": name = "t.g. blade blaster"; break;
                case "tech genus power gladiator wax-1000": name = "t.g. power gladiator"; break;
                case "tech genus wonder magician scx-1000": name = "t.g. wonder magician"; break;
                case "machine king - b.c. 3000": name = "machine king - 3000 b.c."; break;
                case "sacred knight's shield bearer": name = "noble knight's shield-bearer"; break;
                case "sacred knight's spearholder": name = "noble knight's spearholder"; break;
                case "vairon delta": name = "vylon delta"; break;
                case "vairon epsilon": name = "vylon epsilon"; break;
                case "vairon sigma": name = "vylon sigma"; break;
                case "machine emperor wisel": name = "meklord emperor wisel"; break;
                case "machine emperor skiel": name = "meklord emperor skiel"; break;
                case "machine emperor grannel": name = "meklord emperor granel"; break;
                case "garm of the nordic beasts": name = "garmr of the nordic beasts"; break;
                case "endless emptiness": name = "infinite machine"; break;
                case "nonexistence": name = "empty machine"; break;
                case "ghost  knight of jackal": name = "ghost knight of jackal"; break;
                case "galaxy-eyes cloud dragon": name = "galaxy-eyes cloudragon"; break;
                case "heraldry augmentation": name = "augmented heraldry"; break;
                case "marina, princess of sunflowers": name = "mariña, princess of sunflowers"; break;
                case "superheavy samurai oni shutendoji": name = "superheavy samurai ogre shutendoji"; break;
                case "naturia gaiastralio": name = "naturia gaiastrio"; break;

                // Token cards aren't important
                case "token":
                case "ecclesia the exiled":
                case "albaz the shrouded":
                    return null;
            }
            if (name.StartsWith("japanese:"))
            {
                // Some header that appears on some deck lists
                return null;
            }
            if (name.Contains("&#91;notes"))
            {
                return null;
            }
            if (name.StartsWith("speed spell - "))
            {
                return null;
            }
            return name;
        }

        static bool IsMissingCard(string name)
        {
            switch (name)
            {
                case "question":
                case "elemental hero air neos":
                case "shiba-warrior taro":
                case "victory dragon":
                case "holactie the creator of light":
                case "convulsion of nature":
                case "queen of fate - eternia":
                case "grizzly, the red star beast":
                    return true;
                //games
                case "steel fan fighter":
                case "speed world":
                case "speed booster":
                case "des accelerator":
                    return true;
                //zexal world
                case "southern cross":
                case "indestructible airship hindenkraft":
                case "unsinkable titanica":
                case "antidote nurse":
                case "cat girl":
                case "monster cat":
                case "stray cat":
                case "stray cat girl":
                case "cat world":
                case "inviting cat":
                case "gold coins for cats":
                case "cat girl magician":
                case "cosmos":
                case "ferocious flora":
                case "fire lily":
                case "zen garden":
                case "tribulldog":
                case "dogking":
                case "doubulldog":
                case "sumo king dog":
                case "playmaker":
                case "tomato in tomato":
                case "tomato paradise":
                case "tomato king":
                    return true;
                //tag force
                case "burst impact":
                case "the rival's name":
                case "neos spiral force":
                case "last machine acid virus":
                case "kiteroid":
                case "dizzy angel":
                case "dizzy tiger":
                case "flipping the table":
                case "hot sauce bottle":
                case "ojamandala":
                case "pride shout":
                case "plasma warrior eitom":
                case "alchemic kettle - chaos distill":
                case "illusion gate":
                case "amazoness arena":
                case "dark scorpion - tragedy of love":
                case "dark scorpion retreat":
                case "archfiend matador":
                case "dark arena":
                case "ritual of the matador":
                case "maiden in love":
                case "cupid kiss":
                case "happy marriage":
                case "defense maiden":
                case "dark spiral force":
                case "malefic force":
                case "hook the hidden knight":
                case "scab scar knight":
                case "b.e.s. assault core":
                case "shield recovery":
                case "trick battle":
                case "masked knight lv3":
                case "masked knight lv5":
                case "masked knight lv7":
                case "destiny mind":
                case "d - mind":
                case "crystal slicer":
                case "arcana force viii - the strength":
                case "arcana force xii - the hanged man":
                case "suit of sword x":
                case "the sky lord":
                case "the material lord":
                case "the spiritual lord":
                case "infinite fiend mirror":
                case "load of darkness":
                case "infinite fiend summoning mirrors":
                case "toy emperor":
                case "toy soldier":
                case "crowning of the emperor":
                case "fog palace":
                case "fog castle":
                case "sacred protective barrier":
                case "sacred defense barrier":
                case "volcanic wall":
                case "zero sprite":
                case "dark archetype":
                case "dark psycho eye":
                case "the unchosen one":
                case "power zone":
                case "clear cube":
                case "clear phantom":
                case "clear rage golem":
                case "clear vicious knight":
                case "attribute bomb":
                case "attribute mastery":
                case "clear wall":
                case "attribute chameleon":
                case "attribute gravity":
                case "synchro spirits":
                case "mach synchron":
                case "high and low":
                case "toichi the nefarious debt collector":
                case "endless loan":
                case "power converter":
                case "central shield":
                case "pain to power":
                case "double ripple":
                case "dark tuner nightmare hand":
                case "infernity des gunman":
                case "infernity zero":
                case "dark tuner dark ape":
                case "dark wave":
                case "cursed prison":
                case "dark matter":
                case "altar of the bound deity":
                case "dark tuner spider cocoon":
                case "gate defender":
                case "discord counter":
                case "defender's mind":
                case "terminal countdown":
                case "dark tuner chaos rogue":
                case "climactic barricade":
                case "arcana force viii - strength":
                case "blizzard lizard":
                case "dark tuner dark goddess witaka":
                case "passion of baimasse":
                case "jester queen":
                case "late penalty":
                case "tuning barrier":
                case "card of distrain":
                case "hidden passage":
                case "jester's panic":
                case "double type rescue":
                case "chain close":
                case "floral shield":
                case "for our dreams":
                case "fleur de vertiges":
                case "imitation":
                case "wise core":
                case "wisel attack":
                case "wisel attack 3":
                case "wisel carrier":
                case "wisel guard":
                case "wisel guard 3":
                case "wisel top":
                case "wisel top 3":
                case "battle return":
                case "convert ghost":
                case "divergence":
                case "labyrinth of kline":
                case "spark breaker":
                case "twin vortex":
                case "chaos blast":
                case "absurd stealer":
                case "skiel attack":
                case "skiel attack 3":
                case "skiel attack 5":
                case "skiel carrier":
                case "skiel carrier 3":
                case "skiel carrier 5":
                case "skiel guard":
                case "skiel top":
                case "sky core":
                case "infinity force":
                case "grand core":
                case "grannel attack":
                case "grannel carrier":
                case "grannel guard":
                case "grannel top":
                case "granel attack":
                case "granel carrier":
                case "granel guard":
                case "granel top":
                case "destruction trigger":
                case "tuning collapse":
                case "superficial peace":
                case "thousand crisscross":
                case "dimension equilibrium":
                case "gjallarhorn":
                case "aurora draw":
                case "future destruction":
                case "cursed synchro":
                case "meklord emperor creation":
                case "time angel":
                case "empress's crown":
                case "empress's staff":
                case "beast burial ritual":
                case "diving exploder":
                case "dark tuner doom submarine":
                case "basara":
                case "giant ushi oni":
                case "southern stars":
                    return true;
                //duel links
                case "d.d.m - different dimension master":
                    return true;
                //anime
                case "scrap fusion":
                case "abyss boat watchman":
                case "abyss guardian":
                case "abyss kid":
                case "abyss ruler mictlancoatl":
                case "bewitching butterfly":
                case "magic law":
                case "spirit wave barrier":
                case "guidance to the abyss":
                    return true;
                //bug
                case "19 fusion monsters":
                case "29 fusion monsters":
                case "24 fusion monsters":
                    return true;
            }
            return false;
        }
    }
}
#endif