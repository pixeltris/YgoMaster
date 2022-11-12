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
using System.Reflection;

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

        static SoloStartProductionViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("SoloStartProductionViewController", "YgomGame.Solo");
            fieldStep = classInfo.GetField("step");
            finalStepValue = classInfo.GetNestedType("Step").GetField("Final").GetValue().GetValueRef<int>();
            methodDispFirstorSecond = classInfo.GetMethod("DispFirstorSecond");
            hookSelectTurn = new Hook<Del_SelectTurn>(SelectTurn, classInfo.GetMethod("SelectTurn"));
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
                        DuelStarterFirstPlayer = Program.Rand.Next(2);
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
            hookSelectTurn.Original(thisPtr);
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
        delegate IntPtr Del_Duel_begin(IntPtr rulePtr);
        static Hook<Del_Duel_begin> hookDuel_begin;
        delegate IntPtr Del_Duel_end(IntPtr paramPtr);
        static Hook<Del_Duel_end> hookDuel_end;

        static API()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("API", "YgomSystem.Network");
            hookDuel_begin = new Hook<Del_Duel_begin>(Duel_begin, classInfo.GetMethod("Duel_begin"));
            if (ClientSettings.AlwaysWin)
            {
                hookDuel_end = new Hook<Del_Duel_end>(Duel_end, classInfo.GetMethod("Duel_end"));
            }
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
            param["res"] = 1;
            param["finish"] = 1;
            paramPtr = YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(param));
            return hookDuel_end.Original(paramPtr);
        }
    }

    static unsafe class RequestStructure
    {
        static IL2Method methodGetCommand;

        delegate void Del_Complete(IntPtr thisPtr);
        static Hook<Del_Complete> hookComplete;

        static RequestStructure()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("NetworkMain").GetNestedType("RequestStructure");
            methodGetCommand = classInfo.GetProperty("Command").GetGetMethod();
            hookComplete = new Hook<Del_Complete>(Complete, classInfo.GetMethod("Complete"));
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
                if (settings.icon[0] <= 0)
                {
                    settings.icon[0] = Utils.GetValue<int>(userProfile, "icon_id");
                }
                if (settings.icon_frame[0] <= 0)
                {
                    settings.icon_frame[0] = Utils.GetValue<int>(userProfile, "icon_frame_id");
                }
                if (settings.avatar[0] <= 0)
                {
                    settings.avatar[0] = Utils.GetValue<int>(userProfile, "avatar_id");
                }
            }
            if (settings.RandSeed == 0)
            {
                settings.RandSeed = (uint)Program.Rand.Next();
            }
            if (settings.bgms.Count == 0)
            {
                settings.SetRandomBgm(Program.Rand);
            }
            settings.SetRequiredDefaults();
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
                        PInvoke.SetTimeMultiplier(ClientSettings.DuelClientTimeMultiplier != 0 ?
                            ClientSettings.DuelClientTimeMultiplier : ClientSettings.TimeMultiplier);
                        break;
                    case "Duel.end":
                        PInvoke.SetTimeMultiplier(ClientSettings.TimeMultiplier);
                        break;
                }
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
                                    settings.FirstPlayer = YgomGame.Solo.SoloStartProductionViewController.DuelStarterFirstPlayer;
                                    SetDuelRequiredDefaults(settings);
                                    YgomSystem.Utility.ClientWork.DeleteByJsonPath("Duel");
                                    YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(new Dictionary<string, object>()
                                    {
                                        { "Duel", settings.ToDictionary() }
                                    }));
                                }
                            }
                            break;
                        case "Solo.start":
                            {
                                DuelSettings settings = new DuelSettings();
                                settings.CopyFrom(YgomGame.Room.RoomCreateViewController.Settings);
                                SetDuelRequiredDefaults(settings);
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
            hookComplete.Original(thisPtr);
        }
    }

    static unsafe class Request
    {
        static IL2Method methodEntry;

        static Request()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class requestClassInfo = assembly.GetClass("Request", "YgomSystem.Network");
            methodEntry = requestClassInfo.GetMethod("Entry");
        }

        public static IntPtr Entry(string command, string param, float timeout = 30)
        {
            IL2Object result = methodEntry.Invoke(new IntPtr[] { new IL2String(command).ptr, YgomMiniJSON.Json.Deserialize(param), new IntPtr(&timeout) });
            return result != null ? result.ptr : IntPtr.Zero;
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
                                DuellDll.DLL_DuelComCheatCard(cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4], cmdItems[5], cmdItems[6]);
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComCheatCard {0} {1} {2} {3} {4} {5}", cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4], cmdItems[5], cmdItems[6]);
                                }
                            }
                            break;
                        case 1:
                            if (cmdItems.Length >= 5)
                            {
                                DuellDll.DLL_DuelComDoDebugCommand(cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComDoDebugCommand {0} {1} {2} {3}", cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                }
                            }
                            break;
                        case 2:
                            if (cmdItems.Length >= 1)
                            {
                                DuellDll.DLL_DuelComDebugCommand(); Console.WriteLine("DLL_DuelComDebugCommand");
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComDebugCommand");
                                }
                            }
                            break;
                        case 3:
                            if (cmdItems.Length >= 5)
                            {
                                DuellDll.DLL_DuelComDoCommand(cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                if (ClientSettings.CustomDuelCmdLog)
                                {
                                    Console.WriteLine("DLL_DuelComDoCommand {0} {1} {2} {3}", cmdItems[1], cmdItems[2], cmdItems[3], cmdItems[4]);
                                }
                            }
                            break;
                        case 4:
                            if (cmdItems.Length >= 1)
                            {
                                DuellDll.DLL_DuelSysAct();
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
        
        static IL2Class buttonInfoClass;
        static IL2Method buttonInfoCtor;
        static IL2Field buttonTitle;
        static IL2Field buttonCurrentSetting;
        static IL2Field buttonSettingStrings;

        static IL2Class stringClass;
        static IL2Class intClass;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;
        delegate void Del_SetData(IntPtr thisPtr);
        static Hook<Del_SetData> hookSetData;
        delegate void Del_CallAPIRoomCreate(IntPtr thisPtr);
        static Hook<Del_CallAPIRoomCreate> hookCallAPIRoomCreate;
        delegate void Del_OnClick(IntPtr thisPtr);
        static Hook<Del_OnClick> hookOnClick;

        static IntPtr activeViewController;
        static IntPtr activeButton;
        static DuelSettingsManager duelSettingsManager = new DuelSettingsManager();

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
            get { return duelSettingsManager != null ? duelSettingsManager.Settings : null; }
        }

        static RoomCreateViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            ClassInfo = assembly.GetClass("RoomCreateViewController", "YgomGame.Room");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, ClassInfo.GetMethod("OnCreatedView"));
            hookSetData = new Hook<Del_SetData>(SetData, ClassInfo.GetMethod("SetData"));
            hookCallAPIRoomCreate = new Hook<Del_CallAPIRoomCreate>(CallAPIRoomCreate, ClassInfo.GetMethod("CallAPIRoomCreate"));
            fieldInfos = ClassInfo.GetField("infos");
            fieldIsv = ClassInfo.GetField("isv");
            templateInfoClass = ClassInfo.GetNestedType("TemplateInfo");
            labelInfoClass = ClassInfo.GetNestedType("LabelInfo");
            labelInfoCtor = labelInfoClass.GetMethod(".ctor");
            buttonInfoClass = ClassInfo.GetNestedType("ButtonInfo");
            buttonInfoCtor = buttonInfoClass.GetMethod(".ctor");
            hookOnClick = new Hook<Del_OnClick>(OnClick, buttonInfoClass.GetMethod("ProcessOnClicked"));//"OnClick"));//v1.2.0 needed to change to ProcessOnClicked (OnClick RVA was used multiple times)
            buttonTitle = buttonInfoClass.GetField("title");
            buttonCurrentSetting = buttonInfoClass.GetField("currentSetting");
            buttonSettingStrings = buttonInfoClass.GetField("settingStrings");
            bindingTextType = CastUtils.IL2Typeof("BindingTextMeshProUGUI", "YgomSystem.UI", "Assembly-CSharp");

            stringClass = Assembler.GetAssembly("mscorlib").GetClass("String", "System");
            intClass = Assembler.GetAssembly("mscorlib").GetClass("Int32", "System");
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            if (!IsNextInstanceHacked)
            {
                activeViewController = IntPtr.Zero;
                return;
            }
            IsNextInstanceHacked = false;
            activeViewController = thisPtr;

            hookOnCreatedView.Original.Invoke(thisPtr);
            
            // Modify the title of the view
            IntPtr titleObj = UnityEngine.GameObject.FindGameObjectByName(UnityEngine.Component.GetGameObject(thisPtr), "NameText");
            IntPtr titleComponent = UnityEngine.GameObject.GetComponent(titleObj, bindingTextType);
            YgomSystem.UI.BindingTextMeshProUGUI.SetTextId(titleComponent, duelSettingsManager.Title);

            // Modify the text of the button on the bottom right (v1.2.0 changed from "OKButton" to "ButtonOK")
            IntPtr duelStartButtonObj = UnityEngine.GameObject.FindGameObjectByName(UnityEngine.Component.GetGameObject(thisPtr), "ButtonOK");
            IntPtr duelStartButtonTextObj = UnityEngine.GameObject.FindGameObjectByName(duelStartButtonObj, "TextTMP");
            IntPtr duelStartButtonTextComponent = UnityEngine.GameObject.GetComponent(duelStartButtonTextObj, bindingTextType);
            YgomSystem.UI.BindingTextMeshProUGUI.SetTextId(duelStartButtonTextComponent, duelSettingsManager.ButtonText);
        }

        public static void NotificationStackRemove(IntPtr thisPtr)
        {
            if (activeViewController == thisPtr)
            {
                duelSettingsManager.DuelSettingsFromUI();
                activeViewController = IntPtr.Zero;
            }
        }

        static void SetData(IntPtr thisPtr)
        {
            if (IsHacked)
            {
                duelSettingsManager.InitButtons(thisPtr);
            }
            else
            {
                hookSetData.Original(thisPtr);
            }
        }

        static void CallAPIRoomCreate(IntPtr thisPtr)
        {
            if (IsHacked)
            {
                // Update the duel settings
                duelSettingsManager.DuelSettingsFromUI();

                YgomSystem.Network.Request.Entry("Solo.start", "{\"chapter\":" + ClientSettings.DuelStarterLiveChapterId + "}");
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

        static IntPtr AddButton(IL2ListExplicit infosList, IntPtr isv, string title, string[] settings, bool isActionSheet = true, int defaultSetting = 0)
        {
            IL2Array<IntPtr> stringsArray = new IL2Array<IntPtr>(settings != null ? settings.Length : 0, stringClass);
            IL2Array<int> intArray = new IL2Array<int>(settings != null ? settings.Length : 0, intClass);
            for (int i = 0; i < settings.Length; i++)
            {
                stringsArray[i] = new IL2String(settings[i]).ptr;
                intArray[i] = i;
            }
            IntPtr newButton = Import.Object.il2cpp_object_new(buttonInfoClass.ptr);
            buttonInfoCtor.Invoke(newButton, new IntPtr[]
            {
                isv, new IL2String(string.Empty).ptr, new IL2String(title).ptr, stringsArray.ptr, intArray.ptr,
                new IntPtr(&isActionSheet), new IntPtr(&defaultSetting)
            });
            infosList.Add(newButton);
            return newButton;
        }

        static IntPtr AddButtonOnOff(IL2ListExplicit infosList, IntPtr isv, string title, bool isOn)
        {
            return AddButton(infosList, isv, title, new string[] { "On", "Off" }, false, isOn ? 0 : 1);
        }

        static IntPtr AddButtonYesNo(IL2ListExplicit infosList, IntPtr isv, string title, bool isYes)
        {
            return AddButton(infosList, isv, title, new string[] { "Yes", "No" }, false, isYes ? 0 : 1);
        }

        static IntPtr AddButtonTrueFalse(IL2ListExplicit infosList, IntPtr isv, string title, bool isTrue)
        {
            return AddButton(infosList, isv, title, new string[] { "True", "False" }, false, isTrue ? 0 : 1);
        }

        static IntPtr AddButton(IL2ListExplicit infosList, IntPtr isv, string title)
        {
            return AddButton(infosList, isv, title, new string[] { string.Empty, string.Empty }, false);
        }

        static void OnClick(IntPtr buttonPtr)
        {
            if (IsHacked)
            {
                activeButton = buttonPtr;
                if (duelSettingsManager.HandleButtonClick(buttonPtr))
                {
                    return;
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
                public IntPtr LoadFromFile; public IntPtr ClearAllDecks; public IntPtr OpenDeckEditor;
                public IntPtr LoadIncludingDecks; public IntPtr Load; public IntPtr Save;
                public IntPtr StartingPlayer; public IntPtr LifePoints; public IntPtr Hand; public IntPtr Field;
                public IntPtr DuelType; public IntPtr Seed; public IntPtr Shuffle; public IntPtr Cpu;
                public IntPtr CpuFlag; public IntPtr Limit; public IntPtr LP1; public IntPtr LP2; public IntPtr Hand1; public IntPtr Hand2;
                public IntPtr Sleeve1; public IntPtr Sleeve2; public IntPtr Field1; public IntPtr Field2; public IntPtr FieldPart1;
                public IntPtr FieldPart2; public IntPtr Mate1; public IntPtr Mate2; public IntPtr MateBase1; public IntPtr MateBase2;
                public IntPtr Icon1; public IntPtr Icon2; public IntPtr IconFrame1; public IntPtr IconFrame2;
                public IntPtr Player1; /*public IntPtr Player2; public IntPtr Player3; public IntPtr Player4;*/
                public IntPtr BGM;
            }

            DuelSettings settings;
            Buttons buttons;

            static int bgmMax = 0;

            public string Title
            {
                get { return "Duel Starter"; }
            }
            public string ButtonText
            {
                get { return "Start Duel"; }
            }
            public DuelSettings Settings
            {
                get { return settings; }
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

            Dictionary<int, string> GetItemNames(ItemID.Category category, string defaultName = "Default")
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                if (!string.IsNullOrEmpty(defaultName))
                {
                    result[-1] = defaultName;
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
                IL2Array<IntPtr> strings = new IL2Array<IntPtr>(buttonSettingStrings.GetValue(buttonPtr).ptr);
                int settingIndex = buttonCurrentSetting.GetValue(buttonPtr).GetValueRef<int>();
                if (settingIndex < strings.Length)
                {
                    return new IL2String(strings[settingIndex]).ToString();
                }
                return null;
            }

            int GetButtonIndex(IntPtr buttonPtr)
            {
                return buttonCurrentSetting.GetValue(buttonPtr).GetValueRef<int>();
            }

            int GetButtonValueI32(IntPtr buttonPtr, int defaultValue = -1)
            {
                string valueStr = GetButtonValueString(buttonPtr);
                switch (valueStr)
                {
                    case "Default":
                    case "Random":
                        return defaultValue;
                    default:
                        int value;
                        if (int.TryParse(valueStr, out value))
                        {
                            return value;
                        }
                        break;
                }
                return defaultValue;
            }

            bool GetButtonValueBool(IntPtr buttonPtr)
            {
                return GetButtonIndex(buttonPtr) == 0;
            }

            void SetButtonIndex(IntPtr buttonPtr, int index)
            {
                IL2Array<IntPtr> strings = new IL2Array<IntPtr>(buttonSettingStrings.GetValue(buttonPtr).ptr);
                if (index >= strings.Length)
                {
                    index = strings.Length - 1;
                }
                if (index >= 0)
                {
                    buttonCurrentSetting.SetValue(buttonPtr, new IntPtr(&index));
                }
            }

            void SetButtonIndexFromBool(IntPtr buttonPtr, bool value)
            {
                SetButtonIndex(buttonPtr, value ? 0 : 1);
            }

            void SetButtonIndexFromString(IntPtr buttonPtr, string value)
            {
                int index = 0;
                if (!string.IsNullOrEmpty(value))
                {
                    IL2Array<IntPtr> strings = new IL2Array<IntPtr>(buttonSettingStrings.GetValue(buttonPtr).ptr);
                    int len = strings.Length;
                    for (int i = 0; i < len; i++)
                    {
                        if (new IL2String(strings[i]).ToString() == value)
                        {
                            index = i;
                            break;
                        }
                    }
                }
                SetButtonIndex(buttonPtr, index);
            }

            void SetButtonIndexFromI32(IntPtr buttonPtr, int value)
            {
                int index = 0;
                if (value != -1)
                {
                    IL2Array<IntPtr> strings = new IL2Array<IntPtr>(buttonSettingStrings.GetValue(buttonPtr).ptr);
                    int len = strings.Length;
                    for (int i = 0; i < len; i++)
                    {
                        int itemI32;
                        if (int.TryParse(new IL2String(strings[i]).ToString(), out itemI32) && itemI32 == value)
                        {
                            index = i;
                            break;
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
                        IL2Array<IntPtr> strings = new IL2Array<IntPtr>(buttonSettingStrings.GetValue(buttonPtr).ptr);
                        int len = strings.Length;
                        for (int i = 0; i < len; i++)
                        {
                            if (new IL2String(strings[i]).ToString() == name)
                            {
                                index = i;
                                break;
                            }
                        }
                    }
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
                int field = LookupItemName(ItemID.Category.FIELD, GetButtonValueString(buttons.Field));
                for (int i = 0; i < DuelSettings.MaxPlayers; i++)
                {
                    settings.mat[i] = field;
                }
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
                settings.BgmsFromValue(GetButtonValueI32(buttons.BGM));
                settings.MyType = GetButtonValueBool(buttons.Player1) ? 0 : 1;
                //settings.OpponentType = GetButtonValueBool(buttons.Player2) ? 1 : 2;
                //settings.MyPartnerType = GetButtonValueBool(buttons.Player3) ? 0 : 1;
                //settings.OpponentPartnerType = GetButtonValueBool(buttons.Player4) ? 1 : 2;
                SetTeamValue(GetButtonValueI32(buttons.LP1), settings.life, 0, 2, false);
                SetTeamValue(GetButtonValueI32(buttons.LP2), settings.life, 1, 3, false);
                SetTeamValue(GetButtonValueI32(buttons.Hand1), settings.hnum, 0, 2, false);
                SetTeamValue(GetButtonValueI32(buttons.Hand2), settings.hnum, 1, 3, false);
                SetTeamValue(LookupItemName(ItemID.Category.PROTECTOR, GetButtonValueString(buttons.Sleeve1)), settings.sleeve, 0, 2, true);
                SetTeamValue(LookupItemName(ItemID.Category.PROTECTOR, GetButtonValueString(buttons.Sleeve2)), settings.sleeve, 1, 3, true);
                SetTeamValue(LookupItemName(ItemID.Category.FIELD, GetButtonValueString(buttons.Field1)), settings.mat, 0, 2, false);
                SetTeamValue(LookupItemName(ItemID.Category.FIELD, GetButtonValueString(buttons.Field2)), settings.mat, 1, 3, false);
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
            }

            void DuelSettingsToUI()
            {
                SetButtonIndex(buttons.StartingPlayer, settings.FirstPlayer < 0 ? 0 : (settings.FirstPlayer + 1));
                SetButtonIndexFromI32(buttons.LifePoints, settings.AreAllEqual(settings.life) ? settings.life[0] : -1);
                SetButtonIndexFromI32(buttons.Hand, settings.AreAllEqual(settings.hnum) ? settings.hnum[0] : -1);
                SetButtonIndexFromItemId(buttons.Field, settings.AreAllEqual(settings.mat) ? settings.mat[0] : -1);
                SetButtonIndexFromString(buttons.DuelType, ((DuelType)settings.Type).ToString());
                SetButtonIndexFromString(buttons.Seed, settings.RandSeed == 0 ? null : settings.RandSeed.ToString());
                SetButtonIndexFromBool(buttons.Shuffle, !settings.noshuffle);
                SetButtonIndexFromString(buttons.Cpu, settings.cpu == int.MaxValue ? null : settings.cpu.ToString());
                SetButtonIndexFromString(buttons.CpuFlag, settings.cpuflag);
                SetButtonIndexFromString(buttons.Limit, ((DuelLimitedType)settings.Limit).ToString());
                SetButtonIndexFromI32(buttons.BGM, settings.GetBgmValue());
                SetButtonIndexFromBool(buttons.Player1, settings.MyType == 0);
                //SetButtonIndexFromBool(buttons.Player2, settings.OpponentType == 1);
                //SetButtonIndexFromBool(buttons.Player3, settings.MyPartnerType == 0);
                //SetButtonIndexFromBool(buttons.Player4, settings.OpponentPartnerType == 1);
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
                buttons = new Buttons();
                IL2ListExplicit infosList = new IL2ListExplicit(IntPtr.Zero, templateInfoClass, true);
                fieldInfos.SetValue(viewController, infosList.ptr);
                IntPtr isv = fieldIsv.GetValue(viewController).ptr;

                string[] lpStrings = new string[]
                {
                    "Default", "1", "50", "100", "500", "1000", "2000", "3000", "4000", "5000","6000", "7000", "8000", "9000",
                    "10000", "15000", "20000", "25000", "50000", "100000", "250000", "500000", "1000000", "9999999"
                };
                string[] handStrings = new string[]
                {
                    "Default", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15",
                    "20", "25", "30", "35", "45", "55", "60", "65", "65", "70", "75", "80", "85", "90", "95", "100"
                };
                List<string> cpuParamStrings = new List<string>();
                cpuParamStrings.Add("Default");
                for (int i = -100; i <= 100; i++)
                {
                    cpuParamStrings.Add(i.ToString());
                }
                List<string> seedStrings = new List<string>();
                seedStrings.Add("Random");
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

                if (bgmMax == 0)
                {
                    for (int i = 1; i < 99; i++)
                    {
                        if (AssetHelper.FileExists("Sound/AudioClip/BGM/BGM_DUEL_NORMAL_" + i.ToString().PadLeft(2, '0')))
                        {
                            bgmMax = i;
                        }
                    }
                }
                List<string> bgmStrings = new List<string>();
                bgmStrings.Add("Random");
                for (int i = 1; i <= bgmMax; i++)
                {
                    bgmStrings.Add(i.ToString());
                }

                AddLabel(infosList, "Decks");
                AddDeckDetailsButton(infosList, isv, 0);
                AddDeckDetailsButton(infosList, isv, 1);
                //AddDeckDetailsButton(infosList, isv, 2);
                //AddDeckDetailsButton(infosList, isv, 3);
                buttons.LoadFromFile = AddButtonYesNo(infosList, isv, "Load deck from file", false);
                AddLabel(infosList, "Settings");
                buttons.StartingPlayer = AddButton(infosList, isv, "Starting player", new string[] { "Random", "1", "2"/*, "3", "4"*/ });
                buttons.LifePoints = AddButton(infosList, isv, "Life points", lpStrings);
                buttons.Hand = AddButton(infosList, isv, "Hand", handStrings);
                buttons.Field = AddButton(infosList, isv, "Field", GetItemNames(ItemID.Category.FIELD).Values.ToArray());
                buttons.DuelType = AddButton(infosList, isv, "Duel type", duelType.ToArray());
                AddLabel(infosList, "Advanced settings");
                buttons.Seed = AddButton(infosList, isv, "Seed", seedStrings.ToArray());
                buttons.Shuffle = AddButtonYesNo(infosList, isv, "Shuffle", true);
                buttons.Cpu = AddButton(infosList, isv, "Cpu", cpuParamStrings.ToArray());
                buttons.CpuFlag = AddButton(infosList, isv, "CpuFlag", cpuFlagStrings.ToArray());
                buttons.Limit = AddButton(infosList, isv, "Limit", limitedTypeStrings.ToArray());
                buttons.BGM = AddButton(infosList, isv, "BGM", bgmStrings.ToArray());
                buttons.Player1 = AddButton(infosList, isv, "P1", new string[] { "Player", "CPU" }, false, 0);
                //buttons.Player2 = AddButton(infosList, isv, "P2", new string[] { "Player", "CPU" }, false, 1);
                //buttons.Player3 = AddButton(infosList, isv, "P3", new string[] { "Player", "CPU" }, false, 1);
                //buttons.Player4 = AddButton(infosList, isv, "P4", new string[] { "Player", "CPU" }, false, 1);
                buttons.LP1 = AddButton(infosList, isv, "LP1", lpStrings);
                buttons.LP2 = AddButton(infosList, isv, "LP2", lpStrings);
                buttons.Hand1 = AddButton(infosList, isv, "Hand1", handStrings);
                buttons.Hand2 = AddButton(infosList, isv, "Hand2", handStrings);
                buttons.Sleeve1 = AddButton(infosList, isv, "Sleeve1", GetItemNames(ItemID.Category.PROTECTOR).Values.ToArray());
                buttons.Sleeve2 = AddButton(infosList, isv, "Sleeve2", GetItemNames(ItemID.Category.PROTECTOR).Values.ToArray());
                buttons.Field1 = AddButton(infosList, isv, "Field1", GetItemNames(ItemID.Category.FIELD).Values.ToArray());
                buttons.Field2 = AddButton(infosList, isv, "Field2", GetItemNames(ItemID.Category.FIELD).Values.ToArray());
                buttons.FieldPart1 = AddButton(infosList, isv, "FieldPart1", GetItemNames(ItemID.Category.FIELD_OBJ).Values.ToArray());
                buttons.FieldPart2 = AddButton(infosList, isv, "FieldPart2", GetItemNames(ItemID.Category.FIELD_OBJ).Values.ToArray());
                buttons.Mate1 = AddButton(infosList, isv, "Mate1", GetItemNames(ItemID.Category.AVATAR).Values.ToArray());
                buttons.Mate2 = AddButton(infosList, isv, "Mate2", GetItemNames(ItemID.Category.AVATAR).Values.ToArray());
                buttons.MateBase1 = AddButton(infosList, isv, "MateBase1", GetItemNames(ItemID.Category.AVATAR_HOME).Values.ToArray());
                buttons.MateBase2 = AddButton(infosList, isv, "MateBase2", GetItemNames(ItemID.Category.AVATAR_HOME).Values.ToArray());
                buttons.Icon1 = AddButton(infosList, isv, "Icon1", GetItemNames(ItemID.Category.ICON).Values.ToArray());
                buttons.Icon2 = AddButton(infosList, isv, "Icon2", GetItemNames(ItemID.Category.ICON).Values.ToArray());
                buttons.IconFrame1 = AddButton(infosList, isv, "IconFrame1", GetItemNames(ItemID.Category.ICON_FRAME).Values.ToArray());
                buttons.IconFrame2 = AddButton(infosList, isv, "IconFrame2", GetItemNames(ItemID.Category.ICON_FRAME).Values.ToArray());
                AddLabel(infosList, "Load / Save");
                buttons.LoadIncludingDecks = AddButton(infosList, isv, "Load (including decks)");
                buttons.Load = AddButton(infosList, isv, "Load");
                buttons.Save = AddButton(infosList, isv, "Save");
                AddLabel(infosList, "Extra");
                buttons.OpenDeckEditor = AddButton(infosList, isv, "Open deck editor");
                buttons.ClearAllDecks = AddButton(infosList, isv, "Clear selected decks");

                if (hasExistingSetting)
                {
                    DuelSettingsToUI();
                }
            }

            void AddDeckDetailsButton(IL2ListExplicit infosList, IntPtr isv, int deckIndex)
            {
                // NOTE: Changed from string.Empty to "   " as I think there's a bug with empty strings in IL2String
                DeckInfo deckInfo = settings.Deck[deckIndex];
                IntPtr button = AddButton(infosList, isv, "Deck" + (deckIndex + 1), new string[] { "   " });//string.Empty });
                buttons.Decks[button] = deckInfo;
            }

            public bool HandleButtonClick(IntPtr buttonPtr)
            {
                IL2Object titleObj = buttonTitle.GetValue(buttonPtr);
                if (titleObj != null)
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
                    else
                    {
                        DeckInfo deck;
                        if (buttons.Decks.TryGetValue(buttonPtr, out deck))
                        {
                            bool loadFromFile = buttonCurrentSetting.GetValue(buttons.LoadFromFile).GetValueRef<int>() == 0;
                            if (loadFromFile)
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
                            else
                            {
                                IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                                YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "DeckEdit/DeckSelect",
                                    new Dictionary<string, object>()
                                    {
                                        { "GameMode", (int)9 }// SoloSingle
                                    });
                            }
                            return true;
                        }
                    }
                }
                return false;
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
                IL2Array<IntPtr> strings = new IL2Array<IntPtr>(buttonSettingStrings.GetValue(buttonPtr).ptr);
                strings[0] = new IL2String(GetDeckName(deck)).ptr;
            }

            string GetDeckName(DeckInfo deck)
            {
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

        delegate void Del_SelectDeck(IntPtr thisPtr, int mode, int deckId, int tournamentId, int rentalId);
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

        static void SelectDeck(IntPtr thisPtr, int mode, int deckId, int tournamentId, int rentalId)
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
            hookSelectDeck.Original(thisPtr, mode, deckId, tournamentId, rentalId);
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
                    // Redirect the home screen "DUEL" button to RoomCreate
                    YgomGame.Room.RoomCreateViewController.IsNextInstanceHacked = true;
                    prefabpathPtr = new IL2String("Room/RoomCreate").ptr;
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
        static IL2Method methodInstantiate;
        static IL2Method methodInstantiate2;
        static IL2Method methodDestroy;

        static UnityObject()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("Object");
            methodGetName = classInfo.GetProperty("name").GetGetMethod();
            methodInstantiate = classInfo.GetMethod("Instantiate", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Type.Name == classInfo.FullName);
            methodInstantiate2 = classInfo.GetMethod("Instantiate", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Type.Name == classInfo.FullName);
            methodDestroy = classInfo.GetMethod("Destroy");
        }

        public static string GetName(IntPtr thisPtr)
        {
            IL2Object result = methodGetName.Invoke(thisPtr);
            return result != null ? result.GetValueObj<string>() : null;
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
    }

    unsafe static class GameObject
    {
        static IL2Method methodGetTransform;
        static IL2Method methodGetComponentsTObject;
        static IL2Method methodGetComponent;
        static IL2Method methodGetActiveSelf;

        static GameObject()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("GameObject");
            methodGetTransform = classInfo.GetProperty("transform").GetGetMethod();
            methodGetComponentsTObject = classInfo.GetMethod("GetComponents", x => x.GetParameters().Length == 0).MakeGenericMethod(
                new Type[] { typeof(object) });
            methodGetComponent = classInfo.GetMethod("GetComponent", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Type.Name == typeof(Type).FullName);
            methodGetActiveSelf = classInfo.GetProperty("activeSelf").GetGetMethod();
        }

        public static IntPtr GetTranform(IntPtr thisPtr)
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

        public static IntPtr GetComponent(IntPtr thisPtr, IntPtr type)
        {
            IL2Object result = methodGetComponent.Invoke(thisPtr, new IntPtr[] { type });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr FindGameObjectByName(IntPtr thisPtr, string name)
        {
            string thisName = UnityObject.GetName(thisPtr);
            if (thisName == name)
            {
                return thisPtr;
            }
            IntPtr transform = UnityEngine.GameObject.GetTranform(thisPtr);
            int childCount = UnityEngine.Transform.GetChildCount(transform);
            for (int i = 0; i < childCount; i++)
            {
                IntPtr childTransform = UnityEngine.Transform.GetChild(transform, i);
                IntPtr childObject = UnityEngine.Component.GetGameObject(childTransform);
                IntPtr result = FindGameObjectByName(childObject, name);
                if (result != IntPtr.Zero)
                {
                    return result;
                }
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetRootObject(IntPtr thisPtr)
        {
            IntPtr transform = GameObject.GetTranform(thisPtr);
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
            IntPtr rootTransform = GameObject.GetTranform(GetRootObject(thisPtr));
            Dictionary<string, object> result = Transform.ToDictionary(rootTransform);
            return MiniJSON.Json.Format(MiniJSON.Json.Serialize(result));
        }

        public static string Dump(IntPtr thisPtr)
        {
            IntPtr transform = GameObject.GetTranform(thisPtr);
            Dictionary<string, object> result = Transform.ToDictionary(transform);
            return MiniJSON.Json.Format(MiniJSON.Json.Serialize(result));
        }

        public static bool IsActive(IntPtr thisPtr)
        {
            IL2Object result = methodGetActiveSelf.Invoke(thisPtr);
            return result != null && result.GetValueRef<bool>();
        }
    }

    unsafe static class Transform
    {
        static IL2Method methodGetChildCount;
        static IL2Method methodGetChild;
        static IL2Method methodGetParent;
        static IL2Method methodSetParent;

        static IL2Class serializedFieldClassInfo;

        static Transform()
        {
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("Transform");
            methodGetChildCount = classInfo.GetProperty("childCount").GetGetMethod();
            methodGetChild = classInfo.GetMethod("GetChild");
            methodGetParent = classInfo.GetProperty("parent").GetGetMethod();
            methodSetParent = classInfo.GetProperty("parent").GetSetMethod();

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
            Console.WriteLine("set " + thisPtr.ToInt64() + " to " + newParent.ToInt64());
            methodSetParent.Invoke(thisPtr, new IntPtr[] { newParent });
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
}