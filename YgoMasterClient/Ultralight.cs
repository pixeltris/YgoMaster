using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace YgoMasterClient
{
    unsafe static class Ultralight
    {
        public static bool EnableLogging = true;

        public static bool IsLoaded;
        public static uint Width { get; private set; }
        public static uint Height { get; private set; }
        static IntPtr view;
        static IntPtr renderer;
        static IntPtr surface;
        static IntPtr bitmap;

        public static string DllDir
        {
            get { return Path.Combine(Program.CurrentDir, "Ultralight"); }
        }

        public static void Load(uint width, uint height)
        {
            if (IsLoaded)
            {
                return;
            }

            string dllDir = DllDir;
            const string dllAppCoreString = "AppCore.dll";
            const string dllUltralightString = "Ultralight.dll";
            const string dllUltralightCoreString = "UltralightCore.dll";
            const string dllWebCoreString = "WebCore.dll";
            Dictionary<string, IntPtr> dlls = new Dictionary<string, IntPtr>()
            {
                { dllAppCoreString, IntPtr.Zero },
                { dllUltralightString, IntPtr.Zero },
                { dllUltralightCoreString, IntPtr.Zero },
                { dllWebCoreString, IntPtr.Zero }
            };

            if (!Directory.Exists(dllDir))
            {
                return;
            }
            if (!File.Exists(Path.Combine(dllDir, dlls.First().Key)))
            {
                return;
            }
            PInvoke.SetDllDirectoryW(dllDir);
            foreach (KeyValuePair<string, IntPtr> dll in new Dictionary<string, IntPtr>(dlls))
            {
                dlls[dll.Key] = PInvoke.LoadLibrary(dll.Key);
                if (dlls[dll.Key] == IntPtr.Zero)
                {
                    throw new Exception("Failed to load " + Path.Combine(dllDir, dll.Key) + " " + Marshal.GetLastWin32Error());
                }
            }
            PInvoke.SetDllDirectoryW(null);

            IntPtr dllAppCore = dlls[dllAppCoreString];
            IntPtr dllUltralight = dlls[dllUltralightString];
            IntPtr dllWebCore = dlls[dllWebCoreString];

            GetFunc(dllAppCore, out ulEnablePlatformFontLoader);
            GetFunc(dllAppCore, out ulEnablePlatformFileSystem);
            GetFunc(dllAppCore, out ulEnableDefaultLogger);

            GetFunc(dllUltralight, out ulPlatformSetClipboard);
            GetFunc(dllUltralight, out ulCreateConfig);
            GetFunc(dllUltralight, out ulDestroyConfig);
            GetFunc(dllUltralight, out ulConfigSetResourcePathPrefix);
            GetFunc(dllUltralight, out ulCreateStringUTF16);
            GetFunc(dllUltralight, out ulStringAssignString);
            GetFunc(dllUltralight, out ulDestroyString);
            GetFunc(dllUltralight, out ulStringGetData);
            GetFunc(dllUltralight, out ulStringGetLength);
            GetFunc(dllUltralight, out ulCreateRenderer);
            GetFunc(dllUltralight, out ulCreateViewConfig);
            GetFunc(dllUltralight, out ulDestroyViewConfig);
            GetFunc(dllUltralight, out ulViewConfigSetInitialDeviceScale);
            GetFunc(dllUltralight, out ulViewConfigSetIsAccelerated);
            GetFunc(dllUltralight, out ulViewConfigSetIsTransparent);
            GetFunc(dllUltralight, out ulCreateView);
            GetFunc(dllUltralight, out ulViewResize);
            GetFunc(dllUltralight, out ulViewSetDeviceScale);

            GetFunc(dllUltralight, out ulViewSetChangeTitleCallback);
            GetFunc(dllUltralight, out ulViewSetAddConsoleMessageCallback);
            GetFunc(dllUltralight, out ulViewSetDOMReadyCallback);
            GetFunc(dllUltralight, out ulViewSetBeginLoadingCallback);
            GetFunc(dllUltralight, out ulViewSetFinishLoadingCallback);
            GetFunc(dllUltralight, out ulViewLoadURL);
            GetFunc(dllUltralight, out ulViewEvaluateScript);
            GetFunc(dllUltralight, out ulViewLockJSContext);
            GetFunc(dllUltralight, out ulViewUnlockJSContext);
            GetFunc(dllUltralight, out ulViewFireKeyEvent);
            GetFunc(dllUltralight, out ulViewFireMouseEvent);
            GetFunc(dllUltralight, out ulViewFireScrollEvent);
            GetFunc(dllUltralight, out ulCreateScrollEvent);
            GetFunc(dllUltralight, out ulDestroyScrollEvent);
            GetFunc(dllUltralight, out ulCreateKeyEvent);
            GetFunc(dllUltralight, out ulDestroyKeyEvent);
            GetFunc(dllUltralight, out ulCreateMouseEvent);
            GetFunc(dllUltralight, out ulDestroyMouseEvent);
            GetFunc(dllUltralight, out ulUpdate);
            GetFunc(dllUltralight, out ulRender);
            GetFunc(dllUltralight, out ulViewGetSurface);
            GetFunc(dllUltralight, out ulBitmapSurfaceGetBitmap);
            GetFunc(dllUltralight, out ulSurfaceGetDirtyBounds);
            GetFunc(dllUltralight, out ulSurfaceClearDirtyBounds);
            GetFunc(dllUltralight, out ulBitmapWritePNG);
            GetFunc(dllUltralight, out ulBitmapLockPixels);
            GetFunc(dllUltralight, out ulBitmapUnlockPixels);
            GetFunc(dllUltralight, out ulBitmapRawPixels);
            GetFunc(dllUltralight, out ulBitmapGetSize);
            GetFunc(dllUltralight, out ulBitmapGetRowBytes);
            GetFunc(dllUltralight, out ulBitmapGetWidth);
            GetFunc(dllUltralight, out ulBitmapGetHeight);

            // WebCore.dll

            GetFunc(dllWebCore, out JSStringCreateWithUTF8CString);
            GetFunc(dllWebCore, out JSStringGetMaximumUTF8CStringSize);
            GetFunc(dllWebCore, out JSStringGetUTF8CString);
            GetFunc(dllWebCore, out JSStringRelease);
            GetFunc(dllWebCore, out JSValueToStringCopy);
            GetFunc(dllWebCore, out JSObjectMakeFunctionWithCallback);
            GetFunc(dllWebCore, out JSContextGetGlobalObject);
            GetFunc(dllWebCore, out JSObjectSetProperty);
            GetFunc(dllWebCore, out JSValueMakeNull);

            IntPtr config = ulCreateConfig();

            IntPtr resourcePath = CreateULString(Path.Combine(dllDir, "resources" + Path.DirectorySeparatorChar));
            ulConfigSetResourcePathPrefix(config, resourcePath);
            ulDestroyString(resourcePath);

            ulEnablePlatformFontLoader();

            IntPtr assetsPath = CreateULString(Path.Combine(Program.ClientDataDir, "WebUI" + Path.DirectorySeparatorChar));
            ulEnablePlatformFileSystem(assetsPath);
            ulDestroyString(assetsPath);

            if (EnableLogging)
            {
                IntPtr logPath = CreateULString(Path.Combine(dllDir, "ultralight.log"));
                ulEnableDefaultLogger(logPath);
                ulDestroyString(logPath);
            }

            ULClipboard* clipboard = (ULClipboard*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ULClipboard)));
            clipboard->clear = OnClipboardClear.Method.MethodHandle.GetFunctionPointer();
            clipboard->read_plain_text = OnClipboardReadPlainText.Method.MethodHandle.GetFunctionPointer();
            clipboard->write_plain_text = OnClipboardWritePlainText.Method.MethodHandle.GetFunctionPointer();
            ulPlatformSetClipboard((IntPtr)clipboard);

            IntPtr renderer = ulCreateRenderer(config);

            ulDestroyConfig(config);

            IntPtr viewConfig = ulCreateViewConfig();
            ulViewConfigSetInitialDeviceScale(viewConfig, 1.0);
            ulViewConfigSetIsAccelerated(viewConfig, false);
            ulViewConfigSetIsTransparent(viewConfig, true);

            IntPtr view = ulCreateView(renderer, width, height, viewConfig, IntPtr.Zero);

            ulDestroyViewConfig(viewConfig);

            ulViewSetChangeTitleCallback(view, OnSetTitle.Method.MethodHandle.GetFunctionPointer(), IntPtr.Zero);
            ulViewSetAddConsoleMessageCallback(view, OnConsoleMessage.Method.MethodHandle.GetFunctionPointer(), IntPtr.Zero);
            ulViewSetDOMReadyCallback(view, OnDOMReady.Method.MethodHandle.GetFunctionPointer(), IntPtr.Zero);
            ulViewSetBeginLoadingCallback(view, OnBeginLoading.Method.MethodHandle.GetFunctionPointer(), IntPtr.Zero);
            ulViewSetFinishLoadingCallback(view, OnFinishLoading.Method.MethodHandle.GetFunctionPointer(), IntPtr.Zero);

            IntPtr urlString = CreateULString("file:///page.html");
            ulViewLoadURL(view, urlString);
            ulDestroyString(urlString);

            Ultralight.view = view;
            Ultralight.renderer = renderer;
            Ultralight.surface = ulViewGetSurface(view);
            Ultralight.bitmap = ulBitmapSurfaceGetBitmap(surface);

            IsLoaded = true;
            Width = width;
            Height = height;
        }

        public static void InputMouseEvent(ULMouseEventType type, int x, int y, ULMouseButton button)
        {
            if (view == IntPtr.Zero)
            {
                return;
            }
            IntPtr evt = ulCreateMouseEvent(type, x, y, button);
            ulViewFireMouseEvent(view, evt);
            ulDestroyMouseEvent(evt);
        }

        public static void InputScrollEvent(ULScrollEventType type, int delta_x, int delta_y)
        {
            if (view == IntPtr.Zero)
            {
                return;
            }
            IntPtr evt = ulCreateScrollEvent(type, delta_x, delta_y);
            ulViewFireScrollEvent(view, evt);
            ulDestroyScrollEvent(evt);
        }

        public static void InputKeyEvent(ULKeyEventType type, uint modifiers, int virtualKeyCode, int nativeKeyCode, string text, string unmodifiedText, bool isKeypad, bool isAutoRepeat, bool isSystemKey)
        {
            if (view == IntPtr.Zero)
            {
                return;
            }
            IntPtr textPtr = CreateULString(text);
            IntPtr unmodifiedTextPtr = CreateULString(unmodifiedText);
            IntPtr evt = ulCreateKeyEvent(type, modifiers, virtualKeyCode, nativeKeyCode, textPtr, unmodifiedTextPtr, isKeypad, isAutoRepeat, isSystemKey);
            ulViewFireKeyEvent(view, evt);
            ulDestroyKeyEvent(evt);
            ulDestroyString(textPtr);
            ulDestroyString(unmodifiedTextPtr);
        }

        public static void ResizeView(uint width, uint height)
        {
            if (view == IntPtr.Zero)
            {
                return;
            }
            Width = width;
            Height = height;
            ulViewResize(view, width, height);
            Ultralight.surface = ulViewGetSurface(view);
            Ultralight.bitmap = ulBitmapSurfaceGetBitmap(surface);
            Update();
        }

        public static void Update()
        {
            if (renderer == IntPtr.Zero)
            {
                return;
            }
            ulUpdate(renderer);
            ulRender(renderer);
        }

        public static IntPtr LockPixels(out uint width, out uint height, out uint stride)
        {
            if (bitmap == IntPtr.Zero)
            {
                width = 0;
                height = 0;
                stride = 0;
                return IntPtr.Zero;
            }
            width = ulBitmapGetWidth(bitmap);
            height = ulBitmapGetHeight(bitmap);
            stride = ulBitmapGetRowBytes(bitmap);
            return ulBitmapLockPixels(bitmap);
        }

        public static void UnlockPixels()
        {
            if (bitmap == IntPtr.Zero)
            {
                return;
            }
            ulBitmapUnlockPixels(bitmap);
        }

        public static ULIntRect GetDirtyBounds()
        {
            if (surface == IntPtr.Zero)
            {
                return default(ULIntRect);
            }
            return ulSurfaceGetDirtyBounds(surface);
        }

        public static void ClearDirtyBounds()
        {
            if (surface == IntPtr.Zero)
            {
                return;
            }
            ulSurfaceClearDirtyBounds(surface);
        }

        static Action OnClipboardClear = ClipboardClearImpl;
        static void ClipboardClearImpl()
        {
            Clipboard.Clear();
        }

        static Action<IntPtr> OnClipboardReadPlainText = OnClipboardReadPlainTextImpl;
        static void OnClipboardReadPlainTextImpl(IntPtr result)
        {
            IntPtr str = CreateULString(Clipboard.GetText());
            ulStringAssignString(result, str);
            ulDestroyString(str);
        }

        static Action<IntPtr> OnClipboardWritePlainText = OnClipboardWritePlainTextImpl;
        static void OnClipboardWritePlainTextImpl(IntPtr text)
        {
            string str = GetStringData(text);
            Clipboard.SetText(str);
        }

        static Action<IntPtr, IntPtr, IntPtr> OnSetTitle = OnSetTitleImpl;
        static void OnSetTitleImpl(IntPtr userData, IntPtr view, IntPtr title)
        {
            //CreditViewController.SetHeaderTitle(GetStringData(title));
        }

        static Action<IntPtr, IntPtr, IntPtr, int, IntPtr, uint, uint, IntPtr> OnConsoleMessage = OnConsoleMessageImpl;
        static void OnConsoleMessageImpl(IntPtr userData, IntPtr view, IntPtr source, int level, IntPtr message, uint line_number, uint column_number, IntPtr source_id)
        {
            Console.WriteLine("[jslog(" + level + ",ln:" + line_number + ",ch:" + column_number + ")] " + GetStringData(message));
        }

        static Action<IntPtr, IntPtr, long, bool, IntPtr> OnDOMReady = OnDOMReadyImpl;
        static void OnDOMReadyImpl(IntPtr userData, IntPtr view, long frameId, bool isMainFrame, IntPtr url)
        {
            IntPtr ctx = ulViewLockJSContext(view);
            byte[] utf8String = Encoding.UTF8.GetBytes("nativeCall\0");
            IntPtr name = JSStringCreateWithUTF8CString(utf8String);
            IntPtr func = JSObjectMakeFunctionWithCallback(ctx, name, OnCallFromJavascript);
            IntPtr globalObj = JSContextGetGlobalObject(ctx);
            JSObjectSetProperty(ctx, globalObj, name, func, IntPtr.Zero, IntPtr.Zero);
            JSStringRelease(name);
            ulViewUnlockJSContext(view);
        }

        static Action<IntPtr, IntPtr, long, bool, IntPtr> OnBeginLoading = OnBeginLoadingImpl;
        static void OnBeginLoadingImpl(IntPtr userData, IntPtr view, long frameId, bool isMainFrame, IntPtr url)
        {
        }

        static Action<IntPtr, IntPtr, long, bool, IntPtr> OnFinishLoading = OnFinishLoadingImpl;
        static void OnFinishLoadingImpl(IntPtr userData, IntPtr view, long frameId, bool isMainFrame, IntPtr url)
        {
        }

        public static string EvaluateScript(string script)
        {
            if (!IsLoaded)
            {
                return null;
            }
            IntPtr str = CreateULString(script);
            IntPtr result = ulViewEvaluateScript(view, str, IntPtr.Zero);
            ulDestroyString(str);
            return GetStringData(result);
        }

        public delegate void CallFromJavascriptHandler(string name, string arg);
        public static event CallFromJavascriptHandler CallFromJavascript;

        delegate IntPtr Del_JSObjectCallAsFunctionCallback(IntPtr ctx, IntPtr function, IntPtr thisObject, UIntPtr argumentCount, IntPtr* arguments, IntPtr exception);
        static Del_JSObjectCallAsFunctionCallback OnCallFromJavascript = OnCallFromJavascriptImpl;

        static IntPtr OnCallFromJavascriptImpl(IntPtr ctx, IntPtr function, IntPtr thisObject, UIntPtr argumentCount, IntPtr* arguments, IntPtr exception)
        {
            string name = StringFromJSArg(ctx, arguments[0], exception);
            string arg = StringFromJSArg(ctx, arguments[1], exception);
            if (CallFromJavascript != null)
            {
                CallFromJavascript(name, arg);
            }
            return JSValueMakeNull(ctx);
        }

        static string StringFromJSArg(IntPtr ctx, IntPtr arg, IntPtr exception)
        {
            IntPtr str = JSValueToStringCopy(ctx, arg, exception);
            int len = (int)JSStringGetMaximumUTF8CStringSize(str);
            byte[] buffer = new byte[len];
            int realLen = (int)JSStringGetUTF8CString(str, buffer, (UIntPtr)len);
            JSStringRelease(str);
            return Encoding.UTF8.GetString(buffer, 0, realLen).TrimEnd('\0');
        }

        static bool GetFunc<T>(IntPtr dll, out T result)
        {
            string funcName = typeof(T).Name.Substring(4);
            if (dll == IntPtr.Zero)
            {
                throw new Exception("dll not found for " + funcName);
            }
            IntPtr func = PInvoke.GetProcAddress(dll, funcName);
            if (func == IntPtr.Zero)
            {
                throw new Exception("Failed to find function " + funcName);
            }
            result = (T)(object)Marshal.GetDelegateForFunctionPointer(func, typeof(T));
            return result != null;
        }

        static IntPtr CreateULString(string str)
        {
            if (str == null)
            {
                str = string.Empty;
            }
            fixed (char* ptr = str)
            {
                return ulCreateStringUTF16((IntPtr)ptr, str.Length);
            }
        }

        static string GetStringData(IntPtr str)
        {
            byte[] buffer = new byte[ulStringGetLength(str)];
            Marshal.Copy(ulStringGetData(str), buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        // AppCore.dll

        public delegate void Del_ulEnablePlatformFontLoader();
        public static Del_ulEnablePlatformFontLoader ulEnablePlatformFontLoader;

        public delegate void Del_ulEnablePlatformFileSystem(IntPtr base_dir);
        public static Del_ulEnablePlatformFileSystem ulEnablePlatformFileSystem;

        public delegate void Del_ulEnableDefaultLogger(IntPtr log_path);
        public static Del_ulEnableDefaultLogger ulEnableDefaultLogger;

        // Ultralight.dll

        public delegate void Del_ulPlatformSetClipboard(IntPtr clipboard);
        public static Del_ulPlatformSetClipboard ulPlatformSetClipboard;

        delegate IntPtr Del_ulCreateConfig();
        static Del_ulCreateConfig ulCreateConfig;

        delegate void Del_ulDestroyConfig(IntPtr config);
        static Del_ulDestroyConfig ulDestroyConfig;

        delegate void Del_ulConfigSetResourcePathPrefix(IntPtr config, IntPtr resource_path_prefix);
        static Del_ulConfigSetResourcePathPrefix ulConfigSetResourcePathPrefix;

        delegate IntPtr Del_ulCreateStringUTF16(IntPtr str, int len);
        static Del_ulCreateStringUTF16 ulCreateStringUTF16;

        delegate void Del_ulStringAssignString(IntPtr str, IntPtr new_str);
        static Del_ulStringAssignString ulStringAssignString;

        delegate void Del_ulDestroyString(IntPtr str);
        static Del_ulDestroyString ulDestroyString;

        delegate IntPtr Del_ulStringGetData(IntPtr str);
        static Del_ulStringGetData ulStringGetData;

        delegate int Del_ulStringGetLength(IntPtr str);
        static Del_ulStringGetLength ulStringGetLength;

        delegate IntPtr Del_ulCreateRenderer(IntPtr config);
        static Del_ulCreateRenderer ulCreateRenderer;

        delegate IntPtr Del_ulCreateViewConfig();
        static Del_ulCreateViewConfig ulCreateViewConfig;

        delegate IntPtr Del_ulDestroyViewConfig(IntPtr config);
        static Del_ulDestroyViewConfig ulDestroyViewConfig;

        delegate void Del_ulViewConfigSetInitialDeviceScale(IntPtr config, double initial_device_scale);
        static Del_ulViewConfigSetInitialDeviceScale ulViewConfigSetInitialDeviceScale;

        delegate void Del_ulViewConfigSetIsAccelerated(IntPtr config, bool is_accelerated);
        static Del_ulViewConfigSetIsAccelerated ulViewConfigSetIsAccelerated;

        delegate void Del_ulViewConfigSetIsTransparent(IntPtr config, bool is_transparent);
        static Del_ulViewConfigSetIsTransparent ulViewConfigSetIsTransparent;

        delegate IntPtr Del_ulCreateView(IntPtr renderer, uint width, uint height, IntPtr view_config, IntPtr session);
        static Del_ulCreateView ulCreateView;

        delegate void Del_ulViewResize(IntPtr view, uint width, uint height);
        static Del_ulViewResize ulViewResize;

        delegate void Del_ulViewSetDeviceScale(IntPtr view, double scale);
        static Del_ulViewSetDeviceScale ulViewSetDeviceScale;

        delegate void Del_ulViewSetChangeTitleCallback(IntPtr view, IntPtr callback, IntPtr user_data);
        static Del_ulViewSetChangeTitleCallback ulViewSetChangeTitleCallback;

        delegate void Del_ulViewSetAddConsoleMessageCallback(IntPtr view, IntPtr callback, IntPtr user_data);
        static Del_ulViewSetAddConsoleMessageCallback ulViewSetAddConsoleMessageCallback;

        delegate void Del_ulViewSetDOMReadyCallback(IntPtr view, IntPtr callback, IntPtr user_data);
        static Del_ulViewSetDOMReadyCallback ulViewSetDOMReadyCallback;

        delegate void Del_ulViewSetBeginLoadingCallback(IntPtr view, IntPtr callback, IntPtr user_data);
        static Del_ulViewSetBeginLoadingCallback ulViewSetBeginLoadingCallback;

        delegate void Del_ulViewSetFinishLoadingCallback(IntPtr view, IntPtr callback, IntPtr user_data);
        static Del_ulViewSetFinishLoadingCallback ulViewSetFinishLoadingCallback;

        delegate void Del_ulViewLoadURL(IntPtr view, IntPtr url_string);
        static Del_ulViewLoadURL ulViewLoadURL;

        delegate IntPtr Del_ulViewEvaluateScript(IntPtr view, IntPtr js_string, IntPtr exception);
        static Del_ulViewEvaluateScript ulViewEvaluateScript;

        delegate IntPtr Del_ulViewLockJSContext(IntPtr view);
        static Del_ulViewLockJSContext ulViewLockJSContext;

        delegate IntPtr Del_ulViewUnlockJSContext(IntPtr view);
        static Del_ulViewUnlockJSContext ulViewUnlockJSContext;

        delegate void Del_ulViewFireKeyEvent(IntPtr view, IntPtr key_event);
        static Del_ulViewFireKeyEvent ulViewFireKeyEvent;

        delegate void Del_ulViewFireMouseEvent(IntPtr view, IntPtr mouse_event);
        static Del_ulViewFireMouseEvent ulViewFireMouseEvent;

        delegate void Del_ulViewFireScrollEvent(IntPtr view, IntPtr scroll_event);
        static Del_ulViewFireScrollEvent ulViewFireScrollEvent;

        delegate IntPtr Del_ulCreateScrollEvent(ULScrollEventType type, int delta_x, int delta_y);
        static Del_ulCreateScrollEvent ulCreateScrollEvent;

        delegate void Del_ulDestroyScrollEvent(IntPtr evt);
        static Del_ulDestroyScrollEvent ulDestroyScrollEvent;

        delegate IntPtr Del_ulCreateKeyEvent(ULKeyEventType type, uint modifiers, int virtual_key_code, int native_key_code, IntPtr text, IntPtr unmodified_text, bool is_keypad, bool is_auto_repeat, bool is_system_key);
        static Del_ulCreateKeyEvent ulCreateKeyEvent;

        delegate void Del_ulDestroyKeyEvent(IntPtr evt);
        static Del_ulDestroyKeyEvent ulDestroyKeyEvent;

        delegate IntPtr Del_ulCreateMouseEvent(ULMouseEventType type, int x, int y, ULMouseButton button);
        static Del_ulCreateMouseEvent ulCreateMouseEvent;

        delegate void Del_ulDestroyMouseEvent(IntPtr evt);
        static Del_ulDestroyMouseEvent ulDestroyMouseEvent;

        delegate void Del_ulUpdate(IntPtr renderer);
        static Del_ulUpdate ulUpdate;

        delegate void Del_ulRender(IntPtr renderer);
        static Del_ulRender ulRender;

        delegate IntPtr Del_ulViewGetSurface(IntPtr view);
        static Del_ulViewGetSurface ulViewGetSurface;

        delegate IntPtr Del_ulBitmapSurfaceGetBitmap(IntPtr surface);
        static Del_ulBitmapSurfaceGetBitmap ulBitmapSurfaceGetBitmap;

        delegate ULIntRect Del_ulSurfaceGetDirtyBounds(IntPtr surface);
        static Del_ulSurfaceGetDirtyBounds ulSurfaceGetDirtyBounds;

        delegate void Del_ulSurfaceClearDirtyBounds(IntPtr surface);
        static Del_ulSurfaceClearDirtyBounds ulSurfaceClearDirtyBounds;

        delegate IntPtr Del_ulBitmapWritePNG(IntPtr bitmap, string path);
        static Del_ulBitmapWritePNG ulBitmapWritePNG;

        delegate IntPtr Del_ulBitmapLockPixels(IntPtr bitmap);
        static Del_ulBitmapLockPixels ulBitmapLockPixels;

        delegate void Del_ulBitmapUnlockPixels(IntPtr bitmap);
        static Del_ulBitmapUnlockPixels ulBitmapUnlockPixels;

        delegate IntPtr Del_ulBitmapRawPixels(IntPtr bitmap);
        static Del_ulBitmapRawPixels ulBitmapRawPixels;

        delegate UIntPtr Del_ulBitmapGetSize(IntPtr bitmap);
        static Del_ulBitmapGetSize ulBitmapGetSize;

        delegate uint Del_ulBitmapGetRowBytes(IntPtr bitmap);
        static Del_ulBitmapGetRowBytes ulBitmapGetRowBytes;

        delegate uint Del_ulBitmapGetWidth(IntPtr bitmap);
        static Del_ulBitmapGetWidth ulBitmapGetWidth;

        delegate uint Del_ulBitmapGetHeight(IntPtr bitmap);
        static Del_ulBitmapGetHeight ulBitmapGetHeight;

        // WebCore.dll

        delegate IntPtr Del_JSStringCreateWithUTF8CString(byte[] str);
        static Del_JSStringCreateWithUTF8CString JSStringCreateWithUTF8CString;

        delegate UIntPtr Del_JSStringGetMaximumUTF8CStringSize(IntPtr str);
        static Del_JSStringGetMaximumUTF8CStringSize JSStringGetMaximumUTF8CStringSize;

        delegate UIntPtr Del_JSStringGetUTF8CString(IntPtr str, byte[] buffer, UIntPtr bufferSize);
        static Del_JSStringGetUTF8CString JSStringGetUTF8CString;

        delegate void Del_JSStringRelease(IntPtr str);
        static Del_JSStringRelease JSStringRelease;

        delegate IntPtr Del_JSValueToStringCopy(IntPtr ctx, IntPtr value, IntPtr exception);
        static Del_JSValueToStringCopy JSValueToStringCopy;

        delegate IntPtr Del_JSObjectMakeFunctionWithCallback(IntPtr ctx, IntPtr name, Del_JSObjectCallAsFunctionCallback callAsFunction);
        static Del_JSObjectMakeFunctionWithCallback JSObjectMakeFunctionWithCallback;

        delegate IntPtr Del_JSContextGetGlobalObject(IntPtr ctx);
        static Del_JSContextGetGlobalObject JSContextGetGlobalObject;

        delegate IntPtr Del_JSObjectSetProperty(IntPtr ctx, IntPtr obj, IntPtr propertyName, IntPtr value, IntPtr attributes, IntPtr exception);
        static Del_JSObjectSetProperty JSObjectSetProperty;

        delegate IntPtr Del_JSValueMakeNull(IntPtr ctx);
        static Del_JSValueMakeNull JSValueMakeNull;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ULIntRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public bool IsEmpty()
        {
            return left == 0 && top == 0 && right == 0 && bottom == 0;
        }

        public override string ToString()
        {
            return "{left:" + left + "," + "top:" + top + ",right:" + right + ",bottom:" + bottom + "}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ULClipboard
    {
        public IntPtr clear;
        public IntPtr read_plain_text;
        public IntPtr write_plain_text;
    }

    enum ULScrollEventType
    {
        ScrollByPixel,
        ScrollByPage
    }

    enum ULMouseButton
    {
        None,
        Left,
        Middle,
        Right
    }

    enum ULMouseEventType
    {
        MouseMoved,
        MouseDown,
        MouseUp
    }

    enum ULKeyEventType
    {
        /// <summary>
        /// Key-Down event type. (Does not trigger accelerator commands in WebCore)
        /// 
        /// @NOTE: You should probably use RawKeyDown instead when a physical key
        ///        is pressed. This member is only here for historic compatibility
        ///        with WebCore's key event types.
        /// </summary>
        KeyDown,
        /// <summary>
        /// Key-Up event type. Use this when a physical key is released.
        /// </summary>
        KeyUp,
        /// <summary>
        /// Raw Key-Down type. Use this when a physical key is pressed.
        /// 
        /// @NOTE: You should use RawKeyDown for physical key presses since it
        ///        allows WebCore to do additional command translation.
        /// </summary>
        RawKeyDown,
        /// <summary>
        /// Character input event type. Use this when the OS generates text from
        /// a physical key being pressed (eg, WM_CHAR on Windows).
        /// </summary>
        Char
    }
}
