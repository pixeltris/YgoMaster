using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using YgoMaster;
using YgoMaster.Net;
using YgoMaster.Net.Message;
using YgoMasterClient;

namespace YgoMasterClient
{
    static class TradeUtils
    {
        public static bool IsTrading;
        public static bool IsLoading;
        public static bool IsTradeComplete;
        public static bool OwnsMainDeck;
        public static uint TradingPlayerCode;
        public static bool IsTradingPartnerHere;
        public static bool HasTradingPartnerPressedTrade;
        public static bool HasPressedTrade;
        public static DateTime LastTradeAction;
        public static bool HasRecentTradeAction;
        public static CollectionType ActiveCollectionType;
        public static bool IsActiveCollectionTheirs
        {
            get { return ActiveCollectionType == CollectionType.TheirEntire || ActiveCollectionType == CollectionType.TheirTradable; }
        }
        public static DeckInfo InitialDeckState;
        public static Queue<TradeMoveCardMessage> MoveCardQueue = new Queue<TradeMoveCardMessage>();
        public static string MyEntireCollectionJson;
        public static string MyTradableCollectionJson;
        public static string TheirEntireCollectionJson;
        public static string TheirTradableCollectionJson;

        /// <summary>
        /// Sorting gives the same result as adding cards normally but with trading it's harder to see which card was just added
        /// </summary>
        public static bool SortCards = false;

        delegate void Del_Update(IntPtr thisPtr);
        static Hook<Del_Update> hookUpdate;

        static DateTime lastActionCheck;
        static bool hasActionToRun;
        static List<Action> actionsToRunNextUpdate = new List<Action>();

        static TradeUtils()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("NetworkMain", "YgomSystem.Network");
            hookUpdate = new Hook<Del_Update>(Update, classInfo.GetMethod("Update"));
        }
        
        public static void AddAction(Action action)
        {
            lock (actionsToRunNextUpdate)
            {
                actionsToRunNextUpdate.Add(action);
                hasActionToRun = true;
            }
        }

        static void Update(IntPtr thisPtr)
        {
            hookUpdate.Original(thisPtr);
            if (hasActionToRun || lastActionCheck < DateTime.UtcNow - TimeSpan.FromSeconds(2))
            {
                lastActionCheck = DateTime.UtcNow;
                hasActionToRun = false;
                List<Action> actionsToRun = null;
                lock (actionsToRunNextUpdate)
                {
                    if (actionsToRunNextUpdate.Count > 0)
                    {
                        actionsToRun = new List<Action>(actionsToRunNextUpdate);
                        actionsToRunNextUpdate.Clear();
                    }
                }
                if (actionsToRun != null)
                {
                    foreach (Action action in actionsToRun)
                    {
                        action();
                    }
                }
            }
            if (HasRecentTradeAction && LastTradeAction < DateTime.UtcNow - TimeSpan.FromSeconds(ClientSettings.TradeActionDelayInSeconds))
            {
                HasRecentTradeAction = false;
                UpdateTradeButtonText();
            }
        }

        public static void HandleNetMessage(NetClient client, NetMessage message)
        {
            lock (actionsToRunNextUpdate)
            {
                actionsToRunNextUpdate.Add(() =>
                {
                    HandleNetMessageOnMainThread(client, message);
                });
                hasActionToRun = true;
            }
        }

        public static void Disconnected(NetClient client)
        {
            lock (actionsToRunNextUpdate)
            {
                actionsToRunNextUpdate.Add(() =>
                {
                    if (IsTrading)
                    {
                        CloseTrade();
                    }
                });
                hasActionToRun = true;
            }
        }

        static void HandleNetMessageOnMainThread(NetClient client, NetMessage message)
        {
            switch (message.Type)
            {
                case NetMessageType.FriendDuelInvite: OnFriendDuelInvite((FriendDuelInviteMessage)message); break;
                case NetMessageType.TradeEnterRoom: OnTradeEnterRoom((TradeEnterRoomMessage)message); break;
                case NetMessageType.TradeLeaveRoom: OnTradeLeaveRoom((TradeLeaveRoomMessage)message); break;
                case NetMessageType.TradeMoveCard: OnTradeMoveCard((TradeMoveCardMessage)message); break;
                case NetMessageType.TradeStateChange: OnTradeStateChange((TradeStateChangeMessage)message); break;
            }
        }

        static void OnFriendDuelInvite(FriendDuelInviteMessage message)
        {
            // TODO: Move this handler somewhere else not related to trading
            string msg = FixupBannerMsg(ClientSettings.CustomTextDuelInvite, message.PlayerCode, message.Name);
            YgomGame.Menu.ToastMessageInform.Open(msg, ClientSettings.TradeBannerOffsetY, YgomGame.Menu.ToastMessageInform.IconType.None);
        }

        static void OnTradeEnterRoom(TradeEnterRoomMessage message)
        {
            if (!string.IsNullOrEmpty(message.MyEntireCollectionJson)) MyEntireCollectionJson = message.MyEntireCollectionJson;
            if (!string.IsNullOrEmpty(message.MyTradableCollectionJson)) MyTradableCollectionJson = message.MyTradableCollectionJson;
            if (!string.IsNullOrEmpty(message.TheirEntireCollectionJson)) TheirEntireCollectionJson = message.TheirEntireCollectionJson;
            if (!string.IsNullOrEmpty(message.TheirTradableCollectionJson)) TheirTradableCollectionJson = message.TheirTradableCollectionJson;

            if (!string.IsNullOrEmpty(message.DeckJson))
            {
                OnTradeAction();
                IsTrading = true;
                IsLoading = true;
                IsTradeComplete = false;
                MoveCardQueue.Clear();
                TradingPlayerCode = message.PlayerCode;
                IsTradingPartnerHere = message.IsRemotePlayerAlreadyHere;
                HasPressedTrade = false;
                HasTradingPartnerPressedTrade = false;
                ActiveCollectionType = CollectionType.MyEntire;
                OwnsMainDeck = message.OwnsMainDeck;

                DeckInfo deck = new DeckInfo();
                deck.FromDictionaryEx(MiniJSON.Json.Deserialize(message.DeckJson) as Dictionary<string, object>);
                InitialDeckState = deck;

                DeckInfo fakeDeck = new DeckInfo();
                fakeDeck.Id = deck.Id;
                fakeDeck.Name = deck.Name;
                fakeDeck.RegulationId = deck.RegulationId;

                Dictionary<string, object> fakeDeckData = new Dictionary<string, object>()
                {
                    { "DeckList", new Dictionary<string, object>() {
                        { fakeDeck.ToString(), fakeDeck.ToDictionary() }
                    }},
                    { "Deck", new Dictionary<string, object>() {
                        { "list", new Dictionary<string, object>() {
                            { fakeDeck.ToString(), fakeDeck.ToDictionaryEx() }
                        }}
                    }},
                };

                YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(fakeDeckData));
                IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "DeckEdit/DeckEdit",
                    new Dictionary<string, object>()
                    {
                        { "DeckName", deck.Id },
                        { "RegulationID", deck.RegulationId }
                    });
            }
            else if (!string.IsNullOrEmpty(message.Name) && message.PlayerCode != 0 && ClientSettings.TradeBannerVisibleTimeInSeconds > 0)
            {
                if (IsTrading && message.PlayerCode != TradingPlayerCode)
                {
                    // Only show relevant banners when in a trade
                    return;
                }
                OnTradeAction();
                IsTradingPartnerHere = true;
                string msg = message.IsRemotePlayerAlreadyTrading
                    ? ClientSettings.CustomTextTradeBannerAlreadyTrading
                    : ClientSettings.CustomTextTradeBannerEnterTrade;
                msg = FixupBannerMsg(msg, message.PlayerCode, message.Name);
                YgomGame.Menu.ToastMessageInform.Open(msg, ClientSettings.TradeBannerOffsetY, YgomGame.Menu.ToastMessageInform.IconType.None);
            }
            UpdateTradeButtonText();
        }

        static void OnTradeLeaveRoom(TradeLeaveRoomMessage message)
        {
            if (!string.IsNullOrEmpty(message.Name) && message.PlayerCode != 0 && ClientSettings.TradeBannerVisibleTimeInSeconds > 0)
            {
                if (IsTrading && message.PlayerCode != TradingPlayerCode)
                {
                    // Only show relevant banners when in a trade
                    return;
                }
                OnTradeAction();
                IsTradingPartnerHere = false;
                HasTradingPartnerPressedTrade = false;
                string msg = FixupBannerMsg(ClientSettings.CustomTextTradeBannerLeaveTrade, message.PlayerCode, message.Name);
                YgomGame.Menu.ToastMessageInform.Open(msg, ClientSettings.TradeBannerOffsetY, YgomGame.Menu.ToastMessageInform.IconType.None);
            }
            UpdateTradeButtonText();
        }

        public static void OnTradeMoveCard(TradeMoveCardMessage message, bool ignoreLoadingState = false)
        {
            if (!IsTrading)
            {
                return;
            }

            OnTradeAction();

            if (IsLoading && !ignoreLoadingState)
            {
                MoveCardQueue.Enqueue(message);
                return;
            }

            if (message.RemoveCard)
            {
                YgomGame.DeckEditViewController2.RemoveCardWithSound(message.CardId, message.StyleRarity, !message.OtherPlayer, !message.OtherPlayer);
            }
            else
            {
                YgomGame.DeckEditViewController2.AddCardWithSound(message.CardId, message.StyleRarity, !message.OtherPlayer, !message.OtherPlayer);
            }
            UpdateTradeButtonText();
        }

        static void OnTradeStateChange(TradeStateChangeMessage message)
        {
            if (!IsTrading)
            {
                return;
            }

            switch (message.State)
            {
                case TradeStateChange.Wait:
                    HasPressedTrade = false;
                    UpdateTradeButtonText();
                    break;
                case TradeStateChange.PressedTrade:
                    HasTradingPartnerPressedTrade = true;
                    UpdateTradeButtonText();
                    break;
                case TradeStateChange.PressedCancel:
                    HasTradingPartnerPressedTrade = false;
                    UpdateTradeButtonText();
                    break;
                case TradeStateChange.Error:
                    if (!IsTradeComplete)
                    {
                        YgomGame.Menu.CommonDialogViewController.OpenErrorDialog(ClientSettings.CustomTextTradeDialogTitle,
                            ClientSettings.CustomTextTradeFailed, ClientSettings.CustomTextOK, null);
                        YgomGame.DeckEditViewController2.PopViewController();
                    }
                    break;
                case TradeStateChange.Complete:
                    if (!string.IsNullOrEmpty(message.CardListJson))
                    {
                        IsTradeComplete = true;
                        MyEntireCollectionJson = null;
                        YgomGame.Menu.CommonDialogViewController.OpenConfirmationDialog(ClientSettings.CustomTextTradeDialogTitle,
                            ClientSettings.CustomTextTradeComplete, ClientSettings.CustomTextOK, null);
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Cards.have");
                        YgomSystem.Utility.ClientWork.UpdateJson(message.CardListJson);
                    }
                    else
                    {
                        YgomGame.Menu.CommonDialogViewController.OpenErrorDialog(ClientSettings.CustomTextTradeDialogTitle,
                            ClientSettings.CustomTextTradeFailed, ClientSettings.CustomTextOK, null);
                    }
                    YgomGame.DeckEditViewController2.PopViewController();
                    break;
            }
        }

        static string FixupBannerMsg(string msg, uint playerCode, string name)
        {
            msg = msg.Replace("{code}", Utils.FormatPlayerCode(playerCode));
            msg = msg.Replace("{name}", name);
            return msg;
        }

        public static void CloseTrade()
        {
            if (!IsTrading)
            {
                return;
            }
            YgomGame.DeckEditViewController2.PopViewController();
        }

        public static void OnTradeClosed()
        {
            if (!IsTrading)
            {
                return;
            }
            if (!IsTradeComplete)
            {
                Program.NetClient.Send(new TradeLeaveRoomMessage()
                {
                    PlayerCode = TradingPlayerCode
                });
            }
            IsTrading = false;
            OwnsMainDeck = false;
            TradingPlayerCode = 0;
            IsTradingPartnerHere = false;
            HasPressedTrade = false;
            HasTradingPartnerPressedTrade = false;
            if (!string.IsNullOrEmpty(MyEntireCollectionJson))
            {
                YgomSystem.Utility.ClientWork.DeleteByJsonPath("Cards.have");
                YgomSystem.Utility.ClientWork.UpdateJson(MyEntireCollectionJson);
            }
        }

        public static string GetActiveCollectionString()
        {
            switch (ActiveCollectionType)
            {
                default:
                case CollectionType.MyEntire: return ClientSettings.CustomtextTradeMyEntireCollection;
                case CollectionType.MyTradable: return ClientSettings.CustomtextTradeMyTradableCollection;
                case CollectionType.TheirEntire: return ClientSettings.CustomtextTradeTheirEntireCollection;
                case CollectionType.TheirTradable: return ClientSettings.CustomtextTradeTheirTradableCollection;
            }
        }

        static void OnTradeAction()
        {
            LastTradeAction = DateTime.UtcNow;
            HasRecentTradeAction = true;
            HasPressedTrade = false;
            UpdateTradeButtonText();
        }

        public static void UpdateTradeButtonText()
        {
            if (!IsTrading)
            {
                return;
            }
            if (!IsTradingPartnerHere)
            {
                YgomGame.DeckEditViewController2.SetSaveButtonText("Waiting...");
                return;
            }
            if (HasRecentTradeAction)
            {
                YgomGame.DeckEditViewController2.SetSaveButtonText("Wait");
                return;
            }
            if (HasPressedTrade)
            {
                YgomGame.DeckEditViewController2.SetSaveButtonText("Cancel");
                return;
            }
            if (HasTradingPartnerPressedTrade)
            {
                YgomGame.DeckEditViewController2.SetSaveButtonText(ClientSettings.CustomTextTradeCanTradePartnerReady);
                return;
            }
            YgomGame.DeckEditViewController2.SetSaveButtonText(ClientSettings.CustomTextTradeCanTrade);
        }

        public static void OnClickTrade()
        {
            if (!IsTrading)
            {
                return;
            }
            if (!IsTradingPartnerHere || HasRecentTradeAction)
            {
                // Waiting...
            }
            else if (!HasPressedTrade)
            {
                HasPressedTrade = true;
                Program.NetClient.Send(new TradeStateChangeMessage()
                {
                    State = TradeStateChange.PressedTrade
                });
            }
            else
            {
                HasPressedTrade = false;
                Program.NetClient.Send(new TradeStateChangeMessage()
                {
                    State = TradeStateChange.PressedCancel
                });
            }
            UpdateTradeButtonText();
        }

        public enum CollectionType
        {
            MyEntire,
            MyTradable,
            TheirEntire,
            TheirTradable
        }
    }
}

namespace YgomGame.Menu
{
    unsafe static class ProfileViewController
    {
        static IL2Field fieldRootMenu;
        static IL2Field fieldIsMine;
        static IL2Field fieldPcode;

        static IntPtr extendedTextMeshComponentType;
        static IL2Method methodSetText;

        delegate void Del_InitMenuButtons(IntPtr thisPtr);
        static Hook<Del_InitMenuButtons> hookInitMenuButtons;

        delegate void Del_OnClickBlock(IntPtr thisPtr);
        static Hook<Del_OnClickBlock> hookOnClickBlock;

        static ProfileViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ProfileViewController", "YgomGame.Menu");
            fieldRootMenu = classInfo.GetField("rootMenu");
            fieldIsMine = classInfo.GetField("isMine");
            fieldPcode = classInfo.GetField("pcode");
            if (!string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
            {
                hookInitMenuButtons = new Hook<Del_InitMenuButtons>(InitMenuButtons, classInfo.GetMethod("InitMenuButtons"));
                hookOnClickBlock = new Hook<Del_OnClickBlock>(OnClickBlock, classInfo.GetMethod("OnClickBlock"));
            }

            extendedTextMeshComponentType = CastUtils.IL2Typeof("ExtendedTextMeshProUGUI", "YgomSystem.YGomTMPro", "Assembly-CSharp");

            IL2Assembly textMeshAssembly = Assembler.GetAssembly("Unity.TextMeshPro");
            IL2Class tmTextClassInfo = textMeshAssembly.GetClass("TMP_Text", "TMPro");
            IL2Property textProperty = tmTextClassInfo.GetProperty("text");
            methodSetText = textProperty.GetSetMethod();
        }

        static void InitMenuButtons(IntPtr thisPtr)
        {
            hookInitMenuButtons.Original(thisPtr);
            if (fieldIsMine.GetValue(thisPtr).GetValueRef<csbool>() == true)
            {
                return;
            }

            IntPtr rootMenuGameObject = fieldRootMenu.GetValue(thisPtr).ptr;
            IntPtr lastButtonObj = UnityEngine.GameObject.FindGameObjectByName(rootMenuGameObject, "ButtonTemplate(Clone)", true);

            IntPtr lastButtonTextObj = UnityEngine.GameObject.FindGameObjectByName(lastButtonObj, "TextTMP");
            IntPtr lastButtonTextComponent = UnityEngine.GameObject.GetComponent(lastButtonTextObj, extendedTextMeshComponentType);
            methodSetText.Invoke(lastButtonTextComponent, new IntPtr[] { new IL2String(ClientSettings.CustomTextProfileTradeButton).ptr });
        }

        static void OnClickBlock(IntPtr thisPtr)
        {
            long pcode = fieldPcode.GetValue(thisPtr).GetValueRef<long>();
            Program.NetClient.Send(new TradeEnterRoomMessage()
            {
                PlayerCode = (uint)pcode
            });
        }
    }
}

//YgomGame.Menu.ToastMessageInform = banner in middle of screen (entire width of screen)
//YgomGame.Menu.EventNotifyViewController = popup (blocks input)
//YgomGame.Duel.CardReportTelopManager/CardReportTelopController/YgomGame.Stats.CardStats = card stats popup that shows in game (win rate / usage rate)
//YgomGame.PopUpText/YgomGame.PopUpTextManager = the popup text that appears when you hover your mouse over your deck/graveyard (not the one where you hold down your middle mouse)
namespace YgomGame.Menu
{
    unsafe static class ToastMessageInform
    {
        static IL2Field fieldLifeSecond;
        static IL2Method methodOpen;
        static IL2Method methodOpenEx;
        static IL2Method methodOpenWithBlock;

        delegate IntPtr Del_Start(IntPtr thisPtr);
        static Hook<Del_Start> hookStart;

        /*delegate void Del_ShowText(IntPtr thisPtr, IntPtr text, ref worldpos worldPos, bool isforui);
        static Hook<Del_ShowText> hookShowText;

        delegate void Del_UpdateText(IntPtr thisPtr, IntPtr text);
        static Hook<Del_UpdateText> hookUpdateText;*/

        /*delegate void Del_OnClick(IntPtr thisPtr);
        static Hook<Del_OnClick> hookOnClick;*/

        static bool isNextMessageCustom;

        static ToastMessageInform()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ToastMessageInform", "YgomGame.Menu");
            fieldLifeSecond = classInfo.GetField("m_LifeSecond");
            methodOpen = classInfo.GetMethod("Open", x => x.GetParameters()[1].Name == "callback");
            methodOpenEx = classInfo.GetMethod("Open", x => x.GetParameters().Length == 3);
            methodOpenWithBlock = classInfo.GetMethod("OpenWithBlock");
            hookStart = new Hook<Del_Start>(Start, classInfo.GetMethod("Start"));

            /*IL2Class classInfo2 = assembly.GetClass("PopUpText", "YgomGame");
            hookShowText = new Hook<Del_ShowText>(ShowText, classInfo2.GetMethod("ShowText"));
            hookUpdateText = new Hook<Del_UpdateText>(UpdateText, classInfo2.GetMethod("UpdateText"));*/

            /*IL2Class selectionButtonClassInfo = assembly.GetClass("SelectionButton", "YgomSystem.UI");
            hookOnClick = new Hook<Del_OnClick>(OnClick, selectionButtonClassInfo.GetMethod("OnClick"));*/
        }

        /*static void OnClick(IntPtr thisPtr)
        {
            Console.WriteLine("OnClick: " + DateTime.Now.TimeOfDay);
            hookOnClick.Original(thisPtr);

            IntPtr obj = UnityEngine.Component.GetGameObject(thisPtr);
            List<IntPtr> objs = new List<IntPtr>();
            while (obj != IntPtr.Zero)
            {
                objs.Add(obj);
                obj = UnityEngine.GameObject.GetParentObject(obj);
            }
            objs.Reverse();
            Console.WriteLine("Name: " + string.Join(".", objs.Select(x => UnityEngine.UnityObject.GetName(x))));
        }*/

        /*struct worldpos
        {
            public float x;
            public float y;
            public float z;
        }

        static void ShowText(IntPtr thisPtr, IntPtr text, ref worldpos worldPos, bool isforui)
        {
            Console.WriteLine("ShowText " + new IL2String(text).ToString() + " x:" + worldPos.x + " y:" + worldPos.y + " z:" + worldPos.z);

            // TODO: Hook YgomGame.PopUpTextManager.OnEnterPopUpArea / OnExitPopUpArea / RegistPopUpCallback / RemovePopUpCallback

            string str = "";
            try
            {
                str = System.IO.File.ReadAllText("C:/showtext.txt");
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(str))
            {
                text = new IL2String(str).ptr;
            }
            hookShowText.Original(thisPtr, text, ref worldPos, isforui);
        }

        static void UpdateText(IntPtr thisPtr, IntPtr text)
        {
            Console.WriteLine("UpdateText " + new IL2String(text).ToString());
            hookUpdateText.Original(thisPtr, text);
        }*/

        static IntPtr Start(IntPtr thisPtr)
        {
            if (isNextMessageCustom)
            {
                isNextMessageCustom = false;
                float lifeSeconds = ClientSettings.TradeBannerVisibleTimeInSeconds;
                fieldLifeSecond.SetValue(thisPtr, new IntPtr(&lifeSeconds));
            }
            return hookStart.Original(thisPtr);
        }

        public static void Open(string message)
        {
            isNextMessageCustom = true;
            methodOpen.Invoke(new IntPtr[] { new IL2String(message).ptr, IntPtr.Zero });
        }

        public static void Open(string message, float yPos, IconType iconType)
        {
            isNextMessageCustom = true;
            methodOpenEx.Invoke(new IntPtr[] { new IL2String(message).ptr, new IntPtr(&yPos), new IntPtr(&iconType) });
        }

        public static void OpenWithBlock(string message)
        {
            isNextMessageCustom = true;
            methodOpenWithBlock.Invoke(new IntPtr[] { new IL2String(message).ptr, IntPtr.Zero });
        }

        public enum IconType
        {
            None,
            Inform
        }
    }
}