// https://github.com/pixeltris/SonyAlphaUSB/blob/6b459641a2b7fa778e2a8acfa9067c841aca5f96/SonyAlphaUSB/WIALogger.cs#L861
// https://github.com/pixeltris/ModTMNF/blob/26f3953125743c34859bf5e4077047546d970ab5/ModTMNF/TrackmaniaLauncher.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace YgoMasterClient
{
    enum GameLauncherMode
    {
        /// <summary>
        /// This is a great solution but I've previously observed issues with hooking early functions (i.e. WinMain)
        /// </summary>
        Detours,
        /// <summary>
        /// Steals the program entry point to inject very early
        /// </summary>
        StealEntryPoint,
        /// <summary>
        /// Inject into the process instead of launching it
        /// </summary>
        Inject
    }

    class GameLauncher
    {
        public const string TargetProcessName = "masterduel.exe";
        public const string LoaderDll = "YgoMasterLoader.dll";

        public static unsafe bool Launch(GameLauncherMode mode, params string[] args)
        {
            string exeDir = null;
            string dllPath = Path.GetFullPath(LoaderDll);
            string exePath = TargetProcessName;
            try
            {
                if (!File.Exists(dllPath))
                {
                    return false;
                }
                if (!File.Exists(exePath))
                {
                    exePath = Path.Combine("../", exePath);
                }
                if (!File.Exists(exePath))
                {
                    return false;
                }
                exePath = Path.GetFullPath(exePath);
                exeDir = Path.GetFullPath(Path.GetDirectoryName(exePath));
            }
            catch
            {
                return false;
            }

            string commandLine = null;
            if (args != null && args.Length > 0)
            {
                commandLine = string.Empty;
                foreach (string arg in args)
                {
                    if (!string.IsNullOrEmpty(commandLine))
                    {
                        commandLine += " ";
                    }
                    if (arg.Contains(" "))
                    {
                        commandLine += "\"" + arg + "\"";
                    }
                    else
                    {
                        commandLine += arg;
                    }
                }
            }

            if (mode == GameLauncherMode.Inject)
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(TargetProcessName));
                try
                {
                    if (processes.Length == 1)
                    {
                        foreach (ProcessModule module in processes[0].Modules)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(module.ModuleName))
                                {
                                    if (module.ModuleName.Equals(LoaderDll, StringComparison.OrdinalIgnoreCase))
                                    {
                                        System.Windows.Forms.MessageBox.Show("Already injected");
                                        return true;
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                        if (!DllInjector.Inject(processes[0].Handle, dllPath))
                        {
                            System.Windows.Forms.MessageBox.Show("Inject failed");
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Process '" + TargetProcessName + "' not found (or multiple) (open the game or close duplicates)");
                    }
                }
                finally
                {
                    foreach (Process p in processes)
                    {
                        p.Close();
                    }
                }
                return true;
            }

            STARTUPINFO si = default(STARTUPINFO);
            PROCESS_INFORMATION pi = default(PROCESS_INFORMATION);

            if (mode == GameLauncherMode.Detours)
            {
                if (DetourCreateProcessWithDll_Exported(exePath, commandLine, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, exeDir, ref si, out pi, dllPath, IntPtr.Zero) == 0)
                {
                    System.Windows.Forms.MessageBox.Show("DetourCreateProcessWithDll failed " + Marshal.GetLastWin32Error());
                    return false;
                }
                return true;
            }

            try
            {
                bool success = CreateProcess(exePath, commandLine, IntPtr.Zero, IntPtr.Zero, false, DEBUG_ONLY_THIS_PROCESS, IntPtr.Zero, exeDir, ref si, out pi);
                if (!success)
                {
                    return false;
                }

                IntPtr entryPoint = IntPtr.Zero;
                byte[] entryPointInst = new byte[2];

                success = false;
                bool complete = false;
                while (!complete)
                {
                    DEBUG_EVENT debugEvent;
                    if (!WaitForDebugEvent(out debugEvent, 5000))
                    {
                        break;
                    }

                    switch (debugEvent.dwDebugEventCode)
                    {
                        case CREATE_PROCESS_DEBUG_EVENT:
                            {
                                IntPtr hFile = debugEvent.CreateProcessInfo.hFile;
                                if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
                                {
                                    CloseHandle(hFile);
                                }
                            }
                            break;
                        case EXIT_PROCESS_DEBUG_EVENT:
                            complete = true;
                            break;
                        case LOAD_DLL_DEBUG_EVENT:
                            {
                                LOAD_DLL_DEBUG_INFO loadDll = debugEvent.LoadDll;

                                switch (TryStealEntryPoint(ref pi, ref entryPoint, entryPointInst))
                                {
                                    case StealEntryPointResult.FailGetModules:
                                        // Need to wait for more modules to load
                                        break;
                                    case StealEntryPointResult.FailAlloc:
                                    case StealEntryPointResult.FailRead:
                                    case StealEntryPointResult.FailWrite:
                                    case StealEntryPointResult.FailFindTargetModule:
                                        complete = true;
                                        entryPoint = IntPtr.Zero;
                                        break;
                                    case StealEntryPointResult.Success:
                                        complete = true;
                                        break;
                                }

                                IntPtr hFile = loadDll.hFile;
                                if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
                                {
                                    CloseHandle(hFile);
                                }
                            }
                            break;
                    }

                    ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                }

                success = false;

                DebugSetProcessKillOnExit(false);
                DebugActiveProcessStop((int)pi.dwProcessId);

                if (entryPoint != IntPtr.Zero)
                {
                    CONTEXT64 context64 = default(CONTEXT64);
                    context64.ContextFlags = CONTEXT_FLAGS.CONTROL;
                    GetThreadContext(pi.hThread, ref context64);

                    for (int i = 0; i < 100 && context64.Rip != (ulong)entryPoint; i++)
                    {
                        Thread.Sleep(50);

                        context64.ContextFlags = CONTEXT_FLAGS.CONTROL;
                        GetThreadContext(pi.hThread, ref context64);
                    }

                    // If we are at the entry point inject the dll and then restore the entry point instructions
                    if (context64.Rip == (ulong)entryPoint && DllInjector.Inject(pi.hProcess, dllPath))
                    {
                        SuspendThread(pi.hThread);

                        IntPtr byteCount;
                        if (WriteProcessMemory(pi.hProcess, entryPoint, entryPointInst, (IntPtr)2, out byteCount) && (int)byteCount == 2)
                        {
                            success = true;
                        }

                        ResumeThread(pi.hThread);
                    }
                }

                if (!success)
                {
                    TerminateProcess(pi.hProcess, 0);
                }
                return success;
            }
            finally
            {
                if (pi.hThread != IntPtr.Zero)
                {
                    CloseHandle(pi.hThread);
                }
                if (pi.hProcess != IntPtr.Zero)
                {
                    CloseHandle(pi.hProcess);
                }
            }
        }

        private static unsafe StealEntryPointResult TryStealEntryPoint(ref PROCESS_INFORMATION pi, ref IntPtr entryPoint, byte[] entryPointInst)
        {
            int modSize = IntPtr.Size * 1024;
            IntPtr hMods = Marshal.AllocHGlobal(modSize);

            try
            {
                if (hMods == IntPtr.Zero)
                {
                    return StealEntryPointResult.FailAlloc;
                }

                int modsNeeded;
                bool gotZeroMods = false;
                while (!EnumProcessModulesEx(pi.hProcess, hMods, modSize, out modsNeeded, LIST_MODULES_ALL) || modsNeeded == 0)
                {
                    if (modsNeeded == 0)
                    {
                        if (!gotZeroMods)
                        {
                            Thread.Sleep(100);
                            gotZeroMods = true;
                            continue;
                        }
                        else
                        {
                            // process has exited?
                            return StealEntryPointResult.FailGetModules;
                        }
                    }

                    // try again w/ more space...
                    Marshal.FreeHGlobal(hMods);
                    hMods = Marshal.AllocHGlobal(modsNeeded);
                    if (hMods == IntPtr.Zero)
                    {
                        return StealEntryPointResult.FailGetModules;
                    }
                    modSize = modsNeeded;
                }

                int totalNumberofModules = (int)(modsNeeded / IntPtr.Size);
                for (int i = 0; i < totalNumberofModules; i++)
                {
                    IntPtr hModule = Marshal.ReadIntPtr(hMods, i * IntPtr.Size);

                    MODULEINFO moduleInfo;
                    if (GetModuleInformation(pi.hProcess, hModule, out moduleInfo, sizeof(MODULEINFO)))
                    {
                        StringBuilder moduleNameSb = new StringBuilder(1024);
                        if (GetModuleFileNameEx(pi.hProcess, hModule, moduleNameSb, moduleNameSb.Capacity) != 0)
                        {
                            try
                            {
                                string moduleName = Path.GetFileName(moduleNameSb.ToString());
                                if (moduleName.Equals(TargetProcessName, StringComparison.OrdinalIgnoreCase))
                                {
                                    IntPtr byteCount;
                                    if (ReadProcessMemory(pi.hProcess, moduleInfo.EntryPoint, entryPointInst, (IntPtr)2, out byteCount) && (int)byteCount == 2)
                                    {
                                        // TODO: We should probably use VirtualProtect here to ensure read/write/execute

                                        byte[] infLoop = { 0xEB, 0xFE };// JMP -2
                                        if (WriteProcessMemory(pi.hProcess, moduleInfo.EntryPoint, infLoop, (IntPtr)infLoop.Length, out byteCount) &&
                                            (int)byteCount == infLoop.Length)
                                        {
                                            entryPoint = moduleInfo.EntryPoint;
                                            return StealEntryPointResult.Success;
                                        }
                                        else
                                        {
                                            return StealEntryPointResult.FailWrite;
                                        }
                                    }
                                    else
                                    {
                                        return StealEntryPointResult.FailRead;
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                return StealEntryPointResult.FailFindTargetModule;
            }
            finally
            {
                if (hMods != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(hMods);
                }
            }
        }

        enum StealEntryPointResult
        {
            FailAlloc,
            FailGetModules,
            FailFindTargetModule,
            FailRead,
            FailWrite,
            Success,
        }

        [DllImport(LoaderDll, SetLastError = true)]
        static extern int DetourCreateProcessWithDll_Exported(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation, string lpDllName, IntPtr pfCreateProcessA);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
            bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool TerminateProcess(IntPtr hProcess, uint exitCode);

        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool EnumProcessModulesEx([In] IntPtr hProcess, IntPtr lphModule, int cb, [Out] out int lpcbNeeded, int dwFilterFlag);

        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, int cb);

        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WaitForDebugEvent(out DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ContinueDebugEvent(int processId, int threadId, uint continuteStatus);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern void DebugSetProcessKillOnExit(bool killOnExit);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DebugActiveProcessStop(int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Int32 CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, IntPtr size, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static unsafe extern bool GetThreadContext(IntPtr hThread, CONTEXT64* lpContext);

        static unsafe bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext)
        {
            // Hack to align to 16 byte boundry
            byte* buff = stackalloc byte[Marshal.SizeOf(typeof(CONTEXT64)) + 16];
            buff += (ulong)(IntPtr)buff % 16;
            CONTEXT64* ptr = (CONTEXT64*)buff;
            *ptr = lpContext;

            bool result = GetThreadContext(hThread, ptr);
            lpContext = *ptr;
            if (!result && Marshal.GetLastWin32Error() == 998)
            {
                // Align hack failed

            }
            return result;
        }

        [Flags]
        enum ThreadAccess : uint
        {
            Terminate = 0x00001,
            SuspendResume = 0x00002,
            GetContext = 0x00008,
            SetContext = 0x00010,
            SetInformation = 0x00020,
            QueryInformation = 0x00040,
            SetThreadToken = 0x00080,
            Impersonate = 0x00100,
            DirectImpersonation = 0x00200,
            All = 0x1F03FF
        }

        const int DEBUG_ONLY_THIS_PROCESS = 0x00000002;
        const int CREATE_SUSPENDED = 0x00000004;

        const int LIST_MODULES_DEFAULT = 0x00;
        const int LIST_MODULES_32BIT = 0x01;
        const int LIST_MODULES_64BIT = 0x02;
        const int LIST_MODULES_ALL = 0x03;

        const uint CREATE_PROCESS_DEBUG_EVENT = 3;
        const uint EXIT_PROCESS_DEBUG_EVENT = 5;
        const uint LOAD_DLL_DEBUG_EVENT = 6;

        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        const uint DBG_CONTINUE = 0x00010002;

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct DEBUG_EVENT
        {
            [FieldOffset(0)]
            public uint dwDebugEventCode;
            [FieldOffset(4)]
            public int dwProcessId;
            [FieldOffset(8)]
            public int dwThreadId;

            // x64(offset:16, size:164)
            // x86(offset:12, size:86)
            [FieldOffset(16)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 164, ArraySubType = UnmanagedType.U1)]
            public byte[] debugInfo;

            public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo
            {
                get { return GetDebugInfo<CREATE_PROCESS_DEBUG_INFO>(); }
            }

            public LOAD_DLL_DEBUG_INFO LoadDll
            {
                get { return GetDebugInfo<LOAD_DLL_DEBUG_INFO>(); }
            }

            private T GetDebugInfo<T>() where T : struct
            {
                GCHandle handle = GCHandle.Alloc(this.debugInfo, GCHandleType.Pinned);
                T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();
                return result;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LOAD_DLL_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr lpBaseOfDll;
            public uint dwDebugInfoFileOffset;
            public uint nDebugInfoSize;
            public IntPtr lpImageName;
            public ushort fUnicode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CREATE_PROCESS_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr hProcess;
            public IntPtr hThread;
            public IntPtr lpBaseOfImage;
            public uint dwDebugInfoFileOffset;
            public uint nDebugInfoSize;
            public IntPtr lpThreadLocalBase;
            public IntPtr lpStartAddress;
            public IntPtr lpImageName;
            public ushort fUnicode;
        }

        [StructLayout(LayoutKind.Explicit, Size = 1232)]
        unsafe struct CONTEXT64
        {
            // Register Parameter Home Addresses
            [FieldOffset(0x0)]
            internal ulong P1Home;
            [FieldOffset(0x8)]
            internal ulong P2Home;
            [FieldOffset(0x10)]
            internal ulong P3Home;
            [FieldOffset(0x18)]
            internal ulong P4Home;
            [FieldOffset(0x20)]
            internal ulong P5Home;
            [FieldOffset(0x28)]
            internal ulong P6Home;
            // Control Flags
            [FieldOffset(0x30)]
            internal CONTEXT_FLAGS ContextFlags;
            [FieldOffset(0x34)]
            internal uint MxCsr;
            // Segment Registers and Processor Flags
            [FieldOffset(0x38)]
            internal ushort SegCs;
            [FieldOffset(0x3a)]
            internal ushort SegDs;
            [FieldOffset(0x3c)]
            internal ushort SegEs;
            [FieldOffset(0x3e)]
            internal ushort SegFs;
            [FieldOffset(0x40)]
            internal ushort SegGs;
            [FieldOffset(0x42)]
            internal ushort SegSs;
            [FieldOffset(0x44)]
            internal uint EFlags;
            // Debug Registers
            [FieldOffset(0x48)]
            internal ulong Dr0;
            [FieldOffset(0x50)]
            internal ulong Dr1;
            [FieldOffset(0x58)]
            internal ulong Dr2;
            [FieldOffset(0x60)]
            internal ulong Dr3;
            [FieldOffset(0x68)]
            internal ulong Dr6;
            [FieldOffset(0x70)]
            internal ulong Dr7;
            // Integer Registers
            [FieldOffset(0x78)]
            internal ulong Rax;
            [FieldOffset(0x80)]
            internal ulong Rcx;
            [FieldOffset(0x88)]
            internal ulong Rdx;
            [FieldOffset(0x90)]
            internal ulong Rbx;
            [FieldOffset(0x98)]
            internal ulong Rsp;
            [FieldOffset(0xa0)]
            internal ulong Rbp;
            [FieldOffset(0xa8)]
            internal ulong Rsi;
            [FieldOffset(0xb0)]
            internal ulong Rdi;
            [FieldOffset(0xb8)]
            internal ulong R8;
            [FieldOffset(0xc0)]
            internal ulong R9;
            [FieldOffset(0xc8)]
            internal ulong R10;
            [FieldOffset(0xd0)]
            internal ulong R11;
            [FieldOffset(0xd8)]
            internal ulong R12;
            [FieldOffset(0xe0)]
            internal ulong R13;
            [FieldOffset(0xe8)]
            internal ulong R14;
            [FieldOffset(0xf0)]
            internal ulong R15;
            // Program Counter
            [FieldOffset(0xf8)]
            internal ulong Rip;
            // Floating Point State
            [FieldOffset(0x100)]
            internal ulong FltSave;
            [FieldOffset(0x120)]
            internal ulong Legacy;
            [FieldOffset(0x1a0)]
            internal ulong Xmm0;
            [FieldOffset(0x1b0)]
            internal ulong Xmm1;
            [FieldOffset(0x1c0)]
            internal ulong Xmm2;
            [FieldOffset(0x1d0)]
            internal ulong Xmm3;
            [FieldOffset(0x1e0)]
            internal ulong Xmm4;
            [FieldOffset(0x1f0)]
            internal ulong Xmm5;
            [FieldOffset(0x200)]
            internal ulong Xmm6;
            [FieldOffset(0x210)]
            internal ulong Xmm7;
            [FieldOffset(0x220)]
            internal ulong Xmm8;
            [FieldOffset(0x230)]
            internal ulong Xmm9;
            [FieldOffset(0x240)]
            internal ulong Xmm10;
            [FieldOffset(0x250)]
            internal ulong Xmm11;
            [FieldOffset(0x260)]
            internal ulong Xmm12;
            [FieldOffset(0x270)]
            internal ulong Xmm13;
            [FieldOffset(0x280)]
            internal ulong Xmm14;
            [FieldOffset(0x290)]
            internal ulong Xmm15;
            // Vector Registers
            [FieldOffset(0x300)]
            internal ulong VectorRegister;
            [FieldOffset(0x4a0)]
            internal ulong VectorControl;
            // Special Debug Control Registers
            [FieldOffset(0x4a8)]
            internal ulong DebugControl;
            [FieldOffset(0x4b0)]
            internal ulong LastBranchToRip;
            [FieldOffset(0x4b8)]
            internal ulong LastBranchFromRip;
            [FieldOffset(0x4c0)]
            internal ulong LastExceptionToRip;
            [FieldOffset(0x4c8)]
            internal ulong LastExceptionFromRip;
        }

        [Flags]
        enum CONTEXT_FLAGS : uint
        {
            i386 = 0x10000,
            i486 = 0x10000,   //  same as i386
            CONTROL = i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
            INTEGER = i386 | 0x02, // AX, BX, CX, DX, SI, DI
            SEGMENTS = i386 | 0x04, // DS, ES, FS, GS
            FLOATING_POINT = i386 | 0x08, // 387 state
            DEBUG_REGISTERS = i386 | 0x10, // DB 0-3,6,7
            EXTENDED_REGISTERS = i386 | 0x20, // cpu specific extensions
            FULL = CONTROL | INTEGER | SEGMENTS,
            ALL = CONTROL | INTEGER | SEGMENTS | FLOATING_POINT | DEBUG_REGISTERS | EXTENDED_REGISTERS
        }

        static class DllInjector
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern Int32 CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, IntPtr size, out IntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern uint WaitForSingleObject(IntPtr hObject, uint dwMilliseconds);

            enum ThreadWaitValue : uint
            {
                Object0 = 0x00000000,
                Abandoned = 0x00000080,
                Timeout = 0x00000102,
                Failed = 0xFFFFFFFF,
                Infinite = 0xFFFFFFFF
            }

            const uint MEM_COMMIT = 0x1000;
            const uint MEM_RESERVE = 0x2000;
            const uint MEM_RELEASE = 0x8000;

            const uint PAGE_EXECUTE = 0x10;
            const uint PAGE_EXECUTE_READ = 0x20;
            const uint PAGE_EXECUTE_READWRITE = 0x40;
            const uint PAGE_EXECUTE_WRITECOPY = 0x80;
            const uint PAGE_NOACCESS = 0x01;

            public static bool Inject(Process process, string dllPath)
            {
                bool result = false;
                IntPtr hProcess = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, process.Id);
                if (hProcess != IntPtr.Zero)
                {
                    result = Inject(hProcess, dllPath);
                    CloseHandle(hProcess);
                }
                return result;
            }

            public static bool Inject(IntPtr process, string dllPath)
            {
                if (process == IntPtr.Zero)
                {
                    LogError("Process handle is 0");
                    return false;
                }

                if (!File.Exists(dllPath))
                {
                    LogError("Couldn't find the dll to inject (" + dllPath + ")");
                    return false;
                }

                byte[] buffer = Encoding.Unicode.GetBytes(dllPath + "\0");

                IntPtr libAddr = IntPtr.Zero;
                IntPtr memAddr = IntPtr.Zero;
                IntPtr threadAddr = IntPtr.Zero;

                try
                {
                    if (process == IntPtr.Zero)
                    {
                        LogError("Unable to attach to process");
                        return false;
                    }

                    libAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                    if (libAddr == IntPtr.Zero)
                    {
                        LogError("Unable to find address of LoadLibraryW");
                        return false;
                    }

                    memAddr = VirtualAllocEx(process, IntPtr.Zero, (IntPtr)buffer.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
                    if (memAddr == IntPtr.Zero)
                    {
                        LogError("Unable to allocate memory in the target process");
                        return false;
                    }

                    IntPtr bytesWritten;
                    if (!WriteProcessMemory(process, memAddr, buffer, (IntPtr)buffer.Length, out bytesWritten) ||
                        (int)bytesWritten != buffer.Length)
                    {
                        LogError("Unable to write to target process memory");
                        return false;
                    }

                    IntPtr thread = CreateRemoteThread(process, IntPtr.Zero, IntPtr.Zero, libAddr, memAddr, 0, IntPtr.Zero);
                    if (thread == IntPtr.Zero)
                    {
                        LogError("Unable to start thread in target process");
                        return false;
                    }

                    if (WaitForSingleObject(thread, (uint)ThreadWaitValue.Infinite) != (uint)ThreadWaitValue.Object0)
                    {
                        LogError("Failed to wait on the target thread");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    if (threadAddr != IntPtr.Zero)
                    {
                        CloseHandle(threadAddr);
                    }
                    if (memAddr != IntPtr.Zero)
                    {
                        VirtualFreeEx(process, memAddr, IntPtr.Zero, MEM_RELEASE);
                    }
                }
            }

            private static void LogError(string str)
            {
                string error = "DllInjector error: " + str + " - ErrorCode: " + Marshal.GetLastWin32Error();
                Console.WriteLine(error);
                System.Diagnostics.Debug.WriteLine(error);
            }
        }
    }
}
