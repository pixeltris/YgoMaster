using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YgoMasterClient;
using IL2CPP;
using System.Runtime.InteropServices;

namespace UnityEngine
{
    unsafe static class QualitySettings
    {
        public static void CreateVSyncHook()
        {
            // Hook in C++ land as this is called every frame which kills the performance when entering C#
            IL2Assembly assembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class classInfo = assembly.GetClass("QualitySettings");
            PInvoke.CreateVSyncHook(Marshal.ReadIntPtr(classInfo.GetProperty("vSyncCount").GetSetMethod().ptr));
        }
    }
}

namespace YgomGame.Duel
{
    unsafe static class ReplayControl
    {
        static IL2Field field_ffIconOn;

        delegate void Del_OnTapFast(IntPtr thisPtr);
        static Hook<Del_OnTapFast> hookOnTapFast;

        static ReplayControl()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ReplayControl", "YgomGame.Duel");
            field_ffIconOn = classInfo.GetField("ffIconOn");
            hookOnTapFast = new Hook<Del_OnTapFast>(OnTapFast, classInfo.GetMethod("OnTapFast"));
        }

        static void OnTapFast(IntPtr thisPtr)
        {
            // The state of the button is determined based on the current duel speed setting, so set it to what it should be
            bool enabled = false;
            if (UnityEngine.GameObject.IsActive(field_ffIconOn.GetValue(thisPtr).ptr))
            {
                DuelClient.SetDuelSpeed(DuelClient.DuelSpeed.Fastest);
                enabled = true;
            }
            else
            {
                DuelClient.SetDuelSpeed(DuelClient.DuelSpeed.Normal);
                enabled = false;
            }
            hookOnTapFast.Original(thisPtr);
            enabled = !enabled;
            ClientSettings.LoadTimeMultipliers();
            if (enabled && ClientSettings.ReplayControlsTimeMultiplier != 0)
            {
                // Use a normal duel speed and use our time multiplier instead
                DuelClient.SetDuelSpeed(DuelClient.DuelSpeed.Normal);
                PInvoke.SetTimeMultiplier(ClientSettings.ReplayControlsTimeMultiplier);
            }
            else
            {
                PInvoke.SetTimeMultiplier(ClientSettings.DuelClientTimeMultiplier != 0 ?
                    ClientSettings.DuelClientTimeMultiplier : ClientSettings.TimeMultiplier);
            }
        }
    }

    unsafe static class DuelClient
    {
        static IL2Method methodSetDuelSpeed;

        static DuelClient()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("DuelClient", "YgomGame.Duel");
            methodSetDuelSpeed = classInfo.GetMethod("SetDuelSpeed");
        }

        public static void SetDuelSpeed(DuelSpeed duelSpeed)
        {
            methodSetDuelSpeed.Invoke(new IntPtr[] { new IntPtr(&duelSpeed) });
        }

        public enum DuelSpeed
        {
            Normal,
            Fastest
        }
    }

    unsafe static class CameraShaker
    {
        delegate void Del_Shake1(IntPtr thisPtr, int type);
        static Hook<Del_Shake1> hookShake1;
        delegate void Del_Shake2(IntPtr thisPtr, IntPtr label);
        static Hook<Del_Shake2> hookShake2;

        static CameraShaker()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("CameraShaker", "YgomGame.Duel");
            hookShake1 = new Hook<Del_Shake1>(Shake1, classInfo.GetMethod("Shake", x => x.GetParameters()[0].Name == "type"));
            hookShake2 = new Hook<Del_Shake2>(Shake2, classInfo.GetMethod("Shake", x => x.GetParameters()[0].Name == "label"));
        }

        static void Shake1(IntPtr thisPtr, int type)
        {
            if (ClientSettings.DuelClientDisableCameraShake)
            {
                return;
            }
            hookShake1.Original(thisPtr, type);
        }

        static void Shake2(IntPtr thisPtr, IntPtr label)
        {
            if (ClientSettings.DuelClientDisableCameraShake)
            {
                return;
            }
            hookShake2.Original(thisPtr, label);
        }
    }

    unsafe static class Engine
    {
        static IL2Method methodGetCardNum;
        static IL2Method methodGetCardID;
        static IL2Method methodGetCardUniqueID;

        delegate csbool Del_IsReplayMode();
        static Hook<Del_IsReplayMode> hookIsReplayMode;

        static Engine()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("Engine", "YgomGame.Duel");
            methodGetCardNum = classInfo.GetMethod("GetCardNum");
            methodGetCardID = classInfo.GetMethod("GetCardID");
            methodGetCardUniqueID = classInfo.GetMethod("GetCardUniqueID");
            hookIsReplayMode = new Hook<Del_IsReplayMode>(IsReplayMode, classInfo.GetMethod("IsReplayMode"));
        }

        public static int GetCardNum(int player, int locate)
        {
            return methodGetCardNum.Invoke(new IntPtr[] { new IntPtr(&player), new IntPtr(&locate) }).GetValueRef<int>();
        }

        public static int GetCardID(int player, int position, int index)
        {
            return methodGetCardID.Invoke(new IntPtr[] { new IntPtr(&player), new IntPtr(&position), new IntPtr(&index) }).GetValueRef<int>();
        }

        public static int GetCardUniqueID(int player, int position, int index)
        {
            return methodGetCardUniqueID.Invoke(new IntPtr[] { new IntPtr(&player), new IntPtr(&position), new IntPtr(&index) }).GetValueRef<int>();
        }

        static csbool IsReplayMode()
        {
            if (ClientSettings.ReplayControlsAlwaysEnabled)
            {
                return true;
            }
            return hookIsReplayMode.Original();
        }
    }

    unsafe static class EngineApiUtil
    {
        delegate csbool Del_IsCardKnown(IntPtr thisPtr, int player, int position, int index, bool face);
        static Hook<Del_IsCardKnown> hookIsCardKnown;
        delegate csbool Del_IsInsight(IntPtr thisPtr, int player, int position, int index);
        static Hook<Del_IsInsight> hookIsInsight;

        static EngineApiUtil()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("EngineApiUtil", "YgomGame.Duel");
            hookIsCardKnown = new Hook<Del_IsCardKnown>(IsCardKnown, classInfo.GetMethod("IsCardKnown"));
            hookIsInsight = new Hook<Del_IsInsight>(IsInsight, classInfo.GetMethod("IsInsight"));
        }

        static csbool IsCardKnown(IntPtr thisPtr, int player, int position, int index, bool face)
        {
            if (GenericCardListController.IsUpdatingCustomCardList || ClientSettings.DuelClientMillenniumEye)
            {
                return true;
            }
            return hookIsCardKnown.Original(thisPtr, player, position, index, face);
        }

        static csbool IsInsight(IntPtr thisPtr, int player, int position, int index)
        {
            if (GenericCardListController.IsUpdatingCustomCardList || ClientSettings.DuelClientMillenniumEye)
            {
                return true;
            }
            return hookIsInsight.Original(thisPtr, player, position, index);
        }
    }

    unsafe static class GenericCardListController
    {
        public static bool IsUpdatingCustomCardList;

        static bool isCustomCardList;
        const int positionDeck = 15;
        const int positionBanish = 17;

        static IL2Field fieldType;
        static IL2Method methodGetCurrentDataList;
        static IL2Method methodClose;

        delegate void Del_UpdateList(IntPtr thisPtr, int team, int position);
        static Hook<Del_UpdateList> hookUpdateList;
        delegate void Del_UpdateDataList(IntPtr thisPtr);
        static Hook<Del_UpdateDataList> hookUpdateDataList;
        delegate void Del_SetUidCard(IntPtr thisPtr, int dataindex, IntPtr gob);
        static Hook<Del_SetUidCard> hookSetUidCard;

        static GenericCardListController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("GenericCardListController", "YgomGame.Duel");
            fieldType = classInfo.GetField("m_Type");
            methodGetCurrentDataList = classInfo.GetProperty("m_CurrentDataList").GetGetMethod();
            methodClose = classInfo.GetMethod("Close");
            hookUpdateList = new Hook<Del_UpdateList>(UpdateList, classInfo.GetMethod("UpdateList"));
            hookUpdateDataList = new Hook<Del_UpdateDataList>(UpdateDataList, classInfo.GetMethod("UpdateDataList"));
            hookSetUidCard = new Hook<Del_SetUidCard>(SetUidCard, classInfo.GetMethod("SetUidCard"));
        }

        static void UpdateList(IntPtr thisPtr, int team, int position)
        {
            if (ClientSettings.DuelClientShowRemainingCardsInDeck)
            {
                if ((ListType)fieldType.GetValue(thisPtr).GetValueRef<int>() == ListType.EXCLUDED_TEAM0)
                {
                    if ((position == positionDeck && !isCustomCardList) ||
                        (position == positionBanish && isCustomCardList))
                    {
                        IL2List<int> intList = new IL2List<int>(methodGetCurrentDataList.Invoke(thisPtr).ptr);
                        intList.Clear();
                        int type = (int)ListType.NONE;
                        fieldType.SetValue(thisPtr, new IntPtr(&type));
                    }
                }
                if (team == 0 && position == positionDeck)
                {
                    isCustomCardList = true;
                    position = positionBanish;
                    hookUpdateList.Original(thisPtr, team, position);
                    UpdateDataList(thisPtr);
                    return;
                }
                else
                {
                    bool wasShowingDeckContents = isCustomCardList;
                    isCustomCardList = false;
                }
            }
            hookUpdateList.Original(thisPtr, team, position);
        }

        static void UpdateDataList(IntPtr thisPtr)
        {
            if (isCustomCardList && (ListType)fieldType.GetValue(thisPtr).GetValueRef<int>() == ListType.EXCLUDED_TEAM0)
            {
                IL2List<int> intList = new IL2List<int>(methodGetCurrentDataList.Invoke(thisPtr).ptr);
                intList.Clear();
                int count = Engine.GetCardNum(0, positionDeck);
                Dictionary<int, int> cards = new Dictionary<int, int>();
                for (int i = 0; i < count; i++)
                {
                    int cardId = Engine.GetCardID(0, positionDeck, i);
                    int uid = Engine.GetCardUniqueID(0, positionDeck, i);
                    cards[uid] = cardId;
                }
                foreach (KeyValuePair<int, int> card in cards.OrderBy(x => x.Value))
                {
                    int uid = card.Key;
                    intList.Add(new IntPtr(&uid));
                }
                return;
            }
            hookUpdateDataList.Original(thisPtr);
        }

        static void SetUidCard(IntPtr thisPtr, int dataindex, IntPtr gob)
        {
            if (isCustomCardList)
            {
                IsUpdatingCustomCardList = true;
            }
            hookSetUidCard.Original(thisPtr, dataindex, gob);
            IsUpdatingCustomCardList = false;
        }

        public enum ListType
        {
            NONE,
            EXTRA_TEAM0,
            EXTRA_TEAM1,
            GRAVE_TEAM0,
            GRAVE_TEAM1,
            EXCLUDED_TEAM0,
            EXCLUDED_TEAM1,
            OVERLAYMATLIST_TEAM0,
            OVERLAYMATLIST_TEAM1,
            INHERITEFFECTLIST
        }
    }

    unsafe static class CardIndividualSetting
    {
        delegate csbool Del_IsMonsterCutin(int cardID);
        static Hook<Del_IsMonsterCutin> hookIsMonsterCutin;

        static CardIndividualSetting()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("CardIndividualSetting", "YgomGame.Duel");
            hookIsMonsterCutin = new Hook<Del_IsMonsterCutin>(IsMonsterCutin, classInfo.GetMethod("IsMonsterCutin"));
        }

        static csbool IsMonsterCutin(int cardID)
        {
            if (ClientSettings.DuelClientDisableCutinAnimations)
            {
                return false;
            }
            return hookIsMonsterCutin.Original(cardID);
        }
    }

    unsafe static class CardRunEffectSetting
    {
        delegate IntPtr Del_Get(IntPtr thisPtr, int mrk, int player);
        static Hook<Del_Get> hookGet;

        static CardRunEffectSetting()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("CardRunEffectSetting", "YgomGame.Duel");
            hookGet = new Hook<Del_Get>(Get, classInfo.GetMethod("Get"));
        }

        static IntPtr Get(IntPtr thisPtr, int mrk, int player)
        {
            if (ClientSettings.DuelClientDisableCutinAnimations)
            {
                return IntPtr.Zero;
            }
            return hookGet.Original(thisPtr, mrk, player);
        }
    }

    // NOTE: This will completely break anything involving chains. TODO: Find the actual animation runner
    /*unsafe static class RunEffectWorker
    {
        delegate void Del_Run(IntPtr thisPtr, int param1, int param2, int param3);
        static Hook<Del_Run> hookChainRun;
        static Hook<Del_Run> hookChainStep;
        static Hook<Del_Run> hookChainSet;
        static Hook<Del_Run> hookChainEnd;

        static RunEffectWorker()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("RunEffectWorker", "YgomGame.Duel");
            hookChainRun = new Hook<Del_Run>(ChainRun, classInfo.GetMethod("ChainRun"));
            hookChainStep = new Hook<Del_Run>(ChainStep, classInfo.GetMethod("ChainStep"));
            hookChainSet = new Hook<Del_Run>(ChainSet, classInfo.GetMethod("ChainSet"));
            hookChainEnd = new Hook<Del_Run>(ChainEnd, classInfo.GetMethod("ChainEnd"));
        }

        static void ChainRun(IntPtr thisPtr, int param1, int param2, int param3)
        {
            if (ClientSettings.DuelClientDisableChains)
            {
                return;
            }
            hookChainRun.Original(thisPtr, param1, param2, param3);
        }

        static void ChainStep(IntPtr thisPtr, int param1, int param2, int param3)
        {
            if (ClientSettings.DuelClientDisableChains)
            {
                return;
            }
            hookChainStep.Original(thisPtr, param1, param2, param3);
        }

        static void ChainSet(IntPtr thisPtr, int param1, int param2, int param3)
        {
            if (ClientSettings.DuelClientDisableChains)
            {
                return;
            }
            hookChainSet.Original(thisPtr, param1, param2, param3);
        }

        static void ChainEnd(IntPtr thisPtr, int param1, int param2, int param3)
        {
            if (ClientSettings.DuelClientDisableChains)
            {
                return;
            }
            hookChainEnd.Original(thisPtr, param1, param2, param3);
        }
    }*/
}
