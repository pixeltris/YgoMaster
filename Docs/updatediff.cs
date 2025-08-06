// Client version 2.4.0
// This file is generated using the 'updatediff' command in YgoMasterClient. This information is used to determine changes between client versions which impact YgoMaster.
// Run the command, diff against the old file, and use the changes to update code.

//==================================
// Card.cs
//==================================
/// <summary>
/// YgomGame.Card.Content.Frame
/// </summary>
enum CardFrame
{
    Normal,
    Effect,
    Ritual,
    Fusion,
    Oberisk,
    Osiris,
    Ra,
    Magic,
    Trap,
    Token,
    Sync,
    Dsync,
    Xyz,
    Pend,
    PendFx,
    XyzPend,
    SyncPend,
    FusionPend,
    Link,
    RitualPend,
}
/// <summary>
/// YgomGame.Card.Content.Kind
/// </summary>
enum CardKind
{
    Normal,
    Effect,
    Fusion,
    FusionFx,
    Ritual,
    RitualFx,
    Toon,
    Spirit,
    Union,
    Dual,
    Token,
    God,
    Dummy,
    Magic,
    Trap,
    Tuner,
    TunerFx,
    Sync,
    SyncFx,
    SyncTuner,
    Dtuner,
    Dsync,
    Xyz,
    XyzFx,
    Flip,
    Pend,
    PendFx,
    SpEffect,
    SpToon,
    SpSpirit,
    SpTuner,
    SpDtuner,
    FlipTuner,
    PendTuner,
    XyzPend,
    PendFlip,
    SyncPend,
    UnionTuner,
    RitualSpirit,
    FusionTuner,
    SpPend,
    FusionPend,
    Link,
    LinkFx,
    PendNTuner,
    PendSpirit,
    Maximum,
    RirualTunerFX,
    FusionTunerFX,
    TokenTuner,
    R_Fusion,
    R_FusionFX,
    RitualPend,
    RitualFlip,
}
/// <summary>
/// YgomGame.Card.Content.Icon
/// </summary>
enum CardIcon
{
    Null,
    Counter,
    Field,
    Equip,
    Continuous,
    QuickPlay,
    Ritual,
    Ritual_R,
}
//==================================
// Misc.cs
//==================================
/// <summary>
/// YgomGame.Menu.Common.TopicsBannerResourceBinder.BannerPatern
/// </summary>
enum TopicsBannerPatern
{
    NOTIFY = 1,
    ACCESSORY,
    PACK,
    GEM,
    SPECIAL,
    STANDARD,
    EVENT,
    DUELPASS,
    MAINTENANCE,
    UPDATE,
    DUELLIVE,
    STRUCTURE,
    CARDS,
    CUSTOM = 100,
}
/// <summary>
/// YgomGame.Card.Content.Rarity
/// </summary>
enum CardRarity
{
    None,
    Normal,
    Rare,
    SuperRare,
    UltraRare,
}
/// <summary>
/// YgomGame.Deck.SearchFilter.Setting.STYLE
/// </summary>
enum CardStyleRarity
{
    Normal,
    Shine,
    Royal,
    SIZE,
}
/// <summary>
/// YgomGame.Colosseum.ColosseumUtil.StandardRank
/// </summary>
enum StandardRank
{
    ROOKIE = 1,
    BRONZE,
    SILVER,
    GOLD,
    PLATINUM,
    DIAMOND,
    MASTER,
    TEMP08,
    TEMP09,
    TEMP10,
}
/// <summary>
/// YgomGame.Colosseum.ColosseumUtil.PlayMode
/// </summary>
enum PlayMode
{
    NONE,
    RANK,
    TOURNAMENT,
    ROOM,
    EXHIBITION,
    FREE,
    DUELISTCUP,
    RANKEVENT,
    TEAMMATCH,
    DUELTRIAL,
    WCS,
    VERSUS,
    WCS_FINAL,
    RATE,
    RDC,
    DICERALLY,
}
/// <summary>
/// YgomSystem.Network.ServerStatus
/// </summary>
enum ServerStatus
{
    NORMAL,
    INCIDENT,
    MAINTENANCE,
    MAINTENANCE_TEAM,
}
/// <summary>
/// YgomGame.Duel.Util.GameMode
/// </summary>
enum GameMode
{
    Normal,
    Free,
    Single,
    Rank,
    Tournament,
    TournamentSingle,
    Audience,
    Replay,
    RankSingle,
    SoloSingle,
    Room,
    Exhibition,
    DuelistCup,
    RankEvent,
    TeamMatch,
    DuelTrial,
    WCS,
    Versus,
    WcsFinal,
    Rate,
    RDC,
    Dicerally,
    Null,
}
/// <summary>
/// YgomGame.Duel.Util.PlatformID
/// </summary>
enum PlatformID
{
    Invalid,
    Android,
    iOS,
    Steam,
    PS4,
    NX,
    XboxOne,
    Stadia,
    PS5,
    XboxSX,
    Editor = 100,
}
/// <summary>
/// YgomGame.Solo.SoloModeUtil.ChapterStatus
/// </summary>
enum ChapterStatus
{
    UNOPEN = -1,
    OPEN,
    RENTAL_CLEAR,
    MYDECK_CLEAR,
    COMPLETE,
}
/// <summary>
/// YgomGame.Solo.SoloModeUtil.UnlockType
/// </summary>
enum ChapterUnlockType
{
    USER_LEVEL = 1,
    CHAPTER_OR,
    ITEM,
    CHAPTER_AND,
    HAS_ITEM,
}
/// <summary>
/// YgomGame.Solo.SoloModeUtil.DeckType
/// </summary>
enum SoloDeckType
{
    POSSESSION,
    STORY,
}
/// <summary>
/// YgomGame.Room.RoomEntryViewController.Mode
/// </summary>
enum RoomEntryViewController.Mode
{
    NORMAL,
    SPECTER,
}
/// <summary>
/// IDS_DECKEDIT.HOWTOGET_CATEGORY (off by 1?)
/// </summary>
enum HowToObtainCard
{
    None,
    Pack,// HOWTOGET_CATEGORY001
    SoloMode,// HOWTOGET_CATEGORY002
    Tournament,// HOWTOGET_CATEGORY003
    Exhibition,// HOWTOGET_CATEGORY004
    Craft,// HOWTOGET_CATEGORY005
    InitialDistributionStructure,// HOWTOGET_CATEGORY006
    SalesStructure,// HOWTOGET_CATEGORY007
    Mission,// HOWTOGET_CATEGORY008
    DuelResult,// HOWTOGET_CATEGORY009
    BundleDeals,// HOWTOGET_CATEGORY010
    SelectionPack,// HOWTOGET_CATEGORY011
    SecretPack,// HOWTOGET_CATEGORY012
    BonusPack,// HOWTOGET_CATEGORY013
    Other,// HOWTOGET_CATEGORY099
}
/// <summary>
/// IDS_SCORE (IDS_SCORE.DETAIL_XXX)
/// </summary>
enum DuelResultScore
{
    None,
    DuelVictory,
    Draw,
    ComebackVictory,
    QuickVictory,
    DeckOutVictory,
    SpecialVictories,
    NoDamage,
    LowLP,
    LPontheBrink,
    FewCardsLeft,
    CardsontheBrink,
    Over3000Damage,
    Over5000Damage,
    Over9999Damage,
    VictorybyEffectDamageOnly,
    Destroyed5Monsters,
    Destroyed7Monsters,
    Destroyed10Monsters,
    ActivatedSpell,
    ActivatedTrap,
    PerformedSpecialSummon,
    PerformedTributeSummon,
    PerformedFusionSummon,
    PerformedRitualSummon,
    PerformedSynchroSummon,
    PerformedXyzSummon,
    PerformedPendulumSummon,
    LinkSummon,
}
/// <summary>
/// YgomGame.Duel.DuelClient.Step
/// </summary>
enum DuelClientStep
{
    InitLoadRes,
    WaitLoadRes,
    InitializeProcess,
    FinishInitialize,
    WaitConnecting,
    InitEngine,
    InitSound,
    WaitSound,
    InitLoadSound,
    WaitLoadSound,
    WaitGameObjectInit,
    PrepareProcess,
    FinishPrepare,
    WaitCameraWork,
    ShowUpDuel,
    WaitShowUp,
    ExecDuel,
    EndDuel,
    WaitEndNetwork,
    DuelEnd,
    InitTerm,
    WaitEndViewClose,
    WaitTerm,
    End,
    WaitDestroy,
    ConnectingError,
    Beginning,
    InitSequenceStart = 0,
    InitSequenceEnd = 12,
}
/// <summary>
/// YgomGame.Duel.Util.PublicLevel
/// </summary>
enum DuelReplayCardVisibility
{
    AllClose,
    FrontOpen,
    AllOpen,
    Unknown = 255,
}
/// <summary>
/// YgomGame.Utility.ItemUtil.Category
/// </summary>
enum Category
{
    NONE,
    CONSUME,
    CARD,
    AVATAR,
    ICON,
    PROFILE_TAG,
    ICON_FRAME,
    PROTECTOR,
    DECK_CASE,
    FIELD,
    FIELD_OBJ,
    AVATAR_HOME,
    STRUCTURE,
    WALLPAPER,
    PACK_TICKET,
    DECK_LIMIT,
    REPLAY_LIMIT,
    CARD_FILE,
    COIN,
    BOOKMARK_LIMIT,
}
//==================================
// Duel.cs
//==================================
/// <summary>
/// YgomGame.Duel.Engine.ResultType
/// </summary>
enum DuelResultType
{
    None,
    Win,
    Lose,
    Draw,
    Time,
}
/// <summary>
/// YgomGame.Duel.Engine.CpuParam
/// </summary>
enum DuelCpuParam
{
    None,
    Def = -2147483648,
    Fool = 1073741824,
    Light = 536870912,
    MyTurnOnly = 268435456,
    AttackOnly = 67108864,
    Simple = 33554432,
    Simple2 = 16777216,
    Simples = 50331648,
}
/// <summary>
/// YgomGame.Duel.Engine.DuelType
/// </summary>
enum DuelType
{
    Normal,
    Extra,
    Tag,
    Quick,
    Rush,
}
/// <summary>
/// YgomGame.Duel.Engine.AffectType
/// </summary>
enum DuelAffectType
{
    Null,
    Equip,
    Permanent,
    Field,
    Bind,
    Power,
    Target,
    Disable,
    Chain = 256,
}
/// <summary>
/// YgomGame.Duel.Engine.BtlPropFlag
/// </summary>
enum DuelBtlPropFlag
{
    Turn = 1,
    Break,
    Damage = 4,
}
/// <summary>
/// YgomGame.Duel.Engine.CardLink
/// </summary>
enum DuelCardLink
{
    UL,
    U,
    UR,
    L,
    R,
    DL,
    D,
    DR,
}
/// <summary>
/// YgomGame.Duel.Engine.CardLinkBit
/// </summary>
enum DuelCardLinkBit
{
    UL = 1,
    U,
    UR = 4,
    L = 8,
    R = 16,
    DL = 32,
    D = 64,
    DR = 128,
}
/// <summary>
/// YgomGame.Duel.Engine.CardMoveType
/// </summary>
enum DuelCardMoveType
{
    Normal,
    Normal2,
    Summon,
    SpSummon,
    Activate,
    Set,
    Break,
    Explosion,
    Sacrifice,
    Draw,
    Drop,
    Search,
    Used,
    Put,
    Normal3,
    Cost,
    CostSacrifice,
    CostDrop,
}
/// <summary>
/// YgomGame.Duel.Engine.CommandBit
/// </summary>
enum DuelCommandBit
{
    Attack = 1,
    Look,
    SummonSp = 4,
    Action = 8,
    Summon = 16,
    Reverse = 32,
    SetMonst = 64,
    Set = 128,
    Pendulum = 256,
    TurnAtk = 512,
    TurnDef = 1024,
    Surrender = 2048,
    Decide = 4096,
    Draw = 8192,
}
/// <summary>
/// YgomGame.Duel.Engine.CommandType
/// </summary>
enum DuelCommandType
{
    Attack,
    Look,
    SummonSp,
    Action,
    Summon,
    Reverse,
    SetMonst,
    Set,
    Pendulum,
    TurnAtk,
    TurnDef,
    Surrender,
    Decide,
    Draw,
}
/// <summary>
/// YgomGame.Duel.Engine.CounterType
/// </summary>
enum DuelCounterType
{
    Magic,
    Normal,
    Clock,
    Hyper,
    Gem,
    Chronicle,
    Bushido,
    D,
    Shine,
    Gate,
    Worm,
    Deformer,
    Flower,
    Plant,
    Psycho,
    EarthBind,
    Junk,
    Genex,
    Dragonic,
    Ocean,
    BF,
    Death,
    Karakuri,
    Stone,
    Thunder,
    Donguri,
    Greed,
    Chaos,
    Double,
    Destiny,
    Orbital,
    Shark,
    Pumpkin,
    HopeSlash,
    Kattobing,
    Balloon,
    Yosen,
    Sound,
    Em,
    Kaiju,
    Defect,
    Athlete,
    Barrel,
    Summon,
    FireStar,
    Phantasm,
    Otoshidama,
    Ounokagi,
    Piece,
    Girl,
    Gardna,
    Alien,
    Ice,
    Venom,
    Fog,
    Guard,
    Wedge,
    Guard2,
    String,
    Houkai,
    Zushin,
    Predator,
    Scales,
    Police,
    Signal,
    Venemy,
    Burn,
    Illusion,
    GG,
    Rabbit,
    Kyoumei,
    Kyouai,
    Access,
    Shukudai,
    Shiki,
    Kyuzai,
    C,
    Osara,
    Max,
}
/// <summary>
/// YgomGame.Duel.Engine.CutinActivateType
/// </summary>
enum DuelCutinActivateType
{
    NoChain,
    FromField,
    FromHand,
    Activate,
    Effect,
    FldGrave,
    CostEffect,
}
/// <summary>
/// YgomGame.Duel.Engine.CutinSummonType
/// </summary>
enum DuelCutinSummonType
{
    Normal,
    Release1,
    Release2,
    Release3,
    Reverse,
    SpByEffect,
    SpNormal,
    ReSummon,
    PreSynchro,
    PreXyz,
    PrePendulum,
    Link,
}
/// <summary>
/// YgomGame.Duel.Engine.DamageType
/// </summary>
enum DuelDamageType
{
    ByEffect,
    ByBattle,
    ByCost,
    ByLost,
    Recover,
    ByPay,
}
/// <summary>
/// YgomGame.Duel.Engine.DialogEffectType
/// </summary>
enum DuelDialogEffectType
{
    None,
    All,
    More,
    Auto,
    Always,
}
/// <summary>
/// YgomGame.Duel.Engine.DialogInfo
/// </summary>
enum DuelDialogInfo
{
    CardName,
    CardName2,
    SelectItem,
    CardType,
    CardAttr,
    CardLevel,
    Coin,
    Dice,
    Dice2,
    DiceChange,
    NotHappen,
    CardAttr2,
    Info,
    Info2,
    Confirm,
}
/// <summary>
/// YgomGame.Duel.Engine.DialogMixTextType
/// </summary>
enum DuelDialogMixTextType
{
    Null,
    AddString,
    AddCr,
    InsString,
    InsStringNoColor,
    InsCard,
    InsType,
    InsAttr,
    InsNum,
    InsStringIfable,
}
/// <summary>
/// YgomGame.Duel.Engine.DialogOkType
/// </summary>
enum DuelDialogOkType
{
    Stop,
    Once,
    Forever,
    Sub,
}
/// <summary>
/// YgomGame.Duel.Engine.DialogRitualType
/// </summary>
enum DuelDialogRitualType
{
    Ritual,
    Multi,
    Atk,
    Sync,
    Link,
}
/// <summary>
/// YgomGame.Duel.Engine.DialogType
/// </summary>
enum DuelDialogType
{
    Ok,
    Info,
    Confirm,
    YesNo,
    Effect,
    Sort,
    Select,
    Phase,
    SelType,
    SelAttr,
    SelStand,
    SelCoin,
    SelDice,
    SelNum,
    Final,
    Result,
    Discard,
    Ritual,
    Update,
    Close,
}
/// <summary>
/// YgomGame.Duel.Engine.DmgStepType
/// </summary>
enum DuelDmgStepType
{
    Null,
    Start,
    BeforeCalc,
    DamageCalc,
    AfterCalc,
    End,
}
/// <summary>
/// YgomGame.Duel.Engine.FieldAnimeType
/// </summary>
enum DuelFieldAnimeType
{
    Null,
    CardMove,
    CardWarp,
    CardSwap,
}
/// <summary>
/// YgomGame.Duel.Engine.FinishType
/// </summary>
enum DuelFinishType
{
    None,
    Normal,
    NoDeck,
    TimeOut,
    Surrender,
    Failed,
    Exodia,
    Vija,
    YataLock,
    LastBattle,
    CountDown,
    Victory,
    Venom,
    Exodios,
    God,
    Gimmick,
    Gimmick2,
    Jackpot7,
    Miracle,
    RelaySoul,
    Ghostrick,
    Genohryu,
    Winners,
    Elephant,
    Exodia2,
    Exodia3,
    CiNo1000,
    Sekitori,
    Shukudai,
    FinishError = 100,
    FinishDisconnect,
    FinishNoContest,
    FinishEngineCrash = 105,
}
/// <summary>
/// YgomGame.Duel.Engine.LimitedType
/// </summary>
enum DuelLimitedType
{
    None,
    NormalSummon,
    SpecialSummon,
    Set,
    Tribute,
    ChangePos,
    Attack,
    Draw2,
    Turn20,
    Damage,
    Beginner,
    Beginner2,
    Vs2on1,
    Vs2on1_Hand,
    FirstDraw,
    Vs3on1,
    Survival_1on3 = 256,
    Survival_3on3,
    Survival_1on2,
    NoRandom = 16,
    OldRule = 32,
    OldRule2 = 64,
    OldField = 128,
}
/// <summary>
/// YgomGame.Duel.Engine.ListAttribute
/// </summary>
enum DuelListAttribute
{
    FromField = 1,
    FromHand,
    FromDeck = 4,
    FromGrave = 8,
    FromExtra = 16,
    FromExclude = 32,
    DisableEffect = 64,
    CantRevive = 128,
    FusionMaterial = 256,
    DemensionHole = 1024,
    LightForce = 2048,
    Targeted = 4096,
    Tuning = 8192,
    ByBattle = 16384,
    Opponent = 32768,
    Activate = 65536,
    Cost = 131072,
    End = 262144,
    ExtraExclude = 524288,
    FromMask = 63,
}
/// <summary>
/// YgomGame.Duel.Engine.ListType
/// </summary>
enum DuelListType
{
    Null,
    Fusion,
    Deck,
    Grave,
    Exclude,
    View,
    Select,
    SelectMax = 38,
    Selectable,
    SelectableMax = 71,
    SelUpTo,
    SelUpToMax = 104,
    SelFree,
    SelFreeMax = 137,
    BlindSelect,
    AutoSelect,
    SelAllCard,
    SelAllDeck,
    SelAllMonst,
    SelAllMonst2,
    SelAllGadget,
    SelAllIndeck,
    SelAllGunkan,
    SelAllNormal,
    SelAllNormal2,
}
/// <summary>
/// YgomGame.Duel.Engine.MenuActType
/// </summary>
enum DuelMenuActType
{
    Null,
    DrawPhase,
    MainPhase,
    BattlePhase,
    CheckTiming,
    CheckChain,
    SummonChance,
    Location,
    Selection,
    LockOn,
}
/// <summary>
/// YgomGame.Duel.Engine.MenuParamType
/// </summary>
enum DuelMenuParamType
{
    Force = -1,
    Cancel,
    Decide,
    TrueCancel,
    OnlyCancel,
    DecideCancel,
}
/// <summary>
/// YgomGame.Duel.Engine.Phase
/// </summary>
enum DuelPhase
{
    Draw,
    Standby,
    Main1,
    Battle,
    Main2,
    End,
    Null = 7,
}
/// <summary>
/// YgomGame.Duel.Engine.PlayerType
/// </summary>
enum DuelPlayerType
{
    Human,
    CPU,
    Remote,
    Replay,
    Replay2,
    None = -1,
}
/// <summary>
/// YgomSystem.Network.PvP.Command
/// </summary>
enum DuelPvpCommand
{
    ENTRY,
    INIT,
    WAIT,
    READY,
    COMMAND,
    EFFECT,
    CANCEL,
    RESULT,
    DBGCMD,
    CHEATCARD,
    CHAT,
    LIST,
    PHASE,
    SKILL,
    LEAVE,
    EXIT,
    RECOVERY,
    WATCH,
    SURRENDER,
    LATENCY,
    SEND,
    RECV,
    TIME,
    TURN,
    INPUTGUARD,
    DATA = 50,
    REPLAY = 60,
    FLIPINFO,
    TIMEUP = 97,
    FINISH,
    POLL,
    ERROR,
    FATAL = 900,
    CONNECT = 1000,
    RECONNECT,
    CLOSE = 1003,
    PING,
    PONG,
    MATCH,
    DROP,
    MATCH_UPDATE = 1010,
    MATCH_LIST,
    INFO,
}
/// <summary>
/// YgomGame.Duel.Engine.PvpCommandType
/// </summary>
enum DuelPvpCommandType
{
    Input,
    List,
    Dialog,
    Effect,
    Field,
    Data,
    Fusion,
    Time,
    ListFrom,
    FlipInfo,
    FinishAttack,
    MrkList,
    FusionNeed,
    TunerLevel,
    CutinActivate,
}
/// <summary>
/// YgomGame.Duel.Engine.PvpFieldType
/// </summary>
enum DuelPvpFieldType
{
    Prop = 1,
    Pos,
    Uid,
    Vals,
    Icon,
    Skill,
    Rare,
    Attack,
    Show,
    Step,
    SummoningUid,
    PosMask,
    AffectProp,
    End,
}
/// <summary>
/// YgomGame.Duel.Engine.RunCommandType
/// </summary>
enum DuelRunCommandType
{
    Null,
    PriWaitInput,
    PriCpuThinking,
    PriRunDialog,
    PriRunList,
}
/// <summary>
/// YgomGame.Duel.Engine.ShowParam
/// </summary>
enum DuelShowParam
{
    Null,
    Type,
    Attr,
    Card,
    Num,
    AttrMask,
    Dlg,
}
/// <summary>
/// YgomGame.Duel.Engine.SpSummonType
/// </summary>
enum DuelSpSummonType
{
    Fusion,
    SpFusion,
    Synchro,
    Ritual,
    Xyz,
    Pendulum,
    Link,
}
/// <summary>
/// YgomGame.Duel.Engine.StepType
/// </summary>
enum DuelStepType
{
    Null,
    Start,
    Battle,
    Damage,
    End,
}
/// <summary>
/// YgomGame.Duel.Engine.TagType
/// </summary>
enum DuelTagType
{
    Single,
    Tag,
    Team,
}
/// <summary>
/// YgomGame.Duel.Engine.ToEngineActType
/// </summary>
enum DuelToEngineActType
{
    DoCommand,
    CancelCommand,
    MovePhase,
    DebugCommand,
    DoDebug,
    CheatCard,
    DialogResult,
    ListSendBlindIndex,
    ListSendIndex,
    ListSendSelectMulti,
    ForceSurrender,
}
/// <summary>
/// YgomGame.Duel.Engine.ViewType
/// </summary>
enum DuelViewType
{
    Noop = -1,
    Null,
    DuelStart,
    DuelEnd,
    WaitFrame,
    WaitInput,
    PhaseChange,
    TurnChange,
    FieldChange,
    CursorSet,
    BgmUpdate,
    BattleInit,
    BattleSelect,
    BattleAttack,
    BattleRun,
    BattleEnd,
    LifeSet,
    LifeDamage,
    LifeReset,
    HandShuffle,
    HandShow,
    HandOpen,
    DeckShuffle,
    DeckReset,
    DeckFlipTop,
    GraveTop,
    CardLockon,
    CardMove,
    CardSwap,
    CardFlipTurn,
    CardCheat,
    CardSet,
    CardVanish,
    CardBreak,
    CardExplosion,
    CardExclude,
    CardHappen,
    CardDisable,
    CardEquip,
    CardIncTurn,
    CardUpdate,
    ManaSet,
    MonstDeathTurn,
    MonstShuffle,
    TributeSet,
    TributeReset,
    TributeRun,
    MaterialSet,
    MaterialReset,
    MaterialRun,
    TuningSet,
    TuningReset,
    TuningRun,
    ChainSet,
    ChainRun,
    RunSurrender,
    RunDialog,
    RunList,
    RunSummon,
    RunSpSummon,
    RunFusion,
    RunDetail,
    RunCoin,
    RunDice,
    RunYujyo,
    RunSpecialWin,
    RunVija,
    RunExtra,
    RunCommand,
    CutinDraw,
    CutinSummon,
    CutinFusion,
    CutinChain,
    CutinActivate,
    CutinSet,
    CutinReverse,
    CutinTurn,
    CutinFlip,
    CutinTurnEnd,
    CutinDamage,
    CutinBreak,
    CpuThinking,
    HandRundom,
    OverlaySet,
    OverlayReset,
    OverlayRun,
    CutinSuccess,
    ChainEnd,
    LinkSet,
    LinkReset,
    LinkRun,
    RunJanken,
    CutinCoinDice,
    ChainStep,
    RunSpecialefx,
}
//==================================
// ResultCodes.cs
//==================================
enum AccountCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    NO_PLATFORM = 1100,
    NO_TOKEN,
    INVALID_TOKEN,
    AGREE_MISMATCH = 1104,
    PLATFORM_EXISTING,
    PFM_INHERIT_SUCCESS = 1140,
    PFM_INHERIT_NOT_REGISTER,
    PFM_INHERIT_ALREADY_REGISTERED,
    PFM_INHERIT_ERR_INVALID_PLATFORM,
    PFM_INHERIT_ERR_SHA_NON_REG,
    PFM_INHERIT_ERR_INHERIT_FAILED,
    KID_INHERIT_SUCCESS = 1150,
    KID_INHERIT_NOT_LINKED,
    KID_INHERIT_LINKED,
    KID_INHERIT_INHERIT_WAIT,
    KID_INHERIT_API_NEED_AGREE,
    KID_INHERIT_API_UNAVAILABLE,
    KID_INHERIT_NO_DATA,
    KID_INHERIT_API_FAILED,
    KID_INHERIT_NONCE_ERR,
    KID_INHERIT_FAILED,
    KID_INHERIT_PF_RELATION_FAILED_PS,
    KID_INHERIT_PF_RELATION_FAILED_NINTENDO,
    KID_INHERIT_PF_RELATION_FAILED_XBOX,
    KID_INHERIT_PF_RELATION_FAILED_STEAM,
    KID_INHERIT_FAILED_BY_COUNTRY,
    INHERIT_COUNT_LIMIT = 1169,
    PLATFORM_ERROR,
    PLATFORM_REAUTH,
    PLATFORM_REBOOT,
    ERR_PLATFORM_AUTH_EXPIRED,
    ERR_PLATFORM_SERVICE_AUTH_EXPIRED,
    ERR_EXCESSIVE_REPORT = 1180,
    ERR_SAME_TARGET_REPORT,
    ERR_GAME_SETTINGS_FAILED = 1185,
    PASSWD_LOCK = 1190,
    PASSWD_LOCK_INCORRECT,
    PASSWD_LOCK_EXPIRED,
    DEEPLINK_TITLE_BACK,
}
enum AnnounceCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
}
enum BillingCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    PURCHASE_FAILED = 2901,
    PURCHASE_FATAL,
    PURCHASE_CRITICAL,
    INVALID_RECEIPT,
    PROCESSED,
    BILLING_LIMIT,
    REQUIRE_AGE,
    REGISTER_AGE_FAILED,
    UN_COMPLETE_PURCHASE_ITEM,
    PURCHASE_ITEM_ADD_FAILED,
    INVALID_RECEIPT_REACCESS,
    HISTORY_FAILED,
    PLATFORM_AUTH_EXPIRED,
    NO_ITEM_PURCHASED,
    MIIM_MAINTENANCE,
    DIVISION_MAINTENANCE,
    DATE_LIMIT_OVER,
    LIMIT_EXCEEDED,
    PURCHASE_PENDING,
    VOIDED_PURCHASE,
    ADMIN_FINISH_TRANSACTION,
    STEAM_OVER_LAY_OFF = 2930,
    NX_BAASERROR_SERVICE_MAINTENANCE = 2950,
    NX_SUGARERROR_SERVICE_MAINTENANCE,
}
enum CardFileCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 5200,
    ERR_ACCOUNT_NOT_EXIST,
    ERR_UNDEFINED_FILE,
}
enum CardTermDataCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
}
enum CasualCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 3500,
    ERR_OUT_OF_TERM,
}
enum CertificationCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 4500,
    ERR_NO_MASTER_DATA,
    ERR_OVER_GRADE_CHECK,
}
enum CgdbDeckSearchCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    MAINTE,
    INVALID_USER_TOKEN,
    ID_NOT_REGISTERD,
    DECK_NOT_FOUND,
}
enum ChallengeCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 2300,
    ERR_OUT_OF_TERM,
    ERR_INVALID_MISSION_POOL,
}
enum CraftCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERROR_CRAFT_GEN = 1800,
    ERROR_CRAFT_LIMIT,
    ERROR_UPDATE_FAILED,
    ERROR_COMPENSATION_TIMELIMIT,
    ERROR_BOOST_CRAFT_FAILED,
    ERROR_LOCKED_CARD_CONTAINED,
    ERROR_INVALID_PARAM,
    ERROR_COMPENSATION_EXCLUDE_START,
}
enum CupCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 3700,
    ERR_OUT_OF_TERM,
    ERR_INVALID_MISSION_POOL = 3703,
}
enum DeckCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERROR_DECK_CONFIG = 1400,
    ERROR_DECK_LIMIT,
    ERROR_DECK_REGULATION,
    ERROR_PARAMS_CONFIG,
    ERROR_TOURNAMENT_STATUS,
    ERROR_EXHIBITION_STATUS,
    ERROR_DECK_NAME_LEN,
    ERROR_DECK_SAME_CARDS,
    ERROR_DECK_NO,
    ERROR_CARD_ID,
    ERROR_UNFIND_MAINDECK,
    INVALID_DECK_COUNT = 1440,
    INVALID_DECK_NAME,
    INVALID_DECK_BIKO,
    CGN_ID_NOT_LINKED,
    OVER_DECK_LIMIT,
    INVALID_ACCESS,
    CDB_SERVER_ERROR,
    KONAMIID_SERVER_ERROR,
    NEURON_MAINTENANCE,
    NO_CONSTRUCT_DATA = 1460,
    AUTO_CONSTRUCT_TIMEOUT,
    AUTO_CONSTRUCT_NOT_ENOUGH,
}
enum DicerallyCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 5100,
    ERR_OUT_OF_TERM,
    ERROR_FIXED_ACCESSORY,
    ERR_INVALID_MISSION_POOL,
    ERROR_PROCESSING_FAILED,
}
enum DuelCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    TIMEOUT = 1400,
    NOT_FIND_FRIEND,
    REFUSED_FRIEND,
    INVALID_DECK,
}
enum DuelLiveCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    PLAYBACK_FAIL = 3301,
    IN_PREPARATION,
    DISP_DECK_FAIL,
    ADJUST_MENU_FAIL,
}
enum DuelMenuCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
}
enum DuelpassCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERROR_INVALID_MASTER = 3201,
    ERROR_INVALID_TERM,
    ERROR_INVALID_PARAM,
}
enum DuelTrialCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 4100,
    ERR_OUT_OF_TERM,
    ERROR_FIXED_ACCESSORY,
    ERROR_IN_SHOP_MAINTE,
    ERR_INVALID_MISSION_POOL = 4304,
}
enum EnqueteCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERROR_INVALID_TERM = 3401,
    ERROR_INVALID_PARAM,
}
enum ErrorCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    MAINTE,
    MAINTE_FOR_BACK,
    GENERAL,
    GOTO_STORE,
    SECTION_MAINTE,
    ADDITIONAL_DL,
    ACCOUNT_BAN,
    GOTO_STORE_FOR_TITLE,
    SESSION_EXPIRED,
}
enum EventNotifyCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
}
enum ExhibitionCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 3000,
    ERR_DECK_CONFIG,
    ERR_DECK_SAME_CARD,
    ERR_DECK_REGULATION,
    ERR_OUT_OF_TERM,
    ERR_DECK_LIMIT,
    ERROR_FIXED_ACCESSORY,
    ERR_INVALID_MISSION_POOL,
}
enum FriendCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 2000,
    ACCOUNT_NOT_EXIST,
    ACCOUNT_OWN,
    ALREADY_FOLLOWED,
    NO_FOLLOW_ACCOUNT,
    FOLLOW_MAX,
    SAME_VALUE,
    ALREADY_BLOCKED,
    NO_BLOCK_ACCOUNT,
    BLOCK_MAX,
    BLOCK_USER,
    PS_BLOCK_USER,
    XBOX_BLOCK_USER,
}
enum GachaCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 1900,
    ERR_INVALID_DRAW_NUM,
    ERR_OUT_OF_TERM,
    ERR_MISSING_REQUIRED_ITEMS,
}
enum InvitationCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 4800,
    ERR_OUT_OF_TERM,
    ERR_ALREADY_RECEIVED,
    ERR_INVALID_CODE,
    ERR_REQUESTED_MULTIPLE,
    ERR_FAILED_FOR_THRESHOLD,
    ERR_SELF_INVITE_CODE_SENT,
    ERR_RETRY_SEND_CODE_AGAIN,
    ERR_OUT_OF_SEND_CODE_PERIOD,
}
enum ItemCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    LIMIT_NUM = 1600,
    SUB,
}
enum LoginBonusCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 4000,
    ERR_INVALID_ID,
    ERR_INVALID_TERM,
    ERR_ALREADY_RECEIVED,
    ERR_ALREADY_COMPLETED,
    ERROR_IN_SHOP_MAINTE,
}
enum MissionCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERROR_INVALID_POOL = 2601,
    ERROR_INVALID_MISSION,
    ERROR_OUT_OF_TERM_POOL,
    ERROR_OUT_OF_TERM_MISSION,
    ERROR_GET_REWARD_MASTER = 2611,
    ERROR_GET_PROGRESS,
    ERROR_ALREADY_RECEIVE,
    ERROR_ILLEGAL_RECEIVE,
    ERROR_ILLEGAL_PROGRESS,
    ERROR_NON_APPLY_REWARD,
}
enum NotificationCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
}
enum PresentBoxCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 2500,
}
enum PromoCodesCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 4200,
    ERR_OUT_OF_TERM,
    ERR_ALREADY_RECEIVED,
    ERR_INVALID_CODE,
    ERR_ALREADY_USED,
    ERR_INVALID_PLATFORM,
    ERR_REQUESTED_MULTIPLE,
    ERR_FAILED_FOR_THRESHOLD,
    ERR_MASTER_OUT_OF_TERM,
}
enum PvPCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 2100,
    ALREADY_SAVE_REPLAY,
    NO_SAVE_REPLAY,
    ERR_DISABLE_WATCH,
    ERR_NO_DECK_DATA = 2105,
    ERR_BLOCK_USER_REPLAY,
    ERR_BLOCK_USER_WATCH,
    SAVE_REPLAY_MAX,
    NO_REPLAY_DATA,
    TIMEOUT,
    NOT_FIND_FRIEND,
    REFUSED_FRIEND,
    INVALID_DECK,
    INVALID_PERIOD,
    NOT_FIND_OPPONENT,
    NOT_EXIST_ROOM,
    NOT_EXIST_ROOM_OPP,
    AUDIENCE_LIMIT_MAX,
    NOT_EXIST_TEAM,
    INVALID_TEAM_INFO,
    VS_TEAM_DECIDED,
    TEAM_MATCHING_CANCELED,
    VS_TEAM_WAITING,
    ERR_REPLAY_LOCK_MAX = 2150,
    ERR_REPLAY_LOCK,
}
enum RankEventCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 3600,
    ERR_OUT_OF_TERM,
    ERROR_FIXED_ACCESSORY,
}
enum RateDuelCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 4900,
    ERR_OUT_OF_TERM,
    ERR_INVALID_MISSION_POOL,
}
enum RdcCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 3700,
    ERR_OUT_OF_TERM,
    ERR_INVALID_MISSION_POOL = 3703,
}
enum RoomCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_ROOM = 3101,
    ERR_ENTRY_FAILED,
    ERR_INVALID_NEW_ROOM_ID,
    ERR_NO_VACANT_TABLES,
    ERR_INVALID_DECKSET,
    ERR_ROOM_CREATE_FAILED,
    ERR_USER_BLOCK,
    ERR_INVALID_DATA,
    ERR_PLATFORM_AUTH_EXPIRED,
    ERR_ALREADY_ENTRY_ROOM,
    ERR_ROOM_MEMBER_MAX,
    ERR_RIVAL_LEAVE_TABLE,
    ERR_ENTRY_ROOM_CROSS_INVALID,
    ERR_ENTRY_ROOM_CROSS_PF_INVALID,
    ERR_ENTRY_ROOM_CROSS_XB_BOTH_INVALID,
    ERR_ENTRY_ROOM_CROSS_XB_DEVICE_INVALID,
    ERR_ENTRY_ROOM_CROSS_XB_APP_INVALID,
    ERR_ENTRY_ROOM_CROSS_PF_APP_INVALID,
    ERR_ENTRY_ROOM_VALID_CROSS_XB_BOTH_INVALID,
    ERR_ENTRY_ROOM_VALID_CROSS_XB_DEVICE_INVALID,
    ERR_ENTRY_ROOM_VALID_CROSS_XB_APP_INVALID,
    ERR_ENTRY_ROOM_VALID_CROSS_PF_APP_INVALID,
    ERR_DECK_EMPTY,
    ERR_DECK_REG,
    ERR_LIMIT_REGULATION_PERIOD,
    ERR_HOST_CLIENT_VERSION,
}
enum SeasonPtCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 4700,
    TEAM_UNIQUE_ID,
    TEAM_NOT_EXIST,
    TEAM_INVALID,
    ALREADY_DISBAND,
    ALREADY_DISBANDED,
    TEAM_OUT_OF_TERM,
    TEAM_EXIST,
}
enum ServerStatus
{
    NORMAL,
    INCIDENT,
    MAINTENANCE,
    MAINTENANCE_TEAM,
}
enum ShopCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 2700,
    OUT_OF_TERM,
    ITEMS_SHORTAGE,
    LIMIT_MAX,
    PROCESSING_FAILED,
    EXPIRED_COST_ITEM,
}
enum SoloCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_CHAPTER = 2400,
    LOCKED_CHAPTER,
    NPC_NOT_EXIST,
    STORY_DECK_NOT_EXIST,
    INVALID_GATE = 2405,
}
enum Status
{
    BUSY,
    SUCCESS,
    CLOSE,
    FAILED,
    FATAL,
}
enum StructureCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 2200,
    ALREADY_FIRST_SET,
}
enum TdyCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 4600,
    ERR_OUT_OF_TERM,
}
enum TeamCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    ERR_INVALID_PARAM = 3900,
    ERR_INVALID_TEAM,
    ERR_ENTRY_FAILED,
    ERR_INVALID_NEW_TEAM_ID,
    ERR_NO_VACANT_TABLES,
    ERR_INVALID_DECKSET,
    ERR_TEAM_CREATE_FAILED,
    ERR_USER_BLOCK,
    ERR_INVALID_DATA,
    ERR_PLATFORM_AUTH_EXPIRED,
    ERR_ALREADY_ENTRY_TEAM,
    ERR_TEAM_MEMBER_MAX,
    ERR_RIVAL_LEAVE_TABLE,
    ERR_ENTRY_TEAM_CROSS_INVALID,
    ERR_ENTRY_TEAM_CROSS_PF_INVALID,
    ERR_ENTRY_TEAM_CROSS_XB_BOTH_INVALID,
    ERR_ENTRY_TEAM_CROSS_XB_DEVICE_INVALID,
    ERR_ENTRY_TEAM_CROSS_XB_APP_INVALID,
    ERR_ENTRY_TEAM_CROSS_PF_APP_INVALID,
    ERR_ENTRY_TEAM_VALID_CROSS_XB_BOTH_INVALID,
    ERR_ENTRY_TEAM_VALID_CROSS_XB_DEVICE_INVALID,
    ERR_ENTRY_TEAM_VALID_CROSS_XB_APP_INVALID,
    ERR_ENTRY_TEAM_VALID_CROSS_PF_APP_INVALID,
    ERR_DECK_EMPTY,
    ERR_DECK_REG,
    ERR_LIMIT_REGULATION_PERIOD,
    ERR_HOST_CLIENT_VERSION,
    WAITING_OTHER_MEMBER,
    SEARCHING_TEAM,
    MATCHING_TIME_OUT,
    ERR_ALREADY_MEMBER_COMPLETE,
    ERR_KICK_TIMEOUT,
    ERR_KICK_NOT_SEATED,
    ERR_CANNOT_SIT_TABLE,
    ERR_CANNOT_REASON_TEAM_STATUS,
    ERR_TEAM_REMOVED,
    ERR_RIVAL_MATCHING_FAILED,
    ERR_ALREADY_MEMBER_RECRUIT,
    ERR_ALREADY_MATCH_REQUEST,
    ERR_ALREADY_REQUEST_MATCH_COMPLETE,
    ERR_UNFIND_OTHER_TEAM,
    ERR_OTHER_TEAM_STATUS,
    ERR_UNMATCH_REGULATION,
    ERR_ALREADY_REQUEST_MATCH_COMPLETE_OPP_TEAM,
    ERR_REQUEST_TEAM_CROSS_PF_INVALID,
    ERR_REQUEST_VALID_CROSS_PF_BOTH_INVALID,
    ERR_REQUEST_VALID_CROSS_PF_DEVICE_INVALID,
    ERR_REQUEST_VALID_CROSS_PF_APP_INVALID,
    ERR_REQUEST_MY_TEAM_ID,
}
enum TournamentCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 1500,
    ERR_DECK_CONFIG,
    ERR_DECK_SAME_CARD,
    ERR_DECK_REGULATION,
    ERR_OUT_OF_TERM,
    ERROR_DECK_LIMIT,
    ERROR_FIXED_ACCESSORY,
}
enum UserCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    NO_PLATFORM = 1200,
    ERR_INVALID_PARAM,
    ERR_ACCOUNT_NOT_EXIST,
    ERR_INVALID_VALUE,
    ERR_DIFF_FLAG,
    CROSS_PLAY,
    XBOX_CROSS_PLAY,
    ERR_NG_WORD,
    ERR_NAME_LENGTH,
    ERR_ACCOUNT_OWN,
    BLOCK_USER,
}
enum VersusCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 4400,
    ERR_OUT_OF_TERM,
    ERROR_FIXED_ACCESSORY,
    ERROR_NOT_JOINNED_GROUP,
    ERR_INVALID_MISSION_POOL,
}
enum WcsCode
{
    NONE,
    ERROR,
    FATAL,
    CRITICAL,
    INVALID_PARAM = 4300,
    ERR_OUT_OF_TERM,
    ERR_NO_USER_DATA,
    ERR_OUT_OF_CONDITION,
    ERR_NG_WORD,
    ERR_NAME_LENGTH,
    ERR_DECK_CHANGED,
    ERR_TEAM_REGISTERED,
    ERR_FINAL_INVALID_DATA = 4310,
    ERR_FINAL_ROOM_EMPTY,
    ERR_FINAL_ROOM_RIVAL_LEAVE,
    ERR_FINAL_INVALID_TEAM_ID,
    ERR_FINAL_OUT_OF_SUPPORT_TERM,
    ERR_FINAL_SUPPORT_ABSENT,
    ERR_INVALID_MISSION_POOL,
}
//==================================
// Network API
//==================================
//public static YgomSystem.Network.Handle System_info()
//public static YgomSystem.Network.Handle System_set_language(System.String _lang_)
//public static YgomSystem.Network.Handle System_toggle_crossplay()
//public static YgomSystem.Network.Handle System_toggle_duelbgm()
//public static YgomSystem.Network.Handle Account_create(System.Int32 _agreement_type_, System.String[] _agree_info_, System.Int32 _country_, System.Collections.Generic.Dictionary<System.String,System.Object> _enquete_results_)
//public static YgomSystem.Network.Handle Account_create(System.String _auth_session_, System.Int32 _agreement_type_, System.String[] _agree_info_, System.Int32 _country_, System.Collections.Generic.Dictionary<System.String,System.Object> _enquete_results_)
//public static YgomSystem.Network.Handle Account_auth(System.Collections.Generic.Dictionary<System.String,System.Object> _opt_)
//public static YgomSystem.Network.Handle Account_auth(System.String _auth_session_, System.Boolean _valid_steam_overlay_)
//public static YgomSystem.Network.Handle Account_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_auth(System.String _auth_session_, System.Int32 _label_)
//public static YgomSystem.Network.Handle Account_re_agree(System.String[] _agree_info_, System.Boolean _optout_)
//public static YgomSystem.Network.Handle Account_is_regist_platform()
//public static YgomSystem.Network.Handle Account_regist_platform()
//public static YgomSystem.Network.Handle Account_inherit_platform()
//public static YgomSystem.Network.Handle Account_kid_get_link_url()
//public static YgomSystem.Network.Handle Account_kid_check_linked()
//public static YgomSystem.Network.Handle Account_kid_get_inherit_url()
//public static YgomSystem.Network.Handle Account_kid_check_inherited(System.String _kid_inherit_nonce_, System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_kid_get_neuron_token()
//public static YgomSystem.Network.Handle Account_set_opt_out()
//public static YgomSystem.Network.Handle Account_report_user(System.Int32 _reported_pcode_, System.Int32[] _report_ids_)
//public static YgomSystem.Network.Handle Account_unlock(System.String _pass_)
//public static YgomSystem.Network.Handle Account_set_official_settings()
//public static YgomSystem.Network.Handle Account_set_app_settings(System.Collections.Generic.Dictionary<System.String,System.Object> _app_settings_)
//public static YgomSystem.Network.Handle Account_Steam_get_user_id(System.String _session_ticket_)
//public static YgomSystem.Network.Handle Account_Steam_re_auth(System.String _session_ticket_)
//public static YgomSystem.Network.Handle Account_PS_get_user_id(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_PS_re_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_PS_refresh_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_XBox_get_user_id(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_XBox_re_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_XBox_refresh_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_Nx_get_user_id(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_Nx_re_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle Account_Nx_refresh_auth(System.String _auth_session_)
//public static YgomSystem.Network.Handle User_entry()
//public static YgomSystem.Network.Handle User_home()
//public static YgomSystem.Network.Handle User_profile(System.Int64 _pcode_)
//public static YgomSystem.Network.Handle User_record(System.Int64 _pcode_)
//public static YgomSystem.Network.Handle User_set_profile(System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle User_replay_list(System.Int64 _pcode_)
//public static YgomSystem.Network.Handle User_first_name_entry(System.String _name_)
//public static YgomSystem.Network.Handle User_name_entry(System.String _name_)
//public static YgomSystem.Network.Handle User_complete_home_guide()
//public static YgomSystem.Network.Handle User_item_get_history()
//public static YgomSystem.Network.Handle Duel_begin(System.Collections.Generic.Dictionary<System.String,System.Object> _rule_)
//public static YgomSystem.Network.Handle Duel_end(System.Collections.Generic.Dictionary<System.String,System.Object> _params_)
//public static YgomSystem.Network.Handle Duel_matching(System.Collections.Generic.Dictionary<System.String,System.Object> _rule_)
//public static YgomSystem.Network.Handle Duel_matching_cancel()
//public static YgomSystem.Network.Handle Duel_start_waiting()
//public static YgomSystem.Network.Handle Duel_start_selecting(System.Int32 _select_)
//public static YgomSystem.Network.Handle Duel_team_matching_leader(System.Collections.Generic.Dictionary<System.String,System.Object> _rule_)
//public static YgomSystem.Network.Handle Duel_team_matching_member(System.Collections.Generic.Dictionary<System.String,System.Object> _rule_)
//public static YgomSystem.Network.Handle Tournament_info()
//public static YgomSystem.Network.Handle Tournament_entry(System.Int32 _tid_)
//public static YgomSystem.Network.Handle Tournament_detail(System.Int32 _tid_)
//public static YgomSystem.Network.Handle Tournament_reward_list(System.Int32 _tid_)
//public static YgomSystem.Network.Handle Tournament_duel_history(System.Int32 _tid_)
//public static YgomSystem.Network.Handle Tournament_ranking(System.Int32 _tid_)
//public static YgomSystem.Network.Handle Tournament_get_deck_list(System.Int32 _tid_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Tournament_set_deck(System.Int32 _tid_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Tournament_set_deck_accessory(System.Int32 _tid_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Tournament_delete_deck(System.Int32 _tid_)
//public static YgomSystem.Network.Handle Deck_get_deck()
//public static YgomSystem.Network.Handle Deck_get_deck_list(System.Int32 _deck_id_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Deck_update_deck(System.Int32 _deck_id_, System.String _name_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_)
//public static YgomSystem.Network.Handle Deck_update_deck_reg(System.Int32 _deck_id_, System.String _name_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Int32 _regulation_id_)
//public static YgomSystem.Network.Handle Deck_delete_deck(System.Int32 _deck_id_)
//public static YgomSystem.Network.Handle Deck_delete_deck_multi(System.Int32[] _deck_id_list_)
//public static YgomSystem.Network.Handle Deck_check_deck_regulation(System.Int32 _deck_id_, System.Int32 _regulation_id_)
//public static YgomSystem.Network.Handle Deck_set_deck_accessory(System.Int32 _deck_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Deck_set_accessory(System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Deck_set_select_deck(System.Int32 _mode_, System.Int32 _deck_id_)
//public static YgomSystem.Network.Handle Deck_CopyStructure(System.Int32 _structure_id_)
//public static YgomSystem.Network.Handle Deck_copy_structure_multi(System.Int32[] _structure_ids_)
//public static YgomSystem.Network.Handle Deck_SetFavoriteCards(System.Collections.Generic.Dictionary<System.String,System.Object> _card_list_)
//public static YgomSystem.Network.Handle Deck_SetLockCards(System.Collections.Generic.Dictionary<System.String,System.Object> _card_list_)
//public static YgomSystem.Network.Handle Deck_ExportDeck(System.String _N_token_, System.Int32 _deck_id_)
//public static YgomSystem.Network.Handle Deck_GetAccessoryDetail()
//public static YgomSystem.Network.Handle Deck_get_auto_deck(System.Int32[] _card_ids_, System.Int32 _main_deck_num_, System.Int32 _type_)
//public static YgomSystem.Network.Handle Download_begin()
//public static YgomSystem.Network.Handle Download_complete()
//public static YgomSystem.Network.Handle Download_progress(System.String _dl_end_)
//public static YgomSystem.Network.Handle Craft_exchange(System.Int32 _card_id_)
//public static YgomSystem.Network.Handle Craft_exchange_multi(System.Collections.Generic.Dictionary<System.String,System.Object> _card_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _compensation_list_)
//public static YgomSystem.Network.Handle Craft_generate(System.Int32 _card_id_)
//public static YgomSystem.Network.Handle Craft_generate_multi(System.Collections.Generic.Dictionary<System.String,System.Object> _card_list_, System.Boolean _check_, System.Int32[] _confirm_compensation_list_)
//public static YgomSystem.Network.Handle Craft_get_card_route(System.Int32 _card_id_)
//public static YgomSystem.Network.Handle Craft_generate_boost(System.Int32 _card_id_, System.Int32 _premium_type_, System.Int32 _cost_cp_, System.Boolean _check_, System.Int32[] _confirm_compensation_list_)
//public static YgomSystem.Network.Handle Friend_follow(System.Int64 _pcode_, System.Int32 _delete_)
//public static YgomSystem.Network.Handle Friend_follow_other_routes(System.Int64 _pcode_, System.Int32 _route_)
//public static YgomSystem.Network.Handle Friend_set_pin(System.Int64 _pcode_, System.Int32 _delete_, System.Int32 _update_work_)
//public static YgomSystem.Network.Handle Friend_get_follower(System.Int64 _date_, System.Int64 _pcode_, System.Int32 _dir_)
//public static YgomSystem.Network.Handle Friend_get_list(System.Boolean _all_)
//public static YgomSystem.Network.Handle Friend_id_search(System.Int64 _pcode_)
//public static YgomSystem.Network.Handle Friend_tag_search(System.Int32[] _tag_)
//public static YgomSystem.Network.Handle Friend_block(System.Int64 _pcode_, System.Int32 _delete_)
//public static YgomSystem.Network.Handle Friend_refresh_info(System.Int64[] _pcode_list_)
//public static YgomSystem.Network.Handle Mission_get_list()
//public static YgomSystem.Network.Handle Mission_receive(System.Int32 _pool_id_, System.Int32 _mission_id_, System.Int32 _goal_pos_)
//public static YgomSystem.Network.Handle Mission_bulk_receive(System.Int32[] _bulk_pool_id_, System.Int32[] _bulk_mission_id_, System.Int32[] _bulk_goal_pos_)
//public static YgomSystem.Network.Handle PanelMission_get_list()
//public static YgomSystem.Network.Handle PanelMission_receive(System.Int32 _pool_id_, System.Int32 _mission_id_)
//public static YgomSystem.Network.Handle PvP_watch_duel(System.Int64 _pcode_, System.Int64 _rapid_)
//public static YgomSystem.Network.Handle PvP_replay_duel(System.Int64 _pcode_, System.Int64 _did_)
//public static YgomSystem.Network.Handle PvP_save_replay(System.Int32 _mode_, System.Int64 _did_, System.Int32 _eid_)
//public static YgomSystem.Network.Handle PvP_save_replay_new(System.Int32 _mode_, System.Int64 _did_, System.Int32 _eid_, System.Boolean _open_)
//public static YgomSystem.Network.Handle PvP_remove_replay(System.Int64 _did_)
//public static YgomSystem.Network.Handle PvP_replay_duel_history(System.Int32 _idx_, System.Int32 _mode_, System.Int64 _did_, System.Int32 _eid_)
//public static YgomSystem.Network.Handle PvP_replay_duel_history_with_room(System.Int64 _did_, System.Int64 _pcode_)
//public static YgomSystem.Network.Handle PvP_duel_history(System.Int32 _mode_)
//public static YgomSystem.Network.Handle PvP_set_replay_open(System.Int64 _did_, System.Boolean _open_)
//public static YgomSystem.Network.Handle PvP_set_replay_lock(System.Int64 _did_, System.Boolean _lock_)
//public static YgomSystem.Network.Handle PvP_set_replay_tags(System.Int64 _did_, System.Int32[] _tags_)
//public static YgomSystem.Network.Handle PvP_set_replay_pick_cards(System.Int64 _did_, System.Int32 _myid_, System.Collections.Generic.Dictionary<System.String,System.Object> _pcards_)
//public static YgomSystem.Network.Handle PvP_get_history_deck(System.Int64 _did_, System.Int32 _mode_, System.Int32 _idx_, System.Int32 _is_own_)
//public static YgomSystem.Network.Handle PvP_get_replay_deck(System.Int64 _did_)
//public static YgomSystem.Network.Handle Structure_first(System.Int32 _structure_id_)
//public static YgomSystem.Network.Handle Structure_check_have_structure()
//public static YgomSystem.Network.Handle Gacha_get_card_list(System.Int32 _card_list_id_)
//public static YgomSystem.Network.Handle Gacha_get_probability(System.Int32 _gacha_id_, System.Int32 _shop_id_)
//public static YgomSystem.Network.Handle Shop_get_list(System.Int32 _category_, System.Boolean _no_display_, System.Int32[] _recovery_report_)
//public static YgomSystem.Network.Handle Shop_purchase(System.Int32 _shop_id_, System.Int32 _price_id_, System.Int32 _count_, System.Collections.Generic.Dictionary<System.String,System.Object> _args_)
//public static YgomSystem.Network.Handle Shop_visit(System.Int32[] _shop_ids_)
//public static YgomSystem.Network.Handle Challenge_detail(System.Int32 _mode_, System.Int32 _season_id_)
//public static YgomSystem.Network.Handle Challenge_ranking(System.Int32 _mode_, System.Int32 _season_id_)
//public static YgomSystem.Network.Handle Challenge_set_deck(System.Int32 _mode_, System.Int32 _deck_id_)
//public static YgomSystem.Network.Handle Challenge_duel_history(System.Int32 _mode_, System.Int32 _season_id_)
//public static YgomSystem.Network.Handle Challenge_reward_list(System.Int32 _mode_, System.Int32 _season_id_)
//public static YgomSystem.Network.Handle Challenge_set_use_deck(System.Int32 _mode_, System.Int32 _season_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Challenge_get_rental_deck_list(System.Int32 _mode_, System.Int32 _season_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Casual_detail()
//public static YgomSystem.Network.Handle Casual_duel_history()
//public static YgomSystem.Network.Handle Solo_info(System.Boolean _back_)
//public static YgomSystem.Network.Handle Solo_detail(System.Int32 _chapter_)
//public static YgomSystem.Network.Handle Solo_start(System.Int32 _chapter_)
//public static YgomSystem.Network.Handle Solo_deck_check()
//public static YgomSystem.Network.Handle Solo_set_use_deck_type(System.Int32 _chapter_, System.Int32 _deck_type_)
//public static YgomSystem.Network.Handle Solo_skip(System.Int32 _chapter_)
//public static YgomSystem.Network.Handle Solo_gate_entry(System.Int32 _gate_)
//public static YgomSystem.Network.Handle PresentBox_get_list()
//public static YgomSystem.Network.Handle PresentBox_receive(System.Int32 _present_box_id_, System.Int32 _is_all_)
//public static YgomSystem.Network.Handle DuelMenu_info()
//public static YgomSystem.Network.Handle DuelMenu_deck_check(System.Int32 _kind_, System.Int32 _tid_, System.Int32 _regulation_id_)
//public static YgomSystem.Network.Handle Notification_get_list()
//public static YgomSystem.Network.Handle Notification_read(System.Int32 _id_)
//public static YgomSystem.Network.Handle Announce_get_list()
//public static YgomSystem.Network.Handle EventNotify_get_list()
//public static YgomSystem.Network.Handle EventNotify_delete_badge(System.Int32 _type_, System.Int32 _subtype_, System.Int32[] _target_list_)
//public static YgomSystem.Network.Handle Billing_product_list()
//public static YgomSystem.Network.Handle Billing_reservation(System.Int32 _shop_id_, System.Int32 _merchID_, System.String _price_, System.String _currency_)
//public static YgomSystem.Network.Handle Billing_Nx_reservation(System.Int32 _merchID_, System.String _price_, System.String _currency_)
//public static YgomSystem.Network.Handle Billing_XBox_reservation(System.Int32 _merchID_, System.String _price_, System.String _currency_)
//public static YgomSystem.Network.Handle Billing_PS_reservation(System.Int32 _merchID_, System.String _price_, System.String _currency_)
//public static YgomSystem.Network.Handle Billing_purchase(System.String _receipt_, System.String _adid_, System.String _idfa_, System.String _idfv_, System.String _gps_adid_)
//public static YgomSystem.Network.Handle Billing_purchase(System.String _receipt_, System.String _orderid_, System.String _transactionid_)
//public static YgomSystem.Network.Handle Billing_Nx_purchase()
//public static YgomSystem.Network.Handle Billing_XBox_purchase()
//public static YgomSystem.Network.Handle Billing_PS_purchase()
//public static YgomSystem.Network.Handle Billing_cancel()
//public static YgomSystem.Network.Handle Billing_re_store(System.String _receipt_)
//public static YgomSystem.Network.Handle Billing_Steam_re_store(System.Int64 _orderid_, System.Int64 _transactionid_)
//public static YgomSystem.Network.Handle Billing_Nx_re_store()
//public static YgomSystem.Network.Handle Billing_XBox_re_store(System.String _tracking_id_, System.Int32 _merchID_, System.String _product_id_, System.String _price_, System.String _currency_, System.Boolean _is_un_complete_add_item_)
//public static YgomSystem.Network.Handle Billing_PS_re_store(System.String _transaction_id_, System.Int32 _merchID_, System.String _product_id_, System.String _entitlement_label_, System.String _price_, System.String _currency_, System.Boolean _is_un_complete_add_item_)
//public static YgomSystem.Network.Handle Billing_PS_add_incentive_item(System.String _transaction_id_, System.Int32 _ps_incentive_id_, System.String _product_id_, System.String _entitlement_label_, System.Int32 _service_label_)
//public static YgomSystem.Network.Handle Billing_register_age(System.Int32 _age_reg_id_)
//public static YgomSystem.Network.Handle Billing_add_purchased_item()
//public static YgomSystem.Network.Handle Billing_in_complete_item_check()
//public static YgomSystem.Network.Handle Billing_Steam_in_complete_item_check()
//public static YgomSystem.Network.Handle Billing_Nx_in_complete_item_check()
//public static YgomSystem.Network.Handle Billing_XBox_in_complete_item_check()
//public static YgomSystem.Network.Handle Billing_PS_in_complete_item_check()
//public static YgomSystem.Network.Handle Billing_history(System.String _month_, System.Int32 _page_, System.Int32 _page_count_)
//public static YgomSystem.Network.Handle Cgdb_deck_search_init()
//public static YgomSystem.Network.Handle Cgdb_deck_search(System.Int32 _typeCode_, System.Int32[] _categoryList_, System.Int32[] _tagList_, System.Int32[] _cardIdList_, System.String _keyword_, System.String _deckCode_, System.Int32 _sortCode_, System.Int32 _sizePerPage_, System.Int32 _requestPageNo_)
//public static YgomSystem.Network.Handle Cgdb_deck_search_detail(System.String _targetId_, System.Int32 _deckNo_)
//public static YgomSystem.Network.Handle Cgdb_mydeck_search(System.String _userToken_, System.Int32 _sortCode_, System.Int32 _sizePerPage_, System.Int32 _requestPageNo_)
//public static YgomSystem.Network.Handle Cgdb_mydeck_search_detail(System.String _userToken_, System.Int32 _deckNo_)
//public static YgomSystem.Network.Handle Room_room_create(System.Collections.Generic.Dictionary<System.String,System.Object> _room_settings_)
//public static YgomSystem.Network.Handle Room_room_create(System.Collections.Generic.Dictionary<System.String,System.Object> _room_settings_, System.String _context_id_)
//public static YgomSystem.Network.Handle Room_room_entry(System.Int32 _id_, System.Int32 _is_specter_, System.Collections.Generic.Dictionary<System.String,System.Object> _options_)
//public static YgomSystem.Network.Handle Room_room_entry(System.Int32 _id_, System.Int32 _is_specter_, System.Collections.Generic.Dictionary<System.String,System.Object> _options_, System.String _context_id_)
//public static YgomSystem.Network.Handle Room_room_exit()
//public static YgomSystem.Network.Handle Room_get_room_list(System.Collections.Generic.Dictionary<System.String,System.Object> _search_options_)
//public static YgomSystem.Network.Handle Room_table_arrive(System.Int32 _table_no_)
//public static YgomSystem.Network.Handle Room_table_leave()
//public static YgomSystem.Network.Handle Room_room_friend_invite(System.Int64[] _invite_list_)
//public static YgomSystem.Network.Handle Room_room_friend_invite(System.Int64[] _invite_list_, System.String _context_id_)
//public static YgomSystem.Network.Handle Room_room_table_polling()
//public static YgomSystem.Network.Handle Room_is_room_battle_ready(System.Boolean _isBattleReady_, System.Int64 _opp_pcode_)
//public static YgomSystem.Network.Handle Room_set_user_comment(System.Int32 _comment_id_)
//public static YgomSystem.Network.Handle Room_get_result_list()
//public static YgomSystem.Network.Handle Exhibition_detail(System.Int32 _exhid_)
//public static YgomSystem.Network.Handle Exhibition_duel_history(System.Int32 _exhid_)
//public static YgomSystem.Network.Handle Exhibition_set_deck(System.Int32 _exhid_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Exhibition_set_deck_accessory(System.Int32 _exhid_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Exhibition_delete_deck(System.Int32 _exhid_)
//public static YgomSystem.Network.Handle Exhibition_copy_to_deck(System.Int32 _exhid_)
//public static YgomSystem.Network.Handle Exhibition_rental_deck_detail(System.Int32 _exhid_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Exhibition_copy_rental_deck(System.Int32 _exhid_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Exhibition_set_use_deck(System.Int32 _exhid_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Exhibition_get_deck_list(System.Int32 _exhid_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Duelpass_get_info()
//public static YgomSystem.Network.Handle Duelpass_bulk_receive(System.Int32 _season_id_, System.Int32[] _reward_id_list_)
//public static YgomSystem.Network.Handle DuelLive_replay_list(System.Int32 _menu_id_, System.Int32 _section_id_, System.Int32 _opt_)
//public static YgomSystem.Network.Handle DuelLive_replay_duel(System.Int32 _menu_id_, System.Int32 _idx_, System.Int32 _opt_, System.Int32 _mrk_, System.Int32 _reverse_)
//public static YgomSystem.Network.Handle DuelLive_get_replay_deck(System.Int32 _menu_id_, System.Int32 _mrk_, System.Int32 _idx_, System.Int32 _reverse_)
//public static YgomSystem.Network.Handle DuelLive_visit(System.Collections.Generic.Dictionary<System.String,System.Object>[] _menu_ids_)
//public static YgomSystem.Network.Handle Enquete_get_questions(System.Int32 _enquete_id_)
//public static YgomSystem.Network.Handle Enquete_send_answers(System.Int32 _enquete_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _results_)
//public static YgomSystem.Network.Handle RankEvent_detail(System.Int32 _rank_event_id_)
//public static YgomSystem.Network.Handle RankEvent_duel_history(System.Int32 _rank_event_id_)
//public static YgomSystem.Network.Handle RankEvent_reward_list(System.Int32 _rank_event_id_)
//public static YgomSystem.Network.Handle RankEvent_get_deck_list(System.Int32 _rank_event_id_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle RankEvent_set_deck(System.Int32 _rank_event_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle RankEvent_set_deck_accessory(System.Int32 _rank_event_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle RankEvent_delete_deck(System.Int32 _rank_event_id_)
//public static YgomSystem.Network.Handle Cup_detail(System.Int32 _cid_)
//public static YgomSystem.Network.Handle Cup_get_deck_list(System.Int32 _cid_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Cup_set_deck(System.Int32 _cid_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Cup_delete_deck(System.Int32 _cid_)
//public static YgomSystem.Network.Handle Cup_duel_history(System.Int32 _cid_)
//public static YgomSystem.Network.Handle Cup_set_deck_accessory(System.Int32 _cid_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Cup_get_ranking(System.Int32 _cid_)
//public static YgomSystem.Network.Handle CardTermData_get_list()
//public static YgomSystem.Network.Handle Team_set_team_regulation_group_id(System.Int32 _team_regulation_group_id_)
//public static YgomSystem.Network.Handle Team_team_create(System.Collections.Generic.Dictionary<System.String,System.Object> _team_settings_, System.Int32 _team_match_type_)
//public static YgomSystem.Network.Handle Team_team_create(System.Collections.Generic.Dictionary<System.String,System.Object> _team_settings_, System.Int32 _team_match_type_, System.String _context_id_)
//public static YgomSystem.Network.Handle Team_team_exit()
//public static YgomSystem.Network.Handle Team_team_entry(System.Int32 _team_id_, System.Int32 _team_match_type_)
//public static YgomSystem.Network.Handle Team_team_entry(System.Int32 _team_id_, System.Int32 _team_match_type_, System.String _context_id_)
//public static YgomSystem.Network.Handle Team_team_entry_and_arrive(System.Int32 _team_id_, System.Int32 _team_match_type_, System.Int32 _table_no_)
//public static YgomSystem.Network.Handle Team_team_entry_and_arrive(System.Int32 _team_id_, System.Int32 _team_match_type_, System.String _context_id_, System.Int32 _table_no_)
//public static YgomSystem.Network.Handle Team_team_invite(System.Int64[] _invite_list_)
//public static YgomSystem.Network.Handle Team_team_invite(System.Int64[] _invite_list_, System.String _context_id_)
//public static YgomSystem.Network.Handle Team_team_recruit(System.Int32 _num_)
//public static YgomSystem.Network.Handle Team_team_search(System.Int32 _num_, System.Boolean _cancel_flg_)
//public static YgomSystem.Network.Handle Team_team_polling(System.Int32 _team_match_type_)
//public static YgomSystem.Network.Handle Team_team_result_polling()
//public static YgomSystem.Network.Handle Team_table_arrive(System.Int32 _table_no_)
//public static YgomSystem.Network.Handle Team_table_leave()
//public static YgomSystem.Network.Handle Team_post_comment(System.Int32 _comment_id_)
//public static YgomSystem.Network.Handle Team_team_duel_request(System.Int32 _team_id_, System.Int32 _duel_time_)
//public static YgomSystem.Network.Handle Team_team_duel_request_cancel()
//public static YgomSystem.Network.Handle Team_team_duel_reply(System.Boolean _reply_)
//public static YgomSystem.Network.Handle DuelTrial_detail(System.Int32 _duel_trial_id_)
//public static YgomSystem.Network.Handle DuelTrial_duel_history(System.Int32 _duel_trial_id_)
//public static YgomSystem.Network.Handle DuelTrial_get_deck_list(System.Int32 _duel_trial_id_, System.Int32 _idx_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle DuelTrial_set_deck(System.Int32 _duel_trial_id_, System.Int32 _idx_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle DuelTrial_set_deck_accessory(System.Int32 _duel_trial_id_, System.Int32 _idx_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle DuelTrial_delete_deck(System.Int32 _duel_trial_id_, System.Int32 _idx_)
//public static YgomSystem.Network.Handle DuelTrial_set_use_deck(System.Int32 _duel_trial_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle DuelTrial_get_rental_deck_list(System.Int32 _duel_trial_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle DuelTrial_receive_bonus(System.Int32 _duel_trial_id_, System.Int32 _item_id_, System.Int32 _num_)
//public static YgomSystem.Network.Handle PromoCodes_send_code(System.Int32 _promo_codes_id_, System.String _send_code_, System.String _check_code_)
//public static YgomSystem.Network.Handle Wcs_detail(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_get_deck_list(System.Int32 _wcs_id_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Wcs_set_deck(System.Int32 _wcs_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Wcs_delete_deck(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_duel_history(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_set_deck_accessory(System.Int32 _wcs_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Wcs_get_ranking(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_set_region(System.Int32 _wcs_id_, System.Int32 _region_)
//public static YgomSystem.Network.Handle Wcs_get_participation()
//public static YgomSystem.Network.Handle Wcs_confirm_participation(System.Int32 _id_)
//public static YgomSystem.Network.Handle Wcs_get_final_deck_info()
//public static YgomSystem.Network.Handle Wcs_set_final_deck(System.Int32 _wcs_id_, System.Int32 _member_idx_, System.Int32 _slot_, System.String _name_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_)
//public static YgomSystem.Network.Handle Wcs_delete_final_deck(System.Int32 _wcs_id_, System.Int32 _member_idx_, System.Int32 _slot_)
//public static YgomSystem.Network.Handle Wcs_set_final_share_card(System.Int32 _wcs_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _share_cards_)
//public static YgomSystem.Network.Handle Wcs_check_final_deck_regulation(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_complete_final_deck(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_unregister_final_deck(System.Int32 _wcs_id_)
//public static YgomSystem.Network.Handle Wcs_set_final_member_name(System.Int32 _wcs_id_, System.String _name_)
//public static YgomSystem.Network.Handle Wcs_set_final_member_profile(System.Int32 _wcs_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Wcs_set_final_team_name(System.Int32 _wcs_id_, System.String _name_)
//public static YgomSystem.Network.Handle Wcs_set_final_member_status(System.Int32 _wcs_id_, System.Boolean _is_regist_)
//public static YgomSystem.Network.Handle Versus_detail(System.Int32 _versus_id_, System.Int32 _set_group_id_)
//public static YgomSystem.Network.Handle Versus_duel_history(System.Int32 _versus_id_)
//public static YgomSystem.Network.Handle Versus_get_deck_list(System.Int32 _versus_id_, System.Int32 _idx_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Versus_set_deck(System.Int32 _versus_id_, System.Int32 _idx_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Versus_set_deck_accessory(System.Int32 _versus_id_, System.Int32 _idx_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Versus_delete_deck(System.Int32 _versus_id_, System.Int32 _idx_)
//public static YgomSystem.Network.Handle Versus_set_use_deck(System.Int32 _versus_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Versus_get_rental_deck_list(System.Int32 _versus_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle WcsFinal_table_polling()
//public static YgomSystem.Network.Handle WcsFinal_is_duel_ready(System.Boolean _isReady_, System.Int64 _opp_pcode_, System.Int32 _tno_)
//public static YgomSystem.Network.Handle WcsFinal_ad_table_polling(System.Int32 _room_id_)
//public static YgomSystem.Network.Handle WcsFinal_ad_primary_polling()
//public static YgomSystem.Network.Handle WcsFinal_ad_final_polling()
//public static YgomSystem.Network.Handle WcsFinal_user_home()
//public static YgomSystem.Network.Handle WcsFinal_user_entry()
//public static YgomSystem.Network.Handle WcsFinal_duel_menu_info()
//public static YgomSystem.Network.Handle WcsFinal_watch_duel(System.Int64 _pcode_, System.Int64 _rapid_)
//public static YgomSystem.Network.Handle WcsFinal_replay_duel_history(System.Int64 _did_, System.Int64 _pcode_, System.Boolean _is_open_)
//public static YgomSystem.Network.Handle WcsfCampaign_info()
//public static YgomSystem.Network.Handle WcsfCampaign_primary_polling()
//public static YgomSystem.Network.Handle WcsfCampaign_final_polling()
//public static YgomSystem.Network.Handle WcsfCampaign_table_polling(System.Int32 _room_id_)
//public static YgomSystem.Network.Handle WcsfCampaign_watch_duel(System.Int64 _pcode_, System.Int64 _rapid_)
//public static YgomSystem.Network.Handle WcsfCampaign_set_support_team(System.Int32 _team_id_)
//public static YgomSystem.Network.Handle WcsfCampaign_support_entry()
//public static YgomSystem.Network.Handle WcsfCampaign_replay_duel(System.Int64 _did_, System.Int64 _pcode_)
//public static YgomSystem.Network.Handle Certification_detail(System.Int32 _certification_id_)
//public static YgomSystem.Network.Handle Certification_get_academic_exam(System.Int32 _certification_id_, System.Int32 _grade_id_)
//public static YgomSystem.Network.Handle Certification_check_quiz_answers(System.Int32[] _answers_)
//public static YgomSystem.Network.Handle Tdy_detail()
//public static YgomSystem.Network.Handle SeasonPt_id_search(System.Int32 _idx_, System.Int64 _pcode_)
//public static YgomSystem.Network.Handle SeasonPt_info(System.Boolean _is_back_)
//public static YgomSystem.Network.Handle SeasonPt_team_create(System.Int64 _member1_pcode_, System.Int64 _member2_pcode_, System.Int32 _region_)
//public static YgomSystem.Network.Handle SeasonPt_team_disband(System.Int32 _unique_id_)
//public static YgomSystem.Network.Handle SeasonPt_team_member_status(System.Int32 _unique_id_, System.Int32 _action_)
//public static YgomSystem.Network.Handle SeasonPt_team_search(System.Int32 _unique_id_)
//public static YgomSystem.Network.Handle SeasonPt_get_record(System.Boolean _is_own_, System.Int32 _unique_id_)
//public static YgomSystem.Network.Handle SeasonPt_get_ranking()
//public static YgomSystem.Network.Handle SeasonPt_get_result(System.Int32 _season_id_)
//public static YgomSystem.Network.Handle SeasonPt_update_demand_status(System.Int32 _status_)
//public static YgomSystem.Network.Handle Invitation_send_code(System.Int32 _invitation_id_, System.String _send_code_, System.String _check_code_)
//public static YgomSystem.Network.Handle Invitation_get_info_for_invitee(System.Int32 _invitation_id_)
//public static YgomSystem.Network.Handle Invitation_get_info_for_inviter(System.Int32 _invitation_id_)
//public static YgomSystem.Network.Handle RateDuel_detail(System.Int32 _season_id_)
//public static YgomSystem.Network.Handle RateDuel_duel_history(System.Int32 _season_id_)
//public static YgomSystem.Network.Handle RateDuel_get_ranking(System.Int32 _season_id_)
//public static YgomSystem.Network.Handle RateDuel_set_use_deck(System.Int32 _season_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle RateDuel_get_rental_deck_list(System.Int32 _season_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle LoginBonusEx_get_info(System.Int32 _login_bonus_id_)
//public static YgomSystem.Network.Handle LoginBonusEx_get_list()
//public static YgomSystem.Network.Handle LoginBonusEx_receive(System.Int32 _login_bonus_id_)
//public static YgomSystem.Network.Handle LoginBonusEx_purchase(System.Int32 _login_bonus_id_, System.Int32 _shop_id_, System.Int32 _price_id_, System.Int32 _num_)
//public static YgomSystem.Network.Handle LoginBonusEx_get_probability(System.Int32 _gacha_id_, System.Int32 _shop_id_)
//public static YgomSystem.Network.Handle Rdc_detail(System.Int32 _rid_)
//public static YgomSystem.Network.Handle Rdc_get_deck_list(System.Int32 _rid_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Rdc_set_deck(System.Int32 _rid_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Rdc_delete_deck(System.Int32 _rid_)
//public static YgomSystem.Network.Handle Rdc_duel_history(System.Int32 _rid_)
//public static YgomSystem.Network.Handle Rdc_set_deck_accessory(System.Int32 _rid_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Rdc_get_ranking(System.Int32 _rid_)
//public static YgomSystem.Network.Handle Rdc_set_use_deck(System.Int32 _rid_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Rdc_get_rental_deck_list(System.Int32 _rid_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Dicerally_detail(System.Int32 _dicerally_id_)
//public static YgomSystem.Network.Handle Dicerally_duel_history(System.Int32 _dicerally_id_)
//public static YgomSystem.Network.Handle Dicerally_get_deck_list(System.Int32 _dicerally_id_, System.Boolean _is_empty_get_)
//public static YgomSystem.Network.Handle Dicerally_set_deck(System.Int32 _dicerally_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _deck_list_, System.Collections.Generic.Dictionary<System.String,System.Object> _accessory_, System.Collections.Generic.Dictionary<System.String,System.Object> _pick_cards_)
//public static YgomSystem.Network.Handle Dicerally_set_deck_accessory(System.Int32 _dicerally_id_, System.Collections.Generic.Dictionary<System.String,System.Object> _param_)
//public static YgomSystem.Network.Handle Dicerally_delete_deck(System.Int32 _dicerally_id_)
//public static YgomSystem.Network.Handle Dicerally_set_use_deck(System.Int32 _dicerally_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Dicerally_get_rental_deck_list(System.Int32 _dicerally_id_, System.Int32 _rental_idx_)
//public static YgomSystem.Network.Handle Dicerally_update_board(System.Int32 _dicerally_id_, System.Int32 _dice_roll_num_)
//public static YgomSystem.Network.Handle Dicerally_get_card_list(System.Int32 _dicerally_id_, System.Int32 _target_gacha_id_)
//public static YgomSystem.Network.Handle CardFile_get_list(System.Int64 _pcode_)
//public static YgomSystem.Network.Handle CardFile_detail(System.Int64 _pcode_, System.Int32 _item_id_)
//public System.Void .ctor()
//==================================
// duel.dll functions (Engine)
//==================================
//static System.UInt32 DLL_CardRareGetBufferSize()
//static System.Int32 DLL_CardRareGetRareByUniqueID(System.Int32 uniqueId)
//static System.Void DLL_CardRareSetBuffer(System.IntPtr pBuf)
//static System.Void DLL_CardRareSetRare(System.IntPtr pBuf, System.IntPtr rare0, System.IntPtr rare1, System.IntPtr rare2, System.IntPtr rare3)
//static System.Boolean DLL_DeulIsThisEffectiveMonsterWithDual(System.Int32 player, System.Int32 index)
//static System.Int32 DLL_DlgProcGetSummoningMonsterUniqueID()
//static System.UInt32 DLL_DuelCanIDoPutMonster(System.Int32 player)
//static System.Boolean DLL_DuelCanIDoSpecialSummon(System.Int32 player)
//static System.Boolean DLL_DuelCanIDoSummonMonster(System.Int32 player)
//static System.Void DLL_DuelChangeCardIDByUniqueID(System.Int32 uniqueId, System.Int32 cardId)
//static System.Int32 DLL_DuelComCancelCommand2(System.Boolean decide)
//static System.Void DLL_DuelComDebugCommand()
//static System.Void DLL_DuelComDefaultLocation()
//static System.Void DLL_DuelComDoCommand(System.Int32 player, System.Int32 position, System.Int32 index, System.Int32 commandId)
//static System.UInt32 DLL_DuelComGetCommandMask(System.Int32 player, System.Int32 position, System.Int32 index)
//static System.UInt32 DLL_DuelComGetMovablePhase()
//static System.UInt32 DLL_DUELCOMGetPosMaskOfThisHand(System.Int32 player, System.Int32 index, System.Int32 commandId)
//static System.Int32 DLL_DUELCOMGetRecommendSide()
//static System.UInt32 DLL_DuelComGetTextIDOfThisCommand(System.Int32 player, System.Int32 position, System.Int32 index)
//static System.UInt32 DLL_DuelComGetTextIDOfThisSummon(System.Int32 player, System.Int32 position, System.Int32 index)
//static System.Void DLL_DuelComMovePhase(System.Int32 phase)
//static System.Int32 DLL_DuelDlgCanYesNoSkip()
//static System.Int32 DLL_DuelDlgGetMixData(System.Int32 index)
//static System.Int32 DLL_DuelDlgGetMixNum()
//static System.Int32 DLL_DuelDlgGetMixType(System.Int32 index)
//static System.Int32 DLL_DuelDlgGetPosMaskOfThisSummon()
//static System.Int32 DLL_DuelDlgGetSelectItemDefault(System.Int32 index)
//static System.Int32 DLL_DuelDlgGetSelectItemEnable(System.Int32 index)
//static System.Int32 DLL_DuelDlgGetSelectItemNum()
//static System.Int32 DLL_DuelDlgGetSelectItemStr(System.Int32 index)
//static System.Void DLL_DuelDlgSetResult(System.UInt32 result)
//static System.Int32 DLL_DuelGetAttachedEffectList(System.IntPtr lpAffect)
//static System.Int32 DLL_DuelGetAttackTargetMask(System.Int32 player, System.Int32 locate)
//static System.Boolean DLL_DuelGetCantActIcon(System.Int32 player, System.Int32 locate, System.Int32 index, System.Int32 flag)
//static System.Void DLL_DuelGetCardBasicVal(System.Int32 player, System.Int32 pos, System.Int32 index, YgomGame.Duel.Engine.BasicVal& pVal)
//static System.Int32 DLL_DuelGetCardFace(System.Int32 player, System.Int32 position, System.Int32 index)
//static System.UInt32 DLL_DuelGetCardIDByUniqueID2(System.Int32 uniqueId)
//static System.UInt32 DLL_DuelGetCardInHand(System.Int32 player)
//static System.Int32 DLL_DuelGetCardNum(System.Int32 player, System.Int32 locate)
//static System.IntPtr DLL_DuelGetCardPropByUniqueID(System.Int32 uniqueId)
//static System.Int32 DLL_DuelGetCardTurn(System.Int32 player, System.Int32 position, System.Int32 index)
//static System.Int32 DLL_DuelGetCardUniqueID(System.Int32 player, System.Int32 position, System.Int32 index)
//static System.UInt32 DLL_DuelGetCurrentDmgStep()
//static System.UInt32 DLL_DuelGetCurrentPhase()
//static System.UInt32 DLL_DuelGetCurrentStep()
//static System.Int32 DLL_DuelGetDuelFinish()
//static System.Int32 DLL_DuelGetDuelFinishCardID()
//static System.Boolean DLL_DuelGetDuelFlagDeckReverse()
//static System.Int32 DLL_DuelGetDuelResult()
//static System.Void DLL_DuelGetFldAffectIcon(System.Int32 player, System.Int32 locate, System.IntPtr ptr, System.Int32 view_player)
//static System.Int32 DLL_DuelGetFldMonstOrgLevel(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetFldMonstOrgRank(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetFldMonstOrgType(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetFldMonstRank(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetFldPendOrgScale(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetFldPendScale(System.Int32 player, System.Int32 locate)
//static System.Boolean DLL_DuelGetHandCardOpen(System.Int32 player, System.Int32 index)
//static System.Int32 DLL_DuelGetLP(System.Int32 player)
//static System.Int32 DLL_DuelGetThisCardCounter(System.Int32 player, System.Int32 locate, System.Int32 counter)
//static System.Int32 DLL_DuelGetThisCardDirectFlag(System.Int32 player, System.Int32 index)
//static System.Int32 DLL_DuelGetThisCardEffectFlags(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetThisCardEffectIDAtChain(System.Int32 player, System.Int32 locate)
//static System.UInt32 DLL_DuelGetThisCardEffectList(System.Int32 player, System.Int32 locate, System.IntPtr list)
//static System.Int32 DLL_DuelGetThisCardOverlayNum(System.Int32 player, System.Int32 locate)
//static System.UInt32 DLL_DuelGetThisCardParameter(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetThisCardShowParameter(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetThisCardTurnCounter(System.Int32 player, System.Int32 locate)
//static System.Boolean DLL_DuelGetThisMonsterFightableOnEffect(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetTopCardIndex(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelGetTrapMonstBasicVal(System.Int32 cardId, YgomGame.Duel.Engine.BasicVal& pVal)
//static System.UInt32 DLL_DuelGetTurnNum()
//static System.Int32 DLL_DuelIsHuman(System.Int32 player)
//static System.Int32 DLL_DuelIsMyself(System.Int32 player)
//static System.Int32 DLL_DuelIsReplayMode()
//static System.Int32 DLL_DuelIsRival(System.Int32 player)
//static System.Int32 DLL_DuelIsThisCardExist(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelIsThisContinuousCard(System.Int32 player, System.Int32 locate)
//static System.Boolean DLL_DuelIsThisEffectiveMonster(System.Int32 player, System.Int32 index)
//static System.Int32 DLL_DuelIsThisEquipCard(System.Int32 player, System.Int32 locate)
//static System.Boolean DLL_DuelIsThisMagic(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelIsThisNormalMonster(System.Int32 player, System.Int32 locate)
//static System.Boolean DLL_DuelIsThisNormalMonsterInGrave(System.Int32 player, System.Int32 index)
//static System.Boolean DLL_DuelIsThisNormalMonsterInHand(System.Int32 wCardID)
//static System.Int32 DLL_DuelIsThisQuickDuel()
//static System.Boolean DLL_DuelIsThisTrap(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelIsThisTrapMonster(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelIsThisTunerMonster(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelIsThisZoneAvailable(System.Int32 player, System.Int32 locate)
//static System.Int32 DLL_DuelIsThisZoneAvailable2(System.Int32 player, System.Int32 locate, System.Boolean visibleOnly)
//static System.Int32 DLL_DuelListGetCardAttribute(System.Int32 iLookPlayer, System.Int32 wUniqueID)
//static System.Int32 DLL_DuelListGetItemAttribute(System.Int32 index)
//static System.Int32 DLL_DuelListGetItemFrom(System.Int32 index)
//static System.Int32 DLL_DuelListGetItemID(System.Int32 index)
//static System.Int32 DLL_DuelListGetItemMax()
//static System.Int32 DLL_DuelListGetItemMsg(System.Int32 index)
//static System.Int32 DLL_DuelListGetItemTargetUniqueID(System.Int32 index)
//static System.Int32 DLL_DuelListGetItemUniqueID(System.Int32 index)
//static System.Int32 DLL_DuelListGetSelectMax()
//static System.Int32 DLL_DuelListGetSelectMin()
//static System.Void DLL_DuelListInitString()
//static System.Int32 DLL_DuelListIsMultiMode()
//static System.Void DLL_DuelListSetCardExData(System.Int32 index, System.Int32 data)
//static System.Void DLL_DuelListSetIndex(System.Int32 index)
//static System.Int32 DLL_DuelMyself()
//static System.Int32 DLL_DuelResultGetData(System.Int32 player, System.IntPtr dst)
//static System.Int32 DLL_DuelResultGetMemo(System.Int32 player, System.IntPtr dst)
//static System.Int32 DLL_DuelRival()
//static System.UInt32 DLL_DuelSearchCardByUniqueID(System.Int32 uniqueId)
//static System.Void DLL_DuelSetCpuParam(System.Int32 player, System.UInt32 param)
//static System.Void DLL_DuelSetDuelLimitedType(System.UInt32 limitedType)
//static System.Void DLL_DuelSetFirstPlayer(System.Int32 player)
//static System.Void DLL_DuelSetMyPlayerNum(System.Int32 player)
//static System.Void DLL_DuelSetPlayerType(System.Int32 player, System.Int32 type)
//static System.Void DLL_DuelSetRandomSeed(System.UInt32 seed)
//static System.Int32 DLL_DuelSysAct()
//static System.Void DLL_DuelSysClearWork()
//static System.Int32 DLL_DuelSysInitCustom(System.Int32 fDuelType, System.Boolean tag, System.Int32 life0, System.Int32 life1, System.Int32 hand0, System.Int32 hand1, System.Boolean shuf)
//static System.Int32 DLL_DuelSysInitQuestion(System.IntPtr pScript)
//static System.Int32 DLL_DuelSysInitRush()
//static System.Void DLL_DuelSysSetDeck2(System.Int32 player, System.Int32[] mainDeck, System.Int32 mainNum, System.Int32[] extraDeck, System.Int32 extraNum, System.Int32[] sideDeck, System.Int32 sideNum)
//static System.Int32 DLL_DuelWhichTurnNow()
//static System.Int32 DLL_FusionGetMaterialList(System.Int32 uniqueId, System.IntPtr list)
//static System.Int32 DLL_FusionGetMonsterLevelInTuning(System.Int32 wUniqueID)
//static System.Int32 DLL_FusionIsThisTunedMonsterInTuning(System.Int32 wUniqueID)
//static System.Int32 DLL_GetBinHash(System.Int32 iIndex)
//static System.Int32 DLL_GetCardExistNum()
//static System.Int32 DLL_GetRevision()
//static System.Void DLL_SetAddRecordDelegate(YgomGame.Duel.Engine.AddRecord addRecord)
//static System.Void DLL_SetCardExistWork(System.IntPtr pWork, System.Int32 size, System.Int32 count)
//static System.Void DLL_SetDuelChallenge(System.Int32 flagbit)
//static System.Void DLL_SetDuelChallenge2(System.Int32 player, System.Int32 flagbit)
//static System.Void DLL_SetEffectDelegate(YgomGame.Duel.Engine.ThreadRunEffectDeleg runEffct, YgomGame.Duel.Engine.ThreadIsBusyEffectDeleg isBusyEffect)
//static System.Void DLL_SetPlayRecordDelegate(YgomGame.Duel.Engine.NowRecord nowRecord, YgomGame.Duel.Engine.RecordNext recordNext, YgomGame.Duel.Engine.RecordBegin recordBegin, YgomGame.Duel.Engine.IsRecordEnd isRecordEnd)
//static System.Void DLL_SetRareByUniqueID(System.Int32 uniqueId, System.Int32 rare)
//static System.Int32 DLL_SetWorkMemory(System.IntPtr pWork)
//==================================
// duel.dll functions (Content)
//==================================
//static System.Int32 DLL_CardCheckName(System.Int32 cardId, System.Int32 nameType)
//static System.Int32 DLL_CardGetAltCardID(System.Int32 cardId, System.Int32 alterID)
//static System.Int32 DLL_CardGetAlterID(System.Int32 cardId)
//static System.Int32 DLL_CardGetAtk(System.Int32 cardId)
//static System.Int32 DLL_CardGetAtk2(System.Int32 cardId)
//static System.Int32 DLL_CardGetAttr(System.Int32 cardId)
//static System.Int32 DLL_CardGetBasicVal(System.Int32 cardId, YgomGame.Duel.Engine.BasicVal& pVal)
//static System.Int32 DLL_CardGetDef(System.Int32 cardId)
//static System.Int32 DLL_CardGetDef2(System.Int32 cardId)
//static System.Int32 DLL_CardGetFrame(System.Int32 cardId)
//static System.Int32 DLL_CardGetIcon(System.Int32 cardId)
//static System.Int32 DLL_CardGetInternalID(System.Int32 cardId)
//static System.Int32 DLL_CardGetKind(System.Int32 cardId)
//static System.Int32 DLL_CardGetLevel(System.Int32 cardId)
//static System.Int32 DLL_CardGetLimitation(System.Int32 cardId)
//static System.Int32 DLL_CardGetLinkCards(System.Int32 cardId, System.IntPtr pLinkID)
//static System.Int32 DLL_CardGetLinkMask(System.Int32 cardId)
//static System.Int32 DLL_CardGetLinkNum(System.Int32 cardId)
//static System.Int32 DLL_CardGetOriginalID(System.Int32 cardId)
//static System.Int32 DLL_CardGetOriginalID2(System.Int32 cardId)
//static System.Int32 DLL_CardGetRank(System.Int32 cardId)
//static System.Int32 DLL_CardGetScaleL(System.Int32 cardId)
//static System.Int32 DLL_CardGetScaleR(System.Int32 cardId)
//static System.Int32 DLL_CardGetStar(System.Int32 cardId)
//static System.Int32 DLL_CardGetType(System.Int32 cardId)
//static System.Int32 DLL_CardIsThisCardGenre(System.Int32 cardId, System.Int32 genreId)
//static System.Int32 DLL_CardIsThisSameCard(System.Int32 cardA, System.Int32 cardB)
//static System.Int32 DLL_CardIsThisTunerMonster(System.Int32 cardId)
//static System.Void DLL_SetCardGenre(System.Byte[] data)
//static System.Void DLL_SetCardLink(System.Byte[] data, System.Int32 size)
//static System.Void DLL_SetCardNamed(System.Byte[] data)
//static System.Int32 DLL_SetCardProperty(System.Byte[] data, System.Int32 size)
//static System.Void DLL_SetCardSame(System.Byte[] data, System.Int32 size)
//static System.Void DLL_SetInternalID(System.Byte[] data)