using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using YgoMaster;

// TODO:
// - We have a lot of common DLL_ implementations. Create shared handlers to reduce the large amount of code dupe/bloat

namespace YgoMasterClient
{
    unsafe static partial class DuelDll
    {
        delegate uint Del_DLL_CardRareGetBufferSize();
        static Hook<Del_DLL_CardRareGetBufferSize> hookDLL_CardRareGetBufferSize;
        delegate int Del_DLL_CardRareGetRareByUniqueID(int uniqueId);
        static Hook<Del_DLL_CardRareGetRareByUniqueID> hookDLL_CardRareGetRareByUniqueID;
        delegate void Del_DLL_CardRareSetBuffer(IntPtr pBuf);
        static Hook<Del_DLL_CardRareSetBuffer> hookDLL_CardRareSetBuffer;
        delegate void Del_DLL_CardRareSetRare(IntPtr pBuf, IntPtr rare0, IntPtr rare1, IntPtr rare2, IntPtr rare3);
        static Hook<Del_DLL_CardRareSetRare> hookDLL_CardRareSetRare;
        delegate bool Del_DLL_DeulIsThisEffectiveMonsterWithDual(int player, int index);
        static Hook<Del_DLL_DeulIsThisEffectiveMonsterWithDual> hookDLL_DeulIsThisEffectiveMonsterWithDual;
        delegate int Del_DLL_DlgProcGetSummoningMonsterUniqueID();
        static Hook<Del_DLL_DlgProcGetSummoningMonsterUniqueID> hookDLL_DlgProcGetSummoningMonsterUniqueID;
        delegate uint Del_DLL_DuelCanIDoPutMonster(int player);
        static Hook<Del_DLL_DuelCanIDoPutMonster> hookDLL_DuelCanIDoPutMonster;
        delegate bool Del_DLL_DuelCanIDoSpecialSummon(int player);
        static Hook<Del_DLL_DuelCanIDoSpecialSummon> hookDLL_DuelCanIDoSpecialSummon;
        delegate bool Del_DLL_DuelCanIDoSummonMonster(int player);
        static Hook<Del_DLL_DuelCanIDoSummonMonster> hookDLL_DuelCanIDoSummonMonster;
        delegate uint Del_DLL_DuelComGetCommandMask(int player, int position, int index);
        static Hook<Del_DLL_DuelComGetCommandMask> hookDLL_DuelComGetCommandMask;
        delegate uint Del_DLL_DuelComGetMovablePhase();
        static Hook<Del_DLL_DuelComGetMovablePhase> hookDLL_DuelComGetMovablePhase;
        delegate uint Del_DLL_DUELCOMGetPosMaskOfThisHand(int player, int index, int commandId);
        static Hook<Del_DLL_DUELCOMGetPosMaskOfThisHand> hookDLL_DUELCOMGetPosMaskOfThisHand;
        delegate int Del_DLL_DUELCOMGetRecommendSide();
        static Hook<Del_DLL_DUELCOMGetRecommendSide> hookDLL_DUELCOMGetRecommendSide;
        delegate uint Del_DLL_DuelComGetTextIDOfThisCommand(int player, int position, int index);
        static Hook<Del_DLL_DuelComGetTextIDOfThisCommand> hookDLL_DuelComGetTextIDOfThisCommand;
        delegate uint Del_DLL_DuelComGetTextIDOfThisSummon(int player, int position, int index);
        static Hook<Del_DLL_DuelComGetTextIDOfThisSummon> hookDLL_DuelComGetTextIDOfThisSummon;
        delegate int Del_DLL_DuelDlgCanYesNoSkip();
        static Hook<Del_DLL_DuelDlgCanYesNoSkip> hookDLL_DuelDlgCanYesNoSkip;
        delegate int Del_DLL_DuelDlgGetMixNum();
        static Hook<Del_DLL_DuelDlgGetMixNum> hookDLL_DuelDlgGetMixNum;
        delegate int Del_DLL_DuelDlgGetMixData(int index);
        static Hook<Del_DLL_DuelDlgGetMixData> hookDLL_DuelDlgGetMixData;
        delegate int Del_DLL_DuelDlgGetMixType(int index);
        static Hook<Del_DLL_DuelDlgGetMixType> hookDLL_DuelDlgGetMixType;
        delegate int Del_DLL_DuelDlgGetPosMaskOfThisSummon();
        static Hook<Del_DLL_DuelDlgGetPosMaskOfThisSummon> hookDLL_DuelDlgGetPosMaskOfThisSummon;
        delegate int Del_DLL_DuelDlgGetSelectItemEnable(int index);
        static Hook<Del_DLL_DuelDlgGetSelectItemEnable> hookDLL_DuelDlgGetSelectItemEnable;
        delegate int Del_DLL_DuelDlgGetSelectItemNum();
        static Hook<Del_DLL_DuelDlgGetSelectItemNum> hookDLL_DuelDlgGetSelectItemNum;
        delegate int Del_DLL_DuelDlgGetSelectItemStr(int index);
        static Hook<Del_DLL_DuelDlgGetSelectItemStr> hookDLL_DuelDlgGetSelectItemStr;
        delegate int Del_DLL_DuelGetAttackTargetMask(int player, int locate);
        static Hook<Del_DLL_DuelGetAttackTargetMask> hookDLL_DuelGetAttackTargetMask;
        delegate void Del_DLL_DuelGetCardBasicVal(int player, int pos, int index, ref PvpBasicVal pVal);
        static Hook<Del_DLL_DuelGetCardBasicVal> hookDLL_DuelGetCardBasicVal;
        delegate int Del_DLL_DuelGetCardFace(int player, int position, int index);
        static Hook<Del_DLL_DuelGetCardFace> hookDLL_DuelGetCardFace;
        delegate uint Del_DLL_DuelGetCardIDByUniqueID2(int uniqueId);
        static Hook<Del_DLL_DuelGetCardIDByUniqueID2> hookDLL_DuelGetCardIDByUniqueID2;
        delegate uint Del_DLL_DuelGetCardInHand(int player);
        static Hook<Del_DLL_DuelGetCardInHand> hookDLL_DuelGetCardInHand;
        delegate int Del_DLL_DuelGetCardNum(int player, int locate);
        static Hook<Del_DLL_DuelGetCardNum> hookDLL_DuelGetCardNum;
        delegate IntPtr Del_DLL_DuelGetCardPropByUniqueID(int uniqueId);
        static Hook<Del_DLL_DuelGetCardPropByUniqueID> hookDLL_DuelGetCardPropByUniqueID;
        delegate int Del_DLL_DuelGetCardTurn(int player, int position, int index);
        static Hook<Del_DLL_DuelGetCardTurn> hookDLL_DuelGetCardTurn;
        delegate int Del_DLL_DuelGetCardUniqueID(int player, int position, int index);
        static Hook<Del_DLL_DuelGetCardUniqueID> hookDLL_DuelGetCardUniqueID;
        delegate uint Del_DLL_DuelGetCurrentDmgStep();
        static Hook<Del_DLL_DuelGetCurrentDmgStep> hookDLL_DuelGetCurrentDmgStep;
        delegate uint Del_DLL_DuelGetCurrentPhase();
        static Hook<Del_DLL_DuelGetCurrentPhase> hookDLL_DuelGetCurrentPhase;
        delegate uint Del_DLL_DuelGetCurrentStep();
        static Hook<Del_DLL_DuelGetCurrentStep> hookDLL_DuelGetCurrentStep;
        delegate int Del_DLL_DuelGetDuelFinish();
        static Hook<Del_DLL_DuelGetDuelFinish> hookDLL_DuelGetDuelFinish;
        delegate int Del_DLL_DuelGetDuelFinishCardID();
        static Hook<Del_DLL_DuelGetDuelFinishCardID> hookDLL_DuelGetDuelFinishCardID;
        delegate bool Del_DLL_DuelGetDuelFlagDeckReverse();
        static Hook<Del_DLL_DuelGetDuelFlagDeckReverse> hookDLL_DuelGetDuelFlagDeckReverse;
        delegate int Del_DLL_DuelGetDuelResult();
        static Hook<Del_DLL_DuelGetDuelResult> hookDLL_DuelGetDuelResult;
        delegate void Del_DLL_DuelGetFldAffectIcon(int player, int locate, IntPtr ptr, int view_player);
        static Hook<Del_DLL_DuelGetFldAffectIcon> hookDLL_DuelGetFldAffectIcon;
        delegate int Del_DLL_DuelGetFldMonstOrgLevel(int player, int locate);
        static Hook<Del_DLL_DuelGetFldMonstOrgLevel> hookDLL_DuelGetFldMonstOrgLevel;
        delegate int Del_DLL_DuelGetFldMonstOrgRank(int player, int locate);
        static Hook<Del_DLL_DuelGetFldMonstOrgRank> hookDLL_DuelGetFldMonstOrgRank;
        delegate int Del_DLL_DuelGetFldMonstOrgType(int player, int locate);
        static Hook<Del_DLL_DuelGetFldMonstOrgType> hookDLL_DuelGetFldMonstOrgType;
        delegate int Del_DLL_DuelGetFldMonstRank(int player, int locate);
        static Hook<Del_DLL_DuelGetFldMonstRank> hookDLL_DuelGetFldMonstRank;
        delegate int Del_DLL_DuelGetFldPendOrgScale(int player, int locate);
        static Hook<Del_DLL_DuelGetFldPendOrgScale> hookDLL_DuelGetFldPendOrgScale;
        delegate int Del_DLL_DuelGetFldPendScale(int player, int locate);
        static Hook<Del_DLL_DuelGetFldPendScale> hookDLL_DuelGetFldPendScale;
        delegate bool Del_DLL_DuelGetHandCardOpen(int player, int index);
        static Hook<Del_DLL_DuelGetHandCardOpen> hookDLL_DuelGetHandCardOpen;
        delegate int Del_DLL_DuelGetLP(int player);
        static Hook<Del_DLL_DuelGetLP> hookDLL_DuelGetLP;
        delegate int Del_DLL_DuelGetThisCardCounter(int player, int locate, int counter);
        static Hook<Del_DLL_DuelGetThisCardCounter> hookDLL_DuelGetThisCardCounter;
        delegate int Del_DLL_DuelGetThisCardDirectFlag(int player, int index);
        static Hook<Del_DLL_DuelGetThisCardDirectFlag> hookDLL_DuelGetThisCardDirectFlag;
        delegate int Del_DLL_DuelGetThisCardEffectFlags(int player, int locate);
        static Hook<Del_DLL_DuelGetThisCardEffectFlags> hookDLL_DuelGetThisCardEffectFlags;
        delegate int Del_DLL_DuelGetThisCardEffectIDAtChain(int player, int locate);
        static Hook<Del_DLL_DuelGetThisCardEffectIDAtChain> hookDLL_DuelGetThisCardEffectIDAtChain;
        delegate uint Del_DLL_DuelGetThisCardEffectList(int player, int locate, IntPtr list);
        static Hook<Del_DLL_DuelGetThisCardEffectList> hookDLL_DuelGetThisCardEffectList;
        delegate int Del_DLL_DuelGetThisCardOverlayNum(int player, int locate);
        static Hook<Del_DLL_DuelGetThisCardOverlayNum> hookDLL_DuelGetThisCardOverlayNum;
        delegate uint Del_DLL_DuelGetThisCardParameter(int player, int locate);
        static Hook<Del_DLL_DuelGetThisCardParameter> hookDLL_DuelGetThisCardParameter;
        delegate int Del_DLL_DuelGetThisCardShowParameter(int player, int locate);
        static Hook<Del_DLL_DuelGetThisCardShowParameter> hookDLL_DuelGetThisCardShowParameter;
        delegate int Del_DLL_DuelGetThisCardTurnCounter(int player, int locate);
        static Hook<Del_DLL_DuelGetThisCardTurnCounter> hookDLL_DuelGetThisCardTurnCounter;
        delegate bool Del_DLL_DuelGetThisMonsterFightableOnEffect(int player, int locate);
        static Hook<Del_DLL_DuelGetThisMonsterFightableOnEffect> hookDLL_DuelGetThisMonsterFightableOnEffect;
        delegate int Del_DLL_DuelGetTopCardIndex(int player, int locate);
        static Hook<Del_DLL_DuelGetTopCardIndex> hookDLL_DuelGetTopCardIndex;
        delegate int Del_DLL_DuelGetTrapMonstBasicVal(int cardId, ref PvpBasicVal pVal);
        static Hook<Del_DLL_DuelGetTrapMonstBasicVal> hookDLL_DuelGetTrapMonstBasicVal;
        delegate uint Del_DLL_DuelGetTurnNum();
        static Hook<Del_DLL_DuelGetTurnNum> hookDLL_DuelGetTurnNum;
        delegate int Del_DLL_DuelIsHuman(int player);
        static Hook<Del_DLL_DuelIsHuman> hookDLL_DuelIsHuman;
        delegate int Del_DLL_DuelIsMyself(int player);
        static Hook<Del_DLL_DuelIsMyself> hookDLL_DuelIsMyself;
        delegate int Del_DLL_DuelIsReplayMode();
        static Hook<Del_DLL_DuelIsReplayMode> hookDLL_DuelIsReplayMode;
        delegate int Del_DLL_DuelIsRival(int player);
        static Hook<Del_DLL_DuelIsRival> hookDLL_DuelIsRival;
        delegate int Del_DLL_DuelIsThisCardExist(int player, int locate);
        static Hook<Del_DLL_DuelIsThisCardExist> hookDLL_DuelIsThisCardExist;
        delegate int Del_DLL_DuelIsThisContinuousCard(int player, int locate);
        static Hook<Del_DLL_DuelIsThisContinuousCard> hookDLL_DuelIsThisContinuousCard;
        delegate bool Del_DLL_DuelIsThisEffectiveMonster(int player, int index);
        static Hook<Del_DLL_DuelIsThisEffectiveMonster> hookDLL_DuelIsThisEffectiveMonster;
        delegate int Del_DLL_DuelIsThisEquipCard(int player, int locate);
        static Hook<Del_DLL_DuelIsThisEquipCard> hookDLL_DuelIsThisEquipCard;
        delegate bool Del_DLL_DuelIsThisMagic(int player, int locate);
        static Hook<Del_DLL_DuelIsThisMagic> hookDLL_DuelIsThisMagic;
        delegate int Del_DLL_DuelIsThisNormalMonster(int player, int locate);
        static Hook<Del_DLL_DuelIsThisNormalMonster> hookDLL_DuelIsThisNormalMonster;
        delegate bool Del_DLL_DuelIsThisNormalMonsterInGrave(int player, int index);
        static Hook<Del_DLL_DuelIsThisNormalMonsterInGrave> hookDLL_DuelIsThisNormalMonsterInGrave;
        delegate bool Del_DLL_DuelIsThisNormalMonsterInHand(int wCardID);
        static Hook<Del_DLL_DuelIsThisNormalMonsterInHand> hookDLL_DuelIsThisNormalMonsterInHand;
        delegate int Del_DLL_DuelIsThisQuickDuel();
        static Hook<Del_DLL_DuelIsThisQuickDuel> hookDLL_DuelIsThisQuickDuel;
        delegate bool Del_DLL_DuelIsThisTrap(int player, int locate);
        static Hook<Del_DLL_DuelIsThisTrap> hookDLL_DuelIsThisTrap;
        delegate int Del_DLL_DuelIsThisTrapMonster(int player, int locate);
        static Hook<Del_DLL_DuelIsThisTrapMonster> hookDLL_DuelIsThisTrapMonster;
        delegate int Del_DLL_DuelIsThisTunerMonster(int player, int locate);
        static Hook<Del_DLL_DuelIsThisTunerMonster> hookDLL_DuelIsThisTunerMonster;
        delegate int Del_DLL_DuelIsThisZoneAvailable(int player, int locate);
        static Hook<Del_DLL_DuelIsThisZoneAvailable> hookDLL_DuelIsThisZoneAvailable;
        delegate int Del_DLL_DuelIsThisZoneAvailable2(int player, int locate, bool visibleOnly);
        static Hook<Del_DLL_DuelIsThisZoneAvailable2> hookDLL_DuelIsThisZoneAvailable2;
        delegate int Del_DLL_DuelListGetCardAttribute(int iLookPlayer, int wUniqueID);
        static Hook<Del_DLL_DuelListGetCardAttribute> hookDLL_DuelListGetCardAttribute;
        delegate int Del_DLL_DuelListGetItemAttribute(int index);
        static Hook<Del_DLL_DuelListGetItemAttribute> hookDLL_DuelListGetItemAttribute;
        delegate int Del_DLL_DuelListGetItemFrom(int index);
        static Hook<Del_DLL_DuelListGetItemFrom> hookDLL_DuelListGetItemFrom;
        delegate int Del_DLL_DuelListGetItemID(int index);
        static Hook<Del_DLL_DuelListGetItemID> hookDLL_DuelListGetItemID;
        delegate int Del_DLL_DuelListGetItemMax();
        static Hook<Del_DLL_DuelListGetItemMax> hookDLL_DuelListGetItemMax;
        delegate int Del_DLL_DuelListGetItemMsg(int index);
        static Hook<Del_DLL_DuelListGetItemMsg> hookDLL_DuelListGetItemMsg;
        delegate int Del_DLL_DuelListGetItemTargetUniqueID(int index);
        static Hook<Del_DLL_DuelListGetItemTargetUniqueID> hookDLL_DuelListGetItemTargetUniqueID;
        delegate int Del_DLL_DuelListGetItemUniqueID(int index);
        static Hook<Del_DLL_DuelListGetItemUniqueID> hookDLL_DuelListGetItemUniqueID;
        delegate int Del_DLL_DuelListGetSelectMax();
        static Hook<Del_DLL_DuelListGetSelectMax> hookDLL_DuelListGetSelectMax;
        delegate int Del_DLL_DuelListGetSelectMin();
        static Hook<Del_DLL_DuelListGetSelectMin> hookDLL_DuelListGetSelectMin;
        delegate int Del_DLL_DuelListIsMultiMode();
        static Hook<Del_DLL_DuelListIsMultiMode> hookDLL_DuelListIsMultiMode;
        delegate int Del_DLL_DuelMyself();
        static Hook<Del_DLL_DuelMyself> hookDLL_DuelMyself;
        delegate int Del_DLL_DuelResultGetData(int player, IntPtr dst);
        static Hook<Del_DLL_DuelResultGetData> hookDLL_DuelResultGetData;
        delegate int Del_DLL_DuelResultGetMemo(int player, IntPtr dst);
        static Hook<Del_DLL_DuelResultGetMemo> hookDLL_DuelResultGetMemo;
        delegate int Del_DLL_DuelRival();
        static Hook<Del_DLL_DuelRival> hookDLL_DuelRival;
        delegate uint Del_DLL_DuelSearchCardByUniqueID(int uniqueId);
        static Hook<Del_DLL_DuelSearchCardByUniqueID> hookDLL_DuelSearchCardByUniqueID;
        delegate void Del_DLL_DuelSetCpuParam(int player, uint param);
        static Hook<Del_DLL_DuelSetCpuParam> hookDLL_DuelSetCpuParam;
        delegate void Del_DLL_DuelSetDuelLimitedType(uint limitedType);
        static Hook<Del_DLL_DuelSetDuelLimitedType> hookDLL_DuelSetDuelLimitedType;
        delegate void Del_DLL_DuelSetFirstPlayer(int player);
        static Hook<Del_DLL_DuelSetFirstPlayer> hookDLL_DuelSetFirstPlayer;
        delegate void Del_DLL_DuelSetMyPlayerNum(int player);
        static Hook<Del_DLL_DuelSetMyPlayerNum> hookDLL_DuelSetMyPlayerNum;
        delegate void Del_DLL_DuelSetPlayerType(int player, int type);
        static Hook<Del_DLL_DuelSetPlayerType> hookDLL_DuelSetPlayerType;
        delegate void Del_DLL_DuelSetRandomSeed(uint seed);
        static Hook<Del_DLL_DuelSetRandomSeed> hookDLL_DuelSetRandomSeed;
        delegate void Del_DLL_DuelSysClearWork();
        static Hook<Del_DLL_DuelSysClearWork> hookDLL_DuelSysClearWork;
        delegate int Del_DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf);
        static Hook<Del_DLL_DuelSysInitCustom> hookDLL_DuelSysInitCustom;
        delegate int Del_DLL_DuelSysInitQuestion(IntPtr pScript);
        static Hook<Del_DLL_DuelSysInitQuestion> hookDLL_DuelSysInitQuestion;
        delegate int Del_DLL_DuelSysInitRush();
        static Hook<Del_DLL_DuelSysInitRush> hookDLL_DuelSysInitRush;
        delegate void Del_DLL_DuelSysSetDeck2(int player, IntPtr mainDeck, int mainNum, IntPtr extraDeck, int extraNum, IntPtr sideDeck, int sideNum);
        static Hook<Del_DLL_DuelSysSetDeck2> hookDLL_DuelSysSetDeck2;
        delegate int Del_DLL_DuelWhichTurnNow();
        static Hook<Del_DLL_DuelWhichTurnNow> hookDLL_DuelWhichTurnNow;
        delegate int Del_DLL_FusionGetMaterialList(int uniqueId, IntPtr list);
        static Hook<Del_DLL_FusionGetMaterialList> hookDLL_FusionGetMaterialList;
        delegate int Del_DLL_FusionGetMonsterLevelInTuning(int wUniqueID);
        static Hook<Del_DLL_FusionGetMonsterLevelInTuning> hookDLL_FusionGetMonsterLevelInTuning;
        delegate int Del_DLL_FusionIsThisTunedMonsterInTuning(int wUniqueID);
        static Hook<Del_DLL_FusionIsThisTunedMonsterInTuning> hookDLL_FusionIsThisTunedMonsterInTuning;
        delegate void Del_DLL_SetCardExistWork(IntPtr pWork, int size, int count);
        static Hook<Del_DLL_SetCardExistWork> hookDLL_SetCardExistWork;
        delegate void Del_DLL_SetDuelChallenge(int flagbit);
        static Hook<Del_DLL_SetDuelChallenge> hookDLL_SetDuelChallenge;
        delegate void Del_DLL_SetDuelChallenge2(int player, int flagbit);
        static Hook<Del_DLL_SetDuelChallenge2> hookDLL_SetDuelChallenge2;
        delegate int Del_DLL_SetWorkMemory(IntPtr pWork);
        static Hook<Del_DLL_SetWorkMemory> hookDLL_SetWorkMemory;
        delegate int Del_DLL_DuelGetAttachedEffectList(IntPtr lpAffect);
        static Hook<Del_DLL_DuelGetAttachedEffectList> hookDLL_DuelGetAttachedEffectList;
        delegate void Del_DLL_DuelChangeCardIDByUniqueID(int uniqueId, int cardId);
        static Hook<Del_DLL_DuelChangeCardIDByUniqueID> hookDLL_DuelChangeCardIDByUniqueID;
        delegate void Del_DLL_SetRareByUniqueID(int uniqueId, int rare);
        static Hook<Del_DLL_SetRareByUniqueID> hookDLL_SetRareByUniqueID;
        delegate bool Del_DLL_DuelGetCantActIcon(int player, int locate, int index, int flag);
        static Hook<Del_DLL_DuelGetCantActIcon> hookDLL_DuelGetCantActIcon;

        static void InitProxyFunctions(IntPtr lib)
        {
            hookDLL_CardRareGetBufferSize = new Hook<Del_DLL_CardRareGetBufferSize>(DLL_CardRareGetBufferSize, PInvoke.GetProcAddress(lib, "DLL_CardRareGetBufferSize"));
            hookDLL_CardRareGetRareByUniqueID = new Hook<Del_DLL_CardRareGetRareByUniqueID>(DLL_CardRareGetRareByUniqueID, PInvoke.GetProcAddress(lib, "DLL_CardRareGetRareByUniqueID"));
            hookDLL_CardRareSetBuffer = new Hook<Del_DLL_CardRareSetBuffer>(DLL_CardRareSetBuffer, PInvoke.GetProcAddress(lib, "DLL_CardRareSetBuffer"));
            hookDLL_CardRareSetRare = new Hook<Del_DLL_CardRareSetRare>(DLL_CardRareSetRare, PInvoke.GetProcAddress(lib, "DLL_CardRareSetRare"));
            hookDLL_DeulIsThisEffectiveMonsterWithDual = new Hook<Del_DLL_DeulIsThisEffectiveMonsterWithDual>(DLL_DeulIsThisEffectiveMonsterWithDual, PInvoke.GetProcAddress(lib, "DLL_DeulIsThisEffectiveMonsterWithDual"));
            hookDLL_DlgProcGetSummoningMonsterUniqueID = new Hook<Del_DLL_DlgProcGetSummoningMonsterUniqueID>(DLL_DlgProcGetSummoningMonsterUniqueID, PInvoke.GetProcAddress(lib, "DLL_DlgProcGetSummoningMonsterUniqueID"));
            hookDLL_DuelCanIDoPutMonster = new Hook<Del_DLL_DuelCanIDoPutMonster>(DLL_DuelCanIDoPutMonster, PInvoke.GetProcAddress(lib, "DLL_DuelCanIDoPutMonster"));
            hookDLL_DuelCanIDoSpecialSummon = new Hook<Del_DLL_DuelCanIDoSpecialSummon>(DLL_DuelCanIDoSpecialSummon, PInvoke.GetProcAddress(lib, "DLL_DuelCanIDoSpecialSummon"));
            hookDLL_DuelCanIDoSummonMonster = new Hook<Del_DLL_DuelCanIDoSummonMonster>(DLL_DuelCanIDoSummonMonster, PInvoke.GetProcAddress(lib, "DLL_DuelCanIDoSummonMonster"));
            hookDLL_DuelComGetCommandMask = new Hook<Del_DLL_DuelComGetCommandMask>(DLL_DuelComGetCommandMask, PInvoke.GetProcAddress(lib, "DLL_DuelComGetCommandMask"));
            hookDLL_DuelComGetMovablePhase = new Hook<Del_DLL_DuelComGetMovablePhase>(DLL_DuelComGetMovablePhase, PInvoke.GetProcAddress(lib, "DLL_DuelComGetMovablePhase"));
            hookDLL_DUELCOMGetPosMaskOfThisHand = new Hook<Del_DLL_DUELCOMGetPosMaskOfThisHand>(DLL_DUELCOMGetPosMaskOfThisHand, PInvoke.GetProcAddress(lib, "DLL_DUELCOMGetPosMaskOfThisHand"));
            hookDLL_DUELCOMGetRecommendSide = new Hook<Del_DLL_DUELCOMGetRecommendSide>(DLL_DUELCOMGetRecommendSide, PInvoke.GetProcAddress(lib, "DLL_DUELCOMGetRecommendSide"));
            hookDLL_DuelComGetTextIDOfThisCommand = new Hook<Del_DLL_DuelComGetTextIDOfThisCommand>(DLL_DuelComGetTextIDOfThisCommand, PInvoke.GetProcAddress(lib, "DLL_DuelComGetTextIDOfThisCommand"));
            hookDLL_DuelComGetTextIDOfThisSummon = new Hook<Del_DLL_DuelComGetTextIDOfThisSummon>(DLL_DuelComGetTextIDOfThisSummon, PInvoke.GetProcAddress(lib, "DLL_DuelComGetTextIDOfThisSummon"));
            hookDLL_DuelDlgCanYesNoSkip = new Hook<Del_DLL_DuelDlgCanYesNoSkip>(DLL_DuelDlgCanYesNoSkip, PInvoke.GetProcAddress(lib, "DLL_DuelDlgCanYesNoSkip"));
            hookDLL_DuelDlgGetMixNum = new Hook<Del_DLL_DuelDlgGetMixNum>(DLL_DuelDlgGetMixNum, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetMixNum"));
            hookDLL_DuelDlgGetMixType = new Hook<Del_DLL_DuelDlgGetMixType>(DLL_DuelDlgGetMixType, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetMixType"));
            hookDLL_DuelDlgGetMixData = new Hook<Del_DLL_DuelDlgGetMixData>(DLL_DuelDlgGetMixData, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetMixData"));
            hookDLL_DuelDlgGetPosMaskOfThisSummon = new Hook<Del_DLL_DuelDlgGetPosMaskOfThisSummon>(DLL_DuelDlgGetPosMaskOfThisSummon, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetPosMaskOfThisSummon"));
            hookDLL_DuelDlgGetSelectItemEnable = new Hook<Del_DLL_DuelDlgGetSelectItemEnable>(DLL_DuelDlgGetSelectItemEnable, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetSelectItemEnable"));
            hookDLL_DuelDlgGetSelectItemNum = new Hook<Del_DLL_DuelDlgGetSelectItemNum>(DLL_DuelDlgGetSelectItemNum, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetSelectItemNum"));
            hookDLL_DuelDlgGetSelectItemStr = new Hook<Del_DLL_DuelDlgGetSelectItemStr>(DLL_DuelDlgGetSelectItemStr, PInvoke.GetProcAddress(lib, "DLL_DuelDlgGetSelectItemStr"));
            hookDLL_DuelGetAttackTargetMask = new Hook<Del_DLL_DuelGetAttackTargetMask>(DLL_DuelGetAttackTargetMask, PInvoke.GetProcAddress(lib, "DLL_DuelGetAttackTargetMask"));
            hookDLL_DuelGetCardBasicVal = new Hook<Del_DLL_DuelGetCardBasicVal>(DLL_DuelGetCardBasicVal, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardBasicVal"));
            hookDLL_DuelGetCardFace = new Hook<Del_DLL_DuelGetCardFace>(DLL_DuelGetCardFace, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardFace"));
            hookDLL_DuelGetCardIDByUniqueID2 = new Hook<Del_DLL_DuelGetCardIDByUniqueID2>(DLL_DuelGetCardIDByUniqueID2, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardIDByUniqueID2"));
            hookDLL_DuelGetCardInHand = new Hook<Del_DLL_DuelGetCardInHand>(DLL_DuelGetCardInHand, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardInHand"));
            hookDLL_DuelGetCardNum = new Hook<Del_DLL_DuelGetCardNum>(DLL_DuelGetCardNum, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardNum"));
            hookDLL_DuelGetCardPropByUniqueID = new Hook<Del_DLL_DuelGetCardPropByUniqueID>(DLL_DuelGetCardPropByUniqueID, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardPropByUniqueID"));
            hookDLL_DuelGetCardTurn = new Hook<Del_DLL_DuelGetCardTurn>(DLL_DuelGetCardTurn, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardTurn"));
            hookDLL_DuelGetCardUniqueID = new Hook<Del_DLL_DuelGetCardUniqueID>(DLL_DuelGetCardUniqueID, PInvoke.GetProcAddress(lib, "DLL_DuelGetCardUniqueID"));
            hookDLL_DuelGetCurrentDmgStep = new Hook<Del_DLL_DuelGetCurrentDmgStep>(DLL_DuelGetCurrentDmgStep, PInvoke.GetProcAddress(lib, "DLL_DuelGetCurrentDmgStep"));
            hookDLL_DuelGetCurrentPhase = new Hook<Del_DLL_DuelGetCurrentPhase>(DLL_DuelGetCurrentPhase, PInvoke.GetProcAddress(lib, "DLL_DuelGetCurrentPhase"));
            hookDLL_DuelGetCurrentStep = new Hook<Del_DLL_DuelGetCurrentStep>(DLL_DuelGetCurrentStep, PInvoke.GetProcAddress(lib, "DLL_DuelGetCurrentStep"));
            hookDLL_DuelGetDuelFinish = new Hook<Del_DLL_DuelGetDuelFinish>(DLL_DuelGetDuelFinish, PInvoke.GetProcAddress(lib, "DLL_DuelGetDuelFinish"));
            hookDLL_DuelGetDuelFinishCardID = new Hook<Del_DLL_DuelGetDuelFinishCardID>(DLL_DuelGetDuelFinishCardID, PInvoke.GetProcAddress(lib, "DLL_DuelGetDuelFinishCardID"));
            hookDLL_DuelGetDuelFlagDeckReverse = new Hook<Del_DLL_DuelGetDuelFlagDeckReverse>(DLL_DuelGetDuelFlagDeckReverse, PInvoke.GetProcAddress(lib, "DLL_DuelGetDuelFlagDeckReverse"));
            hookDLL_DuelGetDuelResult = new Hook<Del_DLL_DuelGetDuelResult>(DLL_DuelGetDuelResult, PInvoke.GetProcAddress(lib, "DLL_DuelGetDuelResult"));
            hookDLL_DuelGetFldAffectIcon = new Hook<Del_DLL_DuelGetFldAffectIcon>(DLL_DuelGetFldAffectIcon, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldAffectIcon"));
            hookDLL_DuelGetFldMonstOrgLevel = new Hook<Del_DLL_DuelGetFldMonstOrgLevel>(DLL_DuelGetFldMonstOrgLevel, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldMonstOrgLevel"));
            hookDLL_DuelGetFldMonstOrgRank = new Hook<Del_DLL_DuelGetFldMonstOrgRank>(DLL_DuelGetFldMonstOrgRank, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldMonstOrgRank"));
            hookDLL_DuelGetFldMonstOrgType = new Hook<Del_DLL_DuelGetFldMonstOrgType>(DLL_DuelGetFldMonstOrgType, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldMonstOrgType"));
            hookDLL_DuelGetFldMonstRank = new Hook<Del_DLL_DuelGetFldMonstRank>(DLL_DuelGetFldMonstRank, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldMonstRank"));
            hookDLL_DuelGetFldPendOrgScale = new Hook<Del_DLL_DuelGetFldPendOrgScale>(DLL_DuelGetFldPendOrgScale, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldPendOrgScale"));
            hookDLL_DuelGetFldPendScale = new Hook<Del_DLL_DuelGetFldPendScale>(DLL_DuelGetFldPendScale, PInvoke.GetProcAddress(lib, "DLL_DuelGetFldPendScale"));
            hookDLL_DuelGetHandCardOpen = new Hook<Del_DLL_DuelGetHandCardOpen>(DLL_DuelGetHandCardOpen, PInvoke.GetProcAddress(lib, "DLL_DuelGetHandCardOpen"));
            hookDLL_DuelGetLP = new Hook<Del_DLL_DuelGetLP>(DLL_DuelGetLP, PInvoke.GetProcAddress(lib, "DLL_DuelGetLP"));
            hookDLL_DuelGetThisCardCounter = new Hook<Del_DLL_DuelGetThisCardCounter>(DLL_DuelGetThisCardCounter, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardCounter"));
            hookDLL_DuelGetThisCardDirectFlag = new Hook<Del_DLL_DuelGetThisCardDirectFlag>(DLL_DuelGetThisCardDirectFlag, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardDirectFlag"));
            hookDLL_DuelGetThisCardEffectFlags = new Hook<Del_DLL_DuelGetThisCardEffectFlags>(DLL_DuelGetThisCardEffectFlags, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardEffectFlags"));
            hookDLL_DuelGetThisCardEffectIDAtChain = new Hook<Del_DLL_DuelGetThisCardEffectIDAtChain>(DLL_DuelGetThisCardEffectIDAtChain, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardEffectIDAtChain"));
            hookDLL_DuelGetThisCardEffectList = new Hook<Del_DLL_DuelGetThisCardEffectList>(DLL_DuelGetThisCardEffectList, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardEffectList"));
            hookDLL_DuelGetThisCardOverlayNum = new Hook<Del_DLL_DuelGetThisCardOverlayNum>(DLL_DuelGetThisCardOverlayNum, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardOverlayNum"));
            hookDLL_DuelGetThisCardParameter = new Hook<Del_DLL_DuelGetThisCardParameter>(DLL_DuelGetThisCardParameter, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardParameter"));
            hookDLL_DuelGetThisCardShowParameter = new Hook<Del_DLL_DuelGetThisCardShowParameter>(DLL_DuelGetThisCardShowParameter, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardShowParameter"));
            hookDLL_DuelGetThisCardTurnCounter = new Hook<Del_DLL_DuelGetThisCardTurnCounter>(DLL_DuelGetThisCardTurnCounter, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisCardTurnCounter"));
            hookDLL_DuelGetThisMonsterFightableOnEffect = new Hook<Del_DLL_DuelGetThisMonsterFightableOnEffect>(DLL_DuelGetThisMonsterFightableOnEffect, PInvoke.GetProcAddress(lib, "DLL_DuelGetThisMonsterFightableOnEffect"));
            hookDLL_DuelGetTopCardIndex = new Hook<Del_DLL_DuelGetTopCardIndex>(DLL_DuelGetTopCardIndex, PInvoke.GetProcAddress(lib, "DLL_DuelGetTopCardIndex"));
            hookDLL_DuelGetTrapMonstBasicVal = new Hook<Del_DLL_DuelGetTrapMonstBasicVal>(DLL_DuelGetTrapMonstBasicVal, PInvoke.GetProcAddress(lib, "DLL_DuelGetTrapMonstBasicVal"));
            hookDLL_DuelGetTurnNum = new Hook<Del_DLL_DuelGetTurnNum>(DLL_DuelGetTurnNum, PInvoke.GetProcAddress(lib, "DLL_DuelGetTurnNum"));
            hookDLL_DuelIsHuman = new Hook<Del_DLL_DuelIsHuman>(DLL_DuelIsHuman, PInvoke.GetProcAddress(lib, "DLL_DuelIsHuman"));
            hookDLL_DuelIsMyself = new Hook<Del_DLL_DuelIsMyself>(DLL_DuelIsMyself, PInvoke.GetProcAddress(lib, "DLL_DuelIsMyself"));
            hookDLL_DuelIsReplayMode = new Hook<Del_DLL_DuelIsReplayMode>(DLL_DuelIsReplayMode, PInvoke.GetProcAddress(lib, "DLL_DuelIsReplayMode"));
            hookDLL_DuelIsRival = new Hook<Del_DLL_DuelIsRival>(DLL_DuelIsRival, PInvoke.GetProcAddress(lib, "DLL_DuelIsRival"));
            hookDLL_DuelIsThisCardExist = new Hook<Del_DLL_DuelIsThisCardExist>(DLL_DuelIsThisCardExist, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisCardExist"));
            hookDLL_DuelIsThisContinuousCard = new Hook<Del_DLL_DuelIsThisContinuousCard>(DLL_DuelIsThisContinuousCard, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisContinuousCard"));
            hookDLL_DuelIsThisEffectiveMonster = new Hook<Del_DLL_DuelIsThisEffectiveMonster>(DLL_DuelIsThisEffectiveMonster, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisEffectiveMonster"));
            hookDLL_DuelIsThisEquipCard = new Hook<Del_DLL_DuelIsThisEquipCard>(DLL_DuelIsThisEquipCard, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisEquipCard"));
            hookDLL_DuelIsThisMagic = new Hook<Del_DLL_DuelIsThisMagic>(DLL_DuelIsThisMagic, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisMagic"));
            hookDLL_DuelIsThisNormalMonster = new Hook<Del_DLL_DuelIsThisNormalMonster>(DLL_DuelIsThisNormalMonster, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisNormalMonster"));
            hookDLL_DuelIsThisNormalMonsterInGrave = new Hook<Del_DLL_DuelIsThisNormalMonsterInGrave>(DLL_DuelIsThisNormalMonsterInGrave, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisNormalMonsterInGrave"));
            hookDLL_DuelIsThisNormalMonsterInHand = new Hook<Del_DLL_DuelIsThisNormalMonsterInHand>(DLL_DuelIsThisNormalMonsterInHand, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisNormalMonsterInHand"));
            hookDLL_DuelIsThisQuickDuel = new Hook<Del_DLL_DuelIsThisQuickDuel>(DLL_DuelIsThisQuickDuel, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisQuickDuel"));
            hookDLL_DuelIsThisTrap = new Hook<Del_DLL_DuelIsThisTrap>(DLL_DuelIsThisTrap, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisTrap"));
            hookDLL_DuelIsThisTrapMonster = new Hook<Del_DLL_DuelIsThisTrapMonster>(DLL_DuelIsThisTrapMonster, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisTrapMonster"));
            hookDLL_DuelIsThisTunerMonster = new Hook<Del_DLL_DuelIsThisTunerMonster>(DLL_DuelIsThisTunerMonster, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisTunerMonster"));
            hookDLL_DuelIsThisZoneAvailable = new Hook<Del_DLL_DuelIsThisZoneAvailable>(DLL_DuelIsThisZoneAvailable, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisZoneAvailable"));
            hookDLL_DuelIsThisZoneAvailable2 = new Hook<Del_DLL_DuelIsThisZoneAvailable2>(DLL_DuelIsThisZoneAvailable2, PInvoke.GetProcAddress(lib, "DLL_DuelIsThisZoneAvailable2"));
            hookDLL_DuelListGetCardAttribute = new Hook<Del_DLL_DuelListGetCardAttribute>(DLL_DuelListGetCardAttribute, PInvoke.GetProcAddress(lib, "DLL_DuelListGetCardAttribute"));
            hookDLL_DuelListGetItemAttribute = new Hook<Del_DLL_DuelListGetItemAttribute>(DLL_DuelListGetItemAttribute, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemAttribute"));
            hookDLL_DuelListGetItemFrom = new Hook<Del_DLL_DuelListGetItemFrom>(DLL_DuelListGetItemFrom, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemFrom"));
            hookDLL_DuelListGetItemID = new Hook<Del_DLL_DuelListGetItemID>(DLL_DuelListGetItemID, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemID"));
            hookDLL_DuelListGetItemMax = new Hook<Del_DLL_DuelListGetItemMax>(DLL_DuelListGetItemMax, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemMax"));
            hookDLL_DuelListGetItemMsg = new Hook<Del_DLL_DuelListGetItemMsg>(DLL_DuelListGetItemMsg, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemMsg"));
            hookDLL_DuelListGetItemTargetUniqueID = new Hook<Del_DLL_DuelListGetItemTargetUniqueID>(DLL_DuelListGetItemTargetUniqueID, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemTargetUniqueID"));
            hookDLL_DuelListGetItemUniqueID = new Hook<Del_DLL_DuelListGetItemUniqueID>(DLL_DuelListGetItemUniqueID, PInvoke.GetProcAddress(lib, "DLL_DuelListGetItemUniqueID"));
            hookDLL_DuelListGetSelectMax = new Hook<Del_DLL_DuelListGetSelectMax>(DLL_DuelListGetSelectMax, PInvoke.GetProcAddress(lib, "DLL_DuelListGetSelectMax"));
            hookDLL_DuelListGetSelectMin = new Hook<Del_DLL_DuelListGetSelectMin>(DLL_DuelListGetSelectMin, PInvoke.GetProcAddress(lib, "DLL_DuelListGetSelectMin"));
            hookDLL_DuelListIsMultiMode = new Hook<Del_DLL_DuelListIsMultiMode>(DLL_DuelListIsMultiMode, PInvoke.GetProcAddress(lib, "DLL_DuelListIsMultiMode"));
            hookDLL_DuelMyself = new Hook<Del_DLL_DuelMyself>(DLL_DuelMyself, PInvoke.GetProcAddress(lib, "DLL_DuelMyself"));
            hookDLL_DuelResultGetData = new Hook<Del_DLL_DuelResultGetData>(DLL_DuelResultGetData, PInvoke.GetProcAddress(lib, "DLL_DuelResultGetData"));
            hookDLL_DuelResultGetMemo = new Hook<Del_DLL_DuelResultGetMemo>(DLL_DuelResultGetMemo, PInvoke.GetProcAddress(lib, "DLL_DuelResultGetMemo"));
            hookDLL_DuelRival = new Hook<Del_DLL_DuelRival>(DLL_DuelRival, PInvoke.GetProcAddress(lib, "DLL_DuelRival"));
            hookDLL_DuelSearchCardByUniqueID = new Hook<Del_DLL_DuelSearchCardByUniqueID>(DLL_DuelSearchCardByUniqueID, PInvoke.GetProcAddress(lib, "DLL_DuelSearchCardByUniqueID"));
            hookDLL_DuelSetCpuParam = new Hook<Del_DLL_DuelSetCpuParam>(DLL_DuelSetCpuParam, PInvoke.GetProcAddress(lib, "DLL_DuelSetCpuParam"));
            hookDLL_DuelSetDuelLimitedType = new Hook<Del_DLL_DuelSetDuelLimitedType>(DLL_DuelSetDuelLimitedType, PInvoke.GetProcAddress(lib, "DLL_DuelSetDuelLimitedType"));
            hookDLL_DuelSetFirstPlayer = new Hook<Del_DLL_DuelSetFirstPlayer>(DLL_DuelSetFirstPlayer, PInvoke.GetProcAddress(lib, "DLL_DuelSetFirstPlayer"));
            hookDLL_DuelSetMyPlayerNum = new Hook<Del_DLL_DuelSetMyPlayerNum>(DLL_DuelSetMyPlayerNum, PInvoke.GetProcAddress(lib, "DLL_DuelSetMyPlayerNum"));
            hookDLL_DuelSetPlayerType = new Hook<Del_DLL_DuelSetPlayerType>(DLL_DuelSetPlayerType, PInvoke.GetProcAddress(lib, "DLL_DuelSetPlayerType"));
            hookDLL_DuelSetRandomSeed = new Hook<Del_DLL_DuelSetRandomSeed>(DLL_DuelSetRandomSeed, PInvoke.GetProcAddress(lib, "DLL_DuelSetRandomSeed"));
            hookDLL_DuelSysClearWork = new Hook<Del_DLL_DuelSysClearWork>(DLL_DuelSysClearWork, PInvoke.GetProcAddress(lib, "DLL_DuelSysClearWork"));
            hookDLL_DuelSysInitCustom = new Hook<Del_DLL_DuelSysInitCustom>(DLL_DuelSysInitCustom, PInvoke.GetProcAddress(lib, "DLL_DuelSysInitCustom"));
            hookDLL_DuelSysInitQuestion = new Hook<Del_DLL_DuelSysInitQuestion>(DLL_DuelSysInitQuestion, PInvoke.GetProcAddress(lib, "DLL_DuelSysInitQuestion"));
            hookDLL_DuelSysInitRush = new Hook<Del_DLL_DuelSysInitRush>(DLL_DuelSysInitRush, PInvoke.GetProcAddress(lib, "DLL_DuelSysInitRush"));
            hookDLL_DuelSysSetDeck2 = new Hook<Del_DLL_DuelSysSetDeck2>(DLL_DuelSysSetDeck2, PInvoke.GetProcAddress(lib, "DLL_DuelSysSetDeck2"));
            hookDLL_DuelWhichTurnNow = new Hook<Del_DLL_DuelWhichTurnNow>(DLL_DuelWhichTurnNow, PInvoke.GetProcAddress(lib, "DLL_DuelWhichTurnNow"));
            hookDLL_FusionGetMaterialList = new Hook<Del_DLL_FusionGetMaterialList>(DLL_FusionGetMaterialList, PInvoke.GetProcAddress(lib, "DLL_FusionGetMaterialList"));
            hookDLL_FusionGetMonsterLevelInTuning = new Hook<Del_DLL_FusionGetMonsterLevelInTuning>(DLL_FusionGetMonsterLevelInTuning, PInvoke.GetProcAddress(lib, "DLL_FusionGetMonsterLevelInTuning"));
            hookDLL_FusionIsThisTunedMonsterInTuning = new Hook<Del_DLL_FusionIsThisTunedMonsterInTuning>(DLL_FusionIsThisTunedMonsterInTuning, PInvoke.GetProcAddress(lib, "DLL_FusionIsThisTunedMonsterInTuning"));
            hookDLL_SetCardExistWork = new Hook<Del_DLL_SetCardExistWork>(DLL_SetCardExistWork, PInvoke.GetProcAddress(lib, "DLL_SetCardExistWork"));
            hookDLL_SetDuelChallenge = new Hook<Del_DLL_SetDuelChallenge>(DLL_SetDuelChallenge, PInvoke.GetProcAddress(lib, "DLL_SetDuelChallenge"));
            hookDLL_SetDuelChallenge2 = new Hook<Del_DLL_SetDuelChallenge2>(DLL_SetDuelChallenge2, PInvoke.GetProcAddress(lib, "DLL_SetDuelChallenge2"));
            hookDLL_SetWorkMemory = new Hook<Del_DLL_SetWorkMemory>(DLL_SetWorkMemory, PInvoke.GetProcAddress(lib, "DLL_SetWorkMemory"));
            hookDLL_DuelGetAttachedEffectList = new Hook<Del_DLL_DuelGetAttachedEffectList>(DLL_DuelGetAttachedEffectList, PInvoke.GetProcAddress(lib, "DLL_DuelGetAttachedEffectList"));
            hookDLL_DuelChangeCardIDByUniqueID = new Hook<Del_DLL_DuelChangeCardIDByUniqueID>(DLL_DuelChangeCardIDByUniqueID, PInvoke.GetProcAddress(lib, "DLL_DuelChangeCardIDByUniqueID"));
            hookDLL_SetRareByUniqueID = new Hook<Del_DLL_SetRareByUniqueID>(DLL_SetRareByUniqueID, PInvoke.GetProcAddress(lib, "DLL_SetRareByUniqueID"));
            hookDLL_DuelGetCantActIcon = new Hook<Del_DLL_DuelGetCantActIcon>(DLL_DuelGetCantActIcon, PInvoke.GetProcAddress(lib, "DLL_DuelGetCantActIcon"));
        }

        static uint DLL_CardRareGetBufferSize()
        {
            if (IsPvpDuel)
            {
                return 0;
            }
            else
            {
                return hookDLL_CardRareGetBufferSize.Original();
            }
        }

        static int DLL_CardRareGetRareByUniqueID(int uniqueId)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_CardRareGetRareByUniqueID, uniqueId);
            }
            else
            {
                return hookDLL_CardRareGetRareByUniqueID.Original(uniqueId);
            }
        }

        static void DLL_CardRareSetBuffer(IntPtr pBuf)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_CardRareSetBuffer.Original(pBuf);
            }
        }

        static void DLL_CardRareSetRare(IntPtr pBuf, IntPtr rare0, IntPtr rare1, IntPtr rare2, IntPtr rare3)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_CardRareSetRare.Original(pBuf, rare0, rare1, rare2, rare3);
            }
        }

        static bool DLL_DeulIsThisEffectiveMonsterWithDual(int player, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DeulIsThisEffectiveMonsterWithDual, player, index) != 0;
            }
            else
            {
                return hookDLL_DeulIsThisEffectiveMonsterWithDual.Original(player, index);
            }
        }

        static int DLL_DlgProcGetSummoningMonsterUniqueID()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DlgProcGetSummoningMonsterUniqueID);
            }
            else
            {
                return hookDLL_DlgProcGetSummoningMonsterUniqueID.Original();
            }
        }

        static uint DLL_DuelCanIDoPutMonster(int player)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelCanIDoPutMonster, player);
            }
            else
            {
                return hookDLL_DuelCanIDoPutMonster.Original(player);
            }
        }

        static bool DLL_DuelCanIDoSpecialSummon(int player)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelCanIDoSpecialSummon, player) != 0;
            }
            else
            {
                return hookDLL_DuelCanIDoSpecialSummon.Original(player);
            }
        }

        static bool DLL_DuelCanIDoSummonMonster(int player)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelCanIDoSummonMonster, player) != 0;
            }
            else
            {
                return hookDLL_DuelCanIDoSummonMonster.Original(player);
            }
        }

        static uint DLL_DuelComGetCommandMask(int player, int position, int index)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelComGetCommandMask, player, position, index);
            }
            else
            {
                return hookDLL_DuelComGetCommandMask.Original(player, position, index);
            }
        }

        static uint DLL_DuelComGetMovablePhase()
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelComGetMovablePhase);
            }
            else
            {
                return hookDLL_DuelComGetMovablePhase.Original();
            }
        }

        static uint DLL_DUELCOMGetPosMaskOfThisHand(int player, int index, int commandId)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DUELCOMGetPosMaskOfThisHand, player, index, commandId);
            }
            else
            {
                return hookDLL_DUELCOMGetPosMaskOfThisHand.Original(player, index, commandId);
            }
        }

        static int DLL_DUELCOMGetRecommendSide()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DUELCOMGetRecommendSide);
            }
            else
            {
                return hookDLL_DUELCOMGetRecommendSide.Original();
            }
        }

        static uint DLL_DuelComGetTextIDOfThisCommand(int player, int position, int index)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelComGetTextIDOfThisCommand, player, position, index);
            }
            else
            {
                return hookDLL_DuelComGetTextIDOfThisCommand.Original(player, position, index);
            }
        }

        static uint DLL_DuelComGetTextIDOfThisSummon(int player, int position, int index)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelComGetTextIDOfThisSummon, player, position, index);
            }
            else
            {
                return hookDLL_DuelComGetTextIDOfThisSummon.Original(player, position, index);
            }
        }

        static int DLL_DuelDlgCanYesNoSkip()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgCanYesNoSkip);
            }
            else
            {
                return hookDLL_DuelDlgCanYesNoSkip.Original();
            }
        }

        static int DLL_DuelDlgGetMixNum()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetMixNum);
            }
            else
            {
                return hookDLL_DuelDlgGetMixNum.Original();
            }
        }

        static int DLL_DuelDlgGetMixData(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetMixData, index);
            }
            else
            {
                return hookDLL_DuelDlgGetMixData.Original(index);
            }
        }

        static int DLL_DuelDlgGetMixType(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetMixType, index);
            }
            else
            {
                return hookDLL_DuelDlgGetMixType.Original(index);
            }
        }

        static int DLL_DuelDlgGetPosMaskOfThisSummon()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetPosMaskOfThisSummon);
            }
            else
            {
                return hookDLL_DuelDlgGetPosMaskOfThisSummon.Original();
            }
        }

        static int DLL_DuelDlgGetSelectItemEnable(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetSelectItemEnable, index);
            }
            else
            {
                return hookDLL_DuelDlgGetSelectItemEnable.Original(index);
            }
        }

        static int DLL_DuelDlgGetSelectItemNum()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetSelectItemNum);
            }
            else
            {
                return hookDLL_DuelDlgGetSelectItemNum.Original();
            }
        }

        static int DLL_DuelDlgGetSelectItemStr(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelDlgGetSelectItemStr, index);
            }
            else
            {
                return hookDLL_DuelDlgGetSelectItemStr.Original(index);
            }
        }

        static int DLL_DuelGetAttackTargetMask(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetAttackTargetMask, player, locate);
            }
            else
            {
                return hookDLL_DuelGetAttackTargetMask.Original(player, locate);
            }
        }

        static void DLL_DuelGetCardBasicVal(int player, int pos, int index, ref PvpBasicVal pVal)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_DuelGetCardBasicVal, player, pos, index);
                if (result != null && result.Data != null)
                {
                    pVal = pvpEngineState.StructFromByteArray<PvpBasicVal>(result.Data);
                }
                else
                {
                    pVal = default(PvpBasicVal);
                }
            }
            else
            {
                hookDLL_DuelGetCardBasicVal.Original(player, pos, index, ref pVal);
            }
        }

        static int DLL_DuelGetCardFace(int player, int position, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardFace, player, position, index);
            }
            else
            {
                return hookDLL_DuelGetCardFace.Original(player, position, index);
            }
        }

        static uint DLL_DuelGetCardIDByUniqueID2(int uniqueId)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardIDByUniqueID2, uniqueId);
            }
            else
            {
                return hookDLL_DuelGetCardIDByUniqueID2.Original(uniqueId);
            }
        }

        static uint DLL_DuelGetCardInHand(int player)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardInHand, player);
            }
            else
            {
                return hookDLL_DuelGetCardInHand.Original(player);
            }
        }

        static int DLL_DuelGetCardNum(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardNum, player, locate);
            }
            else
            {
                return hookDLL_DuelGetCardNum.Original(player, locate);
            }
        }

        static IntPtr DLL_DuelGetCardPropByUniqueID(int uniqueId)
        {
            if (IsPvpDuel)
            {
                if (CardPropMem == IntPtr.Zero)
                {
                    CardPropMem = Marshal.AllocHGlobal(sizeof(int));
                }
                *(int*)CardPropMem = pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardPropByUniqueID, uniqueId);
                return CardPropMem;
            }
            else
            {
                return hookDLL_DuelGetCardPropByUniqueID.Original(uniqueId);
            }
        }

        static int DLL_DuelGetCardTurn(int player, int position, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardTurn, player, position, index);
            }
            else
            {
                return hookDLL_DuelGetCardTurn.Original(player, position, index);
            }
        }

        static int DLL_DuelGetCardUniqueID(int player, int position, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCardUniqueID, player, position, index);
            }
            else
            {
                return hookDLL_DuelGetCardUniqueID.Original(player, position, index);
            }
        }

        static uint DLL_DuelGetCurrentDmgStep()
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCurrentDmgStep);
            }
            else
            {
                return hookDLL_DuelGetCurrentDmgStep.Original();
            }
        }

        static uint DLL_DuelGetCurrentPhase()
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCurrentPhase);
            }
            else
            {
                return hookDLL_DuelGetCurrentPhase.Original();
            }
        }

        static uint DLL_DuelGetCurrentStep()
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetCurrentStep);
            }
            else
            {
                return hookDLL_DuelGetCurrentStep.Original();
            }
        }

        static int DLL_DuelGetDuelFinish()
        {
            if (IsPvpDuel)
            {
                return DuelEndFinish;
                //return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetDuelFinish);
            }
            else
            {
                return hookDLL_DuelGetDuelFinish.Original();
            }
        }

        static int DLL_DuelGetDuelFinishCardID()
        {
            if (IsPvpDuel)
            {
                return DuelEndFinishCardID;
                //return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetDuelFinishCardID);
            }
            else
            {
                return hookDLL_DuelGetDuelFinishCardID.Original();
            }
        }

        static bool DLL_DuelGetDuelFlagDeckReverse()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetDuelFlagDeckReverse) != 0;
            }
            else
            {
                return hookDLL_DuelGetDuelFlagDeckReverse.Original();
            }
        }

        static int DLL_DuelGetDuelResult()
        {
            if (IsPvpDuel)
            {
                return DuelEndResult;
                //return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetDuelResult);
            }
            else
            {
                return hookDLL_DuelGetDuelResult.Original();
            }
        }

        static void DLL_DuelGetFldAffectIcon(int player, int locate, IntPtr ptr, int view_player)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_DuelGetFldAffectIcon, player, locate, view_player);
                if (result != null && result.Data != null && ptr != IntPtr.Zero)
                {
                    Marshal.Copy(result.Data, 0, ptr, result.Data.Length);
                }
            }
            else
            {
                hookDLL_DuelGetFldAffectIcon.Original(player, locate, ptr, view_player);
            }
        }

        static int DLL_DuelGetFldMonstOrgLevel(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetFldMonstOrgLevel, player, locate);
            }
            else
            {
                return hookDLL_DuelGetFldMonstOrgLevel.Original(player, locate);
            }
        }

        static int DLL_DuelGetFldMonstOrgRank(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetFldMonstOrgRank, player, locate);
            }
            else
            {
                return hookDLL_DuelGetFldMonstOrgRank.Original(player, locate);
            }
        }

        static int DLL_DuelGetFldMonstOrgType(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetFldMonstOrgType, player, locate);
            }
            else
            {
                return hookDLL_DuelGetFldMonstOrgType.Original(player, locate);
            }
        }

        static int DLL_DuelGetFldMonstRank(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetFldMonstRank, player, locate);
            }
            else
            {
                return hookDLL_DuelGetFldMonstRank.Original(player, locate);
            }
        }

        static int DLL_DuelGetFldPendOrgScale(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetFldPendOrgScale, player, locate);
            }
            else
            {
                return hookDLL_DuelGetFldPendOrgScale.Original(player, locate);
            }
        }

        static int DLL_DuelGetFldPendScale(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetFldPendScale, player, locate);
            }
            else
            {
                return hookDLL_DuelGetFldPendScale.Original(player, locate);
            }
        }

        static bool DLL_DuelGetHandCardOpen(int player, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetHandCardOpen, player, index) != 0;
            }
            else
            {
                return hookDLL_DuelGetHandCardOpen.Original(player, index);
            }
        }

        static int DLL_DuelGetLP(int player)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetLP, player);
            }
            else
            {
                return hookDLL_DuelGetLP.Original(player);
            }
        }

        static int DLL_DuelGetThisCardCounter(int player, int locate, int counter)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardCounter, player, locate, counter);
            }
            else
            {
                return hookDLL_DuelGetThisCardCounter.Original(player, locate, counter);
            }
        }

        static int DLL_DuelGetThisCardDirectFlag(int player, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardDirectFlag, player, index);
            }
            else
            {
                return hookDLL_DuelGetThisCardDirectFlag.Original(player, index);
            }
        }

        static int DLL_DuelGetThisCardEffectFlags(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardEffectFlags, player, locate);
            }
            else
            {
                return hookDLL_DuelGetThisCardEffectFlags.Original(player, locate);
            }
        }

        static int DLL_DuelGetThisCardEffectIDAtChain(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardEffectIDAtChain, player, locate);
            }
            else
            {
                return hookDLL_DuelGetThisCardEffectIDAtChain.Original(player, locate);
            }
        }

        static uint DLL_DuelGetThisCardEffectList(int player, int locate, IntPtr list)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_DuelGetThisCardEffectList, player, locate);
                if (result != null && result.Data != null)
                {
                    if (list == IntPtr.Zero)
                    {
                        return (uint)result.Value;
                    }
                    else
                    {
                        Marshal.Copy(result.Data, 0, list, result.Data.Length);
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return hookDLL_DuelGetThisCardEffectList.Original(player, locate, list);
            }
        }

        static int DLL_DuelGetThisCardOverlayNum(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardOverlayNum, player, locate);
            }
            else
            {
                return hookDLL_DuelGetThisCardOverlayNum.Original(player, locate);
            }
        }

        static uint DLL_DuelGetThisCardParameter(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardParameter, player, locate);
            }
            else
            {
                return hookDLL_DuelGetThisCardParameter.Original(player, locate);
            }
        }

        static int DLL_DuelGetThisCardShowParameter(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardShowParameter, player, locate);
            }
            else
            {
                return hookDLL_DuelGetThisCardShowParameter.Original(player, locate);
            }
        }

        static int DLL_DuelGetThisCardTurnCounter(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisCardTurnCounter, player, locate);
            }
            else
            {
                return hookDLL_DuelGetThisCardTurnCounter.Original(player, locate);
            }
        }

        static bool DLL_DuelGetThisMonsterFightableOnEffect(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetThisMonsterFightableOnEffect, player, locate) != 0;
            }
            else
            {
                return hookDLL_DuelGetThisMonsterFightableOnEffect.Original(player, locate);
            }
        }

        static int DLL_DuelGetTopCardIndex(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetTopCardIndex, player, locate);
            }
            else
            {
                return hookDLL_DuelGetTopCardIndex.Original(player, locate);
            }
        }

        static int DLL_DuelGetTrapMonstBasicVal(int cardId, ref PvpBasicVal pVal)
        {
            if (IsPvpDuel)
            {
                // TODO
            }
            return hookDLL_DuelGetTrapMonstBasicVal.Original(cardId, ref pVal);
        }

        public static uint DLL_DuelGetTurnNum()
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelGetTurnNum);
            }
            else
            {
                return hookDLL_DuelGetTurnNum.Original();
            }
        }

        static int DLL_DuelIsHuman(int player)
        {
            if (IsPvpDuel)
            {
                return 1;
            }
            else
            {
                return hookDLL_DuelIsHuman.Original(player);
            }
        }

        static int DLL_DuelIsMyself(int player)
        {
            if (IsPvpDuel)
            {
                return MyID == player ? 1 : 0;
            }
            else
            {
                return hookDLL_DuelIsMyself.Original(player);
            }
        }

        static int DLL_DuelIsReplayMode()
        {
            if (IsPvpDuel)
            {
                return 0;
            }
            else
            {
                return hookDLL_DuelIsReplayMode.Original();
            }
        }

        static int DLL_DuelIsRival(int player)
        {
            if (IsPvpDuel)
            {
                return player == RivalID ? 1 : 0;
            }
            else
            {
                return hookDLL_DuelIsRival.Original(player);
            }
        }

        static int DLL_DuelIsThisCardExist(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisCardExist, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisCardExist.Original(player, locate);
            }
        }

        static int DLL_DuelIsThisContinuousCard(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisContinuousCard, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisContinuousCard.Original(player, locate);
            }
        }

        static bool DLL_DuelIsThisEffectiveMonster(int player, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisEffectiveMonster, player, index) != 0;
            }
            else
            {
                return hookDLL_DuelIsThisEffectiveMonster.Original(player, index);
            }
        }

        static int DLL_DuelIsThisEquipCard(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisEquipCard, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisEquipCard.Original(player, locate);
            }
        }

        static bool DLL_DuelIsThisMagic(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisMagic, player, locate) != 0;
            }
            else
            {
                return hookDLL_DuelIsThisMagic.Original(player, locate);
            }
        }

        static int DLL_DuelIsThisNormalMonster(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisNormalMonster, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisNormalMonster.Original(player, locate);
            }
        }

        static bool DLL_DuelIsThisNormalMonsterInGrave(int player, int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisNormalMonsterInGrave, player, index) != 0;
            }
            else
            {
                return hookDLL_DuelIsThisNormalMonsterInGrave.Original(player, index);
            }
        }

        static bool DLL_DuelIsThisNormalMonsterInHand(int wCardID)
        {
            if (IsPvpDuel)
            {
                // TODO
            }
            return hookDLL_DuelIsThisNormalMonsterInHand.Original(wCardID);
        }

        static int DLL_DuelIsThisQuickDuel()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisQuickDuel);
            }
            else
            {
                return hookDLL_DuelIsThisQuickDuel.Original();
            }
        }

        static bool DLL_DuelIsThisTrap(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisTrap, player, locate) != 0;
            }
            else
            {
                return hookDLL_DuelIsThisTrap.Original(player, locate);
            }
        }

        static int DLL_DuelIsThisTrapMonster(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisTrapMonster, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisTrapMonster.Original(player, locate);
            }
        }

        static int DLL_DuelIsThisTunerMonster(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisTunerMonster, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisTunerMonster.Original(player, locate);
            }
        }

        static int DLL_DuelIsThisZoneAvailable(int player, int locate)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisZoneAvailable, player, locate);
            }
            else
            {
                return hookDLL_DuelIsThisZoneAvailable.Original(player, locate);
            }
        }

        static int DLL_DuelIsThisZoneAvailable2(int player, int locate, bool visibleOnly)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelIsThisZoneAvailable2, player, locate, visibleOnly ? 1 : 0);
            }
            else
            {
                return hookDLL_DuelIsThisZoneAvailable2.Original(player, locate, visibleOnly);
            }
        }

        static int DLL_DuelListGetCardAttribute(int iLookPlayer, int wUniqueID)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetCardAttribute, iLookPlayer, wUniqueID);
            }
            else
            {
                return hookDLL_DuelListGetCardAttribute.Original(iLookPlayer, wUniqueID);
            }
        }

        static int DLL_DuelListGetItemAttribute(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemAttribute, index);
            }
            else
            {
                return hookDLL_DuelListGetItemAttribute.Original(index);
            }
        }

        static int DLL_DuelListGetItemFrom(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemFrom, index);
            }
            else
            {
                return hookDLL_DuelListGetItemFrom.Original(index);
            }
        }

        static int DLL_DuelListGetItemID(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemID, index);
            }
            else
            {
                return hookDLL_DuelListGetItemID.Original(index);
            }
        }

        static int DLL_DuelListGetItemMax()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemMax);
            }
            else
            {
                return hookDLL_DuelListGetItemMax.Original();
            }
        }

        static int DLL_DuelListGetItemMsg(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemMsg, index);
            }
            else
            {
                return hookDLL_DuelListGetItemMsg.Original(index);
            }
        }

        static int DLL_DuelListGetItemTargetUniqueID(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemTargetUniqueID, index);
            }
            else
            {
                return hookDLL_DuelListGetItemTargetUniqueID.Original(index);
            }
        }

        static int DLL_DuelListGetItemUniqueID(int index)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetItemUniqueID, index);
            }
            else
            {
                return hookDLL_DuelListGetItemUniqueID.Original(index);
            }
        }

        static int DLL_DuelListGetSelectMax()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetSelectMax);
            }
            else
            {
                return hookDLL_DuelListGetSelectMax.Original();
            }
        }

        static int DLL_DuelListGetSelectMin()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListGetSelectMin);
            }
            else
            {
                return hookDLL_DuelListGetSelectMin.Original();
            }
        }

        static int DLL_DuelListIsMultiMode()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelListIsMultiMode);
            }
            else
            {
                return hookDLL_DuelListIsMultiMode.Original();
            }
        }

        public static int DLL_DuelMyself()
        {
            if (IsPvpDuel)
            {
                return MyID;
            }
            else
            {
                return hookDLL_DuelMyself.Original();
            }
        }

        static int DLL_DuelResultGetData(int player, IntPtr dst)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_DuelResultGetData, player);
                if (result != null && result.Data != null)
                {
                    if (dst == IntPtr.Zero)
                    {
                        return result.Value;
                    }
                    else
                    {
                        Marshal.Copy(result.Data, 0, dst, result.Data.Length);
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return hookDLL_DuelResultGetData.Original(player, dst);
            }
        }

        static int DLL_DuelResultGetMemo(int player, IntPtr dst)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_DuelResultGetMemo, player);
                if (result != null && result.Data != null)
                {
                    if (dst == IntPtr.Zero)
                    {
                        return result.Value;
                    }
                    else
                    {
                        Marshal.Copy(result.Data, 0, dst, result.Data.Length);
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return hookDLL_DuelResultGetMemo.Original(player, dst);
            }
        }

        static int DLL_DuelRival()
        {
            if (IsPvpDuel)
            {
                return RivalID;
            }
            else
            {
                return hookDLL_DuelRival.Original();
            }
        }

        static uint DLL_DuelSearchCardByUniqueID(int uniqueId)
        {
            if (IsPvpDuel)
            {
                return (uint)pvpEngineState.GetValue(PvpOperationType.DLL_DuelSearchCardByUniqueID, uniqueId);
            }
            else
            {
                return hookDLL_DuelSearchCardByUniqueID.Original(uniqueId);
            }
        }

        static void DLL_DuelSetCpuParam(int player, uint param)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSetCpuParam.Original(player, param);
            }
        }

        static void DLL_DuelSetDuelLimitedType(uint limitedType)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSetDuelLimitedType.Original(limitedType);
            }
        }

        static void DLL_DuelSetFirstPlayer(int player)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSetFirstPlayer.Original(player);
            }
        }

        static void DLL_DuelSetMyPlayerNum(int player)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSetMyPlayerNum.Original(player);
            }
        }

        static void DLL_DuelSetPlayerType(int player, int type)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSetPlayerType.Original(player, type);
            }
        }

        static void DLL_DuelSetRandomSeed(uint seed)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSetRandomSeed.Original(seed);
            }
        }

        static void DLL_DuelSysClearWork()
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSysClearWork.Original();
            }
        }

        static int DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf)
        {
            if (IsPvpDuel)
            {
                // Ignore
                return 1;
            }
            else
            {
                return hookDLL_DuelSysInitCustom.Original(fDuelType, tag, life0, life1, hand0, hand1, shuf);
            }
        }

        static int DLL_DuelSysInitQuestion(IntPtr pScript)
        {
            if (IsPvpDuel)
            {
                return 0;
            }
            else
            {
                return hookDLL_DuelSysInitQuestion.Original(pScript);
            }
        }

        static int DLL_DuelSysInitRush()
        {
            if (IsPvpDuel)
            {
                return 0;
            }
            else
            {
                return hookDLL_DuelSysInitRush.Original();
            }
        }

        static void DLL_DuelSysSetDeck2(int player, IntPtr mainDeck, int mainNum, IntPtr extraDeck, int extraNum, IntPtr sideDeck, int sideNum)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelSysSetDeck2.Original(player, mainDeck, mainNum, extraDeck, extraNum, sideDeck, sideNum);
            }
        }

        static int DLL_DuelWhichTurnNow()
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_DuelWhichTurnNow);
            }
            else
            {
                return hookDLL_DuelWhichTurnNow.Original();
            }
        }

        static int DLL_FusionGetMaterialList(int uniqueId, IntPtr list)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_FusionGetMaterialList, uniqueId);
                if (result != null && result.Data != null)
                {
                    if (list == IntPtr.Zero)
                    {
                        return result.Value;
                    }
                    else
                    {
                        Marshal.Copy(result.Data, 0, list, result.Data.Length);
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return hookDLL_FusionGetMaterialList.Original(uniqueId, list);
            }
        }

        static int DLL_FusionGetMonsterLevelInTuning(int wUniqueID)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_FusionGetMonsterLevelInTuning, wUniqueID);
            }
            else
            {
                return hookDLL_FusionGetMonsterLevelInTuning.Original(wUniqueID);
            }
        }

        static int DLL_FusionIsThisTunedMonsterInTuning(int wUniqueID)
        {
            if (IsPvpDuel)
            {
                return pvpEngineState.GetValue(PvpOperationType.DLL_FusionIsThisTunedMonsterInTuning, wUniqueID);
            }
            else
            {
                return hookDLL_FusionIsThisTunedMonsterInTuning.Original(wUniqueID);
            }
        }

        static void DLL_SetCardExistWork(IntPtr pWork, int size, int count)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_SetCardExistWork.Original(pWork, size, count);
            }
        }

        static void DLL_SetDuelChallenge(int flagbit)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_SetDuelChallenge.Original(flagbit);
            }
        }

        static void DLL_SetDuelChallenge2(int player, int flagbit)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_SetDuelChallenge2.Original(player, flagbit);
            }
        }

        static int DLL_SetWorkMemory(IntPtr pWork)
        {
            if (IsPvpDuel)
            {
                // TODO
            }
            return hookDLL_SetWorkMemory.Original(pWork);
        }

        static int DLL_DuelGetAttachedEffectList(IntPtr lpAffect)
        {
            if (IsPvpDuel)
            {
                PvpEngineOperationResult result = pvpEngineState.GetResult(PvpOperationType.DLL_DuelGetAttachedEffectList);
                if (result != null && result.Data != null)
                {
                    if (lpAffect == IntPtr.Zero)
                    {
                        return result.Value;
                    }
                    else
                    {
                        Marshal.Copy(result.Data, 0, lpAffect, result.Data.Length);
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return hookDLL_DuelGetAttachedEffectList.Original(lpAffect);
            }
        }

        static void DLL_DuelChangeCardIDByUniqueID(int uniqueId, int cardId)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_DuelChangeCardIDByUniqueID.Original(uniqueId, cardId);
            }
        }

        static void DLL_SetRareByUniqueID(int uniqueId, int rare)
        {
            if (IsPvpDuel)
            {
                // Ignore
            }
            else
            {
                hookDLL_SetRareByUniqueID.Original(uniqueId, rare);
            }
        }

        static bool DLL_DuelGetCantActIcon(int player, int locate, int index, int flag)
        {
            if (IsPvpDuel)
            {
                return false; // Ignore ???
            }
            else
            {
                return hookDLL_DuelGetCantActIcon.Original(player, locate, index, flag);
            }
        }
    }
}