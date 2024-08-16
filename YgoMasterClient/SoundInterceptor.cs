using IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMasterClient
{
    static class SoundInterceptor
    {
        delegate void Del_PlayBGM(int idx, float delay);
        static Hook<Del_PlayBGM> hookPlayBGM;

        static SoundInterceptor()
        {
            IL2Assembly gameAssembly = Assembler.GetAssembly("Assembly-CSharp");
            hookPlayBGM = new Hook<Del_PlayBGM>(PlayBGM, gameAssembly.GetClass("Sound", "YgomGame.Duel").GetMethod("PlayBGM"));
        }

        static void PlayBGM(int idx, float delay)
        {
            // Only play BGM 1
            bool singleBgm = YgomSystem.Utility.ClientWork.GetByJsonPath<csbool>("Duel.SingleBgm");

            // Play BGM 1/2 but not BGM 3
            bool doubleBgm = YgomSystem.Utility.ClientWork.GetByJsonPath<csbool>("Duel.DoubleBgm");

            // Only play BGM 1/3 but not BGM 2
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
