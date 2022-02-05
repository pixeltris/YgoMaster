using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using IL2CPP;

namespace YgoMasterClient
{
    class Hook<T>
    {
        public T Func;
        public T Original;
        public IntPtr FuncPtr;
        public IntPtr OriginalPtr;

        public Hook(T hook, IL2Method method)
            : this(hook, Marshal.ReadIntPtr(method.ptr))
        {
        }

        public Hook(T hook, IntPtr address)
        {
            Func = hook;
            FuncPtr = Marshal.GetFunctionPointerForDelegate((Delegate)(object)hook);
            OriginalPtr = IntPtr.Zero;
            PInvoke.WL_CreateHook(address, FuncPtr, ref OriginalPtr);
            Original = (T)(object)Marshal.GetDelegateForFunctionPointer(OriginalPtr, typeof(T));
        }
    }

    class PInvoke
    {
        const string dllName = "YgoMasterLoader.dll";

        public static IntPtr GameModuleBaseAddress { get; private set; }

        public static void InitGameModuleBaseAddress()
        {
            foreach (System.Diagnostics.ProcessModule module in System.Diagnostics.Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
                {
                    GameModuleBaseAddress = module.BaseAddress;
                    break;
                }
            }
        }

        [DllImport(dllName)]
        public static extern int WL_InitHooks();

        [DllImport(dllName)]
        public static extern int WL_HookFunction(IntPtr target, IntPtr detour, ref IntPtr original);

        [DllImport(dllName)]
        public static extern int WL_CreateHook(IntPtr target, IntPtr detour, ref IntPtr original);

        [DllImport(dllName)]
        public static extern int WL_RemoveHook(IntPtr target);

        [DllImport(dllName)]
        public static extern int WL_EnableHook(IntPtr target);

        [DllImport(dllName)]
        public static extern int WL_DisableHook(IntPtr target);

        [DllImport(dllName)]
        public static extern int WL_EnableAllHooks(bool enable);

        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}
