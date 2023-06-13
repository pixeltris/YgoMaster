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

namespace YgoMasterClient
{
    unsafe static class DuelDll
    {
        public static Queue<DuelComMessage> DuelComMessageQueue = new Queue<DuelComMessage>();

        public static Dictionary<DuelViewType, int> LocalIsBusyEffect = new Dictionary<DuelViewType, int>();
        public static Dictionary<ulong, Dictionary<DuelViewType, int>> RemoteIsBusyEffect = new Dictionary<ulong, Dictionary<DuelViewType, int>>();

        public static List<byte> ReplayData = new List<byte>();

        public static List<Action> ActionsToRunInNextSysAct = new List<Action>();

        static DateTime LastSysActLogTime;
        static object LogLocker = new object();

        public static bool HasOpponentSurrendered;
        public static bool HasNetworkError;
        public static bool HasDuelStart;
        public static bool HasDuelEnd;
        public static DateTime BeginDuelTime;
        public static ulong RunEffectSeq;
        public static ulong AddRecordSeq;
        public static bool IsPvpDuel;
        public static bool IsPvpSpectator;
        public static PvpSpectatorRapidState PvpSpectatorRapidState;
        public static int MyID;
        public static bool SendLiveRecordData;
        public static bool IsFieldGuideNear;
        public static bool IsFieldGuideNearReal;
        public static DateTime LastFieldGuideUpdate;
        public static int RivalID
        {
            get { return MyID == 0 ? 1 : 0; }
        }

        static IntPtr WorkMemory;
        static int ActiveUserIdForDoCommand
        {
            get { return *(int*)(WorkMemory + ClientSettings.DuelDllActiveUserDoCommandOffset); }
        }
        static int ActiveUserIdForSetIndex
        {
            get { return *(int*)(WorkMemory + ClientSettings.DuelDllActiveUserSetIndexOffset); }
        }

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

        delegate int Del_RunEffect(int id, int param1, int param2, int param3);
        static Del_RunEffect myRunEffect = RunEffect;
        static Del_RunEffect originalRunEffect;

        delegate int Del_IsBusyEffect(int id);
        static Del_IsBusyEffect myIsBusyEffect = IsBusyEffect;
        static Del_IsBusyEffect originalIsBusyEffect;

        delegate void Del_DLL_SetEffectDelegate(IntPtr runEffect, IntPtr isBusyEffect);
        static Hook<Del_DLL_SetEffectDelegate> hookDLL_SetEffectDelegate;

        delegate int Del_DLL_SetWorkMemory(IntPtr pWork);
        static Hook<Del_DLL_SetWorkMemory> hookDLL_SetWorkMemory;

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

        delegate void Del_DLL_DuelSetPlayerType(int player, int type);
        static Hook<Del_DLL_DuelSetPlayerType> hookDLL_DuelSetPlayerType;

        public delegate void Del_DLL_DuelComCheatCard(int player, int position, int index, int cardId, int face, int turn);
        public static Del_DLL_DuelComCheatCard DLL_DuelComCheatCard;

        public delegate void Del_DLL_DuelComDoDebugCommand(int player, int position, int index, int commandId);
        public static Del_DLL_DuelComDoDebugCommand DLL_DuelComDoDebugCommand;

        public delegate void Del_DLL_DuelComDebugCommand();
        public static Del_DLL_DuelComDebugCommand DLL_DuelComDebugCommand;

        delegate int Del_DLL_DuelSysAct();
        static Hook<Del_DLL_DuelSysAct> hookDLL_DuelSysAct;

        public delegate int Del_DLL_DuelWhichTurnNow();
        public static Del_DLL_DuelWhichTurnNow DLL_DuelWhichTurnNow;

        public delegate int Del_DLL_DuelGetTurnNum();
        public static Del_DLL_DuelGetTurnNum DLL_DuelGetTurnNum;

        public delegate int Del_DLL_DuelMyself();
        public static Del_DLL_DuelMyself DLL_DuelMyself;

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

            if (!string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
            {
                hookDLL_SetEffectDelegate = new Hook<Del_DLL_SetEffectDelegate>(DLL_SetEffectDelegate, PInvoke.GetProcAddress(lib, "DLL_SetEffectDelegate"));
                hookDLL_SetWorkMemory = new Hook<Del_DLL_SetWorkMemory>(DLL_SetWorkMemory, PInvoke.GetProcAddress(lib, "DLL_SetWorkMemory"));
            }

            hookDLL_DuelComMovePhase = new Hook<Del_DLL_DuelComMovePhase>(DLL_DuelComMovePhase, PInvoke.GetProcAddress(lib, "DLL_DuelComMovePhase"));
            hookDLL_DuelComDoCommand = new Hook<Del_DLL_DuelComDoCommand>(DLL_DuelComDoCommand, PInvoke.GetProcAddress(lib, "DLL_DuelComDoCommand"));
            hookDLL_DuelComCancelCommand = new Hook<Del_DLL_DuelComCancelCommand>(DLL_DuelComCancelCommand, PInvoke.GetProcAddress(lib, "DLL_DuelComCancelCommand"));
            hookDLL_DuelComCancelCommand2 = new Hook<Del_DLL_DuelComCancelCommand2>(DLL_DuelComCancelCommand2, PInvoke.GetProcAddress(lib, "DLL_DuelComCancelCommand2"));
            hookDLL_DuelDlgSetResult = new Hook<Del_DLL_DuelDlgSetResult>(DLL_DuelDlgSetResult, PInvoke.GetProcAddress(lib, "DLL_DuelDlgSetResult"));
            hookDLL_DuelListSetCardExData = new Hook<Del_DLL_DuelListSetCardExData>(DLL_DuelListSetCardExData, PInvoke.GetProcAddress(lib, "DLL_DuelListSetCardExData"));
            hookDLL_DuelListSetIndex = new Hook<Del_DLL_DuelListSetIndex>(DLL_DuelListSetIndex, PInvoke.GetProcAddress(lib, "DLL_DuelListSetIndex"));
            hookDLL_DuelListInitString = new Hook<Del_DLL_DuelListInitString>(DLL_DuelListInitString, PInvoke.GetProcAddress(lib, "DLL_DuelListInitString"));
            hookDLL_DuelSysAct = new Hook<Del_DLL_DuelSysAct>(DLL_DuelSysAct, PInvoke.GetProcAddress(lib, "DLL_DuelSysAct"));
            hookDLL_DuelSetPlayerType = new Hook<Del_DLL_DuelSetPlayerType>(DLL_DuelSetPlayerType, PInvoke.GetProcAddress(lib, "DLL_DuelSetPlayerType"));

            DLL_DuelComCheatCard = Utils.GetFunc<Del_DLL_DuelComCheatCard>(PInvoke.GetProcAddress(lib, "DLL_DuelComCheatCard"));
            DLL_DuelComDoDebugCommand = Utils.GetFunc<Del_DLL_DuelComDoDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDoDebugCommand"));
            DLL_DuelComDebugCommand = Utils.GetFunc<Del_DLL_DuelComDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDebugCommand"));
            DLL_DuelWhichTurnNow = Utils.GetFunc<Del_DLL_DuelWhichTurnNow>(PInvoke.GetProcAddress(lib, "DLL_DuelWhichTurnNow"));
            DLL_DuelGetTurnNum = Utils.GetFunc<Del_DLL_DuelGetTurnNum>(PInvoke.GetProcAddress(lib, "DLL_DuelGetTurnNum"));
            DLL_DuelMyself = Utils.GetFunc<Del_DLL_DuelMyself>(PInvoke.GetProcAddress(lib, "DLL_DuelMyself"));

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

        public static void OnDuelBegin(GameMode gameMode)
        {
            LogToFile(string.Empty, false);
            lock (DuelComMessageQueue)
            {
                DuelComMessageQueue.Clear();
            }
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Clear();
            }
            LocalIsBusyEffect.Clear();
            RemoteIsBusyEffect.Clear();
            ReplayData.Clear();
            HasOpponentSurrendered = false;
            HasNetworkError = false;
            HasDuelStart = false;
            HasDuelEnd = false;
            BeginDuelTime = DateTime.UtcNow;
            RunEffectSeq = 0;
            AddRecordSeq = 0;
            IsPvpDuel = Program.NetClient != null && gameMode == GameMode.Room;
            IsPvpSpectator = Program.NetClient != null && gameMode == GameMode.Audience;
            MyID = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.MyID");
            SendLiveRecordData = YgomSystem.Utility.ClientWork.GetByJsonPath<bool>("Duel.SendLiveRecordData");

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
        }

        public static void OnDuelEnd()
        {
            PvpSpectatorRapidState = PvpSpectatorRapidState.None;
            engineInstance = IntPtr.Zero;
            engineInstanceReplayStream = IntPtr.Zero;
            activePlayerFieldEffectInstance = IntPtr.Zero;
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

        static int DLL_SetWorkMemory(IntPtr pWork)
        {
            if (pWork != IntPtr.Zero)
            {
                WorkMemory = pWork;
            }
            return hookDLL_SetWorkMemory.Original(pWork);
        }

        static void DLL_SetEffectDelegate(IntPtr runEffect, IntPtr isBusyEffect)
        {
            originalRunEffect = Utils.GetFunc<Del_RunEffect>(runEffect);
            originalIsBusyEffect = Utils.GetFunc<Del_IsBusyEffect>(isBusyEffect);
            hookDLL_SetEffectDelegate.Original(Marshal.GetFunctionPointerForDelegate(myRunEffect), Marshal.GetFunctionPointerForDelegate(myIsBusyEffect));
        }

        static Del_AddRecord AddRecord = (IntPtr ptr, int size) =>
        {
            AddRecordSeq++;
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
                Log("SetGuideEnable:" + thisPtr + " near:" + near + " enable:" + enable + " turnchange:" + turnchange);
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

        static int InjectDuelEnd()
        {
            HasDuelEnd = true;
            if (IsPvpSpectator)
            {
                EndSpectatorReplayStream();
            }
            if (HasOpponentSurrendered)
            {
                return originalRunEffect((int)DuelViewType.DuelEnd, (int)DuelResultType.Win, (int)DuelFinishType.Surrender, 0);
            }
            else if (HasNetworkError)
            {
                return originalRunEffect((int)DuelViewType.DuelEnd, (int)DuelResultType.Draw, (int)DuelFinishType.FinishError, 0);
            }
            return 0;
        }

        static int RunEffect(int id, int param1, int param2, int param3)
        {
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
                        HasDuelStart = true;
                        break;
                    case DuelViewType.DuelEnd:
                        HasDuelEnd = true;
                        EndSpectatorReplayStream();
                        break;
                }
            }
            else if (IsPvpDuel)
            {
                if (HasNetworkError || HasOpponentSurrendered)
                {
                    if (HasDuelStart)
                    {
                        return InjectDuelEnd();
                    }
                    else
                    {
                        if ((DuelViewType)id == DuelViewType.DuelStart)
                        {
                            HasDuelStart = true;
                        }
                        originalRunEffect(id, param1, param2, param3);
                        return InjectDuelEnd();
                    }
                }

                LocalIsBusyEffect.Clear();
                RemoteIsBusyEffect.Remove(RunEffectSeq);
                RunEffectSeq++;
                Log("RunEffect " + (DuelViewType)id + " " + param1 + " " + param2 + " " + param3 + " whichTurn:" + DLL_DuelWhichTurnNow() + " myID:" + MyID + " seq:" + RunEffectSeq);

                switch ((DuelViewType)id)
                {
                    case DuelViewType.DuelStart:
                        HasDuelStart = true;
                        break;
                    case DuelViewType.DuelEnd:
                        HasDuelEnd = true;
                        break;
                    case DuelViewType.WaitInput:
                        if (ActiveUserIdForDoCommand != MyID)
                        {
                            originalRunEffect((int)DuelViewType.CpuThinking, 0, 0, 0);
                            Log("Ignore " + (DuelViewType)id + " player:" + ActiveUserIdForDoCommand);
                            return 0;
                        }
                        break;
                    case DuelViewType.RunDialog:
                        if (ActiveUserIdForDoCommand != MyID)
                        {
                            originalRunEffect((int)DuelViewType.CpuThinking, 0, 0, 0);
                            Log("Ignore " + (DuelViewType)id + " player:" + ActiveUserIdForDoCommand);
                            Dictionary<DuelViewType, int> remoteIsBusyEffect;
                            if (!RemoteIsBusyEffect.TryGetValue(RunEffectSeq, out remoteIsBusyEffect))
                            {
                                RemoteIsBusyEffect[RunEffectSeq] = remoteIsBusyEffect = new Dictionary<DuelViewType, int>();
                            }
                            if (!remoteIsBusyEffect.ContainsKey((DuelViewType)id))
                            {
                                remoteIsBusyEffect[(DuelViewType)id] = 1;
                            }
                            return 0;
                        }
                        break;
                    case DuelViewType.RunList:
                        if (param1 != MyID)
                        {
                            originalRunEffect((int)DuelViewType.CpuThinking, 0, 0, 0);
                            return 0;
                        }
                        break;
                }
            }
            return originalRunEffect(id, param1, param2, param3);
        }

        static int IsBusyEffect(int id)
        {
            if (HasNetworkError || HasOpponentSurrendered)
            {
                return originalIsBusyEffect(id);
            }

            int result = IsBusyEffectImpl(id);

            /*string remoteStatus = "?";

            Dictionary<DuelViewType, int> remoteIsBusyEffect;
            if (RemoteIsBusyEffect.TryGetValue(RunEffectSeq, out remoteIsBusyEffect))
            {
                int remoteResult;
                if (remoteIsBusyEffect.TryGetValue((DuelViewType)id, out remoteResult))
                {
                    remoteStatus = remoteResult.ToString();
                }
                else
                {
                    remoteStatus = "1-(not_found)";
                }
            }
            else
            {
                remoteStatus = "1-(none)";
            }

            string localStatus = "?";

            int localResult;
            if (LocalIsBusyEffect.TryGetValue((DuelViewType)id, out localResult))
            {
                localStatus = localResult.ToString();
            }
            else
            {
                localStatus = "(error)";
            }

            LogToFile("IsBusyEffect id:" + (DuelViewType)id + " value:" + result + " local:" + localResult + " remote:" + remoteStatus);*/

            return result;
        }

        static int IsBusyEffectImpl(int id)
        {
            int result = originalIsBusyEffect(id);
            //Log("IsBusyEffect id:" + (DuelViewType)id + " result:" + result);

            if (IsPvpDuel)
            {
                int prevResult;
                if (!LocalIsBusyEffect.TryGetValue((DuelViewType)id, out prevResult) || result != prevResult)
                {
                    Log("Send UpdateIsBusyEffect seq:" + RunEffectSeq + " id:" + (DuelViewType)id + " value:" + result);
                    LocalIsBusyEffect[(DuelViewType)id] = result;
                    Program.NetClient.Send(new UpdateIsBusyEffectMessage()
                    {
                        Seq = RunEffectSeq,
                        Id = (DuelViewType)id,
                        Value = result
                    });
                }

                if (result == 0)
                {
                    Dictionary<DuelViewType, int> remoteIsBusyEffect;
                    int remoteResult;
                    if (!RemoteIsBusyEffect.TryGetValue(RunEffectSeq, out remoteIsBusyEffect) ||
                        !remoteIsBusyEffect.TryGetValue((DuelViewType)id, out remoteResult))
                    {
                        if (RemoteIsBusyEffect.ContainsKey(RunEffectSeq + 1))
                        {
                            // Safe to assume 0 as the remote player is already one step ahead
                            return 0;
                        }
                        return 1;
                    }
                    return remoteResult;
                }
            }

            return result;
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

                if ((HasNetworkError || HasOpponentSurrendered) && HasDuelStart)
                {
                    return 1;
                }

                if (DuelComMessageQueue.Count > 0)
                {
                    lock (DuelComMessageQueue)
                    {
                        if (DuelComMessageQueue.Count > 0)
                        {
                            DuelComMessage duelComMessage = DuelComMessageQueue.Peek();

                            if (RunEffectSeq >= duelComMessage.RunEffectSeq)
                            {
                                if (RunEffectSeq != duelComMessage.RunEffectSeq)
                                {
                                    // Our run effect sequence is beyond the other client
                                    Log("[IGNORE] " + duelComMessage.Type + " RunEffectSeq: " + RunEffectSeq + " msgRunEffectSeq: " + duelComMessage.RunEffectSeq);
                                }
                                else
                                {
                                    HandleDuelComMessage(duelComMessage);
                                }

                                DuelComMessageQueue.Dequeue();
                            }
                        }
                    }
                }
            }
            return hookDLL_DuelSysAct.Original();
        }

        static void DLL_DuelComMovePhase(int phase)
        {
            Log("DLL_DuelComMovePhase phase:" + phase + " seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
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
            Log("DLL_DuelComDoCommand player:" + player + " pos:" + position + " indx:" + index + " cmd:" + commandId + " seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
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
            Log("DLL_DuelComCancelCommand seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
                Program.NetClient.Send(new DuelComCancelCommandMessage()
                {
                    RunEffectSeq = RunEffectSeq
                });
            }

            return hookDLL_DuelComCancelCommand.Original();
        }

        static int DLL_DuelComCancelCommand2(bool decide)
        {
            Log("DLL_DuelComCancelCommand2 seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
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
            Log("DLL_DuelDlgSetResult result:" + result + " seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
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
            Log("DLL_DuelListSetCardExData index:" + index + " data:" + data + " seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
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
            Log("DLL_DuelListSetIndex index:" + index + "seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
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
            Log("DLL_DuelListInitString seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
                Program.NetClient.Send(new DuelListInitStringMessage()
                {
                    RunEffectSeq = RunEffectSeq
                });
            }

            hookDLL_DuelListInitString.Original();
        }

        static void DLL_DuelSetPlayerType(int player, int type)
        {
            if (IsPvpDuel)
            {
                type = (int)DuelPlayerType.Human;
            }
            hookDLL_DuelSetPlayerType.Original(player, type);
        }

        public static void HandleNetMessage(NetClient client, NetMessage message)
        {
            DuelComMessage duelComMessage = message as DuelComMessage;
            if (duelComMessage != null && IsPvpDuel)
            {
                lock (DuelComMessageQueue)
                {
                    DuelComMessageQueue.Enqueue(duelComMessage);
                }
            }

            switch (message.Type)
            {
                case NetMessageType.ConnectionResponse: OnConnectionResponse(client, (ConnectionResponseMessage)message); break;
                case NetMessageType.Ping: OnPing(client, (PingMessage)message); break;
                case NetMessageType.DuelError: OnDuelError(client, (DuelErrorMessage)message); break;
                case NetMessageType.UpdateIsBusyEffect: OnUpdateIsBusyEffect(client, (UpdateIsBusyEffectMessage)message); break;
                case NetMessageType.OpponentSurrendered: OnOpponentSurrendered(client, (OpponentSurrenderedMessage)message); break;
                case NetMessageType.OpponentDuelEnded: OnOpponentDuelEnded(client, (OpponentDuelEndedMessage)message); break;
                case NetMessageType.DuelSpectatorData: OnDuelSpectatorData(client, (DuelSpectatorDataMessage)message); break;
                case NetMessageType.DuelSpectatorFieldGuide: OnDuelSpectatorFieldGuide(client, (DuelSpectatorFieldGuideMessage)message); break;
            }
        }

        static void OnConnectionResponse(NetClient client, ConnectionResponseMessage message)
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
                if (HasNetworkError || HasOpponentSurrendered || HasDuelEnd ||
                    (YgomGame.Duel.DuelClient.Step != DuelClientStep.ExecDuel && !IsPvpSpectator))
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
                    if (HasNetworkError || HasOpponentSurrendered || HasDuelEnd ||
                        (YgomGame.Duel.DuelClient.Step != DuelClientStep.ExecDuel && !IsPvpSpectator))
                    {
                        return;
                    }
                    HasNetworkError = true;
                    Log("OnNetworkError");
                    if (IsPvpSpectator && !HasDuelStart)
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

        static void OnPing(NetClient client, PingMessage message)
        {
            // TODO: Send some info stating if we're in a duel
            client.Send(new PongMessage()
            {
                ServerToClientLatency = Utils.GetEpochTime() - Utils.GetEpochTime(message.RequestTime),
                ResponseTime = DateTime.UtcNow,
            });

            if (message.DuelingState != DuelRoomTableState.Dueling && IsPvpDuel &&
                !HasNetworkError && !HasOpponentSurrendered && !HasDuelEnd &&
                BeginDuelTime < DateTime.UtcNow - TimeSpan.FromSeconds(5) &&
                YgomGame.Duel.DuelClient.Instance != IntPtr.Zero)
            {
                OnNetworkError();
            }
        }

        static void OnDuelError(NetClient client, DuelErrorMessage message)
        {
            if (IsPvpDuel || IsPvpSpectator)
            {
                OnNetworkError();
            }
        }

        static void OnOpponentSurrendered(NetClient client, OpponentSurrenderedMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(() =>
                {
                    if (!HasOpponentSurrendered && !HasDuelEnd && !HasNetworkError && YgomGame.Duel.DuelClient.Instance != IntPtr.Zero)
                    {
                        Log("OnOpponentSurrendered");
                        HasOpponentSurrendered = true;
                        InjectDuelEnd();
                    }
                });
            }
        }

        static void OnOpponentDuelEnded(NetClient client, OpponentDuelEndedMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            HasDuelEnd = true;
        }

        static void OnUpdateIsBusyEffect(NetClient client, UpdateIsBusyEffectMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            if (message.Seq < RunEffectSeq)
            {
                Log("Ignore recv UpdateIsBusyEffect seq:" + message.Seq + " id:" + (DuelViewType)message.Id + " value:" + message.Value);
                return;
            }
            Action action = () =>
            {
                Log("Recv UpdateIsBusyEffect seq:" + message.Seq + " id:" + (DuelViewType)message.Id + " value:" + message.Value);
                if (message.Seq < RunEffectSeq)
                {
                    return;
                }
                Dictionary<DuelViewType, int> remoteIsBusyEffect;
                if (!RemoteIsBusyEffect.TryGetValue(message.Seq, out remoteIsBusyEffect))
                {
                    RemoteIsBusyEffect[message.Seq] = remoteIsBusyEffect = new Dictionary<DuelViewType, int>();
                }
                remoteIsBusyEffect[message.Id] = message.Value;
            };
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(action);
            }
        }

        static void OnDuelSpectatorData(NetClient client, DuelSpectatorDataMessage message)
        {
            Action action = () =>
            {
                if (message.Buffer != null && message.Buffer.Length > 0)
                {
                    IntPtr replayStream = GetReplayStream();
                    if (replayStream != IntPtr.Zero)
                    {
                        IL2Array<byte> buffer = new IL2Array<byte>(message.Buffer.Length, IL2SystemClass.Byte);
                        buffer.CopyFrom(message.Buffer);
                        methodReplayStreamAdd.Invoke(replayStream, new IntPtr[] { buffer.ptr });
                    }
                }
            };
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(action);
            }
        }

        static void OnDuelSpectatorFieldGuide(NetClient client, DuelSpectatorFieldGuideMessage message)
        {
            Action action = () =>
            {
                IsFieldGuideNear = message.Near;
                UpdateFieldGuide();
            };
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Add(action);
            }
        }

        static void HandleDuelComMessage(DuelComMessage message)
        {
            switch (message.Type)
            {
                case NetMessageType.DuelComMovePhase: OnDuelComMovePhase((DuelComMovePhaseMessage)message); break;
                case NetMessageType.DuelComDoCommand: OnDuelComDoCommand((DuelComDoCommandMessage)message); break;
                case NetMessageType.DuelComCancelCommand: OnDuelComCancelCommand((DuelComCancelCommandMessage)message); break;
                case NetMessageType.DuelComCancelCommand2: OnDuelComCancelCommand2((DuelComCancelCommand2Message)message); break;
                case NetMessageType.DuelDlgSetResult: OnDuelDlgSetResult((DuelDlgSetResultMessage)message); break;
                case NetMessageType.DuelListSetCardExData: OnDuelListSetCardExData((DuelListSetCardExDataMessage)message); break;
                case NetMessageType.DuelListSetIndex: OnDuelListSetIndex((DuelListSetIndexMessage)message); break;
                case NetMessageType.DuelListInitString: OnDuelListInitString((DuelListInitStringMessage)message); break;
            }
        }

        static void OnDuelComMovePhase(DuelComMovePhaseMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelComMovePhase phase:" + message.Phase + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComMovePhase.Original(message.Phase);
        }

        static void OnDuelComDoCommand(DuelComDoCommandMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelComDoCommand player:" + message.Player + " pos:" + message.Position + " indx:" + message.Index + " cmd:" + message.CommandId + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComDoCommand.Original(message.Player, message.Position, message.Index, message.CommandId);
        }

        static void OnDuelComCancelCommand(DuelComCancelCommandMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelComCancelCommand seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComCancelCommand.Original();
        }

        static void OnDuelComCancelCommand2(DuelComCancelCommand2Message message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelComCancelCommand2 decide:" + message.Decide + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComCancelCommand2.Original(message.Decide);
        }

        static void OnDuelDlgSetResult(DuelDlgSetResultMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelDlgSetResult result:" + message.Result + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelDlgSetResult.Original(message.Result);
        }

        static void OnDuelListSetCardExData(DuelListSetCardExDataMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelListSetCardExData index:" + message.Index + " data:" + message.Data + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelListSetCardExData.Original(message.Index, message.Data);
        }

        static void OnDuelListSetIndex(DuelListSetIndexMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelListSetIndex index:" + message.Index + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelListSetIndex.Original(message.Index);
        }

        static void OnDuelListInitString(DuelListInitStringMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Log("OnDuelListInitString seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelListInitString.Original();
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