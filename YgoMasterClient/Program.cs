using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using IL2CPP;
using YgoMasterClient;
using System.Windows.Forms;
using System.Globalization;
using System.Security.Principal;
using System.Diagnostics;

namespace YgoMasterClient
{
    public static class Program
    {
        public static Random Rand = new Random();
        public static bool IsLive;
        public static string CurrentDir;// Path of where the current assembly is (YgoMasterClient.exe)
        public static string DataDir;// Path of misc data
        public static string ClientDataDir;// Path of the custom client content
        public static string ClientDataDumpDir;// Path to dump client content when dumping is enabled

        static void Main(string[] args)
        {
            bool success = false;
            if (!File.Exists(GameLauncher.LoaderDll))
            {
                MessageBox.Show("Couldn't find " + GameLauncher.LoaderDll);
                return;
            }
            if (!File.Exists(Path.Combine("..", "masterduel_Data", "Plugins", "x86_64", "duel.dll")))
            {
                // Invalid install location...
            }
            else if ((args.Length > 0 && args[0].ToLower() == "live") || !File.Exists("YgoMaster.exe"))
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        string path = Assembly.GetExecutingAssembly().Location;
                        if (!File.Exists(path))
                        {
                            path = "YgoMasterClient.exe";
                        }
                        Process process = new Process();
                        process.StartInfo.Arguments = "live";
                        process.StartInfo.FileName = path;
                        process.StartInfo.Verb = "runas";
                        process.Start();
                        return;
                    }
                }
                success = GameLauncher.Launch(GameLauncherMode.Inject);
            }
            else
            {
                Process[] processes = Process.GetProcessesByName("YgoMaster");
                try
                {
                    if (processes.Length == 0)
                    {
                        string serverExe = Path.Combine(Environment.CurrentDirectory, "YgoMaster.exe");
                        if (File.Exists(serverExe))
                        {
                            Process.Start(serverExe);
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    foreach (Process process in processes)
                    {
                        process.Close();
                    }
                }
                success = GameLauncher.Launch(GameLauncherMode.Detours);
            }
            if (!success)
            {
                MessageBox.Show("Failed. Make sure the YgoMaster folder is inside game folder.\n\nThis should roughly be (depending on your steam install):\n\n " +
                    "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Yu-Gi-Oh! Master Duel\\YgoMaster\\\n\nThe working directory is:\n\n" +
                     Environment.CurrentDirectory, "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        public static int DllMain(string arg)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                IsLive = arg == "live";

                CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                DataDir = YgoMaster.Utils.GetDataDirectory(false, CurrentDir);
                ClientDataDir = Path.Combine(DataDir, "ClientData");
                ClientDataDumpDir = Path.Combine(DataDir, "ClientDataDump");
                YgoMaster.ItemID.Load(DataDir);
                YgoMaster.YdkHelper.LoadIdMap(DataDir);

                if (!ClientSettings.Load())
                {
                    throw new Exception("Failed to load '" + ClientSettings.FilePath + "'");
                }
                if (string.IsNullOrEmpty(ClientSettings.ServerUrl) ||
                    string.IsNullOrEmpty(ClientSettings.ServerPollUrl))
                {
                    throw new Exception("Failed to get server url settings");
                }
                if (ClientSettings.ShowConsole)
                {
                    ConsoleHelper.ShowConsole();
                }

                PInvoke.WL_InitHooks();
                PInvoke.InitGameModuleBaseAddress();

                // All types with hooks must be initialized. It's also a good idea to initialize other types as there's an exception handler here
                // NOTE: As more things are added here the load time will increase (the reflection code does a lot of linear lookups)
                List<Type> nativeTypes = new List<Type>();
                // DuelClientUtils
                nativeTypes.Add(typeof(UnityEngine.QualitySettings));
                nativeTypes.Add(typeof(YgomGame.Duel.ReplayControl));
                nativeTypes.Add(typeof(YgomGame.Duel.DuelClient));
                nativeTypes.Add(typeof(YgomGame.Duel.CameraShaker));
                nativeTypes.Add(typeof(YgomGame.Duel.Engine));
                nativeTypes.Add(typeof(YgomGame.Duel.EngineApiUtil));
                nativeTypes.Add(typeof(YgomGame.Duel.GenericCardListController));
                nativeTypes.Add(typeof(YgomGame.Duel.CardIndividualSetting));
                nativeTypes.Add(typeof(YgomGame.Duel.CardRunEffectSetting));
                //nativeTypes.Add(typeof(YgomGame.Duel.RunEffectWorker));
                // DuelStarter
                nativeTypes.Add(typeof(YgomGame.Menu.ContentViewControllerManager));
                nativeTypes.Add(typeof(YgomGame.Menu.BaseMenuViewController));
                nativeTypes.Add(typeof(YgomSystem.UI.ViewControllerManager));
                nativeTypes.Add(typeof(YgomGame.Room.RoomCreateViewController));
                nativeTypes.Add(typeof(YgomGame.DeckBrowser.DeckBrowserViewController));
                nativeTypes.Add(typeof(YgomSystem.UI.BindingTextMeshProUGUI));
                nativeTypes.Add(typeof(YgomSystem.UI.InfinityScroll.InfinityScrollView));
                nativeTypes.Add(typeof(YgomGame.Solo.SoloSelectChapterViewController));
                nativeTypes.Add(typeof(YgomGame.Solo.SoloStartProductionViewController));
                nativeTypes.Add(typeof(YgomGame.Duel.EngineInitializerByServer));
                nativeTypes.Add(typeof(YgomGame.Duel.EngineInitializer));
                nativeTypes.Add(typeof(YgomSystem.Network.API));
                nativeTypes.Add(typeof(YgomSystem.Network.Request));
                nativeTypes.Add(typeof(YgomSystem.Network.RequestStructure));
                nativeTypes.Add(typeof(DuellDll));
                // DeckEditorUtils
                nativeTypes.Add(typeof(TMPro.TMP_Text));
                nativeTypes.Add(typeof(YgomGame.Deck.DeckView));
                nativeTypes.Add(typeof(YgomGame.Deck.CardCollectionView));
                nativeTypes.Add(typeof(YgomGame.DeckEditViewController2));
                nativeTypes.Add(typeof(YgomGame.SubMenu.DeckEditSubMenuViewController));
                nativeTypes.Add(typeof(YgomGame.SubMenu.SubMenuViewController));
                nativeTypes.Add(typeof(YgomGame.Menu.CommonDialogViewController));
                // Misc
                nativeTypes.Add(typeof(Win32Hooks));
                nativeTypes.Add(typeof(AssetHelper));
                nativeTypes.Add(typeof(YgomGame.Utility.ItemUtil));
                nativeTypes.Add(typeof(YgomSystem.Utility.TextData));
                nativeTypes.Add(typeof(YgomSystem.Utility.ClientWork));
                nativeTypes.Add(typeof(YgomSystem.Utility.ClientWorkUtil));
                nativeTypes.Add(typeof(YgomSystem.Network.ProtocolHttp));
                nativeTypes.Add(typeof(YgomSystem.LocalFileSystem.WindowsStorageIO));
                nativeTypes.Add(typeof(YgomSystem.LocalFileSystem.StandardStorageIO));
                nativeTypes.Add(typeof(YgomMiniJSON.Json));
                nativeTypes.Add(typeof(Steamworks.SteamAPI));
                nativeTypes.Add(typeof(Steamworks.SteamUtils));
                // Uncomment the following after an update (extra things which are normally loaded on demand)
                /*nativeTypes.Add(typeof(UnityEngine.UnityObject));
                nativeTypes.Add(typeof(UnityEngine.GameObject));
                nativeTypes.Add(typeof(UnityEngine.Transform));
                nativeTypes.Add(typeof(UnityEngine.Component));*/
                // Uncomment the following for easier logging of solo content
                foreach (Type type in nativeTypes)
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
                PInvoke.WL_EnableAllHooks(true);

                if (ClientSettings.DisableVSync)
                {
                    UnityEngine.QualitySettings.CreateVSyncHook();
                }
                PInvoke.SetTimeMultiplier(ClientSettings.TimeMultiplier);

                Win32Hooks.Invoke(delegate
                {
                    // Only needed on "live" as the incoming thread wont be the main thread
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                    // Modify the window title when running as "live" to indicate the game is modded
                    if (ClientSettings.ChangeWindowTitleOnLiveMod && IsLive)
                    {
                        Process process = Process.GetCurrentProcess();
                        IntPtr windowHandle = process.MainWindowHandle;
                        string windowTitle = process.MainWindowTitle;
                        const string windowTitleSuffix = " - (modded)";
                        if (windowHandle != IntPtr.Zero && !string.IsNullOrEmpty(windowTitle) && !windowTitle.Contains(windowTitleSuffix))
                        {
                            PInvoke.SetWindowTextW(windowHandle, windowTitle.TrimEnd() + windowTitleSuffix);
                        }
                    }
                });

                if (ClientSettings.ShowConsole)
                {
                    new Thread(delegate()
                        {
                            while (true)
                            {
                                string consoleInput = Console.ReadLine();
                                Win32Hooks.Invoke(delegate
                                {
                                    try
                                    {
                                        string[] splitted = consoleInput.Split();
                                        switch (splitted[0].ToLower())
                                        {
                                            case "itemid":// Creates json for values in IDS_ITEM (all item ids)
                                                {
                                                    Dictionary<YgomGame.Utility.ItemUtil.Category, List<string>> categories = new Dictionary<YgomGame.Utility.ItemUtil.Category, List<string>>();
                                                    IL2Class classInfo = classInfo = Assembler.GetAssembly("Assembly-CSharp").GetClass("IDS_ITEM", "YgomGame.TextIDs");
                                                    IL2Field[] fields = classInfo.GetFields();
                                                    foreach (IL2Field field in fields)
                                                    {
                                                        string fieldName = field.Name;
                                                        int id;
                                                        if (fieldName.StartsWith("ID") && int.TryParse(fieldName.Substring(2), out id))
                                                        {
                                                            string name = YgomGame.Utility.ItemUtil.GetItemName(id);
                                                            YgomGame.Utility.ItemUtil.Category cat = YgomGame.Utility.ItemUtil.GetCategoryFromID(id);
                                                            if (!categories.ContainsKey(cat))
                                                            {
                                                                categories[cat] = new List<string>();
                                                            }
                                                            bool invalid = string.IsNullOrEmpty(name) || name == "deleted" || name == "coming soon" || name.StartsWith("ICON_FRAME");
                                                            string prefix = "    " + (invalid ? "//" : "");
                                                            categories[cat].Add(prefix + id + ",//" + name);
                                                        }
                                                    }
                                                    StringBuilder res = new StringBuilder();
                                                    res.AppendLine("{");
                                                    foreach (KeyValuePair<YgomGame.Utility.ItemUtil.Category, List<string>> cat in categories.OrderBy(x => x.Key))
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
                                            case "itemid-enum":// Creates enums for values in IDS_ITEM (all item ids)
                                                {
                                                    Dictionary<YgomGame.Utility.ItemUtil.Category, List<string>> categories = new Dictionary<YgomGame.Utility.ItemUtil.Category, List<string>>();
                                                    IL2Class classInfo = classInfo = Assembler.GetAssembly("Assembly-CSharp").GetClass("IDS_ITEM", "YgomGame.TextIDs");
                                                    IL2Field[] fields = classInfo.GetFields();
                                                    foreach (IL2Field field in fields)
                                                    {
                                                        string fieldName = field.Name;
                                                        int id;
                                                        if (fieldName.StartsWith("ID") && int.TryParse(fieldName.Substring(2), out id))
                                                        {
                                                            string name = YgomGame.Utility.ItemUtil.GetItemName(id);
                                                            YgomGame.Utility.ItemUtil.Category cat = YgomGame.Utility.ItemUtil.GetCategoryFromID(id);
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
                                                    foreach (KeyValuePair<YgomGame.Utility.ItemUtil.Category, List<string>> cat in categories)
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
                                                                string targetPath = Path.Combine(ClientDataDumpDir, "IDS", classInfo.Name + ".txt");
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
                                                            Process.Start("explorer.exe", "/select, \"" + fullPath +"\"");
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
                                                    string intIdPath = assembly.GetClass("Content", "YgomGame.Card").GetField("IntIdPath").GetValue().GetValueObj<string>();
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
                                                    allEnums["ServerStatus"] = assembly.GetClass("ServerStatus", "YgomSystem.Network");
                                                    allEnums["GameMode"] = assembly.GetClass("Util", "YgomGame.Duel").GetNestedType("GameMode");
                                                    allEnums["ChapterStatus"] = assembly.GetClass("SoloModeUtil", "YgomGame.Solo").GetNestedType("ChapterStatus");
                                                    allEnums["ChapterUnlockType"] = assembly.GetClass("SoloModeUtil", "YgomGame.Solo").GetNestedType("UnlockType");
                                                    allEnums["SoloDeckType"] = assembly.GetClass("SoloModeUtil", "YgomGame.Solo").GetNestedType("DeckType");
                                                    allEnums["HowToObtainCard"] = null;
                                                    allEnums["DuelResultScore"] = null;
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
                                                        tw.WriteLine("// Client version " + assembly.GetClass("Version", "YgomSystem.Utility").GetField("APP_COMMON_VERSION").GetValue().GetValueObj<string>());
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
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });
                            }
                        }).Start();
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                Environment.Exit(0);
                return 1;
            }
            return 0;
        }
    }
}

unsafe static class Win32Hooks
{
    delegate int Del_PeekMessageA(IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
    static Hook<Del_PeekMessageA> hookPeekMessageA;

    static List<Action> actions = new List<Action>();

    static Win32Hooks()
    {
        hookPeekMessageA = new Hook<Del_PeekMessageA>(PeekMessageA, PInvoke.GetProcAddress(PInvoke.GetModuleHandle("user32.dll"), "PeekMessageA"));
    }

    static int PeekMessageA(IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
    {
        lock (actions)
        {
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                actions[i]();
                actions.RemoveAt(i);
            }
        }
        return hookPeekMessageA.Original(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
    }

    public static void Invoke(Action action)
    {
        lock (actions)
        {
            actions.Add(action);
        }
    }
}

unsafe static class DuellDll
{
    delegate int Del_DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf);
    static Hook<Del_DLL_DuelSysInitCustom> hookDLL_DuelSysInitCustom;

    public delegate void Del_DLL_DuelComCheatCard(int player, int position, int index, int cardId, int face, int turn);
    public static Del_DLL_DuelComCheatCard DLL_DuelComCheatCard;

    public delegate void Del_DLL_DuelComDoDebugCommand(int player, int position, int index, int commandId);
    public static Del_DLL_DuelComDoDebugCommand DLL_DuelComDoDebugCommand;

    public delegate void Del_DLL_DuelComDebugCommand();
    public static Del_DLL_DuelComDebugCommand DLL_DuelComDebugCommand;

    public delegate void Del_DLL_DuelComDoCommand(int player, int position, int index, int commandId);
    public static Del_DLL_DuelComDoCommand DLL_DuelComDoCommand;

    public delegate int Del_DLL_DuelSysAct();
    public static Del_DLL_DuelSysAct DLL_DuelSysAct;

    static DuellDll()
    {
        IntPtr lib = PInvoke.LoadLibrary(Path.Combine("masterduel_Data", "Plugins", "x86_64", "duel.dll"));
        if (lib == IntPtr.Zero)
        {
            throw new Exception("Failed to load duel.dll");
        }
        hookDLL_DuelSysInitCustom = new Hook<Del_DLL_DuelSysInitCustom>(DLL_DuelSysInitCustom, PInvoke.GetProcAddress(lib, "DLL_DuelSysInitCustom"));
        DLL_DuelComCheatCard = GetFunc<Del_DLL_DuelComCheatCard>(PInvoke.GetProcAddress(lib, "DLL_DuelComCheatCard"));
        DLL_DuelComDoDebugCommand = GetFunc<Del_DLL_DuelComDoDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDoDebugCommand"));
        DLL_DuelComDebugCommand = GetFunc<Del_DLL_DuelComDebugCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDebugCommand"));
        DLL_DuelComDoCommand = GetFunc<Del_DLL_DuelComDoCommand>(PInvoke.GetProcAddress(lib, "DLL_DuelComDoCommand"));
        DLL_DuelSysAct = GetFunc<Del_DLL_DuelSysAct>(PInvoke.GetProcAddress(lib, "DLL_DuelSysAct"));
    }

    static int DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf)
    {
        //hand0 = 7;
        //hand1 = 0;
        //life0 = 9000000;
        //life1 = 1;
        return hookDLL_DuelSysInitCustom.Original(fDuelType, tag, life0, life1, hand0, hand1, shuf);
    }

    static T GetFunc<T>(IntPtr ptr)
    {
        return (T)(object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
    }
}

namespace YgomGame.Utility
{
    unsafe static class ItemUtil
    {
        static IL2Class classInfo;
        static IL2Method methodGetItemName;
        static IL2Method methodGetItemDesc;
        static IL2Method methodGetCategoryFromID;

        static ItemUtil()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ItemUtil", "YgomGame.Utility");
            methodGetItemName = classInfo.GetMethod("GetItemName", x => x.GetParameters().Length == 2);
            methodGetItemDesc = classInfo.GetMethod("GetItemDesc", x => x.GetParameters().Length == 2);
            methodGetCategoryFromID = classInfo.GetMethod("GetCategoryFromID");
        }

        public static string GetItemName(int itemID)
        {
            IntPtr textGroupLoadHolder = IntPtr.Zero;
            return methodGetItemName.Invoke(new IntPtr[] { new IntPtr(&itemID), textGroupLoadHolder }).GetValueObj<string>();
        }

        public static string GetItemDesc(int itemID)
        {
            bool useMobileSfx = false;
            return methodGetItemDesc.Invoke(new IntPtr[] { new IntPtr(&itemID), new IntPtr(&useMobileSfx) }).GetValueObj<string>();
        }

        public static Category GetCategoryFromID(int itemID)
        {
            return (Category)methodGetCategoryFromID.Invoke(new IntPtr[] { new IntPtr(&itemID) }).GetValueRef<int>();
        }

        /// <summary>
        /// YgomGame.Utility.ItemUtil.Category
        /// </summary>
        public enum Category
        {
            NONE,
            CONSUME,
            CARD,
            AVATAR,
            ICON,
            PROFILE_TAG,
            ICON_FRAME,
            PROTECTOR,
            DECK_CASE,
            FIELD,
            FIELD_OBJ,
            AVATAR_HOME,
            STRUCTURE,
            WALLPAPER,
            PACK_TICKET,
            DECK_LIMIT
        }
    }
}

namespace YgomSystem.Utility
{
    unsafe static class TextData
    {
        public const string HackID = "IDS_SYS.IDHACK:";

        static IL2Class classInfo;
        static IL2Method methodLoad;
        static IL2Method methodGetTextString;
        static IL2Method methodGetTextEnum;

        // System.Object.ToString
        static IL2Method methodObjectToString;

        delegate IntPtr Del_GetTextString(IntPtr textEnum, bool richTextEx);
        delegate IntPtr Del_GetTextEnum(int textEnum, bool richTextEx, IntPtr methodInstance);
        static Hook<Del_GetTextString> hookGetTextString;
        static Hook<Del_GetTextEnum> hookGetTextEnum;

        public static Dictionary<string, string> CustomTextData = new Dictionary<string, string>();

        static TextData()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("TextData", "YgomSystem.Utility");
            methodLoad = classInfo.GetMethod("Load", x => x.GetParameters().Length == 1);
            methodGetTextString = classInfo.GetMethod("GetText").MakeGenericMethod(new Type[] { typeof(string) });
            hookGetTextString = new Hook<Del_GetTextString>(GetTextString, methodGetTextString);
            methodGetTextEnum = classInfo.GetMethod("GetText").MakeGenericMethod(new IntPtr[] { CastUtils.IL2Typeof("Int32Enum", "System", "mscorlib") });
            hookGetTextEnum = new Hook<Del_GetTextEnum>(GetTextEnum, methodGetTextEnum);

            // System.Object.ToString
            methodObjectToString = Assembler.GetAssembly("mscorlib").GetClass("Object", "System").GetMethod("ToString");

            LoadCustomTextData();
        }

        public static void LoadCustomTextData()
        {
            CustomTextData.Clear();
            string targetDir = Path.Combine(Program.ClientDataDir, "IDS");
            if (Directory.Exists(targetDir))
            {
                string idName = null;
                StringBuilder idValue = new StringBuilder();
                foreach (string file in Directory.GetFiles(targetDir))
                {
                    idName = null;
                    idValue.Length = 0;
                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("[IDS_"))
                        {
                            if (!string.IsNullOrEmpty(idName))
                            {
                                CustomTextData[idName] = idValue.ToString();
                            }
                            idName = null;
                            idValue.Length = 0;
                            if (!lines[i].Trim().EndsWith("!"))
                            {
                                idName = lines[i].Trim('[', ']', ' ');
                            }
                        }
                        else
                        {
                            idValue.Append((idValue.Length > 0 ? "\n" : string.Empty) + lines[i]);
                            if (i == lines.Length - 1 && !string.IsNullOrEmpty(idName))
                            {
                                CustomTextData[idName] = idValue.ToString();
                            }
                        }
                    }
                }
            }
        }

        public static void LoadGroup(string groupid)
        {
            methodLoad.Invoke(new IntPtr[] { new IL2String(groupid).ptr });
        }

        public static string GetText(string id)
        {
            return new IL2String(GetTextString(new IL2String(id).ptr, false)).ToString();
        }

        static IntPtr GetTextString(IntPtr textString, bool richTextEx)
        {
            string inputString = new IL2Object(textString).GetValueObj<string>();
            if (ClientSettings.LogIDs)
            {
                Console.WriteLine(inputString);
            }
            if (!string.IsNullOrEmpty(inputString) && inputString.StartsWith(HackID))
            {
                return new IL2String(inputString.Substring(HackID.Length)).ptr;
            }
            string customText;
            if (CustomTextData.TryGetValue(inputString, out customText))
            {
                return new IL2String(customText).ptr;
            }
            return hookGetTextString.Original(textString, richTextEx);
        }

        static IntPtr GetTextEnum(int textEnum, bool richTextEx, IntPtr methodInstance)
        {
            if (methodInstance != IntPtr.Zero)
            {
                IL2Method method = new IL2Method(methodInstance);
                IL2ClassType enumType = method.GetParameters()[0].Type;
                IntPtr enumKlass = Import.Class.il2cpp_class_from_type(enumType.ptr);
                if (enumKlass != IntPtr.Zero)
                {
                    string enumTypeName = Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_name(enumKlass));
                    if (!string.IsNullOrEmpty(enumTypeName))
                    {
                        IntPtr boxed = Import.Object.il2cpp_value_box(enumKlass, new IntPtr(&textEnum));
                        IL2Object strObj = methodObjectToString.Invoke(boxed, isVirtual: true);
                        if (strObj != null)
                        {
                            string enumValueStr = strObj.GetValueObj<string>();
                            string fullString = enumTypeName + "." + enumValueStr;
                            if (ClientSettings.LogIDs)
                            {
                                Console.WriteLine(fullString);
                            }
                            string customText;
                            if (CustomTextData.TryGetValue(fullString, out customText))
                            {
                                return new IL2String(customText).ptr;
                            }
                        }
                    }
                }
            }
            return hookGetTextEnum.Original(textEnum, richTextEx, methodInstance);
        }
    }

    unsafe static class ClientWork
    {
        static IL2Class classInfo;
        static IL2Method methodDeleteByJsonPath;
        static IL2Method methodUpdateJsonRaw;
        static IL2Method methodUpdateJson;
        static IL2Method methodUpdateValue;
        static IL2Method methodGetByJsonPath;
        static IL2Method methodGetStringByJsonPath;

        static ClientWork()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ClientWork", "YgomSystem.Utility");
            methodDeleteByJsonPath = classInfo.GetMethod("deleteByJsonPath");
            methodUpdateJsonRaw = classInfo.GetMethod("updateJson", x => x.GetParameters().Length == 1);
            methodUpdateJson = classInfo.GetMethod("updateJson", x => x.GetParameters().Length == 2);
            methodUpdateValue = classInfo.GetMethod("updateValue", x => x.GetParameters().Length == 3);
            methodGetByJsonPath = classInfo.GetMethod("getByJsonPath", x => x.GetParameters().Length == 1);
            methodGetStringByJsonPath = classInfo.GetMethod("getStringByJsonPath", x => x.GetParameters().Length == 2);
        }

        public static void DeleteByJsonPath(string jsonPath, bool keep = false)
        {
            methodDeleteByJsonPath.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, new IntPtr(&keep) });
        }

        public static void UpdateJson(string jsonString)
        {
            methodUpdateJsonRaw.Invoke(new IntPtr[] { new IL2String(jsonString).ptr });
        }

        public static void UpdateJson(string jsonPath, string jsonString)
        {
            methodUpdateJson.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, new IL2String(jsonString).ptr });
        }

        public static void UpdateValue(string jsonPath, string value, bool keep = false)
        {
            methodUpdateValue.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, new IL2String(value).ptr, new IntPtr(&keep) });
        }

        public static string GetStringByJsonPath(string jsonPath, string defaultValue = "")
        {
            return methodGetStringByJsonPath.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, new IL2String(defaultValue).ptr }).GetValueObj<string>();
        }

        public static IntPtr GetByJsonPath(string jsonPath)
        {
            IL2Object obj = methodGetByJsonPath.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr });
            return obj != null ? obj.ptr : IntPtr.Zero;
        }

        // Custom func to take a json path and get a re-serialized string back
        public static string SerializePath(string jsonPath)
        {
            IL2Object obj = methodGetByJsonPath.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr });
            if (obj == null)
            {
                return null;
            }
            return YgomMiniJSON.Json.Serialize(obj.ptr);
        }
        public static Dictionary<string, object> GetDict(string jsonPath)
        {
            return MiniJSON.Json.Deserialize(SerializePath(jsonPath)) as Dictionary<string, object>;
        }
    }

    unsafe static class ClientWorkUtil
    {
        delegate IntPtr Del_GetToken();
        static Hook<Del_GetToken> hookGetToken;

        static ClientWorkUtil()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ClientWorkUtil", "YgomSystem.Utility");
            hookGetToken = new Hook<Del_GetToken>(GetToken, classInfo.GetMethod("GetToken"));
        }

        static IntPtr GetToken()
        {
            if (!string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
            {
                return new IL2String(Convert.ToBase64String(Encoding.UTF8.GetBytes(ClientSettings.MultiplayerToken))).ptr;
            }
            return hookGetToken.Original();
        }
    }
}

namespace YgomSystem.Network
{
    unsafe static class ProtocolHttp
    {
        static IL2Class classInfo;
        static IL2Method methodGetServerDefaultUrl;

        delegate void Del_GetServerDefaultUrl(int type, out IntPtr url, out IntPtr pollingUrl);
        static Hook<Del_GetServerDefaultUrl> hookGetServerDefaultUrl;

        static ProtocolHttp()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ProtocolHttp", "YgomSystem.Network");
            methodGetServerDefaultUrl = classInfo.GetMethod("GetServerDefaultUrl");

            hookGetServerDefaultUrl = new Hook<Del_GetServerDefaultUrl>(GetServerDefaultUrl, methodGetServerDefaultUrl);
        }

        static void GetServerDefaultUrl(int type, out IntPtr url, out IntPtr pollingUrl)
        {
            // This doesn't work for some reason...
            url = new IL2String(ClientSettings.ServerUrl).ptr;
            pollingUrl = new IL2String(ClientSettings.ServerPollUrl).ptr;
            // But this does...
            YgomSystem.Utility.ClientWork.UpdateJson("$.Server.urls", "{\"System.info\":\"" + ClientSettings.ServerUrl + "\"}");
            YgomSystem.Utility.ClientWork.UpdateValue("$.Server.url", ClientSettings.ServerUrl);
            YgomSystem.Utility.ClientWork.UpdateValue("$.Server.url_polling", ClientSettings.ServerUrl);
        }
    }
}

namespace YgomSystem.LocalFileSystem
{
    class WindowsStorageIO
    {
        static IL2Class classInfo;
        static IL2Method methodGetSteamUserDirectoryName;

        delegate IntPtr Del_GetSteamUserDirectoryName(IntPtr thisPtr);
        static Hook<Del_GetSteamUserDirectoryName> hookGetSteamUserDirectoryName;

        static WindowsStorageIO()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("WindowsStorageIO", "YgomSystem.LocalFileSystem");
            methodGetSteamUserDirectoryName = classInfo.GetMethod("GetSteamUserDirectoryName");
            hookGetSteamUserDirectoryName = new Hook<Del_GetSteamUserDirectoryName>(GetSteamUserDirectoryName, methodGetSteamUserDirectoryName);
        }

        static IntPtr GetSteamUserDirectoryName(IntPtr thisPtr)
        {
            IntPtr result = hookGetSteamUserDirectoryName.Original(thisPtr);
            Console.WriteLine("TODO: GetSteamUserDirectoryName (" + new IL2String(result).ToString() + ")");
            return result;
        }
    }

    class StandardStorageIO
    {
        static IL2Class classInfo;
        static IL2Method methodSetupStorageDirectory;

        delegate void Del_SetupStorageDirectory(IntPtr thisPtr, IntPtr storage, IntPtr mountPath);
        static Hook<Del_SetupStorageDirectory> hookSetupStorageDirectory;

        public static string LocalDataDir;

        static StandardStorageIO()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("StandardStorageIO", "YgomSystem.LocalFileSystem");
            methodSetupStorageDirectory = classInfo.GetMethod("setupStorageDirectory");
            hookSetupStorageDirectory = new Hook<Del_SetupStorageDirectory>(SetupStorageDirectory, methodSetupStorageDirectory);
        }

        static void SetupStorageDirectory(IntPtr thisPtr, IntPtr storage, IntPtr mountPath)
        {
            string path = new IL2String(mountPath).ToString();
            //Console.WriteLine("Mount: " + path);
            if (!string.IsNullOrEmpty(path))
            {
                // NOTE: Allowing "/../LocalSave/" to point to "00000000"
                string localDataPath = "/../LocalData/";
                int localDataPathIndex = path.IndexOf(localDataPath);
                if (localDataPathIndex >= 0)
                {
                    string folderName = path.Substring(localDataPathIndex + localDataPath.Length);
                    path = path.Substring(0, localDataPathIndex + localDataPath.Length);
                    if (folderName == "00000000")
                    {
                        Dictionary<string, DateTime> possibleFolders = new Dictionary<string, DateTime>();
                        Dictionary<string, DateTime> possibleFoldersExactMatch = new Dictionary<string, DateTime>();
                        foreach (string dir in Directory.GetDirectories(path))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(dir);
                            if (dirInfo.Name != folderName)
                            {
                                // Exodia the Forbidden One (card image) - "Card/Images/Illust/tcg/4027"
                                string findFile = Path.Combine(dirInfo.FullName, "0000", "f5", "f5e2cfa8");
                                if (File.Exists(findFile))
                                {
                                    possibleFoldersExactMatch[dirInfo.Name] = dirInfo.LastWriteTime;
                                }
                                else
                                {
                                    possibleFolders[dirInfo.Name] = dirInfo.LastWriteTime;
                                }
                            }
                        }
                        if (possibleFoldersExactMatch.Count > 0)
                        {
                            folderName = possibleFoldersExactMatch.OrderByDescending(x => x.Value).First().Key;
                        }
                        else if (possibleFolders.Count > 0)
                        {
                            folderName = possibleFolders.OrderByDescending(x => x.Value).First().Key;
                        }
                    }
                    path += folderName;
                    LocalDataDir = path;
                }
                hookSetupStorageDirectory.Original(thisPtr, storage, new IL2String(path).ptr);
            }
            else
            {
                hookSetupStorageDirectory.Original(thisPtr, storage, mountPath);
            }
        }
    }
}

namespace YgomMiniJSON
{
    static class Json
    {
        static IL2Class classInfo;
        static IL2Method methodDeserialize;
        static IL2Method methodSerialize;

        static Json()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp-firstpass");
            classInfo = assembly.GetClass("Json", "MiniJSON");
            methodDeserialize = classInfo.GetMethod("Deserialize");
            methodSerialize = classInfo.GetMethod("Serialize");
        }

        public static IntPtr Deserialize(string json)
        {
            IL2Object result = methodDeserialize.Invoke(new IntPtr[] { new IL2String(json).ptr });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static string Serialize(IntPtr ptr)
        {
            return methodSerialize.Invoke(new IntPtr[] { ptr }).GetValueObj<string>();
        }
    }
}

namespace Steamworks
{
    static class SteamAPI
    {
        static IL2Class classInfo;
        static IL2Method methodInit;
        static IL2Method methodShutdown;
        static IL2Method methodRestartAppIfNecessary;
        static IL2Method methodRunCallbacks;

        delegate csbool Del_Init();
        delegate void Del_Shutdown();
        delegate csbool Del_RestartAppIfNecessary(IntPtr unOwnAppID);
        delegate void Del_RunCallbacks();

        static Hook<Del_Init> hookInit;
        static Hook<Del_Shutdown> hookShutdown;
        static Hook<Del_RestartAppIfNecessary> hookRestartAppIfNecessary;
        static Hook<Del_RunCallbacks> hookRunCallbacks;

        static SteamAPI()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("com.rlabrecque.steamworks.net");
            classInfo = assembly.GetClass("SteamAPI", "Steamworks");
            methodInit = classInfo.GetMethod("Init");
            methodShutdown = classInfo.GetMethod("Shutdown");
            methodRestartAppIfNecessary = classInfo.GetMethod("RestartAppIfNecessary");
            methodRunCallbacks = classInfo.GetMethod("RunCallbacks");

            hookInit = new Hook<Del_Init>(InitH, methodInit);
            hookShutdown = new Hook<Del_Shutdown>(Shutdown, methodShutdown);
            hookRestartAppIfNecessary = new Hook<Del_RestartAppIfNecessary>(RestartAppIfNecessary, methodRestartAppIfNecessary);
            hookRunCallbacks = new Hook<Del_RunCallbacks>(RunCallbacks, methodRunCallbacks);
        }

        static csbool InitH()
        {
            return false;
        }

        static void Shutdown()
        {
        }

        static csbool RestartAppIfNecessary(IntPtr unOwnAppId)
        {
            return false;
        }

        static void RunCallbacks()
        {
        }
    }

    static class SteamUtils
    {
        static IL2Class classInfo;
        static IL2Method methodIsOverlayEnabled;

        delegate bool Del_IsOverlayEnabled();
        static Hook<Del_IsOverlayEnabled> hookIsOverlayEnabled;

        static SteamUtils()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("com.rlabrecque.steamworks.net");
            classInfo = assembly.GetClass("SteamUtils", "Steamworks");
            methodIsOverlayEnabled = classInfo.GetMethod("IsOverlayEnabled");

            hookIsOverlayEnabled = new Hook<Del_IsOverlayEnabled>(IsOverlayEnabled, methodIsOverlayEnabled);
        }

        static bool IsOverlayEnabled()
        {
            return false;
        }
    }
}