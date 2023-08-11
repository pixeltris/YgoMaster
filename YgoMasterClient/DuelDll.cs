using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YgoMaster.Net.Message;
using YgoMaster.Net;
using YgoMaster;
using System.Runtime.InteropServices;
using IL2CPP;

// NOTE: Be careful with threading here
// - If you want to run something on the duel thread use ActionsToRunInNextSysAct
// - If you want to run something in the game thread use TradeUtils.AddAction()

namespace YgoMasterClient
{
    unsafe static partial class DuelDll
    {
        public static List<byte> ReplayData = new List<byte>();

        public static List<Action> ActionsToRunInNextSysAct = new List<Action>();

        static DateTime LastSysActLogTime;
        static object LogLocker = new object();

        public static IntPtr CardPropMem;

        public static DuelResultType SpecialResultType;
        public static DuelFinishType SpecialFinishType;
        public static int DuelEndResult;
        public static int DuelEndFinish;
        public static int DuelEndFinishCardID;
        public static bool HasNetworkError;
        public static bool HasDuelStart;
        public static bool HasDuelEnd;
        public static bool HasSysActFinished;
        public static DateTime BeginDuelTime;
        public static ulong RunEffectSeq;
        public static bool IsPvpDuel;
        public static bool IsPvpSpectator;
        public static PvpSpectatorRapidState PvpSpectatorRapidState;
        public static int SpectatorCount;
        public static int MyID;
        public static bool SendLiveRecordData;
        public static bool IsFieldGuideNear;
        public static bool IsFieldGuideNearReal;
        public static DateTime LastFieldGuideUpdate;
        public static bool IsInsideDuelTimerPrepareToDuel;
        public static bool IsTimerEnabled;
        public static DateTime LastCheckTimeOver;
        public static int AddTimeAtStartOfTurn;
        public static int AddTimeAtEndOfTurn;
        public static int RivalID
        {
            get { return MyID == 0 ? 1 : 0; }
        }

        static PvpEngineState pvpEngineState = new PvpEngineState();
        static Queue<PvpEngineState> pvpEngineStates = new Queue<PvpEngineState>();
        static Queue<PvpEngineState> freePvpEngineStates = new Queue<PvpEngineState>();

        static IntPtr engineInstance;
        static IntPtr engineInstanceReplayStream;
        static IL2Field fieldEngineGameMode;
        static IL2Field fieldEngineInstance;
        static IL2Field fieldEngineReplayStream;
        static IL2Method methodReplayStreamAdd;
        static IL2Method methodReplayStreamFinish;
        static IntPtr activePlayerFieldEffectInstance;
        static IL2Method methodSwitchGuide;

        delegate void Del_SetGuideEnable(IntPtr thisPtr, bool near, bool enable, bool turnchange);
        static Hook<Del_SetGuideEnable> hookSetGuideEnable;

        // Engine
        delegate int Del_RunEffect(int id, int param1, int param2, int param3);
        static Del_RunEffect myRunEffect = RunEffect;
        static Del_RunEffect originalRunEffect;

        delegate int Del_IsBusyEffect(int id);
        static Del_IsBusyEffect myIsBusyEffect = IsBusyEffect;
        static Del_IsBusyEffect originalIsBusyEffect;

        delegate void Del_DLL_SetEffectDelegate(IntPtr runEffect, IntPtr isBusyEffect);
        static Hook<Del_DLL_SetEffectDelegate> hookDLL_SetEffectDelegate;

        delegate void Del_DLL_DuelComMovePhase(int phase);
        static Hook<Del_DLL_DuelComMovePhase> hookDLL_DuelComMovePhase;

        delegate void Del_DLL_DuelComDoCommand(int player, int position, int index, int commandId);
        static Hook<Del_DLL_DuelComDoCommand> hookDLL_DuelComDoCommand;

        delegate int Del_DLL_DuelComCancelCommand();
        static Hook<Del_DLL_DuelComCancelCommand> hookDLL_DuelComCancelCommand;

        delegate int Del_DLL_DuelComCancelCommand2(bool decide);
        static Hook<Del_DLL_DuelComCancelCommand2> hookDLL_DuelComCancelCommand2;

        delegate void Del_DLL_DuelDlgSetResult(uint result);
        static Hook<Del_DLL_DuelDlgSetResult> hookDLL_DuelDlgSetResult;

        delegate void Del_DLL_DuelListSetCardExData(int index, int data);
        static Hook<Del_DLL_DuelListSetCardExData> hookDLL_DuelListSetCardExData;

        delegate void Del_DLL_DuelListSetIndex(int index);
        static Hook<Del_DLL_DuelListSetIndex> hookDLL_DuelListSetIndex;

        delegate void Del_DLL_DuelListInitString();
        static Hook<Del_DLL_DuelListInitString> hookDLL_DuelListInitString;

        public delegate void Del_DLL_DuelComCheatCard(int player, int position, int index, int cardId, int face, int turn);
        public static Del_DLL_DuelComCheatCard DLL_DuelComCheatCard;

        public delegate void Del_DLL_DuelComDoDebugCommand(int player, int position, int index, int commandId);
        public static Del_DLL_DuelComDoDebugCommand DLL_DuelComDoDebugCommand;

        public delegate void Del_DLL_DuelComDebugCommand();
        public static Del_DLL_DuelComDebugCommand DLL_DuelComDebugCommand;

        delegate int Del_DLL_DuelSysAct();
        static Hook<Del_DLL_DuelSysAct> hookDLL_DuelSysAct;

        delegate void Del_AddRecord(IntPtr ptr, int size);
        delegate void Del_DLL_SetAddRecordDelegate(Del_AddRecord addRecord);
        static Del_DLL_SetAddRecordDelegate DLL_SetAddRecordDelegate;

        static DuelDll()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class engineClassInfo = assembly.GetClass("Engine", "YgomGame.Duel");
            fieldEngineInstance = engineClassInfo.GetField("s_instance");
            fieldEngineReplayStream = engineClassInfo.GetField("replayStream");
            fieldEngineGameMode = engineClassInfo.GetField("gameMode");

            IL2Class replayStreamClassInfo = assembly.GetClass("ReplayStream", "YgomGame.Duel");
            methodReplayStreamAdd = replayStreamClassInfo.GetMethod("Add");
            methodReplayStreamFinish = replayStreamClassInfo.BaseType.GetMethod("Finish");

            IL2Class fieldEffectClassInfo = assembly.GetClass("ActivePlayerFieldEffect", "YgomGame.Duel");
            hookSetGuideEnable = new Hook<Del_SetGuideEnable>(SetGuideEnable, fieldEffectClassInfo.GetMethod("SetGuideEnable"));
            methodSwitchGuide = fieldEffectClassInfo.GetMethod("SwitchGuide");

            IntPtr lib = PInvoke.LoadLibrary(Path.Combine("masterduel_Data", "Plugins", "x86_64", "duel.dll"));
            if (lib == IntPtr.Zero)
            {
                throw new Exception("Failed to load duel.dll");
            }

            InitProxyFunctions(lib);

            hookDLL_SetEffectDelegate = new Hook<Del_DLL_SetEffectDelegate>(DLL_SetEffectDelegate, PInvoke.GetProcAddress(lib, "DLL_SetEffectDelegate"));
            hookDLL_DuelSysAct = new Hook<Del_DLL_DuelSysAct>(DLL_DuelSysAct, PInvoke.GetProcAddress(lib, "DLL_DuelSysAct"));

            hookDLL_DuelComMovePhase = new Hook<Del_DLL_DuelComMovePhase>(DLL_DuelComMovePhase, PInvoke.GetProcAddress(lib, "DLL_DuelComMovePhase"));
            hookDLL_DuelComDoCommand = new Hook<Del_DLL_DuelComDoCommand>(DLL_DuelComDoCommand, PInvoke.GetProcAddress(lib, "DLL_DuelComDoCommand"));
            hookDLL_DuelComCancelCommand = new Hook<Del_DLL_DuelComCancelCommand>(DLL_DuelComCancelCommand, PInvoke.GetProcAddress(lib, "DLL_DuelComCancelCommand"));
            hookDLL_DuelComCancelCommand2 = new Hook<Del_DLL_DuelComCancelCommand2>(DLL_DuelComCancelCommand2, PInvoke.GetProcAddress(lib, "DLL_DuelComCancelCommand2"));
            hookDLL_DuelDlgSetResult = new Hook<Del_DLL_DuelDlgSetResult>(DLL_DuelDlgSetResult, PInvoke.GetProcAddress(lib, "DLL_DuelDlgSetResult"));
            hookDLL_DuelListSetCardExData = new Hook<Del_DLL_DuelListSetCardExData>(DLL_DuelListSetCardExData, PInvoke.GetProcAddress(lib, "DLL_DuelListSetCardExData"));
            hookDLL_DuelListSetIndex = new Hook<Del_DLL_DuelListSetIndex>(DLL_DuelListSetIndex, PInvoke.GetProcAddress(lib, "DLL_DuelListSetIndex"));
            hookDLL_DuelListInitString = new Hook<Del_DLL_DuelListInitString>(DLL_DuelListInitString, PInvoke.GetProcAddress(lib, "DLL_DuelListInitString"));

            DLL_DuelComCheatCard = Utils.GetFunc<Del_DLL_DuelComCheatCard>(PInvoke.GetProcAddress(lib, "DLL_DuelComCheatCard"));
            DLL_DuelComDoDebugCommand = Utils.GetFunc<Del_DLL_DuelComDoDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDoDebugCommand"));
            DLL_DuelComDebugCommand = Utils.GetFunc<Del_DLL_DuelComDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDebugCommand"));

            DLL_SetAddRecordDelegate = Utils.GetFunc<Del_DLL_SetAddRecordDelegate>(PInvoke.GetProcAddress(lib, "DLL_SetAddRecordDelegate"));
        }

        static void Log(string str)
        {
            if (ClientSettings.PvpLogToConsole)
            {
                Console.WriteLine(str);
            }
            LogToFile(str);
        }

        static void LogToFile(string str, bool append = true)
        {
            if (!ClientSettings.PvpLogToFile)
            {
                return;
            }
            lock (LogLocker)
            {
                try
                {
                    string fileName = Path.Combine(Program.ClientDataDir, "DuelLog.txt");
                    string fullLog = "[" + DateTime.Now.TimeOfDay + "] " + str + (append ? Environment.NewLine : string.Empty);
                    if (append)
                    {
                        File.AppendAllText(fileName, fullLog);
                    }
                    else
                    {
                        File.WriteAllText(fileName, fullLog);
                    }
                }
                catch
                {
                }
            }
        }

        public static void OnDuelRoomBattleReady()
        {
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Clear();
            }
        }

        public static void OnDuelBegin(GameMode gameMode)
        {
            LogToFile(string.Empty, false);
            ReplayData.Clear();
            SpecialResultType = DuelResultType.None;
            SpecialFinishType = DuelFinishType.None;
            DuelEndResult = 0;
            DuelEndFinish = 0;
            DuelEndFinishCardID = 0;
            HasNetworkError = false;
            HasDuelStart = false;
            HasDuelEnd = false;
            HasSysActFinished = false;
            BeginDuelTime = DateTime.UtcNow;
            RunEffectSeq = 0;
            IsPvpDuel = Program.NetClient != null && gameMode == GameMode.Room;
            IsPvpSpectator = Program.NetClient != null && gameMode == GameMode.Audience;
            SpectatorCount = 0;
            MyID = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.MyID");
            SendLiveRecordData = YgomSystem.Utility.ClientWork.GetByJsonPath<bool>("Duel.SendLiveRecordData");
            IsInsideDuelTimerPrepareToDuel = false;

            IsTimerEnabled = IsPvpDuel && YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.TotalTimeMax") > 0;
            LastCheckTimeOver = DateTime.MinValue;
            if (IsPvpDuel)
            {
                AddTimeAtStartOfTurn = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.AddTimeAtStartOfTurn");
                AddTimeAtEndOfTurn = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.AddTimeAtEndOfTurn");
            }
            else
            {
                AddTimeAtStartOfTurn = 0;
                AddTimeAtEndOfTurn = 0;
            }

            engineInstance = IntPtr.Zero;
            engineInstanceReplayStream = IntPtr.Zero;
            activePlayerFieldEffectInstance = IntPtr.Zero;

            PvpSpectatorRapidState = PvpSpectatorRapidState.None;
            if (IsPvpSpectator)
            {
                // NOTE: Although we use "rapid" it doesn't actually do what it does in-game by default which is why we do this speed-up
                if (YgomSystem.Utility.ClientWork.GetByJsonPath<bool>("Duel.rapid") &&
                    ClientSettings.DuelClientSpectatorRapidTimeMultiplayer > 0)
                {
                    PvpSpectatorRapidState = PvpSpectatorRapidState.WaitingForSysAct;
                }
                Program.NetClient.Send(new DuelSpectatorEnterMessage());
            }

            DuelTapSync.ClearState();
        }

        public static void OnDuelEnd()
        {
            PvpSpectatorRapidState = PvpSpectatorRapidState.None;
            engineInstance = IntPtr.Zero;
            engineInstanceReplayStream = IntPtr.Zero;
            activePlayerFieldEffectInstance = IntPtr.Zero;
            DuelTapSync.ClearState();
            DuelEmoteHelper.OnEndDuel();
        }

        static void ClearEngineState()
        {
            lock (pvpEngineState)
            {
                pvpEngineState.Clear();
                foreach (PvpEngineState state in pvpEngineStates)
                {
                    freePvpEngineStates.Enqueue(state);
                }
                foreach (PvpEngineState state in freePvpEngineStates)
                {
                    state.Clear();
                }
            }
        }

        public static void OnInitEngineStep()
        {
            // NOTE: DuelClient.InitEngineStep is also where ReplayStream is set up. Might be useful
            if (IsPvpDuel)
            {
                Log("OnInitEngineStep");
            }
            DLL_SetAddRecordDelegate(AddRecord);
        }

        static void DLL_SetEffectDelegate(IntPtr runEffect, IntPtr isBusyEffect)
        {
            originalRunEffect = Utils.GetFunc<Del_RunEffect>(runEffect);
            originalIsBusyEffect = Utils.GetFunc<Del_IsBusyEffect>(isBusyEffect);
            hookDLL_SetEffectDelegate.Original(Marshal.GetFunctionPointerForDelegate(myRunEffect), Marshal.GetFunctionPointerForDelegate(myIsBusyEffect));
        }

        static Del_AddRecord AddRecord = (IntPtr ptr, int size) =>
        {
            for (int i = 0; i < size; i++)
            {
                ReplayData.Add(*(byte*)(ptr + i));
            }
            if (IsPvpDuel && SendLiveRecordData)
            {
                byte[] buffer = new byte[size];
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                Program.NetClient.Send(new DuelSpectatorDataMessage()
                {
                    Buffer = buffer
                });
            }
        };

        static void SetGuideEnable(IntPtr thisPtr, bool near, bool enable, bool turnchange)
        {
            if (IsPvpDuel)
            {
                //Log("SetGuideEnable:" + thisPtr + " near:" + near + " enable:" + enable + " turnchange:" + turnchange);
            }
            activePlayerFieldEffectInstance = thisPtr;
            LastFieldGuideUpdate = DateTime.UtcNow;
            bool nearEnable = (near && enable) || (!near && !enable);
            IsFieldGuideNearReal = nearEnable;
            if (IsPvpDuel && SendLiveRecordData && near && IsFieldGuideNear != nearEnable)
            {
                IsFieldGuideNear = nearEnable;
                Program.NetClient.Send(new DuelSpectatorFieldGuideMessage()
                {
                    Near = nearEnable
                });
            }
            hookSetGuideEnable.Original(thisPtr, near, enable, turnchange);
        }

        static IntPtr GetReplayStream()
        {
            if (engineInstance == IntPtr.Zero)
            {
                engineInstance = fieldEngineInstance.GetValue().ptr;
            }
            if (engineInstance != IntPtr.Zero && engineInstanceReplayStream == IntPtr.Zero)
            {
                engineInstanceReplayStream = fieldEngineReplayStream.GetValue(engineInstance).ptr;
            }
            return engineInstanceReplayStream;
        }

        static void UpdateFieldGuide()
        {
            LastFieldGuideUpdate = DateTime.UtcNow;
            if (activePlayerFieldEffectInstance != IntPtr.Zero)
            {
                int team = IsFieldGuideNear ? 0 : 1;
                bool forceswitch = true;
                methodSwitchGuide.Invoke(activePlayerFieldEffectInstance, new IntPtr[] { new IntPtr(&team), new IntPtr(&forceswitch) });
            }
        }

        static void EndSpectatorReplayStream()
        {
            IntPtr replayStream = GetReplayStream();
            if (replayStream != IntPtr.Zero)
            {
                methodReplayStreamFinish.Invoke(engineInstanceReplayStream);
            }
        }

        static void UpdateSpectatorCount(int num)
        {
            SpectatorCount = num;
            Action action = () =>
            {
                YgomGame.Duel.DuelHUD.OnChangeWatcherNum(SpectatorCount);
            };
            TradeUtils.AddAction(action);
        }

        static int InjectDuelEnd()
        {
            DuelTapSync.ClearState();
            DuelEmoteHelper.OnEndDuel();
            HasDuelEnd = true;
            if (IsPvpSpectator)
            {
                EndSpectatorReplayStream();
            }
            if (SpecialFinishType != DuelFinishType.None)
            {
                return originalRunEffect((int)DuelViewType.DuelEnd, (int)SpecialResultType, (int)SpecialFinishType, 0);
            }
            else if (HasNetworkError)
            {
                return originalRunEffect((int)DuelViewType.DuelEnd, (int)DuelResultType.Draw, (int)DuelFinishType.FinishError, 0);
            }
            return 0;
        }

        static int RunEffect(int id, int param1, int param2, int param3)
        {
            if (IsPvpDuel || IsPvpSpectator)
            {
                DuelEmoteHelper.OnRunEffect((DuelViewType)id, param1, param2, param3);
            }
            if (IsPvpSpectator)
            {
                if (HasNetworkError)
                {
                    if (HasDuelStart)
                    {
                        EndSpectatorReplayStream();
                        return InjectDuelEnd();
                    }
                    else
                    {
                        if ((DuelViewType)id == DuelViewType.DuelStart)
                        {
                            HasDuelStart = true;
                        }
                        originalRunEffect(id, param1, param2, param3);
                        EndSpectatorReplayStream();
                        return InjectDuelEnd();
                    }
                }

                switch ((DuelViewType)id)
                {
                    case DuelViewType.DuelStart:
                        UpdateSpectatorCount(SpectatorCount);
                        HasDuelStart = true;
                        break;
                    case DuelViewType.DuelEnd:
                        DuelTapSync.ClearState();
                        DuelEmoteHelper.OnEndDuel();
                        HasDuelEnd = true;
                        EndSpectatorReplayStream();
                        break;
                }
            }
            return originalRunEffect(id, param1, param2, param3);
        }

        static int IsBusyEffect(int id)
        {
            if (HasNetworkError || SpecialFinishType != DuelFinishType.None)
            {
                return originalIsBusyEffect(id);
            }
            return originalIsBusyEffect(id);
        }

        public static int DLL_DuelSysAct()
        {
            if (IsPvpDuel || IsPvpSpectator)
            {
                if (LastSysActLogTime < DateTime.UtcNow - TimeSpan.FromSeconds(3))
                {
                    LastSysActLogTime = DateTime.UtcNow;
                    LogToFile("DLL_DuelSysAct");
                }

                if (IsTimerEnabled && SpecialFinishType == DuelFinishType.None && LastCheckTimeOver < DateTime.UtcNow - TimeSpan.FromSeconds(1))
                {
                    LastCheckTimeOver = DateTime.UtcNow;
                    if (YgomGame.Duel.DuelTimer3D.IsPlayerTimeOver)
                    {
                        SpecialResultType = DuelResultType.Lose;
                        SpecialFinishType = DuelFinishType.TimeOut;
                        InjectDuelEnd();
                    }
                }

                if (IsPvpSpectator)
                {
                    if (PvpSpectatorRapidState == PvpSpectatorRapidState.WaitingForSysAct)
                    {
                        PvpSpectatorRapidState = PvpSpectatorRapidState.Active;
                        PInvoke.SetTimeMultiplier(ClientSettings.DuelClientSpectatorRapidTimeMultiplayer);
                    }
                    if (PvpSpectatorRapidState == PvpSpectatorRapidState.Active && YgomGame.Duel.DuelClient.ReplayRealtime)
                    {
                        PvpSpectatorRapidState = PvpSpectatorRapidState.Finished;
                        PInvoke.SetTimeMultiplier(ClientSettings.DuelClientTimeMultiplier != 0 ?
                            ClientSettings.DuelClientTimeMultiplier : ClientSettings.TimeMultiplier);
                    }

                    // Hacky fix for field guide being on the wrong side when spectating a duel
                    if (IsFieldGuideNearReal != IsFieldGuideNear && LastFieldGuideUpdate < DateTime.UtcNow - TimeSpan.FromSeconds(1))
                    {
                        UpdateFieldGuide();
                    }
                }

                if (IsPvpDuel)
                {
                    lock (pvpEngineStates)
                    {
                        if (HasSysActFinished && pvpEngineStates.Count == 0)
                        {
                            return 1;
                        }
                    }

                    PvpEngineState stateUpdate = null;
                    lock (pvpEngineState)
                    {
                        if (pvpEngineState.IsBusyEffect.Count > 0)
                        {
                            HashSet<DuelViewType> changedStates = null;
                            foreach (KeyValuePair<DuelViewType, int> busyState in pvpEngineState.IsBusyEffect)
                            {
                                if (originalIsBusyEffect((int)busyState.Key) == 0)
                                {
                                    if (changedStates == null)
                                    {
                                        changedStates = new HashSet<DuelViewType>();
                                    }
                                    //Log("Send DuelIsBusyEffectMessage " + busyState.Key + " " + pvpEngineState.RunEffectSeq);
                                    changedStates.Add(busyState.Key);
                                    Program.NetClient.Send(new DuelIsBusyEffectMessage()
                                    {
                                        RunEffectSeq = pvpEngineState.RunEffectSeq,
                                        ViewType = busyState.Key
                                    });
                                }
                            }
                            if (changedStates != null)
                            {
                                foreach (DuelViewType viewType in changedStates)
                                {
                                    pvpEngineState.IsBusyEffect.Remove(viewType);
                                }
                            }
                        }
                        else if (pvpEngineStates.Count > 0)
                        {
                            stateUpdate = pvpEngineStates.Dequeue();
                            if (stateUpdate != null)
                            {
                                pvpEngineState.Update(stateUpdate);
                                RunEffectSeq = pvpEngineState.RunEffectSeq;
                            }
                        }
                    }

                    if (stateUpdate != null)
                    {
                        switch (pvpEngineState.ViewType)
                        {
                            case DuelViewType.DuelStart:
                                UpdateSpectatorCount(SpectatorCount);
                                HasDuelStart = true;
                                goto default;
                            case DuelViewType.DuelEnd:
                                DuelTapSync.ClearState();
                                DuelEmoteHelper.OnEndDuel();
                                HasDuelEnd = true;
                                if (MyID != 0)
                                {
                                    switch ((DuelResultType)pvpEngineState.Param1)
                                    {
                                        case DuelResultType.Win:
                                            pvpEngineState.Param1 = (int)DuelResultType.Lose;
                                            break;
                                        case DuelResultType.Lose:
                                            pvpEngineState.Param1 = (int)DuelResultType.Win;
                                            break;
                                    }
                                }
                                // We do this because the DLL_XXXX functions don't seem to be working correctly
                                DuelEndResult = pvpEngineState.Param1;
                                DuelEndFinish = pvpEngineState.Param2;
                                DuelEndFinishCardID = pvpEngineState.Param3;
                                goto default;
                            case DuelViewType.TurnChange:
                                if (pvpEngineState.Param1 == MyID && AddTimeAtStartOfTurn > 0)
                                {
                                    TradeUtils.AddAction(() =>
                                    {
                                        YgomGame.Duel.DuelTimer3D.AddTurnTime(AddTimeAtStartOfTurn, AddTimeAtStartOfTurn + AddTimeAtEndOfTurn);
                                    });
                                }
                                else if (pvpEngineState.Param1 == RivalID && AddTimeAtEndOfTurn > 0)
                                {
                                    TradeUtils.AddAction(() =>
                                    {
                                        YgomGame.Duel.DuelTimer3D.AddTurnTime(AddTimeAtEndOfTurn, AddTimeAtStartOfTurn + AddTimeAtEndOfTurn);
                                    });
                                }
                                goto default;
                            case DuelViewType.WaitInput:
                                if (pvpEngineState.DoCommandUser == MyID)
                                {
                                    goto default;
                                }
                                else
                                {
                                    RunEffect((int)DuelViewType.CpuThinking, 0, 0, 0);
                                }
                                break;
                            case DuelViewType.RunDialog:
                                if (pvpEngineState.RunDialogUser == MyID)
                                {
                                    goto default;
                                }
                                else
                                {
                                    RunEffect((int)DuelViewType.CpuThinking, 0, 0, 0);
                                }
                                break;
                            case DuelViewType.RunList:
                                if (pvpEngineState.Param1 == MyID)
                                {
                                    goto default;
                                }
                                else
                                {
                                    RunEffect((int)DuelViewType.CpuThinking, 0, 0, 0);
                                }
                                break;
                            default:
                                RunEffect((int)pvpEngineState.ViewType, pvpEngineState.Param1, pvpEngineState.Param2, pvpEngineState.Param3);
                                break;
                        }
                        stateUpdate.Clear();
                        lock (pvpEngineState)
                        {
                            freePvpEngineStates.Enqueue(stateUpdate);
                        }
                    }
                }

                if (ActionsToRunInNextSysAct.Count > 0)
                {
                    List<Action> actionsToRun;
                    lock (ActionsToRunInNextSysAct)
                    {
                        actionsToRun = new List<Action>(ActionsToRunInNextSysAct);
                        ActionsToRunInNextSysAct.Clear();
                    }
                    foreach (Action action in actionsToRun)
                    {
                        action();
                    }
                }

                if ((HasNetworkError || SpecialFinishType != DuelFinishType.None) && HasDuelStart)
                {
                    return 1;
                }

                if (IsPvpDuel)
                {
                    return 0;
                }
            }
            return hookDLL_DuelSysAct.Original();
        }

        static void DLL_DuelComMovePhase(int phase)
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelComMovePhase phase:" + phase + " seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelComMovePhaseMessage()
                {
                    RunEffectSeq = RunEffectSeq,
                    Phase = phase
                });
            }
            hookDLL_DuelComMovePhase.Original(phase);
        }

        public static void DLL_DuelComDoCommand(int player, int position, int index, int commandId)
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelComDoCommand player:" + player + " pos:" + position + " indx:" + index + " cmd:" + commandId + " seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelComDoCommandMessage()
                {
                    RunEffectSeq = RunEffectSeq,
                    Player = player,
                    Position = position,
                    Index = index,
                    CommandId = commandId
                });
            }
            hookDLL_DuelComDoCommand.Original(player, position, index, commandId);
        }

        static int DLL_DuelComCancelCommand()
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelComCancelCommand seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelComCancelCommandMessage()
                {
                    RunEffectSeq = RunEffectSeq
                });
            }
            return hookDLL_DuelComCancelCommand.Original();
        }

        static int DLL_DuelComCancelCommand2(bool decide)
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelComCancelCommand2 seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelComCancelCommand2Message()
                {
                    RunEffectSeq = RunEffectSeq,
                    Decide = decide
                });
            }
            return hookDLL_DuelComCancelCommand2.Original(decide);
        }

        static void DLL_DuelDlgSetResult(uint result)
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelDlgSetResult result:" + result + " seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelDlgSetResultMessage()
                {
                    RunEffectSeq = RunEffectSeq,
                    Result = result
                });
            }
            hookDLL_DuelDlgSetResult.Original(result);
        }

        static void DLL_DuelListSetCardExData(int index, int data)
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelListSetCardExData index:" + index + " data:" + data + " seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelListSetCardExDataMessage()
                {
                    RunEffectSeq = RunEffectSeq,
                    Index = index,
                    Data = data
                });
            }
            hookDLL_DuelListSetCardExData.Original(index, data);
        }

        static void DLL_DuelListSetIndex(int index)
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelListSetIndex index:" + index + "seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelListSetIndexMessage()
                {
                    RunEffectSeq = RunEffectSeq,
                    Index = index
                });
            }
            hookDLL_DuelListSetIndex.Original(index);
        }

        static void DLL_DuelListInitString()
        {
            if (IsPvpDuel)
            {
                Log("DLL_DuelListInitString seq:" + RunEffectSeq);
                Program.NetClient.Send(new DuelListInitStringMessage()
                {
                    RunEffectSeq = RunEffectSeq
                });
            }
            hookDLL_DuelListInitString.Original();
        }

        public static void HandleNetMessage(NetClient client, NetMessage message)
        {
            switch (message.Type)
            {
                case NetMessageType.ConnectionResponse: OnConnectionResponse((ConnectionResponseMessage)message); break;
                case NetMessageType.Ping: OnPing((PingMessage)message); break;
                case NetMessageType.DuelError: OnDuelError((DuelErrorMessage)message); break;
                case NetMessageType.OpponentDuelEnded: OnOpponentDuelEnded((OpponentDuelEndedMessage)message); break;
                case NetMessageType.DuelSpectatorData: OnDuelSpectatorData((DuelSpectatorDataMessage)message); break;
                case NetMessageType.DuelSpectatorFieldGuide: OnDuelSpectatorFieldGuide((DuelSpectatorFieldGuideMessage)message); break;
                case NetMessageType.DuelSpectatorCount: OnDuelSpectatorCount((DuelSpectatorCountMessage)message); break;
                case NetMessageType.DuelTapSync: DuelTapSync.OnDuelTapSync((DuelTapSyncMessage)message); break;
                case NetMessageType.DuelEmote: DuelEmoteHelper.OnDuelEmote((DuelEmoteMessage)message); break;
                case NetMessageType.DuelEngineState: OnDuelEngineState((DuelEngineStateMessage)message); break;
                case NetMessageType.DuelIsBusyEffect: OnDuelIsBusyEffect((DuelIsBusyEffectMessage)message); break;
                case NetMessageType.DuelSysActFinished: OnDuelSysActFinished((DuelSysActFinishedMessage)message); break;
            }
        }

        static void OnConnectionResponse(ConnectionResponseMessage message)
        {
            if (!message.Success)
            {
                Log("Session server failed to validate token '" + ClientSettings.MultiplayerToken + "'");
            }
        }

        static void OnNetworkError()
        {
            try
            {
                // try/catch as we aren't in the main thread
                if (HasNetworkError || SpecialFinishType != DuelFinishType.None || HasDuelEnd/* ||
                    (YgomGame.Duel.DuelClient.Step != DuelClientStep.ExecDuel && !IsPvpSpectator)*/)
                {
                    return;
                }
            }
            catch
            {
            }

            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(() =>
                {
                    if (HasNetworkError || SpecialFinishType != DuelFinishType.None || HasDuelEnd/* ||
                        (YgomGame.Duel.DuelClient.Step != DuelClientStep.ExecDuel && !IsPvpSpectator)*/)
                    {
                        return;
                    }
                    HasNetworkError = true;
                    Log("OnNetworkError");
                    if (/*IsPvpSpectator && */!HasDuelStart)
                    {
                        // NOTE: This is really hacky and looks weird but the client can get stuck without this
                        originalRunEffect((int)DuelViewType.DuelStart, 0, 0, 0);
                        HasDuelStart = true;
                    }
                    if (HasDuelStart)
                    {
                        InjectDuelEnd();
                    }
                    if (IsPvpDuel)
                    {
                        Program.NetClient.Send(new DuelErrorMessage());
                    }
                });
            }
        }

        static void OnPing(PingMessage message)
        {
            // TODO: Send some info stating if we're in a duel
            Program.NetClient.Send(new PongMessage()
            {
                ServerToClientLatency = Utils.GetEpochTime() - Utils.GetEpochTime(message.RequestTime),
                ResponseTime = DateTime.UtcNow,
            });

            if (message.DuelingState != DuelRoomTableState.Dueling && IsPvpDuel &&
                !HasNetworkError && SpecialFinishType == DuelFinishType.None && !HasDuelEnd &&
                BeginDuelTime < DateTime.UtcNow - TimeSpan.FromSeconds(5) &&
                YgomGame.Duel.DuelClient.Instance != IntPtr.Zero)
            {
                OnNetworkError();
            }
        }

        static void OnDuelError(DuelErrorMessage message)
        {
            if (IsPvpDuel || IsPvpSpectator)
            {
                OnNetworkError();
            }
        }

        static void OnOpponentDuelEnded(OpponentDuelEndedMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Action action = () =>
            {
                if (message.Result == DuelResultType.Lose && SpecialFinishType == DuelFinishType.None)
                {
                    switch (message.Finish)
                    {
                        case DuelFinishType.TimeOut:
                        case DuelFinishType.Surrender:
                            SpecialResultType = DuelResultType.Win;
                            SpecialFinishType = message.Finish;
                            InjectDuelEnd();
                            break;
                    }
                }
                HasDuelEnd = true;
            };
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(action);
            }
        }

        static void OnDuelSpectatorData(DuelSpectatorDataMessage message)
        {
            Action action = () =>
            {
                if (message.Buffer != null && message.Buffer.Length > 0)
                {
                    if (IsPvpDuel)
                    {
                        if (message.IsFirstData)
                        {
                            ReplayData.Clear();
                        }
                        ReplayData.AddRange(message.Buffer);
                    }
                    else if (IsPvpSpectator)
                    {
                        IntPtr replayStream = GetReplayStream();
                        if (replayStream != IntPtr.Zero)
                        {
                            IL2Array<byte> buffer = new IL2Array<byte>(message.Buffer.Length, IL2SystemClass.Byte);
                            buffer.CopyFrom(message.Buffer);
                            methodReplayStreamAdd.Invoke(replayStream, new IntPtr[] { buffer.ptr });
                        }
                    }
                }
            };
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(action);
            }
        }

        static void OnDuelSpectatorFieldGuide(DuelSpectatorFieldGuideMessage message)
        {
            TradeUtils.AddAction(() =>
            {
                IsFieldGuideNear = message.Near;
                UpdateFieldGuide();
            });
        }

        static void OnDuelSpectatorCount(DuelSpectatorCountMessage message)
        {
            UpdateSpectatorCount(message.Count);
        }

        static void OnDuelEngineState(DuelEngineStateMessage message)
        {
            Log("OnDuelEngineState " + message.RunEffectSeq + " " + message.ViewType);
            if (message.RunEffectSeq == 1)
            {
                ClearEngineState();
            }
            PvpEngineState state;
            lock (pvpEngineState)
            {
                if (freePvpEngineStates.Count > 0)
                {
                    state = freePvpEngineStates.Dequeue();
                    state.Clear();
                }
                else
                {
                    state = new PvpEngineState();
                }
            }
            state.RunEffectSeq = message.RunEffectSeq;
            state.ViewType = message.ViewType;
            state.Param1 = message.Param1;
            state.Param2 = message.Param2;
            state.Param3 = message.Param3;
            state.DoCommandUser = message.DoCommandUser;
            state.RunDialogUser = message.RunDialogUser;
            state.Read(message.CompressedBuffer);
            lock (pvpEngineState)
            {
                pvpEngineStates.Enqueue(state);
            }
        }

        static void OnDuelIsBusyEffect(DuelIsBusyEffectMessage message)
        {
            lock (pvpEngineState)
            {
                PvpEngineState state;
                if (pvpEngineState.RunEffectSeq == message.RunEffectSeq)
                {
                    state = pvpEngineState;
                }
                else
                {
                    state = pvpEngineStates.FirstOrDefault(x => x.RunEffectSeq == message.RunEffectSeq);
                }
                if (state != null)
                {
                    Log("OnDuelIsBusyEffect " + message.RunEffectSeq + " " + message.ViewType);
                    state.IsBusyEffect[message.ViewType] = 0;
                }
                else
                {
                    Utils.LogWarning("Failed to find state for IsBusyEffect seq " + message.RunEffectSeq + " " + message.ViewType);
                }
            }
        }

        static void OnDuelSysActFinished(DuelSysActFinishedMessage message)
        {
            HasSysActFinished = true;
        }
    }

    enum PvpSpectatorRapidState
    {
        None,
        WaitingForSysAct,
        Active,
        Finished
    }
}