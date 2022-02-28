using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YgoMasterClient;
using IL2CPP;

namespace YgomGame.Duel
{
    unsafe static class Engine
    {
        static IL2Method methodGetCardNum;
        static IL2Method methodGetCardID;
        static IL2Method methodGetCardUniqueID;

        static Engine()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("Engine", "YgomGame.Duel");
            methodGetCardNum = classInfo.GetMethod("GetCardNum");
            methodGetCardID = classInfo.GetMethod("GetCardID");
            methodGetCardUniqueID = classInfo.GetMethod("GetCardUniqueID");
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
    }

    unsafe static class EngineApiUtil
    {
        delegate bool Del_IsCardKnown(IntPtr thisPtr, int player, int position, int index, bool face);
        static Hook<Del_IsCardKnown> hookIsCardKnown;
        delegate bool Del_IsInsight(IntPtr thisPtr, int player, int position, int index);
        static Hook<Del_IsInsight> hookIsInsight;

        static EngineApiUtil()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("EngineApiUtil", "YgomGame.Duel");
            hookIsCardKnown = new Hook<Del_IsCardKnown>(IsCardKnown, classInfo.GetMethod("IsCardKnown"));
            hookIsInsight = new Hook<Del_IsInsight>(IsInsight, classInfo.GetMethod("IsInsight"));
        }

        static bool IsCardKnown(IntPtr thisPtr, int player, int position, int index, bool face)
        {
            if (GenericCardListController.IsUpdatingCustomCardList)
            {
                return true;
            }
            return hookIsCardKnown.Original(thisPtr, player, position, index, face);
        }

        static bool IsInsight(IntPtr thisPtr, int player, int position, int index)
        {
            if (GenericCardListController.IsUpdatingCustomCardList)
            {
                return true;
            }
            return hookIsInsight.Original(thisPtr, player, position, index);
        }
    }

    unsafe static class GenericCardListController
    {
        public static bool DuelClientShowRemainingCardsInDeck;
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
            if (DuelClientShowRemainingCardsInDeck)
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
}