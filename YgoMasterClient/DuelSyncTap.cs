using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;

/*namespace YgoMasterClient
{
    /// <summary>
    /// Helper to sync tapping of mate / duel field
    /// </summary>
    unsafe static class DuelSyncTap
    {
        static IntPtr nearBgUnit;
        static IntPtr farBgUnit;

        // BgUnit
        static IL2Field fieldActiveCharacter;

        // BgEffectSettingInner
        delegate void Del_PlayTapEffect(IntPtr thisPtr);
        static Hook<Del_PlayTapEffect> hookPlayTapEffect;

        // Character
        delegate void Del_PlayTapMotion(IntPtr thisPtr);
        static Hook<Del_PlayTapMotion> hookPlayTapMotion;

        static bool IsValidState
        {
            get { return nearBgUnit != IntPtr.Zero && farBgUnit != IntPtr.Zero; }
        }

        static DuelSyncTap()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class duelClientClass = assembly.GetClass("DuelClient", "YgomGame.Duel");
            IL2Class runEffectWorkerClass = assembly.GetClass("RunEffectWorker", "YgomGame.Duel");
            IL2Class gomClass = assembly.GetClass("DuelGameObjectManager", "YgomGame.Duel");
            IL2Class bgManagerClass = assembly.GetClass("BgManager", "YgomGame.Bg");
            
            IL2Class bgEffectManagerInnerClass = assembly.GetClass("BgEffectManagerInner", "YgomGame.Bg");
            hookPlayTapEffect = new Hook<Del_PlayTapEffect>(PlayTapEffect, bgEffectManagerInnerClass.GetMethod("PlayTapEffect"));

            IL2Class bgUnitClass = assembly.GetClass("BgUnit", "YgomGame.Bg");
            fieldActiveCharacter = bgUnitClass.GetField("activeCharacter");
            
            IL2Class characterClass = assembly.GetClass("Character");
            hookPlayTapMotion = new Hook<Del_PlayTapMotion>(PlayTapMotion, characterClass.GetMethod("PlayTapMotion"));

            //characterClass.GetMethod("");

        }

        public static void ClearState()
        {
            nearBgUnit = IntPtr.Zero;
            farBgUnit = IntPtr.Zero;
        }

        static IntPtr GetActiveCharacter(IntPtr bgUnit)
        {

        }

        static IntPtr GetBgEffectManagerInner(IntPtr bgUnit)
        {

        }

        static void PlayTapMotion(IntPtr thisPtr)
        {
            if (IsValidState)
            {
                GetActiveCharacter(nearBgUnit);
            }
            hookPlayTapMotion.Original(thisPtr);
        }

        static void PlayTapEffect(IntPtr thisPtr)
        {
            if (IsValidState)
            {
                if (thisPtr == GetBgEffectManagerInner(nearBgUnit))
                {
                    // Tapped near
                }
                else if (thisPtr == GetBgEffectManagerInner(farBgUnit))
                {
                    // Tapped fars
                }
            }
            hookPlayTapEffect.Original(thisPtr);
        }

        public static void PlayCharacterTapMotion(bool near)
        {

        }

        public static void PlayBgTapEffect(bool near)
        {
        }
    }
}*/
/*
    "value": "Prefabs/Duel/UI/CardInfoDetailForDuel",
    "value": "Prefabs/Duel/UI/CardReportTelop",
    "value": "Prefabs/Duel/UI/CommandOperation",
    "value": "Prefabs/Duel/UI/PopUpText",

MateToScreenLocalPoint - might be useful for finding how to position the bubble



BgEffectSettingInner.PlayTapEffect() - play animation
Character.PlayTapMotion

BgUnit

DuelGameObjectManager.bg (BgManager) (property)

DuelClient.instance.effectWorker(property(RunEffectWorker)).goManager(property(DuelGameObjectManager)).bg(property(BgManager)).GetBgUnit

activeCharacter
Character->PlayTapMotion

 */
//Mat_002_near(Clone)/POS_Avatar_near