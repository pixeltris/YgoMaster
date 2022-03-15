using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace YgoMaster
{
    partial class DuelSimulator
    {
        public delegate void AddRecord(IntPtr ptr, int size);
        public delegate int RunEffect(int id, int param1, int param2, int param3);
        public delegate int IsBusyEffect(int id);
        public delegate IntPtr NowRecord();
        public delegate void RecordNext();
        public delegate void RecordBegin();
        public delegate int IsRecordEnd();

        [StructLayout(LayoutKind.Explicit)]
        public struct BasicVal
        {
            [FieldOffset(0x0)]
            public short CardID;
            [FieldOffset(0x2)]
            public short EffectID;
            [FieldOffset(0x4)]
            public int Atk;
            [FieldOffset(0x8)]
            public int Def;
            [FieldOffset(0xC)]
            public int OrgAtk;
            [FieldOffset(0x10)]
            public int OrgDef;
            [FieldOffset(0x14)]
            public short Type;
            [FieldOffset(0x16)]
            public short Attr;
            [FieldOffset(0x18)]
            public short Element;
            [FieldOffset(0x1A)]
            public short Level;
            [FieldOffset(0x1C)]
            public byte Rank;
            [FieldOffset(0x1D)]
            public byte VoidMagic;
            [FieldOffset(0x1E)]
            public byte VoidTrap;
            [FieldOffset(0x1F)]
            public byte VoidMonst;
        }

        const string dllName = "../masterduel_Data/Plugins/x86_64/duel.dll";
        [DllImport(dllName)]
        private static extern uint DLL_CardRareGetBufferSize();
        [DllImport(dllName)]
        private static extern int DLL_CardRareGetRareByUniqueID(int uniqueId);
        [DllImport(dllName)]
        private static extern void DLL_CardRareSetBuffer(IntPtr pBuf);
        [DllImport(dllName)]
        private static extern void DLL_CardRareSetRare(IntPtr pBuf, IntPtr rare0, IntPtr rare1, IntPtr rare2, IntPtr rare3);
        [DllImport(dllName)]
        private static extern bool DLL_DeulIsThisEffectiveMonsterWithDual(int player, int index);
        [DllImport(dllName)]
        private static extern int DLL_DlgProcGetSummoningMonsterUniqueID();
        [DllImport(dllName)]
        private static extern uint DLL_DuelCanIDoPutMonster(int player);
        [DllImport(dllName)]
        private static extern bool DLL_DuelCanIDoSpecialSummon(int player);
        [DllImport(dllName)]
        private static extern bool DLL_DuelCanIDoSummonMonster(int player);
        [DllImport(dllName)]
        private static extern int DLL_DuelComCancelCommand2(bool decide);
        [DllImport(dllName)]
        private static extern void DLL_DuelComDebugCommand();
        [DllImport(dllName)]
        private static extern void DLL_DuelComDefaultLocation();
        [DllImport(dllName)]
        private static extern void DLL_DuelComDoCommand(int player, int position, int index, int commandId);
        [DllImport(dllName)]
        private static extern uint DLL_DuelComGetCommandMask(int player, int position, int index);
        [DllImport(dllName)]
        private static extern uint DLL_DuelComGetMovablePhase();
        [DllImport(dllName)]
        private static extern uint DLL_DUELCOMGetPosMaskOfThisHand(int player, int index, int commandId);
        [DllImport(dllName)]
        private static extern void DLL_DuelComMovePhase(int phase);
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgCanYesNoSkip();
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetMixData(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetMixNum();
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetMixType(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetPosMaskOfThisSummon();
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetSelectItemEnable(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetSelectItemNum();
        [DllImport(dllName)]
        private static extern int DLL_DuelDlgGetSelectItemStr(int index);
        [DllImport(dllName)]
        private static extern void DLL_DuelDlgSetResult(uint result);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetAttackTargetMask(int player, int locate);
        [DllImport(dllName)]
        private static extern void DLL_DuelGetCardBasicVal(int player, int pos, int index, ref BasicVal pVal);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetCardFace(int player, int position, int index);
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetCardIDByUniqueID2(int uniqueId);
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetCardInHand(int player);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetCardNum(int player, int locate);
        [DllImport(dllName)]
        private static extern IntPtr DLL_DuelGetCardPropByUniqueID(int uniqueId);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetCardTurn(int player, int position, int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetCardUniqueID(int player, int position, int index);
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetCurrentDmgStep();
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetCurrentPhase();
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetCurrentStep();
        [DllImport(dllName)]
        private static extern int DLL_DuelGetDuelFinish();
        [DllImport(dllName)]
        private static extern int DLL_DuelGetDuelFinishCardID();
        [DllImport(dllName)]
        private static extern int DLL_DuelGetDuelResult();
        [DllImport(dllName)]
        private static extern void DLL_DuelGetFldAffectIcon(int player, int locate, IntPtr ptr, int view_player);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetFldMonstOrgLevel(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetFldMonstOrgRank(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetFldMonstOrgType(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetFldMonstRank(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetFldPendOrgScale(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetFldPendScale(int player, int locate);
        [DllImport(dllName)]
        private static extern bool DLL_DuelGetHandCardOpen(int player, int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetLP(int player);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardCounter(int player, int locate, int counter);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardDirectFlag(int player, int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardEffectFlags(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardEffectIDAtChain(int player, int locate);
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetThisCardEffectList(int player, int locate, IntPtr list);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardOverlayNum(int player, int locate);
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetThisCardParameter(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardShowParameter(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetThisCardTurnCounter(int player, int locate);
        [DllImport(dllName)]
        private static extern bool DLL_DuelGetThisMonsterFightableOnEffect(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetTopCardIndex(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelGetTrapMonstBasicVal(int cardId, ref BasicVal pVal);
        [DllImport(dllName)]
        private static extern uint DLL_DuelGetTurnNum();
        [DllImport(dllName)]
        private static extern int DLL_DuelIsHuman(int player);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsMyself(int player);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsReplayMode();
        [DllImport(dllName)]
        private static extern int DLL_DuelIsRival(int player);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisCardExist(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisContinuousCard(int player, int locate);
        [DllImport(dllName)]
        private static extern bool DLL_DuelIsThisEffectiveMonster(int player, int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisEquipCard(int player, int locate);
        [DllImport(dllName)]
        private static extern bool DLL_DuelIsThisMagic(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisNormalMonster(int player, int locate);
        [DllImport(dllName)]
        private static extern bool DLL_DuelIsThisNormalMonsterInGrave(int player, int index);
        [DllImport(dllName)]
        private static extern bool DLL_DuelIsThisNormalMonsterInHand(int wCardID);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisQuickDuel();
        [DllImport(dllName)]
        private static extern bool DLL_DuelIsThisTrap(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisTrapMonster(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisTunerMonster(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisZoneAvailable(int player, int locate);
        [DllImport(dllName)]
        private static extern int DLL_DuelIsThisZoneAvailable2(int player, int locate, bool visibleOnly);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetCardAttribute(int iLookPlayer, int wUniqueID);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetItemAttribute(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetItemFrom(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetItemID(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetItemMax();
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetItemMsg(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetItemUniqueID(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetSelectMax();
        [DllImport(dllName)]
        private static extern int DLL_DuelListGetSelectMin();
        [DllImport(dllName)]
        private static extern void DLL_DuelListInitString();
        [DllImport(dllName)]
        private static extern int DLL_DuelListIsMultiMode();
        [DllImport(dllName)]
        private static extern void DLL_DuelListSetCardExData(int index, int data);
        [DllImport(dllName)]
        private static extern void DLL_DuelListSetIndex(int index);
        [DllImport(dllName)]
        private static extern int DLL_DuelMyself();
        [DllImport(dllName)]
        private static extern int DLL_DuelResultGetData(int player, IntPtr dst);
        [DllImport(dllName)]
        private static extern int DLL_DuelResultGetMemo(int player, IntPtr dst);
        [DllImport(dllName)]
        private static extern int DLL_DuelRival();
        [DllImport(dllName)]
        private static extern uint DLL_DuelSearchCardByUniqueID(int uniqueId);
        [DllImport(dllName)]
        private static extern void DLL_DuelSetCpuParam(int player, uint param);
        [DllImport(dllName)]
        private static extern void DLL_DuelSetDuelLimitedType(uint limitedType);
        [DllImport(dllName)]
        private static extern void DLL_DuelSetFirstPlayer(int player);
        [DllImport(dllName)]
        private static extern void DLL_DuelSetMyPlayerNum(int player);
        [DllImport(dllName)]
        private static extern void DLL_DuelSetPlayerType(int player, int type);
        [DllImport(dllName)]
        private static extern void DLL_DuelSetRandomSeed(uint seed);
        [DllImport(dllName)]
        private static extern int DLL_DuelSysAct();
        [DllImport(dllName)]
        private static extern void DLL_DuelSysClearWork();
        [DllImport(dllName)]
        private static extern int DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool noshuffle);//bool shuf);
        [DllImport(dllName)]
        private static extern int DLL_DuelSysInitQuestion(IntPtr pScript);
        [DllImport(dllName)]
        private static extern int DLL_DuelSysInitRush();
        [DllImport(dllName)]
        private static extern void DLL_DuelSysSetDeck2(int player, int[] mainDeck, int mainNum, int[] extraDeck, int extraNum, int[] sideDeck, int sideNum);
        [DllImport(dllName)]
        private static extern int DLL_DuelWhichTurnNow();
        [DllImport(dllName)]
        private static extern int DLL_FusionGetMaterialList(int uniqueId, IntPtr list);
        [DllImport(dllName)]
        private static extern int DLL_FusionGetMonsterLevelInTuning(int wUniqueID);
        [DllImport(dllName)]
        private static extern int DLL_FusionIsThisTunedMonsterInTuning(int wUniqueID);
        [DllImport(dllName)]
        private static extern int DLL_GetBinHash(int iIndex);
        [DllImport(dllName)]
        private static extern int DLL_GetCardExistNum();
        [DllImport(dllName)]
        private static extern int DLL_GetRevision();
        [DllImport(dllName)]
        private static extern void DLL_SetAddRecordDelegate(AddRecord addRecord);
        [DllImport(dllName)]
        private static extern void DLL_SetCardExistWork(IntPtr pWork, int size, int count);
        [DllImport(dllName)]
        private static extern void DLL_SetDuelChallenge(int flagbit);
        [DllImport(dllName)]
        private static extern void DLL_SetDuelChallenge2(int player, int flagbit);
        [DllImport(dllName)]
        private static extern void DLL_SetEffectDelegate(RunEffect runEffct, IsBusyEffect isBusyEffect);
        [DllImport(dllName)]
        private static extern void DLL_SetPlayRecordDelegate(NowRecord nowRecord, RecordNext recordNext, RecordBegin recordBegin, IsRecordEnd isRecordEnd);
        [DllImport(dllName)]
        private static extern int DLL_SetWorkMemory(IntPtr pWork);
    }
}
