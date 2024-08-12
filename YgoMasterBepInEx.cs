using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace YgoMasterBepInEx
{
    [BepInPlugin("YgoMasterBepInEx", "YgoMasterBepInEx", "1.0.0")]
    public class YgoMasterBepInExPlugin : BasePlugin
    {
        public override void Load()
        {
            // TODO: Support "live" mode in YgoMaster+BepInEx
            string targetDir = null;
            string[] args = Environment.GetCommandLineArgs();
            if (args != null)
            {
                string targetArg = "--YgoMasterDir=";
                foreach (string arg in args)
                {
                    if (arg.StartsWith(targetArg))
                    {
                        try
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(arg.Substring(targetArg.Length).Trim('\"'));
                            if (!dirInfo.Exists)
                            {
                                return;
                            }
                            targetDir = dirInfo.Name;
                            Console.WriteLine("YgoMasterDir: '" + targetDir + "'");
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(targetDir))
            {
                return;
            }
            string ygoMasterLoaderFile = Path.Combine(targetDir, "YgoMasterLoader.dll");
            string ygoMasterClientFile = Path.Combine(targetDir, "YgoMasterClient.exe");
            if (!File.Exists(ygoMasterLoaderFile))
            {
                Console.WriteLine("Failed to find '" + ygoMasterLoaderFile + "'");
                return;
            }
            if (!File.Exists(ygoMasterClientFile))
            {
                Console.WriteLine("Failed to find '" + ygoMasterClientFile + "'");
                return;
            }
            Console.WriteLine("Loading YgoMasterLoader.dll");
            IntPtr handle = LoadLibrary(ygoMasterLoaderFile);
            Console.WriteLine("YgoMasterLoader handle: " + handle);
            if (handle == IntPtr.Zero)
            {
                return;
            }
            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(ygoMasterClientFile));
            string dllMainArg = null;//"live";
            assembly.GetType("YgoMasterClient.Program").GetMethod("DllMain").Invoke(null, new object[] { dllMainArg });
        }
        
        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(string lpFileName);
    }
}