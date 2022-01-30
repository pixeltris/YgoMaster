// https://github.com/pixeltris/SonyAlphaUSB/blob/fd69cbb818f3ce0b0ccc01a734c822ecbe9d1147/WIA%20Logger/SonyAlphaUSBLoader.cpp
// cl YgoMasterLoader.cpp /LD /Fe:../Build/YgoMasterLoader.dll (compile with x64 tools)
//
// Notes about detours:
// - This version was used https://github.com/microsoft/Detours/releases/tag/v4.0.1
// - It was compiled by using the nmake command (no args or edits) via "Visual Studio x64 Native Tools Command Prompt"
// - The resulting lib "/lib.X64/detours.lib" and "/include/detours.h" were manually copied into the current directory for usage
#include <windows.h>
#include <wchar.h>
#include <stdio.h>
#include <metahost.h>
#include "detours.h"
#undef _MSC_VER
#include "min_minhook.h"

#pragma comment(lib, "mscoree.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "detours.lib")

#ifdef __cplusplus
#define LIBRARY_API extern "C" __declspec (dllexport)
#else
#define LIBRARY_API __declspec (dllexport)
#endif

typedef HMODULE (WINAPI* Real_LoadLibraryW)(LPCWSTR fileName);
Real_LoadLibraryW Original_LoadLibraryW;
typedef void* (*Real_il2cpp_init)(const char* domain_name);
Real_il2cpp_init Original_il2cpp_init;

BOOL hooksInitialized = FALSE;
BOOL runningLive = FALSE;

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

LIBRARY_API BOOL DetourCreateProcessWithDll_Exported(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes, LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags, LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo, LPPROCESS_INFORMATION lpProcessInformation, LPCSTR lpDllName, PDETOUR_CREATE_PROCESS_ROUTINEA pfCreateProcessA)
{
    return DetourCreateProcessWithDll(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation, lpDllName, pfCreateProcessA);
}

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
    
    wchar_t* binaryPath = L"YgoMasterClient.exe";
    if (!FileExists(binaryPath))
    {
        binaryPath = L"Build\\YgoMasterClient.exe";
        if (!FileExists(binaryPath))
        {
            binaryPath = L"YgoMaster\\YgoMasterClient.exe";
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

void* Mine_il2cpp_init(const char* domain_name)
{
    void* initRes = Original_il2cpp_init(domain_name);
    LoadDotNet();
    return initRes;
}

HMODULE WINAPI Mine_LoadLibraryW(LPCWSTR lpLibFileName)
{
    HMODULE handle = Original_LoadLibraryW(lpLibFileName);
    if (wcsstr(lpLibFileName, L"GameAssembly.dll") != NULL)
    {
        MH_DisableHook(&LoadLibraryW);
        WL_HookFunction(GetProcAddress(handle, "il2cpp_init"), &Mine_il2cpp_init, (LPVOID*)&Original_il2cpp_init);
    }
    return handle;
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
        CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)LoadDotNet, NULL, NULL, NULL);
        return 0;
    }
    
    if (MH_CreateHook(&LoadLibraryW, &Mine_LoadLibraryW, (LPVOID*)&Original_LoadLibraryW) != MH_OK)
    {
        return 1;
    }
    
    if (MH_EnableHook(&LoadLibraryW) != MH_OK)
    {
        return 1;
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