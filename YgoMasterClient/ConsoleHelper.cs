using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using IL2CPP;
using System.Diagnostics;
using YgoMaster;

namespace YgoMasterClient
{
    class ConsoleHelper
    {
        public static void Run()
        {
            new Thread(delegate ()
            {
                while (true)
                {
                    string consoleInput = Console.ReadLine();
                    Win32Hooks.Invoke(delegate
                    {
                        try
                        {
                            HandleCommand(consoleInput);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
            }).Start();
        }

        unsafe static void HandleCommand(string consoleInput)
        {
            string[] splitted = consoleInput.Split();
            switch (splitted[0].ToLower())
            {
                case "itemid":// Creates json for values in IDS_ITEM (all item ids)
                    {
                        // TODO: Get the exact values from internal data rather than probing by id
                        Console.WriteLine("Getting item ids...");
                        bool dumpInvalid = false;
                        if (splitted.Length > 1)
                        {
                            bool.TryParse(splitted[1], out dumpInvalid);
                        }
                        Dictionary<ItemID.Category, List<string>> categories = new Dictionary<ItemID.Category, List<string>>();
                        foreach (ItemID.Category category in Enum.GetValues(typeof(ItemID.Category)))
                        {
                            switch (category)
                            {
                                case ItemID.Category.NONE:
                                case ItemID.Category.CARD:
                                    continue;
                            }
                            int offset = YgomGame.Utility.ItemUtil.GetCategoryOffset(category);
                            for (int id = offset; id <= offset + 9999; id++)
                            {
                                if (YgomGame.Utility.ItemUtil.GetCategoryFromID(id) != category)
                                {
                                    continue;
                                }
                                string name = YgomGame.Utility.ItemUtil.GetItemName(id);
                                if (string.IsNullOrWhiteSpace(name))
                                {
                                    continue;
                                }
                                if (!categories.ContainsKey(category))
                                {
                                    categories[category] = new List<string>();
                                }
                                bool invalid = string.IsNullOrEmpty(name) || name == "deleted" || name == "coming soon" || name.StartsWith("ICON_FRAME");
                                if (invalid && !dumpInvalid)
                                {
                                    continue;
                                }
                                string prefix = "    " + (invalid ? "//" : "");
                                categories[category].Add(prefix + id + ",//" + name);
                            }
                            Console.WriteLine("Done " + category);
                        }
                        StringBuilder res = new StringBuilder();
                        res.AppendLine("{");
                        foreach (KeyValuePair<ItemID.Category, List<string>> cat in categories.OrderBy(x => x.Key))
                        {
                            res.AppendLine("  \"" + cat.Key + "\": [");
                            res.AppendLine(string.Join(Environment.NewLine, cat.Value));
                            res.AppendLine("  ],");
                        }
                        res.AppendLine("}");
                        File.WriteAllText("ItemID.json", res.ToString());
                        Console.WriteLine("Done");
                    }
                    break;
                case "itemid_old":// Creates json for values in IDS_ITEM (all item ids)
                    {
                        bool dumpInvalid = false;
                        if (splitted.Length > 1)
                        {
                            bool.TryParse(splitted[1], out dumpInvalid);
                        }
                        Dictionary<ItemID.Category, List<string>> categories = new Dictionary<ItemID.Category, List<string>>();
                        IL2Class classInfo = classInfo = Assembler.GetAssembly("Assembly-CSharp").GetClass("IDS_ITEM", "YgomGame.TextIDs");
                        IL2Field[] fields = classInfo.GetFields();
                        foreach (IL2Field field in fields)
                        {
                            string fieldName = field.Name;
                            int id;
                            if (fieldName.StartsWith("ID") && int.TryParse(fieldName.Substring(2), out id) &&
                                (ClientSettings.BrokenItems == null || !ClientSettings.BrokenItems.Contains(id)))
                            {
                                string name = YgomGame.Utility.ItemUtil.GetItemName(id);
                                if (IsWeirdName(name))
                                {
                                    continue;
                                }
                                ItemID.Category cat = YgomGame.Utility.ItemUtil.GetCategoryFromID(id);
                                if (!categories.ContainsKey(cat))
                                {
                                    categories[cat] = new List<string>();
                                }
                                bool invalid = string.IsNullOrEmpty(name) || name == "deleted" || name == "coming soon" || name.StartsWith("ICON_FRAME");
                                if (invalid && !dumpInvalid)
                                {
                                    continue;
                                }
                                string prefix = "    " + (invalid ? "//" : "");
                                categories[cat].Add(prefix + id + ",//" + name);
                            }
                        }
                        StringBuilder res = new StringBuilder();
                        res.AppendLine("{");
                        foreach (KeyValuePair<ItemID.Category, List<string>> cat in categories.OrderBy(x => x.Key))
                        {
                            res.AppendLine("  \"" + cat.Key + "\": [");
                            res.AppendLine(string.Join(Environment.NewLine, cat.Value));
                            res.AppendLine("  ],");
                        }
                        res.AppendLine("}");
                        File.WriteAllText("ItemID.json", res.ToString());
                        Console.WriteLine("Done");
                    }
                    break;
                case "itemid_enum":// Creates enums for values in IDS_ITEM (all item ids)
                    {
                        Dictionary<ItemID.Category, List<string>> categories = new Dictionary<ItemID.Category, List<string>>();
                        IL2Class classInfo = classInfo = Assembler.GetAssembly("Assembly-CSharp").GetClass("IDS_ITEM", "YgomGame.TextIDs");
                        IL2Field[] fields = classInfo.GetFields();
                        foreach (IL2Field field in fields)
                        {
                            string fieldName = field.Name;
                            int id;
                            if (fieldName.StartsWith("ID") && int.TryParse(fieldName.Substring(2), out id))
                            {
                                string name = YgomGame.Utility.ItemUtil.GetItemName(id);
                                ItemID.Category cat = YgomGame.Utility.ItemUtil.GetCategoryFromID(id);
                                if (!categories.ContainsKey(cat))
                                {
                                    categories[cat] = new List<string>();
                                }
                                //name = name.Replace("\"", "\\\"");
                                string prefix = name == "deleted" ? "//" : "";
                                //categories[cat].Add(prefix + "[Name(\"" + name + "\")]" + fieldName + " = " + id + ",");
                                categories[cat].Add(prefix + fieldName + " = " + id + ",//" + name);
                            }
                        }
                        StringBuilder res = new StringBuilder();
                        foreach (KeyValuePair<ItemID.Category, List<string>> cat in categories)
                        {
                            res.AppendLine("public enum " + cat.Key + "{");
                            res.AppendLine(string.Join(Environment.NewLine, cat.Value));
                            res.AppendLine("}");
                        }
                        File.WriteAllText("dump-itemid-enum.txt", res.ToString());
                        Console.WriteLine("Done");
                    }
                    break;
                case "packnames":// Gets all the card pack names
                    {
                        // TODO: Ensure IDS_CARDPACK is loaded before calling GetText (currently need to be in shop screen)
                        IL2Class classInfo = Assembler.GetAssembly("Assembly-CSharp").GetClass("IDS_CARDPACK", "YgomGame.TextIDs");
                        IL2Field[] fields = classInfo.GetFields();
                        StringBuilder sb = new StringBuilder();
                        Dictionary<int, string> idVals = new Dictionary<int, string>();
                        foreach (IL2Field field in fields)
                        {
                            string fieldName = field.Name;
                            if (fieldName.StartsWith("ID") && fieldName.EndsWith("_NAME"))
                            {
                                int id = int.Parse(((fieldName.Split('_'))[0]).Substring(2));
                                idVals[id] = YgomSystem.Utility.TextData.GetText("IDS_CARDPACK." + fieldName);
                            }
                        }
                        foreach (KeyValuePair<int, string> idVal in idVals.OrderBy(x => x.Key))
                        {
                            if (!string.IsNullOrEmpty(idVal.Value))
                            {
                                sb.AppendLine("ID" + idVal.Key.ToString().PadLeft(4, '0') + "_NAME" + " " + idVal.Value);
                            }
                        }
                        File.WriteAllText("dump-packnames.txt", sb.ToString());
                        Console.WriteLine("Done");
                    }
                    break;
                case "packimages":// Attempts to discover all card pack images (which are based on a given card id)
                    {
                        Console.WriteLine("Dumping pack image names...");
                        // TODO: Improve this (get a card id list as currently this will be very slow)
                        StringBuilder sb = new StringBuilder();
                        sb.Append("\"PackShopImages\": [");
                        sb.Append("\"CardPackTex01_0000\",");
                        for (int j = 0; j < 30000; j++)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                string name = "CardPackTex" + i.ToString().PadLeft(2, '0') + "_" + j;
                                if (AssetHelper.FileExists("Images/CardPack/<_RESOURCE_TYPE_>/<_CARD_ILLUST_>/" + name))
                                {
                                    sb.Append("\"" + name + "\",");
                                }
                            }
                        }
                        sb.Append("]");
                        File.WriteAllText("dump-packimages.txt", sb.ToString());
                        Console.WriteLine("Done");
                    }
                    break;
                case "text":// Gets a IDS_XXXX value based on the input string (e.g. "IDS_CARD.STYLE3")
                    {
                        string id = splitted[1];
                        Console.WriteLine(YgomSystem.Utility.TextData.GetText(id));
                    }
                    break;
                case "textenum":// Gets all text from the given enum
                    {
                        string enumName = splitted[1];
                        IL2Class classInfo = Assembler.GetAssembly("Assembly-CSharp").GetClass(enumName, "YgomGame.TextIDs");
                        if (classInfo == null)
                        {
                            Console.WriteLine("Invalid enum");
                        }
                        else
                        {
                            YgomSystem.Utility.TextData.LoadGroup(enumName);
                            foreach (IL2Field field in classInfo.GetFields())
                            {
                                string fieldName = field.Name;
                                string str = YgomSystem.Utility.TextData.GetText(enumName + "." + fieldName);
                                if (!string.IsNullOrEmpty(str))
                                {
                                    Console.WriteLine(fieldName);
                                    Console.WriteLine(str);
                                }
                            }
                        }
                    }
                    break;
                case "textdump":// Dumps all IDS enums
                    {
                        foreach (IL2Class classInfo in Assembler.GetAssembly("Assembly-CSharp").GetClasses())
                        {
                            if (classInfo.IsEnum && classInfo.Namespace == "YgomGame.TextIDs" && classInfo.Name.StartsWith("IDS"))
                            {
                                StringBuilder sb = new StringBuilder();
                                YgomSystem.Utility.TextData.LoadGroup(classInfo.Name);
                                foreach (IL2Field field in classInfo.GetFields())
                                {
                                    if (field.Name == "value__")
                                    {
                                        continue;
                                    }
                                    string name = classInfo.Name + "." + field.Name;
                                    string str = YgomSystem.Utility.TextData.GetText(name);
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        sb.AppendLine("[" + name + "]" + Environment.NewLine + str);
                                    }
                                }
                                if (sb.Length > 0)
                                {
                                    string targetPath = Path.Combine(Program.ClientDataDumpDir, "IDS", classInfo.Name + ".txt");
                                    try
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                                    }
                                    catch { }
                                    string text = sb.ToString();
                                    if (text.EndsWith(Environment.NewLine))
                                    {
                                        text = text.Substring(0, text.Length - Environment.NewLine.Length);
                                    }
                                    File.WriteAllText(targetPath, text);
                                }
                            }
                        }
                        Console.WriteLine("Done");
                    }
                    break;
                case "textreload":// Reloads custom text data (IDS)
                    {
                        YgomSystem.Utility.TextData.LoadCustomTextData();
                        Console.WriteLine("Done");
                    }
                    break;
                case "soloreload":// Reloads custom solo data
                    {
                        AssetHelper.LoadSoloData();
                        Console.WriteLine("Done");
                    }
                    break;
                case "resultcodes":// Gets all network result codes found in "YgomSystem.Network"
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (IL2Class classInfo in Assembler.GetAssembly("Assembly-CSharp").GetClasses())
                        {
                            if (classInfo.Namespace == "YgomSystem.Network" && classInfo.IsEnum && classInfo.Name.EndsWith("Code"))
                            {
                                sb.AppendLine("public enum " + classInfo.Name);
                                sb.AppendLine("{");
                                foreach (IL2Field field in classInfo.GetFields())
                                {
                                    if (field.Name == "value__")
                                    {
                                        continue;
                                    }
                                    sb.AppendLine("    " + field.Name + " = " + field.GetValue().GetValueRef<int>() + ",");
                                }
                                sb.AppendLine("}");
                                sb.AppendLine();
                            }
                        }
                        File.WriteAllText("dump-resultcodes.txt", sb.ToString());
                        Console.WriteLine("Done");
                    }
                    break;
                // TODO: Remove "locate"/"locateraw"? It doesn't serve much purpose and "crc" is more accurate
                //       - One reason to keep locate/locateraw is that it might be useful for non-existing files
                //         as "crc" might do the auto conversion wrong for non-existing files (SD/HighEnd_HD)
                case "locate":// Finds a file on disk from the given input path which is in /LocalData/
                case "locateraw":// Don't convert the file path
                    {
                        // NOTE: Some images (just cards?) resolve to /masterduel_Data/StreamingAssets/AssetBundle/
                        string path = consoleInput.Trim().Substring(consoleInput.Trim().IndexOf(' ') + 1);
                        string convertedPath = splitted[0].ToLower() == "locate" ? AssetHelper.ConvertAssetPath(path) : path;
                        bool exists = AssetHelper.FileExists(path);
                        string convertedPathOnDisk = AssetHelper.GetAssetBundleOnDiskConverted(convertedPath);
                        string autoConvertPathOnDisk = AssetHelper.GetAssetBundleOnDisk(path);
                        string dir = YgomSystem.LocalFileSystem.StandardStorageIO.LocalDataDir;
                        bool existsOnDisk = File.Exists(Path.Combine(dir, "0000", convertedPathOnDisk));
                        bool existsOnDiskNoConvert = File.Exists(Path.Combine(dir, "0000", autoConvertPathOnDisk));
                        Console.WriteLine("Converted: " + convertedPath);
                        Console.WriteLine(convertedPathOnDisk + " existsOnDisk: " + existsOnDisk +
                            " existsOnDisk(auto):" + existsOnDiskNoConvert + " exists:" + exists);
                    }
                    break;
                case "crc":// Gets the CRC of a file path and shows the file in explorer if it exists
                    {
                        string path = consoleInput.Trim().Substring(consoleInput.Trim().IndexOf(' ') + 1);
                        string pathOnDisk = AssetHelper.GetAssetBundleOnDisk(path);
                        string dir = YgomSystem.LocalFileSystem.StandardStorageIO.LocalDataDir;
                        string fullPath = Path.Combine(dir, "0000", pathOnDisk);
                        Console.WriteLine(pathOnDisk);
                        if (File.Exists(fullPath))
                        {
                            try
                            {
                                Process.Start("explorer.exe", "/select, \"" + fullPath + "\"");
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            Console.WriteLine("File does not exist on disk");
                        }
                    }
                    break;
                case "carddata":// dumps card data
                    {
                        IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                        IL2Class contentClassInfo = assembly.GetClass("Content", "YgomGame.Card");
                        IntPtr instance = contentClassInfo.GetField("s_instance").GetValue().ptr;
                        string intIdPath = contentClassInfo.GetField("IntIdPath").GetValue(instance).GetValueObj<string>();
                        Console.WriteLine("intIdPath: " + intIdPath);
                        string path = intIdPath.Replace("/#/", "/");
                        path = path.Substring(0, path.LastIndexOf('/'));
                        Console.WriteLine("path: " + path);
                        string[] files =
                        {
                            path + "/#/CARD_Genre",
                            path + "/#/CARD_IntID",
                            path + "/#/CARD_Named",
                            path + "/#/CARD_Prop",
                            path + "/#/CARD_RubyIndx",
                            path + "/#/CARD_RubyName",
                            path + "/en-US/CARD_Desc",
                            path + "/en-US/CARD_Indx",
                            path + "/en-US/CARD_Name",
                            path + "/en-US/DLG_Indx",
                            path + "/en-US/DLG_Text",
                            path + "/en-US/WORD_Indx",
                            path + "/en-US/WORD_Text",
                            path + "/MD/all_gadget_monsters",
                            path + "/MD/all_monsters",
                            path + "/MD/cards_all",
                            path + "/MD/cards_in_maindeck",
                            path + "/MD/CARD_Link",
                            path + "/MD/CARD_Same",
                            path + "/MD/monsters_in_maindeck"
                        };
                        foreach (string file in files)
                        {
                            if (file.Contains("CARD_") || file.Contains("DLG_") || file.Contains("WORD_"))
                            {
                                byte[] buffer = AssetHelper.GetBytesDecryptionData(file);
                                if (buffer != null && buffer.Length > 0)
                                {
                                    string fullPath = Path.Combine(Program.ClientDataDumpDir, file + ".bytes");
                                    try
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                    }
                                    catch { }
                                    try
                                    {
                                        File.WriteAllBytes(fullPath, buffer);
                                    }
                                    catch { }
                                }
                            }
                        }
                        Console.WriteLine("Done");
                    }
                    break;
                case "updatediff":// Dumps referenced enums / functions (use this to diff and update enums / funcs)
                    {
                        IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                        IL2Class engineClass = assembly.GetClass("Engine", "YgomGame.Duel");
                        IL2Class contentClass = assembly.GetClass("Content", "YgomGame.Card");
                        Dictionary<string, IL2Class> allEnums = new Dictionary<string, IL2Class>();
                        allEnums["Card.cs"] = null;
                        allEnums["CardFrame"] = contentClass.GetNestedType("Frame");
                        allEnums["CardKind"] = contentClass.GetNestedType("Kind");
                        allEnums["CardIcon"] = contentClass.GetNestedType("Icon");
                        allEnums["Misc.cs"] = null;
                        allEnums["TopicsBannerPatern"] = assembly.GetClass("TopicsBannerResourceBinder", "YgomGame.Menu.Common").GetNestedType("BannerPatern");
                        allEnums["CardRarity"] = contentClass.GetNestedType("Rarity");
                        allEnums["CardStyleRarity"] = assembly.GetClass("SearchFilter", "YgomGame.Deck").GetNestedType("Setting").GetNestedType("STYLE");
                        allEnums["StandardRank"] = assembly.GetClass("ColosseumUtil", "YgomGame.Colosseum").GetNestedType("StandardRank");
                        allEnums["PlayMode"] = assembly.GetClass("ColosseumUtil", "YgomGame.Colosseum").GetNestedType("PlayMode");
                        allEnums["ServerStatus"] = assembly.GetClass("ServerStatus", "YgomSystem.Network");
                        allEnums["GameMode"] = assembly.GetClass("Util", "YgomGame.Duel").GetNestedType("GameMode");
                        allEnums["PlatformID"] = assembly.GetClass("Util", "YgomGame.Duel").GetNestedType("PlatformID");
                        allEnums["ChapterStatus"] = assembly.GetClass("SoloModeUtil", "YgomGame.Solo").GetNestedType("ChapterStatus");
                        allEnums["ChapterUnlockType"] = assembly.GetClass("SoloModeUtil", "YgomGame.Solo").GetNestedType("UnlockType");
                        allEnums["SoloDeckType"] = assembly.GetClass("SoloModeUtil", "YgomGame.Solo").GetNestedType("DeckType");
                        allEnums["RoomEntryViewController.Mode"] = assembly.GetClass("RoomEntryViewController", "YgomGame.Room").GetNestedType("Mode");
                        allEnums["HowToObtainCard"] = null;
                        allEnums["DuelResultScore"] = null;
                        //allEnums["DuelRoomTableState"] = null;// TODO: Generate
                        allEnums["DuelClientStep"] = assembly.GetClass("DuelClient", "YgomGame.Duel").GetNestedType("Step");
                        allEnums["DuelReplayCardVisibility"] = assembly.GetClass("Util", "YgomGame.Duel").GetNestedType("PublicLevel");
                        allEnums["Category"] = assembly.GetClass("ItemUtil", "YgomGame.Utility").GetNestedType("Category");
                        allEnums["Duel.cs"] = null;
                        allEnums["DuelResultType"] = engineClass.GetNestedType("ResultType");
                        allEnums["DuelCpuParam"] = engineClass.GetNestedType("CpuParam");
                        allEnums["DuelType"] = engineClass.GetNestedType("DuelType");
                        allEnums["DuelAffectType"] = engineClass.GetNestedType("AffectType");
                        allEnums["DuelBtlPropFlag"] = engineClass.GetNestedType("BtlPropFlag");
                        allEnums["DuelCardLink"] = engineClass.GetNestedType("CardLink");
                        allEnums["DuelCardLinkBit"] = engineClass.GetNestedType("CardLinkBit");
                        allEnums["DuelCardMoveType"] = engineClass.GetNestedType("CardMoveType");
                        allEnums["DuelCommandBit"] = engineClass.GetNestedType("CommandBit");
                        allEnums["DuelCommandType"] = engineClass.GetNestedType("CommandType");
                        allEnums["DuelCounterType"] = engineClass.GetNestedType("CounterType");
                        allEnums["DuelCutinActivateType"] = engineClass.GetNestedType("CutinActivateType");
                        allEnums["DuelCutinSummonType"] = engineClass.GetNestedType("CutinSummonType");
                        allEnums["DuelDamageType"] = engineClass.GetNestedType("DamageType");
                        allEnums["DuelDialogEffectType"] = engineClass.GetNestedType("DialogEffectType");
                        allEnums["DuelDialogInfo"] = engineClass.GetNestedType("DialogInfo");
                        allEnums["DuelDialogMixTextType"] = engineClass.GetNestedType("DialogMixTextType");
                        allEnums["DuelDialogOkType"] = engineClass.GetNestedType("DialogOkType");
                        allEnums["DuelDialogRitualType"] = engineClass.GetNestedType("DialogRitualType");
                        allEnums["DuelDialogType"] = engineClass.GetNestedType("DialogType");
                        allEnums["DuelDmgStepType"] = engineClass.GetNestedType("DmgStepType");
                        allEnums["DuelFieldAnimeType"] = engineClass.GetNestedType("FieldAnimeType");
                        allEnums["DuelFinishType"] = engineClass.GetNestedType("FinishType");
                        allEnums["DuelLimitedType"] = engineClass.GetNestedType("LimitedType");
                        allEnums["DuelListAttribute"] = engineClass.GetNestedType("ListAttribute");
                        allEnums["DuelListType"] = engineClass.GetNestedType("ListType");
                        allEnums["DuelMenuActType"] = engineClass.GetNestedType("MenuActType");
                        allEnums["DuelMenuParamType"] = engineClass.GetNestedType("MenuParamType");
                        allEnums["DuelPhase"] = engineClass.GetNestedType("Phase");
                        allEnums["DuelPlayerType"] = engineClass.GetNestedType("PlayerType");
                        allEnums["DuelPvpCommand"] = assembly.GetClass("PvP", "YgomSystem.Network").GetNestedType("Command");
                        allEnums["DuelPvpCommandType"] = engineClass.GetNestedType("PvpCommandType");
                        allEnums["DuelPvpFieldType"] = engineClass.GetNestedType("PvpFieldType");
                        allEnums["DuelRunCommandType"] = engineClass.GetNestedType("RunCommandType");
                        allEnums["DuelShowParam"] = engineClass.GetNestedType("ShowParam");
                        allEnums["DuelSpSummonType"] = engineClass.GetNestedType("SpSummonType");
                        allEnums["DuelStepType"] = engineClass.GetNestedType("StepType");
                        allEnums["DuelTagType"] = engineClass.GetNestedType("TagType");
                        allEnums["DuelToEngineActType"] = engineClass.GetNestedType("ToEngineActType");
                        allEnums["DuelViewType"] = engineClass.GetNestedType("ViewType");

                        using (TextWriter tw = File.CreateText("updatediff.cs"))
                        {
                            tw.WriteLine("// Client version " + assembly.GetClass("Version", "YgomSystem.Utility").GetProperty("AppVersion").GetGetMethod().Invoke().GetValueObj<string>());
                            tw.WriteLine("// This file is generated using the 'updatediff' command in YgoMasterClient. This information is used to determine changes between client versions which impact YgoMaster.");
                            tw.WriteLine("// Run the command, diff against the old file, and use the changes to update code.");
                            tw.WriteLine();

                            foreach (KeyValuePair<string, IL2Class> enumClass in allEnums)
                            {
                                if (enumClass.Value == null)
                                {
                                    if (enumClass.Key.EndsWith(".cs"))
                                    {
                                        tw.WriteLine("//==================================");
                                        tw.WriteLine("// " + enumClass.Key);
                                        tw.WriteLine("//==================================");
                                    }
                                    else
                                    {
                                        switch (enumClass.Key)
                                        {
                                            case "HowToObtainCard":
                                                YgomSystem.Utility.TextData.LoadGroup("IDS_DECKEDIT");
                                                IL2Class idsDeckEditClass = assembly.GetClass("IDS_DECKEDIT", "YgomGame.TextIDs");
                                                tw.WriteLine("/// <summary>");
                                                tw.WriteLine("/// IDS_DECKEDIT.HOWTOGET_CATEGORY (off by 1?)");
                                                tw.WriteLine("/// </summary>");
                                                tw.WriteLine("enum HowToObtainCard");
                                                tw.WriteLine("{");
                                                tw.WriteLine("    None,");
                                                foreach (IL2Field field in idsDeckEditClass.GetFields())
                                                {
                                                    if (field.Name.StartsWith("HOWTOGET_CATEGORY"))
                                                    {
                                                        string str = YgomSystem.Utility.TextData.GetText(idsDeckEditClass.Name + "." + field.Name);
                                                        str = FixWeirdNameForEnum(str);
                                                        str = str.Replace(" ", string.Empty);
                                                        if (!string.IsNullOrEmpty(str))
                                                        {
                                                            tw.WriteLine("    " + str + ",// " + field.Name);
                                                        }
                                                    }
                                                }
                                                tw.WriteLine("}");
                                                break;
                                            case "DuelResultScore":
                                                YgomSystem.Utility.TextData.LoadGroup("IDS_SCORE");
                                                IL2Class idsScoreClass = assembly.GetClass("IDS_SCORE", "YgomGame.TextIDs");
                                                tw.WriteLine("/// <summary>");
                                                tw.WriteLine("/// IDS_SCORE (IDS_SCORE.DETAIL_XXX)");
                                                tw.WriteLine("/// </summary>");
                                                tw.WriteLine("enum DuelResultScore");
                                                tw.WriteLine("{");
                                                tw.WriteLine("    None,");
                                                foreach (IL2Field field in idsScoreClass.GetFields())
                                                {
                                                    if (field.Name.StartsWith("DETAIL_"))
                                                    {
                                                        string str = YgomSystem.Utility.TextData.GetText(idsScoreClass.Name + "." + field.Name);
                                                        str = FixWeirdNameForEnum(str);
                                                        str = str.Replace(" ", string.Empty);
                                                        str = str.Replace("!", string.Empty);
                                                        tw.WriteLine("    " + str + ",");
                                                    }
                                                }
                                                tw.WriteLine("}");
                                                break;
                                            default:
                                                Console.WriteLine(enumClass.Key + " is null");
                                                break;
                                        }
                                    }
                                    continue;
                                }

                                tw.WriteLine("/// <summary>");
                                tw.WriteLine("/// " + enumClass.Value.FullNameEx);
                                tw.WriteLine("/// </summary>");
                                tw.WriteLine("enum " + enumClass.Key);
                                tw.WriteLine("{");
                                int nextValue = 0;
                                foreach (IL2Field field in enumClass.Value.GetFields())
                                {
                                    if (field.Name == "value__")
                                    {
                                        continue;
                                    }
                                    int value = field.GetValue().GetValueRef<int>();
                                    tw.WriteLine("    " + field.Name + (nextValue == value ? "," : " = " + value + ","));
                                    nextValue = value + 1;
                                }
                                tw.WriteLine("}");
                            }

                            tw.WriteLine("//==================================");
                            tw.WriteLine("// ResultCodes.cs");
                            tw.WriteLine("//==================================");
                            foreach (IL2Class enumClass in assembly.GetClasses().OrderBy(x => x.Name))
                            {
                                if (enumClass.Namespace == "YgomSystem.Network" && enumClass.IsEnum)
                                {
                                    tw.WriteLine("enum " + enumClass.Name);
                                    tw.WriteLine("{");
                                    int nextValue = 0;
                                    foreach (IL2Field field in enumClass.GetFields())
                                    {
                                        if (field.Name == "value__")
                                        {
                                            continue;
                                        }
                                        int value = field.GetValue().GetValueRef<int>();
                                        tw.WriteLine("    " + field.Name + (nextValue == value ? "," : " = " + value + ","));
                                        nextValue = value + 1;
                                    }
                                    tw.WriteLine("}");
                                }
                            }

                            tw.WriteLine("//==================================");
                            tw.WriteLine("// Network API");
                            tw.WriteLine("//==================================");
                            IL2Class apiClass = assembly.GetClass("API", "YgomSystem.Network");
                            foreach (IL2Method method in apiClass.GetMethods())
                            {
                                tw.WriteLine("//" + method.GetSignature());
                            }

                            tw.WriteLine("//==================================");
                            tw.WriteLine("// duel.dll functions (Engine)");
                            tw.WriteLine("//==================================");
                            foreach (IL2Method method in engineClass.GetMethods().OrderBy(x => x.Name))
                            {
                                if (method.Name.StartsWith("DLL_"))
                                {
                                    tw.WriteLine("//" + method.GetSignature());
                                }
                            }

                            tw.WriteLine("//==================================");
                            tw.WriteLine("// duel.dll functions (Content)");
                            tw.WriteLine("//==================================");
                            foreach (IL2Method method in contentClass.GetMethods().OrderBy(x => x.Name))
                            {
                                if (method.Name.StartsWith("DLL_"))
                                {
                                    tw.WriteLine("//" + method.GetSignature());
                                }
                            }
                        }

                        Console.WriteLine("Done");
                    }
                    break;
                case "updatejson":// Update the json data store for data that is sent server->client (useful for testing)
                    YgomSystem.Utility.ClientWork.UpdateJson(splitted[1], splitted[2]);
                    Console.WriteLine("Done");
                    break;
                case "updatejsonraw":// Update the json data store for data that is sent server->client (useful for testing)
                    YgomSystem.Utility.ClientWork.UpdateJson(splitted[1]);
                    Console.WriteLine("Done");
                    break;
                case "logjson":// Logs json object at path
                    Console.WriteLine(YgomSystem.Utility.ClientWork.SerializePath(splitted[1]));
                    break;
                case "cardswithart":// List all cards with art (requires setup of YgoMaster/Data/CardData/)
                    {
                        HashSet<int> missingCardsWithArt = new HashSet<int>();
                        IntPtr cardRarePtr = YgomSystem.Utility.ClientWork.GetByJsonPath("$.Master.CardRare");
                        if (cardRarePtr != IntPtr.Zero)
                        {
                            IL2Dictionary<string, object> cardRare = new IL2Dictionary<string, object>(cardRarePtr);
                            Dictionary<int, YdkHelper.GameCardInfo> cards = YdkHelper.LoadCardDataFromGame(Program.DataDir);
                            foreach (KeyValuePair<int, YdkHelper.GameCardInfo> card in cards)
                            {
                                if (!cardRare.ContainsKey(card.Key.ToString()) && AssetHelper.FileExists("Card/Images/Illust/tcg/" + card.Key) && card.Value.Frame != YgoMaster.CardFrame.Token)
                                {
                                    missingCardsWithArt.Add(card.Key);
                                }
                            }
                        }
                        Console.WriteLine("Missing cards with art: " + string.Join(", ", missingCardsWithArt));
                    }
                    break;
                case "vcargs":// Get the args for the top view controller
                    {
                        IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                        if (manager != IntPtr.Zero)
                        {
                            IntPtr topViewController = YgomSystem.UI.ViewControllerManager.GetStackTopViewController(manager);
                            if (topViewController != IntPtr.Zero)
                            {
                                IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                                IL2Class classInfo = assembly.GetClass("ViewController", "YgomSystem.UI");
                                IL2Object instance = classInfo.GetField("args").GetValue(topViewController);

                                string className = "UNKNOWN";
                                IntPtr viewControllerClass = Import.Object.il2cpp_object_get_class(topViewController);
                                if (viewControllerClass != IntPtr.Zero)
                                {
                                    className = Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_name(viewControllerClass));
                                }

                                if (instance != null)
                                {
                                    Console.WriteLine(className + " args: " + YgomMiniJSON.Json.Serialize(instance.ptr));
                                }
                                else
                                {
                                    Console.WriteLine(className + " args: (null)");
                                }
                            }
                        }
                    }
                    break;
                case "pvpops":// Generates enum for YgoMaster.PvpOperation
                    {
                        using (TextWriter tw = File.CreateText(Path.Combine(Program.CurrentDir, "PvpOps.txt")))
                        {
                            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                            IL2Class engineClass = assembly.GetClass("Engine", "YgomGame.Duel");
                            foreach (IL2Method method in engineClass.GetMethods().OrderBy(x => x.Name))
                            {
                                if (method.Name.StartsWith("DLL_"))
                                {
                                    tw.WriteLine(method.Name + ",");
                                }
                            }
                        }
                        Console.WriteLine("Done");
                    }
                    break;
                case "unityplayerupdate":// Updates UnityPlayer.dll function addresses based on the provided PDB
                    UnityPlayerPdb.Update();// NOTE: This can crash, if it does just copy this code into Main and run exe from MD folder
                    break;
                case "bgreload":
                    CustomBackground.ReloadBg();
                    break;
                case "solo_clear":// Clears all of solo content (used for updating secret packs using new accounts)
                    {
                        Dictionary<string, object> solo = YgomSystem.Utility.ClientWork.GetDict("$.Master.Solo");
                        if (solo == null || solo.Count == 0)
                        {
                            Console.WriteLine("No data");
                            return;
                        }

                        IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                        IL2Class apiClass = assembly.GetClass("API", "YgomSystem.Network");
                        IL2Class handleClass = assembly.GetClass("Handle", "YgomSystem.Network");
                        IL2Method isCompleted = handleClass.GetMethod("IsCompleted");
                        IL2Method Solo_info = apiClass.GetMethod("Solo_info");
                        IL2Method Solo_gate_entry = apiClass.GetMethod("Solo_gate_entry");
                        IL2Method Solo_detail = apiClass.GetMethod("Solo_detail");
                        IL2Method Solo_set_use_deck_type = apiClass.GetMethod("Solo_set_use_deck_type");
                        IL2Method Solo_start = apiClass.GetMethod("Solo_start");
                        IL2Method Duel_begin = apiClass.GetMethod("Duel_begin");
                        IL2Method Duel_end = apiClass.GetMethod("Duel_end");

                        Dictionary<string, object> allChapterData = Utils.GetDictionary(solo, "chapter");
                        if (allChapterData == null)
                        {
                            Console.WriteLine("Failed to find chapter data");
                            return;
                        }
                        Dictionary<int, Dictionary<string, object>> allChapters = new Dictionary<int, Dictionary<string, object>>();
                        Dictionary<int, List<int>> allGatesChapterIds = new Dictionary<int, List<int>>();
                        foreach (KeyValuePair<string, object> gateChapterData in allChapterData)
                        {
                            Dictionary<string, object> chapters = gateChapterData.Value as Dictionary<string, object>;
                            int gateId;
                            if (!int.TryParse(gateChapterData.Key, out gateId) || chapters == null)
                            {
                                continue;
                            }
                            List<int> chapterIds = new List<int>();
                            foreach (KeyValuePair<string, object> chapter in chapters)
                            {
                                Dictionary<string, object> chapterData = chapter.Value as Dictionary<string, object>;
                                int chapterId;
                                if (!int.TryParse(chapter.Key, out chapterId) || chapterData == null)
                                {
                                    continue;
                                }
                                allChapters[chapterId] = chapterData;
                                chapterIds.Add(chapterId);
                            }
                            allGatesChapterIds[gateId] = chapterIds;
                        }

                        Dictionary<string, object> allGateData = Utils.GetDictionary(solo, "gate");
                        if (allGateData == null)
                        {
                            Console.WriteLine("Failed to find gate data");
                            return;
                        }

                        Func<int, Dictionary<string, object>, int> GetChapterStatus = (int chapterId, Dictionary<string, object> cleared) =>
                        {
                            int gateId = chapterId / 10000;
                            Dictionary<string, object> gateCleared = Utils.GetDictionary(cleared, gateId.ToString());
                            if (gateCleared != null)
                            {
                                return Utils.GetValue<int>(gateCleared, chapterId.ToString());
                            }
                            return 0;
                        };

                        Func<int, Dictionary<string, object>, Dictionary<string, object>, bool> AreUnlockConditionsMet = (int unlockId, Dictionary<string, object> cleared, Dictionary<string, object> itemHave) =>
                        {
                            Dictionary<string, object> allUnlockData = Utils.GetDictionary(solo, "unlock");
                            Dictionary<string, object> allUnlockItemData = Utils.GetDictionary(solo, "unlock_item");
                            if (allUnlockData == null || allUnlockItemData == null)
                            {
                                Utils.LogWarning("Failed to get all unlock data");
                                return false;
                            }
                            Dictionary<string, object> unlockData = Utils.GetDictionary(allUnlockData, unlockId.ToString());
                            if (unlockData == null)
                            {
                                Utils.LogWarning("Failed to get unlock data for unlock_id " + unlockId);
                                return false;
                            }
                            bool unlockConditionsMet = true;
                            foreach (KeyValuePair<string, object> unlockRequirement in unlockData)
                            {
                                ChapterUnlockType unlockType;
                                if (Enum.TryParse(unlockRequirement.Key, out unlockType))
                                {
                                    switch (unlockType)
                                    {
                                        case ChapterUnlockType.HAS_ITEM:
                                        case ChapterUnlockType.ITEM:
                                            {
                                                List<object> itemSetList = unlockRequirement.Value as List<object>;
                                                if (itemSetList == null)
                                                {
                                                    continue;
                                                }
                                                foreach (object itemSet in itemSetList)
                                                {
                                                    int itemSetId = (int)Convert.ChangeType(itemSet, typeof(int));
                                                    Dictionary<string, object> itemsByCategory = Utils.GetDictionary(allUnlockItemData, itemSetId.ToString());
                                                    if (itemsByCategory == null)
                                                    {
                                                        Utils.LogWarning("Failed to find unlock_item " + itemSetId + " for unlock_id" + unlockId);
                                                        continue;
                                                    }
                                                    foreach (KeyValuePair<string, object> itemCategory in itemsByCategory)
                                                    {
                                                        Dictionary<string, object> items = itemCategory.Value as Dictionary<string, object>;
                                                        ItemID.Category category;
                                                        if (!Enum.TryParse(itemCategory.Key, out category))
                                                        {
                                                            continue;
                                                        }
                                                        foreach (KeyValuePair<string, object> item in items)
                                                        {
                                                            int itemId;
                                                            int count = (int)Convert.ChangeType(item.Value, typeof(int));
                                                            if (!int.TryParse(item.Key, out itemId))
                                                            {
                                                                continue;
                                                            }
                                                            if (itemHave == null || Utils.GetValue<int>(itemHave, itemId.ToString()) < count)
                                                            {
                                                                unlockConditionsMet = false;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case ChapterUnlockType.CHAPTER_OR:
                                            {
                                                List<int> chapterIds = Utils.GetIntList(unlockData, unlockRequirement.Key);
                                                if (!chapterIds.Any(x => GetChapterStatus(x, cleared) != 0))
                                                {
                                                    unlockConditionsMet = false;
                                                }
                                            }
                                            break;
                                        case ChapterUnlockType.CHAPTER_AND:
                                            {
                                                List<int> chapterIds = Utils.GetIntList(unlockData, unlockRequirement.Key);
                                                if (!chapterIds.All(x => GetChapterStatus(x, cleared) != 0))
                                                {
                                                    unlockConditionsMet = false;
                                                }
                                            }
                                            break;
                                        case ChapterUnlockType.USER_LEVEL:
                                            {
                                                // TODO
                                            }
                                            break;
                                    }
                                }
                            }
                            return unlockConditionsMet;
                        };

                        Func<int, Dictionary<string, object>, Dictionary<string, object>, bool> IsGateUnlocked = (int gateId, Dictionary<string, object> cleared, Dictionary<string, object> itemHave) =>
                        {
                            Dictionary<string, object> gateData = Utils.GetDictionary(allGateData, gateId.ToString());
                            if (gateData == null)
                            {
                                return false;
                            }
                            int unlockId = Utils.GetValue<int>(gateData, "unlock_id");
                            return unlockId == 0 || AreUnlockConditionsMet(unlockId, cleared, itemHave);
                        };

                        Func<int, Dictionary<string, object>, Dictionary<string, object>, bool> IsAvailable = (int chapterId, Dictionary<string, object> cleared, Dictionary<string, object> itemHave) =>
                        {
                            int gateId = chapterId / 10000;
                            if (!IsGateUnlocked(gateId, cleared, itemHave))
                            {
                                return false;
                            }

                            Dictionary<string, object> chapter;
                            if (!allChapters.TryGetValue(chapterId, out chapter))
                            {
                                return false;
                            }

                            int parentChapterId = Utils.GetValue<int>(chapter, "parent_chapter");
                            if (parentChapterId != 0)
                            {
                                Dictionary<string, object> parentChapter;
                                if (!allChapters.TryGetValue(parentChapterId, out parentChapter))
                                {
                                    return false;
                                }
                                if (GetChapterStatus(parentChapterId, cleared) == 0)
                                {
                                    return false;
                                }
                            }

                            int unlockId = Utils.GetValue<int>(chapter, "unlock_id");
                            if (unlockId != 0)
                            {
                                return AreUnlockConditionsMet(unlockId, cleared, itemHave);
                            }

                            return true;
                        };

                        Func<int> GetNextChapter = () =>
                        {
                            bool hasData = false;
                            Dictionary<string, object> cleared = null;
                            Dictionary<string, object> itemHave = null;
                            Win32Hooks.Invoke(() =>
                            {
                                cleared = YgomSystem.Utility.ClientWork.GetDict("Solo.cleared");
                                itemHave = YgomSystem.Utility.ClientWork.GetDict("Item.have");
                                hasData = true;
                            });
                            while (!hasData)
                            {
                                Thread.Sleep(1);
                            }
                            if (cleared != null)
                            {
                                foreach (KeyValuePair<int, Dictionary<string, object>> chapter in allChapters)
                                {
                                    if (GetChapterStatus(chapter.Key, cleared) != 3 && IsAvailable(chapter.Key, cleared, itemHave))
                                    {
                                        return chapter.Key;
                                    }
                                }
                            }
                            return 0;
                        };

                        new Thread(delegate ()
                        {
                            int lastGateId = 0;
                            while (true)
                            {
                                int chapterId = GetNextChapter();
                                if (chapterId == 0)
                                {
                                    Console.WriteLine("Done");
                                    break;
                                }

                                Dictionary<string, object> chapterData;
                                if (!allChapters.TryGetValue(chapterId, out chapterData))
                                {
                                    continue;
                                }

                                int gateId = chapterId / 10000;
                                if (gateId != lastGateId)
                                {
                                    lastGateId = gateId;
                                    MakeNetworkRequest(isCompleted, () =>
                                    {
                                        int gateIdLoc = gateId;
                                        return Solo_gate_entry.Invoke(new IntPtr[] { new IntPtr(&gateIdLoc) }).ptr;
                                    });
                                }

                                Console.WriteLine("Doing chapter " + chapterId + " for gate " + gateId);

                                Dictionary<string, object> gateData = Utils.GetDictionary(allGateData, gateId.ToString());
                                
                                bool isGoalOnly = Utils.GetValue<int>(gateData, "clear_chapter") == chapterId;
                                if ((Utils.GetValue<int>(chapterData, "mydeck_set_id") != 0 || Utils.GetValue<int>(chapterData, "set_id") != 0) &&
                                    (Utils.GetValue<int>(chapterData, "npc_id") > 0 /*&& Utils.GetValue<int>(chapterData, "difficulty") > 0*/))
                                {
                                    isGoalOnly = false;
                                }

                                bool isScenario = Utils.IsScenarioChapter(Utils.GetValue<string>(chapterData, "begin_sn"));
                                bool isLock = Utils.GetValue<int>(chapterData, "unlock_id") != 0;

                                /*MakeNetworkRequest(isCompleted, () =>
                                {
                                    int chapterIdLoc = chapterId;
                                    return Solo_detail.Invoke(new IntPtr[] { new IntPtr(&chapterIdLoc) }).ptr;
                                });*/

                                if (isScenario || isLock || isGoalOnly)
                                {
                                    MakeNetworkRequest(isCompleted, () =>
                                    {
                                        int chapterIdLoc = chapterId;
                                        return Solo_start.Invoke(new IntPtr[] { new IntPtr(&chapterIdLoc) }).ptr;
                                    });
                                }
                                else
                                {
                                    int myDeckSetId = Utils.GetValue<int>(chapterData, "mydeck_set_id");
                                    int loanerDeckSetId = Utils.GetValue<int>(chapterData, "set_id");

                                    for (int i = 0; i < 2; i++)
                                    {
                                        if ((SoloDeckType)i == SoloDeckType.POSSESSION && myDeckSetId == 0)
                                        {
                                            continue;
                                        }
                                        if ((SoloDeckType)i == SoloDeckType.STORY && loanerDeckSetId == 0)
                                        {
                                            continue;
                                        }

                                        MakeNetworkRequest(isCompleted, () =>
                                        {
                                            int chapterIdLoc = chapterId;
                                            int deckType = i;
                                            return Solo_set_use_deck_type.Invoke(new IntPtr[] { new IntPtr(&chapterIdLoc), new IntPtr(&deckType) }).ptr;
                                        });

                                        MakeNetworkRequest(isCompleted, () =>
                                        {
                                            int chapterIdLoc = chapterId;
                                            return Solo_start.Invoke(new IntPtr[] { new IntPtr(&chapterIdLoc) }).ptr;
                                        });

                                        MakeNetworkRequest(isCompleted, () =>
                                        {
                                            Dictionary<string, object> rule = new Dictionary<string, object>()
                                            {
                                                { "GameMode", 9 },
                                                { "chapter", chapterId },
                                                { "FirstPlayer", 1 }
                                            };
                                            return Duel_begin.Invoke(new IntPtr[] { YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(rule)) }).ptr;
                                        });

                                        MakeNetworkRequest(isCompleted, () =>
                                        {
                                            Dictionary<string, object> param = new Dictionary<string, object>()
                                            {
                                                { "res", (int)DuelResultType.Win },
                                                { "finish", (int)DuelFinishType.Normal }
                                            };
                                            return Duel_end.Invoke(new IntPtr[] { YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(param)) }).ptr;
                                        });
                                    }
                                }
                            }
                        }).Start();
                    }
                    break;
                case "dismantle_all_cards":// Dismantles every single card you own of the given type(s) (Normal, Rare, SuperRare, UltraRare)
                    {
                        HashSet<CardRarity> rarities = new HashSet<CardRarity>();
                        Dictionary<CardRarity, int> raritiesNum = new Dictionary<CardRarity, int>();
                        for (int i = 1; i < splitted.Length; i++)
                        {
                            CardRarity value;
                            if (Enum.TryParse(splitted[i], out value))
                            {
                                rarities.Add(value);
                            }
                        }
                        if (rarities.Count > 0)
                        {
                            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                            IL2Class apiClass = assembly.GetClass("API", "YgomSystem.Network");
                            IL2Method Craft_exchange_multi = apiClass.GetMethod("Craft_exchange_multi");

                            Dictionary<string, object> cardList = new Dictionary<string, object>();

                            Dictionary<string, object> cardRare = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Master.CardRare")) as Dictionary<string, object>;
                            if (cardRare == null)
                            {
                                Console.WriteLine("Failed to get CardRare");
                                return;
                            }

                            Dictionary<string, object> cardsHave = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Cards.have")) as Dictionary<string, object>;
                            if (cardsHave == null)
                            {
                                Console.WriteLine("Failed to get Cards.have");
                                return;
                            }

                            foreach (KeyValuePair<string, object> entry in cardsHave)
                            {
                                int cardId;
                                Dictionary<string, object> cardData = entry.Value as Dictionary<string, object>;
                                if (int.TryParse(entry.Key, out cardId) && cardData != null)
                                {
                                    CardRarity rarity = Utils.GetValue<CardRarity>(cardRare, entry.Key);
                                    if (rarities.Contains(rarity))
                                    {
                                        int n = Utils.GetValue<int>(cardData, "n");
                                        int p1n = Utils.GetValue<int>(cardData, "p1n");
                                        int p2n = Utils.GetValue<int>(cardData, "p2n");
                                        int total = n + p1n + p2n;
                                        if (total == 0)
                                        {
                                            continue;
                                        }
                                        cardList[entry.Key] = new Dictionary<string, object>()
                                        {
                                            { "num", n },
                                            { "p1_num", p1n },
                                            { "p2_num", p2n }
                                        };
                                        int grandTotal;
                                        raritiesNum.TryGetValue(rarity, out grandTotal);
                                        raritiesNum[rarity] = grandTotal + total;
                                    }
                                }
                            }

                            if (cardList.Count == 0)
                            {
                                Console.WriteLine("Nothing to dismantle");
                            }
                            else
                            {
                                foreach (KeyValuePair<CardRarity, int> rarity in raritiesNum)
                                {
                                    Console.WriteLine("Dismantle " + rarity.Value + " " + rarity.Key);
                                }
                                IL2Array<int> compenstationList = new IL2Array<int>(0);
                                Craft_exchange_multi.Invoke(new IntPtr[] { YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(cardList)), compenstationList.ptr });
                            }

                            Console.WriteLine("Done");
                        }
                        else
                        {
                            Console.WriteLine("No rarities specified");
                        }
                    }
                    break;
                case "num_secrets":// Logs the number of secret packs unlocked
                    {
                        HashSet<int> ownedSecrets = new HashSet<int>();
                        Dictionary<string, object> livePackShop = YgomSystem.Utility.ClientWork.GetDict("$.Shop.PackShop");
                        if (livePackShop == null)
                        {
                            Console.WriteLine("Failed to find live pack shop data");
                            return;
                        }
                        foreach (KeyValuePair<string, object> pack in livePackShop)
                        {
                            Dictionary<string, object> shopData = pack.Value as Dictionary<string, object>;
                            if (shopData == null)
                            {
                                continue;
                            }
                            int packType = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetCategory") ? "targetCategory" : "packType");
                            int packId = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetId") ? "targetId" : "packId");
                            if (packType == 3)
                            {
                                ownedSecrets.Add(packId);
                            }
                        }

                        HashSet<int> secretsInShopFile = new HashSet<int>();
                        string shopFile = Path.Combine(Program.DataDir, "Shop.json");
                        if (!File.Exists(shopFile))
                        {
                            Console.WriteLine("Failed to find " + shopFile);
                            return;
                        }
                        Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(shopFile)) as Dictionary<string, object>;
                        if (data == null)
                        {
                            Console.WriteLine("Failed to load " + shopFile);
                            return;
                        }
                        Dictionary<string, object> packShop = Utils.GetDictionary(data, "PackShop");
                        if (packShop == null)
                        {
                            Console.WriteLine("Couldn't find pack shop data");
                            return;
                        }
                        foreach (KeyValuePair<string, object> shop in packShop)
                        {
                            Dictionary<string, object> shopData = shop.Value as Dictionary<string, object>;
                            if (shopData == null)
                            {
                                continue;
                            }
                            int packType = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetCategory") ? "targetCategory" : "packType");
                            int packId = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetId") ? "targetId" : "packId");
                            if (packType == 3)
                            {
                                secretsInShopFile.Add(packId);
                            }
                        }
                        
                        Console.WriteLine("Secret packs: " + ownedSecrets.Count);
                        Console.WriteLine("Secrets in shop file: " + secretsInShopFile.Count);
                    }
                    break;
                case "craft_secrets":// Crafts cards to unlock all secret packs
                    {
                        string shopFile = Path.Combine(Program.DataDir, "Shop.json");
                        if (!File.Exists(shopFile))
                        {
                            Console.WriteLine("Failed to find " + shopFile);
                            return;
                        }

                        Dictionary<string, object> cardRareData = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Master.CardRare")) as Dictionary<string, object>;
                        List<object> cardCraftData = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("$.Master.CardCr")) as List<object>;
                        HashSet<int> craftableCardIds = new HashSet<int>();
                        if (cardRareData == null)
                        {
                            Console.WriteLine("Failed to get Master.CardRare");
                            return;
                        }
                        if (cardCraftData == null)
                        {
                            Console.WriteLine("Failed to get Master.CardCr");
                            return;
                        }
                        foreach (object value in cardCraftData)
                        {
                            int cardId = (int)Convert.ChangeType(value, typeof(int));
                            if (cardId != 0)
                            {
                                craftableCardIds.Add(cardId);
                            }
                        }
                        Dictionary<int, CardRarity> cardRare = new Dictionary<int, CardRarity>();
                        foreach (KeyValuePair<string, object> cardRareEntry in cardRareData)
                        {
                            int cardId;
                            if (int.TryParse(cardRareEntry.Key, out cardId))
                            {
                                cardRare[cardId] = (CardRarity)(int)Convert.ChangeType(cardRareEntry.Value, typeof(int));
                            }
                        }

                        Dictionary<int, List<int>> packs = new Dictionary<int, List<int>>();// <packId, List<cardId>>
                        Dictionary<int, List<int>> packsPerCard = new Dictionary<int, List<int>>();// <cardId, List<packId>>

                        Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(shopFile)) as Dictionary<string, object>;
                        if (data == null)
                        {
                            Console.WriteLine("Failed to load " + shopFile);
                            return;
                        }
                        Dictionary<string, object> packShop = Utils.GetDictionary(data, "PackShop");
                        if (packShop == null)
                        {
                            Console.WriteLine("Couldn't find pack shop data");
                            return;
                        }

                        HashSet<int> ownedSecrets = new HashSet<int>();
                        Dictionary<string, object> livePackShop = YgomSystem.Utility.ClientWork.GetDict("$.Shop.PackShop");
                        if (livePackShop == null)
                        {
                            Console.WriteLine("Failed to find live pack shop data");
                            return;
                        }
                        foreach (KeyValuePair<string, object> pack in livePackShop)
                        {
                            Dictionary<string, object> shopData = pack.Value as Dictionary<string, object>;
                            if (shopData == null)
                            {
                                continue;
                            }
                            int packType = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetCategory") ? "targetCategory" : "packType");
                            int packId = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetId") ? "targetId" : "packId");
                            if (packType == 3)
                            {
                                ownedSecrets.Add(packId);
                            }
                        }

                        foreach (KeyValuePair<string, object> shop in packShop)
                        {
                            Dictionary<string, object> shopData = shop.Value as Dictionary<string, object>;
                            if (shopData == null)
                            {
                                continue;
                            }
                            int packType = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetCategory") ? "targetCategory" : "packType");
                            int packId = Utils.GetValue<int>(shopData, shopData.ContainsKey("targetId") ? "targetId" : "packId");
                            object cardListObj;
                            if (packType == 3 && shopData.TryGetValue("cardList", out cardListObj))
                            {
                                if (ownedSecrets.Contains(packId))
                                {
                                    continue;
                                }
                                HashSet<int> cardIds = new HashSet<int>();
                                if (cardListObj is List<object>)
                                {
                                    List<object> cardList = cardListObj as List<object>;
                                    foreach (object card in cardList)
                                    {
                                        cardIds.Add((int)Convert.ChangeType(card, typeof(int)));
                                    }
                                }
                                else if (cardListObj is Dictionary<string, object>)
                                {
                                    Dictionary<string, object> cardList = cardListObj as Dictionary<string, object>;
                                    foreach (KeyValuePair<string, object> card in cardList)
                                    {
                                        int cardId;
                                        if (int.TryParse(card.Key, out cardId))
                                        {
                                            cardIds.Add(cardId);
                                        }
                                    }
                                }
                                packs[packId] = cardIds.ToList();
                                foreach (int cid in cardIds)
                                {
                                    CardRarity rarity;
                                    if (cardRare.TryGetValue(cid, out rarity) && craftableCardIds.Contains(cid) && rarity >= CardRarity.SuperRare)
                                    {
                                        List<int> packIds = new List<int>();
                                        if (!packsPerCard.TryGetValue(cid, out packIds))
                                        {
                                            packsPerCard[cid] = packIds = new List<int>();
                                        }
                                        packIds.Add(packId);
                                    }
                                }
                            }
                        }

                        List<int> cardsToCraft = new List<int>();
                        HashSet<int> packsObtained = new HashSet<int>();
                        int srCost = 0;
                        int urCost = 0;

                        int currentCpSR = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("$.Item.have.5");//CP-SR
                        int currentCpUR = YgomSystem.Utility.ClientWork.GetByJsonPath<int>("$.Item.have.6");//CP-UR
                        //currentCpSR = 3000;
                        //currentCpUR = 3000;

                        Action<CardRarity> removeRarity = (CardRarity rarity) =>
                        {
                            foreach (int cardId in packsPerCard.Keys.ToList())
                            {
                                CardRarity cardRarity;
                                if (cardRare.TryGetValue(cardId, out cardRarity) && cardRarity == rarity)
                                {
                                    packsPerCard.Remove(cardId);
                                }
                            }
                        };

                        while (packsObtained.Count != packs.Count)
                        {
                        start:
                            KeyValuePair<int, List<int>> entry = packsPerCard.OrderByDescending(x => x.Value.Count).FirstOrDefault();
                            if (entry.Key == 0 || entry.Value == null || entry.Value.Count == 0)
                            {
                                break;
                            }
                            List<int> packsForThisCard = entry.Value.ToList();
                            cardsToCraft.Add(entry.Key);
                            foreach (int packId in packsForThisCard)
                            {
                                packsObtained.Add(packId);
                            }

                            CardRarity rarity;
                            if (cardRare.TryGetValue(entry.Key, out rarity))
                            {
                                switch (rarity)
                                {
                                    case CardRarity.SuperRare:
                                        if (currentCpSR - (srCost + 30) <= 0)
                                        {
                                            removeRarity(rarity);
                                            goto start;
                                        }
                                        srCost += 30;
                                        break;
                                    case CardRarity.UltraRare:
                                        if (currentCpUR - (urCost + 30) <= 0)
                                        {
                                            removeRarity(rarity);
                                            goto start;
                                        }
                                        urCost += 30;
                                        break;
                                }
                            }
                            foreach (KeyValuePair<int, List<int>> card in packsPerCard)
                            {
                                card.Value.RemoveAll(x => packsForThisCard.Contains(x));
                            }
                            packsPerCard.Remove(entry.Key);
                        }

                        Console.WriteLine("Craft: " + string.Join(",", cardsToCraft));
                        Console.WriteLine("SRCost:" + srCost + " URCost:" + urCost);
                        Console.WriteLine("CurrentCpSR:" + currentCpSR + " CurrentCpUR:" + currentCpUR);
                        Console.WriteLine("Crafting " + cardsToCraft.Count + " cards to obtain " + packsObtained.Count + " / " + packs.Count + " packs");

                        if (cardsToCraft.Count == 0)
                        {
                            return;
                        }

                        if (packsObtained.Count != packs.Count)
                        {
                            Console.WriteLine("Not crafting as this wont give all the desired packs. Missing packs: " +
                                string.Join(",", packs.Keys.Except(packsObtained)));
                            return;
                        }

                        IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                        IL2Class apiClass = assembly.GetClass("API", "YgomSystem.Network");
                        IL2Method Craft_generate_multi = apiClass.GetMethod("Craft_generate_multi");

                        Dictionary<string, object> cardListData = new Dictionary<string, object>();
                        foreach (int cardId in cardsToCraft)
                        {
                            cardListData[cardId.ToString()] = new Dictionary<string, object>()
                            {                                        
                                { "num", 1 },
                                { "p1_num", 0 },
                                { "p2_num", 0 }
                            };
                        }
                        csbool check = false;
                        Craft_generate_multi.Invoke(new IntPtr[] { YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(cardListData)), new IntPtr(&check), IntPtr.Zero });
                    }
                    break;
                case "auto_free_pull":// Opens every pack with a free pull
                    {
                        IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                        IL2Class apiClass = assembly.GetClass("API", "YgomSystem.Network");
                        IL2Class handleClass = assembly.GetClass("Handle", "YgomSystem.Network");
                        IL2Method isCompleted = handleClass.GetMethod("IsCompleted");
                        IL2Method Shop_purchase = apiClass.GetMethod("Shop_purchase");

                        Dictionary<string, object> packShop = YgomSystem.Utility.ClientWork.GetDict("$.Shop.PackShop");
                        if (packShop == null)
                        {
                            Console.WriteLine("Failed to find live pack shop data");
                            return;
                        }
                        new Thread(delegate ()
                        {
                            foreach (KeyValuePair<string, object> shop in packShop)
                            {
                                Dictionary<string, object> shopData = shop.Value as Dictionary<string, object>;
                                if (shopData == null)
                                {
                                    continue;
                                }
                                Dictionary<string, object> prices = Utils.GetDictionary(shopData, "prices");
                                if (prices == null)
                                {
                                    continue;
                                }
                                foreach (KeyValuePair<string, object> price in prices)
                                {
                                    Dictionary<string, object> priceData = price.Value as Dictionary<string, object>;
                                    if (priceData == null)
                                    {
                                        continue;
                                    }
                                    int freePulls;
                                    if (Utils.TryGetValue(priceData, "free_num", out freePulls) && freePulls > 0)
                                    {
                                        MakeNetworkRequest(isCompleted, () =>
                                        {
                                            int shopId = int.Parse(shop.Key);
                                            int priceId = Utils.GetValue<int>(priceData, "price_id");
                                            int count = freePulls;
                                            Console.WriteLine("Pull shopid:" + shopId + " priceid:" + priceId + " count:" + freePulls);
                                            return Shop_purchase.Invoke(new IntPtr[]
                                            {
                                                new IntPtr(&shopId),
                                                new IntPtr(&priceId),
                                                new IntPtr(&count),
                                                IntPtr.Zero
                                            }).ptr;
                                        });
                                    }
                                }
                            }
                            Console.WriteLine("Done");
                        }).Start();
                    }
                    break;
                case "card_base_data_size":// Get size of CardBaseData
                    {
                        IL2Assembly mscorlibAssembly = Assembler.GetAssembly("mscorlib");
                        IL2Method sizeOf = mscorlibAssembly.GetClass("Marshal").GetMethod("SizeOf", x => x.GetParameters().Length == 1);

                        IL2Assembly assembly2 = Assembler.GetAssembly("Assembly-CSharp");
                        IL2Class targetStruct = assembly2.GetClass("CardBaseData", "YgomGame.Deck");

                        Console.WriteLine("expected: " + sizeof(YgomGame.Deck.CardBaseData));
                        foreach (System.Reflection.FieldInfo field in typeof(YgomGame.Deck.CardBaseData).GetFields())
                        {
                            // https://stackoverflow.com/questions/30817924/obtain-non-explicit-field-offset/56512720#56512720
                            Console.WriteLine(field.Name + "=" + (Marshal.ReadInt32(field.FieldHandle.Value + (4 + IntPtr.Size)) & 0xFFFFFF));
                        }
                        Console.WriteLine();
                        int baseOffset = 0x10;// See IL2Object.GetValueRef
                        Console.WriteLine("sizeof(CardBaseData) = " + sizeOf.Invoke(new IntPtr[] { targetStruct.IL2Typeof() }).GetValueRef<int>());
                        foreach (IL2Field field in targetStruct.GetFields())
                        {
                            Console.WriteLine(field.Name + "=" + (field.Token - baseOffset));
                        }
                    }
                    break;
            }
        }

        static void MakeNetworkRequest(IL2Method isCompleted, Func<IntPtr> networkFunc)
        {
            IntPtr handle = IntPtr.Zero;
            uint handleRef = 0;
            Win32Hooks.Invoke(() =>
            {
                handle = networkFunc();
                handleRef = Import.Handler.il2cpp_gchandle_new(handle, true);
                Console.WriteLine("Handle: " + handle);
            });
            while (handle == IntPtr.Zero)
            {
                Thread.Sleep(50);
            }
            Console.WriteLine("OK");
            bool complete = false;
            while (!complete)
            {
                Win32Hooks.Invoke(() =>
                {
                    if (!complete && isCompleted.Invoke(handle).GetValueRef<csbool>())
                    {
                        complete = true;
                        Console.WriteLine("Complete");
                    }
                });
                Thread.Sleep(50);
            }
            Win32Hooks.Invoke(() =>
            {
                Import.Handler.il2cpp_gchandle_free(handleRef);
            });
        }

        static bool IsWeirdName(string name)
        {
            name = name.Replace("\r", "").Replace("\n", "").Trim();
            uint val;
            if (name.StartsWith("(") && name.EndsWith(")") && uint.TryParse(name.Trim('(', ')'), System.Globalization.NumberStyles.HexNumber, null, out val))
            {
                //Example: 1001042,//( 8da1a40b )
                return true;
            }
            return false;
        }

        static string FixWeirdNameForEnum(string name)
        {
            if (IsWeirdName(name))
            {
                return "_" + name.Trim().Trim('(', ')').Trim();
            }
            return name;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private const UInt32 StdOutputHandle = 0xFFFFFFF5;

        private static IntPtr consoleHandle;
        private static TextWriter output;

        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        private static string title;
        public static string Title
        {
            get
            {
                return consoleHandle == IntPtr.Zero ? title : title = Console.Title;
            }
            set
            {
                title = value;
                if (consoleHandle != IntPtr.Zero)
                {
                    Console.Title = value;
                }
            }
        }

        static ConsoleHelper()
        {
            output = new ConsoleTextWriter();
            //Console.SetOut(output);

            consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                Console.Title = Title;
            }
        }

        public static bool IsConsoleVisible
        {
            get { return (consoleHandle = GetConsoleWindow()) != IntPtr.Zero && IsWindowVisible(consoleHandle); }
            //get { return (consoleHandle = GetConsoleWindow()) != IntPtr.Zero; }
        }

        public static void ToggleConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                FreeConsole();
            }
        }

        public static void ShowConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle == IntPtr.Zero)
            {
                AllocConsole();
                consoleHandle = GetConsoleWindow();
            }
            else
            {
                ShowWindow(consoleHandle, SW_SHOW);
            }

            if (consoleHandle != IntPtr.Zero)
            {
                Console.Title = title != null ? title : string.Empty;
            }
        }

        public static void HideConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                ShowWindow(consoleHandle, SW_HIDE);
            }
        }

        public static void CloseConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                FreeConsole();
            }
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        // TODO: WriteConsole may not write all the data, chunk this data into several calls if nessesary

        // WriteConsoleW issues reference:
        // https://svn.apache.org/repos/asf/logging/log4net/tags/log4net-1_2_9/src/Appender/ColoredConsoleAppender.cs

        public override void Write(string value)
        {
            uint written;
            if (!WriteConsoleW(new IntPtr(7), value, (uint)value.Length, out written, IntPtr.Zero) || written < value.Length)
            {
                if (GetConsoleWindow() != IntPtr.Zero)
                {
                    //System.Diagnostics.Debugger.Break();
                }
            }
        }

        public override void WriteLine(string value)
        {
            value = value + Environment.NewLine;
            uint written;
            if (!WriteConsoleW(new IntPtr(7), value, (uint)value.Length, out written, IntPtr.Zero) || written < value.Length)
            {
                if (GetConsoleWindow() != IntPtr.Zero)
                {
                    //System.Diagnostics.Debugger.Break();
                }
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool WriteConsoleW(IntPtr hConsoleOutput, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer,
           uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
           IntPtr lpReserved);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer,
           uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
           IntPtr lpReserved);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCP(int wCodePageID);

        [DllImport("kernel32.dll")]
        static extern uint GetACP();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
    }
}
