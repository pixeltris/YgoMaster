using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using YgoMaster.Net;
using YgoMaster.Net.Message;

namespace YgoMaster
{
    class PvpEngineState
    {
        const int PosNum = 18;
        const int PosField = 12;
        const int PosHand = 13;
        const int PosGrave = 16;
        const int PosSelect = 18;

        public static bool SendEntireState = false;

        public ulong RunEffectSeq;
        public DuelViewType ViewType;
        public int Param1;
        public int Param2;
        public int Param3;
        public int RunDialogUser;
        public int DoCommandUser;

        public NetClient Client;
        public Dictionary<PvpOperationType, Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult>> Ops = new Dictionary<PvpOperationType, Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult>>();
        public Dictionary<DuelViewType, int> IsBusyEffect = new Dictionary<DuelViewType, int>();
        Stopwatch stopwatch = new Stopwatch();
        int callsThisCycle;
        int updatesThisCycle;

        public PvpEngineState()
        {
            foreach (PvpOperationType operationType in Enum.GetValues(typeof(PvpOperationType)))
            {
                Ops.Add(operationType, new Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult>());
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<PvpOperationType, Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult>> data in Ops)
            {
                data.Value.Clear();
            }
            IsBusyEffect.Clear();
        }

        bool IsLocatePos(int pos)
        {
            return pos < PosSelect;
        }

        bool IsFieldPos(int pos)
        {
            return pos <= PosField;
        }

        public void RunEffect(DuelViewType viewType, int param1, int param2, int param3, int doCommandUser, int runDialogUser)
        {
            RunEffectSeq++;
            ViewType = viewType;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            DoCommandUser = doCommandUser;
            RunDialogUser = runDialogUser;
            if (SendEntireState)
            {
                Clear();
            }
            else
            {
                IsBusyEffect.Clear();
            }

            callsThisCycle = 0;
            updatesThisCycle = 0;
            Console.WriteLine("RunEffect " + viewType + " " + param1 + " " + param2 + " " + param3);
            stopwatch.Restart();

            // TOOD:
            // DLL_DuelGetTrapMonstBasicVal
            // DLL_DuelIsThisNormalMonsterInHand

            int myself = Pvp.DLL_DuelMyself();
            int rival = Pvp.DLL_DuelRival();

            for (int player = 0; player < 2; player++)
            {
                int resultDataLen = Pvp.DLL_DuelResultGetData(player, null);
                if (resultDataLen > 0)
                {
                    byte[] resultData = new byte[resultDataLen * sizeof(int)];
                    Pvp.DLL_DuelResultGetData(player, resultData);
                    UpdateValue(PvpOperationType.DLL_DuelResultGetData, resultDataLen, resultData, player);
                }
                else
                {
                    UpdateValue(PvpOperationType.DLL_DuelResultGetData, 0, new byte[0]);
                }

                int memoLen = Pvp.DLL_DuelResultGetMemo(player, null);
                if (memoLen > 0)
                {
                    byte[] memo = new byte[memoLen * sizeof(int)];
                    Pvp.DLL_DuelResultGetMemo(player, memo);
                    UpdateValue(PvpOperationType.DLL_DuelResultGetMemo, memoLen, memo, player);
                }
                else
                {
                    UpdateValue(PvpOperationType.DLL_DuelResultGetMemo, 0, new byte[0]);
                }

                UpdateValue(PvpOperationType.DLL_DuelCanIDoPutMonster, (int)Pvp.DLL_DuelCanIDoPutMonster(player), player);
                UpdateValue(PvpOperationType.DLL_DuelCanIDoSpecialSummon, Pvp.DLL_DuelCanIDoSpecialSummon(player) ? 1 : 0, player);
                UpdateValue(PvpOperationType.DLL_DuelCanIDoSummonMonster, Pvp.DLL_DuelCanIDoSummonMonster(player) ? 1 : 0, player);
                UpdateValue(PvpOperationType.DLL_DuelGetCardInHand, (int)Pvp.DLL_DuelGetCardInHand(player), player);
                UpdateValue(PvpOperationType.DLL_DuelGetLP, Pvp.DLL_DuelGetLP(player), player);
                //UpdateValue(PvpOperationType.DLL_DuelIsHuman, Pvp.DLL_DuelIsHuman(player), player);
                //UpdateValue(PvpOperationType.DLL_DuelIsMyself, Pvp.DLL_DuelIsMyself(player), player);
                //UpdateValue(PvpOperationType.DLL_DuelIsRival, Pvp.DLL_DuelIsRival(player), player);

                for (int pos = 0; pos <= PosNum; pos++)
                {
                    UpdateValue(PvpOperationType.DLL_DuelGetAttackTargetMask, Pvp.DLL_DuelGetAttackTargetMask(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelGetTopCardIndex, Pvp.DLL_DuelGetTopCardIndex(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisCardExist, Pvp.DLL_DuelIsThisCardExist(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisContinuousCard, Pvp.DLL_DuelIsThisContinuousCard(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisEffectiveMonster, Pvp.DLL_DuelIsThisEffectiveMonster(player, pos) ? 1 : 0, player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisEquipCard, Pvp.DLL_DuelIsThisEquipCard(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisMagic, Pvp.DLL_DuelIsThisMagic(player, pos) ? 1 : 0, player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisNormalMonster, Pvp.DLL_DuelIsThisNormalMonster(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisTrap, Pvp.DLL_DuelIsThisTrap(player, pos) ? 1 : 0, player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisTrapMonster, Pvp.DLL_DuelIsThisTrapMonster(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisTunerMonster, Pvp.DLL_DuelIsThisTunerMonster(player, pos), player, pos);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisZoneAvailable, Pvp.DLL_DuelIsThisZoneAvailable(player, pos), player, pos);

                    UpdateValue(PvpOperationType.DLL_DuelIsThisZoneAvailable2, Pvp.DLL_DuelIsThisZoneAvailable2(player, pos, true), player, pos, 1);
                    UpdateValue(PvpOperationType.DLL_DuelIsThisZoneAvailable2, Pvp.DLL_DuelIsThisZoneAvailable2(player, pos, false), player, pos, 0);

                    //if (IsFieldPos(pos))
                    {
                        // NOTE: Assuming "index" is actually "locate" for DLL_DeulIsThisEffectiveMonsterWithDual
                        UpdateValue(PvpOperationType.DLL_DeulIsThisEffectiveMonsterWithDual, Pvp.DLL_DeulIsThisEffectiveMonsterWithDual(player, pos) ? 1 : 0, player, pos);
                        
                        for (int counter = 0; counter < (int)DuelCounterType.Max; counter++)
                        {
                            UpdateValue(PvpOperationType.DLL_DuelGetThisCardCounter, Pvp.DLL_DuelGetThisCardCounter(player, pos, counter), player, pos, counter);
                        }
                        
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardDirectFlag, Pvp.DLL_DuelGetThisCardDirectFlag(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardEffectFlags, Pvp.DLL_DuelGetThisCardEffectFlags(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardEffectIDAtChain, Pvp.DLL_DuelGetThisCardEffectIDAtChain(player, pos), player, pos);
                        
                        uint effectListLen = Pvp.DLL_DuelGetThisCardEffectList(player, pos, null);
                        if (effectListLen > 0)
                        {
                            byte[] effectListBuffer = new byte[effectListLen * sizeof(int)];
                            Pvp.DLL_DuelGetThisCardEffectList(player, pos, effectListBuffer);
                            UpdateValue(PvpOperationType.DLL_DuelGetThisCardEffectList, (int)effectListLen, effectListBuffer, player, pos);
                        }
                        else
                        {
                            UpdateValue(PvpOperationType.DLL_DuelGetThisCardEffectList, 0, new byte[0]);
                        }
                        
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardOverlayNum, Pvp.DLL_DuelGetThisCardOverlayNum(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardParameter, (int)Pvp.DLL_DuelGetThisCardParameter(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardShowParameter, (int)Pvp.DLL_DuelGetThisCardShowParameter(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetThisCardTurnCounter, (int)Pvp.DLL_DuelGetThisCardTurnCounter(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetThisMonsterFightableOnEffect, Pvp.DLL_DuelGetThisMonsterFightableOnEffect(player, pos) ? 1 : 0, player, pos);
                        
                        UpdateValue(PvpOperationType.DLL_DuelGetFldMonstOrgLevel, Pvp.DLL_DuelGetFldMonstOrgLevel(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetFldMonstOrgRank, Pvp.DLL_DuelGetFldMonstOrgRank(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetFldMonstOrgType, Pvp.DLL_DuelGetFldMonstOrgType(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetFldMonstRank, Pvp.DLL_DuelGetFldMonstRank(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetFldPendOrgScale, Pvp.DLL_DuelGetFldPendOrgScale(player, pos), player, pos);
                        UpdateValue(PvpOperationType.DLL_DuelGetFldPendScale, Pvp.DLL_DuelGetFldPendScale(player, pos), player, pos);

                        for (int i = 0; i < 3; i++)
                        {
                            int view_player = 0;
                            switch (i)
                            {
                                case 0:
                                    view_player = myself;
                                    break;
                                case 1:
                                    view_player = rival;
                                    break;
                                case 2:
                                    view_player = -1;
                                    break;
                            }
                            byte[] fldAffectBuffer = new byte[sizeof(short) * ((PosField + 1) * 2)];
                            Pvp.DLL_DuelGetFldAffectIcon(player, pos, fldAffectBuffer, view_player);
                            UpdateValue(PvpOperationType.DLL_DuelGetFldAffectIcon, 0, fldAffectBuffer, player, pos, view_player);
                        }
                    }

                    UpdateValue(PvpOperationType.DLL_DuelComGetCommandMask, (int)Pvp.DLL_DuelComGetCommandMask(player, pos, 0), player, pos, 0);
                    UpdateValue(PvpOperationType.DLL_DuelComGetTextIDOfThisCommand, (int)Pvp.DLL_DuelComGetTextIDOfThisCommand(player, pos, 0), player, pos, 0);
                    UpdateValue(PvpOperationType.DLL_DuelComGetTextIDOfThisSummon, (int)Pvp.DLL_DuelComGetTextIDOfThisSummon(player, pos, 0), player, pos, 0);

                    int numCards = UpdateValue(PvpOperationType.DLL_DuelGetCardNum, Pvp.DLL_DuelGetCardNum(player, pos), player, pos);
                    for (int index = 0; index <= numCards; index++)
                    {
                        int commandMask = UpdateValue(PvpOperationType.DLL_DuelComGetCommandMask, (int)Pvp.DLL_DuelComGetCommandMask(player, pos, index), player, pos, index);
                        UpdateValue(PvpOperationType.DLL_DuelComGetTextIDOfThisCommand, (int)Pvp.DLL_DuelComGetTextIDOfThisCommand(player, pos, index), player, pos, index);
                        UpdateValue(PvpOperationType.DLL_DuelComGetTextIDOfThisSummon, (int)Pvp.DLL_DuelComGetTextIDOfThisSummon(player, pos, index), player, pos, index);

                        int uid = UpdateValue(PvpOperationType.DLL_DuelGetCardUniqueID, Pvp.DLL_DuelGetCardUniqueID(player, pos, index), player, pos, index);

                        // We could do these once per uid that we see but cards could maybe be modified?
                        UpdateValue(PvpOperationType.DLL_CardRareGetRareByUniqueID, Pvp.DLL_CardRareGetRareByUniqueID(uid), uid);
                        UpdateValue(PvpOperationType.DLL_DuelGetCardIDByUniqueID2, (int)Pvp.DLL_DuelGetCardIDByUniqueID2(uid), uid);
                        UpdateValue(PvpOperationType.DLL_DuelSearchCardByUniqueID, (int)Pvp.DLL_DuelSearchCardByUniqueID(uid), uid);
                        
                        int functionMaterialListLen = Pvp.DLL_FusionGetMaterialList(uid, null);
                        if (functionMaterialListLen > 0)
                        {
                            byte[] functionMaterialList = new byte[functionMaterialListLen * sizeof(int)];
                            Pvp.DLL_FusionGetMaterialList(uid, functionMaterialList);
                            UpdateValue(PvpOperationType.DLL_FusionGetMaterialList, functionMaterialListLen, functionMaterialList, uid);
                        }
                        UpdateValue(PvpOperationType.DLL_FusionGetMonsterLevelInTuning, Pvp.DLL_FusionGetMonsterLevelInTuning(uid), uid);
                        UpdateValue(PvpOperationType.DLL_FusionIsThisTunedMonsterInTuning, Pvp.DLL_FusionIsThisTunedMonsterInTuning(uid), uid);
                        
                        for (int i = 0; i < 3; i++)
                        {
                            int view_player = 0;
                            switch (i)
                            {
                                case 0:
                                    view_player = myself;
                                    break;
                                case 1:
                                    view_player = rival;
                                    break;
                                case 2:
                                    view_player = -1;
                                    break;
                            }
                            UpdateValue(PvpOperationType.DLL_DuelListGetCardAttribute, Pvp.DLL_DuelListGetCardAttribute(view_player, uid), view_player, uid);
                        }
                        
                        unsafe
                        {
                            IntPtr prop = Pvp.DLL_DuelGetCardPropByUniqueID(uid);
                            int propValue = prop != IntPtr.Zero ? *(int*)prop : 0;
                            UpdateValue(PvpOperationType.DLL_DuelGetCardPropByUniqueID, propValue, uid);
                        }
                        
                        if (pos == PosHand)
                        {
                            for (int commandId = 0; commandId < (int)DuelCommandType.COUNT; commandId++)
                            {
                                int commandIdFlag = 1 << commandId;
                                if ((commandMask & commandIdFlag) != 0)
                                {
                                    UpdateValue(PvpOperationType.DLL_DUELCOMGetPosMaskOfThisHand, (int)Pvp.DLL_DUELCOMGetPosMaskOfThisHand(player, index, commandId), player, index, commandId);
                                }
                            }
                        
                            UpdateValue(PvpOperationType.DLL_DuelGetHandCardOpen, Pvp.DLL_DuelGetHandCardOpen(player, index) ? 1 : 0, player, index);
                        }

                        
                        PvpBasicVal basicVal = default(PvpBasicVal);
                        Pvp.DLL_DuelGetCardBasicVal(player, pos, index, ref basicVal);
                        UpdateValue(PvpOperationType.DLL_DuelGetCardBasicVal, 0, StructToByteArray(basicVal), player, pos, index);

                        UpdateValue(PvpOperationType.DLL_DuelGetCardFace, Pvp.DLL_DuelGetCardFace(player, pos, index), player, pos, index);
                        UpdateValue(PvpOperationType.DLL_DuelGetCardTurn, Pvp.DLL_DuelGetCardTurn(player, pos, index), player, pos, index);

                        if (pos == PosGrave)
                        {
                            UpdateValue(PvpOperationType.DLL_DuelIsThisNormalMonsterInGrave, Pvp.DLL_DuelIsThisNormalMonsterInGrave(player, index) ? 1 : 0, player, index);
                        }
                    }
                }
            }

            // Always get 1 mix num at minimum as some places get mix num without checking mix num
            int mixNum = Math.Max(1, UpdateValue(PvpOperationType.DLL_DuelDlgGetMixNum, Pvp.DLL_DuelDlgGetMixNum()));
            for (int i = 0; i < mixNum; i++)
            {
                UpdateValue(PvpOperationType.DLL_DuelDlgGetMixData, Pvp.DLL_DuelDlgGetMixData(i), i);
                UpdateValue(PvpOperationType.DLL_DuelDlgGetMixType, Pvp.DLL_DuelDlgGetMixType(i), i);
            }
            
            int selectItemNum = Math.Max(1, UpdateValue(PvpOperationType.DLL_DuelDlgGetSelectItemNum, Pvp.DLL_DuelDlgGetSelectItemNum()));
            for (int i = 0; i < selectItemNum; i++)
            {
                UpdateValue(PvpOperationType.DLL_DuelDlgGetSelectItemEnable, Pvp.DLL_DuelDlgGetSelectItemEnable(i), i);
                UpdateValue(PvpOperationType.DLL_DuelDlgGetSelectItemStr, Pvp.DLL_DuelDlgGetSelectItemStr(i), i);
            }

            int listItemNum = Math.Max(1, UpdateValue(PvpOperationType.DLL_DuelListGetItemMax, Pvp.DLL_DuelListGetItemMax()));
            for (int i = 0; i < listItemNum; i++)
            {
                UpdateValue(PvpOperationType.DLL_DuelListGetItemAttribute, Pvp.DLL_DuelListGetItemAttribute(i), i);
                UpdateValue(PvpOperationType.DLL_DuelListGetItemFrom, Pvp.DLL_DuelListGetItemFrom(i), i);
                UpdateValue(PvpOperationType.DLL_DuelListGetItemID, Pvp.DLL_DuelListGetItemID(i), i);
                UpdateValue(PvpOperationType.DLL_DuelListGetItemMsg, Pvp.DLL_DuelListGetItemMsg(i), i);
                UpdateValue(PvpOperationType.DLL_DuelListGetItemTargetUniqueID, Pvp.DLL_DuelListGetItemTargetUniqueID(i), i);
                UpdateValue(PvpOperationType.DLL_DuelListGetItemUniqueID, Pvp.DLL_DuelListGetItemUniqueID(i), i);
            }

            UpdateValue(PvpOperationType.DLL_DuelListGetSelectMax, Pvp.DLL_DuelListGetSelectMax());
            UpdateValue(PvpOperationType.DLL_DuelListGetSelectMin, Pvp.DLL_DuelListGetSelectMin());
            UpdateValue(PvpOperationType.DLL_DuelListIsMultiMode, Pvp.DLL_DuelListIsMultiMode());

            UpdateValue(PvpOperationType.DLL_DlgProcGetSummoningMonsterUniqueID, Pvp.DLL_DlgProcGetSummoningMonsterUniqueID());
            UpdateValue(PvpOperationType.DLL_DuelComGetMovablePhase, (int)Pvp.DLL_DuelComGetMovablePhase());
            UpdateValue(PvpOperationType.DLL_DUELCOMGetRecommendSide, Pvp.DLL_DUELCOMGetRecommendSide());
            UpdateValue(PvpOperationType.DLL_DuelDlgCanYesNoSkip, Pvp.DLL_DuelDlgCanYesNoSkip());
            UpdateValue(PvpOperationType.DLL_DuelDlgGetPosMaskOfThisSummon, Pvp.DLL_DuelDlgGetPosMaskOfThisSummon());
            UpdateValue(PvpOperationType.DLL_DuelGetCurrentDmgStep, (int)Pvp.DLL_DuelGetCurrentDmgStep());
            UpdateValue(PvpOperationType.DLL_DuelGetCurrentPhase, (int)Pvp.DLL_DuelGetCurrentPhase());
            UpdateValue(PvpOperationType.DLL_DuelGetCurrentStep, (int)Pvp.DLL_DuelGetCurrentStep());
            UpdateValue(PvpOperationType.DLL_DuelGetDuelFinish, Pvp.DLL_DuelGetDuelFinish());
            UpdateValue(PvpOperationType.DLL_DuelGetDuelFinishCardID, Pvp.DLL_DuelGetDuelFinishCardID());
            UpdateValue(PvpOperationType.DLL_DuelGetDuelFlagDeckReverse, Pvp.DLL_DuelGetDuelFlagDeckReverse() ? 1 : 0);
            UpdateValue(PvpOperationType.DLL_DuelGetDuelResult, Pvp.DLL_DuelGetDuelResult());
            UpdateValue(PvpOperationType.DLL_DuelGetTurnNum, (int)Pvp.DLL_DuelGetTurnNum());
            UpdateValue(PvpOperationType.DLL_DuelWhichTurnNow, Pvp.DLL_DuelWhichTurnNow());

            UpdateValue(PvpOperationType.DLL_DuelIsThisQuickDuel, Pvp.DLL_DuelIsThisQuickDuel());
            //UpdateValue(PvpOperationType.DLL_DuelIsReplayMode, Pvp.DLL_DuelIsReplayMode());

            // Added v2.2.1 for the "((!))" list view in the bottom right of the duel which shows the activation status of cards like "Maxx C"
            int attachedEffectListNum = Pvp.DLL_DuelGetAttachedEffectList(null);
            if (attachedEffectListNum > 0)
            {
                byte[] attachedEffectList = new byte[attachedEffectListNum * 8];//Engine.AffectProp - ushort(player),ushort(cardID),ushort(type),ushort(param)
                Pvp.DLL_DuelGetAttachedEffectList(attachedEffectList);
                UpdateValue(PvpOperationType.DLL_DuelGetAttachedEffectList, attachedEffectListNum, attachedEffectList);
            }
            else
            {
                UpdateValue(PvpOperationType.DLL_DuelGetAttachedEffectList, 0, null);
            }

            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                Write(bw);
                bw.Flush();
                buffer = ms.ToArray();
            }

            byte[] compressedBuffer = LZ4.Compress(buffer);
            Client.Send(new DuelEngineStateMessage()
            {
                RunEffectSeq = RunEffectSeq,
                ViewType = viewType,
                Param1 = param1,
                Param2 = param2,
                Param3 = param3,
                DoCommandUser = (byte)doCommandUser,
                RunDialogUser = (byte)runDialogUser,
                CompressedBuffer = compressedBuffer
            });

            Console.WriteLine("Calls:" + callsThisCycle + " updates:" + updatesThisCycle + " len:" + buffer.Length + " compressed:" + compressedBuffer.Length + " " + stopwatch.Elapsed);
        }

        int UpdateValue(PvpOperationType operationType, int value)
        {
            return UpdateValue(operationType, value, new PvpEngineOperationArgs(0));
        }

        int UpdateValue(PvpOperationType operationType, int value, int arg1)
        {
            return UpdateValue(operationType, value, new PvpEngineOperationArgs(arg1));
        }

        int UpdateValue(PvpOperationType operationType, int value, int arg1, int arg2)
        {
            return UpdateValue(operationType, value, new PvpEngineOperationArgs(arg1, arg2));
        }

        int UpdateValue(PvpOperationType operationType, int value, int arg1, int arg2, int arg3)
        {
            return UpdateValue(operationType, value, new PvpEngineOperationArgs(arg1, arg2, arg3));
        }

        void UpdateValue(PvpOperationType operationType, int value, byte[] data)
        {
            UpdateValue(operationType, value, data, new PvpEngineOperationArgs(0));
        }

        void UpdateValue(PvpOperationType operationType, int value, byte[] data, int arg1)
        {
            UpdateValue(operationType, value, data, new PvpEngineOperationArgs(arg1));
        }

        void UpdateValue(PvpOperationType operationType, int value, byte[] data, int arg1, int arg2)
        {
            UpdateValue(operationType, value, data, new PvpEngineOperationArgs(arg1, arg2));
        }

        void UpdateValue(PvpOperationType operationType, int value, byte[] data, int arg1, int arg2, int arg3)
        {
            UpdateValue(operationType, value, data, new PvpEngineOperationArgs(arg1, arg2, arg3));
        }

        int UpdateValue(PvpOperationType operationType, int value, PvpEngineOperationArgs args)
        {
            callsThisCycle++;
            PvpEngineOperationResult result = GetOrCreateResult(operationType, args);
            if (result.Value != value)
            {
                result.Value = value;
                result.HasChanged = true;
            }
            if (result.HasChanged)
            {
                updatesThisCycle++;
            }
            return value;
        }

        void UpdateValue(PvpOperationType operationType, int value, byte[] data, PvpEngineOperationArgs args)
        {
            callsThisCycle++;
            PvpEngineOperationResult result = GetOrCreateResult(operationType, args);
            if (result.Value != value || result.Data == null || (result.Data != null && data == null) || !data.SequenceEqual(result.Data))
            {
                updatesThisCycle++;
                result.Value = value;
                result.Data = data;
                result.HasChanged = true;
            }
            if (result.HasChanged)
            {
                updatesThisCycle++;
            }
        }

        public unsafe byte[] StructToByteArray<T>(T value) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            byte* ptr = stackalloc byte[buffer.Length];
            Marshal.StructureToPtr(value, (IntPtr)ptr, false);
            Marshal.Copy((IntPtr)ptr, buffer, 0, buffer.Length);
            return buffer;
        }

        public unsafe T StructFromByteArray<T>(byte[] value)
        {
            int structSize = Marshal.SizeOf(typeof(T));
            byte* ptr = stackalloc byte[structSize];
            Marshal.Copy(value, 0, (IntPtr)ptr, structSize);
            T result = (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            return result;
        }

        public void Update(PvpEngineState stateUpdate)
        {
            RunEffectSeq = stateUpdate.RunEffectSeq;
            ViewType = stateUpdate.ViewType;
            Param1 = stateUpdate.Param1;
            Param2 = stateUpdate.Param2;
            Param3 = stateUpdate.Param3;
            DoCommandUser = stateUpdate.DoCommandUser;
            RunDialogUser = stateUpdate.RunDialogUser;
            if (SendEntireState)
            {
                Clear();
            }
            foreach (KeyValuePair<PvpOperationType, Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult>> op in stateUpdate.Ops)
            {
                Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult> thisValues = Ops[op.Key];
                foreach (KeyValuePair<PvpEngineOperationArgs, PvpEngineOperationResult> value in op.Value)
                {
                    thisValues[value.Key] = value.Value;
                }
            }
            IsBusyEffect.Clear();
            foreach (KeyValuePair<DuelViewType, int> busyEffect in stateUpdate.IsBusyEffect)
            {
                IsBusyEffect[busyEffect.Key] = busyEffect.Value;
            }
        }

        public int GetValue(PvpOperationType operationType)
        {
            return GetValue(operationType, new PvpEngineOperationArgs(0));
        }

        public int GetValue(PvpOperationType operationType, int arg1)
        {
            return GetValue(operationType, new PvpEngineOperationArgs(arg1));
        }

        public int GetValue(PvpOperationType operationType, int arg1, int arg2)
        {
            return GetValue(operationType, new PvpEngineOperationArgs(arg1, arg2));
        }

        public int GetValue(PvpOperationType operationType, int arg1, int arg2, int arg3)
        {
            return GetValue(operationType, new PvpEngineOperationArgs(arg1, arg2, arg3));
        }

        public int GetValue(PvpOperationType operationType, PvpEngineOperationArgs args)
        {
            PvpEngineOperationResult result = GetResult(operationType, args);
            return result != null ? result.Value : 0;
        }

        public PvpEngineOperationResult GetResult(PvpOperationType operationType)
        {
            return GetResult(operationType, new PvpEngineOperationArgs(0));
        }

        public PvpEngineOperationResult GetResult(PvpOperationType operationType, int arg1)
        {
            return GetResult(operationType, new PvpEngineOperationArgs(arg1));
        }

        public PvpEngineOperationResult GetResult(PvpOperationType operationType, int arg1, int arg2)
        {
            return GetResult(operationType, new PvpEngineOperationArgs(arg1, arg2));
        }

        public PvpEngineOperationResult GetResult(PvpOperationType operationType, int arg1, int arg2, int arg3)
        {
            return GetResult(operationType, new PvpEngineOperationArgs(arg1, arg2, arg3));
        }

        public PvpEngineOperationResult GetResult(PvpOperationType operationType, PvpEngineOperationArgs args)
        {
            Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult> data = Ops[operationType];
            PvpEngineOperationResult result;
            lock (this)
            {
                data.TryGetValue(args, out result);
            }
            return result;
        }

        PvpEngineOperationResult GetOrCreateResult(PvpOperationType operationType, PvpEngineOperationArgs arg)
        {
            Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult> data = Ops[operationType];
            PvpEngineOperationResult result;
            if (!data.TryGetValue(arg, out result))
            {
                data[arg] = result = new PvpEngineOperationResult();
                if (SendEntireState)
                {
                    result.HasChanged = true;
                }
            }
            return result;
        }

        public void Read(byte[] compressedBuffer)
        {
            byte[] buffer = LZ4.Decompress(compressedBuffer);
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Read(reader);
            }
        }

        void Read(BinaryReader reader)
        {
            int numEntries = reader.ReadInt32();
            for (int i = 0; i < numEntries; i++)
            {
                PvpOperationType type = (PvpOperationType)reader.ReadByte();
                Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult> data = Ops[type];
                int count = reader.ReadInt32();
                for (int j = 0; j < count; j++)
                {
                    PvpEngineOperationArgs args = default(PvpEngineOperationArgs);
                    args.Read(reader);
                    PvpEngineOperationResult result = new PvpEngineOperationResult();
                    result.Read(reader);
                    data[args] = result;
                }
            }
        }

        void Write(BinaryWriter writer)
        {
            long offset = writer.BaseStream.Position;
            int numEntries = 0;
            writer.Write(numEntries);

            foreach (KeyValuePair<PvpOperationType, Dictionary<PvpEngineOperationArgs, PvpEngineOperationResult>> op in Ops)
            {
                IEnumerable<KeyValuePair<PvpEngineOperationArgs, PvpEngineOperationResult>> values = op.Value.Where(x => x.Value.HasChanged);
                int count = values.Count();
                if (count > 0)
                {
                    numEntries++;
                    writer.Write((byte)op.Key);
                    writer.Write(count);
                    foreach (KeyValuePair<PvpEngineOperationArgs, PvpEngineOperationResult> value in values)
                    {
                        value.Key.Write(writer);
                        value.Value.Write(writer);
                    }
                }
            }

            long tempOffset = writer.BaseStream.Position;
            writer.BaseStream.Position = offset;
            writer.Write(numEntries);
            writer.BaseStream.Position = tempOffset;
        }
    }

    struct PvpEngineOperationArgs
    {
        public int Arg1;
        public int Arg2;
        public int Arg3;

        public PvpEngineOperationArgs(int arg1)
        {
            Arg1 = arg1;
            Arg2 = 0;
            Arg3 = 0;
        }

        public PvpEngineOperationArgs(int arg1, int arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = 0;
        }

        public PvpEngineOperationArgs(int arg1, int arg2, int arg3)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public void Read(BinaryReader reader)
        {
            Arg1 = reader.ReadInt32();
            Arg2 = reader.ReadInt32();
            Arg3 = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Arg1);
            writer.Write(Arg2);
            writer.Write(Arg3);
        }
    }

    class PvpEngineOperationResult
    {
        public bool HasChanged;
        public int Value;
        public byte[] Data;

        public void Read(BinaryReader reader)
        {
            Value = reader.ReadInt32();
            if (reader.ReadBoolean())
            {
                int dataLen = reader.ReadInt32();
                Data = reader.ReadBytes(dataLen);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Value);
            writer.Write(Data != null);
            if (Data != null)
            {
                writer.Write(Data.Length);
                writer.Write(Data);
            }
            HasChanged = false;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PvpBasicVal
    {
        public short CardID;
        public short EffectID;
        public int Atk;
        public int Def;
        public int OrgAtk;
        public int OrgDef;
        public short Type;
        public short Attr;
        public short Element;
        public short Level;
        public byte Rank;
        public byte VoidMagic;
        public byte VoidTrap;
        public byte VoidMonst;

        public override string ToString()
        {
            return "{" + CardID + "," + EffectID + "," + Atk + "," + Def + "," + OrgAtk + "," + OrgDef + "," + Type + "," +
                Attr + "," + Element + "," + Level + "," + Rank + "," + VoidMagic + "," + VoidTrap + "," + VoidMonst + "}";
        }
    }

    class Pvp
    {
        public const string DllName = "../masterduel_Data/Plugins/x86_64/duel.dll";

        delegate void AddRecord(IntPtr ptr, int size);
        delegate int RunEffect(int id, int param1, int param2, int param3);
        delegate int IsBusyEffect(int id);
        delegate IntPtr NowRecord();
        delegate void RecordNext();
        delegate void RecordBegin();
        delegate int IsRecordEnd();

        bool keepConsoleAlive;
        NetClient netClient;
        DateTime lastPing;
        bool hasDuelEnd;
        IntPtr engineWork;
        int doCommandUserOffset;
        int runDialogUserOffset;
        PvpEngineState engineState;

        RunEffect runEffect;
        IsBusyEffect isBusyEffect;
        AddRecord addRecord;
        bool isFirstAddRecord;

        byte[] bufferInternalID;
        byte[] bufferProp;
        byte[] bufferSame;
        byte[] bufferGenre;
        byte[] bufferNamed;
        byte[] bufferLink;

        public Pvp()
        {
            runEffect = DoRunEffect;
            isBusyEffect = DoIsBusyEffect;
            addRecord = DoAddRecord;
        }

        bool InitContent()
        {
            string dataDir = Utils.GetDataDirectory(true);
            string cardDataDir = Path.Combine(dataDir, "CardData");
            if (!Directory.Exists(cardDataDir))
            {
                return false;
            }

            bufferInternalID = File.ReadAllBytes(Path.Combine(cardDataDir, "#", "CARD_IntID.bytes"));
            DLL_SetInternalID(GetPtr(bufferInternalID));

            bufferProp = File.ReadAllBytes(Path.Combine(cardDataDir, "#", "CARD_Prop.bytes"));
            DLL_SetCardProperty(GetPtr(bufferProp), bufferProp.Length);

            bufferSame = File.ReadAllBytes(Path.Combine(cardDataDir, "MD", "CARD_Same.bytes"));
            DLL_SetCardSame(GetPtr(bufferSame), bufferSame.Length);

            bufferGenre = File.ReadAllBytes(Path.Combine(cardDataDir, "#", "CARD_Genre.bytes"));
            DLL_SetCardGenre(GetPtr(bufferGenre));

            bufferNamed = File.ReadAllBytes(Path.Combine(cardDataDir, "#", "CARD_Named.bytes"));
            DLL_SetCardNamed(GetPtr(bufferNamed));

            bufferLink = File.ReadAllBytes(Path.Combine(cardDataDir, "MD", "CARD_Link.bytes"));
            DLL_SetCardLink(GetPtr(bufferLink), bufferLink.Length);

            return true;
        }

        IntPtr GetPtr(byte[] buffer)
        {
            IntPtr res = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, res, buffer.Length);
            return res;
        }

        public void Run(string json)
        {
            Dictionary<string, object> data = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;

            string sessionServerIP = Utils.GetValue<string>(data, "SessionServerIP");
            int sessionServerPort = Utils.GetValue<int>(data, "SessionServerPort");
            string key = Utils.GetValue<string>(data, "Key");
            uint duelRoomId = Utils.GetValue<uint>(data, "DuelRoomId");
            uint playerId1 = Utils.GetValue<uint>(data, "PlayerId1");
            uint playerId2 = Utils.GetValue<uint>(data, "PlayerId2");
            int sleep = Utils.GetValue<int>(data, "Sleep");
            int callsPerSleep = Utils.GetValue<int>(data, "CallsPerSleep");
            bool noDelay = Utils.GetValue<bool>(data, "NoDelay");
            keepConsoleAlive = Utils.GetValue<bool>(data, "KeepConsoleAlive");
            doCommandUserOffset = Utils.GetValue<int>(data, "DoCommandUserOffset");
            runDialogUserOffset = Utils.GetValue<int>(data, "RunDialogUserOffset");
            DuelSettings duelSettings = new DuelSettings();
            duelSettings.FromDictionary(Utils.GetDictionary(data, "Duel"));

            lastPing = DateTime.UtcNow;

            netClient = new NetClient();
            netClient.NoDelay = noDelay;
            netClient.Disconnected += (NetClient nc) =>
            {
                Console.WriteLine("netClient.Disconnected");
                if (!keepConsoleAlive)
                {
                    Environment.Exit(0);
                }
            };
            try
            {
                netClient.Connect(sessionServerIP, sessionServerPort);
            }
            catch
            {
            }
            if (!netClient.IsConnected)
            {
                Console.WriteLine("!netClient.IsConnected (make sure MultiplayerPvpClientConnectIP is correct)");
                if (!keepConsoleAlive)
                {
                    Environment.Exit(0);
                }
            }
            netClient.HandleMessage += HandleNetMessage;

            try
            {
                engineState = new PvpEngineState();
                engineState.Client = netClient;

                netClient.Send(new PvpServerConnectionRequestMessage()
                {
                    Key = key,
                    DuelRoomId = duelRoomId,
                    PlayerId1 = playerId1,
                    PlayerId2 = playerId2,
                });

                if (!InitContent())
                {
                    throw new Exception("Failed to init content");
                }

                for (int i = 0; i < 2; i++)
                {
                    if (duelSettings.hnum[i] == 0) duelSettings.hnum[i] = 5;
                    if (duelSettings.life[i] == 0) duelSettings.life[i] = 8000;
                }

                int num = DLL_SetWorkMemory(IntPtr.Zero);
                engineWork = Marshal.AllocHGlobal(num);
                if (engineWork == IntPtr.Zero)
                {
                    throw new Exception("Failed to allocate memory for duel");
                }
                DLL_SetWorkMemory(engineWork);

                bool tag = false;
                isFirstAddRecord = true;

                DLL_SetEffectDelegate(runEffect, isBusyEffect);
                DLL_DuelSysClearWork();
                DLL_DuelSetMyPlayerNum(0);
                DLL_DuelSetRandomSeed(duelSettings.RandSeed);
                SetDeck(0, duelSettings.Deck[0]);
                SetDeck(1, duelSettings.Deck[1]);
                DLL_DuelSetPlayerType(0, (int)DuelPlayerType.Human);
                DLL_DuelSetPlayerType(1, (int)DuelPlayerType.Human);
                DLL_DuelSetCpuParam(0, GetCpuParam(100));
                DLL_DuelSetCpuParam(1, GetCpuParam(100));
                DLL_DuelSetFirstPlayer(duelSettings.FirstPlayer);
                DLL_DuelSetDuelLimitedType((uint)DuelLimitedType.None);
                DLL_SetAddRecordDelegate(addRecord);
                DLL_DuelSysInitCustom((int)DuelType.Normal, tag, duelSettings.life[0], duelSettings.life[1], duelSettings.hnum[0], duelSettings.hnum[1], duelSettings.noshuffle);

                uint cardRareBufferSize = DLL_CardRareGetBufferSize();
                IntPtr cardRarePtr = Marshal.AllocHGlobal((int)cardRareBufferSize);
                if (cardRarePtr == IntPtr.Zero)
                {
                    throw new Exception("Failed to allocate memory for card rare");
                }

                int[] rare0 = GetCardRare(duelSettings.Deck[0]);
                int[] rare1 = GetCardRare(duelSettings.Deck[1]);
                DLL_CardRareSetRare(cardRarePtr, rare0, rare1, null, null);

                while (true)
                {
                    if (lastPing < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                    {
                        break;
                    }

                    for (int i = 0; i < callsPerSleep; i++)
                    {
                        int res;
                        lock (engineState)
                        {
                            res = DLL_DuelSysAct();
                        }
                        if (res > 0)
                        {
                            netClient.Send(new DuelSysActFinishedMessage());
                            Thread.Sleep(10000);
                            CloseClient();
                            break;
                        }
                        else if (hasDuelEnd)
                        {
                            Thread.Sleep(2000);
                            CloseClient();
                            break;
                        }
                    }
                    if (sleep > 0)
                    {
                        Thread.Sleep(sleep);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                CloseClient();
            }

            CloseClient();
            Thread.Sleep(2000);
            if (keepConsoleAlive)
            {
                Process.GetCurrentProcess().WaitForExit();
                Console.WriteLine("END");
            }
            else
            {
                Environment.Exit(0);
            }
        }

        unsafe int DoRunEffect(int id, int param1, int param2, int param3)
        {
            int doCommandUser = *(int*)(engineWork + doCommandUserOffset);
            int runDialogUser = *(int*)(engineWork + runDialogUserOffset);
            engineState.RunEffect((DuelViewType)id, param1, param2, param3, doCommandUser, runDialogUser);
            //Console.WriteLine("DoRun " + (DuelViewType)id);
            return 0;
        }

        int DoIsBusyEffect(int id)
        {
            int isBusyState;
            if (!engineState.IsBusyEffect.TryGetValue((DuelViewType)id, out isBusyState))
            {
                engineState.IsBusyEffect[(DuelViewType)id] = 0;
                isBusyState = 0;
                netClient.Send(new DuelIsBusyEffectMessage()
                {
                    RunEffectSeq = engineState.RunEffectSeq,
                    ViewType = (DuelViewType)id
                });
            }
            return isBusyState == 2 ? 0 : 1;
        }

        void DoAddRecord(IntPtr ptr, int size)
        {
            byte[] buffer = new byte[size];
            Marshal.Copy(ptr, buffer, 0, size);
            netClient.Send(new DuelSpectatorDataMessage()
            {
                IsFirstData = isFirstAddRecord,
                Buffer = buffer
            });
            isFirstAddRecord = false;
        }

        void SetDeck(int player, DeckInfo deck)
        {
            int[] main = deck.MainDeckCards.GetIds().ToArray();
            int[] extra = deck.ExtraDeckCards.GetIds().ToArray();
            int[] side = deck.SideDeckCards.GetIds().ToArray();
            DLL_DuelSysSetDeck2(player, main, main.Length, extra, extra.Length, side, side.Length);
        }

        int[] GetCardRare(DeckInfo deck)
        {
            IEnumerable<int> main = deck.MainDeckCards.GetCollection().Select(x => (int)x.Value);
            IEnumerable<int> extra = deck.ExtraDeckCards.GetCollection().Select(x => (int)x.Value);
            return main.Concat(extra).ToArray();
        }

        uint GetCpuParam(int val, DuelCpuParam param = DuelCpuParam.None)
        {
            val = Math.Min(100, Math.Max(-100, val));
            if (val < 0)
            {
                param |= DuelCpuParam.Def;
                val = -val;
            }
            return (uint)(val | (int)param);
        }

        void HandleNetMessage(NetClient client, NetMessage message)
        {
            switch (message.Type)
            {
                case NetMessageType.Ping: OnPing((PingMessage)message); break;
                case NetMessageType.ConnectionResponse: OnConnectionResponse((ConnectionResponseMessage)message); break;
                case NetMessageType.OpponentDuelEnded: hasDuelEnd = true; break;
                case NetMessageType.DuelError: OnDuelError((DuelErrorMessage)message); break;
                case NetMessageType.DuelIsBusyEffect: OnDuelIsBusyEffect((DuelIsBusyEffectMessage)message); break;
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

        void OnPing(PingMessage message)
        {
            lastPing = DateTime.UtcNow;
            netClient.Send(new PongMessage()
            {
                ServerToClientLatency = Utils.GetEpochTime() - Utils.GetEpochTime(message.RequestTime),
                ResponseTime = DateTime.UtcNow,
            });
        }

        void OnConnectionResponse(ConnectionResponseMessage message)
        {
            if (!message.Success)
            {
                CloseClient();
            }
        }

        void OnDuelError(DuelErrorMessage message)
        {
            CloseClient();
        }

        void OnDuelIsBusyEffect(DuelIsBusyEffectMessage message)
        {
            lock (engineState)
            {
                //Console.WriteLine("Recv OnDuelIsBusyEffect " + message.ViewType + " " + message.RunEffectSeq + " | " + engineState.RunEffectSeq);
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    engineState.IsBusyEffect[message.ViewType]++;
                }
            }
        }

        void OnDuelComMovePhase(DuelComMovePhaseMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelComMovePhase(message.Phase);
                }
            }
        }

        void OnDuelComDoCommand(DuelComDoCommandMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    Console.WriteLine("OnDuelComDoCommand player:" + message.Player + " pos:" + message.Position + " indx:" + message.Index + " cmd:" + message.CommandId + " seq:" + engineState.RunEffectSeq + " mseq:" + message.RunEffectSeq);
                    DLL_DuelComDoCommand(message.Player, message.Position, message.Index, message.CommandId);
                }
                else
                {
                    Utils.LogWarning("OnDuelComDoCommand bad seq expected:" + engineState.RunEffectSeq + " got:" + message.RunEffectSeq);
                }
            }
        }

        void OnDuelComCancelCommand(DuelComCancelCommandMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelComCancelCommand();
                }
            }
        }

        void OnDuelComCancelCommand2(DuelComCancelCommand2Message message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelComCancelCommand2(message.Decide);
                }
            }
        }

        void OnDuelDlgSetResult(DuelDlgSetResultMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelDlgSetResult(message.Result);
                }
            }
        }

        void OnDuelListSetCardExData(DuelListSetCardExDataMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelListSetCardExData(message.Index, message.Data);
                }
            }
        }

        void OnDuelListSetIndex(DuelListSetIndexMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelListSetIndex(message.Index);
                }
            }
        }

        void OnDuelListInitString(DuelListInitStringMessage message)
        {
            lock (engineState)
            {
                if (engineState.RunEffectSeq == message.RunEffectSeq)
                {
                    DLL_DuelListInitString();
                }
            }
        }

        void CloseClient()
        {
            try
            {
                netClient.Close();
            }
            catch
            {
            }
            new Thread(delegate ()
            {
                Console.WriteLine("CloseClient");
                Thread.Sleep(2000);
                if (!keepConsoleAlive)
                {
                    Environment.Exit(0);
                }
            }).Start();
        }

        [DllImport(DllName)]
        static extern void DLL_SetInternalID(IntPtr data);
        [DllImport(DllName)]
        static extern int DLL_SetCardProperty(IntPtr data, int size);
        [DllImport(DllName)]
        static extern void DLL_SetCardSame(IntPtr data, int size);
        [DllImport(DllName)]
        static extern void DLL_SetCardGenre(IntPtr data);
        [DllImport(DllName)]
        static extern void DLL_SetCardNamed(IntPtr data);
        [DllImport(DllName)]
        static extern void DLL_SetCardLink(IntPtr data, int size);

        [DllImport(DllName)]
        public static extern uint DLL_CardRareGetBufferSize();
        [DllImport(DllName)]
        public static extern int DLL_CardRareGetRareByUniqueID(int uniqueId);
        [DllImport(DllName)]
        public static extern void DLL_CardRareSetBuffer(IntPtr pBuf);
        [DllImport(DllName)]
        public static extern void DLL_CardRareSetRare(IntPtr pBuf, int[] rare0, int[] rare1, int[] rare2, int[] rare3);
        [DllImport(DllName)]
        public static extern bool DLL_DeulIsThisEffectiveMonsterWithDual(int player, int index);
        [DllImport(DllName)]
        public static extern int DLL_DlgProcGetSummoningMonsterUniqueID();
        [DllImport(DllName)]
        public static extern uint DLL_DuelCanIDoPutMonster(int player);
        [DllImport(DllName)]
        public static extern bool DLL_DuelCanIDoSpecialSummon(int player);
        [DllImport(DllName)]
        public static extern bool DLL_DuelCanIDoSummonMonster(int player);
        [DllImport(DllName)]
        public static extern int DLL_DuelComCancelCommand();
        [DllImport(DllName)]
        public static extern int DLL_DuelComCancelCommand2(bool decide);
        [DllImport(DllName)]
        public static extern void DLL_DuelComDebugCommand();
        [DllImport(DllName)]
        public static extern void DLL_DuelComDefaultLocation();
        [DllImport(DllName)]
        public static extern void DLL_DuelComDoCommand(int player, int position, int index, int commandId);
        [DllImport(DllName)]
        public static extern uint DLL_DuelComGetCommandMask(int player, int position, int index);
        [DllImport(DllName)]
        public static extern uint DLL_DuelComGetMovablePhase();
        [DllImport(DllName)]
        public static extern uint DLL_DUELCOMGetPosMaskOfThisHand(int player, int index, int commandId);
        [DllImport(DllName)]
        public static extern int DLL_DUELCOMGetRecommendSide();
        [DllImport(DllName)]
        public static extern uint DLL_DuelComGetTextIDOfThisCommand(int player, int position, int index);
        [DllImport(DllName)]
        public static extern uint DLL_DuelComGetTextIDOfThisSummon(int player, int position, int index);
        [DllImport(DllName)]
        public static extern void DLL_DuelComMovePhase(int phase);
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgCanYesNoSkip();
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetMixData(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetMixNum();
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetMixType(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetPosMaskOfThisSummon();
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetSelectItemEnable(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetSelectItemNum();
        [DllImport(DllName)]
        public static extern int DLL_DuelDlgGetSelectItemStr(int index);
        [DllImport(DllName)]
        public static extern void DLL_DuelDlgSetResult(uint result);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetAttackTargetMask(int player, int locate);
        [DllImport(DllName)]
        public static extern void DLL_DuelGetCardBasicVal(int player, int pos, int index, ref PvpBasicVal pVal);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetCardFace(int player, int position, int index);
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetCardIDByUniqueID2(int uniqueId);
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetCardInHand(int player);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetCardNum(int player, int locate);
        [DllImport(DllName)]
        public static extern IntPtr DLL_DuelGetCardPropByUniqueID(int uniqueId);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetCardTurn(int player, int position, int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetCardUniqueID(int player, int position, int index);
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetCurrentDmgStep();
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetCurrentPhase();
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetCurrentStep();
        [DllImport(DllName)]
        public static extern int DLL_DuelGetDuelFinish();
        [DllImport(DllName)]
        public static extern int DLL_DuelGetDuelFinishCardID();
        [DllImport(DllName)]
        public static extern bool DLL_DuelGetDuelFlagDeckReverse();
        [DllImport(DllName)]
        public static extern int DLL_DuelGetDuelResult();
        [DllImport(DllName)]
        public static extern void DLL_DuelGetFldAffectIcon(int player, int locate, byte[] ptr, int view_player);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetFldMonstOrgLevel(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetFldMonstOrgRank(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetFldMonstOrgType(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetFldMonstRank(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetFldPendOrgScale(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetFldPendScale(int player, int locate);
        [DllImport(DllName)]
        public static extern bool DLL_DuelGetHandCardOpen(int player, int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetLP(int player);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardCounter(int player, int locate, int counter);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardDirectFlag(int player, int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardEffectFlags(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardEffectIDAtChain(int player, int locate);
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetThisCardEffectList(int player, int locate, byte[] list);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardOverlayNum(int player, int locate);
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetThisCardParameter(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardShowParameter(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetThisCardTurnCounter(int player, int locate);
        [DllImport(DllName)]
        public static extern bool DLL_DuelGetThisMonsterFightableOnEffect(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetTopCardIndex(int player, int locate);
        //[DllImport(DllName)]
        //public static extern int DLL_DuelGetTrapMonstBasicVal(int cardId, ref BasicVal pVal);
        [DllImport(DllName)]
        public static extern uint DLL_DuelGetTurnNum();
        [DllImport(DllName)]
        public static extern int DLL_DuelIsHuman(int player);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsMyself(int player);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsReplayMode();
        [DllImport(DllName)]
        public static extern int DLL_DuelIsRival(int player);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisCardExist(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisContinuousCard(int player, int locate);
        [DllImport(DllName)]
        public static extern bool DLL_DuelIsThisEffectiveMonster(int player, int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisEquipCard(int player, int locate);
        [DllImport(DllName)]
        public static extern bool DLL_DuelIsThisMagic(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisNormalMonster(int player, int locate);
        [DllImport(DllName)]
        public static extern bool DLL_DuelIsThisNormalMonsterInGrave(int player, int index);
        //[DllImport(DllName)]
        //public static extern bool DLL_DuelIsThisNormalMonsterInHand(int wCardID);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisQuickDuel();
        [DllImport(DllName)]
        public static extern bool DLL_DuelIsThisTrap(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisTrapMonster(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisTunerMonster(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisZoneAvailable(int player, int locate);
        [DllImport(DllName)]
        public static extern int DLL_DuelIsThisZoneAvailable2(int player, int locate, bool visibleOnly);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetCardAttribute(int iLookPlayer, int wUniqueID);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemAttribute(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemFrom(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemID(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemMax();
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemMsg(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemTargetUniqueID(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetItemUniqueID(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetSelectMax();
        [DllImport(DllName)]
        public static extern int DLL_DuelListGetSelectMin();
        [DllImport(DllName)]
        public static extern void DLL_DuelListInitString();
        [DllImport(DllName)]
        public static extern int DLL_DuelListIsMultiMode();
        [DllImport(DllName)]
        public static extern void DLL_DuelListSetCardExData(int index, int data);
        [DllImport(DllName)]
        public static extern void DLL_DuelListSetIndex(int index);
        [DllImport(DllName)]
        public static extern int DLL_DuelMyself();
        [DllImport(DllName)]
        public static extern int DLL_DuelResultGetData(int player, byte[] dst);
        [DllImport(DllName)]
        public static extern int DLL_DuelResultGetMemo(int player, byte[] dst);
        [DllImport(DllName)]
        public static extern int DLL_DuelRival();
        [DllImport(DllName)]
        public static extern uint DLL_DuelSearchCardByUniqueID(int uniqueId);
        [DllImport(DllName)]
        public static extern void DLL_DuelSetCpuParam(int player, uint param);
        [DllImport(DllName)]
        public static extern void DLL_DuelSetDuelLimitedType(uint limitedType);
        [DllImport(DllName)]
        public static extern void DLL_DuelSetFirstPlayer(int player);
        [DllImport(DllName)]
        public static extern void DLL_DuelSetMyPlayerNum(int player);
        [DllImport(DllName)]
        public static extern void DLL_DuelSetPlayerType(int player, int type);
        [DllImport(DllName)]
        public static extern void DLL_DuelSetRandomSeed(uint seed);
        [DllImport(DllName)]
        public static extern int DLL_DuelSysAct();
        [DllImport(DllName)]
        public static extern void DLL_DuelSysClearWork();
        [DllImport(DllName)]
        public static extern int DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf);
        [DllImport(DllName)]
        public static extern int DLL_DuelSysInitQuestion(IntPtr pScript);
        [DllImport(DllName)]
        public static extern int DLL_DuelSysInitRush();
        [DllImport(DllName)]
        public static extern void DLL_DuelSysSetDeck2(int player, int[] mainDeck, int mainNum, int[] extraDeck, int extraNum, int[] sideDeck, int sideNum);
        [DllImport(DllName)]
        public static extern int DLL_DuelWhichTurnNow();
        [DllImport(DllName)]
        public static extern int DLL_FusionGetMaterialList(int uniqueId, byte[] list);
        [DllImport(DllName)]
        public static extern int DLL_FusionGetMonsterLevelInTuning(int wUniqueID);
        [DllImport(DllName)]
        public static extern int DLL_FusionIsThisTunedMonsterInTuning(int wUniqueID);
        //[DllImport(DllName)]
        //static extern int DLL_GetBinHash(int iIndex);
        //[DllImport(DllName)]
        //static extern int DLL_GetCardExistNum();
        //[DllImport(DllName)]
        //static extern int DLL_GetRevision();
        [DllImport(DllName)]
        static extern void DLL_SetAddRecordDelegate(AddRecord addRecord);
        [DllImport(DllName)]
        static extern void DLL_SetCardExistWork(IntPtr pWork, int size, int count);
        //[DllImport(DllName)]
        //static extern void DLL_SetDuelChallenge(int flagbit);
        //[DllImport(DllName)]
        //static extern void DLL_SetDuelChallenge2(int player, int flagbit);
        [DllImport(DllName)]
        static extern void DLL_SetEffectDelegate(RunEffect runEffct, IsBusyEffect isBusyEffect);
        [DllImport(DllName)]
        static extern void DLL_SetPlayRecordDelegate(NowRecord nowRecord, RecordNext recordNext, RecordBegin recordBegin, IsRecordEnd isRecordEnd);
        [DllImport(DllName)]
        static extern int DLL_SetWorkMemory(IntPtr pWork);
        [DllImport(DllName)]
        public static extern int DLL_DuelGetAttachedEffectList(byte[] lpAffect);
        [DllImport(DllName)]
        private static extern void DLL_DuelChangeCardIDByUniqueID(int uniqueId, int cardId);//v2.3.0 TODO:?
        [DllImport(DllName)]
        private static extern void DLL_SetRareByUniqueID(int uniqueId, int rare);//v2.3.0 TODO:?
        [DllImport(DllName)]
        private static extern bool DLL_DuelGetCantActIcon(int player, int locate, int index, int flag);//v2.4.0 TODO:?
    }

    enum PvpOperationType
    {
        DLL_CardRareGetBufferSize,
        DLL_CardRareGetRareByUniqueID,
        DLL_CardRareSetBuffer,
        DLL_CardRareSetRare,
        DLL_DeulIsThisEffectiveMonsterWithDual,
        DLL_DlgProcGetSummoningMonsterUniqueID,
        DLL_DuelCanIDoPutMonster,
        DLL_DuelCanIDoSpecialSummon,
        DLL_DuelCanIDoSummonMonster,
        DLL_DuelComCancelCommand2,
        DLL_DuelComDebugCommand,
        DLL_DuelComDefaultLocation,
        DLL_DuelComDoCommand,
        DLL_DuelComGetCommandMask,
        DLL_DuelComGetMovablePhase,
        DLL_DUELCOMGetPosMaskOfThisHand,
        DLL_DUELCOMGetRecommendSide,
        DLL_DuelComGetTextIDOfThisCommand,
        DLL_DuelComGetTextIDOfThisSummon,
        DLL_DuelComMovePhase,
        DLL_DuelDlgCanYesNoSkip,
        DLL_DuelDlgGetMixData,
        DLL_DuelDlgGetMixNum,
        DLL_DuelDlgGetMixType,
        DLL_DuelDlgGetPosMaskOfThisSummon,
        DLL_DuelDlgGetSelectItemEnable,
        DLL_DuelDlgGetSelectItemNum,
        DLL_DuelDlgGetSelectItemStr,
        DLL_DuelDlgSetResult,
        DLL_DuelGetAttackTargetMask,
        DLL_DuelGetCardBasicVal,
        DLL_DuelGetCardFace,
        DLL_DuelGetCardIDByUniqueID2,
        DLL_DuelGetCardInHand,
        DLL_DuelGetCardNum,
        DLL_DuelGetCardPropByUniqueID,
        DLL_DuelGetCardTurn,
        DLL_DuelGetCardUniqueID,
        DLL_DuelGetCurrentDmgStep,
        DLL_DuelGetCurrentPhase,
        DLL_DuelGetCurrentStep,
        DLL_DuelGetDuelFinish,
        DLL_DuelGetDuelFinishCardID,
        DLL_DuelGetDuelFlagDeckReverse,
        DLL_DuelGetDuelResult,
        DLL_DuelGetFldAffectIcon,
        DLL_DuelGetFldMonstOrgLevel,
        DLL_DuelGetFldMonstOrgRank,
        DLL_DuelGetFldMonstOrgType,
        DLL_DuelGetFldMonstRank,
        DLL_DuelGetFldPendOrgScale,
        DLL_DuelGetFldPendScale,
        DLL_DuelGetHandCardOpen,
        DLL_DuelGetLP,
        DLL_DuelGetThisCardCounter,
        DLL_DuelGetThisCardDirectFlag,
        DLL_DuelGetThisCardEffectFlags,
        DLL_DuelGetThisCardEffectIDAtChain,
        DLL_DuelGetThisCardEffectList,
        DLL_DuelGetThisCardOverlayNum,
        DLL_DuelGetThisCardParameter,
        DLL_DuelGetThisCardShowParameter,
        DLL_DuelGetThisCardTurnCounter,
        DLL_DuelGetThisMonsterFightableOnEffect,
        DLL_DuelGetTopCardIndex,
        DLL_DuelGetTrapMonstBasicVal,
        DLL_DuelGetTurnNum,
        DLL_DuelIsHuman,
        DLL_DuelIsMyself,
        DLL_DuelIsReplayMode,
        DLL_DuelIsRival,
        DLL_DuelIsThisCardExist,
        DLL_DuelIsThisContinuousCard,
        DLL_DuelIsThisEffectiveMonster,
        DLL_DuelIsThisEquipCard,
        DLL_DuelIsThisMagic,
        DLL_DuelIsThisNormalMonster,
        DLL_DuelIsThisNormalMonsterInGrave,
        DLL_DuelIsThisNormalMonsterInHand,
        DLL_DuelIsThisQuickDuel,
        DLL_DuelIsThisTrap,
        DLL_DuelIsThisTrapMonster,
        DLL_DuelIsThisTunerMonster,
        DLL_DuelIsThisZoneAvailable,
        DLL_DuelIsThisZoneAvailable2,
        DLL_DuelListGetCardAttribute,
        DLL_DuelListGetItemAttribute,
        DLL_DuelListGetItemFrom,
        DLL_DuelListGetItemID,
        DLL_DuelListGetItemMax,
        DLL_DuelListGetItemMsg,
        DLL_DuelListGetItemTargetUniqueID,
        DLL_DuelListGetItemUniqueID,
        DLL_DuelListGetSelectMax,
        DLL_DuelListGetSelectMin,
        DLL_DuelListInitString,
        DLL_DuelListIsMultiMode,
        DLL_DuelListSetCardExData,
        DLL_DuelListSetIndex,
        DLL_DuelMyself,
        DLL_DuelResultGetData,
        DLL_DuelResultGetMemo,
        DLL_DuelRival,
        DLL_DuelSearchCardByUniqueID,
        DLL_DuelSetCpuParam,
        DLL_DuelSetDuelLimitedType,
        DLL_DuelSetFirstPlayer,
        DLL_DuelSetMyPlayerNum,
        DLL_DuelSetPlayerType,
        DLL_DuelSetRandomSeed,
        DLL_DuelSysAct,
        DLL_DuelSysClearWork,
        DLL_DuelSysInitCustom,
        DLL_DuelSysInitQuestion,
        DLL_DuelSysInitRush,
        DLL_DuelSysSetDeck2,
        DLL_DuelWhichTurnNow,
        DLL_FusionGetMaterialList,
        DLL_FusionGetMonsterLevelInTuning,
        DLL_FusionIsThisTunedMonsterInTuning,
        DLL_GetBinHash,
        DLL_GetCardExistNum,
        DLL_GetRevision,
        DLL_SetAddRecordDelegate,
        DLL_SetCardExistWork,
        DLL_SetDuelChallenge,
        DLL_SetDuelChallenge2,
        DLL_SetEffectDelegate,
        DLL_SetPlayRecordDelegate,
        DLL_SetWorkMemory,
        DLL_DuelGetAttachedEffectList,//v2.2.1
    }
}