// Compile using Visual Studio x64 Native Tools Command Prompt
// cl YgoMasterLoader.cpp /LD /DWITHDETOURS /Fe:../YgoMaster/YgoMasterLoader.dll
//
// Notes about detours:
// - This version was used https://github.com/microsoft/Detours/releases/tag/v4.0.1
// - It was compiled by using the nmake command (no args or edits) via "Visual Studio x64 Native Tools Command Prompt"
// - The resulting lib "/lib.X64/detours.lib" and "/include/detours.h" were manually copied into the current directory for usage
//
// This code was based on:
// https://github.com/pixeltris/SonyAlphaUSB/blob/fd69cbb818f3ce0b0ccc01a734c822ecbe9d1147/WIA%20Logger/SonyAlphaUSBLoader.cpp

// VS2015+ (see https://docs.microsoft.com/en-us/cpp/porting/visual-cpp-change-history-2003-2015?redirectedfrom=MSDN&view=msvc-170#stdio_and_conio)
#if defined(_MSC_VER) && (_MSC_VER >= 1900)
#pragma comment(lib, "legacy_stdio_definitions.lib")
#endif

#include <windows.h>
#include <wchar.h>
#include <stdio.h>
#include <metahost.h>
#include <shlwapi.h>
#if WITHDETOURS
#include "detours.h"
#endif
#undef _MSC_VER
#include "min_minhook.h"

#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "mscoree.lib")
#pragma comment(lib, "user32.lib")
#if WITHDETOURS
#pragma comment(lib, "detours.lib")
#endif

#ifdef __cplusplus
#define LIBRARY_API extern "C" __declspec (dllexport)
#else
#define LIBRARY_API __declspec (dllexport)
#endif

typedef int (__thiscall* Sig_SetVSyncCount)(void* thisPtr, int value);
Sig_SetVSyncCount Original_SetVSyncCount;
typedef BOOL (WINAPI* Sig_QueryPerformanceCounter)(LARGE_INTEGER* lpPerformanceCount);
Sig_QueryPerformanceCounter Original_QueryPerformanceCounter;
typedef HMODULE (WINAPI* Sig_LoadLibraryW)(LPCWSTR fileName);
Sig_LoadLibraryW Original_LoadLibraryW;
typedef void* (*Sig_il2cpp_init)(const char* domain_name);
Sig_il2cpp_init Original_il2cpp_init;

BOOL hooksInitialized = FALSE;
BOOL runningLive = FALSE;
LARGE_INTEGER lastTime = {0};
LARGE_INTEGER fakeTime = {0};
double timeMultiplier = 1.0;

BOOL FileExists(LPCWSTR szPath)
{
    DWORD dwAttrib = GetFileAttributesW(szPath);
    return (dwAttrib != INVALID_FILE_ATTRIBUTES && !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
}

LIBRARY_API MH_STATUS WL_InitHooks()
{
	if (!hooksInitialized)
	{
		hooksInitialized = TRUE;
		return MH_Initialize();
	}
	return MH_OK;
}

LIBRARY_API MH_STATUS WL_HookFunction(LPVOID target, LPVOID detour, LPVOID* original)
{
	MH_STATUS status = MH_CreateHook(target, detour, original);
	if (status == MH_OK)
	{
		return MH_EnableHook(target);
	}
	return status;
}

LIBRARY_API MH_STATUS WL_CreateHook(LPVOID target, LPVOID detour, LPVOID* original)
{
	return MH_CreateHook(target, detour, original);
}

LIBRARY_API MH_STATUS WL_RemoveHook(LPVOID target)
{
	return MH_RemoveHook(target);
}

LIBRARY_API MH_STATUS WL_EnableHook(LPVOID target)
{
	return MH_EnableHook(target);
}

LIBRARY_API MH_STATUS WL_DisableHook(LPVOID target)
{
	return MH_DisableHook(target);
}

LIBRARY_API MH_STATUS WL_EnableAllHooks(BOOL enable)
{
    return EnableAllHooksLL(enable);
}

#if WITHDETOURS
LIBRARY_API BOOL DetourCreateProcessWithDll_Exported(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes, LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags, LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo, LPPROCESS_INFORMATION lpProcessInformation, LPCSTR lpDllName, PDETOUR_CREATE_PROCESS_ROUTINEA pfCreateProcessA)
{
    return DetourCreateProcessWithDll(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation, lpDllName, pfCreateProcessA);
}
#endif

HRESULT LoadDotNetImpl()
{
    ICLRMetaHost* metaHost;
    ICLRRuntimeHost* runtimeHost;
    ICLRRuntimeInfo *runtimeInfo;
    HRESULT result = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&metaHost);
    if (!SUCCEEDED(result))
    {
        return result;
    }
    
    result = metaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&runtimeInfo);
    if (!SUCCEEDED(result))
    {
        return result;
    }
    
    result = runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&runtimeHost);
    if (!SUCCEEDED(result))
    {
        return result;
    }
    
    result = runtimeHost->Start();
    if (!SUCCEEDED(result))
    {
        return result;
    }
    
    result = runtimeInfo->BindAsLegacyV2Runtime();
    if (!SUCCEEDED(result))
    {
        return result;
    }

    wchar_t binaryPath[MAX_PATH] = {0};
    
    HMODULE hm;
    if(GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPWSTR)&LoadDotNetImpl, &hm))
    {
        wchar_t dllPath[MAX_PATH] = {0};
        GetModuleFileNameW(hm, dllPath, MAX_PATH);
        if (wcslen(dllPath) > 0 && FileExists(dllPath))
        {
            PathRemoveFileSpecW(dllPath);
            wcscpy(binaryPath, dllPath);
            wcscat(binaryPath, L"\\YgoMasterClient.exe");
        }
    }
    if (!FileExists(binaryPath))
    {
        return 0x1333337;
    }
    result = runtimeHost->ExecuteInDefaultAppDomain(binaryPath, L"YgoMasterClient.Program", L"DllMain", runningLive ? L"live" : NULL, NULL);
    return result;
}

void LoadDotNet()
{
    HRESULT res = LoadDotNetImpl();
    if (res)
    {
        char buffer[1024];
        sprintf(buffer, "LoadDotNet failed with 0x%lx", res);
        MessageBox(0, buffer, 0, 0);
        TerminateProcess(GetCurrentProcess(), 0);
    }
}

void* Hook_il2cpp_init(const char* domain_name)
{
    void* initRes = Original_il2cpp_init(domain_name);
    LoadDotNet();
    return initRes;
}

HMODULE WINAPI Hook_LoadLibraryW(LPCWSTR lpLibFileName)
{
    HMODULE handle = Original_LoadLibraryW(lpLibFileName);
    if (wcsstr(lpLibFileName, L"GameAssembly.dll") != NULL)
    {
        MH_RemoveHook(&LoadLibraryW);
        WL_HookFunction(GetProcAddress(handle, "il2cpp_init"), &Hook_il2cpp_init, (LPVOID*)&Original_il2cpp_init);
    }
    return handle;
}

BOOL Hook_QueryPerformanceCounter(LARGE_INTEGER* lpPerformanceCount)
{
    BOOL result = Original_QueryPerformanceCounter(lpPerformanceCount);
    LONGLONG diff = (LONGLONG)((lpPerformanceCount->QuadPart - lastTime.QuadPart) * timeMultiplier);
    fakeTime.QuadPart += diff;
    lastTime.QuadPart = lpPerformanceCount->QuadPart;
    lpPerformanceCount->QuadPart = fakeTime.QuadPart;
    return result;
}

LIBRARY_API void SetTimeMultiplier(double value)
{
    timeMultiplier = value != 0 ? value : 1;
}

int Hook_SetVSyncCount(void* thisPtr, int value)
{
    value = 0;
    return Original_SetVSyncCount(thisPtr, value);
}

LIBRARY_API void CreateVSyncHook(void* funcPtr)
{
    MH_CreateHook(funcPtr, &Hook_SetVSyncCount, (LPVOID*)&Original_SetVSyncCount);
    MH_EnableHook(funcPtr);
}

int CreateHooks()
{
    if (MH_Initialize() != MH_OK)    
    {
        return 1;
    }
    
    if (GetModuleHandleW(L"GameAssembly.dll"))
    {
        runningLive = true;
    }
    else if (MH_CreateHook(&LoadLibraryW, &Hook_LoadLibraryW, (LPVOID*)&Original_LoadLibraryW) != MH_OK)
    {
        return 1;
    }
    
    QueryPerformanceCounter(&lastTime);
    fakeTime = lastTime;
    if (MH_CreateHook(&QueryPerformanceCounter, &Hook_QueryPerformanceCounter, (LPVOID*)&Original_QueryPerformanceCounter) != MH_OK)
    {
        return 1;
    }
    
    if (EnableAllHooksLL(true) != MH_OK)
    {
        return 1;
    }
    
    if (runningLive)
    {
        CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)LoadDotNet, NULL, NULL, NULL);
    }
    
    return 0;
}

BOOL WINAPI DllMain(HINSTANCE hDll, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason)
    {
        case DLL_PROCESS_ATTACH:
            DisableThreadLibraryCalls(hDll);
            CreateHooks();
            break;
    }
    return TRUE;
}