#if defined(_MSC_VER) && (_MSC_VER >= 1900)
#pragma comment(lib, "legacy_stdio_definitions.lib")
#endif

#include <stdio.h>
#include <windows.h>
#include <shlwapi.h>
#include "mono.h"

#pragma comment(lib, "shlwapi.lib")

int main(int argc, char *argv[])
{
    if (argc < 2)
    {
        printf("No arg\n");
        return 1;
    }
    
    wchar_t monoDllPath[MAX_PATH] = {0};
    if (!MonoExists(monoDllPath))
    {
        printf("Couldn't find mono\n");
        return 1;
    }
    
    if (!FileExists(argv[1]))
    {
        printf("Couldn't find '%s'\n", argv[1]);
        return 1;
    }
    
    if (!LoadMono(monoDllPath, argv[1], "MonoRun"))
    {
        printf("Load mono failed. Create mono_log.txt in the current directory and run the command again to see the error log\n");
        return 1;
    }
    
    return 0;
}