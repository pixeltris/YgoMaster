using IL2CPP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMasterClient
{
    /// <summary>
    /// Fixes language mismatch between LocalSave and LocalData (forces YgoMaster to use the active language)
    /// </summary>
    static class FixLanguage
    {
        delegate void Del_setup();
        static Hook<Del_setup> hooksetup;

        static bool hasFixedLanguage = false;

        static FixLanguage()
        {
            if (Program.IsLive)
            {
                return;
            }

            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("Locale", "YgomSystem.Utility");
            hooksetup = new Hook<Del_setup>(setup, classInfo.GetMethod("setup"));
        }

        static void setup()
        {
            TryFixLanguage();
            hooksetup.Original();
        }

        static void TryFixLanguage()
        {
            if (hasFixedLanguage)
            {
                return;
            }
            hasFixedLanguage = true;
            try
            {
                string dlcVersionFile = Path.Combine(YgomSystem.LocalFileSystem.StandardStorageIO.LocalDataDir, "0000", AssetHelper.GetAssetBundleOnDisk("dlcVersion"));
                if (!File.Exists(dlcVersionFile))
                {
                    return;                    
                }
                Dictionary<string, object> data = new MiniMessagePack.MiniMessagePacker().Unpack(File.ReadAllBytes(dlcVersionFile)) as Dictionary<string, object>;
                if (data == null)
                {
                    return;
                }
                Dictionary<string, object> ncg = YgoMaster.Utils.GetDictionary(data, "ncg");
                if (ncg == null)
                {
                    return;
                }
                foreach (string key in ncg.Keys)
                {
                    if (key.Contains("-"))
                    {
                        string targetLang = key;
                        string currentLang = YgomSystem.Utility.ClientWork.GetStringByJsonPath("$.Persistence.System.lang");
                        if (currentLang != targetLang)
                        {
                            Console.WriteLine("Change LocalSave language (" + currentLang + " -> " + targetLang + ")");
                            YgomSystem.Utility.ClientWork.UpdateValue("$.Persistence.System.lang", targetLang);
                        }
                        break;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
