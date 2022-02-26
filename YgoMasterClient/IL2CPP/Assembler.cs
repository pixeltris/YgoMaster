using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public static class Assembler
    {
        private static IntPtr domain;
        private static List<IL2Assembly> listAssemblies = new List<IL2Assembly>();
        private static Dictionary<string, IL2Assembly> assembliesCached = new Dictionary<string, IL2Assembly>();

        private static void Loading()
        {
            domain = Import.Domain.il2cpp_domain_get();

            uint count = 0;
            IntPtr assemblies = Import.Domain.il2cpp_domain_get_assemblies(domain, ref count);
            IntPtr[] assembliesarr = NativeUtils.IntPtrToStructureArray<IntPtr>(assemblies, count);
            foreach (IntPtr assembly in assembliesarr)
            {
                if (assembly != IntPtr.Zero)
                {
                    IntPtr image = Import.Assembly.il2cpp_assembly_get_image(assembly);
                    if (image != IntPtr.Zero)
                    {
                        listAssemblies.Add(new IL2Assembly(Import.Assembly.il2cpp_assembly_get_image(assembly)));
                    }
                }
            }
        }

        public static IL2Assembly[] GetAssemblies()
        {
            if (listAssemblies.Count == 0)
                Loading();

            return listAssemblies.ToArray();
        }

        public static IL2Assembly GetAssembly(string name)
        {
            IL2Assembly returnval;
            if (assembliesCached.TryGetValue(name, out returnval))
            {
                return returnval;
            }
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            foreach (IL2Assembly asm in GetAssemblies())
            {
                if (asm.Name.Equals(name))
                {
                    returnval = asm;
                    break;
                }
            }
            assembliesCached[name] = returnval;
            return returnval;
        }

        /*private static Dictionary<string, IL2Assembly> getAssemblyList()
        {
            Dictionary<string, IL2Assembly> result = new Dictionary<string, IL2Assembly>();
            foreach (var aName in assemblers)
            {
                result.Add(aName.Key, GetAssembly(aName.Value));
            }
            return result;
        }

        public static Dictionary<string, string> assemblers = new Dictionary<string, string>()
        {
            {  "acs", "Assembly-CSharp" },
            {  "mscorlib", "mscorlib" },
            {  "System", "System" },
            {  "System.Core", "System.Core" },
            {  "VRCCore-Standalone", "VRCCore-Standalone" },
            {  "UnityEngine.CoreModule", "UnityEngine.CoreModule" },
            {  "UnityEngine.ImageConversionModule", "UnityEngine.ImageConversionModule" },
            {  "UnityEngine.InputLegacyModule", "UnityEngine.InputLegacyModule" },
            {  "UnityEngine.Analytics", "UnityEngine.UnityAnalyticsModule" },
            {  "UnityEngine.AnimationModule", "UnityEngine.AnimationModule" },
            {  "UnityEngine.PhysicsModule", "UnityEngine.PhysicsModule" },
            {  "UnityEngine.UI", "UnityEngine.UI" },
            {  "UnityEngine.UIModule", "UnityEngine.UIModule" },
            {  "UnityEngine.IMGUI", "UnityEngine.IMGUIModule" },
            {  "Photon", "Photon-DotNet" },
            {  "VRCSDKBase", "VRCSDKBase" },
            {  "VRCSDK2", "VRCSDK2" },
            {  "VRCSDK3", "VRCSDK3" },
            {  "VRCSDK3A", "VRCSDK3A" },
            {  "VRC.Udon", "VRC.Udon" },
            {  "VRC.UI.Core", "VRC.UI.Core" },
            {  "VRC.UI.Elements", "VRC.UI.Elements" },
            {  "Steamworks", "Facepunch.Steamworks.Win64" },
            {  "Transmtn", "Transmtn" },
            {  "Unity.TextMeshPro", "Unity.TextMeshPro" }
        };


        public static Dictionary<string, IL2Assembly> list = getAssemblyList();*/
    }
}
