#ifdef WITH_MONO
#define MONO_VERBOSE_LOGGING
typedef enum
{
	MONO_DEBUG_FORMAT_NONE,
	MONO_DEBUG_FORMAT_MONO,
	/* Deprecated, the mdb debugger is not longer supported. */
	MONO_DEBUG_FORMAT_DEBUGGER
} MonoDebugFormat;

typedef int int32;
typedef int32 mono_bool;
typedef void MonoDomain;
typedef void MonoAssembly;
typedef void MonoImage;
typedef void MonoMethodDesc;
typedef void MonoMethod;
typedef void MonoObject;
typedef void MonoString;
typedef void MonoArray;
typedef int32 gint32;

struct _MonoObject
{
	void *vtable;
	void *synchronisation;
};

// TODO: Validate this structure is still up to date for the latest version of mono
struct MonoException
{
	_MonoObject object;
	MonoString *class_name;
	MonoString *message;
	MonoObject *_data;
	MonoObject *inner_ex;
	MonoString *help_link;
	/* Stores the IPs and the generic sharing infos
	(vtable/MRGCTX) of the frames. */
	MonoArray  *trace_ips;
	MonoString *stack_trace;
	MonoString *remote_stack_trace;
	gint32	    remote_stack_index;
	/* Dynamic methods referenced by the stack trace */
	MonoObject *dynamic_methods;
	gint32	    hresult;
	MonoString *source;
	MonoObject *serialization_manager;
	MonoObject *captured_traces;
	MonoArray  *native_trace_ips;
	gint32 caught_in_unmanaged;
};

typedef void(*MonoPrintCallback)(const char* str, mono_bool is_stdout);
typedef void(*MonoLogCallback)(const char* log_domain, const char* log_level, const char* message, mono_bool fatal, void* user_data);
typedef void*(*MonoDlFallbackLoad) (const char *name, int flags, char **err, void *user_data);
typedef void*(*MonoDlFallbackSymbol) (void *handle, const char *name, char **err, void *user_data);
typedef void*(*MonoDlFallbackClose) (void *handle, void *user_data);

typedef void(*import__mono_config_parse)(const char* filename);
typedef void(*import__mono_domain_set_config)(MonoDomain *domain, const char* base_dir, const char* config_file_name);
typedef void(*import__mono_set_dirs)(const char* assembly_dir, const char* config_dir);
typedef void(*import__mono_debug_init)(MonoDebugFormat format);
typedef MonoDomain*(*import__mono_jit_init_version)(const char* domain_name, const char* runtime_version);
typedef MonoAssembly*(*import__mono_domain_assembly_open)(MonoDomain* domain, const char* name);
typedef MonoImage*(*import__mono_assembly_get_image)(MonoAssembly* assembly);
typedef MonoMethodDesc*(*import__mono_method_desc_new)(const char* name, mono_bool include_namespace);
typedef MonoMethod*(*import__mono_method_desc_search_in_image)(MonoMethodDesc* desc, MonoImage* image);
typedef void(*import__mono_method_desc_free)(MonoMethodDesc* desc);
typedef MonoObject*(*import__mono_runtime_invoke)(MonoMethod* method, void* obj, void** params, MonoException** exc);
typedef void*(*import__mono_object_unbox)(MonoObject* obj);
typedef MonoString*(*import__mono_string_new)(MonoDomain *domain, const char *text);
typedef char*(*import__mono_string_to_utf8)(MonoString* string_obj);
typedef void(*import__mono_trace_init)();
typedef void(*import__mono_trace_set_level_string)(const char* value);
typedef void(*import__mono_trace_set_mask_string)(const char* value);
typedef void(*import__mono_trace_set_log_handler)(MonoLogCallback callback, void* user_data);
typedef void(*import__mono_trace_set_print_handler)(MonoPrintCallback callback);
typedef void(*import__mono_trace_set_printerr_handler)(MonoPrintCallback callback);
typedef void*(*import__mono_dl_fallback_register)(MonoDlFallbackLoad load_func, MonoDlFallbackSymbol symbol_func, MonoDlFallbackClose close_func, void *user_data);

import__mono_config_parse mono_config_parse;
import__mono_domain_set_config mono_domain_set_config;
import__mono_set_dirs mono_set_dirs;
import__mono_debug_init mono_debug_init;
import__mono_jit_init_version mono_jit_init_version;
import__mono_domain_assembly_open mono_domain_assembly_open;
import__mono_assembly_get_image mono_assembly_get_image;
import__mono_method_desc_new mono_method_desc_new;
import__mono_method_desc_search_in_image mono_method_desc_search_in_image;
import__mono_method_desc_free mono_method_desc_free;
import__mono_runtime_invoke mono_runtime_invoke;
import__mono_object_unbox mono_object_unbox;
import__mono_string_new mono_string_new;
import__mono_string_to_utf8 mono_string_to_utf8;
import__mono_trace_init mono_trace_init;
import__mono_trace_set_level_string mono_trace_set_level_string;
import__mono_trace_set_mask_string mono_trace_set_mask_string;
import__mono_trace_set_log_handler mono_trace_set_log_handler;
import__mono_trace_set_print_handler mono_trace_set_print_handler;
import__mono_trace_set_printerr_handler mono_trace_set_printerr_handler;
import__mono_dl_fallback_register mono_dl_fallback_register;

#ifdef MONO_VERBOSE_LOGGING
BOOL monoLogEnabled = false;
#define MONO_LOG_FILE "mono_log.txt"
void MonoLog(const char* str)
{
    if (!monoLogEnabled)
    {
        return;
    }
    FILE *file;
    errno_t err = fopen_s(&file, MONO_LOG_FILE, "a+");
    if (!err)
    {
        fprintf(file, "%s", str);
        fclose(file);
    }
}

void OnMonoLog(const char* log_domain, const char* log_level, const char* message, mono_bool fatal, void* user_data)
{
    if (!monoLogEnabled)
    {
        return;
    }
    char buff[1024] = {0};
    sprintf_s(buff, sizeof(buff), "log_domain:%s log_level:%s msg:%s fatal:%d\n", log_domain, log_level, message, fatal);
    MonoLog(buff);
}

void OnMonoPrint(const char* str, mono_bool is_stdout)
{
    if (!monoLogEnabled)
    {
        return;
    }
    char buff[1024] = {0};
    sprintf_s(buff, sizeof(buff), "%s\n", str);// TODO: Confirm we want the \n here
    MonoLog(buff);
}
#endif

BOOL FileExists(LPCSTR szPath)
{
    DWORD dwAttrib = GetFileAttributes(szPath);
    return (dwAttrib != INVALID_FILE_ATTRIBUTES && !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
}

BOOL LoadMono(wchar_t* monoDllPath, BOOL runningLive)
{
    monoLogEnabled = FileExists(MONO_LOG_FILE);
    MonoLog("Loading mono dll\n");
    HMODULE dllHandle = LoadLibraryW(monoDllPath);
    if (dllHandle == NULL)
    {
        MonoLog("Failed to load mono dll\n");
        return false;
    }
    
	mono_config_parse = (import__mono_config_parse)GetProcAddress(dllHandle, TEXT("mono_config_parse"));
	mono_domain_set_config = (import__mono_domain_set_config)GetProcAddress(dllHandle, TEXT("mono_domain_set_config"));
	mono_set_dirs = (import__mono_set_dirs)GetProcAddress(dllHandle, TEXT("mono_set_dirs"));
	mono_debug_init = (import__mono_debug_init)GetProcAddress(dllHandle, TEXT("mono_debug_init"));
	mono_jit_init_version = (import__mono_jit_init_version)GetProcAddress(dllHandle, TEXT("mono_jit_init_version"));
	mono_domain_assembly_open = (import__mono_domain_assembly_open)GetProcAddress(dllHandle, TEXT("mono_domain_assembly_open"));
	mono_assembly_get_image = (import__mono_assembly_get_image)GetProcAddress(dllHandle, TEXT("mono_assembly_get_image"));
	mono_method_desc_new = (import__mono_method_desc_new)GetProcAddress(dllHandle, TEXT("mono_method_desc_new"));
	mono_method_desc_search_in_image = (import__mono_method_desc_search_in_image)GetProcAddress(dllHandle, TEXT("mono_method_desc_search_in_image"));
	mono_method_desc_free = (import__mono_method_desc_free)GetProcAddress(dllHandle, TEXT("mono_method_desc_free"));
	mono_runtime_invoke = (import__mono_runtime_invoke)GetProcAddress(dllHandle, TEXT("mono_runtime_invoke"));
	mono_object_unbox = (import__mono_object_unbox)GetProcAddress(dllHandle, TEXT("mono_object_unbox"));
	mono_string_new = (import__mono_string_new)GetProcAddress(dllHandle, TEXT("mono_string_new"));
	mono_string_to_utf8 = (import__mono_string_to_utf8)GetProcAddress(dllHandle, TEXT("mono_string_to_utf8"));
	mono_trace_init = (import__mono_trace_init)GetProcAddress(dllHandle, TEXT("mono_trace_init"));
	mono_trace_set_level_string = (import__mono_trace_set_level_string)GetProcAddress(dllHandle, TEXT("mono_trace_set_level_string"));
	mono_trace_set_mask_string = (import__mono_trace_set_mask_string)GetProcAddress(dllHandle, TEXT("mono_trace_set_mask_string"));
	mono_trace_set_log_handler = (import__mono_trace_set_log_handler)GetProcAddress(dllHandle, TEXT("mono_trace_set_log_handler"));
	mono_trace_set_print_handler = (import__mono_trace_set_print_handler)GetProcAddress(dllHandle, TEXT("mono_trace_set_print_handler"));
	mono_trace_set_printerr_handler = (import__mono_trace_set_printerr_handler)GetProcAddress(dllHandle, TEXT("mono_trace_set_printerr_handler"));
	mono_dl_fallback_register = (import__mono_dl_fallback_register)GetProcAddress(dllHandle, TEXT("mono_dl_fallback_register"));

	if (mono_config_parse == NULL ||
		mono_domain_set_config == NULL ||
		mono_set_dirs == NULL ||
		mono_debug_init == NULL ||
		mono_jit_init_version == NULL ||
		mono_domain_assembly_open == NULL ||
		mono_assembly_get_image == NULL ||
		mono_method_desc_new == NULL ||
		mono_method_desc_search_in_image == NULL ||
		mono_method_desc_free == NULL ||
		mono_runtime_invoke == NULL ||
		mono_object_unbox == NULL ||
		mono_string_new == NULL ||
		mono_string_to_utf8 == NULL ||
		mono_trace_init == NULL ||
		mono_trace_set_level_string == NULL ||
		mono_trace_set_mask_string == NULL ||
		mono_trace_set_log_handler == NULL ||
		mono_trace_set_print_handler == NULL ||
		mono_trace_set_printerr_handler == NULL ||
		mono_dl_fallback_register == NULL)
	{
        MonoLog("Failed to find mono functions\n");
		return false;
	}
    
#ifdef MONO_VERBOSE_LOGGING
	// mono_trace_init() doesn't seem to print anything on MacOS (regardless of environment variables)
	mono_trace_init();

    mono_trace_set_level_string("debug");// error, critical, warning, message, info, debug
    mono_trace_set_mask_string("all");// all, asm, type, dll, gc, cfg, aot, security
    mono_trace_set_log_handler(OnMonoLog, NULL);
    mono_trace_set_print_handler(OnMonoPrint);
    mono_trace_set_printerr_handler(OnMonoPrint);
#endif

    HMODULE hm;
    if(!GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPWSTR)&LoadMono, &hm))
    {
        return false;
    }
    
    // TODO: Figure out how to support wchar_t under mono?
    char dllPath[MAX_PATH] = {0};
    GetModuleFileName(hm, dllPath, MAX_PATH);
    if (strlen(dllPath) <= 0)
    {
        return false;
    }
    PathRemoveFileSpec(dllPath);
    
    char assemblyDir[MAX_PATH] = {0};
    strcpy(assemblyDir, dllPath);
    strcat(assemblyDir, "\\mono\\lib");
    
    char configDir[MAX_PATH] = {0};
    strcpy(configDir, dllPath);
    strcat(configDir, "\\mono\\etc");
    
    char targetAssemblyPath[MAX_PATH] = {0};
    strcpy(targetAssemblyPath, dllPath);
    strcat(targetAssemblyPath, "\\YgoMasterClient.exe");

    mono_set_dirs(assemblyDir, configDir);
    //mono_debug_init(MONO_DEBUG_FORMAT_MONO);
    mono_config_parse(NULL);

    MonoDomain* monoDomain = mono_jit_init_version("DefaultDomain", "v4.0.30319");

    // Workaround to avoid this exception:
	// System.Configuration.ConfigurationErrorsException: Error Initializing the configuration system.
	// ---> System.ArgumentException: The 'ExeConfigFilename' argument cannot be null.
	//mono_domain_set_config(monoDomain, ".", "");

    MonoAssembly* assembly = mono_domain_assembly_open((MonoDomain*)monoDomain, targetAssemblyPath);
    if (assembly == NULL)
    {
        MonoLog("mono_domain_assembly_open failed\n");
        return false;
    }
    MonoImage* image = mono_assembly_get_image(assembly);
    if (image == NULL)
    {
        MonoLog("mono_assembly_get_image failed\n");
        return false;
    }
    MonoMethodDesc* methodDesc = mono_method_desc_new("YgoMasterClient.Program:DllMain", true);
    if (methodDesc == NULL)
    {
        MonoLog("mono_method_desc_new failed\n");
        return false;
    }
    MonoMethod* method = mono_method_desc_search_in_image(methodDesc, image);
    if (method == NULL)
    {
        MonoLog("mono_method_desc_search_in_image failed\n");
        return false;
    }
    mono_method_desc_free(methodDesc);
    void* args = { (void*)mono_string_new((MonoDomain*)monoDomain, runningLive ? "live" : "") };
    MonoException* exception = NULL;
    MonoObject* result = mono_runtime_invoke(method, NULL, &args, &exception);
    int32 retVal = 0;
    if (exception != NULL || (retVal = *(int32*)mono_object_unbox(result)) != 0)
    {
        MonoLog("Error when running YgoMasterClient.Program:DllMain\n");
        while (exception != NULL)
        {
            MonoLog(mono_string_to_utf8(exception->message));
            MonoLog("\n");
            exception = (MonoException*)exception->inner_ex;
        }
        MonoLog("\n");
    }
    return true;
}

#endif