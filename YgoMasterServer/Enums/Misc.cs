using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    /// <summary>
    /// TopicsBannerResourceBinder.BannerPatern
    /// </summary>
    enum TopicsBannerPatern
    {
        NOTIFY = 1,// White text (middle)
        ACCESSORY,// White text (bottom left) with faded blue line above
        PACK,// Not sure, results in file error as a card id needs to be specified
        GEM,// White text (middle left)
        SPECIAL,// White text (bottom left) with faded white line above. Also 2 blank images (probably card pack / card id?)
        STANDARD,// Orange text (low)
        EVENT,// White text (middle) with white line underneath
        DUELPASS,// Small white text (top left)
        MAINTENANCE,// Orange text (middle)
        UPDATE,// Black text (bottom middle)
        DUELLIVE,// Small white text (bottom-ish middle)
        STRUCTURE,// Not sure, result in file error as a card id needs to be specified
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
        UltraRare
    }

    /// <summary>
    /// YgomGame.Deck.SearchFilter.Setting.STYLE (sort of)
    /// </summary>
    enum CardStyleRarity
    {
        None,
        Normal,
        Shine,
        Royal
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
        KING
    }

    /// <summary>
    /// YgomGame.Colosseum..ColosseumUtil.PlayMode
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
    }

    /// <summary>
    /// YgomSystem.Network.ServerStatus
    /// </summary>
    enum ServerStatus
    {
        NORMAL,
        INCIDENT,
        MAINTENANCE,
        MAINTENANCE_TEAM
    }

    enum OrbType
    {
        None,
        Dark,
        Light,
        Earth,
        Water,
        Fire,
        Wind
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
        Null
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
        Editor = 100
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
        COMPLETE
    }

    /// <summary>
    /// YgomGame.Solo.SoloModeUtil.UnlockType (defines the type of unlock requirements for a given unlock)
    /// </summary>
    enum ChapterUnlockType
    {
        USER_LEVEL = 1,
        CHAPTER_OR,
        ITEM,
        CHAPTER_AND,
        HAS_ITEM
    }

    /// <summary>
    /// YgomGame.Solo.SoloModeUtil.DeckType
    /// </summary>
    enum SoloDeckType
    {
        POSSESSION,
        STORY
    }

    /// <summary>
    /// IDS_DECKEDIT.HOWTOGET_CATEGORY (off by 1?)
    /// </summary>
    enum HowToObtainCard
    {
        None = 0,
        Pack = 1,
        Solo = 2,
        Tournament = 3,// Assumed
        Exhibition = 4,// Assumed
        //Craft = 5,// Assumed removed
        InitialDistributionStructure = 5,
        SalesStructure = 6,
        Mission = 7,// Assumed
        DuelResult = 8,
        BundleDeals = 9
    }

    /// <summary>
    /// IDS_SCORE (IDS_SCORE.DETAIL_XXX)
    /// </summary>
    enum DuelResultScore
    {
        None,
        DuelVistory,
        Draw,
        ComebackVictory,
        QuickVictory,
        DeckOutVictory,
        SpecialVictories,
        NoDamage,
        LowLP,
        LPOnTheBrink,
        FewCardsLeft,
        CardsOnTheBrink,
        Over3000Damage,
        Over5000Damage,
        Over9999Damage,
        VictoryByEffectDamageOnly,
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
        LinkSummon
    }

    /// <summary>
    /// NOTE: Our values aren't quite right...
    /// 2 = response to clicking "Begin Duel" where opponent hasn't yet clicked it
    /// 3 = what the opponent sees in response to "room_table_polling" after doing the above
    /// 4 = when you do "Begin Duel" after your opponent
    /// 5 = matched? or after coin flip?
    /// 
    /// Not sure what the real enum name is
    /// </summary>
    enum DuelRoomTableState
    {
        None,
        Joinable = 1,
        Matching = 3,
        Matched = 4,
        Dueling = 5
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
        WaitTerm,
        End,
        WaitDestroy,
        ConnectingError,
        Beginning,
        InitSequenceStart = 0,
        InitSequenceEnd = 12
    }

    /// <summary>
    /// YgomGame.Duel.Util.PublicLevel
    /// </summary>
    enum DuelReplayCardVisibility
    {
        AllClose,
        FrontOpen,
        AllOpen
    }
}