using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using YgoMaster.Net.Message;

namespace YgoMasterClient
{
    /// <summary>
    /// Helper to sync tapping of mate / duel field
    /// </summary>
    unsafe static class DuelTapSync
    {
        // DuelClient
        static IL2Field fieldDuelClientInstance;
        static IL2Method methodGetEffectWorker;

        // RunEffectWorker
        static IL2Method methodGetGoManager;

        // DuelGameObjectManager
        static IL2Method methodGetBg;

        // BgManager
        static IL2Method methodGetBgUnit;
        static IntPtr nearBgUnit;
        static IntPtr farBgUnit;

        // BgUnit
        static IL2Field fieldActiveCharacter;
        static IL2Field fieldEffectManager;
        delegate void Del_PlayCharaEntryAnimation(IntPtr thisPtr);
        static Hook<Del_PlayCharaEntryAnimation> hookPlayCharaEntryAnimation;

        // BgEffectManagerInner
        static IL2Field fieldTriggerSettings;

        // BgEffectSettingInner
        static IL2Class classInfoBgEffectSettingInner;
        static IL2Field fieldAnimationLabel;
        static IL2Field fieldManager;
        delegate void Del_PlayTapEffect(IntPtr thisPtr);
        static Hook<Del_PlayTapEffect> hookPlayTapEffect;

        // Character
        delegate void Del_PlayTapMotion(IntPtr thisPtr);
        static Hook<Del_PlayTapMotion> hookPlayTapMotion;
        delegate void Del_PlayMotion(IntPtr thisPtr, int motionId);
        static Hook<Del_PlayMotion> hookPlayMotion;
        static bool isDoingPlayTapMotion;

        static bool IsValidState
        {
            get { return nearBgUnit != IntPtr.Zero && farBgUnit != IntPtr.Zero; }
        }

        static DuelTapSync()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class duelClientClass = assembly.GetClass("DuelClient", "YgomGame.Duel");
            fieldDuelClientInstance = duelClientClass.GetField("instance");
            methodGetEffectWorker = duelClientClass.GetProperty("effectWorker").GetGetMethod();
            IL2Class runEffectWorkerClass = assembly.GetClass("RunEffectWorker", "YgomGame.Duel");
            methodGetGoManager = runEffectWorkerClass.GetProperty("goManager").GetGetMethod();
            IL2Class gomClass = assembly.GetClass("DuelGameObjectManager", "YgomGame.Duel");
            methodGetBg = gomClass.GetProperty("bg").GetGetMethod();
            IL2Class bgManagerClass = assembly.GetClass("BgManager", "YgomGame.Bg");
            methodGetBgUnit = bgManagerClass.GetMethod("GetBgUnit", x => x.GetParameters()[0].Name == "isMyself");

            IL2Class bgEffectManagerInnerClass = assembly.GetClass("BgEffectManagerInner", "YgomGame.Bg");
            fieldTriggerSettings = bgEffectManagerInnerClass.GetField("triggerSettings");

            classInfoBgEffectSettingInner = assembly.GetClass("BgEffectSettingInner", "YgomGame.Bg");
            fieldAnimationLabel = classInfoBgEffectSettingInner.GetField("animationLabel");
            fieldManager = classInfoBgEffectSettingInner.GetField("manager");
            hookPlayTapEffect = new Hook<Del_PlayTapEffect>(PlayTapEffect, classInfoBgEffectSettingInner.GetMethod("PlayTapEffect"));

            IL2Class bgUnitClass = assembly.GetClass("BgUnit", "YgomGame.Bg");
            fieldActiveCharacter = bgUnitClass.GetField("activeCharacter");
            fieldEffectManager = bgUnitClass.GetField("effectManager");
            hookPlayCharaEntryAnimation = new Hook<Del_PlayCharaEntryAnimation>(PlayCharaEntryAnimation, bgUnitClass.GetMethod("PlayCharaEntryAnimation"));

            IL2Class characterClass = assembly.GetClass("Character");
            hookPlayTapMotion = new Hook<Del_PlayTapMotion>(PlayTapMotion, characterClass.GetMethod("PlayTapMotion"));
            hookPlayMotion = new Hook<Del_PlayMotion>(PlayMotion, characterClass.GetMethod("PlayMotion"));
        }

        public static void ClearState()
        {
            TradeUtils.AddAction(() =>
            {
                nearBgUnit = IntPtr.Zero;
                farBgUnit = IntPtr.Zero;
            });
        }

        static void PlayCharaEntryAnimation(IntPtr thisPtr)
        {
            // NOTE: We hook here to init state because doing so earlier than this point results in
            // clicked characters getting stuck (can't click them normally and can't sync them)
            hookPlayCharaEntryAnimation.Original(thisPtr);
            InitState();
        }

        static void InitState()
        {
            IL2Object duelClient = fieldDuelClientInstance.GetValue();
            if (duelClient == null)
            {
                return;
            }
            IL2Object effectWorker = methodGetEffectWorker.Invoke(duelClient);
            if (effectWorker == null)
            {
                return;
            }
            IL2Object goManager = methodGetGoManager.Invoke(effectWorker);
            if (goManager == null)
            {
                return;
            }
            IL2Object bg = methodGetBg.Invoke(goManager);
            if (bg == null)
            {
                return;
            }
            bool isMyself = true;
            nearBgUnit = methodGetBgUnit.Invoke(bg.ptr, new IntPtr[] { new IntPtr(&isMyself) }).ptr;
            isMyself = false;
            farBgUnit = methodGetBgUnit.Invoke(bg.ptr, new IntPtr[] { new IntPtr(&isMyself) }).ptr;
        }

        static IntPtr GetActiveCharacter(IntPtr bgUnit)
        {
            IL2Object result = fieldActiveCharacter.GetValue(bgUnit);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        static IntPtr GetBgEffectManagerInner(IntPtr bgUnit)
        {
            IL2Object result = fieldEffectManager.GetValue(bgUnit);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        static void PlayTapMotion(IntPtr thisPtr)
        {
            isDoingPlayTapMotion = true;
            hookPlayTapMotion.Original(thisPtr);
            isDoingPlayTapMotion = false;
        }

        static void PlayMotion(IntPtr thisPtr, int motionId)
        {
            if (isDoingPlayTapMotion && IsValidState && DuelDll.IsPvpDuel && ClientSettings.PvpDuelTapSyncEnabled)
            {
                if (thisPtr == GetActiveCharacter(nearBgUnit))
                {
                    Program.NetClient.Send(new DuelTapSyncMessage()
                    {
                        AnimationId = motionId,
                        Character = true,
                        Near = true
                    });
                }

                if (thisPtr == GetActiveCharacter(farBgUnit))
                {
                    Program.NetClient.Send(new DuelTapSyncMessage()
                    {
                        AnimationId = motionId,
                        Character = true,
                        Near = false
                    });
                }
            }
            hookPlayMotion.Original(thisPtr, motionId);
        }

        static void PlayTapEffect(IntPtr thisPtr)
        {
            if (IsValidState && DuelDll.IsPvpDuel && ClientSettings.PvpDuelTapSyncEnabled)
            {
                int animationLabel = fieldAnimationLabel.GetValue(thisPtr).GetValueRef<int>();
                IntPtr manager = fieldManager.GetValue(thisPtr).ptr;
                if (manager == GetBgEffectManagerInner(nearBgUnit))
                {
                    Program.NetClient.Send(new DuelTapSyncMessage()
                    {
                        AnimationId = animationLabel,
                        Character = false,
                        Near = true
                    });
                }
                else if (manager == GetBgEffectManagerInner(farBgUnit))
                {
                    Program.NetClient.Send(new DuelTapSyncMessage()
                    {
                        AnimationId = animationLabel,
                        Character = false,
                        Near = false
                    });
                }
            }
            hookPlayTapEffect.Original(thisPtr);
        }

        public static void OnDuelTapSync(DuelTapSyncMessage message)
        {
            if (message.Character)
            {
                PlayCharacterTapMotion(message.Near, message.AnimationId);
            }
            else
            {
                PlayBgTapEffect(message.Near, message.AnimationId);
            }
        }

        public static void PlayCharacterTapMotion(bool near, int motionId)
        {
            TradeUtils.AddAction(() =>
            {
                if (!IsValidState)
                {
                    return;
                }

                IntPtr bgUnit = near ? nearBgUnit : farBgUnit;
                IntPtr character = GetActiveCharacter(bgUnit);
                if (character != IntPtr.Zero)
                {
                    hookPlayMotion.Original(character, motionId);
                }
            });
        }

        public static void PlayBgTapEffect(bool near, int animationLabel)
        {
            TradeUtils.AddAction(() =>
            {
                if (!IsValidState)
                {
                    return;
                }

                IntPtr bgUnit = near ? nearBgUnit : farBgUnit;
                IntPtr manager = GetBgEffectManagerInner(bgUnit);
                if (manager != IntPtr.Zero)
                {
                    IL2ListExplicit triggerList = new IL2ListExplicit(fieldTriggerSettings.GetValue(manager).ptr, classInfoBgEffectSettingInner);
                    int count = triggerList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr trigger = triggerList[i];
                        if (fieldAnimationLabel.GetValue(trigger).GetValueRef<int>() == animationLabel)
                        {
                            hookPlayTapEffect.Original(trigger);
                            break;
                        }
                    }
                }
            });
        }
    }
}