using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using YgoMaster;
using YgoMasterClient;
using IL2CPP;

namespace TMPro
{
    static unsafe class TMP_Text
    {
        static IL2Method methodGetText;
        static IL2Method methodSetText;

        static TMP_Text()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Unity.TextMeshPro");
            IL2Class classInfo = assembly.GetClass("TMP_Text", "TMPro");
            methodGetText = classInfo.GetProperty("text").GetGetMethod();
            methodSetText = classInfo.GetProperty("text").GetSetMethod();
        }

        public static string GetText(IntPtr thisPtr)
        {
            IL2Object result = methodGetText.Invoke(thisPtr);
            return result != null ? result.GetValueObj<string>() : null;
        }

        public static void SetText(IntPtr thisPtr, string value)
        {
            methodSetText.Invoke(thisPtr, new IntPtr[] { new IL2String(value).ptr });
        }
    }
}

// Helper to display information about trunk in the title of the program (owned cards, filtered, etc)
// TODO: Create a text GameObject and use that instead. Could possibly be done by cloning the UR text, move it horizontally and update the text
namespace YgomGame.Deck
{
    static unsafe class CardCollectionView
    {
        static IL2Class cardBaseDataClassInfo;
        static IL2Method GetDataList;
        delegate void Del_Start(IntPtr thisPtr);
        static Hook<Del_Start> hookStart;
        delegate void Del_UpdateView(IntPtr thisPtr, bool updateDataCount, bool select);
        static Hook<Del_UpdateView> hookUpdateView;

        static CardCollectionView()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            cardBaseDataClassInfo = assembly.GetClass("CardBaseData", "YgomGame.Deck");
            IL2Class classInfo = assembly.GetClass("CardCollectionView", "YgomGame.Deck");
            GetDataList = classInfo.GetProperty("m_DataList").GetGetMethod();
            hookStart = new Hook<Del_Start>(Start, classInfo.GetMethod("Start"));
            hookUpdateView = new Hook<Del_UpdateView>(UpdateView, classInfo.GetMethod("UpdateView"));
        }

        static void Start(IntPtr thisPtr)
        {
            hookStart.Original(thisPtr);
        }

        public static void UpdateView(IntPtr thisPtr, bool updateDataCount, bool select = true)
        {
            hookUpdateView.Original(thisPtr, updateDataCount, select);

            IL2ListExplicit cardList = new IL2ListExplicit(GetDataList.Invoke(thisPtr).ptr, cardBaseDataClassInfo);
            int count = cardList.Count;
            HashSet<int> cids = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                CardBaseData cardData = cardList.GetRef<CardBaseData>(i);
                cids.Add(cardData.CardID);
            }

            DeckEditViewController2.NumFilteredCards = cids.Count;
            DeckEditViewController2.SetExtraText();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CardBaseData
    {
        public int CardID;
        public int PremiumID;
        public bool IsOwned;
        public int Obtained;
        public int Inventory;
        public int Rarity;
    }

    unsafe static class DeckView
    {
        static IL2Class deckCardClassInfo;
        static IL2Class cardBaseDataClassInfo;
        static IL2Field fieldMainDeckCards;
        static IL2Field fieldMainCardDataList;
        static IL2Field fieldExtraDeckCards;
        static IL2Field fieldExtraCardDataList;
        static IL2Method methodSetDismantleMode;
        static IL2Method methodAddToMainDeckByID;
        static IL2Method methodAddToExtraDeckByID;
        static IL2Method methodRemoveCardFromMainOrExtra;
        static IL2Method methodSortMainDeckCards;
        static IL2Method methodSortExtraDeckCards;

        delegate int Del_GetAddableType(IntPtr thisPtr, int cardID, int regulation);
        static Hook<Del_GetAddableType> hookGetAddableType;

        static DeckView()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("DeckView", "YgomGame.Deck");
            hookGetAddableType = new Hook<Del_GetAddableType>(GetAddableType, classInfo.GetMethod("GetAddableType"));
            methodSetDismantleMode = classInfo.GetMethod("SetDismantleMode");
            methodAddToMainDeckByID = classInfo.GetMethod("AddToMainDeckByID");
            methodAddToExtraDeckByID = classInfo.GetMethod("AddToExtraDeckByID");
            methodRemoveCardFromMainOrExtra = classInfo.GetMethod("RemoveCardFromMainOrExtra");
            methodSortMainDeckCards = classInfo.GetMethod("sortMainDeckCards");
            methodSortExtraDeckCards = classInfo.GetMethod("sortExtraDeckCards");

            deckCardClassInfo = assembly.GetClass("DeckCard", "YgomGame.Deck");
            cardBaseDataClassInfo = assembly.GetClass("CardBaseData", "YgomGame.Deck");
            fieldMainDeckCards = classInfo.GetField("mainDeckCards");
            fieldMainCardDataList = classInfo.GetField("mainCardDataList");
            fieldExtraDeckCards = classInfo.GetField("extraDeckCards");
            fieldExtraCardDataList = classInfo.GetField("extraCardDataList");
        }

        static int GetAddableType(IntPtr thisPtr, int cardID, int regulation)
        {
            if (ClientSettings.DeckEditorDisableLimits)
            {
                return 0;// YgomGame.Deck.DeckView.AddableType.Addable
            }
            else
            {
                return hookGetAddableType.Original(thisPtr, cardID, regulation);
            }
        }

        public static void SetCards(IntPtr thisPtr, CardCollection mainDeck, CardCollection extraDeck)
        {
            // TODO: Look into ways of improving the performance of this

            bool b = false;
            methodSetDismantleMode.Invoke(thisPtr, new IntPtr[] { new IntPtr(&b) });

            IL2ListExplicit mainCardDataList = new IL2ListExplicit(fieldMainCardDataList.GetValue(thisPtr).ptr, cardBaseDataClassInfo);
            IL2ListExplicit extraCardDataList = new IL2ListExplicit(fieldExtraCardDataList.GetValue(thisPtr).ptr, cardBaseDataClassInfo);

            for (int i = 0; i < 2; i++)
            {
                IL2ListExplicit targetList = i == 0 ? mainCardDataList : extraCardDataList;
                int numCards = targetList.Count;
                for (int j = 0; j < numCards; j++)
                {
                    CardBaseData cardData = targetList.GetRef<CardBaseData>(0);
                    IntPtr removedCard = IntPtr.Zero;
                    methodRemoveCardFromMainOrExtra.Invoke(thisPtr, new IntPtr[]
                    {
                        new IntPtr(&cardData.CardID), new IntPtr(&cardData.PremiumID), new IntPtr(&removedCard)
                    });
                }
            }

            // Possibly not required? But GetRemainPremiumCard does stuff with CardCollectionInfo (possibly detached from the view)
            DeckEditViewController2.UpdateCollectionView(true, false);

            if (mainDeck != null && extraDeck != null)
            {
                IL2Dictionary<string, object> cardRare = null;
                IntPtr cardRarePtr = YgomSystem.Utility.ClientWork.GetByJsonPath("$.Master.CardRare");
                if (cardRarePtr != IntPtr.Zero)
                {
                    cardRare = new IL2Dictionary<string, object>(cardRarePtr);
                }

                for (int i = 0; i < 2; i++)
                {
                    IL2Method method = i == 0 ? methodAddToMainDeckByID : methodAddToExtraDeckByID;
                    List<KeyValuePair<int, CardStyleRarity>> collection = i == 0 ? mainDeck.GetCollection() : extraDeck.GetCollection();
                    foreach (KeyValuePair<int, CardStyleRarity> card in collection)
                    {
                        if (cardRare != null && !cardRare.ContainsKey(card.Key.ToString()))
                        {
                            continue;
                        }

                        int id = card.Key;
                        int prem = (int)card.Value;
                        bool owned = false;

                        if (ClientSettings.DeckEditorConvertStyleRarity)
                        {
                            // Convert the style rarity (prem) based on owned cards
                            prem = (int)CardStyleRarity.Normal;
                            owned = false;
                            for (int j = (int)CardStyleRarity.Royal; j >= (int)CardStyleRarity.Normal; j--)
                            {
                                if (DeckEditViewController2.GetRemainPremiumCard(id, j) > 0)
                                {
                                    prem = j;
                                    owned = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            owned = DeckEditViewController2.GetRemainPremiumCard(id, prem) > 0;
                        }

                        int reg = 0;//?
                        bool sort = false;
                        bool isIni = false;//? isInitializeSelect?
                        bool noAdd = false;//?
                        method.Invoke(thisPtr, new IntPtr[]
                        {
                            new IntPtr(&id), new IntPtr(&prem), new IntPtr(&owned), new IntPtr(&reg), new IntPtr(&sort),
                            new IntPtr(&isIni), new IntPtr(&noAdd)
                        });
                    }
                }
                methodSortMainDeckCards.Invoke(thisPtr);
                methodSortExtraDeckCards.Invoke(thisPtr);
            }

            // Updates the used card counters in the card collection view
            DeckEditViewController2.UpdateCollectionView(true, false);
        }

        public static DeckInfo GetDeckInfo(IntPtr thisPtr)
        {
            DeckInfo result = new DeckInfo();
            IL2ListExplicit mainCardDataList = new IL2ListExplicit(fieldMainCardDataList.GetValue(thisPtr).ptr, cardBaseDataClassInfo);
            IL2ListExplicit extraCardDataList = new IL2ListExplicit(fieldExtraCardDataList.GetValue(thisPtr).ptr, cardBaseDataClassInfo);
            for (int i = 0; i < 2; i++)
            {
                IL2ListExplicit targetList = i == 0 ? mainCardDataList : extraCardDataList;
                CardCollection collection = i == 0 ? result.MainDeckCards : result.ExtraDeckCards;
                int numCards = targetList.Count;
                for (int j = 0; j < numCards; j++)
                {
                    CardBaseData cardData = targetList.GetRef<CardBaseData>(j);
                    collection.Add(cardData.CardID, (CardStyleRarity)cardData.PremiumID);
                }
            }
            return result;
        }
    }
}

namespace YgomGame
{
    static unsafe class DeckEditViewController2
    {
        public static int NumCards;
        public static int NumOwnedCards;
        public static int NumOwnedCardsEstimate;
        public static int NumFilteredCards;

        static IntPtr currentInstance;
        static IL2Field fieldDeckView;
        static IL2Field fieldCollectionView;
        static IL2Method methodGetRemainPremiumCard;
        static IL2Method methodGetDeckName;
        static IL2Method methodGetDeckID;

        static IntPtr extraTextComponent;
        static IntPtr imageType;// UnityEngine.UI.Image
        static IntPtr textMeshType;// YgomSystem.YGomTMPro.ExtendedTextMeshProUGUI
        static IntPtr rectTransformType;// UnityEngine.CoreModule.RectTransform
        static IL2Method rectTransformSetAnchoredPosition;
        static IL2Method rectTransformSetOffsetMin;
        static IL2Method rectTransformSetSizeDelta;
        static IL2Method tmTextSetEnableWordWrapping;

        delegate void Del_NotificationStackRemove(IntPtr thisPtr);
        static Hook<Del_NotificationStackRemove> hookNotificationStackRemove;
        delegate void Del_InitializeView(IntPtr thisPtr);
        static Hook<Del_InitializeView> hookInitializeView;

        static DeckEditViewController2()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("DeckEditViewController2", "YgomGame");
            fieldDeckView = classInfo.GetField("m_DeckView");
            fieldCollectionView = classInfo.GetField("m_CollectionView");
            methodGetRemainPremiumCard = classInfo.GetMethod("getRemainPremiumCard");
            methodGetDeckName = classInfo.GetProperty("m_DeckName").GetGetMethod();
            methodGetDeckID = classInfo.GetProperty("m_DeckID").GetGetMethod();
            hookInitializeView = new Hook<Del_InitializeView>(InitializeView, classInfo.GetMethod("InitializeView"));
            hookNotificationStackRemove = new Hook<Del_NotificationStackRemove>(NotificationStackRemove, classInfo.GetMethod("NotificationStackRemove"));

            imageType = CastUtils.IL2Typeof("Image", "UnityEngine.UI", "UnityEngine.UI");
            textMeshType = CastUtils.IL2Typeof("ExtendedTextMeshProUGUI", "YgomSystem.YGomTMPro", "Assembly-CSharp");
            rectTransformType = CastUtils.IL2Typeof("RectTransform", "UnityEngine", "UnityEngine.CoreModule");

            IL2Assembly coreModuleAssembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Assembly uiAssembly = Assembler.GetAssembly("UnityEngine.UI");
            IL2Assembly textMeshAssembly = Assembler.GetAssembly("Unity.TextMeshPro");
            IL2Class rectTransformClassInfo = coreModuleAssembly.GetClass("RectTransform", "UnityEngine");
            rectTransformSetAnchoredPosition = rectTransformClassInfo.GetProperty("anchoredPosition").GetSetMethod();
            rectTransformSetOffsetMin = rectTransformClassInfo.GetProperty("offsetMin").GetSetMethod();
            rectTransformSetSizeDelta = rectTransformClassInfo.GetProperty("sizeDelta").GetSetMethod();
            IL2Class tmTextClassInfo = textMeshAssembly.GetClass("TMP_Text", "TMPro");
            tmTextSetEnableWordWrapping = tmTextClassInfo.GetProperty("enableWordWrapping").GetSetMethod();
        }

        static void InitializeView(IntPtr thisPtr)
        {
            hookInitializeView.Original(thisPtr);
            currentInstance = thisPtr;
            InitExtraTextComponent();
            //System.IO.File.WriteAllText("deck-dump.txt", UnityEngine.GameObject.Dump(UnityEngine.Component.GetGameObject(thisPtr)));
        }

        static void InitExtraTextComponent()
        {
            if (!ClientSettings.DeckEditorShowStats)
            {
                return;
            }
            // TODO: Add error handling in the case that some of the details of this change after a patch
            IntPtr craftPointGroupObj = UnityEngine.GameObject.FindGameObjectByName(UnityEngine.Component.GetGameObject(currentInstance), "CraftPointGroup");
            IntPtr windowObj = UnityEngine.Component.GetGameObject(UnityEngine.Transform.GetParent(UnityEngine.GameObject.GetTranform(craftPointGroupObj)));

            IntPtr craftPointGroupObj2 = UnityEngine.UnityObject.Instantiate(craftPointGroupObj, UnityEngine.GameObject.GetTranform(windowObj));
            UnityEngine.UnityObject.Destroy(UnityEngine.GameObject.GetComponent(craftPointGroupObj2, imageType));// Get rid of the backgound

            // Get rid of unused objects
            UnityEngine.UnityObject.Destroy(UnityEngine.GameObject.FindGameObjectByName(craftPointGroupObj2, "CraftPointR"));
            UnityEngine.UnityObject.Destroy(UnityEngine.GameObject.FindGameObjectByName(craftPointGroupObj2, "CraftPointSR"));
            UnityEngine.UnityObject.Destroy(UnityEngine.GameObject.FindGameObjectByName(craftPointGroupObj2, "CraftPointUR"));

            IntPtr objCpN = UnityEngine.GameObject.FindGameObjectByName(craftPointGroupObj2, "CraftPointN");
            IntPtr objCpNIcon = UnityEngine.GameObject.FindGameObjectByName(objCpN, "Icon");
            IntPtr objCpNText = UnityEngine.GameObject.FindGameObjectByName(objCpN, "NumText");
            UnityEngine.UnityObject.Destroy(objCpNIcon);// Get rid of the icon

            extraTextComponent = UnityEngine.GameObject.GetComponent(objCpNText, textMeshType);

            // Disable word wrapping
            bool enableWordWrapping = false;
            tmTextSetEnableWordWrapping.Invoke(extraTextComponent, new IntPtr[] { new IntPtr(&enableWordWrapping) });

            // Increase the text area size
            IntPtr rectTransform = UnityEngine.GameObject.GetComponent(objCpNText, rectTransformType);
            YgoMasterClient.AssetHelper.Vector2 sizeDelta = new AssetHelper.Vector2(1000, 200);
            rectTransformSetSizeDelta.Invoke(rectTransform, new IntPtr[] { new IntPtr(&sizeDelta) });

            // Reposition the text (move it more to the left, it's currently positioned accounting for the icon)
            YgoMasterClient.AssetHelper.Vector2 anchorPos = new AssetHelper.Vector2(0, 0);
            rectTransformSetAnchoredPosition.Invoke(rectTransform, new IntPtr[] { new IntPtr(&anchorPos) });
            //YgoMasterClient.AssetHelper.Vector2 offsetMin = new AssetHelper.Vector2(0, -100);
            //rectTransformSetOffsetMin.Invoke(rectTransform, new IntPtr[] { new IntPtr(&offsetMin) });

            /*{x:52,y:0} - anchoredPosition
            {x:1,y:1} - anchorMax
            {x:0,y:0} - anchorMin
            {x:1052,y:100} - offsetMax
            {x:52,y:-100} - offsetMin
            {x:0,y:0.5} - pivot*/

            SetExtraText();
        }

        static void NotificationStackRemove(IntPtr thisPtr)
        {
            currentInstance = IntPtr.Zero;
            extraTextComponent = IntPtr.Zero;
            hookNotificationStackRemove.Original(thisPtr);
        }

        public static void SetExtraText()
        {
            if (!ClientSettings.DeckEditorShowStats)
            {
                return;
            }
            IntPtr cardRarePtr = YgomSystem.Utility.ClientWork.GetByJsonPath("$.Master.CardRare");
            if (cardRarePtr != IntPtr.Zero)
            {
                IL2Dictionary<string, object> cardRare = new IL2Dictionary<string, object>(cardRarePtr);
                NumCards = cardRare.Count;
            }
            else
            {
                NumCards = 0;
            }
            IntPtr cardsHavePtr = YgomSystem.Utility.ClientWork.GetByJsonPath("$.Cards.have");
            if (cardsHavePtr != IntPtr.Zero)
            {
                IL2Dictionary<string, object> cardsHaveIL2 = new IL2Dictionary<string, object>(cardsHavePtr);
                int count = cardsHaveIL2.Count;
                if (count != NumOwnedCardsEstimate)
                {
                    NumOwnedCardsEstimate = count;
                    NumOwnedCards = 0;
                    Dictionary<string, object> cardsHave = MiniJSON.Json.Deserialize(YgomMiniJSON.Json.Serialize(cardsHavePtr)) as Dictionary<string, object>;
                    if (cardsHave != null)
                    {
                        foreach (KeyValuePair<string, object> entry in cardsHave)
                        {
                            int cardId;
                            Dictionary<string, object> cardData = entry.Value as Dictionary<string, object>;
                            if (int.TryParse(entry.Key, out cardId) && cardData != null)
                            {
                                int totalNum = Utils.GetValue<int>(cardData, "tn");
                                if (totalNum > 0)
                                {
                                    NumOwnedCards++;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                NumOwnedCards = 0;
            }
            SetExtraText(NumOwnedCards + " / " + NumCards + " owned (" + NumFilteredCards + " filtered)");
        }

        public static void SetExtraText(string text)
        {
            if (ClientSettings.DeckEditorShowStats && currentInstance != IntPtr.Zero && extraTextComponent != IntPtr.Zero)
            {
                TMPro.TMP_Text.SetText(extraTextComponent, text);
            }
        }

        public static void SetCards(CardCollection mainDeck, CardCollection extraDeck)
        {
            if (currentInstance == IntPtr.Zero)
            {
                return;
            }
            IntPtr deckView = fieldDeckView.GetValue(currentInstance).ptr;
            Deck.DeckView.SetCards(deckView, mainDeck, extraDeck);
        }

        public static DeckInfo GetDeckInfo()
        {
            if (currentInstance == IntPtr.Zero)
            {
                return null;
            }
            IntPtr deckView = fieldDeckView.GetValue(currentInstance).ptr;
            return Deck.DeckView.GetDeckInfo(deckView);
        }

        public static int GetRemainPremiumCard(int id, int prem)
        {
            if (currentInstance == IntPtr.Zero)
            {
                return 0;
            }
            return methodGetRemainPremiumCard.Invoke(currentInstance, new IntPtr[] { new IntPtr(&id), new IntPtr(&prem) }).GetValueRef<int>();
        }

        public static void UpdateCollectionView(bool updateDataCount, bool select = true)
        {
            if (currentInstance == IntPtr.Zero)
            {
                return;
            }
            IntPtr collectionView = fieldCollectionView.GetValue(currentInstance).ptr;
            Deck.CardCollectionView.UpdateView(collectionView, updateDataCount, select);
        }

        public static string GetDeckName()
        {
            if (currentInstance == IntPtr.Zero)
            {
                return null;
            }
            IL2Object result = methodGetDeckName.Invoke(currentInstance);
            return result != null ? result.GetValueObj<string>() : null;
        }

        public static int GetDeckID()
        {
            if (currentInstance == IntPtr.Zero)
            {
                return 0;
            }
            IL2Object result = methodGetDeckName.Invoke(currentInstance);
            return result != null ? result.GetValueRef<int>() : 0;
        }
    }
}

namespace YgomGame.SubMenu
{
    unsafe static class DeckEditSubMenuViewController
    {
        static IL2Class classInfo;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;

        // Used when importing cards by name
        class BasicCardInfo
        {
            public int Id;
            public string Name;
            public bool IsExtraDeck;
        }
        static bool allCardNamesLoaded;
        static Dictionary<int, BasicCardInfo> allCards = new Dictionary<int, BasicCardInfo>();
        static Dictionary<string, BasicCardInfo> allCardsByNameLower = new Dictionary<string, BasicCardInfo>();

        static DeckEditSubMenuViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("DeckEditSubMenuViewController", "YgomGame.SubMenu");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, classInfo.GetMethod("OnCreatedView"));
        }

        static bool LoadAllCardNames()
        {
            if (allCardNamesLoaded)
            {
                return allCards.Count > 0;
            }
            Dictionary<string, object> cardRare = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Master.CardRare")) as Dictionary<string, object>;
            if (cardRare != null)
            {
                IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                IL2Class contentClassInfo = assembly.GetClass("Content", "YgomGame.Card");
                IntPtr contentInstance = contentClassInfo.GetField("s_instance").GetValue().ptr;
                IL2Method methodGetName = contentClassInfo.GetMethod("GetName");
                IL2Method methodIsExtraDeckCard = contentClassInfo.GetMethod("IsExtraDeckCard");
                foreach (KeyValuePair<string, object> entry in cardRare)
                {
                    int cardId;
                    if (int.TryParse(entry.Key, out cardId))
                    {
                        bool replaceAlnum = true;
                        IL2Object stringObj = methodGetName.Invoke(contentInstance, new IntPtr[] { new IntPtr(&cardId), new IntPtr(&replaceAlnum) });
                        if (stringObj != null)
                        {
                            string name = stringObj.GetValueObj<string>();
                            BasicCardInfo cardInfo = new BasicCardInfo()
                            {
                                Id = cardId,
                                Name = name,
                                IsExtraDeck = methodIsExtraDeckCard.Invoke(contentInstance, new IntPtr[] { new IntPtr(&cardId) }).GetValueRef<bool>()
                            };
                            allCards[cardId] = cardInfo;
                            allCardsByNameLower[name.ToLowerInvariant()] = cardInfo;
                        }
                    }
                }
                allCardNamesLoaded = true;
            }
            return allCards.Count > 0;
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            hookOnCreatedView.Original(thisPtr);
            SubMenuViewController.AddMenuItem(thisPtr, "From clipboard (YDKe / YDK / JSON / TXT)", OnLoadFromClipboard);
            SubMenuViewController.AddMenuItem(thisPtr, "To clipboard (YDKe)", OnSaveToClipboardYDKe);
            SubMenuViewController.AddMenuItem(thisPtr, "Load file", OnLoad);
            SubMenuViewController.AddMenuItem(thisPtr, "Save file", OnSave);
            //SubMenuViewController.AddMenuItem(thisPtr, "Open decks folder", OnOpenDecksFolder);
            SubMenuViewController.AddMenuItem(thisPtr, "Clear deck", OnClear);
            SubMenuViewController.AddMenuItem(thisPtr, "Card collection stats", OnCardCollectionStats);
        }

        static void LoadDeckFromFile(string path)
        {
            if (File.Exists(path))
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                switch (extension)
                {
                    case ".txt":
                    case ".ydk":
                    case ".json":
                        LoadDeckFromString(File.ReadAllText(path));
                        break;
                }
            }
        }

        static void LoadDeckFromString(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                DeckInfo deck = new DeckInfo();
                text = text.Trim().Trim('\r', '\n');
                if (text.StartsWith("ydke://", StringComparison.OrdinalIgnoreCase))
                {
                    string[] splitted = text.Substring(7).Split(new char[] { '!' });
                    for (int i = 0; i < 3; i++)
                    {
                        if (i < splitted.Length && !string.IsNullOrEmpty(splitted[i]))
                        {
                            CardCollection collection = null;
                            switch (i)
                            {
                                case 0: collection = deck.MainDeckCards; break;
                                case 1: collection = deck.ExtraDeckCards; break;
                                case 2: collection = deck.SideDeckCards; break;
                            }
                            byte[] buffer = Convert.FromBase64String(splitted[i]);
                            for (int j = 0; j < buffer.Length; j += 4)
                            {
                                int id = (int)YdkHelper.GetOfficialId(BitConverter.ToInt32(buffer, j));
                                if (id >= 0)
                                {
                                    collection.Add(id);
                                }
                            }
                        }
                    }
                }
                else if (text.StartsWith("{") && text.Contains("\"m\""))
                {
                    deck.FromDictionaryEx(MiniJSON.Json.DeserializeStripped(text) as Dictionary<string, object>);
                }
                else if (text.Contains("<table") && text.Contains("</table>"))
                {
                    if (LoadAllCardNames())
                    {
                        int numFailed = 0;
                        StringBuilder failedSb = new StringBuilder();
                        string[] tables = Utils.FindAllContentBetween(text, 0, text.Length, "<table", "</table>");
                        foreach (string table in tables)
                        {
                            string[] tableEntries = Utils.FindAllContentBetween(table, 0, table.Length, "<tr", "</tr>");
                            if (tableEntries.Length == 0)
                            {
                                failedSb.AppendLine("Failed to rows");
                                numFailed++;
                                continue;
                            }
                            string[] headers = Utils.FindAllContentBetween(tableEntries[0], 0, tableEntries[0].Length, "<th", "</th>");
                            int nameColumnIndex = -1;
                            int quantityColumnIndex = -1;
                            for (int i = 0; i < headers.Length; i++)
                            {
                                string header = headers[i] + "<";
                                string[] items = Utils.FindAllContentBetween(header, 0, header.Length, ">", "<", int.MaxValue, true);
                                if (items.Length > 0)
                                {
                                    switch (items.Last().ToLowerInvariant())
                                    {
                                        case "english name":
                                        case "name":
                                        case "card":
                                            nameColumnIndex = i;
                                            break;
                                        case "qty":
                                        case "quantity":
                                        case "count":
                                            quantityColumnIndex = i;
                                            break;
                                    }
                                }
                            }
                            if (nameColumnIndex >= 0)
                            {
                                foreach (string tableEntry in tableEntries)
                                {
                                    string[] columns = Utils.FindAllContentBetween(tableEntry, 0, tableEntry.Length, "<td", "</td>");
                                    if (columns.Length > nameColumnIndex && (columns.Length > quantityColumnIndex || quantityColumnIndex == -1))
                                    {
                                        string name = Utils.GetInnerText(columns[nameColumnIndex].TrimStart('>')).ToLowerInvariant();
                                        string quantityStr =quantityColumnIndex == -1 ? "1" : Utils.GetInnerText(columns[quantityColumnIndex].TrimStart('>'));
                                        int quantity;
                                        BasicCardInfo cardInfo;
                                        if (int.TryParse(quantityStr, out quantity) && allCardsByNameLower.TryGetValue(name, out cardInfo))
                                        {
                                            if (cardInfo.IsExtraDeck)
                                            {
                                                for (int i = 0; i < quantity; i++)
                                                {
                                                    deck.ExtraDeckCards.Add(cardInfo.Id);
                                                }
                                            }
                                            else
                                            {
                                                for (int i = 0; i < quantity; i++)
                                                {
                                                    deck.MainDeckCards.Add(cardInfo.Id);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            failedSb.AppendLine(name + " (x" + quantityStr + ")");
                                            numFailed++;
                                        }
                                    }
                                    else if (!tableEntry.Contains("<th"))
                                    {
                                        failedSb.AppendLine("Failed to find name / quantity for row");
                                        numFailed++;
                                    }
                                }
                            }
                            else
                            {
                                failedSb.AppendLine("Failed to find name / quantity columns");
                                numFailed++;
                            }
                        }
                        if (numFailed > 0)
                        {
                            Clipboard.SetText(failedSb.ToString());
                            YgomGame.Menu.CommonDialogViewController.OpenConfirmationDialogScroll("Info", "Failed to copy " + numFailed +
                                " rows. Text has been copied to the clipboard.\n\n" + failedSb.ToString(), "OK", null, null, true, 500);
                        }
                    }
                    else
                    {
                        YgomGame.Menu.CommonDialogViewController.OpenErrorDialog("Error", "Failed to load card data", "OK", null);
                        return;
                    }
                }
                else if (text.Contains("#main"))
                {
                    YdkHelper.LoadDeck(deck, text);
                }
                else
                {
                    if (LoadAllCardNames())
                    {
                        int numFailed = 0;
                        StringBuilder failedSb = new StringBuilder();
                        Dictionary<string, int> cardNames = Utils.GetCardNamesLowerAndCount(text);
                        foreach (KeyValuePair<string, int> cardName in cardNames)
                        {
                            string name = cardName.Key;
                            int count = cardName.Value;
                            BasicCardInfo cardInfo;
                            if (allCardsByNameLower.TryGetValue(name, out cardInfo))
                            {
                                if (cardInfo.IsExtraDeck)
                                {
                                    for (int i = 0; i < count; i++)
                                    {
                                        deck.ExtraDeckCards.Add(cardInfo.Id);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < count; i++)
                                    {
                                        deck.MainDeckCards.Add(cardInfo.Id);
                                    }
                                }
                            }
                            else
                            {
                                failedSb.AppendLine(name + " (x" + count + ")");
                                numFailed++;
                            }
                        }
                        if (numFailed > 0)
                        {
                            Clipboard.SetText(failedSb.ToString());
                            YgomGame.Menu.CommonDialogViewController.OpenConfirmationDialogScroll("Info", "Failed to copy " + numFailed +
                                " rows. Text has been copied to the clipboard.\n\n" + failedSb.ToString(), "OK", null, null, true, 500);
                        }
                    }
                    else
                    {
                        YgomGame.Menu.CommonDialogViewController.OpenErrorDialog("Error", "Failed to load card data", "OK", null);
                        return;
                    }
                }
                if (deck.GetAllCards().Count > 0)
                {
                    YgomGame.DeckEditViewController2.SetCards(deck.MainDeckCards, deck.ExtraDeckCards);
                    PopViewController();
                }
            }
            catch (Exception e)
            {
                YgomGame.Menu.CommonDialogViewController.OpenConfirmationDialogScroll("Error", "An exception occurred\n" + e, "OK", null, null, true, 500);
            }
        }

        static Action OnLoad = () =>
        {
            // Duplicate of DuelStarter code...
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter =
                "JSON (*.json)|*.json" +
                "|YDK (*.ydk)|*.ydk" +
                "|JSON / YDK (*.json;*.ydk)|*.json;*.ydk" +
                "|All Files (*.*)|*.*";
            ofd.FilterIndex = 3;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
            {
                LoadDeckFromFile(ofd.FileName);
            }
        };

        static Action OnLoadFromClipboard = () =>
        {
            if (Clipboard.ContainsFileDropList())
            {
                System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();
                if (files.Count > 0)
                {
                    LoadDeckFromFile(files[0]);
                }
            }
            else if (Clipboard.ContainsText())
            {
                LoadDeckFromString(Clipboard.GetText());
            }
        };

        static Action OnSaveToClipboardYDKe = () =>
        {
            DeckInfo deck = YgomGame.DeckEditViewController2.GetDeckInfo();
            if (deck != null && deck.GetAllCards().Count > 0)
            {
                List<byte[]> buffers = new List<byte[]>();
                for (int i = 0; i < 3; i++)
                {
                    CardCollection collection = null;
                    switch (i)
                    {
                        case 0: collection = deck.MainDeckCards; break;
                        case 1: collection = deck.ExtraDeckCards; break;
                        case 2: collection = deck.SideDeckCards; break;
                    }
                    byte[] buffer = new byte[collection.Count * 4];
                    int index = 0;
                    foreach (KeyValuePair<int, CardStyleRarity> card in collection.GetCollection())
                    {
                        // TODO: Handle cases where there's no YDK id for an official ID? (probably unlikely)
                        Buffer.BlockCopy(BitConverter.GetBytes((int)YdkHelper.GetYdkId(card.Key)), 0, buffer, index, 4);
                        index += 4;
                    }
                    buffers.Add(buffer);
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("ydke://");
                foreach (byte[] buffer in buffers)
                {
                    sb.Append(Convert.ToBase64String(buffer) + "!");
                }
                Clipboard.SetText(sb.ToString());
            }
        };

        static Action OnSave = () =>
        {
            try
            {
                DeckInfo deck = DeckEditViewController2.GetDeckInfo();
                deck.Name = DeckEditViewController2.GetDeckName();
                int deckId = DeckEditViewController2.GetDeckID();
                if (deckId > 0)
                {
                    // Also see YgomGame.Deck.DeckInfo for other things we may want
                    deck.Accessory.FromDictionary(MiniJSON.Json.Deserialize(
                        YgomSystem.Utility.ClientWork.SerializePath("$.Deck.list." + deckId + ".accessory")) as Dictionary<string, object>);
                }
                if (deck.GetAllCards().Count > 0)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter =
                        "JSON (*.json)|*.json" +
                        "|YDK (*.ydk)|*.ydk";
                    sfd.FilterIndex = 2;
                    sfd.RestoreDirectory = true;
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        deck.File = sfd.FileName;
                        string extension = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                        switch (extension)
                        {
                            case ".ydk":
                                YdkHelper.SaveDeck(deck);
                                break;
                            case ".json":
                                File.WriteAllText(deck.File, MiniJSON.Json.Serialize(deck.ToDictionaryEx()));
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                YgomGame.Menu.CommonDialogViewController.OpenConfirmationDialogScroll("Error", "An exception occurred\n" + e, "OK", null, null, true, 500);
            }
        };

        static Action OnClear = () =>
        {
            YgomGame.Menu.CommonDialogViewController.OpenYesNoConfirmationDialog("Confirmation", "Are you sure you want to clear the deck?", OnClearConfirm);
        };
        static Action OnClearConfirm = () =>
        {
            YgomGame.DeckEditViewController2.SetCards(null, null);
            PopViewController();
        };

        static Action OnOpenDecksFolder = () =>
        {
            string path = Path.GetFullPath(Path.Combine(Program.DataDir, "Decks"));
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
            }
            if (Directory.Exists(path))
            {
                Process.Start("explorer", path);
            }
            else
            {
                YgomGame.Menu.CommonDialogViewController.OpenErrorDialog("Error", "Failed to find / create decks folder", "OK", null);
            }
        };

        static string GetPercentStr(int num, int owned)
        {
            string valStr = "100";
            if (owned == 0)
            {
                valStr = "0";
            }
            else if (owned < num)
            {
                valStr = (((double)owned / (double)num) * 100.0).ToString("N2");
            }
            return " (" + valStr + "%)";
        }

        static Action OnCardCollectionStats = () =>
        {
            // TODO: Add section at the end of the dialog which states the number of cards which can / cannot be dismantled?

            Dictionary<CardRarity, int> numCards = new Dictionary<CardRarity, int>();
            Dictionary<CardRarity, int> numOwned = new Dictionary<CardRarity, int>();
            Dictionary<CardRarity, int> numOwnedDup = new Dictionary<CardRarity, int>();
            Dictionary<CardRarity, int> numOwnedExtra = new Dictionary<CardRarity, int>();
            foreach (CardRarity rarity in Enum.GetValues(typeof(CardRarity)))
            {
                numCards[rarity] = 0;
                numOwned[rarity] = 0;
                numOwnedDup[rarity] = 0;
                numOwnedExtra[rarity] = 0;
            }

            Dictionary<CardStyleRarity, int> numOwnedStyle = new Dictionary<CardStyleRarity, int>();
            Dictionary<CardStyleRarity, int> numOwnedDupStyle = new Dictionary<CardStyleRarity, int>();
            Dictionary<CardStyleRarity, int> numOwnedExtraStyle = new Dictionary<CardStyleRarity, int>();
            foreach (CardStyleRarity styleRarity in Enum.GetValues(typeof(CardStyleRarity)))
            {
                numOwnedStyle[styleRarity] = 0;
                numOwnedDupStyle[styleRarity] = 0;
                numOwnedExtraStyle[styleRarity] = 0;
            }

            int numOwnedTotal = 0;

            Dictionary<string, object> cardRare = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Master.CardRare")) as Dictionary<string, object>;
            if (cardRare != null)
            {
                foreach (KeyValuePair<string, object> entry in cardRare)
                {
                    int cardId;
                    if (int.TryParse(entry.Key, out cardId))
                    {
                        CardRarity rarity = (CardRarity)(int)Convert.ChangeType(entry.Value, typeof(int));
                        numCards[CardRarity.None]++;
                        numCards[rarity]++;
                    }
                }
            }
            Dictionary<string, object> cardsHave = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Cards.have")) as Dictionary<string, object>;
            if (cardsHave != null)
            {
                foreach (KeyValuePair<string, object> entry in cardsHave)
                {
                    int cardId;
                    Dictionary<string, object> cardData = entry.Value as Dictionary<string, object>;
                    if (int.TryParse(entry.Key, out cardId) && cardData != null)
                    {
                        int totalNum = Utils.GetValue<int>(cardData, "tn");
                        if (totalNum > 0)
                        {
                            CardRarity rarity = (CardRarity)Utils.GetValue<int>(cardRare, entry.Key);
                            numOwned[CardRarity.None]++;
                            numOwned[rarity]++;
                            numOwnedDup[CardRarity.None] += Math.Min(3, totalNum);
                            numOwnedDup[rarity] += Math.Min(3, totalNum);
                            numOwnedExtra[CardRarity.None] += Math.Max(0, totalNum - 3);
                            numOwnedExtra[rarity] += Math.Max(0, totalNum - 3);
                            numOwnedTotal += totalNum;
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            string dismantleNumKey = null, noDismantleNumKey = null;
                            CardStyleRarity styleRarity = CardStyleRarity.None;
                            switch (i)
                            {
                                case 0:
                                    dismantleNumKey = "n";
                                    noDismantleNumKey = "p_n";
                                    styleRarity = CardStyleRarity.Normal;
                                    break;
                                case 1:
                                    dismantleNumKey = "p1n";
                                    noDismantleNumKey = "p_p1n";
                                    styleRarity = CardStyleRarity.Shine;
                                    break;
                                case 2:
                                    dismantleNumKey = "p2n";
                                    noDismantleNumKey = "p_p2n";
                                    styleRarity = CardStyleRarity.Royal;
                                    break;
                            }
                            int num = Math.Max(0, Utils.GetValue<int>(cardData, dismantleNumKey)) + Math.Max(0, Utils.GetValue<int>(cardData, noDismantleNumKey));
                            if (num > 0)
                            {
                                numOwnedStyle[styleRarity]++;
                                numOwnedDupStyle[styleRarity] += Math.Min(3, num);
                                numOwnedExtraStyle[styleRarity] += Math.Max(0, num - 3);
                            }
                        }
                    }
                }
            }

            // NOTE: Removed percentages on the card pool. It's nice to have, but confusing relative to the other percentages.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Owned total: " + numOwnedTotal);
            sb.AppendLine();
            sb.AppendLine("[Card pool]");
            sb.AppendLine("Total: " + numCards[CardRarity.None]);
            sb.AppendLine("N: " + numCards[CardRarity.Normal]);// + GetPercentStr(numCards[CardRarity.None], numCards[CardRarity.Normal]));
            sb.AppendLine("R: " + numCards[CardRarity.Rare]);// + GetPercentStr(numCards[CardRarity.None], numCards[CardRarity.Rare]));
            sb.AppendLine("SR: " + numCards[CardRarity.SuperRare]);// + GetPercentStr(numCards[CardRarity.None], numCards[CardRarity.SuperRare]));
            sb.AppendLine("UR: " + numCards[CardRarity.UltraRare]);// + GetPercentStr(numCards[CardRarity.None], numCards[CardRarity.UltraRare]));
            sb.AppendLine();
            sb.AppendLine("[Owned cards (1x)]");
            sb.AppendLine("Total: " + numOwned[CardRarity.None] + GetPercentStr(numCards[CardRarity.None], numOwned[CardRarity.None]));
            sb.AppendLine("N: " + numOwned[CardRarity.Normal] + GetPercentStr(numCards[CardRarity.Normal], numOwned[CardRarity.Normal]));
            sb.AppendLine("R: " + numOwned[CardRarity.Rare] + GetPercentStr(numCards[CardRarity.Rare], numOwned[CardRarity.Rare]));
            sb.AppendLine("SR: " + numOwned[CardRarity.SuperRare] + GetPercentStr(numCards[CardRarity.SuperRare], numOwned[CardRarity.SuperRare]));
            sb.AppendLine("UR: " + numOwned[CardRarity.UltraRare] + GetPercentStr(numCards[CardRarity.UltraRare], numOwned[CardRarity.UltraRare]));
            sb.AppendLine();
            sb.AppendLine("[Owned cards (up to 3x)]");
            sb.AppendLine("Total: " + numOwnedDup[CardRarity.None] + GetPercentStr(numCards[CardRarity.None] * 3, numOwnedDup[CardRarity.None]));
            sb.AppendLine("N: " + numOwnedDup[CardRarity.Normal] + GetPercentStr(numCards[CardRarity.Normal] * 3, numOwnedDup[CardRarity.Normal]));
            sb.AppendLine("R: " + numOwnedDup[CardRarity.Rare] + GetPercentStr(numCards[CardRarity.Rare] * 3, numOwnedDup[CardRarity.Rare]));
            sb.AppendLine("SR: " + numOwnedDup[CardRarity.SuperRare] + GetPercentStr(numCards[CardRarity.SuperRare] * 3, numOwnedDup[CardRarity.SuperRare]));
            sb.AppendLine("UR: " + numOwnedDup[CardRarity.UltraRare] + GetPercentStr(numCards[CardRarity.UltraRare] * 3, numOwnedDup[CardRarity.UltraRare]));
            sb.AppendLine();
            sb.AppendLine("[Extra cards (over 3x)]");
            sb.AppendLine("Total: " + numOwnedExtra[CardRarity.None]);
            sb.AppendLine("N: " + numOwnedExtra[CardRarity.Normal]);
            sb.AppendLine("R: " + numOwnedExtra[CardRarity.Rare]);
            sb.AppendLine("SR: " + numOwnedExtra[CardRarity.SuperRare]);
            sb.AppendLine("UR: " + numOwnedExtra[CardRarity.UltraRare]);
            sb.AppendLine();
            sb.AppendLine("[Rarity (1x)]");
            sb.AppendLine("Normal: " + numOwnedStyle[CardStyleRarity.Normal] + GetPercentStr(numCards[CardRarity.None], numOwnedStyle[CardStyleRarity.Normal]));
            sb.AppendLine("Shine: " + numOwnedStyle[CardStyleRarity.Shine] + GetPercentStr(numCards[CardRarity.None], numOwnedStyle[CardStyleRarity.Shine]));
            sb.AppendLine("Royal: " + numOwnedStyle[CardStyleRarity.Royal] + GetPercentStr(numCards[CardRarity.SuperRare] + numCards[CardRarity.UltraRare], numOwnedStyle[CardStyleRarity.Royal]));
            sb.AppendLine();
            sb.AppendLine("[Rarity (up to 3x)]");
            sb.AppendLine("Normal: " + numOwnedDupStyle[CardStyleRarity.Normal] + GetPercentStr(numCards[CardRarity.None] * 3, numOwnedDupStyle[CardStyleRarity.Normal]));
            sb.AppendLine("Shine: " + numOwnedDupStyle[CardStyleRarity.Shine] + GetPercentStr(numCards[CardRarity.None] * 3, numOwnedDupStyle[CardStyleRarity.Shine]));
            sb.AppendLine("Royal: " + numOwnedDupStyle[CardStyleRarity.Royal] + GetPercentStr((numCards[CardRarity.SuperRare] + numCards[CardRarity.UltraRare]) * 3, numOwnedDupStyle[CardStyleRarity.Royal]));
            sb.AppendLine();
            sb.AppendLine("[Rarity (over 3x)]");
            sb.AppendLine("Normal: " + numOwnedExtraStyle[CardStyleRarity.Normal]);
            sb.AppendLine("Shine: " + numOwnedExtraStyle[CardStyleRarity.Shine]);
            sb.AppendLine("Royal: " + numOwnedExtraStyle[CardStyleRarity.Royal]);
            sb.AppendLine();
            sb.AppendLine("======== Index ========");
            sb.AppendLine("- \"Card pool\" is all cards in the game");
            sb.AppendLine("- \"Owned cards (1x)\" is all cards you own capped to 1x per card");
            sb.AppendLine("- \"Owned cards (up to 3x)\" is all cards you own capped to 3x per card");
            sb.AppendLine("- \"Extra cards (over 3x)\" is all cards you own over 3x (excludes the first 3x)");
            sb.AppendLine("- \"Rarity (1x)\" is all rarities you own capped to 1x per card");
            sb.AppendLine("- \"Rarity (up to 3x)\" is all rarities you own capped to 3x per card");
            sb.AppendLine("- \"Rarity (over 3x)\" is all rarities you own over 3x (excludes the first 3x)");
            YgomGame.Menu.CommonDialogViewController.OpenConfirmationDialogScroll("Card collection info", sb.ToString(), "OK", null, null, true, 720);
        };

        static void PopViewController()
        {
            IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
            IntPtr view = YgomSystem.UI.ViewControllerManager.GetViewController(manager, classInfo.IL2Typeof());
            if (view != IntPtr.Zero)
            {
                YgomSystem.UI.ViewControllerManager.PopChildViewController(manager, view);
            }
        }
    }

    unsafe static class SubMenuViewController
    {
        static IL2Method methodAddMenuItem;

        static SubMenuViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("SubMenuViewController", "YgomGame.SubMenu");
            methodAddMenuItem = classInfo.GetMethod("AddMenuItem");
        }

        public static void AddMenuItem(IntPtr thisPtr, string text, Action clickCallback, string onClickSL = null)
        {
            //IntPtr callback = DelegateHelper.DelegateIl2cppDelegate(clickCallback, clickCallback.GetType().GetClass());
            IntPtr callback = UnityEngine.Events._UnityAction.CreateUnityAction(clickCallback);
            methodAddMenuItem.Invoke(thisPtr, new IntPtr[] { new IL2String(text).ptr, callback, new IL2String(onClickSL).ptr });
        }
    }
}