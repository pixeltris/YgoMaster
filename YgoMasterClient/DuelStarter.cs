using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using IL2CPP;
using YgoMasterClient;
using YgoMaster;

// TODO:
// Merge the live / non-live duel starters (they currently take a different code path). It would make sense to keep live and remove non-live.
//  - live sends a regular "Duel.begin" with "Solo/SoloStartProduction" including the chapter id and then in the
//    hook of "RequestStructure.Complete" it checks for the "Duel.begin" response and injects the custom duel data
//  - non-live sends a regular "Duel.begin" with "Solo/SoloStartProduction" but modifies the request inside the
//    hook of "YgomSystem.Network.API.Duel_begin" and inserts "duelStarterData" which contains the custom duel data which is
//    then detected by the server and sent back to the client (with inserted random seed)

namespace YgomGame.Solo
{
    static unsafe class SoloSelectChapterViewController
    {
        static IL2Class classDuelResultViewController;
        static IL2Class classDuelClient;

        static IL2Field fieldChapterId;

        delegate void Del_Play(IntPtr thisPtr, IntPtr chapterData);
        static Hook<Del_Play> hookDuelDialogPlay;
        static Hook<Del_Play> hookTutorialDialogPlay;

        delegate void Del_NotificationStack(IntPtr thisPtr, IntPtr vcm, IntPtr vc, bool isEntry);
        static Hook<Del_NotificationStack> hookNotificationStack;

        static SoloSelectChapterViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("SoloSelectChapterViewController", "YgomGame.Solo");
            IL2Class chapterClassInfo = classInfo.GetNestedType("Chapter");
            fieldChapterId = chapterClassInfo.GetField("id");
            IL2Class duelDialogClassInfo = classInfo.GetNestedType("DuelDialog");
            IL2Class tutorialDialogClassInfo = classInfo.GetNestedType("TutorialDialog");
            hookDuelDialogPlay = new Hook<Del_Play>(DuelDialogPlay, duelDialogClassInfo.GetMethod("Play"));
            hookTutorialDialogPlay = new Hook<Del_Play>(TutorialDialogPlay, tutorialDialogClassInfo.GetMethod("Play"));
            hookNotificationStack = new Hook<Del_NotificationStack>(NotificationStack, classInfo.GetMethod("NotificationStack"));

            classDuelResultViewController = assembly.GetClass("DuelResultViewController", "YgomGame.Menu");
            classDuelClient = assembly.GetClass("DuelClient", "YgomGame.Duel");
        }

        static void NotificationStack(IntPtr thisPtr, IntPtr vcm, IntPtr vc, bool isEntry)
        {
            if (!Program.IsLive && vcm != IntPtr.Zero && YgomSystem.UI.ViewControllerManager.GetStackTopViewController(vcm) == thisPtr &&
                vc != IntPtr.Zero && Import.Object.il2cpp_object_get_class(vc) == classDuelResultViewController.ptr)
            {
                // Fix for duel result screen -> solo chapter complete animation
                YgomSystem.Utility.ClientWork.DeleteByJsonPath("Duel.result");
                vc = Import.Object.il2cpp_object_new(classDuelClient.ptr);
            }
            hookNotificationStack.Original(thisPtr, vcm, vc, isEntry);
        }

        static void DuelDialogPlay(IntPtr thisPtr, IntPtr chapterData)
        {
            if (HandlePlay(thisPtr, chapterData))
            {
                return;
            }
            hookDuelDialogPlay.Original(thisPtr, chapterData);
        }

        static void TutorialDialogPlay(IntPtr thisPtr, IntPtr chapterData)
        {
            if (HandlePlay(thisPtr, chapterData))
            {
                return;
            }
            hookTutorialDialogPlay.Original(thisPtr, chapterData);
        }

        static bool HandlePlay(IntPtr thisPtr, IntPtr chapterData)
        {
            if (chapterData != IntPtr.Zero)
            {
                if (fieldChapterId.GetValue(chapterData).GetValueRef<int>() == ClientSettings.DuelStarterLiveChapterId &&
                    (Program.IsLive || ClientSettings.DuelStarterLiveNotLiveTest))
                {
                    int clearStatus;
                    string clearStatusStr = YgomSystem.Utility.ClientWork.SerializePath("$.Solo.cleared." +
                        (ClientSettings.DuelStarterLiveChapterId / 10000) + "." + ClientSettings.DuelStarterLiveChapterId);
                    if (!string.IsNullOrEmpty(clearStatusStr) && int.TryParse(clearStatusStr, out clearStatus) && clearStatus == 3)
                    {
                        IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                        if (manager != IntPtr.Zero)
                        {
                            YgomGame.Room.RoomCreateViewController.IsNextInstanceHacked = true;
                            YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "Room/RoomCreate");
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    static unsafe class SoloStartProductionViewController
    {
        public static int DuelStarterFirstPlayer;
        static readonly int finalStepValue;// NOTE: This value changed in v1.2.0 so we now fetch it at runtime

        static IL2Field fieldStep;
        static IL2Method methodDispFirstorSecond;

        delegate void Del_SelectTurn(IntPtr thisPtr);
        static Hook<Del_SelectTurn> hookSelectTurn;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;

        static SoloStartProductionViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("SoloStartProductionViewController", "YgomGame.Solo");
            fieldStep = classInfo.GetField("step");
            finalStepValue = classInfo.GetNestedType("Step").GetField("Final").GetValue().GetValueRef<int>();
            methodDispFirstorSecond = classInfo.GetMethod("DispFirstorSecond");
            hookSelectTurn = new Hook<Del_SelectTurn>(SelectTurn, classInfo.GetMethod("SelectTurn"));
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, classInfo.GetMethod("OnCreatedView"));
        }

        static void SelectTurn(IntPtr thisPtr)
        {
            if (YgomGame.Room.RoomCreateViewController.IsHacked)
            {
                DuelSettings settings = YgomGame.Room.RoomCreateViewController.Settings;
                if (settings != null)
                {
                    DuelStarterFirstPlayer = settings.FirstPlayer;
                    if (DuelStarterFirstPlayer < 0)
                    {
                        DuelStarterFirstPlayer = Utils.Rand.Next(2);
                    }
                    if (ClientSettings.DuelStarterShowFirstPlayer)
                    {
                        YgomGame.Colosseum.ColosseumUtil.Turn turn = DuelStarterFirstPlayer == 0 ?
                            Colosseum.ColosseumUtil.Turn.FIRST : Colosseum.ColosseumUtil.Turn.SECOND;
                        methodDispFirstorSecond.Invoke(thisPtr, new IntPtr[] { new IntPtr(&turn) });
                    }
                    int nextStep = finalStepValue;// YgomGame.Solo.SoloStartProductionViewController.Step.Final
                    fieldStep.SetValue(thisPtr, new IntPtr(&nextStep));
                    return;
                }
            }

            if (SoloVisualNovel.Run(thisPtr))
            {
                return;
            }
            hookSelectTurn.Original(thisPtr);
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            SoloVisualNovel.SetJsonFileNames();
            SoloVisualNovel.Init();
            hookOnCreatedView.Original(thisPtr);
        }
    }
}

namespace YgomGame.Colosseum
{
    static unsafe class ColosseumUtil
    {
        public enum Turn
        {
            FIRST,
            SECOND,
            RANDOM,
            NONE = 10
        }
    }
}

namespace YgomSystem.Network
{
    static unsafe class API
    {
        static IL2Method methodCompress;

        delegate IntPtr Del_Duel_begin(IntPtr rulePtr);
        static Hook<Del_Duel_begin> hookDuel_begin;
        delegate IntPtr Del_Duel_end(IntPtr paramPtr);
        static Hook<Del_Duel_end> hookDuel_end;

        static API()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("API", "YgomSystem.Network");
            hookDuel_begin = new Hook<Del_Duel_begin>(Duel_begin, classInfo.GetMethod("Duel_begin"));
            hookDuel_end = new Hook<Del_Duel_end>(Duel_end, classInfo.GetMethod("Duel_end"));

            IL2Class zlibClassInfo = assembly.GetClass("Zlib");
            methodCompress = zlibClassInfo.GetMethod("Compress");
        }

        static IntPtr Duel_begin(IntPtr rulePtr)
        {
            if (YgomGame.Room.RoomCreateViewController.IsHacked)
            {
                Dictionary<string, object> rule = MiniJSON.Json.Deserialize(YgomMiniJSON.Json.Serialize(rulePtr)) as Dictionary<string, object>;
                if (rule == null)
                {
                    rule = new Dictionary<string, object>();
                }
                if (Program.IsLive || ClientSettings.DuelStarterLiveNotLiveTest)
                {
                    // Remove entries not valid for practice duels
                    rule.Remove("FirstPlayer");
                }
                else
                {
                    DuelSettings settings = new DuelSettings();
                    settings.CopyFrom(YgomGame.Room.RoomCreateViewController.Settings);
                    settings.ClearRandomDeckPaths(!ClientSettings.RandomDecksDontSetCpuName);
                    settings.FirstPlayer = YgomGame.Solo.SoloStartProductionViewController.DuelStarterFirstPlayer;
                    rule["duelStarterData"] = settings.ToDictionary();
                }
                rulePtr = YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(rule));
            }
            return hookDuel_begin.Original(rulePtr);
        }

        static IntPtr Duel_end(IntPtr paramPtr)
        {
            Dictionary<string, object> param = MiniJSON.Json.Deserialize(YgomMiniJSON.Json.Serialize(paramPtr)) as Dictionary<string, object>;
            if (param == null)
            {
                param = new Dictionary<string, object>();
            }

            if (DuelDll.IsPvpDuel)
            {
                if (DuelDll.SpecialResultType != DuelResultType.None)
                {
                    param["res"] = (int)DuelDll.SpecialResultType;
                    param["finish"] = (int)DuelDll.SpecialFinishType;
                }
                else if (DuelDll.HasNetworkError)
                {
                    param["res"] = (int)DuelResultType.Draw;
                    param["finish"] = (int)DuelFinishType.FinishError;
                }
            }
            else if (ClientSettings.AlwaysWin)
            {
                param["res"] = (int)DuelResultType.Win;
                param["finish"] = (int)DuelFinishType.Normal;
            }

            if (!Program.IsLive)
            {
                param["turn"] = DuelDll.DLL_DuelGetTurnNum() + 1;
                param["replayData"] = GetReplayDataString(param);
            }
            paramPtr = YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(param));
            return hookDuel_end.Original(paramPtr);
        }

        public static string GetReplayDataString(Dictionary<string, object> param)
        {
            DuelFinishType finish = (DuelFinishType)Utils.GetValue<int>(param, "finish");

            List<byte> replayData = new List<byte>(DuelDll.ReplayData);

            if (finish == DuelFinishType.Surrender)
            {
                DuelResultType res = DuelResultType.Lose;
                if (param.ContainsKey("res"))
                {
                    res = Utils.GetValue<DuelResultType>(param, "res");
                }
                int myId = DuelDll.DLL_DuelMyself();
                if ((res == DuelResultType.Win && myId == 0) ||
                    (res == DuelResultType.Lose && myId == 1))
                {
                    // Surrender opponent
                    replayData.AddRange(new byte[] { 0x22, 0x80, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    replayData.AddRange(new byte[] { 0x05, 0x00, 0x01, 0x00, 0x04, 0x00, 0x00, 0x00 });
                }
                else
                {
                    // Surrender self
                    replayData.AddRange(new byte[] { 0x22, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    replayData.AddRange(new byte[] { 0x05, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00 });
                }
            }

            byte[] compressedBuffer = null;

            byte[] replayBuffer = replayData.ToArray();
            IL2Array<byte> nativeReplayBuffer = new IL2Array<byte>(replayBuffer.Length, IL2SystemClass.Byte);
            nativeReplayBuffer.CopyFrom(replayBuffer);
            IL2Object nativeCompressedBufferObj = methodCompress.Invoke(new IntPtr[] { nativeReplayBuffer.ptr });
            if (nativeCompressedBufferObj != null)
            {
                IL2Array<byte> nativeCompressedBuffer = new IL2Array<byte>(nativeCompressedBufferObj.ptr);
                compressedBuffer = nativeCompressedBuffer.ToByteArray();
            }

            int[] fin = new int[]
            {
                (int)finish,
                (int)finish
            };
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["b"] = compressedBuffer;
            dict["f"] = fin;
            return Convert.ToBase64String(MessagePack.Pack(dict));
        }
    }

    static unsafe class RequestStructure
    {
        static IL2Method methodGetCommand;
        static IL2Field fieldCode;

        static IL2Field fieldRequest;
        static IL2Method methodIsCompleted;
        delegate void Del_CommandEvent(IntPtr command, IntPtr handle);
        static Hook<Del_CommandEvent> hookCommandEvent;

        static RequestStructure()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("NetworkMain").GetNestedType("RequestStructure");
            methodGetCommand = classInfo.GetProperty("Command").GetGetMethod();
            fieldCode = classInfo.GetField("code");

            hookCommandEvent = new Hook<Del_CommandEvent>(CommandEvent, assembly.GetClass("Request", "YgomSystem.Network").GetMethod("CommandEvent"));
            IL2Class handleClassInfo = assembly.GetClass("Handle", "YgomSystem.Network");
            fieldRequest = handleClassInfo.GetField("m_Request");
            methodIsCompleted = handleClassInfo.GetMethod("IsCompleted");
        }

        public static void SetCode(IntPtr thisPtr, int code)
        {
            fieldCode.SetValue(thisPtr, new IntPtr(&code));
        }

        static void CommandEvent(IntPtr command, IntPtr handle)
        {
            if (methodIsCompleted.Invoke(handle).GetValueRef<csbool>())
            {
                Complete(fieldRequest.GetValue(handle).ptr);
            }
            hookCommandEvent.Original(command, handle);
        }

        static void Complete(IntPtr thisPtr)
        {
            IL2Object cmdObj = methodGetCommand.Invoke(thisPtr);
            string cmd = null;
            if (cmdObj != null)
            {
                cmd = cmdObj.GetValueObj<string>();
            }
            if (!string.IsNullOrEmpty(cmd))
            {
                switch (cmd)
                {
                    case "Duel.begin":
                        if (Program.IsLive)
                        {
                            YgomGame.Menu.ProfileReplayViewController.LiveDuelBeginData =
                                MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("Duel")) as Dictionary<string, object>;
                        }

                        PInvoke.SetTimeMultiplier(ClientSettings.DuelClientTimeMultiplier != 0 ?
                            ClientSettings.DuelClientTimeMultiplier : ClientSettings.TimeMultiplier);

                        DuelDll.OnDuelBegin((GameMode)YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.GameMode"));
                        break;
                    case "Duel.end":
                        DuelDll.OnDuelEnd();
                        PInvoke.SetTimeMultiplier(ClientSettings.TimeMultiplier);
                        break;
                    case "Room.is_room_battle_ready":
                        DuelDll.OnDuelRoomBattleReady();
                        break;
                }

                YgomGame.Menu.ProfileReplayViewController.OnNetworkComplete(thisPtr, cmd);

                if (YgomGame.Room.RoomCreateViewController.IsHacked)
                {
                    switch (cmd)
                    {
                        case "Duel.begin":
                            {
                                if (Program.IsLive || ClientSettings.DuelStarterLiveNotLiveTest)
                                {
                                    DuelSettings settings = new DuelSettings();
                                    settings.CopyFrom(YgomGame.Room.RoomCreateViewController.Settings);
                                    settings.ClearRandomDeckPaths(!ClientSettings.RandomDecksDontSetCpuName);
                                    settings.FirstPlayer = YgomGame.Solo.SoloStartProductionViewController.DuelStarterFirstPlayer;
                                    YgomSystem.Utility.ClientWork.DeleteByJsonPath("Duel");
                                    YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(new Dictionary<string, object>()
                                    {
                                        { "Duel", settings.ToDictionary() }
                                    }));
                                }
                            }
                            break;
                        case "Duel.end":
                            {
                                if (YgomGame.Room.RoomCreateViewController.IsHacked && YgomGame.Room.RoomCreateViewController.Settings.avatar[0] > 0)
                                {
                                    YgomSystem.Utility.ClientWork.UpdateJson("DuelResult.resultInfo.avatar", YgomGame.Room.RoomCreateViewController.Settings.avatar[0].ToString());
                                }
                            }
                            break;
                        case "Solo.start":
                            {
                                DuelSettings settings = new DuelSettings();
                                settings.CopyFrom(YgomGame.Room.RoomCreateViewController.Settings);
                                settings.ClearRandomDeckPaths(!ClientSettings.RandomDecksDontSetCpuName);
                                YgomSystem.Utility.ClientWork.DeleteByJsonPath("Duel");
                                YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(new Dictionary<string, object>()
                                {
                                    { "Duel", settings.ToDictionaryForSoloStart() }
                                }));

                                // Open the duel loading screen (the client will automatically send "Duel.begin")
                                IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                                if (Program.IsLive || ClientSettings.DuelStarterLiveNotLiveTest)
                                {
                                    Dictionary<string, object> args = new Dictionary<string, object>()
                                    {
                                        { "chapter", ClientSettings.DuelStarterLiveChapterId }
                                    };
                                    YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "Solo/SoloStartProduction", args);
                                }
                                else
                                {
                                    YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "Solo/SoloStartProduction");
                                }
                            }
                            break;
                    }
                }
            }
        }
    }

    static unsafe class Request
    {
        delegate IntPtr Del_Entry(IntPtr commandPtr, IntPtr paramPtr, float timeOut);
        static Hook<Del_Entry> hookEntry;

        static Request()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class requestClassInfo = assembly.GetClass("Request", "YgomSystem.Network");
            hookEntry = new Hook<Del_Entry>(Entry, requestClassInfo.GetMethod("Entry"));
        }

        static IntPtr Entry(IntPtr commandPtr, IntPtr paramPtr, float timeOut)
        {
            YgomGame.Menu.ProfileReplayViewController.OnNetworkEntry(ref commandPtr, ref paramPtr, ref timeOut);

            return hookEntry.Original(commandPtr, paramPtr, timeOut);
        }

        public static IntPtr Entry(string command, string param, float timeout = 30)
        {
            return hookEntry.Original(new IL2String(command).ptr, YgomMiniJSON.Json.Deserialize(param), timeout);
        }
    }
}

// This is bugged unfortunately. While you can force control of the opponent, the client doesn't handle it properly
// - Cannot see opponents cards
// - Cannot change phases
// - Deck does some weird card draw thing
// - Drawn card is positioned incorrectly (empty space in hand)
namespace YgomGame.Duel
{
    static unsafe class EngineInitializerByServer
    {
        delegate int Del_get_rivalType(IntPtr thisPtr);
        static Hook<Del_get_rivalType> hookget_rivalType;

        static EngineInitializerByServer()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("EngineInitializerByServer", "YgomGame.Duel");
            hookget_rivalType = new Hook<Del_get_rivalType>(get_rivalType, classInfo.GetProperty("rivalType").GetGetMethod());
        }

        static int get_rivalType(IntPtr thisPtr)
        {
            if (YgomGame.Room.RoomCreateViewController.IsHacked)
            {
                if (YgomGame.Room.RoomCreateViewController.Settings.OpponentType == 1)
                {
                    Console.WriteLine("NOTE: player VS self is broken");
                }
                // OpponentType is 1 for player, 2 (or any other value) for CPU
                return YgomGame.Room.RoomCreateViewController.Settings.OpponentType == 1 ? 0 : 1;
            }
            return hookget_rivalType.Original(thisPtr);
        }
    }

    static unsafe class EngineInitializer
    {
        delegate void Del_InitEngine(IntPtr thisPtr, IntPtr runEffect, IntPtr isBusyEffect, IntPtr recmanref);
        static Hook<Del_InitEngine> hookInitEngine;

        static EngineInitializer()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("EngineInitializer", "YgomGame.Duel");
            hookInitEngine = new Hook<Del_InitEngine>(InitEngine, classInfo.GetMethod("InitEngine"));
        }

        static void InitEngine(IntPtr thisPtr, IntPtr runEffect, IntPtr isBusyEffect, IntPtr recmanref)
        {
            hookInitEngine.Original(thisPtr, runEffect, isBusyEffect, recmanref);
            Dictionary<string, object> duelData = YgomSystem.Utility.ClientWork.GetDict("Duel");
            List<object> cmds;
            if (duelData != null && Utils.TryGetValue(duelData, "cmds", out cmds))
            {
                if (ClientSettings.CustomDuelCmdLog)
                {
                    Console.WriteLine(MiniJSON.Json.Serialize(cmds));
                }
                foreach (object cmdObj in cmds)
                {
                    List<object> cmdItemsObj = cmdObj as List<object>;
                    if (cmdItemsObj == null)
                    {
                        if (ClientSettings.CustomDuelCmdLog)
                        {
                            Console.WriteLine("cmdItemsObj null");
                        }
                        continue;
                    }
                    int[] cmdItems = cmdItemsObj.Select(x => (int)Convert.ChangeType(x, typeof(int))).ToArray();
                    if (cmdItems.Length < 1)
                    {
                        if (ClientSettings.CustomDuelCmdLog)
                        {
                            Console.WriteLine("cmdItems empty");
                        }
                        continue;
                    }
                    switch (cmdItems[0])
                    {
                        case 0:
                            if (cmdItems.Length >= 7)
                            {
                                DuelDll.DLL_DuelComCheatCard(cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4], cmdItems[5], cmdItems[6]);
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComCheatCard {0} {1} {2} {3} {4} {5}", cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4], cmdItems[5], cmdItems[6]);
                                }
                            }
                            break;
                        case 1:
                            if (cmdItems.Length >= 5)
                            {
                                DuelDll.DLL_DuelComDoDebugCommand(cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComDoDebugCommand {0} {1} {2} {3}", cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                }
                            }
                            break;
                        case 2:
                            if (cmdItems.Length >= 1)
                            {
                                DuelDll.DLL_DuelComDebugCommand();
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComDebugCommand");
                                }
                            }
                            break;
                        case 3:
                            if (cmdItems.Length >= 5)
                            {
                                DuelDll.DLL_DuelComDoCommand(cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComDoCommand {0} {1} {2} {3}", cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                }
                            }
                            break;
                        case 4:
                            if (cmdItems.Length >= 1)
                            {
                                DuelDll.DLL_DuelSysAct();
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelSysAct");
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}

namespace YgomGame.Room
{
    static unsafe class RoomCreateViewController
    {
        public static IL2Class ClassInfo;
        static IntPtr bindingTextType;
        static IL2Field fieldInfos;
        static IL2Field fieldIsv;
        static IL2Class templateInfoClass;
        static IL2Class labelInfoClass;
        static IL2Method labelInfoCtor;

        static IL2Class buttonInfoBaseClass;
        static IL2Field buttonInfoBaseTitle;
        static IL2Method buttonInfoBaseValueText;
        static IL2Class buttonStringInfoClass;
        static IL2Method buttonStringInfoCtor;
        static IL2Property buttonStringInfoCurrentValue;
        static IL2Class buttonBoolValueInfoClass;
        static IL2Method buttonBoolValueInfoCtor;
        static IL2Property buttonBoolValueInfoCurrentValue;
        static IL2Method buttonBoolValueInfoSwitchValue;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;
        delegate void Del_SetupData(IntPtr thisPtr);
        static Hook<Del_SetupData> hookSetupData;
        delegate void Del_CallAPIRoomCreate(IntPtr thisPtr);
        static Hook<Del_CallAPIRoomCreate> hookCallAPIRoomCreate;
        delegate void Del_OnClick(IntPtr thisPtr);
        static Hook<Del_OnClick> hookOnClick;

        static IntPtr activeViewController;
        static IntPtr activeButton;
        static DuelSettingsManager duelSettingsManager = new DuelSettingsManager();

        static Dictionary<IntPtr, string[]> buttonsActionSheets = new Dictionary<IntPtr, string[]>();

        public static bool IsNextInstanceHacked = false;
        public static bool IsHacked
        {
            get
            {
                if (activeViewController != IntPtr.Zero)
                {
                    IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                    IntPtr roomView = YgomSystem.UI.ViewControllerManager.GetViewController(manager, YgomGame.Room.RoomCreateViewController.ClassInfo.IL2Typeof());
                    return roomView != IntPtr.Zero && roomView == activeViewController;
                }
                return false;
            }
        }
        public static IntPtr HackedInstance
        {
            get { return activeViewController; }
        }

        public static DuelSettings Settings
        {
            get { return duelSettingsManager != null ? duelSettingsManager.SettingsClone : null; }
        }

        static RoomCreateViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            ClassInfo = assembly.GetClass("RoomCreateViewController", "YgomGame.Room");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, ClassInfo.GetMethod("OnCreatedView"));
            hookSetupData = new Hook<Del_SetupData>(SetupData, ClassInfo.GetMethod("SetupData"));
            hookCallAPIRoomCreate = new Hook<Del_CallAPIRoomCreate>(CallAPIRoomCreate, ClassInfo.GetMethod("CallAPIRoomCreate"));
            fieldInfos = ClassInfo.GetField("infos");
            fieldIsv = ClassInfo.GetField("isv");
            templateInfoClass = ClassInfo.GetNestedType("TemplateInfo");
            labelInfoClass = ClassInfo.GetNestedType("LabelInfo");
            labelInfoCtor = labelInfoClass.GetMethod(".ctor");
            buttonInfoBaseClass = ClassInfo.GetNestedType("ButtonInfoBase");
            buttonInfoBaseTitle = buttonInfoBaseClass.GetField("title");
            buttonInfoBaseValueText = buttonInfoBaseClass.GetProperty("valueText").GetGetMethod();
            hookOnClick = new Hook<Del_OnClick>(OnClick, buttonInfoBaseClass.GetMethod("OnClick"));
            buttonStringInfoClass = ClassInfo.GetNestedType("ButtonStringInfo");
            buttonStringInfoCtor = buttonStringInfoClass.GetMethod(".ctor");
            buttonStringInfoCurrentValue = buttonStringInfoClass.GetProperty("currentValue");
            buttonBoolValueInfoClass = ClassInfo.GetNestedType("ButtonBoolValueInfo");
            buttonBoolValueInfoCtor = buttonBoolValueInfoClass.GetMethod(".ctor");
            buttonBoolValueInfoCurrentValue = buttonBoolValueInfoClass.GetProperty("currentValue");
            buttonBoolValueInfoSwitchValue = buttonBoolValueInfoClass.GetMethod("SwitchValue");
            bindingTextType = CastUtils.IL2Typeof("BindingTextMeshProUGUI", "YgomSystem.UI", "Assembly-CSharp");
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            if (!IsNextInstanceHacked)
            {
                activeViewController = IntPtr.Zero;
                hookOnCreatedView.Original.Invoke(thisPtr);
                return;
            }
            IsNextInstanceHacked = false;
            activeViewController = thisPtr;

            hookOnCreatedView.Original.Invoke(thisPtr);
            
            // Modify the title of the view
            IntPtr titleObj = UnityEngine.GameObject.FindGameObjectByName(UnityEngine.Component.GetGameObject(thisPtr), "NameText");
            IntPtr titleComponent = UnityEngine.GameObject.GetComponent(titleObj, bindingTextType);
            YgomSystem.UI.BindingTextMeshProUGUI.SetTextId(titleComponent, ClientSettings.CustomTextDuelStarterTitle);

            // Modify the text of the button on the bottom right (v1.2.0 changed from "OKButton" to "ButtonOK")
            IntPtr duelStartButtonObj = UnityEngine.GameObject.FindGameObjectByName(UnityEngine.Component.GetGameObject(thisPtr), "ButtonOK");
            IntPtr duelStartButtonTextObj = UnityEngine.GameObject.FindGameObjectByName(duelStartButtonObj, "TextTMP");
            IntPtr duelStartButtonTextComponent = UnityEngine.GameObject.GetComponent(duelStartButtonTextObj, bindingTextType);
            YgomSystem.UI.BindingTextMeshProUGUI.SetTextId(duelStartButtonTextComponent, ClientSettings.CustomTextDuelStarterDuelButton);
        }

        public static void NotificationStackRemove(IntPtr thisPtr)
        {
            if (activeViewController == thisPtr)
            {
                duelSettingsManager.DuelSettingsFromUI();
                activeViewController = IntPtr.Zero;
            }
        }

        static void SetupData(IntPtr thisPtr)
        {
            if (IsHacked)
            {
                duelSettingsManager.InitButtons(thisPtr);
            }
            else
            {
                hookSetupData.Original(thisPtr);
            }
        }

        static void CallAPIRoomCreate(IntPtr thisPtr)
        {
            if (IsHacked)
            {
                // Update the duel settings
                duelSettingsManager.SettingsClone = null;
                duelSettingsManager.DuelSettingsFromUI();

                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "chapter", ClientSettings.DuelStarterLiveChapterId }
                };
                if (!Program.IsLive)
                {
                    data["customduel"] = true;
                }
                YgomSystem.Network.Request.Entry("Solo.start", MiniJSON.Json.Serialize(data));
            }
            else
            {
                hookCallAPIRoomCreate.Original(thisPtr);
            }
        }

        static IntPtr AddLabel(IL2ListExplicit infosList, string label)
        {
            IntPtr newLabel = Import.Object.il2cpp_object_new(labelInfoClass.ptr);
            labelInfoCtor.Invoke(newLabel, new IntPtr[] { new IL2String(label).ptr });
            infosList.Add(newLabel);
            return newLabel;
        }

        static IntPtr AddButtonString(IL2ListExplicit infosList, string settingLabel, string title, string[] actionSheet)
        {
            IntPtr newButton = Import.Object.il2cpp_object_new(buttonStringInfoClass.ptr);
            buttonStringInfoCtor.Invoke(newButton, new IntPtr[]
            {
                new IL2String(settingLabel).ptr,
                new IL2String(title).ptr,
                new IL2String(actionSheet == null ? "    " : actionSheet[0]).ptr
            });
            infosList.Add(newButton);
            if (actionSheet != null)
            {
                buttonsActionSheets[newButton] = actionSheet;
            }
            return newButton;
        }

        static IntPtr AddButtonBool(IL2ListExplicit infosList, string settingLabel, string title, bool defaultValue)
        {
            IntPtr newButton = Import.Object.il2cpp_object_new(buttonBoolValueInfoClass.ptr);
            buttonBoolValueInfoCtor.Invoke(newButton, new IntPtr[]
            {
                new IL2String(settingLabel).ptr,
                new IL2String(title).ptr,
                new IntPtr(&defaultValue)
            });
            infosList.Add(newButton);
            return newButton;
        }

        static void OnClick(IntPtr buttonPtr)
        {
            if (IsHacked)
            {
                // NOTE: This type checking is required as the OnClick RVA is shared with multiple functions
                IntPtr klass = Import.Object.il2cpp_object_get_class(buttonPtr);
                while (klass != IntPtr.Zero && klass != buttonInfoBaseClass.ptr)
                {
                    klass = Import.Class.il2cpp_class_get_parent(klass);
                }
                if (klass != IntPtr.Zero)
                {
                    activeButton = buttonPtr;
                    if (duelSettingsManager.HandleButtonClick(buttonPtr))
                    {
                        return;
                    }
                }
            }
            hookOnClick.Original.Invoke(buttonPtr);
        }

        public static void UpdateDeck(int deckId)
        {
            if (IsHacked)
            {
                duelSettingsManager.UpdateDeck(activeButton, deckId);
            }
        }

        public static void UpdateData()
        {
            if (IsHacked)
            {
                YgomSystem.UI.InfinityScroll.InfinityScrollView.UpdateData(fieldIsv.GetValue(activeViewController).ptr);
            }
        }

        public static void OnUpdateData(IntPtr isv)
        {
            if (IsHacked)
            {
                IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                IntPtr roomView = YgomSystem.UI.ViewControllerManager.GetViewController(manager, YgomGame.Room.RoomCreateViewController.ClassInfo.IL2Typeof());
                if (roomView != IntPtr.Zero && fieldIsv.GetValue(roomView).ptr == isv)
                {
                    // This shouldn't be required, but there's some bug where the deck names get swapped out.
                    // I think something in the ISV caches the string pointers and they can become invalid?
                    // TODO: Look into this issue more instead of doing this hack (annoyingly reproducing it is inconsistent)
                    duelSettingsManager.UpdateDeckNames();
                }
            }
        }

        public static void OnPopChildViewController(IntPtr manager)
        {
            if (IsHacked)
            {
                IntPtr contentManager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                IntPtr roomView = YgomSystem.UI.ViewControllerManager.GetViewController(contentManager, YgomGame.Room.RoomCreateViewController.ClassInfo.IL2Typeof());
                if (roomView != IntPtr.Zero && manager == contentManager)
                {
                    // Another hack... (this time when re-entering the view controller sometimes the deck name gets swapped out, so update it)
                    duelSettingsManager.UpdateDeckNames();
                }
            }
        }

        class DuelSettingsManager
        {
            class Buttons
            {
                public Dictionary<IntPtr, DeckInfo> Decks = new Dictionary<IntPtr, DeckInfo>();
                public IntPtr LoadDeckFrom; public IntPtr ClearAllDecks; public IntPtr OpenDeckEditor;
                public IntPtr LoadIncludingDecks; public IntPtr Load; public IntPtr Save;
                public IntPtr StartingPlayer; public IntPtr LifePoints; public IntPtr Hand; public IntPtr Field;
                public IntPtr DuelType; public IntPtr Seed; public IntPtr Shuffle; public IntPtr Cpu;
                public IntPtr CpuFlag; public IntPtr Limit; public IntPtr LP1; public IntPtr LP2; public IntPtr Hand1; public IntPtr Hand2;
                public IntPtr Sleeve1; public IntPtr Sleeve2; public IntPtr Field1; public IntPtr Field2; public IntPtr FieldPart1;
                public IntPtr FieldPart2; public IntPtr Mate1; public IntPtr Mate2; public IntPtr MateBase1; public IntPtr MateBase2;
                public IntPtr Icon1; public IntPtr Icon2; public IntPtr IconFrame1; public IntPtr IconFrame2;
                public IntPtr Coin1; public IntPtr Coin2;
                public IntPtr Player1; /*public IntPtr Player2; public IntPtr Player3; public IntPtr Player4;*/
                public IntPtr BGM; public IntPtr BGM1; public IntPtr BGM2; public IntPtr BGM3;
            }

            DuelSettings settings;
            DuelSettings settingsClone;
            Buttons buttons;

            static List<string> bgmStrings = new List<string>();
            static List<string> allBgms = new List<string>();
            static List<string> allBgmsRawList = new List<string>();

            public DuelSettings Settings
            {
                get { return settings; }
            }
            public DuelSettings SettingsClone
            {
                get
                {
                    if (settingsClone == null)
                    {
                        settingsClone = new DuelSettings();
                        settingsClone.CopyFrom(settings);
                        settingsClone.LoadRandomDecks();
                        SetDuelRequiredDefaults(settingsClone);
                    }
                    return settingsClone;
                }
                set
                {
                    settingsClone = value;
                }
            }

            static void SetDuelRequiredDefaults(DuelSettings settings)
            {
                Dictionary<string, object> userProfile = YgomSystem.Utility.ClientWork.GetDict("$.User.profile");
                if (userProfile != null)
                {
                    if (string.IsNullOrEmpty(settings.name[0]))
                    {
                        settings.name[0] = Utils.GetValue<string>(userProfile, "name");
                    }
                    if (settings.icon[0] <= 0 && settings.icon[0] != -2)
                    {
                        settings.icon[0] = Utils.GetValue<int>(userProfile, "icon_id");
                    }
                    if (settings.icon_frame[0] <= 0 && settings.icon_frame[0] != -2)
                    {
                        settings.icon_frame[0] = Utils.GetValue<int>(userProfile, "icon_frame_id");
                    }
                    if (settings.avatar[0] == -1 && settings.Deck[0].Accessory.AvatarId <= 0)
                    {
                        settings.avatar[0] = Utils.GetValue<int>(userProfile, "avatar_id");
                    }
                }
                if (settings.RandSeed == 0)
                {
                    settings.RandSeed = (uint)Utils.Rand.Next();
                }
                settings.SetRequiredDefaults();
                if (settings.bgms.Count == 1)
                {
                    string bgm = settings.bgms[0];
                    int bgmValue;
                    if (int.TryParse(bgm, out bgmValue))
                    {
                        settings.BgmsFromValue(bgmValue);
                    }
                    else if (bgm == ClientSettings.CustomTextRandom)
                    {
                        settings.SetRandomBgm();
                    }
                    else if (bgm == ClientSettings.CustomTextDefault)
                    {
                        settings.bgms.Clear();
                    }
                    else if (bgm == ClientSettings.CustomTextDuelStarterP1DuelField)
                    {
                        settings.SetBgm(DuelBgmMode.Myself);
                    }
                    else if (bgm == ClientSettings.CustomTextDuelStarterP2DuelField)
                    {
                        settings.SetBgm(DuelBgmMode.Rival);
                    }
                    else if (bgm.StartsWith("BGM_"))
                    {
                        string valueStr = bgm.Split('_').Last();
                        if (int.TryParse(valueStr, out bgmValue))
                        {
                            settings.BgmsFromValue(bgmValue);
                        }
                        else
                        {
                            settings.bgms.Clear();
                        }
                    }
                    else
                    {
                        settings.bgms.Clear();
                    }
                }
                if (ClientSettings.DuelStarterShowAllBgms)
                {
                    if (settings.bgms.Count < 3 || allBgmsRawList.Count == 0)
                    {
                        settings.bgms.Clear();
                    }
                    else
                    {
                        for (int i = 0; i < settings.bgms.Count; i++)
                        {
                            string bgm = settings.bgms[i];
                            if (bgm == ClientSettings.CustomTextRandom)
                            {
                                settings.bgms[i] = allBgmsRawList[Utils.Rand.Next(allBgmsRawList.Count)];
                            }
                            else if (bgm == ClientSettings.CustomTextDefault)
                            {
                                settings.SetBgm((DuelBgmMode)YgomSystem.Utility.ClientWork.GetByJsonPath<int>("$.Persistence.App.Settings.Duelbgm"), i);
                            }
                            else if (bgm == ClientSettings.CustomTextDuelStarterP1DuelField)
                            {
                                settings.SetBgm(DuelBgmMode.Myself, i);
                            }
                            else if (bgm == ClientSettings.CustomTextDuelStarterP2DuelField)
                            {
                                settings.SetBgm(DuelBgmMode.Rival, i);
                            }
                        }
                    }
                }
                if (settings.bgms.Count == 0)
                {
                    settings.SetBgm((DuelBgmMode)YgomSystem.Utility.ClientWork.GetByJsonPath<int>("$.Persistence.App.Settings.Duelbgm"));
                }
            }

            public void Save()
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JSON (*.json)|*.json";
                sfd.RestoreDirectory = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DuelSettingsFromUI();
                        File.WriteAllText(sfd.FileName, MiniJSON.Json.Serialize(settings.ToDictionary()));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Failed to save '" + sfd.FileName + "'\n\n" + e);
                    }
                }
            }

            public void Load(bool includeDecks)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "JSON (*.json)|*.json";
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
                {
                    bool updatedData = false;
                    string exception = null;
                    try
                    {
                        Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(ofd.FileName)) as Dictionary<string, object>;
                        if (data != null)
                        {
                            DuelSettings tempSettings = new DuelSettings();
                            tempSettings.CopyFrom(settings);
                            settings.FromDictionary(data);
                            if (!includeDecks)
                            {
                                settings.CopyDecksFrom(tempSettings);
                            }
                            DuelSettingsToUI();
                            updatedData = true;
                        }
                    }
                    catch (Exception e)
                    {
                        exception = e.ToString();
                    }
                    if (!updatedData)
                    {
                        MessageBox.Show("Failed to load '" + ofd.FileName + "'\n\n" + exception);
                    }
                    UpdateData();
                }
            }

            int LookupItemName(ItemID.Category category, string name, int defaultValue = -1)
            {
                if (name == ClientSettings.CustomTextRandom)
                {
                    return -2;
                }
                else if (name == ClientSettings.CustomTextNone)
                {
                    return -3;
                }
                foreach (int value in ItemID.Values[category])
                {
                    string itemName = YgomGame.Utility.ItemUtil.GetItemName(value);
                    if (!string.IsNullOrEmpty(itemName) && itemName == name)
                    {
                        return value;
                    }
                }
                return defaultValue;
            }

            Dictionary<int, string> GetItemNames(ItemID.Category category, bool addNone = false)
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                result[-1] = ClientSettings.CustomTextDefault;
                result[-2] = ClientSettings.CustomTextRandom;
                if (addNone)
                {
                    result[-3] = ClientSettings.CustomTextNone;
                }
                foreach (int value in ItemID.Values[category])
                {
                    string name = YgomGame.Utility.ItemUtil.GetItemName(value);
                    if (!string.IsNullOrEmpty(name))
                    {
                        result[value] = name;
                    }
                }
                return result;
            }

            string GetButtonValueString(IntPtr buttonPtr)
            {
                return new IL2String(buttonInfoBaseValueText.Invoke(buttonPtr).ptr).ToString();
            }

            int GetButtonValueStringIndex(IntPtr buttonPtr)
            {
                string value = GetButtonValueString(buttonPtr);
                string[] actionSheet;
                if (buttonsActionSheets.TryGetValue(buttonPtr, out actionSheet))
                {
                    return Array.IndexOf(actionSheet, value);
                }
                return -1;
            }

            int GetButtonValueI32(IntPtr buttonPtr, int defaultValue = -1)
            {
                string valueStr = GetButtonValueString(buttonPtr);
                if (valueStr == ClientSettings.CustomTextDefault || valueStr == ClientSettings.CustomTextRandom)
                {
                    return defaultValue;
                }
                else
                {
                    int value;
                    if (int.TryParse(valueStr, out value))
                    {
                        return value;
                    }
                    return defaultValue;
                }
            }

            bool GetButtonValueBool(IntPtr buttonPtr)
            {
                return buttonBoolValueInfoCurrentValue.GetGetMethod().Invoke(buttonPtr).GetValueRef<csbool>();
            }

            void SetButtonValueBool(IntPtr buttonPtr, bool value)
            {
                buttonBoolValueInfoCurrentValue.GetSetMethod().Invoke(buttonPtr, new IntPtr[] { new IntPtr(&value) });
            }

            void SetButtonValueString(IntPtr buttonPtr, string value)
            {
                string[] actionSheet;
                if (value == null && buttonsActionSheets.TryGetValue(buttonPtr, out actionSheet) && actionSheet.Length > 0)
                {
                    value = actionSheet[0];
                }
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "    ";
                }
                buttonStringInfoCurrentValue.GetSetMethod().Invoke(buttonPtr, new IntPtr[] { new IL2String(value).ptr });
            }

            void SetButtonIndex(IntPtr buttonPtr, int index)
            {
                string[] actionSheet;
                if (buttonsActionSheets.TryGetValue(buttonPtr, out actionSheet) && index >= 0 && index < actionSheet.Length)
                {
                    SetButtonValueString(buttonPtr, actionSheet[index]);
                }
            }

            void SetButtonIndexFromI32(IntPtr buttonPtr, int value)
            {
                int index = 0;
                if (value == -3)
                {
                    index = 2;
                }
                else if (value == -2)
                {
                    index = 1;
                }
                else if (value != -1)
                {
                    string[] actionSheet;
                    if (buttonsActionSheets.TryGetValue(buttonPtr, out actionSheet))
                    {
                        int len = actionSheet.Length;
                        for (int i = 0; i < len; i++)
                        {
                            int itemI32;
                            if (int.TryParse(new IL2String(actionSheet[i]).ToString(), out itemI32) && itemI32 == value)
                            {
                                index = i;
                                break;
                            }
                        }
                    }
                }
                SetButtonIndex(buttonPtr, index);
            }

            void SetButtonIndexFromItemId(IntPtr buttonPtr, int itemId)
            {
                int index = 0;
                if (itemId > 0)
                {
                    string name = YgomGame.Utility.ItemUtil.GetItemName(itemId);
                    if (!string.IsNullOrEmpty(name))
                    {
                        string[] actionSheet;
                        if (buttonsActionSheets.TryGetValue(buttonPtr, out actionSheet))
                        {
                            int len = actionSheet.Length;
                            for (int i = 0; i < len; i++)
                            {
                                if (new IL2String(actionSheet[i]).ToString() == name)
                                {
                                    index = i;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (itemId == -2)
                {
                    index = 1;
                }
                else if (itemId == -3)
                {
                    index = 2;
                }
                SetButtonIndex(buttonPtr, index);
            }

            void SetTeamValue(int value, int[] array, int teamPlayer1, int teamPlayer2, bool setDefault)
            {
                if (value != -1 || setDefault)
                {
                    array[teamPlayer1] = value;
                    array[teamPlayer2] = value;
                }
            }

            public void DuelSettingsFromUI()
            {
                settings.FirstPlayer = GetButtonValueI32(buttons.StartingPlayer);
                if (settings.FirstPlayer >= 0)
                {
                    settings.FirstPlayer--;
                }
                int lp = GetButtonValueI32(buttons.LifePoints);
                for (int i = 0; i < DuelSettings.MaxPlayers; i++)
                {
                    settings.life[i] = lp;
                }
                int hand = GetButtonValueI32(buttons.Hand);
                for (int i = 0; i < DuelSettings.MaxPlayers; i++)
                {
                    settings.hnum[i] = hand;
                }
                settings.SharedField = LookupItemName(ItemID.Category.FIELD, GetButtonValueString(buttons.Field));
                DuelType duelType;
                Enum.TryParse<DuelType>(GetButtonValueString(buttons.DuelType), out duelType);
                settings.Type = (int)duelType;
                settings.RandSeed = (uint)GetButtonValueI32(buttons.Seed, 0);
                settings.noshuffle = !GetButtonValueBool(buttons.Shuffle);
                settings.cpu = GetButtonValueI32(buttons.Cpu, int.MaxValue);
                DuelCpuParam cpuParam;
                if (Enum.TryParse<DuelCpuParam>(GetButtonValueString(buttons.CpuFlag), out cpuParam) && cpuParam != DuelCpuParam.None)
                {
                    settings.cpuflag = cpuParam.ToString();
                }
                else
                {
                    settings.cpuflag = null;
                }
                DuelLimitedType limitedType;
                Enum.TryParse<DuelLimitedType>(GetButtonValueString(buttons.Limit), out limitedType);
                settings.Limit = (int)limitedType;
                settings.bgms.Clear();
                if (ClientSettings.DuelStarterShowAllBgms)
                {
                    settings.bgms.Add(GetButtonValueString(buttons.BGM1));
                    settings.bgms.Add(GetButtonValueString(buttons.BGM2));
                    settings.bgms.Add(GetButtonValueString(buttons.BGM3));
                }
                else
                {
                    settings.bgms.Add(GetButtonValueString(buttons.BGM));
                }
                settings.MyType = GetButtonValueStringIndex(buttons.Player1) == 0 ? 0 : 1;
                //settings.OpponentType = GetButtonValueIndex(buttons.Player2) == 0 ? 1 : 2;
                //settings.MyPartnerType = GetButtonValueIndex(buttons.Player3) == 0 ? 0 : 1;
                //settings.OpponentPartnerType = GetButtonValueIndex(buttons.Player4) == 0 ? 1 : 2;
                SetTeamValue(GetButtonValueI32(buttons.LP1), settings.life, 0, 2, false);
                SetTeamValue(GetButtonValueI32(buttons.LP2), settings.life, 1, 3, false);
                SetTeamValue(GetButtonValueI32(buttons.Hand1), settings.hnum, 0, 2, false);
                SetTeamValue(GetButtonValueI32(buttons.Hand2), settings.hnum, 1, 3, false);
                SetTeamValue(LookupItemName(ItemID.Category.PROTECTOR, GetButtonValueString(buttons.Sleeve1)), settings.sleeve, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.PROTECTOR, GetButtonValueString(buttons.Sleeve2)), settings.sleeve, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.FIELD, GetButtonValueString(buttons.Field1)), settings.mat, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.FIELD, GetButtonValueString(buttons.Field2)), settings.mat, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.FIELD_OBJ, GetButtonValueString(buttons.FieldPart1)), settings.duel_object, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.FIELD_OBJ, GetButtonValueString(buttons.FieldPart2)), settings.duel_object, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.AVATAR, GetButtonValueString(buttons.Mate1)), settings.avatar, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.AVATAR, GetButtonValueString(buttons.Mate2)), settings.avatar, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.AVATAR_HOME, GetButtonValueString(buttons.MateBase1)), settings.avatar_home, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.AVATAR_HOME, GetButtonValueString(buttons.MateBase2)), settings.avatar_home, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.ICON, GetButtonValueString(buttons.Icon1)), settings.icon, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.ICON, GetButtonValueString(buttons.Icon2)), settings.icon, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.ICON_FRAME, GetButtonValueString(buttons.IconFrame1)), settings.icon_frame, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.ICON_FRAME, GetButtonValueString(buttons.IconFrame2)), settings.icon_frame, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.COIN, GetButtonValueString(buttons.Coin1)), settings.coin, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.COIN, GetButtonValueString(buttons.Coin2)), settings.coin, 1, 3, true);

                /*TODO: Fix up DeckInfo.File loading/saving so this code can be used // Reload decks which are loaded from files
                for (int i = 0; i < settings.Deck.Length; i++)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(settings.Deck[i].File) && !settings.Deck[i].IsRandomDeckPath && File.Exists(settings.Deck[i].File))
                        {
                            settings.Deck[i].Load();
                        }
                    }
                    catch
                    {
                    }
                }*/

                YgomSystem.Utility.ClientWork.UpdateJson("$.Persistence.System.DuelStarterUI", MiniJSON.Json.Serialize(settings.ToDictionary()));
            }

            void DuelSettingsToUI()
            {
                SetButtonIndex(buttons.StartingPlayer, settings.FirstPlayer < 0 ? 0 : (settings.FirstPlayer + 1));
                SetButtonIndexFromI32(buttons.LifePoints, settings.AreAllEqual(settings.life) ? settings.life[0] : -1);
                SetButtonIndexFromI32(buttons.Hand, settings.AreAllEqual(settings.hnum) ? settings.hnum[0] : -1);
                SetButtonIndexFromItemId(buttons.Field, settings.SharedField);
                SetButtonValueString(buttons.DuelType, ((DuelType)settings.Type).ToString());
                SetButtonValueString(buttons.Seed, settings.RandSeed == 0 ? null : settings.RandSeed.ToString());
                SetButtonValueBool(buttons.Shuffle, !settings.noshuffle);
                SetButtonValueString(buttons.Cpu, settings.cpu == int.MaxValue ? null : settings.cpu.ToString());
                SetButtonValueString(buttons.CpuFlag, settings.cpuflag);
                SetButtonValueString(buttons.Limit, ((DuelLimitedType)settings.Limit).ToString());
                if (ClientSettings.DuelStarterShowAllBgms)
                {
                    SetButtonValueString(buttons.BGM1, settings.bgms.Count < 3 ? null : settings.bgms[0]);
                    SetButtonValueString(buttons.BGM2, settings.bgms.Count < 3 ? null : settings.bgms[1]);
                    SetButtonValueString(buttons.BGM3, settings.bgms.Count < 3 ? null : settings.bgms[2]);
                }
                else
                {
                    SetButtonValueString(buttons.BGM, settings.bgms.Count == 0 ? null : settings.bgms[0]);
                }
                SetButtonIndex(buttons.Player1, settings.MyType == 0 ? 0 : 1);
                //SetButtonIndex(buttons.Player2, settings.OpponentType == 1 ? 0 : 1);
                //SetButtonIndex(buttons.Player3, settings.MyPartnerType == 0 ? 0 : 1);
                //SetButtonIndex(buttons.Player4, settings.OpponentPartnerType == 1 ? 0 : 1);
                SetButtonIndexFromI32(buttons.LP1, settings.AreAllEqual(settings.life) ? -1 : settings.life[0]);
                SetButtonIndexFromI32(buttons.LP2, settings.AreAllEqual(settings.life) ? -1 : settings.life[1]);
                SetButtonIndexFromI32(buttons.Hand1, settings.AreAllEqual(settings.hnum) ? -1 : settings.hnum[0]);
                SetButtonIndexFromI32(buttons.Hand2, settings.AreAllEqual(settings.hnum) ? -1 : settings.hnum[1]);
                SetButtonIndexFromItemId(buttons.Sleeve1, settings.AreAllEqual(settings.sleeve) ? -1 : settings.sleeve[0]);
                SetButtonIndexFromItemId(buttons.Sleeve2, settings.AreAllEqual(settings.sleeve) ? -1 : settings.sleeve[1]);
                SetButtonIndexFromItemId(buttons.Field1, settings.AreAllEqual(settings.mat) ? -1 : settings.mat[0]);
                SetButtonIndexFromItemId(buttons.Field2, settings.AreAllEqual(settings.mat) ? -1 : settings.mat[1]);
                SetButtonIndexFromItemId(buttons.FieldPart1, settings.duel_object[0]);
                SetButtonIndexFromItemId(buttons.FieldPart2, settings.duel_object[1]);
                SetButtonIndexFromItemId(buttons.Mate1, settings.avatar[0]);
                SetButtonIndexFromItemId(buttons.Mate2, settings.avatar[1]);
                SetButtonIndexFromItemId(buttons.MateBase1, settings.avatar_home[0]);
                SetButtonIndexFromItemId(buttons.MateBase2, settings.avatar_home[1]);
                SetButtonIndexFromItemId(buttons.Icon1, settings.icon[0]);
                SetButtonIndexFromItemId(buttons.Icon2, settings.icon[1]);
                SetButtonIndexFromItemId(buttons.IconFrame1, settings.icon_frame[0]);
                SetButtonIndexFromItemId(buttons.IconFrame2, settings.icon_frame[1]);
                SetButtonIndexFromItemId(buttons.Coin1, settings.coin[0]);
                SetButtonIndexFromItemId(buttons.Coin2, settings.coin[1]);
                foreach (KeyValuePair<IntPtr, DeckInfo> deck in buttons.Decks)
                {
                    UpdateDeckName(deck.Key, deck.Value);
                }
            }

            public void InitButtons(IntPtr viewController)
            {
                bool hasExistingSetting = settings != null;
                if (!hasExistingSetting)
                {
                    settings = new DuelSettings();
                }
                buttonsActionSheets.Clear();
                buttons = new Buttons();
                IL2ListExplicit infosList = new IL2ListExplicit(IntPtr.Zero, templateInfoClass, true);
                fieldInfos.SetValue(viewController, infosList.ptr);
                IntPtr isv = fieldIsv.GetValue(viewController).ptr;

                string[] lpStrings = new string[]
                {
                    ClientSettings.CustomTextDefault, "1", "50", "100", "500", "1000", "2000", "3000", "4000", "5000","6000", "7000", "8000", "9000",
                    "10000", "15000", "20000", "25000", "50000", "100000", "250000", "500000", "1000000", "9999999"
                };
                string[] handStrings = new string[]
                {
                    ClientSettings.CustomTextDefault, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15",
                    "20", "25", "30", "35", "45", "55", "60", "65", "65", "70", "75", "80", "85", "90", "95", "100"
                };
                List<string> cpuParamStrings = new List<string>();
                cpuParamStrings.Add(ClientSettings.CustomTextDefault);
                for (int i = 0; i <= 100; i++)
                {
                    cpuParamStrings.Add(i.ToString());
                }
                List<string> seedStrings = new List<string>();
                seedStrings.Add(ClientSettings.CustomTextRandom);
                for (int i = 1; i <= 500; i++)
                {
                    seedStrings.Add(i.ToString());
                }
                List<string> cpuFlagStrings = new List<string>();
                foreach (string name in Enum.GetNames(typeof(DuelCpuParam)))
                {
                    cpuFlagStrings.Add(name);
                }
                List<string> limitedTypeStrings = new List<string>();
                foreach (string name in Enum.GetNames(typeof(DuelLimitedType)))
                {
                    limitedTypeStrings.Add(name);
                }
                List<string> duelType = new List<string>();
                duelType.Add(DuelType.Normal.ToString());
                duelType.Add(DuelType.Speed.ToString());
                duelType.Add(DuelType.Rush.ToString());
                //duelType.Add(DuelType.Tag.ToString());

                if (bgmStrings.Count == 0)
                {
                    bgmStrings.Add(ClientSettings.CustomTextDefault);
                    bgmStrings.Add(ClientSettings.CustomTextRandom);
                    bgmStrings.Add(ClientSettings.CustomTextDuelStarterP1DuelField);
                    bgmStrings.Add(ClientSettings.CustomTextDuelStarterP2DuelField);
                    for (int i = 1; i < 99; i++)
                    {
                        if (AssetHelper.FileExists("Sound/AudioClip/BGM/BGM_DUEL_NORMAL_" + i.ToString().PadLeft(2, '0')))
                        {
                            bgmStrings.Add(i.ToString());
                        }
                    }
                }

                AddLabel(infosList, ClientSettings.CustomTextDuelStarterDecks);
                AddDeckDetailsButton(infosList, 0);
                AddDeckDetailsButton(infosList, 1);
                //AddDeckDetailsButton(infosList, 2);
                //AddDeckDetailsButton(infosList, 3);
                buttons.LoadDeckFrom = AddButtonString(infosList, "LoadDeckFrom", ClientSettings.CustomTextDuelStarterLoadDeckFrom, new string[]
                {
                    ClientSettings.CustomTextDuelStarterLoadDeckFromGame,
                    ClientSettings.CustomTextDuelStarterLoadDeckFromFile,
                    ClientSettings.CustomTextDuelStarterLoadDeckFromFolder
                });
                buttons.OpenDeckEditor = AddButtonString(infosList, "OpenDeckEditor", ClientSettings.CustomTextDuelStarterOpenDeckEditor, null);
                AddLabel(infosList, ClientSettings.CustomTextDuelStarterSettings);
                buttons.StartingPlayer = AddButtonString(infosList, "StartingPlayer", ClientSettings.CustomTextDuelStarterStartingPlayer, new string[] { ClientSettings.CustomTextRandom, "1", "2"/*, "3", "4"*/ });
                buttons.LifePoints = AddButtonString(infosList, "LifePoints", ClientSettings.CustomTextDuelStarterLifePoints, lpStrings);
                buttons.Hand = AddButtonString(infosList, "Hand", ClientSettings.CustomTextDuelStarterHand, handStrings);
                buttons.Field = AddButtonString(infosList, "Field", ClientSettings.CustomTextDuelStarterField, GetItemNames(ItemID.Category.FIELD).Values.ToArray());
                buttons.DuelType = AddButtonString(infosList, "DuelType", ClientSettings.CustomTextDuelStarterDuelType, duelType.ToArray());
                AddLabel(infosList, ClientSettings.CustomTextDuelStarterAdvancedSettings);
                buttons.Seed = AddButtonString(infosList, "Seed", ClientSettings.CustomTextDuelStarterSeed, seedStrings.ToArray());
                buttons.Shuffle = AddButtonBool(infosList, "Shuffle", ClientSettings.CustomTextDuelStarterShuffle, true);
                buttons.Cpu = AddButtonString(infosList, "Cpu", ClientSettings.CustomTextDuelStarterCpu, cpuParamStrings.ToArray());
                buttons.CpuFlag = AddButtonString(infosList, "CpuFlag", ClientSettings.CustomTextDuelStarterCpuFlag, cpuFlagStrings.ToArray());
                buttons.Limit = AddButtonString(infosList, "Limit", ClientSettings.CustomTextDuelStarterLimit, limitedTypeStrings.ToArray());
                if (ClientSettings.DuelStarterShowAllBgms)
                {
                    if (allBgms.Count == 0)
                    {
                        allBgms.Add(ClientSettings.CustomTextDefault);
                        allBgms.Add(ClientSettings.CustomTextRandom);
                        allBgms.Add(ClientSettings.CustomTextDuelStarterP1DuelField);
                        allBgms.Add(ClientSettings.CustomTextDuelStarterP2DuelField);
                        try
                        {
                            foreach (string line in File.ReadAllLines(Path.Combine(Program.DataDir, "Bgm.json")))
                            {
                                if (line.StartsWith("//BGM_"))
                                {
                                    string[] splitted = line.TrimStart('/').Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string bgm in splitted)
                                    {
                                        if (bgm.StartsWith("BGM_"))
                                        {
                                            allBgms.Add(bgm);
                                            allBgmsRawList.Add(bgm);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    buttons.BGM1 = AddButtonString(infosList, "BGM1", ClientSettings.CustomTextDuelStarterBGM1, allBgms.ToArray());
                    buttons.BGM2 = AddButtonString(infosList, "BGM2", ClientSettings.CustomTextDuelStarterBGM2, allBgms.ToArray());
                    buttons.BGM3 = AddButtonString(infosList, "BGM3", ClientSettings.CustomTextDuelStarterBGM3, allBgms.ToArray());
                }
                else
                {
                    buttons.BGM = AddButtonString(infosList, "BGM", ClientSettings.CustomTextDuelStarterBGM, bgmStrings.ToArray());
                }
                buttons.Player1 = AddButtonString(infosList, "Player1", ClientSettings.CustomTextDuelStarterP1, new string[] { ClientSettings.CustomTextDuelStarterPlayer, ClientSettings.CustomTextDuelStarterCPU });
                //buttons.Player2 = AddButtonString(infosList, "Player2", ClientSettings.CustomTextDuelStarterP2, new string[] { ClientSettings.CustomTextDuelStarterPlayer, ClientSettings.CustomTextDuelStarterCPU });
                //buttons.Player3 = AddButtonString(infosList, "Player3", ClientSettings.CustomTextDuelStarterP3, new string[] { ClientSettings.CustomTextDuelStarterPlayer, ClientSettings.CustomTextDuelStarterCPU });
                //buttons.Player4 = AddButtonString(infosList, "Player4", ClientSettings.CustomTextDuelStarterP4, new string[] { ClientSettings.CustomTextDuelStarterPlayer, ClientSettings.CustomTextDuelStarterCPU });
                buttons.LP1 = AddButtonString(infosList, "LP1", ClientSettings.CustomTextDuelStarterLP1, lpStrings);
                buttons.LP2 = AddButtonString(infosList, "LP2", ClientSettings.CustomTextDuelStarterLP2, lpStrings);
                buttons.Hand1 = AddButtonString(infosList, "Hand1", ClientSettings.CustomTextDuelStarterHand1, handStrings);
                buttons.Hand2 = AddButtonString(infosList, "Hand2", ClientSettings.CustomTextDuelStarterHand2, handStrings);
                buttons.Sleeve1 = AddButtonString(infosList, "Sleeve1", ClientSettings.CustomTextDuelStarterSleeve1, GetItemNames(ItemID.Category.PROTECTOR).Values.ToArray());
                buttons.Sleeve2 = AddButtonString(infosList, "Sleeve2", ClientSettings.CustomTextDuelStarterSleeve2, GetItemNames(ItemID.Category.PROTECTOR).Values.ToArray());
                buttons.Field1 = AddButtonString(infosList, "Field1", ClientSettings.CustomTextDuelStarterField1, GetItemNames(ItemID.Category.FIELD).Values.ToArray());
                buttons.Field2 = AddButtonString(infosList, "Field2", ClientSettings.CustomTextDuelStarterField2, GetItemNames(ItemID.Category.FIELD).Values.ToArray());
                buttons.FieldPart1 = AddButtonString(infosList, "FieldPart1", ClientSettings.CustomTextDuelStarterFieldPart1, GetItemNames(ItemID.Category.FIELD_OBJ).Values.ToArray());
                buttons.FieldPart2 = AddButtonString(infosList, "FieldPart2", ClientSettings.CustomTextDuelStarterFieldPart2, GetItemNames(ItemID.Category.FIELD_OBJ).Values.ToArray());
                buttons.Mate1 = AddButtonString(infosList, "Mate1", ClientSettings.CustomTextDuelStarterMate1, GetItemNames(ItemID.Category.AVATAR, true).Values.ToArray());
                buttons.Mate2 = AddButtonString(infosList, "Mate2", ClientSettings.CustomTextDuelStarterMate2, GetItemNames(ItemID.Category.AVATAR, true).Values.ToArray());
                buttons.MateBase1 = AddButtonString(infosList, "MateBase1", ClientSettings.CustomTextDuelStarterMateBase1, GetItemNames(ItemID.Category.AVATAR_HOME, true).Values.ToArray());
                buttons.MateBase2 = AddButtonString(infosList, "MateBase2", ClientSettings.CustomTextDuelStarterMateBase2, GetItemNames(ItemID.Category.AVATAR_HOME, true).Values.ToArray());
                buttons.Icon1 = AddButtonString(infosList, "Icon1", ClientSettings.CustomTextDuelStarterIcon1, GetItemNames(ItemID.Category.ICON).Values.ToArray());
                buttons.Icon2 = AddButtonString(infosList, "Icon2", ClientSettings.CustomTextDuelStarterIcon2, GetItemNames(ItemID.Category.ICON).Values.ToArray());
                buttons.IconFrame1 = AddButtonString(infosList, "IconFrame1", ClientSettings.CustomTextDuelStarterIconFrame1, GetItemNames(ItemID.Category.ICON_FRAME).Values.ToArray());
                buttons.IconFrame2 = AddButtonString(infosList, "IconFrame2", ClientSettings.CustomTextDuelStarterIconFrame2, GetItemNames(ItemID.Category.ICON_FRAME).Values.ToArray());
                buttons.Coin1 = AddButtonString(infosList, "Coin1", ClientSettings.CustomTextDuelStarterCoin1, GetItemNames(ItemID.Category.COIN).Values.ToArray());
                buttons.Coin2 = AddButtonString(infosList, "Coin2", ClientSettings.CustomTextDuelStarterCoin2, GetItemNames(ItemID.Category.COIN).Values.ToArray());
                AddLabel(infosList, ClientSettings.CustomTextDuelStarterLoadSave);
                buttons.LoadIncludingDecks = AddButtonString(infosList, "LoadIncludingDecks", ClientSettings.CustomTextDuelStarterLoadIncludingDecks, null);
                buttons.Load = AddButtonString(infosList, "Load", ClientSettings.CustomTextDuelStarterLoad, null);
                buttons.Save = AddButtonString(infosList, "Save", ClientSettings.CustomTextDuelStarterSave, null);
                AddLabel(infosList, ClientSettings.CustomTextDuelStarterExta);
                buttons.ClearAllDecks = AddButtonString(infosList, "ClearAllDecks", ClientSettings.CustomTextDuelStarterClearSelectedDecks, null);

                if (!hasExistingSetting)
                {
                    try
                    {
                        Dictionary<string, object> dict = YgomSystem.Utility.ClientWork.GetDict("$.Persistence.System.DuelStarterUI");
                        if (dict != null && dict.Count > 0)
                        {
                            settings.FromDictionary(dict);
                            hasExistingSetting = true;
                        }
                    }
                    catch
                    {
                    }
                }
                if (hasExistingSetting)
                {
                    DuelSettingsToUI();
                }
            }

            void AddDeckDetailsButton(IL2ListExplicit infosList, int deckIndex)
            {
                // NOTE: Changed from string.Empty to "   " as I think there's a bug with empty strings in IL2String
                DeckInfo deckInfo = settings.Deck[deckIndex];
                IntPtr button = AddButtonString(infosList, "deck" + (deckIndex + 1), ClientSettings.CustomTextDuelStarterDeck + (deckIndex + 1), new string[] { "   " });//string.Empty });
                buttons.Decks[button] = deckInfo;
            }

            public bool HandleButtonClick(IntPtr buttonPtr)
            {
                if (buttonPtr == buttons.ClearAllDecks)
                {
                    foreach (KeyValuePair<IntPtr, DeckInfo> deck in buttons.Decks)
                    {
                        deck.Value.Clear();
                        UpdateDeckName(deck.Key, deck.Value);
                    }
                    UpdateData();
                }
                else if (buttonPtr == buttons.OpenDeckEditor)
                {
                    IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                    YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "DeckEdit/DeckSelect");
                }
                else if (buttonPtr == buttons.LoadIncludingDecks)
                {
                    Load(includeDecks: true);
                    UpdateData();
                }
                else if (buttonPtr == buttons.Load)
                {
                    Load(includeDecks: false);
                    UpdateData();
                }
                else if (buttonPtr == buttons.Save)
                {
                    Save();
                    UpdateData();
                }
                else if (buttonPtr == buttons.Shuffle)
                {
                    buttonBoolValueInfoSwitchValue.Invoke(buttonPtr);
                    UpdateData();
                }
                else
                {
                    DeckInfo deck;
                    if (buttons.Decks.TryGetValue(buttonPtr, out deck))
                    {
                        int loadDeckFrom = Array.IndexOf(buttonsActionSheets[buttons.LoadDeckFrom], GetButtonValueString(buttons.LoadDeckFrom));
                        switch (loadDeckFrom)
                        {
                            case 0:// Game
                                {
                                    IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                                    YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "DeckEdit/DeckSelect",
                                        new Dictionary<string, object>()
                                        {
                                            { "GameMode", (int)9 }// SoloSingle
                                        });
                                }
                                break;
                            case 1:// File
                                {
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
                                        try
                                        {
                                            string extension = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                                            switch (extension)
                                            {
                                                case ".ydk":
                                                    deck.Clear();
                                                    deck.File = ofd.FileName;
                                                    YdkHelper.LoadDeck(deck);
                                                    break;
                                                case ".json":
                                                    deck.Clear();
                                                    deck.File = ofd.FileName;
                                                    deck.FromDictionaryEx(MiniJSON.Json.DeserializeStripped(
                                                        File.ReadAllText(ofd.FileName)) as Dictionary<string, object>);
                                                    break;
                                                default:
                                                    deck.Clear();
                                                    break;
                                            }
                                            UpdateDeckName(buttonPtr, deck);
                                            UpdateData();
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Load deck failed '" + ofd.FileName + "' exception: " + e);
                                        }
                                    }
                                }
                                break;
                            case 2:// Folder (random deck)
                                {
                                    string decksDir = Path.Combine(Program.LocalPlayerSaveDataDir, "Decks");
                                    Utils.TryCreateDirectory(decksDir);

                                    if (deck.IsRandomDeckPath)
                                    {
                                        decksDir = deck.File;
                                    }

                                    /*FolderBrowserDialog fbd = new FolderBrowserDialog();
                                    fbd.SelectedPath = decksDir;
                                    if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath) && Directory.Exists(fbd.SelectedPath))*/
                                    FolderPicker fbd = new FolderPicker();
                                    fbd.InputPath = decksDir;
                                    if (fbd.ShowDialog() == true)
                                    {
                                        DirectoryInfo directoryInfo = new DirectoryInfo(fbd.ResultPath);//fbd.SelectedPath);
                                        deck.Clear();
                                        deck.File = directoryInfo.FullName;
                                        UpdateDeckName(buttonPtr, deck);
                                        UpdateData();
                                    }
                                }
                                break;
                        }
                        return true;
                    }
                    else
                    {
                        // TODO: Ensure this is a ButtonStringInfo
                        string[] actionSheet;
                        if (buttonsActionSheets.TryGetValue(buttonPtr, out actionSheet))
                        {
                            string title = new IL2String(buttonInfoBaseTitle.GetValue(buttonPtr).ptr).ToString();
                            YgomGame.Menu.ActionSheetViewController.Open(title, actionSheet, OnClickActionSheetMenuItem);
                        }
                    }
                }
                return false;
            }

            static Action<IntPtr, int> OnClickActionSheetMenuItem = OnClickActionSheetMenuItemImpl;
            static void OnClickActionSheetMenuItemImpl(IntPtr ctx, int index)
            {
                string[] actionSheet;
                if (buttonsActionSheets.TryGetValue(activeButton, out actionSheet) && index >= 0 && index < actionSheet.Length)
                {
                    duelSettingsManager.SetButtonValueString(activeButton, actionSheet[index]);
                    UpdateData();
                }
            }

            public void UpdateDeck(IntPtr buttonPtr, int deckId)
            {
                DeckInfo deck;
                if (buttons.Decks.TryGetValue(buttonPtr, out deck))
                {
                    Dictionary<string, object> dataCombined = new Dictionary<string, object>();

                    string deckDataStr = YgomSystem.Utility.ClientWork.SerializePath("DeckList." + deckId);
                    if (!string.IsNullOrEmpty(deckDataStr))
                    {
                        Dictionary<string, object> data = MiniJSON.Json.Deserialize(deckDataStr) as Dictionary<string, object>;
                        if (data != null)
                        {
                            foreach (KeyValuePair<string, object> entry in data)
                            {
                                dataCombined[entry.Key] = entry.Value;
                            }
                        }
                    }

                    string deckInfoStr = YgomSystem.Utility.ClientWork.SerializePath("Deck.list." + deckId);
                    if (!string.IsNullOrEmpty(deckInfoStr))
                    {
                        Dictionary<string, object> info = MiniJSON.Json.Deserialize(deckInfoStr) as Dictionary<string, object>;
                        if (info != null)
                        {
                            foreach (KeyValuePair<string, object> entry in info)
                            {
                                dataCombined[entry.Key] = entry.Value;
                            }
                        }
                    }

                    deck.Clear();
                    deck.FromDictionaryEx(dataCombined);
                    UpdateDeckName(buttonPtr, deck);
                    UpdateData();
                }
            }

            public void UpdateDeckNames()
            {
                foreach (KeyValuePair<IntPtr, DeckInfo> deck in buttons.Decks)
                {
                    UpdateDeckName(deck.Key, deck.Value);
                }
                UpdateData();
            }

            void UpdateDeckName(IntPtr buttonPtr, DeckInfo deck)
            {
                buttonStringInfoCurrentValue.GetSetMethod().Invoke(buttonPtr, new IntPtr[] { new IL2String(GetDeckName(deck)).ptr });
            }

            string GetDeckName(DeckInfo deck)
            {
                if (deck.IsRandomDeckPath)
                {
                    return ClientSettings.CustomTextRandom + " (" + new DirectoryInfo(deck.File).Name + ")";
                }
                if (!string.IsNullOrEmpty(deck.Name))
                {
                    return deck.Name;
                }
                if (!string.IsNullOrEmpty(deck.File))
                {
                    return Path.GetFileName(deck.File);
                }
                return "   ";//string.Empty;
            }
        }
    }
}

namespace YgomGame.DeckBrowser
{
    unsafe static class DeckBrowserViewController
    {
        static IL2Field fieldOnCompleteSelectDeckCallback;
        static IL2Method methodActionInvoke;

        delegate void Del_SelectDeck(IntPtr thisPtr, int mode, int deckId, int eventID);
        static Hook<Del_SelectDeck> hookSelectDeck;

        static DeckBrowserViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("DeckBrowserViewController", "YgomGame.DeckBrowser");
            hookSelectDeck = new Hook<Del_SelectDeck>(SelectDeck, classInfo.GetMethod("SelectDeck"));
            fieldOnCompleteSelectDeckCallback = classInfo.GetField("onCompleteSelectDeckCallback");

            IL2Class actionClassInfo = new IL2Class(Import.Class.il2cpp_class_from_type(fieldOnCompleteSelectDeckCallback.ReturnType.ptr));
            methodActionInvoke = actionClassInfo.GetMethod("Invoke");
        }

        static void SelectDeck(IntPtr thisPtr, int mode, int deckId, int eventID)
        {
            IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
            IntPtr roomView = YgomSystem.UI.ViewControllerManager.GetViewController(manager, YgomGame.Room.RoomCreateViewController.ClassInfo.IL2Typeof());
            if (roomView != IntPtr.Zero)
            {
                YgomGame.Room.RoomCreateViewController.UpdateDeck(deckId);
                // Calling this directly avoids sending data and stops the useless additional confirmation popup
                methodActionInvoke.Invoke(fieldOnCompleteSelectDeckCallback.GetValue(thisPtr));
                return;
            }
            hookSelectDeck.Original(thisPtr, mode, deckId, eventID);
        }
    }
}

namespace YgomGame.Menu
{
    unsafe static class ContentViewControllerManager
    {
        static IL2Method methodGetManager;

        static ContentViewControllerManager()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ContentViewControllerManager", "YgomGame.Menu");
            methodGetManager = classInfo.GetMethod("GetManager");
        }

        public static IntPtr GetManager()
        {
            IL2Object result = methodGetManager.Invoke();
            return result != null ? result.ptr : IntPtr.Zero;
        }
    }

    // This is only used to hook NotificationStackRemove, unfortunately the ViewController implementation is shared between many functions
    // (and therefore cannot be hooked without impacting other things)
    unsafe static class BaseMenuViewController
    {
        delegate void Del_NotificationStackRemove(IntPtr thisPtr);
        static Hook<Del_NotificationStackRemove> hookNotificationStackRemove;

        static BaseMenuViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("BaseMenuViewController", "YgomGame.Menu");
            hookNotificationStackRemove = new Hook<Del_NotificationStackRemove>(NotificationStackRemove, classInfo.GetMethod("NotificationStackRemove"));
        }

        static void NotificationStackRemove(IntPtr thisPtr)
        {
            if (thisPtr == YgomGame.Room.RoomCreateViewController.HackedInstance)
            {
                YgomGame.Room.RoomCreateViewController.NotificationStackRemove(thisPtr);
            }
            hookNotificationStackRemove.Original(thisPtr);
        }
    }
}

namespace YgomSystem.UI
{
    unsafe static class ViewControllerManager
    {
        static IL2Method methodGetViewControllerManagerWithName;
        static IL2Method methodPopChildViewController;
        static IL2Method methodPushChildViewControllerWithArgs;
        static IL2Method methodPushChildViewControllerObj;
        static IL2Method methodSwapBottomChildViewController;
        static IL2Method methodSwapTopChildViewController;
        static IL2Method methodGetStackTopViewController;
        static IL2Method methodLoadViewControllerPrefab;
        static IL2Method methodGetViewControllerT;
        static Dictionary<IntPtr, IL2Method> methodGetViewControllerTInstances = new Dictionary<IntPtr, IL2Method>();

        delegate void Del_PushChildViewController(IntPtr thisPtr, IntPtr prefabpathPtr);
        static Hook<Del_PushChildViewController> hookPushChildViewController;
        delegate void Del_PopChildViewController2(IntPtr thisPtr, IntPtr popTatget);
        static Hook<Del_PopChildViewController2> hookPopChildViewController2;
        delegate IntPtr Del_LoadViewControllerPrefab(IntPtr thisPtr, IntPtr prefabpathPtr);
        static Hook<Del_LoadViewControllerPrefab> hookLoadViewControllerPrefab;

        static ViewControllerManager()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ViewControllerManager", "YgomSystem.UI");
            methodGetViewControllerManagerWithName = classInfo.GetMethod("GetViewControllerManagerWithName");
            methodLoadViewControllerPrefab = classInfo.GetMethod("LoadViewControllerPrefab");
            methodPopChildViewController = classInfo.GetMethod("PopChildViewController", x => x.GetParameters().Length == 0);
            hookPopChildViewController2 = new Hook<Del_PopChildViewController2>(PopChildViewController, classInfo.GetMethod("PopChildViewController", x => x.GetParameters().Length == 1));
            hookPushChildViewController = new Hook<Del_PushChildViewController>(PushChildViewControllerHook, classInfo.GetMethod("PushChildViewController", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Name == "prefabpath"));
            methodPushChildViewControllerWithArgs = classInfo.GetMethod("PushChildViewController", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Name == "prefabpath");
            methodPushChildViewControllerObj = classInfo.GetMethod("PushChildViewController", x => x.GetParameters()[0].Name == "prefab");
            methodSwapBottomChildViewController = classInfo.GetMethod("SwapBottomChildViewController", x => x.GetParameters()[0].Name == "prefabpath");
            methodSwapTopChildViewController = classInfo.GetMethod("SwapTopChildViewController", x => x.GetParameters()[0].Name == "prefabpath");
            methodGetStackTopViewController = classInfo.GetMethod("GetStackTopViewController");
            hookLoadViewControllerPrefab = new Hook<Del_LoadViewControllerPrefab>(LoadViewControllerPrefab, classInfo.GetMethod("LoadViewControllerPrefab"));
            methodGetViewControllerT = classInfo.GetMethod("GetViewController");
        }

        public static IntPtr LoadViewControllerPrefab(IntPtr thisPtr, IntPtr prefabpathPtr)
        {
            /*if (prefabpathPtr != IntPtr.Zero)
            {
                string prefabpath = new IL2String(prefabpathPtr).ToString();
                Console.WriteLine("vc: " + prefabpath);
            }*/
            return hookLoadViewControllerPrefab.Original.Invoke(thisPtr, prefabpathPtr);
        }

        public static IntPtr GetViewControllerManagerWithName(string name)
        {
            IL2Object result = methodGetViewControllerManagerWithName.Invoke(new IntPtr[] { new IL2String(name).ptr });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static void SwapBottomChildViewController(IntPtr thisPtr, string prefabpath)
        {
            methodSwapBottomChildViewController.Invoke(thisPtr, new IntPtr[] { new IL2String(prefabpath).ptr });
        }

        public static void SwapTopChildViewController(IntPtr thisPtr, string prefabpath)
        {
            methodSwapTopChildViewController.Invoke(thisPtr, new IntPtr[] { new IL2String(prefabpath).ptr });
        }

        public static void PushChildViewController(IntPtr thisPtr, string prefabpath)
        {
            PushChildViewControllerHook(thisPtr, new IL2String(prefabpath).ptr);
        }

        private static void PushChildViewControllerHook(IntPtr thisPtr, IntPtr prefabpathPtr)
        {
            if (prefabpathPtr != IntPtr.Zero)
            {
                string prefabpath = new IL2String(prefabpathPtr).ToString();
                if (!Program.IsLive && prefabpath == "Colosseum/Colosseum")
                {
                    if (string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
                    {
                        // Redirect the home screen "DUEL" button to RoomCreate
                        YgomGame.Room.RoomCreateViewController.IsNextInstanceHacked = true;
                        prefabpathPtr = new IL2String("Room/RoomCreate").ptr;
                    }
                    else
                    {
                        YgomGame.Menu.CommonDialogViewController.OpenYesNoConfirmationDialog(
                            ClientSettings.CustomTextDuelStarterPveOrPvpTitle,
                            ClientSettings.CustomTextDuelStarterPveOrPvpText,
                            OpenDuelStarterMenu, OpenRoomMenu, null,
                            ClientSettings.CustomTextDuelStarterPveOrPvpTextBtnPvE,
                            ClientSettings.CustomTextDuelStarterPveOrPvpTextBtnPvP);
                        return;
                    }
                }
            }
            hookPushChildViewController.Original.Invoke(thisPtr, prefabpathPtr);
        }

        public static void PushChildViewController(IntPtr thisPtr, string prefabpath, Dictionary<string, object> args)
        {
            methodPushChildViewControllerWithArgs.Invoke(thisPtr, new IntPtr[]
            {
                new IL2String(prefabpath).ptr, YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(args))
            });
        }

        public static void PushChildViewController(IntPtr thisPtr, IntPtr prefab)
        {
            methodPushChildViewControllerObj.Invoke(thisPtr, new IntPtr[] { prefab });
        }

        public static void PopChildViewController(IntPtr thisPtr)
        {
            methodPopChildViewController.Invoke(thisPtr);
        }

        public static void PopChildViewController(IntPtr thisPtr, IntPtr popTarget)
        {
            hookPopChildViewController2.Original(thisPtr, popTarget);
            YgomGame.Room.RoomCreateViewController.OnPopChildViewController(thisPtr);
            YgomGame.Menu.ProfileReplayViewController.OnPopChildViewController(thisPtr, popTarget);
        }

        public static IntPtr LoadViewControllerPrefab(IntPtr thisPtr, string prefabpath)
        {
            IL2Object result = methodLoadViewControllerPrefab.Invoke(thisPtr, new IntPtr[] { new IL2String(prefabpath).ptr });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr GetViewController(IntPtr thisPtr, IntPtr type)
        {
            IL2Method method;
            if (!methodGetViewControllerTInstances.TryGetValue(type, out method))
            {
                method = methodGetViewControllerT.MakeGenericMethod(new IntPtr[] { type });
                methodGetViewControllerTInstances[type] = method;
            }
            IL2Object result = method.Invoke(thisPtr);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr GetStackTopViewController(IntPtr thisPtr)
        {
            IL2Object result = methodGetStackTopViewController.Invoke(thisPtr);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        static Action OpenRoomMenu = () =>
        {
            IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
            if (manager != IntPtr.Zero)
            {
                int roomId = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Room.room_info.room_id");
                csbool isMember = YgomSystem.Utility.ClientWork.GetByJsonPath<csbool>("Room.room_info.is_join_player");
                if (roomId != 0)
                {
                    PushChildViewController(manager, "Room/Room", new Dictionary<string, object>()
                    {
                        { "Mode", isMember ? YgomGame.Room.RoomEntryViewController.Mode.NORMAL : YgomGame.Room.RoomEntryViewController.Mode.SPECTER }
                    });
                }
                else
                {
                    YgomGame.Menu.ActionSheetViewController.Open(YgomSystem.Utility.TextData.GetText("IDS_ROOM.ROOM_MATCH"),
                        new string[]
                        {
                            YgomSystem.Utility.TextData.GetText("IDS_ROOM.ROOM_CREATE"),
                            YgomSystem.Utility.TextData.GetText("IDS_ROOM.ROOM_ENTRY"),
                            YgomSystem.Utility.TextData.GetText("IDS_ROOM.SPECTATE")
                        },
                        OnClickRoomMatchMenuItem);
                }
            }
        };

        static Action<IntPtr, int> OnClickRoomMatchMenuItem = OnClickRoomMatchMenuItemImpl;
        static void OnClickRoomMatchMenuItemImpl(IntPtr ctx, int index)
        {
            IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
            if (manager != IntPtr.Zero)
            {
                switch (index)
                {
                    case 0:
                        PushChildViewController(manager, "Room/RoomCreate");
                        break;
                    case 1:
                        PushChildViewController(manager, "Room/RoomEntry", new Dictionary<string, object>()
                        {
                            { "Mode", YgomGame.Room.RoomEntryViewController.Mode.NORMAL }
                        });
                        break;
                    case 2:
                        PushChildViewController(manager, "Room/RoomEntry", new Dictionary<string, object>()
                        {
                            { "Mode", YgomGame.Room.RoomEntryViewController.Mode.SPECTER }
                        });
                        break;
                }
            }
        }

        static Action OpenDuelStarterMenu = () =>
        {
            IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
            if (manager != IntPtr.Zero)
            {
                YgomGame.Room.RoomCreateViewController.IsNextInstanceHacked = true;
                PushChildViewController(manager, "Room/RoomCreate");
            }
        };
    }

    static unsafe class BindingTextMeshProUGUI
    {
        static IL2Method methodGetTextId;
        static IL2Method methodSetTextId;

        static BindingTextMeshProUGUI()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("BindingTextMeshProUGUI", "YgomSystem.UI");
            IL2Class baseClass = classInfo.BaseType;
            methodGetTextId = baseClass.GetProperty("TextId").GetGetMethod();
            methodSetTextId = baseClass.GetProperty("TextId").GetSetMethod();
        }

        public static string GetTextId(IntPtr thisPtr)
        {
            IL2Object result = methodGetTextId.Invoke(thisPtr);
            return result != null ? result.GetValueObj<string>() : null;
        }

        public static void SetTextId(IntPtr thisPtr, string value)
        {
            methodSetTextId.Invoke(thisPtr, new IntPtr[] { new IL2String(YgomSystem.Utility.TextData.HackID + value).ptr });
        }
    }
}

namespace YgomGame.Room
{
    unsafe static class RoomEntryViewController
    {
        public enum Mode
        {
            NORMAL,
            SPECTER
        }
    }
}

namespace YgomSystem.UI.InfinityScroll
{
    unsafe static class InfinityScrollView
    {
        delegate void Del_UpdateData(IntPtr thisPtr);
        static Hook<Del_UpdateData> hookUpdateData;

        static InfinityScrollView()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("InfinityScrollView", "YgomSystem.UI.InfinityScroll");
            hookUpdateData = new Hook<Del_UpdateData>(UpdateData, classInfo.GetMethod("UpdateData"));
        }

        static bool overflowGuard;
        public static void UpdateData(IntPtr thisPtr)
        {
            if (!overflowGuard)
            {
                overflowGuard = true;
                YgomGame.Room.RoomCreateViewController.OnUpdateData(thisPtr);
                hookUpdateData.Original.Invoke(thisPtr);
                overflowGuard = false;
            }
            else
            {
                hookUpdateData.Original.Invoke(thisPtr);
            }
        }
    }
}

namespace UnityEngine
{
    unsafe static class UnityObject
    {
        static IL2Method methodGetName;
        static IL2Method methodSetName;
        static IL2Method methodInstantiate;
        static IL2Method methodInstantiate2;
        static IL2Method methodDestroy;
        static IL2Method methodDontDestroyOnLoad;

        static UnityObject()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("Object");
            IL2Property propertyName = classInfo.GetProperty("name");
            methodGetName = propertyName.GetGetMethod();
            methodSetName = propertyName.GetSetMethod();
            methodInstantiate = classInfo.GetMethod("Instantiate", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Type.Name == classInfo.FullName);
            methodInstantiate2 = classInfo.GetMethod("Instantiate", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Type.Name == classInfo.FullName);
            methodDestroy = classInfo.GetMethod("Destroy");
            methodDontDestroyOnLoad = classInfo.GetMethod("DontDestroyOnLoad");
        }

        public static string GetName(IntPtr thisPtr)
        {
            IL2Object result = methodGetName.Invoke(thisPtr);
            return result != null ? result.GetValueObj<string>() : null;
        }

        public static void SetName(IntPtr thisPtr, string name)
        {
            methodSetName.Invoke(thisPtr, new IntPtr[] { new IL2String(name).ptr });
        }

        public static IntPtr Instantiate(IntPtr original)
        {
            IL2Object result = methodInstantiate.Invoke(new IntPtr[] { original });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr Instantiate(IntPtr original, IntPtr parentTransform)
        {
            IL2Object result = methodInstantiate2.Invoke(new IntPtr[] { original, parentTransform });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static void Destroy(IntPtr obj, float t = 0)
        {
            methodDestroy.Invoke(new IntPtr[] { obj, new IntPtr(&t) });
        }

        public static void DontDestroyOnLoad(IntPtr obj)
        {
            methodDontDestroyOnLoad.Invoke(new IntPtr[] { obj });
        }
    }

    unsafe static class GameObject
    {
        static IL2Class classInfo;
        static IL2Method methodCtor;
        static IL2Method methodFind;
        static IL2Method methodGetTransform;
        static IL2Method methodGetComponentsTObject;
        static IL2Method methodGetComponent;
        static IL2Method methodAddComponent;
        static IL2Method methodGetActiveSelf;
        static IL2Method methodSetActive;

        static GameObject()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            classInfo = assembly.GetClass("GameObject");
            methodCtor = classInfo.GetMethod(".ctor", x => x.GetParameters().Length == 0);
            methodFind = classInfo.GetMethod("Find");
            methodGetTransform = classInfo.GetProperty("transform").GetGetMethod();
            methodGetComponentsTObject = classInfo.GetMethod("GetComponents", x => x.GetParameters().Length == 0).MakeGenericMethod(
                new Type[] { typeof(object) });
            methodGetComponent = classInfo.GetMethod("GetComponent", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Type.Name == typeof(Type).FullName);
            methodAddComponent = classInfo.GetMethod("AddComponent", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Type.Name == typeof(Type).FullName);
            methodGetActiveSelf = classInfo.GetProperty("activeSelf").GetGetMethod();
            methodSetActive = classInfo.GetMethod("SetActive");
        }

        public static IntPtr New()
        {
            IntPtr result = Import.Object.il2cpp_object_new(classInfo.ptr);
            methodCtor.Invoke(result);
            return result;
        }

        public static IntPtr Find(string name)
        {
            IL2Object result = methodFind.Invoke(new IntPtr[] { new IL2String(name).ptr });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr GetTransform(IntPtr thisPtr)
        {
            IL2Object result = methodGetTransform.Invoke(thisPtr);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr[] GetComponents(IntPtr thisPtr)
        {
            IL2Object result = methodGetComponentsTObject.Invoke(thisPtr, new IntPtr[] { methodGetComponentsTObject.ptr });
            if (result != null)
            {
                IL2Array<IntPtr> array = new IL2Array<IntPtr>(result.ptr);
                return array.ToArray();
            }
            return null;
        }

        public static IntPtr[] GetComponents(IntPtr thisPtr, IntPtr klass)
        {
            List<IntPtr> components = new List<IntPtr>();
            IL2Object result = methodGetComponentsTObject.Invoke(thisPtr, new IntPtr[] { methodGetComponentsTObject.ptr });
            if (result != null)
            {
                IL2Array<IntPtr> array = new IL2Array<IntPtr>(result.ptr);
                int len = array.Length;
                for (int i = 0; i < len; i++)
                {
                    IntPtr obj = array[i];
                    IntPtr objKlass = Import.Object.il2cpp_object_get_class(obj);
                    if (objKlass == klass || Import.Class.il2cpp_class_is_subclass_of(objKlass, klass, false))
                    {
                        components.Add(obj);
                    }
                }
            }
            return components.ToArray();
        }

        public static IntPtr GetComponent(IntPtr thisPtr, IntPtr type)
        {
            IL2Object result = methodGetComponent.Invoke(thisPtr, new IntPtr[] { type });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr AddComponent(IntPtr thisPtr, IntPtr type)
        {
            IL2Object result = methodAddComponent.Invoke(thisPtr, new IntPtr[] { type });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr FindGameObjectByName(IntPtr thisPtr, string name, bool reverseSearch = false, bool recursive = true, bool excludeSelf = false)
        {
            if (!excludeSelf)
            {
                string thisName = UnityObject.GetName(thisPtr);
                if (thisName == name)
                {
                    return thisPtr;
                }
            }
            IntPtr transform = UnityEngine.GameObject.GetTransform(thisPtr);
            int childCount = UnityEngine.Transform.GetChildCount(transform);
            int start = reverseSearch ? childCount - 1 : 0;
            int end = reverseSearch ? -1 : childCount;
            int increment = reverseSearch ? -1 : 1;
            for (int i = start; i != end; i += increment)
            {
                IntPtr childTransform = UnityEngine.Transform.GetChild(transform, i);
                IntPtr childObject = UnityEngine.Component.GetGameObject(childTransform);
                if (recursive)
                {
                    IntPtr result = FindGameObjectByName(childObject, name);
                    if (result != IntPtr.Zero)
                    {
                        return result;
                    }
                }
                else
                {
                    string childName = UnityObject.GetName(childTransform);
                    if (childName == name)
                    {
                        return childObject;
                    }
                }
            }
            return IntPtr.Zero;
        }

        public static IntPtr FindGameObjectByPath(IntPtr thisPtr, string path)
        {
            string[] splitted = path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            IntPtr obj = thisPtr;
            if (splitted.Length > 0)
            {
                for (int i = 0; i < splitted.Length; i++)
                {
                    string objName = splitted[i];
                    obj = FindGameObjectByName(obj, objName, false, false, true);
                    if (obj == IntPtr.Zero)
                    {
                        break;
                    }
                }
            }
            else
            {
                obj = IntPtr.Zero;
            }
            return obj;
        }

        public static void DestroyChildObjects(IntPtr thisPtr)
        {
            IntPtr transform = UnityEngine.GameObject.GetTransform(thisPtr);
            int childCount = UnityEngine.Transform.GetChildCount(transform);
            for (int i = childCount - 1; i >= 0; i--)
            {
                IntPtr childTransform = UnityEngine.Transform.GetChild(transform, i);
                IntPtr childObject = UnityEngine.Component.GetGameObject(childTransform);
                UnityEngine.UnityObject.Destroy(childObject);
            }
        }

        public static List<IntPtr> GetChildren(IntPtr thisPtr)
        {
            List<IntPtr> result = new List<IntPtr>();
            IntPtr transform = GameObject.GetTransform(thisPtr);
            int childCount = Transform.GetChildCount(transform);
            for (int i = 0; i < childCount; i++)
            {
                IntPtr childTransform = Transform.GetChild(transform, i);
                IntPtr childObject = Component.GetGameObject(childTransform);
                result.Add(childObject);
            }
            return result;
        }

        public static IntPtr GetParentObject(IntPtr thisPtr)
        {
            IntPtr transform = GameObject.GetTransform(thisPtr);
            if (transform == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            IntPtr parent = Transform.GetParent(transform);
            if (parent == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            return Component.GetGameObject(parent);
        }

        public static IntPtr GetRootObject(IntPtr thisPtr)
        {
            IntPtr transform = GameObject.GetTransform(thisPtr);
            while (true)
            {
                IntPtr parent = Transform.GetParent(transform);
                if (parent == IntPtr.Zero)
                {
                    break;
                }
                transform = parent;
            }
            return Component.GetGameObject(transform);
        }

        public static string DumpFromRoot(IntPtr thisPtr)
        {
            IntPtr rootTransform = GameObject.GetTransform(GetRootObject(thisPtr));
            Dictionary<string, object> result = Transform.ToDictionary(rootTransform);
            return MiniJSON.Json.Format(MiniJSON.Json.Serialize(result));
        }

        public static string Dump(IntPtr thisPtr)
        {
            IntPtr transform = GameObject.GetTransform(thisPtr);
            Dictionary<string, object> result = Transform.ToDictionary(transform);
            return MiniJSON.Json.Format(MiniJSON.Json.Serialize(result));
        }

        public static bool IsActive(IntPtr thisPtr)
        {
            IL2Object result = methodGetActiveSelf.Invoke(thisPtr);
            return result != null && result.GetValueRef<bool>();
        }

        public static void SetActive(IntPtr thisPtr, bool value)
        {
            methodSetActive.Invoke(thisPtr, new IntPtr[] { new IntPtr(&value) });
        }
    }

    unsafe static class Transform
    {
        static IL2Method methodGetChildCount;
        static IL2Method methodGetChild;
        static IL2Method methodGetParent;
        static IL2Method methodSetParent;
        static IL2Method methodGetPosition;
        static IL2Method methodSetPosition;
        static IL2Method methodGetLocalPosition;
        static IL2Method methodSetLocalPosition;
        static IL2Method methodGetLocalScale;
        static IL2Method methodSetLocalScale;
        static IL2Method methodGetSiblingIndex;
        static IL2Method methodSetSiblingIndex;
        static IL2Method methodSetAsLastSibling;

        static IL2Class serializedFieldClassInfo;

        static Transform()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("Transform");
            methodGetChildCount = classInfo.GetProperty("childCount").GetGetMethod();
            methodGetChild = classInfo.GetMethod("GetChild");
            methodGetParent = classInfo.GetProperty("parent").GetGetMethod();
            methodSetParent = classInfo.GetProperty("parent").GetSetMethod();
            methodGetPosition = classInfo.GetProperty("position").GetGetMethod();
            methodSetPosition = classInfo.GetProperty("position").GetSetMethod();
            methodGetLocalPosition = classInfo.GetProperty("localPosition").GetGetMethod();
            methodSetLocalPosition = classInfo.GetProperty("localPosition").GetSetMethod();
            methodGetLocalScale = classInfo.GetProperty("localScale").GetGetMethod();
            methodSetLocalScale = classInfo.GetProperty("localScale").GetSetMethod();
            methodGetSiblingIndex = classInfo.GetMethod("GetSiblingIndex");
            methodSetSiblingIndex = classInfo.GetMethod("SetSiblingIndex");
            methodSetAsLastSibling = classInfo.GetMethod("SetAsLastSibling");

            serializedFieldClassInfo = Assembler.GetAssembly("UnityEngine.CoreModule").GetClass("SerializeField");
        }

        public static int GetChildCount(IntPtr thisPtr)
        {
            return methodGetChildCount.Invoke(thisPtr).GetValueRef<int>();
        }

        public static IntPtr GetChild(IntPtr thisPtr, int index)
        {
            IL2Object result = methodGetChild.Invoke(thisPtr, new IntPtr[] { new IntPtr(&index) });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr GetParent(IntPtr thisPtr)
        {
            IL2Object result = methodGetParent.Invoke(thisPtr);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static void SetParent(IntPtr thisPtr, IntPtr newParent)
        {
            methodSetParent.Invoke(thisPtr, new IntPtr[] { newParent });
        }

        public static Vector3 GetPosition(IntPtr thisPtr)
        {
            IL2Object result = methodGetPosition.Invoke(thisPtr);
            return result != null ? result.GetValueRef<Vector3>() : default(Vector3);
        }

        public static void SetPosition(IntPtr thisPtr, Vector3 position)
        {
            methodSetPosition.Invoke(thisPtr, new IntPtr[] { new IntPtr(&position) });
        }

        public static Vector3 GetLocalPosition(IntPtr thisPtr)
        {
            IL2Object result = methodGetLocalPosition.Invoke(thisPtr);
            return result != null ? result.GetValueRef<Vector3>() : default(Vector3);
        }

        public static void SetLocalPosition(IntPtr thisPtr, Vector3 position)
        {
            methodSetLocalPosition.Invoke(thisPtr, new IntPtr[] { new IntPtr(&position) });
        }

        public static Vector3 GetLocalScale(IntPtr thisPtr)
        {
            IL2Object result = methodGetLocalScale.Invoke(thisPtr);
            return result != null ? result.GetValueRef<Vector3>() : default(Vector3);
        }

        public static void SetLocalScale(IntPtr thisPtr, Vector3 scale)
        {
            methodSetLocalScale.Invoke(thisPtr, new IntPtr[] { new IntPtr(&scale) });
        }

        public static int GetSiblingIndex(IntPtr thisPtr)
        {
            return methodGetSiblingIndex.Invoke(thisPtr).GetValueRef<int>();
        }

        public static void SetSiblingIndex(IntPtr thisPtr, int index)
        {
            methodSetSiblingIndex.Invoke(thisPtr, new IntPtr[] { new IntPtr(&index) });
        }

        public static void SetAsLastSibling(IntPtr thisPtr)
        {
            methodSetAsLastSibling.Invoke(thisPtr);
        }

        public static void Dump(IntPtr transform, int depth = 0)
        {
            IntPtr obj = Component.GetGameObject(transform);
            Console.WriteLine(string.Empty.PadLeft(depth, ' ') + "name:" + UnityObject.GetName(obj));

            IntPtr[] components = GameObject.GetComponents(obj);
            Console.WriteLine(string.Empty.PadLeft(depth, ' ') + "components: " + (components == null ? 0 : components.Length));
            if (components != null)
            {
                foreach (IntPtr component in components)
                {
                    IntPtr type = Import.Object.il2cpp_object_get_class(component);
                    string typeName = Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_name(type));
                    Console.WriteLine(string.Empty.PadLeft(depth, ' ') + "|" + UnityObject.GetName(component) + " type:" + typeName);
                }
            }

            int childCount = UnityEngine.Transform.GetChildCount(transform);
            for (int i = 0; i < childCount; i++)
            {
                IntPtr childTransform = UnityEngine.Transform.GetChild(transform, i);
                Dump(childTransform, depth + 1);
            }
        }

        public static Dictionary<string, object> ToDictionary(IntPtr transform)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<object> componentsList = new List<object>();
            List<object> childrenList = new List<object>();
            IntPtr obj = Component.GetGameObject(transform);
            result["name"] = UnityObject.GetName(obj);
            result["components"] = componentsList;
            result["children"] = childrenList;
            IntPtr[] components = GameObject.GetComponents(obj);
            if (components != null)
            {
                foreach (IntPtr component in components)
                {
                    Dictionary<string, object> componentData = new Dictionary<string, object>();
                    Dictionary<string, object> componentMembers = new Dictionary<string, object>();
                    IL2Class componentClassInfo = new IL2Class(Import.Object.il2cpp_object_get_class(component));
                    componentData["type"] = componentClassInfo.Name;
                    componentData["members"] = componentMembers;
                    componentsList.Add(componentData);
                    IL2Class classInHierarchy = componentClassInfo;
                    while (classInHierarchy != null)
                    {
                        foreach (IL2Field field in classInHierarchy.GetFields())
                        {
                            if (field.HasAttribute(serializedFieldClassInfo))
                            {
                                switch (field.ReturnType.Name)
                                {
                                    case "System.Byte": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Byte>(); break;
                                    case "System.SByte": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.SByte>(); break;
                                    case "System.Int16": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Int16>(); break;
                                    case "System.UInt16": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.UInt16>(); break;
                                    case "System.Int32": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Int32>(); break;
                                    case "System.UInt32": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.UInt32>(); break;
                                    case "System.Int64": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Int64>(); break;
                                    case "System.UInt64": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.UInt64>(); break;
                                    case "System.String":
                                        {
                                            IL2Object stringObj = field.GetValue(component);
                                            componentMembers[field.Name] = stringObj == null ? null : stringObj.GetValueObj<string>();
                                        }
                                        break;
                                    case "System.Single": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Single>(); break;
                                    case "System.Double": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Double>(); break;
                                    case "System.Boolean": componentMembers[field.Name] = field.GetValue(component).GetValueRef<System.Boolean>(); break;
                                    default:
                                        componentMembers[field.Name] = new Dictionary<string, object>()
                                        {
                                            { "type", field.ReturnType.Name },
                                        };
                                        break;
                                }
                            }
                        }
                        classInHierarchy = classInHierarchy.BaseType;
                    }
                }
            }
            int childCount = UnityEngine.Transform.GetChildCount(transform);
            for (int i = 0; i < childCount; i++)
            {
                IntPtr childTransform = UnityEngine.Transform.GetChild(transform, i);
                childrenList.Add(ToDictionary(childTransform));
            }
            return result;
        }
    }

    unsafe static class Component
    {
        static IL2Method methodGetGameObject;

        static Component()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("Component");
            methodGetGameObject = classInfo.GetProperty("gameObject").GetGetMethod();
        }

        public static IntPtr GetGameObject(IntPtr thisPtr)
        {
            IL2Object result = methodGetGameObject.Invoke(thisPtr);
            return result != null ? result.ptr : IntPtr.Zero;
        }
    }

    struct Vector3
    {
        public float x;
        public float y;
        public float z;
        
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}