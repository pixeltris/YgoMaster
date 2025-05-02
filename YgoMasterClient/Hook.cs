using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using IL2CPP;
using System.Diagnostics;

namespace YgoMasterClient
{
    class Hook<T>
    {
        public T Func;
        public T Original;
        public IntPtr FuncPtr;
        public IntPtr OriginalPtr;
        public IL2Method Method;

        public Hook(T hook, IL2Method method)
            : this(hook, Marshal.ReadIntPtr(method.ptr))
        {
            this.Method = method;
        }

        public Hook(T hook, IntPtr address)
        {
            Func = hook;
            FuncPtr = Marshal.GetFunctionPointerForDelegate((Delegate)(object)hook);
            OriginalPtr = IntPtr.Zero;
            int result = PInvoke.WL_CreateHook(address, FuncPtr, ref OriginalPtr);
            if (result == 3)
            {
                throw new Exception("Hook already exists for " + hook);
            }
            Original = (T)(object)Marshal.GetDelegateForFunctionPointer(OriginalPtr, typeof(T));
        }
    }

    class PInvoke
    {
        const string dllName = "YgoMasterLoader.dll";

        public static IntPtr GameModuleBaseAddress { get; private set; }

        public static void InitGameModuleBaseAddress()
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
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

        [DllImport(dllName)]
        public static extern void SetTimeMultiplier(double value);

        [DllImport(dllName)]
        public static extern void CreateVSyncHook(IntPtr funcPtr);

        [DllImport(dllName)]
        public static extern int lib_mp3dec_ex_t_sizeof();

        [DllImport(dllName)]
        public static extern int lib_mp3dec_ex_open_w(IntPtr dec, [MarshalAs(UnmanagedType.LPWStr)] string file_name, int flags);

        [DllImport(dllName)]
        public static extern int lib_mp3dec_ex_seek(IntPtr dec, ulong position);

        [DllImport(dllName)]
        public static extern int lib_mp3dec_ex_get_info(IntPtr dec, out ulong samples, out int channels, out int hz);

        [DllImport(dllName)]
        public static extern int lib_mp3dec_ex_read(IntPtr dec, float[] buf, int samples);

        [DllImport(dllName)]
        public static extern void lib_mp3dec_ex_close(IntPtr dec);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetDllDirectoryW(string lpPathName);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetWindowTextW(IntPtr hWnd, string lpString);
    }
}
