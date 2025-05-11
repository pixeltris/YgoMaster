using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Security.Principal;
using System.Diagnostics;
using IL2CPP;
using YgoMasterClient;
using YgoMaster;
using YgoMaster.Net;
using YgoMaster.Net.Message;

namespace YgoMasterClient
{
    static class Program
    {
        public static bool IsLive;
        public static string CurrentDir;// Path of where the current assembly is (YgoMasterClient.exe)
        public static string DataDir;// Path of misc data
        public static string LocalPlayerSaveDataDir;// Path of where local player save data
        public static string ClientDataDir;// Path of the custom client content
        public static string ClientDataDumpDir;// Path to dump client content when dumping is enabled
        public static NetClient NetClient;
        public static bool IsMonoRun;

        static void Main(string[] args)
        {
            bool isMultiplayerClient = false;
            try
            {
                CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                DataDir = Utils.GetDataDirectory(false, CurrentDir);
                ClientDataDir = Path.Combine(DataDir, "ClientData");
                ClientSettings.Load();
                if (!string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
                {
                    isMultiplayerClient = true;
                }
            }
            catch
            {
            }

            bool success = false;
            if (!File.Exists(GameLauncher.LoaderDll))
            {
                ShowMessageBox("Couldn't find " + GameLauncher.LoaderDll);
                return;
            }
            if (!File.Exists(Path.Combine("..", "masterduel_Data", "Plugins", "x86_64", "duel.dll")))
            {
                // Invalid install location...
            }
            else if ((args.Length > 0 && args[0].ToLower() == "live") || (!File.Exists("YgoMaster.exe") && !isMultiplayerClient))
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
                if (!isMultiplayerClient && !ClientSettings.DontAutoRunServerExe)
                {
                    Process[] processes = Process.GetProcessesByName(IsMonoRun ? "MonoRun" : "YgoMaster");
                    try
                    {
                        int processCount = processes.Length;
                        if (IsMonoRun && processCount > 0)
                        {
                            foreach (Process process in processes)
                            {
                                List<string> processArgs = ProcessCommandLine.RetrieveArgs(process);
                                if (processArgs.Count < 2 || processArgs[1] != "YgoMaster.exe")
                                {
                                    processCount--;
                                }
                            }
                        }
                        if (processCount == 0)
                        {
                            string serverExe = Path.Combine(Environment.CurrentDirectory, "YgoMaster.exe");
                            if (File.Exists(serverExe))
                            {
                                if (IsMonoRun)
                                {
                                    Process.Start("MonoRun.exe", "YgoMaster.exe").Close();
                                }
                                else
                                {
                                    Process.Start(serverExe).Close();
                                }
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
                }
                switch (ClientSettings.LaunchMode)
                {
                    case GameLauncherMode.Inject:
                        ClientSettings.LaunchMode = GameLauncherMode.Detours;
                        break;
                }
                success = GameLauncher.Launch(ClientSettings.LaunchMode);
            }
            if (!success)
            {
                ShowMessageBoxError("Failed. Make sure the YgoMaster folder is inside game folder.\n\nThis should roughly be (depending on your steam install):\n\n " +
                    "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Yu-Gi-Oh! Master Duel\\YgoMaster\\\n\nThe working directory is:\n\n" +
                     Environment.CurrentDirectory);
            }
        }

        public static int DllMain(string arg)
        {
            if (arg == "MonoRun")
            {
                Console.WriteLine("(MonoRun)");
                IsMonoRun = true;
                Main(new string[0]);
                return 0;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                IsLive = arg == "live";

                CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                DataDir = Utils.GetDataDirectory(false, CurrentDir);
                LocalPlayerSaveDataDir = Path.Combine(DataDir, "Players", "Local");
                ClientDataDir = Path.Combine(DataDir, "ClientData");
                ClientDataDumpDir = Path.Combine(DataDir, "ClientDataDump");
                DuelSettings.LoadBgmInfo(Path.Combine(DataDir, "Bgm.json"));
                ItemID.Load(DataDir);
                YdkHelper.LoadIdMap(DataDir);

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

                string pluginsDir = Path.Combine(CurrentDir, "Plugins");
                if (Directory.Exists(pluginsDir))
                {
                    foreach (string file in Directory.GetFiles(pluginsDir, "*.dll"))
                    {
                        PInvoke.LoadLibrary(file);
                    }
                }

                if (ClientSettings.ReflectionValidatorDump)
                {
                    ReflectionValidator.IsDumping = true;
                }
                else if (ClientSettings.ReflectionValidatorValidate)
                {
                    ReflectionValidator.ValidateDump();
                }

                if (!IsLive)
                {
                    SimpleProxy.Run();
                }

                if (!string.IsNullOrEmpty(ClientSettings.MultiplayerToken) &&
                    !string.IsNullOrEmpty(ClientSettings.SessionServerIP) && ClientSettings.SessionServerPort != 0)
                {
                    NetClient = new NetClient();
                    NetClient.NoDelay = ClientSettings.MultiplayerNoDelay;
                    NetClient.Disconnected += (x) =>
                    {
                        if (ClientSettings.MultiplayerLogConnectionState)
                        {
                            Console.WriteLine("Disconnected from " + ClientSettings.SessionServerIP + ":" + ClientSettings.SessionServerPort);
                        }
                        DuelDll.HasNetworkError = true;
                    };

                    DateTime lastConnectAttempt = DateTime.MinValue;

                    new Thread(delegate ()
                    {
                        string ip = ClientSettings.ResolveIP(ClientSettings.SessionServerIP);

                        while (true)
                        {
                            if (!NetClient.IsConnected && lastConnectAttempt < DateTime.UtcNow - TimeSpan.FromSeconds(ClientSettings.MultiplayerConnectDelayInSeconds))
                            {
                                lastConnectAttempt = DateTime.UtcNow;
                                try
                                {
                                    NetClient.Connect(ip, ClientSettings.SessionServerPort);
                                    NetClient.Send(new ConnectionRequestMessage()
                                    {
                                        Token = ClientSettings.MultiplayerToken
                                    });
                                    if (ClientSettings.MultiplayerLogConnectionState)
                                    {
                                        Console.WriteLine("Connected to " + ClientSettings.SessionServerIP + ":" + ClientSettings.SessionServerPort);
                                    }
                                }
                                catch
                                {
                                    if (ClientSettings.MultiplayerLogConnectionState)
                                    {
                                        Console.WriteLine("Failed to connect to " + ClientSettings.SessionServerIP + ":" + ClientSettings.SessionServerPort);
                                    }
                                }
                            }
                            if (NetClient.IsConnected && NetClient.LastMessageTime < DateTime.UtcNow - TimeSpan.FromSeconds(ClientSettings.MultiplayerPingTimeoutInSeconds))
                            {
                                if (ClientSettings.MultiplayerLogConnectionState)
                                {
                                    Console.WriteLine("Ping timeout " + ClientSettings.SessionServerIP + ":" + ClientSettings.SessionServerPort);
                                }
                                NetClient.Close();
                            }
                            Thread.Sleep(1000);
                        }
                    }).Start();
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
                nativeTypes.Add(typeof(YgomGame.Duel.DuelHUD_PrepareToDuelProcess));
                nativeTypes.Add(typeof(YgomGame.Duel.Util));
                nativeTypes.Add(typeof(YgomGame.Duel.Engine));
                nativeTypes.Add(typeof(YgomGame.Duel.EngineApiUtil));
                nativeTypes.Add(typeof(YgomGame.Duel.GenericCardListController));
                nativeTypes.Add(typeof(YgomGame.Duel.CardIndividualSetting));
                nativeTypes.Add(typeof(YgomGame.Duel.CardRunEffectSetting));
                nativeTypes.Add(typeof(YgomGame.Duel.DuelHUD));
                nativeTypes.Add(typeof(YgomGame.Duel.DuelTimer3D));
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
                // DeckEditorUtils
                nativeTypes.Add(typeof(TMPro.TMP_Text));
                nativeTypes.Add(typeof(YgomGame.Deck.DeckView));
                nativeTypes.Add(typeof(YgomGame.Deck.CardCollectionView));
                nativeTypes.Add(typeof(YgomGame.DeckEditViewController2));
                nativeTypes.Add(typeof(YgomGame.SubMenu.DeckEditSubMenuViewController));
                nativeTypes.Add(typeof(YgomGame.SubMenu.SubMenuViewController));
                nativeTypes.Add(typeof(YgomSystem.Sound));
                // DuelReplayUtils
                nativeTypes.Add(typeof(YgomGame.SubMenu.HomeSubMenuViewController));
                nativeTypes.Add(typeof(YgomGame.Menu.ProfileReplayViewController));
                // TradeUtils
                nativeTypes.Add(typeof(YgomGame.Menu.ProfileViewController));
                nativeTypes.Add(typeof(YgomGame.Menu.ToastMessageInform));
                nativeTypes.Add(typeof(TradeUtils));
                // Misc
                nativeTypes.Add(typeof(DuelDll));
                nativeTypes.Add(typeof(DuelTapSync));
                nativeTypes.Add(typeof(DuelEmoteHelper));
                nativeTypes.Add(typeof(YgomGame.Menu.CommonDialogViewController));
                nativeTypes.Add(typeof(YgomGame.Menu.ActionSheetViewController));
                nativeTypes.Add(typeof(Win32Hooks));
                nativeTypes.Add(typeof(AssetHelper));
                nativeTypes.Add(typeof(WallpaperCycle));
                nativeTypes.Add(typeof(CustomBackground));
                nativeTypes.Add(typeof(HomeViewTweaks));
                nativeTypes.Add(typeof(FixDeleteFile));
                nativeTypes.Add(typeof(FixLanguage));
                nativeTypes.Add(typeof(SoundInterceptor));
                nativeTypes.Add(typeof(SoloVisualNovel));
                nativeTypes.Add(typeof(SoloVisualNovelChapterView));
                nativeTypes.Add(typeof(YgomGame.Duel.DuelTutorialSetting));
                nativeTypes.Add(typeof(YgomGame.Tutorial.CardFlyingViewController));
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
                nativeTypes.Add(typeof(UnityEngine.UnityObject));
                nativeTypes.Add(typeof(UnityEngine.GameObject));
                nativeTypes.Add(typeof(UnityEngine.Transform));
                nativeTypes.Add(typeof(UnityEngine.Component));
                // Uncomment the following for easier logging of solo content
                foreach (Type type in nativeTypes)
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
                PInvoke.WL_EnableAllHooks(true);

                ReflectionValidator.FinishDumping();

                DuelEmoteHelper.Load();

                if (NetClient != null)
                {
                    NetClient.HandleMessage += DuelDll.HandleNetMessage;
                    NetClient.HandleMessage += TradeUtils.HandleNetMessage;
                    NetClient.Disconnected += TradeUtils.Disconnected;
                }

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

                    YgomSystem.Utility.TextData.LoadCustomTextData();
                    CustomBackground.Init();
                    AssetHelper.Init();
                });

                if (ClientSettings.ShowConsole)
                {
                    ConsoleHelper.Run();
                }
            }
            catch (Exception e)
            {
                string clientVersion = null;
                try
                {
                    IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
                    if (assembly != null)
                    {
                        IL2Class Version_Class = assembly.GetClass("Version", "YgomSystem.Utility");
                        if (Version_Class != null)
                        {
                            IL2Property Version_AppCommonVersion = Version_Class.GetProperty("AppCommonVersion");
                            if (Version_AppCommonVersion != null)
                            {
                                IL2Object obj = Version_AppCommonVersion.GetGetMethod().Invoke();
                                if (obj != null)
                                {
                                    clientVersion = new IL2String(obj.ptr).ToString();
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Game version: " + clientVersion);
                sb.AppendLine("Supported version: " + ClientSettings.SupportedGameVersion);
                sb.AppendLine();
                if (clientVersion != ClientSettings.SupportedGameVersion)
                {
                    sb.AppendLine("--------------------- READ ME ---------------------");
                    sb.AppendLine("Unsupported game version. Game updates typically break YgoMaster. You'll need to download the latest version of YgoMaster or wait until a new release. Or download the latest version of the game using Steam if \"supported version\" is greater than \"game version\".");
                    sb.AppendLine("---------------------------------------------------");
                    sb.AppendLine();
                }
                sb.AppendLine(e.ToString());

                ShowMessageBox(sb.ToString());
                Environment.Exit(0);
                return 1;
            }
            return 0;
        }

        // The following are in seperate functions due to .NET Core erroring out from the lack of WinForms support

        static void ShowMessageBox(string str)
        {
            Console.WriteLine(str);
            if (Environment.Version.Major == 4)
            {
                ShowMessageBoxImpl(str);
            }
        }

        static void ShowMessageBoxImpl(string str)
        {
            System.Windows.Forms.MessageBox.Show(str);
        }

        static void ShowMessageBoxError(string str)
        {
            Console.WriteLine(str);
            if (Environment.Version.Major == 4)
            {
                ShowMessageBoxImpl(str);
            }
        }

        static void ShowMessageBoxErrorImpl(string error)
        {
            System.Windows.Forms.MessageBox.Show(error, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.None);
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
                if (!AssetHelper.IsQuitting)
                {
                    actions[i]();
                }
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

namespace YgomGame.Utility
{
    unsafe static class ItemUtil
    {
        static IL2Class classInfo;
        static IL2Method methodGetItemName;
        static IL2Method methodGetItemDesc;
        static IL2Method methodGetCategoryFromID;
        static IL2Method methodGetCategoryOffset;

        static ItemUtil()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ItemUtil", "YgomGame.Utility");
            methodGetItemName = classInfo.GetMethod("GetItemName", x => x.GetParameters().Length == 2);
            methodGetItemDesc = classInfo.GetMethod("GetItemDesc", x => x.GetParameters().Length == 2);
            methodGetCategoryFromID = classInfo.GetMethod("GetCategoryFromID");
            methodGetCategoryOffset = classInfo.GetMethod("GetCategoryOffset");
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

        public static ItemID.Category GetCategoryFromID(int itemID)
        {
            return (ItemID.Category)methodGetCategoryFromID.Invoke(new IntPtr[] { new IntPtr(&itemID) }).GetValueRef<int>();
        }

        public static int GetCategoryOffset(ItemID.Category category)
        {
            return methodGetCategoryOffset.Invoke(new IntPtr[] { new IntPtr(&category) }).GetValueRef<int>();
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

        delegate IntPtr Del_GetTextString(IntPtr textEnum, bool richTextEx, IntPtr methodInstance);
        delegate IntPtr Del_GetTextEnum(int textEnum, bool richTextEx, IntPtr methodInstance);
        delegate csbool Del_ContainsText(IntPtr textEnum, IntPtr methodInstance);
        static Hook<Del_GetTextString> hookGetTextString;
        static Hook<Del_GetTextEnum> hookGetTextEnum;
        static Hook<Del_ContainsText> hookContainsText;

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
            IL2Method methodContainsTextEnum = classInfo.GetMethod("ContainsText").MakeGenericMethod(new IntPtr[] { IL2SystemClass.String.IL2Typeof() });
            hookContainsText = new Hook<Del_ContainsText>(ContainsText, methodContainsTextEnum);

            // System.Object.ToString
            methodObjectToString = Assembler.GetAssembly("mscorlib").GetClass("Object", "System").GetMethod("ToString");
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
            return new IL2String(GetTextString(new IL2String(id).ptr, false, methodGetTextString.ptr)).ToString();
        }

        static IntPtr GetTextString(IntPtr textString, bool richTextEx, IntPtr methodInstance)
        {
            // TODO: Confirm textString is of type string as v1.9.0 only has TextData.GetText<object> (no TextData.GetText<string>)
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
            return hookGetTextString.Original(textString, richTextEx, methodInstance);
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
                            if (fullString == "IDS_SYS.FATAL_FILE_ERROR")
                            {
                                IntPtr result = hookGetTextEnum.Original(textEnum, richTextEx, methodInstance);
                                return new IL2String(ClientSettings.CustomTextFileLoadErrorEx + new IL2String(result).ToString()).ptr;
                            }
                        }
                    }
                }
            }
            return hookGetTextEnum.Original(textEnum, richTextEx, methodInstance);
        }

        static csbool ContainsText(IntPtr textEnum, IntPtr methodInstance)
        {
            IntPtr klass = Import.Object.il2cpp_object_get_class(textEnum);
            if (klass == IL2SystemClass.String.ptr)
            {
                string inputString = new IL2String(textEnum).ToString();
                if (!string.IsNullOrEmpty(inputString) && (inputString.StartsWith(HackID) || CustomTextData.ContainsKey(inputString)))
                {
                    return true;
                }
            }
            return hookContainsText.Original(textEnum, methodInstance);
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

        public static void UpdateJson(string jsonPath, string jsonString, bool keep = false)
        {
            //methodUpdateJson.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, new IL2String(jsonString).ptr });
            methodUpdateValue.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, YgomMiniJSON.Json.Deserialize(jsonString), new IntPtr(&keep) });
        }

        public static void UpdateValue(string jsonPath, string value, bool keep = false)
        {
            methodUpdateValue.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, new IL2String(value).ptr, new IntPtr(&keep) });
        }

        public static void UpdateValue(string jsonPath, IntPtr value, bool keep = false)
        {
            methodUpdateValue.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr, value, new IntPtr(&keep) });
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

        public static T GetByJsonPath<T>(string jsonPath) where T : struct
        {
            IL2Object obj = methodGetByJsonPath.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr });
            return obj != null ? obj.GetValueRef<T>() : default(T);
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

        delegate void Del_ResetData();
        static Hook<Del_ResetData> hookResetData;

        static ClientWorkUtil()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ClientWorkUtil", "YgomSystem.Utility");
            hookGetToken = new Hook<Del_GetToken>(GetToken, classInfo.GetMethod("GetToken"));
            hookResetData = new Hook<Del_ResetData>(ResetData, classInfo.GetMethod("ResetData"));
        }

        static IntPtr GetToken()
        {
            if (!string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
            {
                return new IL2String(Convert.ToBase64String(Encoding.UTF8.GetBytes(ClientSettings.MultiplayerToken))).ptr;
            }
            return hookGetToken.Original();
        }

        static void ResetData()
        {
            hookResetData.Original();
            YgomSystem.Network.ProtocolHttp.SetUrls();
        }
    }
}

namespace YgomSystem.Network
{
    unsafe static class ProtocolHttp
    {
        delegate void Del_GetServerDefaultUrl(int type, out IntPtr url, out IntPtr pollingUrl);
        static Hook<Del_GetServerDefaultUrl> hookGetServerDefaultUrl;

        static ProtocolHttp()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ProtocolHttp", "YgomSystem.Network");
            hookGetServerDefaultUrl = new Hook<Del_GetServerDefaultUrl>(GetServerDefaultUrl, classInfo.GetMethod("GetServerDefaultUrl"));
        }

        static void GetServerDefaultUrl(int type, out IntPtr url, out IntPtr pollingUrl)
        {
            // This doesn't work for some reason...
            url = new IL2String(ClientSettings.ServerUrl).ptr;
            pollingUrl = new IL2String(ClientSettings.ServerPollUrl).ptr;
            // But this does...
            SetUrls();
        }

        public static void SetUrls()
        {
            Dictionary<string, object> urls = new Dictionary<string, object>()
            {
                { "System.info", ClientSettings.ServerUrl },
                { "Account.auth", ClientSettings.ServerUrl },
                { "Account.create", ClientSettings.ServerUrl },
                { "Account.inherit", ClientSettings.ServerUrl },
                { "Account.Steam.get_user_id", ClientSettings.ServerUrl },
                { "Account.Steam.re_auth", ClientSettings.ServerUrl },
                { "Billing.add_purchased_item", ClientSettings.ServerUrl },
                { "Billing.cancel", ClientSettings.ServerUrl },
                { "Billing.history", ClientSettings.ServerUrl },
                { "Billing.in_complete_item_check", ClientSettings.ServerUrl },
                { "Billing.product_list", ClientSettings.ServerUrl },
                { "Billing.purchase", ClientSettings.ServerUrl },
                { "Billing.re_store", ClientSettings.ServerUrl },
                { "Billing.reservation", ClientSettings.ServerUrl },
            };
            YgomSystem.Utility.ClientWork.UpdateJson("$.Server.urls", MiniJSON.Json.Serialize(urls));
            YgomSystem.Utility.ClientWork.UpdateValue("$.Server.url", ClientSettings.ServerUrl);
            YgomSystem.Utility.ClientWork.UpdateValue("$.Server.url_polling", ClientSettings.ServerPollUrl);
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

        private static string localDataDir;
        public static string LocalDataDir
        {
            get
            {
                if (string.IsNullOrEmpty(localDataDir))
                {
                    foreach (string dir in Directory.GetDirectories("LocalData"))
                    {
                        if (dir != "00000000")
                        {
                            localDataDir = dir;
                            break;
                        }
                    }
                }
                return localDataDir;
            }
            private set { localDataDir = value; }
        }

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
                                // "dlcVersion" - should be modified any time new data is downloaded
                                FileInfo fileInfo = new FileInfo(Path.Combine(dirInfo.FullName, "0000", "32", "32782cb5"));
                                if (fileInfo.Exists)
                                {
                                    possibleFoldersExactMatch[dirInfo.Name] = fileInfo.LastWriteTime;
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

        delegate IntPtr Del_GetIPCountry();
        static Hook<Del_GetIPCountry> hookGetIPCountry;
        delegate csbool Del_IsOverlayEnabled();
        static Hook<Del_IsOverlayEnabled> hookIsOverlayEnabled;
        delegate csbool Del_ShowGamepadTextInput(int eInputMode, int eLineInputMode, IntPtr pchDescription, uint unCharMax, IntPtr pchExistingText);
        static Hook<Del_ShowGamepadTextInput> hookShowGamepadTextInput;
        delegate csbool Del_GetEnteredGamepadTextInput(IntPtr pchText, uint cchText);
        static Hook<Del_GetEnteredGamepadTextInput> hookGetEnteredGamepadTextInput;
        delegate csbool Del_IsSteamRunningOnSteamDeck();
        static Hook<Del_IsOverlayEnabled> hookIsSteamRunningOnSteamDeck;
        delegate csbool Del_ShowFloatingGamepadTextInput(int eKeyboardMode, int nTextFieldXPosition, int nTextFieldYPosition, int nTextFieldWidth, int nTextFieldHeight);
        static Hook<Del_ShowFloatingGamepadTextInput> hookShowFloatingGamepadTextInput;

        static SteamUtils()
        {
            if (Program.IsLive)
            {
                return;
            }
            IL2Assembly assembly = Assembler.GetAssembly("com.rlabrecque.steamworks.net");
            classInfo = assembly.GetClass("SteamUtils", "Steamworks");
            methodIsOverlayEnabled = classInfo.GetMethod("IsOverlayEnabled");

            hookGetIPCountry = new Hook<Del_GetIPCountry>(GetIPCountry, classInfo.GetMethod("GetIPCountry"));
            hookIsOverlayEnabled = new Hook<Del_IsOverlayEnabled>(IsOverlayEnabled, methodIsOverlayEnabled);
            hookShowGamepadTextInput = new Hook<Del_ShowGamepadTextInput>(ShowGamepadTextInput, classInfo.GetMethod("ShowGamepadTextInput"));
            hookGetEnteredGamepadTextInput = new Hook<Del_GetEnteredGamepadTextInput>(GetEnteredGamepadTextInput, classInfo.GetMethod("GetEnteredGamepadTextInput"));
            hookIsSteamRunningOnSteamDeck = new Hook<Del_IsOverlayEnabled>(IsSteamRunningOnSteamDeck, classInfo.GetMethod("IsSteamRunningOnSteamDeck"));
            hookShowFloatingGamepadTextInput = new Hook<Del_ShowFloatingGamepadTextInput>(ShowFloatingGamepadTextInput, classInfo.GetMethod("ShowFloatingGamepadTextInput"));
        }

        static IntPtr GetIPCountry()
        {
            return IntPtr.Zero;
        }

        static csbool IsOverlayEnabled()
        {
            return false;
        }

        static csbool ShowGamepadTextInput(int eInputMode, int eLineInputMode, IntPtr pchDescription, uint unCharMax, IntPtr pchExistingText)
        {
            return false;
        }

        static csbool GetEnteredGamepadTextInput(IntPtr pchText, uint cchText)
        {
            return false;
        }

        static csbool IsSteamRunningOnSteamDeck()
        {
            return false;
        }

        static csbool ShowFloatingGamepadTextInput(int eKeyboardMode, int nTextFieldXPosition, int nTextFieldYPosition, int nTextFieldWidth, int nTextFieldHeight)
        {
            return false;
        }
    }
}