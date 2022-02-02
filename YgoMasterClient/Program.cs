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

namespace YgoMasterClient
{
    public static class Program
    {
        public const string ServerUrl = "http://localhost/ygo";
        public const string ServerPollUrl = "http://localhost/ygo/poll";
        static bool runConsole = true;

        /// <summary>
        /// Directory path of YgoMasterClient.exe
        /// </summary>
        public static string CurrentDir;
        /// <summary>
        /// Directory path of the custom client content
        /// </summary>
        public static string ClientDataDir;
        /// <summary>
        /// Directory path of the server content
        /// </summary>
        public static string ServerDataDir;

        static void Main(string[] args)
        {
            if (!GameLauncher.Launch(GameLauncherMode.Detours, args))
            {
                MessageBox.Show("Failed! Make sure the YgoMaster folder is inside game folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static int DllMain(string arg)
        {
            try
            {
                PInvoke.WL_InitHooks();
                PInvoke.InitGameModuleBaseAddress();

                CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ClientDataDir = Path.Combine(CurrentDir, "ClientData");
                ServerDataDir = Path.Combine(CurrentDir, "Data");

                List<Type> nativeTypes = new List<Type>();
                if (arg == "live")
                {
                    // TODO
                    nativeTypes.Add(typeof(DuellDll));
                }
                else
                {
                    nativeTypes.Add(typeof(DuellDll));
                    nativeTypes.Add(typeof(YgomSystem.ResourceManager));
                    nativeTypes.Add(typeof(YgomGame.Utility.ItemUtil));
                    nativeTypes.Add(typeof(YgomSystem.Utility.TextData));
                    nativeTypes.Add(typeof(YgomSystem.Network.ProtocolHttp));
                    nativeTypes.Add(typeof(YgomSystem.Utility.ClientWork));
                    nativeTypes.Add(typeof(MiniJSON.Json));
                    nativeTypes.Add(typeof(Steamworks.SteamAPI));
                    nativeTypes.Add(typeof(Steamworks.SteamUtils));
                }
                foreach (Type type in nativeTypes)
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }

                if (runConsole)
                {
                    // Use a form as an easy way to get execution on the main thread
                    Form dummyForm = new Form();
                    dummyForm.Opacity = 0;
                    dummyForm.FormBorderStyle = FormBorderStyle.None;
                    dummyForm.Show();
                    dummyForm.Hide();
                    ConsoleHelper.ShowConsole();
                    new Thread(delegate()
                        {
                            while (true)
                            {
                                string consoleInput = Console.ReadLine();
                                dummyForm.Invoke((MethodInvoker)delegate
                                {
                                    try
                                    {
                                        string[] splitted = consoleInput.Split();
                                        switch (splitted[0].ToLower())
                                        {
                                            case "itemid":
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
                                                            string prefix = name == "deleted" ? "//" : "";
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
                                                    File.WriteAllText("dump-itemid.txt", res.ToString());
                                                    Console.WriteLine("Done");
                                                }
                                                break;
                                            case "packnames":
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
                                            case "packimages":
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
                                                            if (YgomSystem.ResourceManager.Exists("Images/CardPack/<_RESOURCE_TYPE_>/<_CARD_ILLUST_>/" + name))
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
                                            case "text":
                                                {
                                                    string id = splitted[1];
                                                    Console.WriteLine(YgomSystem.Utility.TextData.GetText(id));
                                                }
                                                break;
                                            case "texte":// Gets all text from the given enum
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
                                            case "resultcodes":
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
                return 1;
            }
            return 0;
        }
    }
}

unsafe static class DuellDll
{
    delegate void Del_DLL_DuelSetPlayerType(int player, int type);
    static Hook<Del_DLL_DuelSetPlayerType> hookDLL_DuelSetPlayerType;

    delegate int Del_DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf);
    static Hook<Del_DLL_DuelSysInitCustom> hookDLL_DuelSysInitCustom;

    static DuellDll()
    {
        IntPtr lib = PInvoke.LoadLibrary("masterduel_Data/Plugins/x86_64/duel.dll");
        if (lib == IntPtr.Zero)
        {
            throw new Exception("Failed to load duel.dll");
        }
        hookDLL_DuelSetPlayerType = new Hook<Del_DLL_DuelSetPlayerType>(DLL_DuelSetPlayerType, PInvoke.GetProcAddress(lib, "DLL_DuelSetPlayerType"));
        hookDLL_DuelSysInitCustom = new Hook<Del_DLL_DuelSysInitCustom>(DLL_DuelSysInitCustom, PInvoke.GetProcAddress(lib, "DLL_DuelSysInitCustom"));
    }

    static void DLL_DuelSetPlayerType(int player, int type)
    {
        //type = 1;
        //Console.WriteLine("DLL_DuelSetPlayerType " + player + " " + type);
        hookDLL_DuelSetPlayerType.Original(player, type);
    }

    static int DLL_DuelSysInitCustom(int fDuelType, bool tag, int life0, int life1, int hand0, int hand1, bool shuf)
    {
        //hand0 = 3;
        //hand1 = 0;
        //life0 = 900000;
        //life1 = 1;
        return hookDLL_DuelSysInitCustom.Original(fDuelType, tag, life0, life1, hand0, hand1, shuf);
    }
}

namespace YgomSystem
{
    unsafe static class ResourceManager
    {
        static IL2Class classInfo;
        static IL2Method methodExists;
        static IL2Method methodGetResource;

        delegate IntPtr Del_GetResource(IntPtr thisPtr, IntPtr path, IntPtr workPath);
        static Hook<Del_GetResource> hookGetResource;

        static ResourceManager()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ResourceManager", "YgomSystem");
            methodExists = classInfo.GetMethod("Exists");
            methodGetResource = classInfo.GetMethod("getResource", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Name == "path");
            hookGetResource = new Hook<Del_GetResource>(GetResource, methodGetResource);
        }

        public static bool Exists(string filename)
        {
            IL2Object obj = methodExists.Invoke(new IntPtr[] { new IL2String(filename).ptr });
            return obj != null ? obj.GetValueRef<bool>() : false;
        }

        static IntPtr GetResource(IntPtr thisPtr, IntPtr path, IntPtr workPath)
        {
            Console.WriteLine("path:" + new IL2String(path).ToString());
            string str = new IL2String(path).ToString();
            /*if (!string.IsNullOrEmpty(str) && str.Contains("Illust/0000"))
            {
                Console.WriteLine("ImageBase: " + PInvoke.GameModuleBaseAddress.ToInt64().ToString("X16"));
                path = new IL2String("Card/Images/Illust/<_CARD_ILLUST_>/15635").ptr;
                //System.Diagnostics.Debugger.Launch();
            }*/
            return hookGetResource.Original(thisPtr, path, workPath);
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

        static ItemUtil()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ItemUtil", "YgomGame.Utility");
            methodGetItemName = classInfo.GetMethod("GetItemName", x => x.GetParameters().Length == 1);
            methodGetItemDesc = classInfo.GetMethod("GetItemDesc", x => x.GetParameters().Length == 2);
            methodGetCategoryFromID = classInfo.GetMethod("GetCategoryFromID");
        }

        public static string GetItemName(int itemID)
        {
            return methodGetItemName.Invoke(new IntPtr[] { new IntPtr(&itemID) }).GetValueObj<string>();
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
            PACK_TICKET
        }
    }
}

namespace YgomSystem.Network
{
    unsafe static class ProtocolHttp
    {
        static IL2Class classInfo;
        static IL2Method methodGetServerDefaultUrl;

        delegate void Del_GetServerDefaultUrl(int type, IntPtr* outUrl, IntPtr* outPollingUrl);
        static Hook<Del_GetServerDefaultUrl> hookGetServerDefaultUrl;

        static ProtocolHttp()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ProtocolHttp", "YgomSystem.Network");
            methodGetServerDefaultUrl = classInfo.GetMethod("GetServerDefaultUrl");

            hookGetServerDefaultUrl = new Hook<Del_GetServerDefaultUrl>(GetServerDefaultUrl, methodGetServerDefaultUrl);
        }

        static void GetServerDefaultUrl(int type, IntPtr* outUrl, IntPtr* outPollingUrl)
        {
            // This doesn't work for some reason...
            *outUrl = new IL2String(Program.ServerUrl).ptr;
            *outPollingUrl = new IL2String(Program.ServerPollUrl).ptr;
            // But this does...
            YgomSystem.Utility.ClientWork.UpdateJson("$.Server.urls", "{\"System.info\":\"" + Program.ServerUrl + "\"}");
            YgomSystem.Utility.ClientWork.UpdateValue("$.Server.url", Program.ServerUrl);
            YgomSystem.Utility.ClientWork.UpdateValue("$.Server.url_polling", Program.ServerUrl);
        }
    }
}


namespace YgomSystem.Utility
{
    unsafe static class TextData
    {
        static IL2Class classInfo;
        static IL2Method methodLoad;

        delegate IntPtr Del_GetText(IntPtr textPtr, bool richTextEx);
        static Hook<Del_GetText> hookGetText;

        static TextData()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("TextData", "YgomSystem.Utility");
            methodLoad = classInfo.GetMethod("Load", x => x.GetParameters().Length == 1);

            // TODO: Determine a way to get a generic address via reflection
            hookGetText = new Hook<Del_GetText>(GetText, PInvoke.GameModuleBaseAddress + 0xE732C0);
        }

        public static void LoadGroup(string groupid)
        {
            methodLoad.Invoke(new IntPtr[] { new IL2String(groupid).ptr });
        }

        public static string GetText(string id)
        {
            return new IL2String(GetText(new IL2String(id).ptr, false)).ToString();
        }

        static IntPtr GetText(IntPtr textPtr, bool richTextEx)
        {
            string inputString = new IL2Object(textPtr).GetValueObj<string>();
            Console.WriteLine(inputString);
            const string header = "IDS_SYS.IDHACK:";
            if (!string.IsNullOrEmpty(inputString) && inputString.StartsWith(header))
            {
                return new IL2String(inputString.Substring(header.Length)).ptr;
            }
            return hookGetText.Original(textPtr, richTextEx);
        }
    }

    unsafe static class ClientWork
    {
        static IL2Class classInfo;
        static IL2Method methodUpdateJsonRaw;
        static IL2Method methodUpdateJson;
        static IL2Method methodUpdateValue;
        static IL2Method methodGetByJsonPath;
        static IL2Method methodGetStringByJsonPath;

        static ClientWork()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            classInfo = assembly.GetClass("ClientWork", "YgomSystem.Utility");
            methodUpdateJsonRaw = classInfo.GetMethod("updateJson", x => x.GetParameters().Length == 1);
            methodUpdateJson = classInfo.GetMethod("updateJson", x => x.GetParameters().Length == 2);
            methodUpdateValue = classInfo.GetMethod("updateValue", x => x.GetParameters().Length == 3);
            methodGetByJsonPath = classInfo.GetMethod("getByJsonPath", x => x.GetParameters().Length == 1);
            methodGetStringByJsonPath = classInfo.GetMethod("getStringByJsonPath", x => x.GetParameters().Length == 2);
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

        // Custom func to take a json path and get a re-serialized string back
        public static string SerializePath(string jsonPath)
        {
            IL2Object obj = methodGetByJsonPath.Invoke(new IntPtr[] { new IL2String(jsonPath).ptr });
            return MiniJSON.Json.Serialize(obj);
        }
    }
}

namespace MiniJSON
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

        public static IL2Object Deserialize(string json)
        {
            return methodDeserialize.Invoke(new IntPtr[] { new IL2String(json).ptr });
        }

        public static string Serialize(IL2Object obj)
        {
            return methodSerialize.Invoke(new IntPtr[] { obj.ptr }).GetValueObj<string>();
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
        static IL2Method methodReleaseCurrentThreadMemory;
        static IL2Method methodRunCallbacks;
        static IL2Method methodIsSteamRunning;

        delegate bool Del_Init();
        delegate void Del_Shutdown();
        delegate bool Del_RestartAppIfNecessary(IntPtr unOwnAppID);
        delegate void Del_ReleaseCurrentThreadMemory();
        delegate void Del_RunCallbacks();
        delegate bool Del_IsSteamRunning();

        static Hook<Del_Init> hookInit;
        static Hook<Del_Shutdown> hookShutdown;
        static Hook<Del_RestartAppIfNecessary> hookRestartAppIfNecessary;
        static Hook<Del_ReleaseCurrentThreadMemory> hookReleaseCurrentThreadMemory;
        static Hook<Del_RunCallbacks> hookRunCallbacks;
        static Hook<Del_IsSteamRunning> hookIsSteamRunning;

        static SteamAPI()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp-firstpass");
            classInfo = assembly.GetClass("SteamAPI", "Steamworks");
            methodInit = classInfo.GetMethod("Init");
            methodShutdown = classInfo.GetMethod("Shutdown");
            methodRestartAppIfNecessary = classInfo.GetMethod("RestartAppIfNecessary");
            methodReleaseCurrentThreadMemory = classInfo.GetMethod("ReleaseCurrentThreadMemory");
            methodRunCallbacks = classInfo.GetMethod("RunCallbacks");
            methodIsSteamRunning = classInfo.GetMethod("IsSteamRunning");

            hookInit = new Hook<Del_Init>(InitH, methodInit);
            hookShutdown = new Hook<Del_Shutdown>(Shutdown, methodShutdown);
            hookRestartAppIfNecessary = new Hook<Del_RestartAppIfNecessary>(RestartAppIfNecessary, methodRestartAppIfNecessary);
            hookReleaseCurrentThreadMemory = new Hook<Del_ReleaseCurrentThreadMemory>(ReleaseCurrentThreadMemory, methodReleaseCurrentThreadMemory);
            hookRunCallbacks = new Hook<Del_RunCallbacks>(RunCallbacks, methodRunCallbacks);
            hookIsSteamRunning = new Hook<Del_IsSteamRunning>(IsSteamRunning, methodIsSteamRunning);
        }

        static bool InitH()
        {
            return false;
        }

        static void Shutdown()
        {
        }

        static bool RestartAppIfNecessary(IntPtr unOwnAppId)
        {
            return false;
        }

        static void ReleaseCurrentThreadMemory()
        {
        }

        static void RunCallbacks()
        {
        }

        static bool IsSteamRunning()
        {
            return true;
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
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp-firstpass");
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