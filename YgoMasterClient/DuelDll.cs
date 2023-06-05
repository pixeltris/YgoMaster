using IL2CPP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YgoMaster.Net.Message;
using YgoMaster.Net;
using YgoMaster;
using System.Windows.Forms;

namespace YgoMasterClient
{
    unsafe static class DuelDll
    {
        public static Queue<DuelComMessage> DuelComMessageQueue = new Queue<DuelComMessage>();

        public static int LastIsBusyEffectSyncSeq;
        public static Dictionary<DuelViewType, int> LocalIsBusyEffect = new Dictionary<DuelViewType, int>();
        public static Dictionary<ulong, Dictionary<DuelViewType, int>> RemoteIsBusyEffect = new Dictionary<ulong, Dictionary<DuelViewType, int>>();

        public static List<Action> ActionsToRunInNextSysAct = new List<Action>();

        public static int LastIsBusyEffectId = -1;
        public static ulong RunEffectSeq;
        public static bool IsPvpDuel;
        public static int MyID;
        public static int RivalID
        {
            get { return MyID == 0 ? 1 : 0; }
        }

        static IntPtr WorkMemory;

        public delegate int Del_RunEffect(int id, int param1, int param2, int param3);
        static Del_RunEffect myRunEffect = RunEffect;
        static Del_RunEffect originalRunEffect;

        public delegate int Del_IsBusyEffect(int id);
        static Del_IsBusyEffect myIsBusyEffect = IsBusyEffect;
        static Del_IsBusyEffect originalIsBusyEffect;

        public delegate void Del_DLL_SetEffectDelegate(IntPtr runEffct, IntPtr isBusyEffect);
        static Hook<Del_DLL_SetEffectDelegate> hookDLL_SetEffectDelegate;

        public delegate int Del_DLL_SetWorkMemory(IntPtr pWork);
        static Hook<Del_DLL_SetWorkMemory> hookDLL_SetWorkMemory;

        delegate void Del_DLL_DuelComMovePhase(int phase);
        static Hook<Del_DLL_DuelComMovePhase> hookDLL_DuelComMovePhase;

        delegate void Del_DLL_DuelComDoCommand(int player, int position, int index, int commandId);
        static Hook<Del_DLL_DuelComDoCommand> hookDLL_DuelComDoCommand;

        delegate int Del_DLL_DuelComCancelCommand();
        static Hook<Del_DLL_DuelComCancelCommand> hookDLL_DuelComCancelCommand;

        delegate int Del_DLL_DuelComCancelCommand2(bool decide);
        static Hook<Del_DLL_DuelComCancelCommand2> hookDLL_DuelComCancelCommand2;

        delegate int Del_DLL_DuelGetCardFace(int player, int position, int index);
        static Hook<Del_DLL_DuelGetCardFace> hookDLL_DuelGetCardFace;

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

        public delegate int Del_DLL_DuelGetLP(int player);
        public static Del_DLL_DuelGetLP DLL_DuelGetLP;

        static DuelDll()
        {
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
            hookDLL_DuelGetCardFace = new Hook<Del_DLL_DuelGetCardFace>(DLL_GetCardFace, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardFace"));

            DLL_DuelComCheatCard = Utils.GetFunc<Del_DLL_DuelComCheatCard>(PInvoke.GetProcAddress(lib, "DLL_DuelComCheatCard"));
            DLL_DuelComDoDebugCommand = Utils.GetFunc<Del_DLL_DuelComDoDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDoDebugCommand"));
            DLL_DuelComDebugCommand = Utils.GetFunc<Del_DLL_DuelComDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDebugCommand"));
            DLL_DuelWhichTurnNow = Utils.GetFunc<Del_DLL_DuelWhichTurnNow>(PInvoke.GetProcAddress(lib, "DLL_DuelWhichTurnNow"));
            DLL_DuelGetLP = Utils.GetFunc<Del_DLL_DuelGetLP>(PInvoke.GetProcAddress(lib, "DLL_DuelGetLP"));
        }

        public static void OnDuelBegin(GameMode gameMode)
        {
            lock (DuelComMessageQueue)
            {
                DuelComMessageQueue.Clear();
            }
            lock (ActionsToRunInNextSysAct)
            {
                ActionsToRunInNextSysAct.Clear();
            }
            LastIsBusyEffectSyncSeq = 0;
            LocalIsBusyEffect.Clear();
            RemoteIsBusyEffect.Clear();
            LastIsBusyEffectId = -1;
            RunEffectSeq = 0;
            IsPvpDuel = Program.NetClient != null && gameMode == GameMode.Room;
            MyID = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("Duel.MyID");
        }

        static int DLL_SetWorkMemory(IntPtr pWork)
        {
            if (pWork != IntPtr.Zero)
            {
                WorkMemory = pWork;
            }
            return hookDLL_SetWorkMemory.Original(pWork);
        }

        static void DLL_SetEffectDelegate(IntPtr runEffct, IntPtr isBusyEffect)
        {
            originalRunEffect = Utils.GetFunc<Del_RunEffect>(runEffct);
            originalIsBusyEffect = Utils.GetFunc<Del_IsBusyEffect>(isBusyEffect);
            hookDLL_SetEffectDelegate.Original(Marshal.GetFunctionPointerForDelegate(myRunEffect), Marshal.GetFunctionPointerForDelegate(myIsBusyEffect));
        }

        static int RunEffect(int id, int param1, int param2, int param3)
        {
            LocalIsBusyEffect.Clear();
            RemoteIsBusyEffect.Remove(RunEffectSeq);
            RunEffectSeq++;
            Console.WriteLine("RunEffect " + (DuelViewType)id + " " + param1 + " " + param2 + " " + param3 + " lp0:" + DLL_DuelGetLP(0) + " lp1:" + DLL_DuelGetLP(1) + " whichTurn:" + DLL_DuelWhichTurnNow() + " myID:" + MyID + " seq:" + RunEffectSeq);
            if (IsPvpDuel)
            {
                switch ((DuelViewType)id)
                {
                    case DuelViewType.WaitInput:
                        if (*(int*)(WorkMemory + ClientSettings.DuelDllActiveUserDoCommandOffset) != MyID)
                        {
                            Console.WriteLine("Ignore " + (DuelViewType)id + " player:" + *(int*)(WorkMemory + ClientSettings.DuelDllActiveUserDoCommandOffset));
                            return 0;
                        }
                        break;
                    case DuelViewType.RunDialog:
                        if (*(int*)(WorkMemory + ClientSettings.DuelDllActiveUserDoCommandOffset) != MyID)
                        {
                            Console.WriteLine("Ignore " + (DuelViewType)id + " player:" + *(int*)(WorkMemory + ClientSettings.DuelDllActiveUserDoCommandOffset));
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
                            return 0;
                        }
                        break;
                }
            }
            return originalRunEffect(id, param1, param2, param3);
        }

        static int IsBusyEffect(int id)
        {
            int result = originalIsBusyEffect(id);
            //Console.WriteLine("IsBusyEffect id:" + (DuelViewType)id + " result:" + result);

            /*if (LastIsBusyEffectId != id)
            {
                LastIsBusyEffectId = id;
                Console.WriteLine("IsBusyEffect id:" + (DuelViewType)id + " result:" + result);
            }
            if (result == 0)
            {
                LastIsBusyEffectId = -1;
            }*/

            if (IsPvpDuel)
            {
                int prevResult;
                if (!LocalIsBusyEffect.TryGetValue((DuelViewType)id, out prevResult) || result != prevResult)
                {
                    Console.WriteLine("Send UpdateIsBusyEffect seq:" + RunEffectSeq + " id:" + (DuelViewType)id + " value:" + result);
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
                    if (!RemoteIsBusyEffect.TryGetValue(RunEffectSeq, out remoteIsBusyEffect))
                    {
                        return 1;
                    }

                    int remoteResult;
                    if (!remoteIsBusyEffect.TryGetValue((DuelViewType)id, out remoteResult))
                    {
                        /*if (remoteIsBusyEffect.Count > 0)
                        {
                            switch ((DuelViewType)id)
                            {
                                case DuelViewType.Null:
                                case DuelViewType.Noop:
                                    foreach (int value in remoteIsBusyEffect.Values)
                                    {
                                        if (value != 0)
                                        {
                                            return 1;
                                        }
                                    }
                                    break;
                            }
                            return 0;
                        }*/
                        return 1;
                    }

                    return remoteResult;
                }
            }

            return result;
        }

        public static int DLL_DuelSysAct()
        {
            if (IsPvpDuel)
            {
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
                                    Console.WriteLine("[IGNORE] " + duelComMessage.Type + " RunEffectSeq: " + RunEffectSeq + " msgRunEffectSeq: " + duelComMessage.RunEffectSeq);
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
            Console.WriteLine("DLL_DuelComMovePhase phase:" + phase + " seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelComDoCommand player:" + player + " pos:" + position + " indx:" + index + " cmd:" + commandId + " seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelComCancelCommand seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelComCancelCommand2 seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelDlgSetResult result:" + result + " seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelListSetCardExData index:" + index + " data:" + data + " seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelListSetIndex index:" + index + "seq:" + RunEffectSeq);
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
            Console.WriteLine("DLL_DuelListInitString seq:" + RunEffectSeq);
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
                hookDLL_DuelSetPlayerType.Original(player, (int)DuelPlayerType.Human);
            }
            else
            {
                hookDLL_DuelSetPlayerType.Original(player, type);
            }
        }

        static int DLL_GetCardFace(int player, int position, int index)
        {
            //return 1;
            return hookDLL_DuelGetCardFace.Original(player, position, index);
        }

        public static void HandleNetMessage(NetClient client, NetMessage message)
        {
            DuelComMessage duelComMessage = message as DuelComMessage;
            if (duelComMessage != null)
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
            }
        }

        static void OnConnectionResponse(NetClient client, ConnectionResponseMessage message)
        {
            if (!message.Success)
            {
                Console.WriteLine("Session server failed to validate token '" + ClientSettings.MultiplayerToken + "'");
            }
        }

        static void OnPing(NetClient client, PingMessage message)
        {
            if (message.DuelingState != DuelRoomTableState.Dueling)
            {
                // TODO: Check if actively dueling and stop the duel (network error)
            }

            client.Send(new PongMessage()
            {
                ServerToClientLatency = Utils.GetEpochTime() - Utils.GetEpochTime(message.RequestTime),
                ResponseTime = DateTime.UtcNow
            });
        }

        static void OnDuelError(NetClient client, DuelErrorMessage message)
        {
            Console.WriteLine("TODO: OnDuelError");
        }

        static void OnUpdateIsBusyEffect(NetClient client, UpdateIsBusyEffectMessage message)
        {
            if (message.Seq < RunEffectSeq)
            {
                Console.WriteLine("Ignore recv UpdateIsBusyEffect seq:" + message.Seq + " id:" + (DuelViewType)message.Id + " value:" + message.Value);
                return;
            }
            Action action = () =>
            {
                Console.WriteLine("Recv UpdateIsBusyEffect seq:" + message.Seq + " id:" + (DuelViewType)message.Id + " value:" + message.Value);
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
            Console.WriteLine("OnDuelComMovePhase phase:" + message.Phase + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComMovePhase.Original(message.Phase);
        }

        static void OnDuelComDoCommand(DuelComDoCommandMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelComDoCommand player:" + message.Player + " pos:" + message.Position + " indx:" + message.Index + " cmd:" + message.CommandId + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComDoCommand.Original(message.Player, message.Position, message.Index, message.CommandId);
        }

        static void OnDuelComCancelCommand(DuelComCancelCommandMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelComCancelCommand seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComCancelCommand.Original();
        }

        static void OnDuelComCancelCommand2(DuelComCancelCommand2Message message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelComCancelCommand2 decide:" + message.Decide + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelComCancelCommand2.Original(message.Decide);
        }

        static void OnDuelDlgSetResult(DuelDlgSetResultMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelDlgSetResult result:" + message.Result + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelDlgSetResult.Original(message.Result);
        }

        static void OnDuelListSetCardExData(DuelListSetCardExDataMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelListSetCardExData index:" + message.Index + " data:" + message.Data + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelListSetCardExData.Original(message.Index, message.Data);
        }

        static void OnDuelListSetIndex(DuelListSetIndexMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelListSetIndex index:" + message.Index + " seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelListSetIndex.Original(message.Index);
        }

        static void OnDuelListInitString(DuelListInitStringMessage message)
        {
            if (!IsPvpDuel)
            {
                return;
            }
            Console.WriteLine("OnDuelListInitString seq:" + RunEffectSeq + " mseq:" + message.RunEffectSeq);
            hookDLL_DuelListInitString.Original();
        }
    }
}