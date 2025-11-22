using IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YgoMasterClient;

namespace YgomGame.Duel
{
    static unsafe class DuelTutorialSetting
    {
        delegate csbool Del_IsTutorialChapter(IntPtr thisPtr, int chapter);
        static Hook<Del_IsTutorialChapter> hookIsTutorialChapter;

        static DuelTutorialSetting()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            hookIsTutorialChapter = new Hook<Del_IsTutorialChapter>(IsTutorialChapter, assembly.GetClass("DuelTutorialSetting", "YgomGame.Duel").GetMethod("IsTutorialChapter"));
        }

        static csbool IsTutorialChapter(IntPtr thisPtr, int chapter)
        {
            if (ClientSettings.SoloRemoveDuelTutorials)
            {
                return false;
            }
            return hookIsTutorialChapter.Original(thisPtr, chapter);
        }
    }
}
