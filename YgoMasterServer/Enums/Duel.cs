using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    /// <summary>
    /// YgomGame.Duel.Engine.ResultType
    /// </summary>
    enum DuelResultType
    {
        None,
        Win,
        Lose,
        Draw,
        Time
    }

    /// <summary>
    /// YgomGame.Duel.Engine.CpuParam
    /// </summary>
    [Flags]
    enum DuelCpuParam : uint
    {
        None = 0,
        Def = 0x80000000,
        Fool = 0x40000000,
        Light = 0x20000000,
        MyTurnOnly = 0x10000000,
        AttackOnly = 0x04000000,
        Simple = 0x02000000,
        Simple2 = 0x01000000,
        Simples = 0x03000000
    }

    /// <summary>
    /// YgomGame.Duel.Engine.DuelType
    /// </summary>
    enum DuelType
    {
        Normal = 0,
        Extra = 1,
        Tag = 2,
        Speed = 3,//Quick = 3,
        Rush = 4
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
        Chain = 256
    }

    /// <summary>
    /// YgomGame.Duel.Engine.BtlPropFlag
    /// </summary>
    enum DuelBtlPropFlag
    {
        Turn = 1,
        Break = 2,
        Damage = 4
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
        DR
    }

    /// <summary>
    /// YgomGame.Duel.Engine.CardLinkBit
    /// </summary>
    enum DuelCardLinkBit
    {
        UL = 1,
        U = 2,
        UR = 4,
        L = 8,
        R = 16,
        DL = 32,
        D = 64,
        DR = 128
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
        Normal3,//v1.3.1
    }

    /// <summary>
    /// YgomGame.Duel.Engine.CommandBit
    /// </summary>
    [Flags]
    enum DuelCommandBit
    {
        Attack = 1,
        Look = 2,
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
        Draw = 8192
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
        Draw
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
        Illusion,//v1.1.1
        GG,//v1.1.1
        Max
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
        CostEffect
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
        Link
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
        Recover
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
        Always
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
        Confirm
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
        InsStringIfable,//v1.3.1
    }

    /// <summary>
    /// YgomGame.Duel.Engine.DialogOkType
    /// </summary>
    enum DuelDialogOkType
    {
        Stop,
        Once,
        Forever,
        Sub
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
        Link
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
        Close
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
        End
    }

    /// <summary>
    /// YgomGame.Duel.Engine.FieldAnimeType
    /// </summary>
    enum DuelFieldAnimeType
    {
        Null,
        CardMove,
        CardWarp,
        CardSwap
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
        FinishError = 100,
        FinishDisconnect,
        FinishNoContest
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
        Survival_1on2
    }

    /// <summary>
    /// YgomGame.Duel.Engine.ListAttribute
    /// </summary>
    [Flags]
    enum DuelListAttribute
    {
        FromField = 1,
        FromHand = 2,
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
        Activate = 65536,//v1.3.1
        Cost = 131072,//v1.3.1
        End = 262144,//v1.3.1
        FromMask = 63
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
        SelAllCard,
        SelAllDeck,
        SelAllMonst,
        SelAllMonst2,
        SelAllGadget,
        SelAllIndeck
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
        LockOn
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
        DecideCancel
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
        Null = 7
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
        None = -1
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
        CutinActivate
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
        End
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
        PriRunList
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
        AttrMask
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
        Link
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
        End
    }

    /// <summary>
    /// YgomGame.Duel.Engine.TagType
    /// </summary>
    enum DuelTagType
    {
        Single,
        Tag,
        Team
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
        ForceSurrender
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
        RunSpecialefx,//v1.1.1
    }
}
