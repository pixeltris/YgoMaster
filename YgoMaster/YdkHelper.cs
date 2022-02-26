// https://github.com/pixeltris/Lotd/blob/36c8a54d4fa58345974957c0bb061b2878ee9353/Lotd/YdkHelper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Diagnostics;

namespace YgoMaster
{
    /// <summary>
    /// Helper class for loading YDK files (YGOPro)
    /// </summary>
    static class YdkHelper
    {
        static Dictionary<long, long> ydkIdToOfficialId = new Dictionary<long, long>();
        static Dictionary<long, long> officialIdToYdkId = new Dictionary<long, long>();
        const string idMapFileName = "YdkIds.txt";
        const string customYdkHeader = "YgoMaster";

        enum DeckType
        {
            None,
            Main,
            Extra,
            Side
        }

        public static long GetOfficialId(long ydkId)
        {
            long result;
            ydkIdToOfficialId.TryGetValue(ydkId, out result);
            return result;
        }

        public static long GetYdkId(long officialId)
        {
            long result;
            officialIdToYdkId.TryGetValue(officialId, out result);
            return result;
        }

        public static void LoadDeck(DeckInfo deck)
        {
            if (!File.Exists(deck.File))
            {
                return;
            }
            if (string.IsNullOrEmpty(deck.Name))
            {
                deck.Name = Path.GetFileNameWithoutExtension(deck.File) + ".ydk";
            }
            LoadDeck(deck, File.ReadAllText(deck.File));
        }

        public static void LoadDeck(DeckInfo deck, string str)
        {
            deck.DisplayCards.Clear();
            deck.MainDeckCards.Clear();
            deck.ExtraDeckCards.Clear();
            deck.SideDeckCards.Clear();
            deck.TrayCards.Clear();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            string[] lines = str.Replace("\r", "").Split('\n');
            DeckType deckType = DeckType.Main;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("!"))
                {
                    if (line.Substring(1).Trim().StartsWith("side"))
                    {
                        deckType = DeckType.Side;
                    }
                }
                else if (line.StartsWith("#"))
                {
                    string lineTrimmed = line.Substring(1).Trim();
                    if (lineTrimmed.StartsWith("main"))
                    {
                        deckType = DeckType.Main;
                    }
                    if (lineTrimmed.StartsWith("extra"))
                    {
                        deckType = DeckType.Extra;
                    }
                    else if (lineTrimmed.StartsWith(customYdkHeader))
                    {
                        string possibleJson = line.Substring(line.IndexOf(customYdkHeader) + customYdkHeader.Length).Trim();
                        try
                        {
                            deck.FromDictionaryEx(MiniJSON.Json.Deserialize(possibleJson) as Dictionary<string, object>);
                            return;
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    long ydkCardId, officialCardId;
                    if (long.TryParse(line.Trim(), out ydkCardId) &&
                        ydkIdToOfficialId.TryGetValue(ydkCardId, out officialCardId))
                    {
                        switch (deckType)
                        {
                            case DeckType.Main:
                                deck.MainDeckCards.Add((int)officialCardId);
                                break;
                            case DeckType.Extra:
                                deck.ExtraDeckCards.Add((int)officialCardId);
                                break;
                            case DeckType.Side:
                                deck.SideDeckCards.Add((int)officialCardId);
                                break;
                        }
                    }
                }
            }
        }

        public static void SaveDeck(DeckInfo deck)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#" + customYdkHeader + " " + MiniJSON.Json.Serialize(deck.ToDictionaryEx()));
            sb.AppendLine("#main");
            foreach (int cardId in deck.MainDeckCards.GetIds())
            {
                long ydkCardId;
                if (officialIdToYdkId.TryGetValue(cardId, out ydkCardId))
                {
                    sb.AppendLine(ydkCardId.ToString());
                }
            }
            sb.AppendLine("#extra");
            foreach (int cardId in deck.ExtraDeckCards.GetIds())
            {
                long ydkCardId;
                if (officialIdToYdkId.TryGetValue(cardId, out ydkCardId))
                {
                    sb.AppendLine(ydkCardId.ToString());
                }
            }
            sb.AppendLine("!side");
            foreach (int cardId in deck.SideDeckCards.GetIds())
            {
                long ydkCardId;
                if (officialIdToYdkId.TryGetValue(cardId, out ydkCardId))
                {
                    sb.AppendLine(ydkCardId.ToString());
                }
            }
            File.WriteAllText(deck.File, sb.ToString());
        }

        public static void LoadIdMap(string dataDir)
        {
            ydkIdToOfficialId.Clear();
            officialIdToYdkId.Clear();

            string idMapFile = Path.Combine(dataDir, idMapFileName);
            if (File.Exists(idMapFile))
            {
                string[] lines = File.ReadAllLines(idMapFile);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] splitted = line.Split();
                        if (splitted.Length >= 2)
                        {
                            long ydkId, officialId;
                            if (long.TryParse(splitted[0], out ydkId) &&
                                long.TryParse(splitted[1], out officialId))
                            {
                                ydkIdToOfficialId[ydkId] = officialId;
                                // There are duplicates, the first should be the original card art
                                if (!officialIdToYdkId.ContainsKey(officialId))
                                {
                                    officialIdToYdkId[officialId] = ydkId;
                                }
                            }
                        }
                    }
                }
            }

            if (ydkIdToOfficialId.Count > 0)
            {
                //ValidateCardIds();
            }
        }

        public static void GenerateIdMap(string dataDir)
        {
            ydkIdToOfficialId.Clear();
            officialIdToYdkId.Clear();

            Dictionary<int, GameCardInfo> gameCards = LoadCardDataFromGame(dataDir);
            Dictionary<string, GameCardInfo> altNames = new Dictionary<string, GameCardInfo>();
            foreach (GameCardInfo card in gameCards.Values)
            {
                string name = card.Name;
                bool isAltName = false;
                if (name.Contains("#"))
                {
                    name = name.Replace("#", string.Empty);
                    isAltName = true;
                }
                if (name.Contains("・"))
                {
                    name = name.Replace("・", string.Empty);
                    isAltName = true;
                }
                if (name.Contains("β"))
                {
                    name = name.Replace("β", "Beta");
                    isAltName = true;
                }
                if (name.Contains("α"))
                {
                    name = name.Replace("α", "Alpha");
                    isAltName = true;
                }
                if (name.Contains("Ω"))
                {
                    name = name.Replace("Ω", "Omega");
                    isAltName = true;
                }
                if (name.Contains("Ür"))
                {
                    name = name.Replace("Ür", "Ur");
                    isAltName = true;
                }
                if (name.Contains("Cú"))
                {
                    name = name.Replace("Cú", "Cu");
                    isAltName = true;
                }
                if (name.Contains("ñ"))
                {
                    name = name.Replace("ñ", "n");
                    isAltName = true;
                }
                if (name.Contains("é"))
                {
                    name = name.Replace("é", "e");
                    isAltName = true;
                }
                if (name.Contains("The"))
                {
                    name = name.Replace("The", "the");
                    isAltName = true;
                }
                if (isAltName)
                {
                    altNames[name] = card;
                }
            }
            // Manually fix unmatched cards
            AddAlternativeCardName(gameCards, altNames, 8330, "Falchion Beta");//Falchionβ
            
            // These cards don't have english names yet (listed as "No.XXXXX" where XXXXX is the card id)
            AddCardManually(15613529, 16047);//Time Thief Tempwhaler
            AddCardManually(15613529, 16619);//Mystical Elf - Mystical Burst Stream
            AddCardManually(41002238, 16620);//Kaiser Glider - Golden Burst
            AddCardManually(77406972, 16621);//Giltia the D. Knight - Soul Spear
            AddCardManually(4991081, 16622);//Harpie's Pet Dragon - Fearsome Fire Blast
            
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string json = client.DownloadString("https://db.ygoprodeck.com/api/v7/cardinfo.php");
                Dictionary<string, object> jsonData = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;

                object dataListObj;
                List<object> dataList = null;
                if (jsonData != null && jsonData.TryGetValue("data", out dataListObj) && (dataList = dataListObj as List<object>) != null)
                {
                    foreach (object dataObj in dataList)
                    {
                        Dictionary<string, object> data = dataObj as Dictionary<string, object>;
                        long ydkId = (long)Convert.ChangeType(data["id"], typeof(long));
                        string ydkName = data["name"] as string;
                        GameCardInfo cardInfo = gameCards.Values.FirstOrDefault(x => x.Name.Equals(ydkName, StringComparison.InvariantCultureIgnoreCase));
                        if (cardInfo == null)
                        {
                            altNames.TryGetValue(ydkName, out cardInfo);
                        }
                        if (cardInfo != null)
                        {
                            ydkIdToOfficialId[ydkId] = cardInfo.Id;
                            officialIdToYdkId[cardInfo.Id] = ydkId;

                            object cardImagesObj;
                            if (data.TryGetValue("card_images", out cardImagesObj))
                            {
                                List<object> cardImagesList = cardImagesObj as List<object>;
                                if (cardImagesList != null)
                                {
                                    foreach (object obj in cardImagesList)
                                    {
                                        Dictionary<string, object> cardImageData = obj as Dictionary<string, object>;
                                        if (cardImageData != null && cardImageData.ContainsKey("id"))
                                        {
                                            long imageYdkId = (long)Convert.ChangeType(cardImageData["id"], typeof(long));
                                            ydkIdToOfficialId[imageYdkId] = cardInfo.Id;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            using (TextWriter writer = File.CreateText(Path.Combine(dataDir, idMapFileName)))
            {
                foreach (KeyValuePair<long, long> cardId in ydkIdToOfficialId)
                {
                    writer.WriteLine(cardId.Key + " " + cardId.Value);
                }
            }
            
            if (ydkIdToOfficialId.Count > 0)
            {
                ValidateCardIds(gameCards);
            }
        }

        static bool Exists(Dictionary<int, GameCardInfo> gameCards, string name)
        {
            return gameCards.Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != null;
        }

        private static void AddCardManually(long ydkId, long officialId)
        {
            ydkIdToOfficialId[ydkId] = officialId;
            officialIdToYdkId[officialId] = ydkId;
        }

        private static void AddAlternativeCardName(Dictionary<int, GameCardInfo> gameCards,
            Dictionary<string, GameCardInfo> alternativeCardNames, short id, string name)
        {
            GameCardInfo card;
            if (gameCards.TryGetValue(id, out card))
            {
                alternativeCardNames[name] = card;
            }
        }

        private static void ValidateCardIds(Dictionary<int, GameCardInfo> gameCards)
        {
            int numMissingCards = 0;
            foreach (GameCardInfo card in gameCards.Values)
            {
                if (!officialIdToYdkId.ContainsKey(card.Id) && card.Id != 0)
                {
                    numMissingCards++;
                    Debug.WriteLine("Couldn't find YDK card '" + card.Name + "' (" + card.Id + ")");
                }
            }
            if (numMissingCards > 0)
            {
                Debug.WriteLine("Failed to find " + numMissingCards + " YDK card ids");
            }
        }

        class GameCardInfo
        {
            public int Index;
            public int Id;
            public string Name;
            public string Desc;
        }

        static Dictionary<int, GameCardInfo> LoadCardDataFromGame(string dataDir)
        {
            // https://github.com/pixeltris/Lotd/blob/36c8a54d4fa58345974957c0bb061b2878ee9353/Lotd/FileFormats/bin/CardManager.cs#L221
            Dictionary<int, GameCardInfo> cardsById = new Dictionary<int, GameCardInfo>();
            List<GameCardInfo> cardsByIndex = new List<GameCardInfo>();
            string cardIndxFile = Path.Combine(dataDir, "CardData/en-US/CARD_Indx.bytes");
            string cardNameFile = Path.Combine(dataDir, "CardData/en-US/CARD_Name.bytes");
            string cardDescFile = Path.Combine(dataDir, "CardData/en-US/CARD_Desc.bytes");
            string cardPropFile = Path.Combine(dataDir, "CardData/CARD_Prop.bytes");
            using (BinaryReader indxReader = new BinaryReader(File.OpenRead(cardIndxFile)))
            using (BinaryReader nameReader = new BinaryReader(File.OpenRead(cardNameFile)))
            using (BinaryReader descriptionReader = new BinaryReader(File.OpenRead(cardDescFile)))
            using (BinaryReader cardPropReader = new BinaryReader(File.OpenRead(cardPropFile)))
            {
                Dictionary<uint, string> namesByOffset = ReadStrings(nameReader);
                Dictionary<uint, string> descriptionsByOffset = ReadStrings(descriptionReader);

                while (true)
                {
                    uint nameOffset = indxReader.ReadUInt32();
                    uint descriptionOffset = indxReader.ReadUInt32();
                    if (indxReader.BaseStream.Position >= indxReader.BaseStream.Length)
                    {
                        // The last index points to an invalid offset
                        break;
                    }
                    cardsByIndex.Add(new GameCardInfo()
                    {
                        Index = cardsByIndex.Count,
                        Name = namesByOffset[nameOffset],
                        Desc = descriptionsByOffset[descriptionOffset],
                    });
                }
                foreach (GameCardInfo card in cardsByIndex)
                {
                    int a1 = cardPropReader.ReadInt32();
                    int a2 = cardPropReader.ReadInt32();

                    card.Id = a1 & 0xFFFF;
                    if (card.Id > 0)
                    {
                        cardsById[card.Id] = card;
                    }
                }
            }
            return cardsById;
        }

        static Dictionary<uint, string> ReadStrings(BinaryReader reader)
        {
            Dictionary<uint, string> result = new Dictionary<uint, string>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint offset = (uint)reader.BaseStream.Position;
                string name = ReadNullTerminatedString(reader, Encoding.UTF8);
                result.Add(offset, name);
            }
            return result;
        }

        static string ReadNullTerminatedString(BinaryReader reader, Encoding encoding)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StreamReader streamReader = new StreamReader(reader.BaseStream, encoding);

            long startOffset = reader.BaseStream.Position;

            int intChar;
            while ((intChar = streamReader.Read()) != -1)
            {
                char c = (char)intChar;
                if (c == '\0')
                {
                    break;
                }
                stringBuilder.Append(c);
            }

            string result = stringBuilder.ToString();

            // StreamReader breaks the offset by reading too much. Get the actual amount of bytes read.
            reader.BaseStream.Position = startOffset + encoding.GetByteCount(result + '\0');

            return result;
        }
    }
}
