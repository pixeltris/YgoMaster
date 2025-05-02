using IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMasterClient
{
    unsafe static class SoundInterceptor
    {
        static IL2Method methodPlaySE;
        static IL2Method methodStop;

        delegate void Del_PlayBGM(int idx, float delay);
        static Hook<Del_PlayBGM> hookPlayBGM;

        delegate int Del_Play(IntPtr labelName, float delay);
        static Hook<Del_Play> hookPlay;

        static SoundInterceptor()
        {
            IL2Assembly gameAssembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class soundClass = gameAssembly.GetClass("Sound", "YgomGame.Duel");
            methodPlaySE = soundClass.GetMethod("PlaySE", x => x.GetParameters().Length == 1);
            methodStop = soundClass.GetMethod("Stop", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Name == "label");
            hookPlayBGM = new Hook<Del_PlayBGM>(PlayBGM, soundClass.GetMethod("PlayBGM"));
            hookPlay = new Hook<Del_Play>(Play, gameAssembly.GetClass("AudioManager", "USnd").GetMethod("Play"));
        }

        static int Play(IntPtr labelNamePtr, float delay)
        {
            /*string labelName = new IL2String(labelNamePtr).ToString();
            if (labelName == "SE_DUEL_ENTRY_LOOP")
            {
                return -1;
            }*/
            return hookPlay.Original(labelNamePtr, delay);
        }

        static void PlayBGM(int idx, float delay)
        {
            // Only play BGM 0
            bool singleBgm = YgomSystem.Utility.ClientWork.GetByJsonPath<csbool>("Duel.SingleBgm");

            // Play BGM 0/1 but not BGM 2
            bool doubleBgm = YgomSystem.Utility.ClientWork.GetByJsonPath<csbool>("Duel.DoubleBgm");

            // Play BGM 0/2 but not BGM 1
            bool noKeycardBGM = YgomSystem.Utility.ClientWork.GetByJsonPath<csbool>("Duel.NoKeycardBgm");
            
            if (singleBgm && idx > 0)
            {
                return;
            }

            if (doubleBgm && idx == 2)
            {
                return;
            }

            if (noKeycardBGM && idx == 1)
            {
                return;
            }

            hookPlayBGM.Original(idx, delay);
        }

        public static void StopSE(string label, float fade = -1)
        {
            methodStop.Invoke(new IntPtr[] { new IL2String(label).ptr, new IntPtr(&fade) });
        }

        public static void PlaySE(string label)
        {
            methodPlaySE.Invoke(new IntPtr[] { new IL2String(label).ptr });
        }

        /// <summary>
        /// YgomGame.Duel.Sound.DuelBGM
        /// </summary>
        enum DuelBGM
        {
            DuelEarly,
            DuelMiddle,
            DuelLate,
            DuelStart = -1
        }
    }
}
