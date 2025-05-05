using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using YgoMaster;

namespace YgoMasterClient
{
    unsafe static partial class AssetHelper
    {
        [ThreadStatic]
        static float[] audioBufferTLS;
        static IntPtr audioBufferIL2CPP;
        static Dictionary<string, List<CustomAssetLoadRequest>> customAssetLoadRequests = new Dictionary<string, List<CustomAssetLoadRequest>>();
        struct CustomAssetLoadRequest
        {
            public IntPtr CompleteHandler;
            public uint GcHandle;
        }

        enum AssetType
        {
            None,
            Audio,
            Image,
            Protector,
            IconFrame,
            Wallpaper
        }

        enum TweenClassType
        {
            TweenAlpha,
            TweenPosition,
            TweenRotation,
            TweenScale
        }

        public static bool IsLoadingCustomAsset
        {
            get
            {
                lock (customAssetLoadRequests)
                {
                    return customAssetLoadRequests.Count > 0;
                }
            }
        }
        public static bool IsQuitting { get; private set; }

        // TODO: Put these definitions into nested classes (or just use more explicit names)

        // YgomSystem.Utility.DeviceInfo
        static IL2Method methodGetResourceType;

        // YgomSystem.ResourceSystem.ResourceUtility
        static IL2Class resourceUtilityClassInfo;
        static IL2Method methodConvertAutoPath;
        static IL2Method methodGetCrc;

        // YgomSystem.ResourceManager
        static IL2Class resourceManagerClassInfo;
        static IL2Method methodExists;
        static IL2Method methodGetResource;
        static IL2Method methodGetAsset;
        static IL2Method methodUnload;
        static IL2Method methodCallErrorHandler;
        static IL2Field fieldResourceDictionary;
        static IL2Field fieldResourceManagerInstance;

        delegate IntPtr Del_GetResource(IntPtr thisPtr, IntPtr pathPtr, IntPtr workPathPtr);
        static Hook<Del_GetResource> hookGetResource;
        delegate uint Del_Load(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInterface, IntPtr completeHandler, csbool disableErrorNotify);
        static Hook<Del_Load> hookLoad;
        delegate uint Del_LoadImmediate(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInterface, IntPtr completeHandler, csbool disableErrorNotify);
        static Hook<Del_LoadImmediate> hookLoadImmediate;

        static IntPtr resourceMangerInstance;
        static IL2DictionaryExplicit resourceDictionary;

        // YgoSystem.ResourceManager.RequestCompleteHandler
        static IL2Class requestCompleteHandlerClassInfo;
        static IL2Method methodInvoke;

        // YgomSystem.ResourceSystem.Resource
        static IL2Class resourceClassInfo;
        static IL2Method methodResourceCtor;
        static IL2Method methodGetAssets;
        static IL2Method methodSetAssets;
        static IL2Method methodGetBytes;
        static IL2Method methodSetBytes;
        static IL2Method methodSetError;
        static IL2Method methodGetRefCount;
        static IL2Method methodSetRefCount;
        static IL2Method methodSetResType;
        static IL2Method methodGetDone;
        static IL2Method methodSetDone;
        static IL2Method methodSetSystemType;
        static IL2Method methodSetQueueId;
        static IL2Method methodGetPath;
        static IL2Method methodSetPath;
        static IL2Method methodGetLoadPath;
        static IL2Method methodSetLoadPath;

        // YgomGame.Card.Content
        static IL2Field fieldContentInstance;
        static IL2Method methodGetBytesDecryptionData;

        // UnityEngine.ImageConversionModule.ImageConversion
        static IL2Class imageConversionClassInfo;
        static IL2Method methodLoadImage;
        static IL2Method methodEncodeToJPG;

        // UnityEngine.CoreModule (Texture2D, Texture, RenderTexture, Sprite, Rect, Vector2)
        const int TextureFormat_ARGB32 = 5;
        const int RenderTextureFormat_ARGB32 = 0;
        const int FilterMode_Point = 0;
        public static IL2Class texture2DClassInfo;// Texture2D
        public static IL2Method methodTexture2DCtor;
        public static IL2Method methodTexture2DCtor2;
        static IL2Method methodGetIsReadable;
        static IL2Method methodGetFormat;
        static IL2Method methodReadPixels;
        public static IL2Method methodSetPixels32;
        public static IL2Method methodApply;
        //static IL2Class textureClassInfo;// Texture
        static IL2Method methodGetWidth;
        static IL2Method methodGetHeight;
        static IL2Method methodGetFilterMode;
        static IL2Method methodSetFilterMode;
        static IL2Class renderTextureClassInfo;// RenderTexture
        static IL2Method methodGetTemporary;
        static IL2Method methodReleaseTemporary;
        static IL2Method methodGetActive;
        static IL2Method methodSetActive;
        static IL2Class graphicsClassInfo;// Graphics
        static IL2Method methodBlit;
        static IL2Class spriteClassInfo;// Sprite
        static IL2Method methodSpriteCreate;
        static IL2Method methodGetRect;
        static IL2Method methodGetPixelsPerUnit;
        static IL2Method methodGetTexture;
        static IL2Class monoBehaviourClassInfo;// MonoBehaviour
        static IL2Class gameObjectClassInfo;// GameObject
        static IL2Method methodGetComponentInChildren;
        static IL2Method methodGetComponent;
        static IL2Class imageClassInfo;// Image
        static IntPtr imageClassType;
        static IL2Method methodGetSprite;
        static IL2Method methodSetSprite;
        static IL2Class rectClassInfo;// Rect
        static IL2Class vector2ClassInfo;// Vector2
        static IL2Class objectClassInfo;// Object (UnityEngine)
        static IL2Method methodGetName;
        static IL2Method methodSetName;
        static IL2Method methodSetTexture;// Material
        static IL2Method methodSetFloat;
        static IL2Method methodSetInt;
        static IL2Method methodSetColor;
        static IntPtr rectTransformClassType;// RectTransform
        static IL2Method rectTransformSetSizeDelta;

        // YgomSystem.UI.Tween
        static Dictionary<TweenClassType, IL2Class> tweenClassInfos = new Dictionary<TweenClassType, IL2Class>();
        static IL2Class tweenClassInfo;
        static IL2Field tweenLabel;
        static IL2Field tweenAlphaFrom;
        static IL2Field tweenAlphaTo;
        static IL2Field tweenAlphaIsRecursive;
        static IL2Field tweenPositionFrom;
        static IL2Field tweenPositionTo;
        static IL2Field tweenRotationFrom;
        static IL2Field tweenRotationTo;
        static IL2Field tweenRotationQuaternionLerp;
        static IL2Field tweenScaleFrom;
        static IL2Field tweenScaleTo;

        static IL2Method methodAddQuitting;

        // mscorlib
        static IL2Method methodReadAllBytes;// File

        // UnityEngine.AudioClip
        delegate IntPtr Del_AudioClip_CUSTOM_Construct_Internal();
        static Del_AudioClip_CUSTOM_Construct_Internal AudioClip_CUSTOM_Construct_Internal;
        delegate void Del_AudioClip_CUSTOM_CreateUserSound(IntPtr thisPtr, IntPtr name, int lengthSamples, int channels, int frequency, csbool stream);
        static Del_AudioClip_CUSTOM_CreateUserSound AudioClip_CUSTOM_CreateUserSound;
        delegate csbool Del_AudioClip_CUSTOM_SetData(IntPtr clip, IntPtr data, int numsamples, int samplesOffset);
        static Del_AudioClip_CUSTOM_SetData AudioClip_CUSTOM_SetData;
        //class ScriptingBackendNativeObjectPtrOpaque * __cdecl AudioClip_CUSTOM_Construct_Internal(void)
        //void __cdecl AudioClip_CUSTOM_CreateUserSound(class ScriptingBackendNativeObjectPtrOpaque *,class ScriptingBackendNativeStringPtrOpaque *,int,int,int,unsigned char)
        //unsigned char __cdecl AudioClip_CUSTOM_SetData(class ScriptingBackendNativeObjectPtrOpaque *,class ScriptingBackendNativeArrayPtrOpaque *,int,int)
        //unsigned char __cdecl AudioClip_CUSTOM_GetData(class ScriptingBackendNativeObjectPtrOpaque *,class ScriptingBackendNativeArrayPtrOpaque *,int,int)
        //unsigned char __cdecl AudioClip_CUSTOM_LoadAudioData(class ScriptingBackendNativeObjectPtrOpaque *)
        //unsigned char __cdecl AudioClip_CUSTOM_UnloadAudioData(class ScriptingBackendNativeObjectPtrOpaque *)
        //class ScriptingBackendNativeStringPtrOpaque * __cdecl AudioClip_CUSTOM_GetName(class ScriptingBackendNativeObjectPtrOpaque *)
        //void * __cdecl DownloadHandlerAudioClip_CUSTOM_Create(class ScriptingBackendNativeObjectPtrOpaque *,class ScriptingBackendNativeStringPtrOpaque *,enum FMOD_SOUND_TYPE)

        // UnityEngine.Networking.DownloadHandlerTexture
        delegate IntPtr Del_DownloadHandlerTexture_CUSTOM_Create(IntPtr obj, csbool readable);
        static Del_DownloadHandlerTexture_CUSTOM_Create DownloadHandlerTexture_CUSTOM_Create;
        delegate IntPtr Del_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative(IntPtr thisPtr);
        static Del_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative DownloadHandlerTexture_CUSTOM_InternalGetTextureNative;

        // UnityEngine.Networking.UnityWebRequest
        static IL2Class webRequestClass;
        static IL2Method webRequestSend;
        static IL2Method webRequestCtor;
        static IL2Method webRequestGetResult;
        static IL2Method webRequestDispose;
        static IL2Method webRequestSetAutoDisposeDownloadHandler;
        // UnityEngine.Networking.DownloadHandler
        static IL2Class downloadHandlerClass;
        static IL2Method downloadHandlerCtor;

        static List<AsyncLoadTextureRequest> asyncLoadTextureRequests = new List<AsyncLoadTextureRequest>();
        class AsyncLoadTextureRequest
        {
            public DateTime RequestTime;
            public IntPtr WebRequest;
            public IntPtr DownloadHandler;
            public Action<IntPtr> Callback;
        }

        static List<AsyncLoadRequest> assetLoadRequests = new List<AsyncLoadRequest>();
        class AsyncLoadRequest
        {
            public DateTime RequestTime;
            public IntPtr ResouecePtr;
            public Action Callback;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector2
        {
            public float x;
            public float y;

            public Vector2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public override string ToString()
            {
                return "{x:" + x + "," + "y:" + y + "}";
            }
        }

        public struct Rect
        {
            public float m_XMin;
            public float m_YMin;
            public float m_Width;
            public float m_Height;

            public Rect(float x, float y, float width, float height)
            {
                this.m_XMin = x;
                this.m_YMin = y;
                this.m_Width = width;
                this.m_Height = height;
            }

            public override string ToString()
            {
                return "{x:" + m_XMin + "," + "y:" + m_YMin + ",w:" + m_Width + ",h:" + m_Height + "}";
            }
        }

        public enum ResourceType
        {
            Unknown,
            /// <summary>
            /// High Resolution
            /// </summary>
            HighEnd_HD,
            /// <summary>
            /// Normal
            /// </summary>
            HighEnd,
            /// <summary>
            /// ???
            /// </summary>
            LowEnd
        }

        /// <summary>
        /// ResourceManager.Resource.Type
        /// </summary>
        enum ResourceManager_Resource_Type
        {
            None,
            BuiltIn,
            AssetBundle,
            Binary,
            Network,
            StreamingAssets,
            StreamingBinary,
            LocalFile,
            StreaminFile
        }

        /// <summary>
        /// ResourceManager.ReqType
        /// </summary>
        enum ResourceManager_ReqType
        {
            Sound,
            Sound2,
            Sound3,
            Sound4,
            Default
            //... (this goes on Default2-32)
        }

        static AssetHelper()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class deviceInfoClassInfo = assembly.GetClass("DeviceInfo", "YgomSystem.Utility");
            methodGetResourceType = deviceInfoClassInfo.GetMethod("GetResourceType");

            resourceUtilityClassInfo = assembly.GetClass("ResourceUtility", "YgomSystem.ResourceSystem");
            methodConvertAutoPath = resourceUtilityClassInfo.GetMethod("ConvertAutoPath");
            methodGetCrc = resourceUtilityClassInfo.GetMethod("GetCrc");

            resourceManagerClassInfo = assembly.GetClass("ResourceManager", "YgomSystem");
            methodExists = resourceManagerClassInfo.GetMethod("Exists");
            methodGetResource = resourceManagerClassInfo.GetMethod("getResource", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Name == "path");
            methodGetAsset = resourceManagerClassInfo.GetMethod("GetAsset");
            methodUnload = resourceManagerClassInfo.GetMethod("unload", x => x.GetParameters()[0].Name == "path");
            methodCallErrorHandler = resourceManagerClassInfo.GetMethod("CallErrorHandler");
            fieldResourceDictionary = resourceManagerClassInfo.GetField("resourceDictionary");
            fieldResourceManagerInstance = resourceManagerClassInfo.GetField("s_instance");
            hookGetResource = new Hook<Del_GetResource>(GetResource, methodGetResource);
            hookLoad = new Hook<Del_Load>(Load, resourceManagerClassInfo.GetMethod("load"));
            hookLoadImmediate = new Hook<Del_LoadImmediate>(LoadImmediate, resourceManagerClassInfo.GetMethod("loadImmediate"));

            requestCompleteHandlerClassInfo = assembly.GetClass("RequestCompleteHandler");//, "YgomSystem");
            methodInvoke = requestCompleteHandlerClassInfo.GetMethod("Invoke");

            resourceClassInfo = assembly.GetClass("Resource", "YgomSystem.ResourceSystem");
            methodResourceCtor = resourceClassInfo.GetMethod(".ctor");
            methodGetAssets = resourceClassInfo.GetProperty("Assets").GetGetMethod();
            methodSetAssets = resourceClassInfo.GetProperty("Assets").GetSetMethod();
            methodGetBytes = resourceClassInfo.GetProperty("Bytes").GetGetMethod();
            methodSetBytes = resourceClassInfo.GetProperty("Bytes").GetSetMethod();
            methodSetError = resourceClassInfo.GetProperty("Error").GetSetMethod();
            methodGetRefCount = resourceClassInfo.GetProperty("RefCount").GetGetMethod();
            methodSetRefCount = resourceClassInfo.GetProperty("RefCount").GetSetMethod();
            methodSetResType = resourceClassInfo.GetProperty("ResType").GetSetMethod();
            methodGetDone = resourceClassInfo.GetProperty("Done").GetGetMethod();
            methodSetDone = resourceClassInfo.GetProperty("Done").GetSetMethod();
            methodSetSystemType = resourceClassInfo.GetProperty("SystemType").GetSetMethod();
            methodSetQueueId = resourceClassInfo.GetProperty("queueId").GetSetMethod();
            methodGetPath = resourceClassInfo.GetProperty("Path").GetGetMethod();
            methodSetPath = resourceClassInfo.GetProperty("Path").GetSetMethod();
            methodGetLoadPath = resourceClassInfo.GetProperty("LoadPath").GetGetMethod();
            methodSetLoadPath = resourceClassInfo.GetProperty("LoadPath").GetSetMethod();

            IL2Class contentClassInfo = assembly.GetClass("Content", "YgomGame.Card");
            fieldContentInstance = contentClassInfo.GetField("s_instance");
            methodGetBytesDecryptionData = contentClassInfo.GetMethod("GetBytesDecryptionData");

            IL2Assembly imageConversionAssembly = Assembler.GetAssembly("UnityEngine.ImageConversionModule");
            imageConversionClassInfo = imageConversionAssembly.GetClass("ImageConversion");
            methodLoadImage = imageConversionClassInfo.GetMethod("LoadImage", x => x.GetParameters().Length == 2);
            methodEncodeToJPG = imageConversionClassInfo.GetMethod("EncodeToJPG", x => x.GetParameters().Length == 1);

            IL2Assembly coreModuleAssembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Assembly uiAssembly = Assembler.GetAssembly("UnityEngine.UI");
            texture2DClassInfo = coreModuleAssembly.GetClass("Texture2D");// Texture2D
            methodTexture2DCtor = texture2DClassInfo.GetMethod(".ctor", x => x.GetParameters().Length == 2);
            methodTexture2DCtor2 = texture2DClassInfo.GetMethod(".ctor", x => x.GetParameters().Length == 4 && x.GetParameters()[2].Name == "textureFormat");
            methodGetIsReadable = texture2DClassInfo.GetProperty("isReadable").GetGetMethod();
            methodGetFormat = texture2DClassInfo.GetProperty("format").GetGetMethod();
            methodReadPixels = texture2DClassInfo.GetMethod("ReadPixels", x => x.GetParameters().Length == 3);
            methodSetPixels32 = texture2DClassInfo.GetMethod("SetPixels32", x => x.GetParameters().Length == 1);
            methodApply = texture2DClassInfo.GetMethod("Apply", x => x.GetParameters().Length == 2);
            IL2Class textureClassInfo = coreModuleAssembly.GetClass("Texture");// Texture (putting class here as I've used it by mistake before...)
            methodGetWidth = textureClassInfo.GetProperty("width").GetGetMethod();
            methodGetHeight = textureClassInfo.GetProperty("height").GetGetMethod();
            methodGetFilterMode = textureClassInfo.GetProperty("filterMode").GetGetMethod();
            methodSetFilterMode = textureClassInfo.GetProperty("filterMode").GetSetMethod();
            renderTextureClassInfo = coreModuleAssembly.GetClass("RenderTexture");// RenderTexture
            methodGetTemporary = renderTextureClassInfo.GetMethod("GetTemporary", x => x.GetParameters().Length == 4);
            methodReleaseTemporary = renderTextureClassInfo.GetMethod("ReleaseTemporary");
            methodGetActive = renderTextureClassInfo.GetProperty("active").GetGetMethod();
            methodSetActive = renderTextureClassInfo.GetProperty("active").GetSetMethod();
            graphicsClassInfo = coreModuleAssembly.GetClass("Graphics");// Graphics
            methodBlit = graphicsClassInfo.GetMethod("Blit", x => x.GetParameters().Length == 2);
            spriteClassInfo = coreModuleAssembly.GetClass("Sprite");// Sprite
            methodSpriteCreate = spriteClassInfo.GetMethod("Create", x => x.GetParameters().Length == 4);
            methodGetRect = spriteClassInfo.GetProperty("rect").GetGetMethod();
            methodGetPixelsPerUnit = spriteClassInfo.GetProperty("pixelsPerUnit").GetGetMethod();
            methodGetTexture = spriteClassInfo.GetProperty("texture").GetGetMethod();
            monoBehaviourClassInfo = coreModuleAssembly.GetClass("MonoBehaviour");// MonoBehaviour
            gameObjectClassInfo = coreModuleAssembly.GetClass("GameObject");// GameObject
            methodGetComponentInChildren = gameObjectClassInfo.GetMethod("GetComponentInChildren", x => x.GetParameters().Length == 2);
            methodGetComponent = gameObjectClassInfo.GetMethod("GetComponent", x => x.GetParameters().Length == 1 && x.GetParameters()[0].Type.Name == typeof(Type).FullName);
            imageClassInfo = uiAssembly.GetClass("Image");// Image
            imageClassType = imageClassInfo.IL2Typeof();
            methodGetSprite = imageClassInfo.GetProperty("sprite").GetGetMethod();
            methodSetSprite = imageClassInfo.GetProperty("sprite").GetSetMethod();
            rectClassInfo = coreModuleAssembly.GetClass("Rect");// Rect
            vector2ClassInfo = coreModuleAssembly.GetClass("Vector2");// Vector2
            objectClassInfo = coreModuleAssembly.GetClass("Object");
            methodGetName = objectClassInfo.GetProperty("name").GetGetMethod();
            methodSetName = objectClassInfo.GetProperty("name").GetSetMethod();
            IL2Class materialClassInfo = coreModuleAssembly.GetClass("Material", "UnityEngine");// Material
            methodSetTexture = materialClassInfo.GetMethod("SetTexture", x => x.GetParameters()[0].Name == "name");
            methodSetFloat = materialClassInfo.GetMethod("SetFloat", x => x.GetParameters()[0].Name == "name");
            methodSetInt = materialClassInfo.GetMethod("SetInt", x => x.GetParameters()[0].Name == "name");
            methodSetColor = materialClassInfo.GetMethod("SetColor", x => x.GetParameters()[0].Name == "name");
            IL2Class rectTransformClassInfo = coreModuleAssembly.GetClass("RectTransform", "UnityEngine");
            rectTransformClassType = rectTransformClassInfo.IL2Typeof();
            rectTransformSetSizeDelta = rectTransformClassInfo.GetProperty("sizeDelta").GetSetMethod();

            tweenClassInfo = assembly.GetClass("Tween", "YgomSystem.UI");
            tweenLabel = tweenClassInfo.GetField("label");
            
            IL2Class tweenAlphaClassInfo = assembly.GetClass("TweenAlpha", "YgomSystem.UI");
            tweenClassInfos[TweenClassType.TweenAlpha] = tweenAlphaClassInfo;
            tweenAlphaFrom = tweenAlphaClassInfo.GetField("from");
            tweenAlphaTo = tweenAlphaClassInfo.GetField("to");
            tweenAlphaIsRecursive = tweenAlphaClassInfo.GetField("isRecusive");

            IL2Class tweenPositionClassInfo = assembly.GetClass("TweenPosition", "YgomSystem.UI");
            tweenClassInfos[TweenClassType.TweenPosition] = tweenPositionClassInfo;
            tweenPositionFrom = tweenPositionClassInfo.GetField("from");
            tweenPositionTo = tweenPositionClassInfo.GetField("to");

            IL2Class tweenRotationClassInfo = assembly.GetClass("TweenRotation", "YgomSystem.UI");
            tweenClassInfos[TweenClassType.TweenRotation] = tweenRotationClassInfo;
            tweenRotationFrom = tweenRotationClassInfo.GetField("from");
            tweenRotationTo = tweenRotationClassInfo.GetField("to");
            tweenRotationQuaternionLerp = tweenRotationClassInfo.GetField("quaternionLerp");

            IL2Class tweenScaleClassInfo = assembly.GetClass("TweenScale", "YgomSystem.UI");
            tweenClassInfos[TweenClassType.TweenScale] = tweenScaleClassInfo;
            tweenScaleFrom = tweenScaleClassInfo.GetField("from");
            tweenScaleTo = tweenScaleClassInfo.GetField("to");

            IL2Assembly mscorlibAssembly = Assembler.GetAssembly("mscorlib");
            IL2Class fileClassInfo = mscorlibAssembly.GetClass("File");
            methodReadAllBytes = fileClassInfo.GetMethod("ReadAllBytes");

            long unityPlayer = PInvoke.GetModuleHandle("UnityPlayer.dll").ToInt64();
            AudioClip_CUSTOM_Construct_Internal = (Del_AudioClip_CUSTOM_Construct_Internal)Marshal.GetDelegateForFunctionPointer((IntPtr)(unityPlayer + ClientSettings.UnityPlayerRVA_AudioClip_CUSTOM_Construct_Internal), typeof(Del_AudioClip_CUSTOM_Construct_Internal));
            AudioClip_CUSTOM_CreateUserSound = (Del_AudioClip_CUSTOM_CreateUserSound)Marshal.GetDelegateForFunctionPointer((IntPtr)(unityPlayer + ClientSettings.UnityPlayerRVA_AudioClip_CUSTOM_CreateUserSound), typeof(Del_AudioClip_CUSTOM_CreateUserSound));
            AudioClip_CUSTOM_SetData = (Del_AudioClip_CUSTOM_SetData)Marshal.GetDelegateForFunctionPointer((IntPtr)(unityPlayer + ClientSettings.UnityPlayerRVA_AudioClip_CUSTOM_SetData), typeof(Del_AudioClip_CUSTOM_SetData));

            IL2Assembly webRequestAssembly = Assembler.GetAssembly("UnityEngine.UnityWebRequestModule");
            webRequestClass = webRequestAssembly.GetClass("UnityWebRequest");
            webRequestCtor = webRequestClass.GetMethod(".ctor", x => x.GetParameters().Length == 4);
            webRequestSend = webRequestClass.GetMethod("SendWebRequest");
            webRequestGetResult = webRequestClass.GetProperty("result").GetGetMethod();
            webRequestDispose = webRequestClass.GetMethod("Dispose");
            webRequestSetAutoDisposeDownloadHandler = webRequestClass.GetProperty("disposeDownloadHandlerOnDispose").GetSetMethod();
            downloadHandlerClass = webRequestAssembly.GetClass("DownloadHandler");
            downloadHandlerCtor = downloadHandlerClass.GetMethod(".ctor");

            DownloadHandlerTexture_CUSTOM_Create = (Del_DownloadHandlerTexture_CUSTOM_Create)Marshal.GetDelegateForFunctionPointer((IntPtr)(unityPlayer + ClientSettings.UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_Create), typeof(Del_DownloadHandlerTexture_CUSTOM_Create));
            DownloadHandlerTexture_CUSTOM_InternalGetTextureNative = (Del_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative)Marshal.GetDelegateForFunctionPointer((IntPtr)(unityPlayer + ClientSettings.UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative), typeof(Del_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative));

            methodAddQuitting = coreModuleAssembly.GetClass("Application", "UnityEngine").GetMethod("add_quitting");

            InitSoloTypes();
        }

        public static void Init()
        {
            methodAddQuitting.Invoke(new IntPtr[] { UnityEngine.Events._UnityAction.CreateAction(OnQuitting) });
            LoadSoloData();
        }

        static Action OnQuitting = () =>
        {
            IsQuitting = true;
        };

        // Load a texture / sprite https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735
        // Save a texture https://github.com/sinai-dev/UniverseLib/blob/6e1654b9bc822cde06d3a845182e86e861878d14/src/Runtime/TextureHelper.cs#L100-L151

        static byte[] TextureToPNG(IntPtr texture)
        {
            bool swappedActiveRenderTexture = false;
            IL2Object origRenderTexture = null;
            try
            {
                bool isReadable = methodGetIsReadable.Invoke(texture).GetValueRef<bool>();
                int format = methodGetFormat.Invoke(texture).GetValueRef<int>();
                if (format != TextureFormat_ARGB32 || !isReadable)
                {
                    // TODO: Might want to do an Object.Destroy after using the new texture?
                    int origFilter = methodGetFilterMode.Invoke(texture).GetValueRef<int>();
                    origRenderTexture = methodGetActive.Invoke();

                    int width = methodGetWidth.Invoke(texture).GetValueRef<int>();
                    int height = methodGetHeight.Invoke(texture).GetValueRef<int>();
                    if (width == 0 || height == 0)
                    {
                        return null;
                    }
                    int depthBuffer = 0;
                    int renderTextureFormat = RenderTextureFormat_ARGB32;
                    IL2Object rt = methodGetTemporary.Invoke(new IntPtr[] { new IntPtr(&width), new IntPtr(&height), new IntPtr(&depthBuffer), new IntPtr(&renderTextureFormat) });
                    if (rt == null)
                    {
                        return null;
                    }
                    int filterMode = FilterMode_Point;
                    methodSetFilterMode.Invoke(rt.ptr, new IntPtr[] { new IntPtr(&filterMode) });
                    methodSetActive.Invoke(new IntPtr[] { rt.ptr });
                    swappedActiveRenderTexture = true;
                    methodBlit.Invoke(new IntPtr[] { texture, rt.ptr });

                    IntPtr newTexture = Import.Object.il2cpp_object_new(texture2DClassInfo.ptr);
                    if (newTexture == IntPtr.Zero)
                    {
                        return null;
                    }
                    methodTexture2DCtor.Invoke(newTexture, new IntPtr[] { new IntPtr(&width), new IntPtr(&height) });
                    Rect sourceRect = new Rect(0, 0, width, height);
                    int destX = 0, destY = 0;
                    bool updateMipmaps = false, makeNoLongerReadable = false;
                    methodReadPixels.Invoke(newTexture, new IntPtr[] { new IntPtr(&sourceRect), new IntPtr(&destX), new IntPtr(&destY) });
                    methodApply.Invoke(newTexture, new IntPtr[] { new IntPtr(&updateMipmaps), new IntPtr(&makeNoLongerReadable) });
                    methodSetFilterMode.Invoke(newTexture, new IntPtr[] { new IntPtr(&origFilter) });
                    texture = newTexture;

                    methodReleaseTemporary.Invoke(new IntPtr[] { rt.ptr });
                }

                IL2Object textureData = methodEncodeToJPG.Invoke(new IntPtr[] { texture });
                if (textureData != null)
                {
                    byte[] buffer = new IL2Array<byte>(textureData.ptr).ToByteArray();
                    if (buffer.Length > 0)
                    {
                        using (MemoryStream inMs = new MemoryStream(buffer))
                        using (MemoryStream outMs = new MemoryStream())
                        {
                            System.Drawing.Bitmap.FromStream(inMs).Save(outMs, System.Drawing.Imaging.ImageFormat.Png);
                            return outMs.ToArray();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TextureToPNG failed. Exception: " + e);
            }
            finally
            {
                if (swappedActiveRenderTexture)
                {
                    methodSetActive.Invoke(new IntPtr[] { origRenderTexture != null ? origRenderTexture.ptr : IntPtr.Zero });
                }
            }
            return null;
        }

        public static IntPtr TextureFromPNG(string filePath, string assetName, bool mipChain = true)
        {
            IL2Object bytes = methodReadAllBytes.Invoke(new IntPtr[] { new IL2String(filePath).ptr });
            if (bytes != null)
            {
                IntPtr newTexture = Import.Object.il2cpp_object_new(texture2DClassInfo.ptr);
                if (newTexture != IntPtr.Zero)
                {
                    int textureWidth = 2, textureHeight = 2;
                    int textureFormat = 4;// RGBA32
                    methodTexture2DCtor2.Invoke(newTexture, new IntPtr[] { new IntPtr(&textureWidth), new IntPtr(&textureHeight), new IntPtr(&textureFormat), new IntPtr(&mipChain) });
                    methodLoadImage.Invoke(new IntPtr[] { newTexture, bytes.ptr });
                    if (!string.IsNullOrEmpty(assetName))
                    {
                        methodSetName.Invoke(newTexture, new IntPtr[] { new IL2String(assetName).ptr });
                    }
                    return newTexture;
                }
            }
            return IntPtr.Zero;
        }

        public static IntPtr SpriteFromTexture(IntPtr texture, string assetName, Rect rect = default(Rect), float pixelsPerUnit = 0)
        {
            if (texture == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            int width = methodGetWidth.Invoke(texture).GetValueRef<int>();
            int height = methodGetHeight.Invoke(texture).GetValueRef<int>();
            if (rect.Equals(default(Rect)))
            {
                rect = new Rect(0, 0, width, height);
            }
            if (pixelsPerUnit == 0)
            {
                pixelsPerUnit = GetResourceType() == ResourceType.HighEnd_HD ? 100 : 50;
            }
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            IL2Object newSpriteAsset = methodSpriteCreate.Invoke(
                new IntPtr[] { texture, new IntPtr(&rect), new IntPtr(&pivot), new IntPtr(&pixelsPerUnit) });
            if (newSpriteAsset != null)
            {
                methodSetName.Invoke(newSpriteAsset.ptr, new IntPtr[] { new IL2String(assetName).ptr });
                return newSpriteAsset.ptr;
            }
            return IntPtr.Zero;
        }

        public static IntPtr SpriteFromPNG(string filePath, string assetName, Rect rect = default(Rect), float pixelsPerUnit = 0)
        {
            IntPtr newTextureAsset = TextureFromPNG(filePath, assetName);
            if (newTextureAsset != IntPtr.Zero)
            {
                return SpriteFromTexture(newTextureAsset, assetName, rect, pixelsPerUnit);
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetSpriteTexture(IntPtr sprite)
        {
            IL2Object result = methodGetTexture.Invoke(sprite);
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static void GetTextureSize(IntPtr texture, out int width, out int height)
        {
            width = methodGetWidth.Invoke(texture).GetValueRef<int>();
            height = methodGetHeight.Invoke(texture).GetValueRef<int>();
        }

        public static void GetSpriteSize(IntPtr sprite, out int width, out int height)
        {
            IntPtr texture = GetSpriteTexture(sprite);
            if (texture != IntPtr.Zero)
            {
                GetTextureSize(texture, out width, out height);
            }
            else
            {
                width = 0;
                height = 0;
            }
        }

        static bool TryLoadCustomFile(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, out uint result, bool loadImmediate)
        {
            result = 0;
            if (resourceMangerInstance != thisPtr)
            {
                IL2Object resourceDictionaryObj = fieldResourceDictionary.GetValue(thisPtr);
                if (resourceDictionaryObj == null)
                {
                    return false;
                }
                resourceMangerInstance = thisPtr;
                resourceDictionary = new IL2DictionaryExplicit(resourceDictionaryObj.ptr,
                    Assembler.GetAssembly("mscorlib").GetClass(typeof(uint).Name, typeof(uint).Namespace), resourceClassInfo);
            }
            if (pathPtr == IntPtr.Zero)
            {
                return false;
            }
            string path = new IL2String(pathPtr).ToString();
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            string loadPath = ConvertAssetPath(path);
            string customAssetPath = null;
            AssetType assetType = AssetType.Image;
            if (loadPath.StartsWith("Sound/AudioClip") && AudioLoader.Available)
            {
                assetType = AssetType.Audio;
                foreach (string format in AudioLoader.SupportedFormats)
                {
                    customAssetPath = Path.Combine(Program.ClientDataDir, loadPath + format);
                    if (File.Exists(customAssetPath))
                    {
                        break;
                    }
                }
            }
            else
            {
                string extension = ".png";
                if (path.StartsWith("WallPaper/") && path.Contains("<_CARD_ILLUST_>/Front"))
                {
                    assetType = AssetType.Wallpaper;
                    extension = ".json";
                }
                else if (path.Contains("/ProfileFrameMat"))
                {
                    loadPath = Path.Combine(loadPath, "Mat");
                    assetType = AssetType.IconFrame;
                }
                else if (loadPath.StartsWith("Protector/"))
                {
                    assetType = AssetType.Protector;
                }
                customAssetPath = Path.Combine(Program.ClientDataDir, loadPath + extension);
                if (extension == ".png" && !File.Exists(customAssetPath))
                {
                    customAssetPath = Path.ChangeExtension(customAssetPath, ".jpg");
                }
            }
            if (customAssetPath != null && File.Exists(customAssetPath))
            {
                uint crc = methodGetCrc.Invoke(new IntPtr[] { pathPtr }).GetValueRef<uint>();
                result = crc;
                if (resourceDictionary.ContainsKey((int)crc))
                {
                    IntPtr resourcePtr = resourceDictionary[(int)crc];
                    int refCount = methodGetRefCount.Invoke(resourcePtr).GetValueRef<int>();
                    refCount++;
                    methodSetRefCount.Invoke(resourcePtr, new IntPtr[] { new IntPtr(&refCount) });
                    if (completeHandler != IntPtr.Zero)
                    {
                        List<CustomAssetLoadRequest> loadRequests;
                        if (customAssetLoadRequests.TryGetValue(path, out loadRequests))
                        {
                            if (loadRequests == null)
                            {
                                customAssetLoadRequests[path] = loadRequests = new List<CustomAssetLoadRequest>();
                            }
                            loadRequests.Add(new CustomAssetLoadRequest()
                            {
                                CompleteHandler = completeHandler,
                                GcHandle = Import.Handler.il2cpp_gchandle_new(completeHandler, true)
                            });
                            return true;
                        }
                        methodInvoke.Invoke(completeHandler, new IntPtr[] { pathPtr });
                    }
                    return true;
                }
                else
                {
                    IntPtr resourcePtr = Import.Object.il2cpp_object_new(resourceClassInfo.ptr);
                    if (resourcePtr == IntPtr.Zero)
                    {
                        return false;
                    }
                    methodResourceCtor.Invoke(resourcePtr);

                    int resType = (int)ResourceManager_Resource_Type.LocalFile;
                    int queueId = (int)ResourceManager_ReqType.Default;
                    int refCount = 1;
                    if (assetType == AssetType.Audio && path.Contains("/BGM_"))
                    {
                        // They load BGM audio like this:
                        // ~ Loading screen ~
                        // Load(BGM_DUEL_XXX)
                        // Unload(BGM_DUEL_XXX)
                        // ~ In-duel ~
                        // LoadImmediate(BGM_DUEL_XXX)

                        // The LoadImmediate call would result in slow loading of custom audio due to the previous Unload.
                        // To fix this we force the BGM to stay loaded by increasing the ref count by 1 (will stay loaded forever).
                        // TODO: Instead of this we could skip the first Unload call then manually call Unload when the duel ends
                        refCount++;
                    }
                    IntPtr[] arg = new IntPtr[1];
                    arg[0] = new IntPtr(&refCount); methodSetRefCount.Invoke(resourcePtr, arg);
                    arg[0] = new IntPtr(&resType); methodSetResType.Invoke(resourcePtr, arg);
                    arg[0] = systemTypeInstance; methodSetSystemType.Invoke(resourcePtr, arg);
                    arg[0] = new IntPtr(&queueId); methodSetQueueId.Invoke(resourcePtr, arg);
                    arg[0] = pathPtr; methodSetPath.Invoke(resourcePtr, arg);
                    arg[0] = new IL2String(loadPath).ptr; methodSetLoadPath.Invoke(resourcePtr, arg);
                    resourceDictionary.Add((int)crc, resourcePtr);

                    switch (assetType)
                    {
                        case AssetType.Audio:
                            LoadCustomAudio(thisPtr, pathPtr, systemTypeInstance, completeHandler, disableErrorNotify, path, loadPath, customAssetPath, resourcePtr, false);//loadImmediate);
                            break;
                        case AssetType.Image:
                            LoadCustomImage(thisPtr, pathPtr, systemTypeInstance, completeHandler, disableErrorNotify, path, loadPath, customAssetPath, resourcePtr, loadImmediate);
                            break;
                        case AssetType.Protector:
                            LoadCustomMaterial(thisPtr, pathPtr, systemTypeInstance, completeHandler, disableErrorNotify, path, loadPath, customAssetPath, resourcePtr, assetType, loadImmediate);
                            break;
                        case AssetType.IconFrame:
                            LoadCustomMaterial(thisPtr, pathPtr, systemTypeInstance, completeHandler, disableErrorNotify, path, loadPath, customAssetPath, resourcePtr, assetType, loadImmediate);
                            break;
                        case AssetType.Wallpaper:
                            LoadCustomWallpaper(thisPtr, pathPtr, systemTypeInstance, completeHandler, disableErrorNotify, path, loadPath, customAssetPath, resourcePtr);
                            break;
                    }
                    return true;
                }
            }
            return false;
        }

        static void LoadCustomAudio(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, string path, string loadPath, string customAssetPath, IntPtr resourcePtr, bool loadImmediate)
        {
            IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(1, objectClassInfo);

            uint assetsArrayGcHandle = Import.Handler.il2cpp_gchandle_new(assetsArray.ptr, true);
            uint resourcePtrHandle = Import.Handler.il2cpp_gchandle_new(resourcePtr, true);
            uint completeHandlerHandle = Import.Handler.il2cpp_gchandle_new(completeHandler, true);

            Action<Action> invokeImmediate = (Action action) =>
            {
                action();
            };
            Action<Action> invoke = loadImmediate ? invokeImmediate : Win32Hooks.Invoke;

            WaitCallback work = (o) =>
            {
                float volume = 1;
                bool hasCustomVolume = false;
                try
                {
                    string volumeFile = Path.Combine(Path.GetDirectoryName(customAssetPath), Path.GetFileNameWithoutExtension(customAssetPath) + ".txt");
                    if (File.Exists(volumeFile))
                    {
                        using (StreamReader reader = File.OpenText(volumeFile))
                        {
                            if (float.TryParse(reader.ReadLine(), out volume))
                            {
                                hasCustomVolume = true;
                            }
                        }
                    }
                }
                catch
                {
                }

                string name = Path.GetFileNameWithoutExtension(customAssetPath);
                AutoResetEvent waitForMainThread = new AutoResetEvent(false);

                // TODO: Make this buffer bigger depending on file size?
                // NOTE: 2048*2048 = 4MB which might be overkill for sound effects
                int bufferSize = 2048 * 2048;//128;
                if (audioBufferTLS == null)
                {
                    audioBufferTLS = new float[bufferSize];
                }
                float[] buffer = audioBufferTLS;

                IAudioLoader audioLoader = AudioLoader.CreateInstance();
                AudioInfo info = audioLoader != null ? audioLoader.Open(customAssetPath) : null;
                if (info == null)
                {
                    if (audioLoader != null)
                    {
                        audioLoader.Close();
                    }
                    invoke(() =>
                    {
                        if (IsQuitting)
                        {
                            return;
                        }
                        FinishLoad(assetsArray, resourcePtr, path, completeHandler);
                    });
                    return;
                }

                IntPtr audioClip = IntPtr.Zero;
                uint audioClipGcHandle = 0;
                //IL2Array<float> data = null;
                //uint dataGcHandle = 0;
                invoke(() =>
                {
                    if (IsQuitting)
                    {
                        return;
                    }
                    if (audioBufferIL2CPP == IntPtr.Zero)
                    {
                        // As we're working on a single thread we can save having to create garbage by
                        // creating one float array and reusing it
                        audioBufferIL2CPP = new IL2Array<float>(bufferSize, IL2SystemClass.Float).ptr;
                        Import.Handler.il2cpp_gchandle_new(audioBufferIL2CPP, true);
                    }
                    audioClip = AudioClip_CUSTOM_Construct_Internal();
                    AudioClip_CUSTOM_CreateUserSound(audioClip, new IL2String(name).ptr, info.LengthSamples / info.Channels, info.Channels, info.SampleRate, false);
                    audioClipGcHandle = Import.Handler.il2cpp_gchandle_new(audioClip, true);
                    //data = new IL2Array<float>(buffer.Length, IL2SystemClass.Float);
                    //dataGcHandle = Import.Handler.il2cpp_gchandle_new(data.ptr, true);
                    waitForMainThread.Set();
                });
                waitForMainThread.WaitOne();

                int index = 0;
                int dataLen = bufferSize;
                while (index < info.LengthSamples)
                {
                    int read = audioLoader.Read(buffer);

                    if (read == 0)
                        break;

                    if (hasCustomVolume)
                    {
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] *= volume;
                        }
                    }

                    invoke(() =>
                    {
                        if (IsQuitting)
                        {
                            return;
                        }
                        if (index + bufferSize >= info.LengthSamples)
                        {
                            dataLen = info.LengthSamples - index;
                            //Import.Handler.il2cpp_gchandle_free(dataGcHandle);
                            //data = new IL2Array<float>(buffer.Length, IL2SystemClass.Float);
                            //dataGcHandle = Import.Handler.il2cpp_gchandle_new(data.ptr, true);
                        }

                        //Marshal.Copy(buffer, 0, (IntPtr)((long*)data.ptr + 4), dataLen);
                        //AudioClip_CUSTOM_SetData(audioClip, data.ptr, dataLen / info.Channels, index / info.Channels);

                        Marshal.Copy(buffer, 0, (IntPtr)((long*)audioBufferIL2CPP + 4), dataLen);
                        AudioClip_CUSTOM_SetData(audioClip, audioBufferIL2CPP, dataLen / info.Channels, index / info.Channels);

                        waitForMainThread.Set();
                    });
                    waitForMainThread.WaitOne();

                    index += read;
                }

                invoke(() =>
                {
                    if (IsQuitting)
                    {
                        return;
                    }
                    assetsArray[0] = audioClip;
                    FinishLoad(assetsArray, resourcePtr, path, completeHandler);
                    Import.Handler.il2cpp_gchandle_free(audioClipGcHandle);
                    //Import.Handler.il2cpp_gchandle_free(dataGcHandle);
                    Import.Handler.il2cpp_gchandle_free(completeHandlerHandle);
                    Import.Handler.il2cpp_gchandle_free(resourcePtrHandle);
                    Import.Handler.il2cpp_gchandle_free(assetsArrayGcHandle);
                });

                audioLoader.Close();
            };

            customAssetLoadRequests[path] = null;
            if (loadImmediate)
            {
                work(null);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(work);
            }
        }

        static void LoadCustomImage(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, string path, string loadPath, string customAssetPath, IntPtr resourcePtr, bool loadImmediate)
        {
            if (ClientSettings.DisableAsyncImageLoad)
            {
                loadImmediate = true;
            }

            Rect rect = default(Rect);
            bool mipChain = true;
            bool setPointFilterMode = false;

            // Card packs have some weird artifacting if the sprite isn't the correct size
            // NOTE: Explicitly check for "CardPackTex" to allow for custom images which ignore this code
            if (loadPath.StartsWith("Images/CardPack/") && loadPath.Contains("CardPackTex"))
            {
                if (loadPath.Contains("/SD/"))
                {
                    rect = new Rect(2.292893f, 0, 250.4142f, 512);
                }
                else
                {
                    rect = new Rect(7.292893f, 0, 496.4142f, 1024);
                }
                mipChain = false;
            }
            if (loadPath.StartsWith("LinkEvolution/"))
            {
                // Prevent blurry images. NOTE: Removed as this screws up when scaling the images
                //setPointFilterMode = true;
            }

            bool hasSprite = true;

            if (loadPath.StartsWith("Card/Images/Illust/"))
            {
                // Card images need to be just a Texture2D (no sprite)
                hasSprite = false;
            }

            if (loadImmediate)
            {
                IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(hasSprite ? 2 : 1, objectClassInfo);
                if (assetsArray.ptr == IntPtr.Zero)
                {
                    return;
                }

                string assetName = Path.GetFileNameWithoutExtension(customAssetPath);
                IntPtr newTextureAsset = TextureFromPNG(customAssetPath, assetName, mipChain);
                if (setPointFilterMode)
                {
                    int filterMode = FilterMode_Point;
                    methodSetFilterMode.Invoke(newTextureAsset, new IntPtr[] { new IntPtr(&filterMode) });
                }
                assetsArray[0] = newTextureAsset;

                if (hasSprite)
                {
                    IntPtr newSpriteAsset = SpriteFromTexture(newTextureAsset, assetName, rect);
                    if (newSpriteAsset != IntPtr.Zero)
                    {
                        assetsArray[1] = newSpriteAsset;
                    }
                }

                FinishLoad(assetsArray, resourcePtr, path, completeHandler);
            }
            else
            {
                customAssetLoadRequests[path] = null;

                uint resourcePtrHandle = Import.Handler.il2cpp_gchandle_new(resourcePtr, true);
                uint completeHandlerHandle = Import.Handler.il2cpp_gchandle_new(completeHandler, true);

                string assetName = Path.GetFileNameWithoutExtension(customAssetPath);
                AsyncTextureFromPNG(customAssetPath, assetName, (IntPtr texture) =>
                {
                    if (texture == IntPtr.Zero)
                    {
                        //Console.WriteLine("Not found");
                        bool disableNotify = disableErrorNotify;
                        methodCallErrorHandler.Invoke(thisPtr, new IntPtr[] { resourcePtr, new IntPtr(&disableNotify) });
                        customAssetLoadRequests.Remove(path);
                    }
                    else
                    {
                        if (setPointFilterMode)
                        {
                            int filterMode = FilterMode_Point;
                            methodSetFilterMode.Invoke(texture, new IntPtr[] { new IntPtr(&filterMode) });
                        }

                        //Console.WriteLine("Found. Refs: " + methodGetRefCount.Invoke(resourcePtr).GetValueRef<int>());
                        IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(hasSprite ? 2 : 1, objectClassInfo);
                        if (assetsArray.ptr == IntPtr.Zero)
                        {
                            customAssetLoadRequests.Remove(path);
                            return;
                        }

                        assetsArray[0] = texture;

                        if (hasSprite)
                        {
                            IntPtr newSpriteAsset = SpriteFromTexture(texture, assetName, rect);
                            if (newSpriteAsset != IntPtr.Zero)
                            {
                                assetsArray[1] = newSpriteAsset;
                            }
                        }

                        FinishLoad(assetsArray, resourcePtr, path, completeHandler);
                    }

                    Import.Handler.il2cpp_gchandle_free(completeHandlerHandle);
                    Import.Handler.il2cpp_gchandle_free(resourcePtrHandle);
                });
            }
        }

        static void AsyncTextureFromPNG(string filePath, string assetName, Action<IntPtr> callback)
        {
            AsyncLoadTextureRequest request = new AsyncLoadTextureRequest();
            request.Callback = callback;
            request.RequestTime = DateTime.UtcNow;

            // Should be a DownloadHandlerTexture but the game doesn't have it
            request.DownloadHandler = Import.Object.il2cpp_object_new(downloadHandlerClass.ptr);
            if (request.DownloadHandler == IntPtr.Zero)
            {
                Console.WriteLine("TODO: Handle failing of DownloadHandler ctor");
                callback(IntPtr.Zero);
                return;
            }
            downloadHandlerCtor.Invoke(request.DownloadHandler);

            // +16 is accessed in DownloadHandlerTexture_CUSTOM_InternalGetTextureNative
            // TODO: The false param will need to be updated in a future unity version as they add additional params for mipmap options
            *(IntPtr*)(request.DownloadHandler + 16) = DownloadHandlerTexture_CUSTOM_Create(request.DownloadHandler, false);

            request.WebRequest = Import.Object.il2cpp_object_new(webRequestClass.ptr);
            if (request.WebRequest == IntPtr.Zero)
            {
                Console.WriteLine("TODO: Handle failing of UnityWebRequest ctor");
                callback(IntPtr.Zero);
                return;
            }

            bool autoDisposeDownloadHandler = true;
            webRequestSetAutoDisposeDownloadHandler.Invoke(request.WebRequest, new IntPtr[] { new IntPtr(&autoDisposeDownloadHandler) });

            string uri = "file://";
            try
            {
                uri = uri + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filePath)).Replace("\\", "/");
                //Console.WriteLine(filePath);
            }
            catch
            {
            }
            webRequestCtor.Invoke(request.WebRequest, new IntPtr[] { new IL2String(uri).ptr, new IL2String("GET").ptr, request.DownloadHandler, IntPtr.Zero });

            Import.Handler.il2cpp_gchandle_new(request.DownloadHandler, true);
            Import.Handler.il2cpp_gchandle_new(request.WebRequest, true);
            webRequestSend.Invoke(request.WebRequest);

            asyncLoadTextureRequests.Add(request);
        }

        public static void Update()
        {
            /*if (asyncLoadTextureRequests.Count > 0)
            {
                Console.WriteLine("asyncLoadTextureRequests: " + asyncLoadTextureRequests.Count);
            }
            if (assetLoadRequests.Count > 0)
            {
                Console.WriteLine("assetLoadRequests: " + assetLoadRequests.Count);
            }*/

            const int maxPerFrame = 2;
            for (int i = asyncLoadTextureRequests.Count - 1, j = 0; i >= 0; i--)
            {
                AsyncLoadTextureRequest request = asyncLoadTextureRequests[i];
                int result = webRequestGetResult.Invoke(request.WebRequest).GetValueRef<int>();
                // UnityWebRequest.Result.InProgress = 0
                // UnityWebRequest.Result.Success = 1
                if (result != 0 || request.RequestTime < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    IntPtr texture = result != 1 ? IntPtr.Zero : DownloadHandlerTexture_CUSTOM_InternalGetTextureNative(request.DownloadHandler);
                    request.Callback(texture);
                    webRequestDispose.Invoke(request.WebRequest);
                    asyncLoadTextureRequests.RemoveAt(i);
                    j++;
                    if (j >= maxPerFrame)
                    {
                        break;// Limit how many you do per frame
                    }
                }
            }

            for (int i = assetLoadRequests.Count - 1; i >= 0; i--)
            {
                AsyncLoadRequest request = assetLoadRequests[i];
                if (methodGetDone.Invoke(request.ResouecePtr).GetValueRef<csbool>() ||
                    request.RequestTime < DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    request.Callback();
                    assetLoadRequests.RemoveAt(i);
                }
            }
        }

        static void LoadCustomMaterial(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, string path, string loadPath, string customAssetPath, IntPtr resourcePtr, AssetType assetType, bool loadImmediate)
        {
            if (ClientSettings.DisableAsyncImageLoad)
            {
                loadImmediate = true;
            }
            if (!loadImmediate)
            {
                LoadCustomMaterialAsync(thisPtr, pathPtr, systemTypeInstance, completeHandler, disableErrorNotify, path, loadPath, customAssetPath, resourcePtr, assetType);
                return;
            }

            string baseMat = null;
            const string baseMatKey = "BaseMat";
            string dir = Path.GetDirectoryName(customAssetPath);
            string settingsFile = Path.Combine(dir, Path.GetFileNameWithoutExtension(customAssetPath) + ".json");
            Dictionary<string, object> settings = null;
            if (File.Exists(settingsFile))
            {
                settings = MiniJSON.Json.DeserializeStripped(File.ReadAllText(settingsFile)) as Dictionary<string, object>;
                if (settings != null)
                {
                    baseMat = Utils.GetValue<int>(settings, baseMatKey).ToString().PadLeft(4, '0');
                }
            }

            bool isDuluxe = false;
            string extraImg = Path.Combine(dir, Path.GetFileNameWithoutExtension(customAssetPath) + "_1.png");

            IntPtr existingAssetPath;
            if (assetType == AssetType.Protector)
            {
                isDuluxe = File.Exists(extraImg);
                if (string.IsNullOrEmpty(baseMat))
                {
                    baseMat = (isDuluxe ? "2001" : "0005");
                }
                existingAssetPath = new IL2String("Protector/tcg/" + baseMat + "/PMat").ptr;
            }
            else
            {
                if (string.IsNullOrEmpty(baseMat))
                {
                    baseMat = "1030001";
                }
                existingAssetPath = new IL2String("Images/ProfileFrame/ProfileFrameMat" + baseMat).ptr;
            }

            hookLoadImmediate.Original(thisPtr, existingAssetPath, IntPtr.Zero, IntPtr.Zero, false);
            IntPtr workPathPtr = IntPtr.Zero;
            IntPtr existingAssetResourcePtr = hookGetResource.Original(thisPtr, existingAssetPath, (IntPtr)(&workPathPtr));
            bool force = false;
            methodUnload.Invoke(thisPtr, new IntPtr[] { existingAssetPath, new IntPtr(&force) });
            if (existingAssetResourcePtr == IntPtr.Zero)
            {
                return;
            }
            IL2Object existingAssetsArrayObj = methodGetAssets.Invoke(existingAssetResourcePtr);
            if (existingAssetsArrayObj == null)
            {
                return;
            }
            IL2Array<IntPtr> existingAssetsArray = new IL2Array<IntPtr>(existingAssetsArrayObj.ptr);
            if (existingAssetsArray.Length == 0)
            {
                return;
            }
            IntPtr existingObj = existingAssetsArray[0];
            if (existingObj == IntPtr.Zero)
            {
                return;
            }
            IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(1, objectClassInfo);
            assetsArray[0] = UnityEngine.UnityObject.Instantiate(existingObj);

            string assetName = Path.GetFileNameWithoutExtension(customAssetPath);
            IntPtr newTextureAsset = TextureFromPNG(customAssetPath, assetName);
            methodSetTexture.Invoke(assetsArray[0], new IntPtr[] { new IL2String("_MainTex").ptr, newTextureAsset });
            if (isDuluxe)
            {
                string assetName2 = Path.GetFileNameWithoutExtension(extraImg);
                IntPtr newTextureAsset2 = TextureFromPNG(extraImg, assetName2);
                methodSetTexture.Invoke(assetsArray[0], new IntPtr[] { new IL2String("_MainTex2").ptr, newTextureAsset2 });
            }

            if (settings != null)
            {
                foreach (KeyValuePair<string, object> entry in settings)
                {
                    if (entry.Key == baseMatKey || entry.Value == null)
                    {
                        continue;
                    }
                    TypeCode typeCode = Type.GetTypeCode(entry.Value.GetType());
                    switch (typeCode)
                    {
                        case TypeCode.Double:
                            {
                                float val = Convert.ToSingle(entry.Value);
                                methodSetFloat.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, new IntPtr(&val) });
                            }
                            break;
                        case TypeCode.Int64:
                            {
                                int val = Convert.ToInt32(entry.Value);
                                methodSetInt.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, new IntPtr(&val) });
                            }
                            break;
                        case TypeCode.String:
                            {
                                string[] splitted = (entry.Value as string).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splitted.Length == 4)
                                {
                                    float* color = stackalloc float[4];
                                    if (splitted.Length > 0) float.TryParse(splitted[0], out color[0]);
                                    if (splitted.Length > 1) float.TryParse(splitted[1], out color[1]);
                                    if (splitted.Length > 2) float.TryParse(splitted[2], out color[2]);
                                    if (splitted.Length > 3) float.TryParse(splitted[3], out color[3]);
                                    methodSetColor.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, new IntPtr(color) });
                                }
                                else if (splitted.Length > 0)
                                {
                                    string imageFile = Path.Combine(dir, splitted[0] + ".png");
                                    if (File.Exists(imageFile))
                                    {
                                        IntPtr tex = TextureFromPNG(imageFile, splitted[0]);
                                        methodSetTexture.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, tex });
                                    }
                                }
                            }
                            break;
                        default:
                            Console.WriteLine("Unsupported type " + typeCode + " in material value setter");
                            break;
                    }
                }
            }

            FinishLoad(assetsArray, resourcePtr, path, completeHandler);
        }

        static void LoadCustomMaterialAsync(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, string path, string loadPath, string customAssetPath, IntPtr resourcePtr, AssetType assetType)
        {
            // TODO: Correct cleanup
            uint resourcePtrHandle = Import.Handler.il2cpp_gchandle_new(resourcePtr, true);
            uint completeHandlerHandle = Import.Handler.il2cpp_gchandle_new(completeHandler, true);

            string baseMat = null;
            const string baseMatKey = "BaseMat";
            string dir = Path.GetDirectoryName(customAssetPath);
            string settingsFile = Path.Combine(dir, Path.GetFileNameWithoutExtension(customAssetPath) + ".json");
            Dictionary<string, object> settings = null;
            if (File.Exists(settingsFile))
            {
                settings = MiniJSON.Json.DeserializeStripped(File.ReadAllText(settingsFile)) as Dictionary<string, object>;
                if (settings != null)
                {
                    baseMat = Utils.GetValue<int>(settings, baseMatKey).ToString().PadLeft(4, '0');
                }
            }

            bool isDuluxe = false;
            string extraImg = Path.Combine(dir, Path.GetFileNameWithoutExtension(customAssetPath) + "_1.png");

            IntPtr existingAssetPath;
            if (assetType == AssetType.Protector)
            {
                isDuluxe = File.Exists(extraImg);
                if (string.IsNullOrEmpty(baseMat))
                {
                    baseMat = (isDuluxe ? "2001" : "0005");
                }
                existingAssetPath = new IL2String("Protector/tcg/" + baseMat + "/PMat").ptr;
            }
            else
            {
                if (string.IsNullOrEmpty(baseMat))
                {
                    baseMat = "1030001";
                }
                existingAssetPath = new IL2String("Images/ProfileFrame/ProfileFrameMat" + baseMat).ptr;
            }
            uint existingAssetPathHandle = Import.Handler.il2cpp_gchandle_new(existingAssetPath, true);

            uint crc = hookLoad.Original(thisPtr, existingAssetPath, IntPtr.Zero, IntPtr.Zero, false);
            if (!resourceDictionary.ContainsKey((int)crc))
            {
                Console.WriteLine("ASYNC_TODO_CLEANUP-1");
                bool disableNotify = disableErrorNotify;
                methodCallErrorHandler.Invoke(thisPtr, new IntPtr[] { resourcePtr, new IntPtr(&disableNotify) });
                return;
            }

            IntPtr existingAssetResourcePtr = resourceDictionary[(int)crc];
            Action callback = () =>
            {
                IntPtr workPathPtr = IntPtr.Zero;
                bool force = false;
                methodUnload.Invoke(thisPtr, new IntPtr[] { existingAssetPath, new IntPtr(&force) });
                if (existingAssetResourcePtr == IntPtr.Zero)
                {
                    Console.WriteLine("ASYNC_TODO_CLEANUP-2");
                    return;
                }
                IL2Object existingAssetsArrayObj = methodGetAssets.Invoke(existingAssetResourcePtr);
                if (existingAssetsArrayObj == null)
                {
                    Console.WriteLine("ASYNC_TODO_CLEANUP-3");
                    return;
                }
                IL2Array<IntPtr> existingAssetsArray = new IL2Array<IntPtr>(existingAssetsArrayObj.ptr);
                if (existingAssetsArray.Length == 0)
                {
                    Console.WriteLine("ASYNC_TODO_CLEANUP-4");
                    return;
                }
                IntPtr existingObj = existingAssetsArray[0];
                if (existingObj == IntPtr.Zero)
                {
                    Console.WriteLine("ASYNC_TODO_CLEANUP-5");
                    return;
                }
                IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(1, objectClassInfo);
                assetsArray[0] = UnityEngine.UnityObject.Instantiate(existingObj);
                uint assetsGcHandle = Import.Handler.il2cpp_gchandle_new(assetsArray.ptr, true);

                // TODO: Load both textures at the same time (and during the Load call)
                string assetName = Path.GetFileNameWithoutExtension(customAssetPath);
                AsyncTextureFromPNG(customAssetPath, assetName, (IntPtr newTextureAsset) =>
                {
                    if (newTextureAsset == IntPtr.Zero)
                    {
                        Console.WriteLine("ASYNC_TODO_CLEANUP-6");
                        bool disableNotify = disableErrorNotify;
                        methodCallErrorHandler.Invoke(thisPtr, new IntPtr[] { resourcePtr, new IntPtr(&disableNotify) });
                        return;
                    }

                    methodSetTexture.Invoke(assetsArray[0], new IntPtr[] { new IL2String("_MainTex").ptr, newTextureAsset });

                    string assetName2 = Path.GetFileNameWithoutExtension(extraImg);
                    Action<IntPtr> onTextureLoadFinished = (IntPtr newTextureAsset2) =>
                    {
                        if (newTextureAsset2 != IntPtr.Zero)
                        {
                            methodSetTexture.Invoke(assetsArray[0], new IntPtr[] { new IL2String("_MainTex2").ptr, newTextureAsset2 });
                        }

                        if (settings != null)
                        {
                            foreach (KeyValuePair<string, object> entry in settings)
                            {
                                if (entry.Key == baseMatKey || entry.Value == null)
                                {
                                    continue;
                                }
                                TypeCode typeCode = Type.GetTypeCode(entry.Value.GetType());
                                switch (typeCode)
                                {
                                    case TypeCode.Double:
                                        {
                                            float val = Convert.ToSingle(entry.Value);
                                            methodSetFloat.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, new IntPtr(&val) });
                                        }
                                        break;
                                    case TypeCode.Int64:
                                        {
                                            int val = Convert.ToInt32(entry.Value);
                                            methodSetInt.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, new IntPtr(&val) });
                                        }
                                        break;
                                    case TypeCode.String:
                                        {
                                            string[] splitted = (entry.Value as string).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (splitted.Length == 4)
                                            {
                                                float* color = stackalloc float[4];
                                                if (splitted.Length > 0) float.TryParse(splitted[0], out color[0]);
                                                if (splitted.Length > 1) float.TryParse(splitted[1], out color[1]);
                                                if (splitted.Length > 2) float.TryParse(splitted[2], out color[2]);
                                                if (splitted.Length > 3) float.TryParse(splitted[3], out color[3]);
                                                methodSetColor.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, new IntPtr(color) });
                                            }
                                            else if (splitted.Length > 0)
                                            {
                                                string imageFile = Path.Combine(dir, splitted[0] + ".png");
                                                if (File.Exists(imageFile))
                                                {
                                                    IntPtr tex = TextureFromPNG(imageFile, splitted[0]);
                                                    methodSetTexture.Invoke(assetsArray[0], new IntPtr[] { new IL2String(entry.Key).ptr, tex });
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        Console.WriteLine("Unsupported type " + typeCode + " in material value setter");
                                        break;
                                }
                            }
                        }

                        Import.Handler.il2cpp_gchandle_free(existingAssetPathHandle);
                        Import.Handler.il2cpp_gchandle_free(assetsGcHandle);
                        Import.Handler.il2cpp_gchandle_free(completeHandlerHandle);
                        Import.Handler.il2cpp_gchandle_free(resourcePtrHandle);

                        FinishLoad(assetsArray, resourcePtr, path, completeHandler);
                    };
                    if (isDuluxe)
                    {
                        AsyncTextureFromPNG(extraImg, assetName2, onTextureLoadFinished);
                    }
                    else
                    {
                        onTextureLoadFinished(IntPtr.Zero);
                    }
                });
            };

            assetLoadRequests.Add(new AsyncLoadRequest()
            {
                ResouecePtr = existingAssetResourcePtr,
                RequestTime = DateTime.UtcNow,
                Callback = callback
            });
        }

        static void LoadCustomWallpaper(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, string path, string loadPath, string customAssetPath, IntPtr resourcePtr)
        {
            Dictionary<string, object> settings = MiniJSON.Json.DeserializeStripped(File.ReadAllText(customAssetPath)) as Dictionary<string, object>;
            if (settings == null)
            {
                return;
            }
            int baseId = Utils.GetValue<int>(settings, "Base");
            if (baseId == 0)
            {
                return;
            }
            string baseIdString = baseId.ToString().PadLeft(4, '0');
            string idString = Path.GetFileNameWithoutExtension(customAssetPath).Replace("Front", string.Empty);
            IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(1, objectClassInfo);
            IntPtr existingAssetPath = new IL2String("WallPaper/WallPaper" + baseIdString + "/<_CARD_ILLUST_>/Front" + baseIdString).ptr;
            hookLoadImmediate.Original(thisPtr, existingAssetPath, IntPtr.Zero, IntPtr.Zero, false);
            IntPtr workPathPtr = IntPtr.Zero;
            IntPtr existingAssetResourcePtr = hookGetResource.Original(thisPtr, existingAssetPath, (IntPtr)(&workPathPtr));
            bool force = false;
            methodUnload.Invoke(thisPtr, new IntPtr[] { existingAssetPath, new IntPtr(&force) });
            if (existingAssetResourcePtr == IntPtr.Zero)
            {
                return;
            }
            IL2Object existingAssetsArrayObj = methodGetAssets.Invoke(existingAssetResourcePtr);
            if (existingAssetsArrayObj == null)
            {
                return;
            }
            IL2Array<IntPtr> existingAssetsArray = new IL2Array<IntPtr>(existingAssetsArrayObj.ptr);
            if (existingAssetsArray.Length == 0)
            {
                return;
            }
            IntPtr existingObj = existingAssetsArray[0];
            if (existingObj == IntPtr.Zero)
            {
                return;
            }
            assetsArray[0] = existingObj;
            for (int i = 0; i < assetsArray.Length; i++)
            {
                IntPtr obj = assetsArray[i];
                if (obj == IntPtr.Zero) continue;
                IntPtr type = Import.Object.il2cpp_object_get_class(obj);
                if (type == gameObjectClassInfo.ptr)
                {
                    assetsArray[0] = obj = UnityEngine.UnityObject.Instantiate(obj);
                    List<Dictionary<string, object>> objSettingsList = Utils.GetDictionaryCollection(settings, "GameObjects");
                    if (objSettingsList != null)
                    {
                        LoadCustomWallpaperGameObjects(customAssetPath, baseIdString, idString, obj, obj, objSettingsList);
                    }
                }
            }

            FinishLoad(assetsArray, resourcePtr, path, completeHandler);
        }

        static void LoadCustomWallpaperGameObjects(string customAssetPath, string baseIdString, string idString, IntPtr rootObj, IntPtr parentObj, List<Dictionary<string, object>> objSettingsList)
        {
            foreach (Dictionary<string, object> objSettings in objSettingsList)
            {
                string objName = Utils.GetValue<string>(objSettings, "Name");
                if (string.IsNullOrWhiteSpace(objName))
                {
                    continue;
                }
                objName = objName.Replace("{BASEID}", baseIdString).Replace("{ID}", idString);
                IntPtr obj = UnityEngine.GameObject.FindGameObjectByName(parentObj, objName, false, false);
                if (obj == IntPtr.Zero)
                {
                    if (obj == IntPtr.Zero)
                    {
                        obj = UnityEngine.GameObject.New();
                    }
                    UnityEngine.Transform.SetParent(UnityEngine.GameObject.GetTransform(obj), UnityEngine.GameObject.GetTransform(parentObj));
                    UnityEngine.UnityObject.SetName(obj, objName);
                }
                if (Utils.GetValue<bool>(objSettings, "Disable"))
                {
                    UnityEngine.GameObject.SetActive(obj, false);
                }
                else if (Utils.GetValue<bool>(objSettings, "Destroy"))
                {
                    UnityEngine.Transform.SetParent(UnityEngine.GameObject.GetTransform(obj), IntPtr.Zero);
                    UnityEngine.UnityObject.Destroy(obj);
                }
                else
                {
                    string imageFile = Utils.GetValue<string>(objSettings, "Image");
                    if (!string.IsNullOrWhiteSpace(imageFile))
                    {
                        imageFile = Path.Combine(Path.GetDirectoryName(customAssetPath), imageFile.Replace("{ID}", idString) + ".png");
                    }
                    if (File.Exists(imageFile))
                    {
                        IL2Object image = methodGetComponent.Invoke(obj, new IntPtr[] { imageClassInfo.IL2Typeof() });
                        if (image == null)
                        {
                            IntPtr imagePtr = UnityEngine.GameObject.AddComponent(obj, imageClassType);
                            if (imagePtr != IntPtr.Zero)
                            {
                                image = new IL2Object(imagePtr);
                            }
                        }
                        if (image != null)
                        {
                            IntPtr sprite = SpriteFromPNG(imageFile, Path.GetFileNameWithoutExtension(imageFile));
                            if (sprite != IntPtr.Zero)
                            {
                                methodSetSprite.Invoke(image.ptr, new IntPtr[] { sprite });
                                IL2Object texture = methodGetTexture.Invoke(sprite);
                                if (texture != null)
                                {
                                    int width = methodGetWidth.Invoke(texture.ptr).GetValueRef<int>();
                                    int height = methodGetHeight.Invoke(texture.ptr).GetValueRef<int>();
                                    int overrideWidth, overrideHeight;
                                    if (Utils.TryGetValue(objSettings, "Width", out overrideWidth) && overrideWidth > 0)
                                    {
                                        width = overrideWidth;
                                    }
                                    if (Utils.TryGetValue(objSettings, "Height", out overrideHeight) && overrideHeight > 0)
                                    {
                                        height = overrideHeight;
                                    }
                                    float scale = Utils.GetValue<float>(objSettings, "Scale");
                                    if (scale > 0)
                                    {
                                        width = (int)(width * scale);
                                        height = (int)(height * scale);
                                    }
                                    IntPtr rectTransform = UnityEngine.GameObject.GetComponent(obj, rectTransformClassType);
                                    Vector2 sizeDelta = new Vector2(width, height);
                                    rectTransformSetSizeDelta.Invoke(rectTransform, new IntPtr[] { new IntPtr(&sizeDelta) });
                                }
                            }
                        }
                    }

                    IntPtr[] components = UnityEngine.GameObject.GetComponents(obj);
                    List<KeyValuePair<IntPtr, string>> tweenComponents = new List<KeyValuePair<IntPtr, string>>();
                    List<Dictionary<string, object>> tweenSettingsList = Utils.GetDictionaryCollection(objSettings, "Tween");
                    if (tweenSettingsList.Count > 0)
                    {
                        foreach (IntPtr component in components)
                        {
                            IntPtr klass = Import.Object.il2cpp_object_get_class(component);
                            while (klass != IntPtr.Zero && klass != tweenClassInfo.ptr)
                            {
                                klass = Import.Class.il2cpp_class_get_parent(klass);
                            }
                            if (klass != IntPtr.Zero)
                            {
                                tweenComponents.Add(new KeyValuePair<IntPtr, string>(Import.Object.il2cpp_object_get_class(component), tweenLabel.GetValue(component).GetValueObj<string>()));
                            }
                        }
                    }

                    if (objSettings.ContainsKey("Offset"))
                    {
                        UnityEngine.Vector3 offset = GetVector3(objSettings, "Offset");

                        IntPtr showComponent = components.FirstOrDefault(x =>
                            Import.Object.il2cpp_object_get_class(x) == tweenClassInfos[TweenClassType.TweenPosition].ptr &&
                            tweenLabel.GetValue(x).GetValueObj<string>() == "Show");

                        IntPtr loopComponent = components.FirstOrDefault(x =>
                            Import.Object.il2cpp_object_get_class(x) == tweenClassInfos[TweenClassType.TweenPosition].ptr &&
                            tweenLabel.GetValue(x).GetValueObj<string>() == "Loop");

                        for (int i = 0; i < 2; i++)
                        {
                            IntPtr component = i == 0 ? showComponent : loopComponent;
                            if (component != IntPtr.Zero)
                            {
                                UnityEngine.Vector3 from = tweenPositionFrom.GetValue(component).GetValueRef<UnityEngine.Vector3>();
                                UnityEngine.Vector3 to = tweenPositionTo.GetValue(component).GetValueRef<UnityEngine.Vector3>();
                                from.x += offset.x;
                                from.y += offset.y;
                                from.z += offset.z;
                                to.x += offset.x;
                                to.y += offset.y;
                                to.z += offset.z;
                                tweenPositionFrom.SetValue(component, new IntPtr(&from));
                                tweenPositionTo.SetValue(component, new IntPtr(&to));
                            }
                        }
                    }

                    foreach (Dictionary<string, object> tweenSettings in tweenSettingsList)
                    {
                        TweenClassType tweenType = Utils.GetValue<TweenClassType>(tweenSettings, "Type");
                        IL2Class componentClassInfo;
                        tweenClassInfos.TryGetValue(tweenType, out componentClassInfo);
                        if (componentClassInfo == null)
                        {
                            continue;
                        }

                        string label = Utils.GetValue<string>(tweenSettings, "Label");
                        IntPtr component = components.FirstOrDefault(x =>
                            Import.Object.il2cpp_object_get_class(x) == componentClassInfo.ptr &&
                            tweenLabel.GetValue(x).GetValueObj<string>() == label);
                        if (component == IntPtr.Zero)
                        {
                            continue;
                        }

                        switch (tweenType)
                        {
                            case TweenClassType.TweenAlpha:
                                {
                                    float from;
                                    if (Utils.TryGetValue(tweenSettings, "From", out from))
                                    {
                                        tweenAlphaFrom.SetValue(component, new IntPtr(&from));
                                    }
                                    float to;
                                    if (Utils.TryGetValue(tweenSettings, "To", out to))
                                    {
                                        tweenAlphaTo.SetValue(component, new IntPtr(&to));
                                    }
                                    bool isRecusive;
                                    if (Utils.TryGetValue(tweenSettings, "IsRecursive", out isRecusive))
                                    {
                                        tweenAlphaIsRecursive.SetValue(component, new IntPtr(&isRecusive));
                                    }
                                }
                                break;
                            case TweenClassType.TweenPosition:
                                {
                                    UnityEngine.Vector3 from = GetVector3(tweenSettings, "From");
                                    UnityEngine.Vector3 to = GetVector3(tweenSettings, "To");
                                    tweenPositionFrom.SetValue(component, new IntPtr(&from));
                                    tweenPositionTo.SetValue(component, new IntPtr(&to));
                                }
                                break;
                            case TweenClassType.TweenRotation:
                                {
                                    UnityEngine.Vector3 from = GetVector3(tweenSettings, "From");
                                    UnityEngine.Vector3 to = GetVector3(tweenSettings, "To");
                                    tweenRotationFrom.SetValue(component, new IntPtr(&from));
                                    tweenRotationTo.SetValue(component, new IntPtr(&to));
                                    bool quaternionLerp;
                                    if (Utils.TryGetValue(tweenSettings, "QuaternionLerp", out quaternionLerp))
                                    {
                                        tweenRotationQuaternionLerp.SetValue(component, new IntPtr(&quaternionLerp));
                                    }
                                }
                                break;
                            case TweenClassType.TweenScale:
                                {
                                    UnityEngine.Vector3 from = GetVector3(tweenSettings, "From");
                                    UnityEngine.Vector3 to = GetVector3(tweenSettings, "To");
                                    tweenScaleFrom.SetValue(component, new IntPtr(&from));
                                    tweenScaleTo.SetValue(component, new IntPtr(&to));
                                }
                                break;
                        }
                    }

                    bool targetSiblingBefore = true;
                    string targetSiblingName = Utils.GetValue<string>(objSettings, "Before");
                    if (string.IsNullOrEmpty(targetSiblingName))
                    {
                        targetSiblingName = Utils.GetValue<string>(objSettings, "After");
                        targetSiblingBefore = false;
                    }
                    if (!string.IsNullOrEmpty(targetSiblingName))
                    {
                        IntPtr targetSibling = UnityEngine.GameObject.FindGameObjectByName(parentObj, targetSiblingName.Replace("{BASEID}", baseIdString), false, false);
                        if (targetSibling != IntPtr.Zero)
                        {
                            IntPtr transform = UnityEngine.GameObject.GetTransform(obj);
                            IntPtr targetSiblingTransform = UnityEngine.GameObject.GetTransform(targetSibling);
                            int index = UnityEngine.Transform.GetSiblingIndex(targetSiblingTransform);
                            UnityEngine.Transform.SetSiblingIndex(transform, targetSiblingBefore ? index : index + 1);
                        }
                    }

                    List<object> childObjSettingsList;
                    if (Utils.TryGetValue(objSettings, "Children", out childObjSettingsList))
                    {
                        LoadCustomWallpaperGameObjects(customAssetPath, baseIdString, idString, rootObj, obj, childObjSettingsList.Select(x => (Dictionary<string, object>)x).ToList());
                    }
                }
            }
        }

        static UnityEngine.Vector3 GetVector3(Dictionary<string, object> settings, string key)
        {
            UnityEngine.Vector3 result = default(UnityEngine.Vector3);
            Dictionary<string, object> resultJson = Utils.GetDictionary(settings, key);
            if (resultJson != null)
            {
                result.x = Utils.GetValue<float>(resultJson, "X");
                result.y = Utils.GetValue<float>(resultJson, "Y");
                result.z = Utils.GetValue<float>(resultJson, "Z");
            }
            return result;
        }

        static void FinishLoad(IL2Array<IntPtr> assetsArray, IntPtr resourcePtr, string path, IntPtr completeHandler)
        {
            IntPtr pathPtr = new IL2String(path).ptr;
            bool done = true;
            IntPtr[] arg = new IntPtr[1];
            arg[0] = assetsArray.ptr; methodSetAssets.Invoke(resourcePtr, arg);
            arg[0] = new IntPtr(&done); methodSetDone.Invoke(resourcePtr, arg);

            if (completeHandler != IntPtr.Zero)
            {
                methodInvoke.Invoke(completeHandler, new IntPtr[] { pathPtr });
            }

            List<CustomAssetLoadRequest> requests;
            if (customAssetLoadRequests.TryGetValue(path, out requests) && requests != null)
            {
                foreach (CustomAssetLoadRequest request in requests)
                {
                    methodInvoke.Invoke(request.CompleteHandler, new IntPtr[] { pathPtr });
                    Import.Handler.il2cpp_gchandle_free(request.GcHandle);
                }
            }
            customAssetLoadRequests.Remove(path);
        }

        public static uint Load(string path, IntPtr completeHandler)
        {
            IntPtr mgr = fieldResourceManagerInstance.GetValue().ptr;
            return Load(mgr, new IL2String(path).ptr, IntPtr.Zero, completeHandler, false);
        }

        static uint Load(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInstance, IntPtr completeHandler, csbool disableErrorNotify)
        {
            if (ClientSettings.AssetHelperLog && path != IntPtr.Zero)
            {
                Console.WriteLine(new IL2String(path).ToString());
            }
            if (ClientSettings.AssetHelperDisableFileErrorPopup)
            {
                disableErrorNotify = true;
            }
            uint result;
            if (TryLoadCustomFile(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify, out result, false))
            {
                return result;
            }
            return hookLoad.Original(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify);
        }

        static uint LoadImmediate(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInstance, IntPtr completeHandler, csbool disableErrorNotify)
        {
            if (ClientSettings.AssetHelperLog && path != IntPtr.Zero)
            {
                Console.WriteLine(new IL2String(path).ToString() + " (LoadImmediate)");
            }
            if (ClientSettings.AssetHelperDisableFileErrorPopup)
            {
                disableErrorNotify = true;
            }
            uint result;
            if (TryLoadCustomFile(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify, out result, true))
            {
                return result;
            }
            return hookLoadImmediate.Original(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify);
        }

        public static IntPtr LoadImmediateAsset(string path)
        {
            IntPtr mgr = fieldResourceManagerInstance.GetValue().ptr;
            LoadImmediate(mgr, new IL2String(path).ptr, IntPtr.Zero, IntPtr.Zero, false);
            IL2Object result = methodGetAsset.Invoke(mgr, new IntPtr[] { new IL2String(path).ptr, IntPtr.Zero });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr LoadImmediateAssets(string path)
        {
            IntPtr mgr = fieldResourceManagerInstance.GetValue().ptr;
            LoadImmediate(mgr, new IL2String(path).ptr, IntPtr.Zero, IntPtr.Zero, false);
            IntPtr workPathPtr = IntPtr.Zero;
            IntPtr resourcePtr = hookGetResource.Original(mgr, new IL2String(path).ptr, (IntPtr)(&workPathPtr));
            IL2Object result = methodGetAssets.Invoke(resourcePtr, new IntPtr[] { new IL2String(path).ptr, IntPtr.Zero });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static IntPtr GetAsset(string path)
        {
            IntPtr mgr = fieldResourceManagerInstance.GetValue().ptr;
            IL2Object result = methodGetAsset.Invoke(mgr, new IntPtr[] { new IL2String(path).ptr, IntPtr.Zero });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        public static void Unload(string path, bool force = false)
        {
            IntPtr mgr = fieldResourceManagerInstance.GetValue().ptr;
            methodUnload.Invoke(mgr, new IntPtr[] { new IL2String(path).ptr, new IntPtr(&force) });
        }

        public static byte[] GetBytesDecryptionData(string path)
        {
            IL2Object bytesArrayObj = null;
            IL2Object instance = fieldContentInstance.GetValue();
            if (instance != null)
            {
                hookLoadImmediate.Original(resourceMangerInstance, new IL2String(path).ptr, IntPtr.Zero, IntPtr.Zero, false);
                bytesArrayObj = methodGetBytesDecryptionData.Invoke(instance, new IntPtr[] { new IL2String(path).ptr });
                if (bytesArrayObj != null)
                {
                    return new IL2Array<byte>(bytesArrayObj.ptr).ToByteArray();
                }
            }
            return null;
        }

        static IntPtr GetResource(IntPtr thisPtr, IntPtr pathPtr, IntPtr workPathPtr)
        {
            int soloGateBgId = 0;
            if (pathPtr != IntPtr.Zero)
            {
                string inputPath = new IL2String(pathPtr).ToString();
                const string gateBgHeader = "Prefabs/Solo/BackGrounds/Front/gateBGUI";
                if (inputPath.StartsWith(gateBgHeader) && int.TryParse(inputPath.Substring(gateBgHeader.Length), out soloGateBgId) && !FileExists(inputPath))
                {
                    // NOTE: This will most likely keep the asset in the ResourceManager for the lifetime of the process (no Unload call)
                    pathPtr = new IL2String(gateBgHeader + "0001").ptr;
                    hookLoadImmediate.Original(thisPtr, pathPtr, IntPtr.Zero, IntPtr.Zero, false);
                }
            }
            IntPtr resourcePtr = hookGetResource.Original(thisPtr, pathPtr, workPathPtr);
            if (resourcePtr == IntPtr.Zero)
            {
                return resourcePtr;
            }

            string loadPath = null;// The target path after conversion (<_CARD_ILLUST_>, <_RESOURCE_TYPE_>, etc)
            IL2Object loadPathObj = methodGetLoadPath.Invoke(resourcePtr);
            if (loadPathObj != null)
            {
                loadPath = loadPathObj.GetValueObj<string>();
            }

            if (ClientSettings.AssetHelperDump && !string.IsNullOrEmpty(loadPath))
            {
                if (loadPath.StartsWith("External/CardCategory/") ||
                    loadPath.StartsWith("Card/Data/"))
                {
                    string fullPath = Path.Combine(Program.ClientDataDumpDir, loadPath + ".bytes");
                    if (!File.Exists(fullPath))
                    {
                        IL2Object bytesArrayObj = null;
                        if (!(loadPath.Contains("CARD_") || loadPath.Contains("DLG_") || loadPath.Contains("WORD_")))
                        {
                            bytesArrayObj = methodGetBytes.Invoke(resourcePtr);
                        }
                        if (bytesArrayObj != null)
                        {
                            byte[] buffer = new IL2Array<byte>(bytesArrayObj.ptr).ToByteArray();
                            if (buffer != null && buffer.Length > 0)
                            {
                                try
                                {
                                    Utils.TryCreateDirectory(Path.GetDirectoryName(fullPath));
                                    File.WriteAllBytes(fullPath, buffer);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }

            IL2Object assetsArrayObj = methodGetAssets.Invoke(resourcePtr);
            IL2Array<IntPtr> assetsArray = null;
            if (assetsArrayObj != null)
            {
                bool hasDumpedTexture = false;
                assetsArray = new IL2Array<IntPtr>(assetsArrayObj.ptr);
                for (int i = 0; i < assetsArray.Length; i++)
                {
                    IntPtr obj = assetsArray[i];
                    if (obj == IntPtr.Zero) continue;
                    IntPtr type = Import.Object.il2cpp_object_get_class(obj);
                    if (type == IntPtr.Zero) continue;
                    string typeName = Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_name(type));
                    //Console.WriteLine(typeName + " - " + loadPath);

                    if (type == gameObjectClassInfo.ptr && soloGateBgId > 0)
                    {
                        InjectSoloBackground(obj, soloGateBgId);
                    }
                    if (type == soloCardThumbSettingsClassInfo.ptr)
                    {
                        InjectSoloThumbs(obj);
                    }

                    if (ClientSettings.AssetHelperDump)
                    {
                        if (!hasDumpedTexture)
                        {
                            IntPtr texture = IntPtr.Zero;
                            if (type == spriteClassInfo.ptr)
                            {
                                IL2Object textureObj = methodGetTexture.Invoke(obj);
                                if (textureObj != null)
                                {
                                    texture = textureObj.ptr;
                                }
                            }
                            if (type == texture2DClassInfo.ptr)
                            {
                                texture = obj;
                            }
                            if (texture != IntPtr.Zero)
                            {
                                hasDumpedTexture = true;
                                // TODO: Sanitize the file path
                                string fullPath = Path.Combine(Program.ClientDataDumpDir, loadPath + ".png");
                                if (!File.Exists(fullPath))
                                {
                                    byte[] buffer = TextureToPNG(texture);
                                    if (buffer != null)
                                    {
                                        try
                                        {
                                            Utils.TryCreateDirectory(Path.GetDirectoryName(fullPath));
                                            File.WriteAllBytes(fullPath, buffer);
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resourcePtr;
        }

        public static bool FileExists(string path)
        {
            IL2Object obj = methodExists.Invoke(new IntPtr[] { new IL2String(path).ptr });
            return obj != null ? obj.GetValueRef<bool>() : false;
        }

        /// <summary>
        /// Replaces templated file path segments with platform specific file path segments (#, _RESOURCE_TYPE_, _CARD_ILLUST_, etc)
        /// </summary>
        public static string ConvertAssetPath(string path)
        {
            // TODO: Replace ConvertAutoPath with our own implementation as we need to fixup things anyway?
            path = methodConvertAutoPath.Invoke(new IntPtr[] { new IL2String(path).ptr }).GetValueObj<string>();
            // Images/CardPack/<_RESOURCE_TYPE_>/<_CARD_ILLUST_>/CardPackTex01_0000 <--- input string
            // Images/CardPack/<_RESOURCE_TYPE_>/tcg/CardPackTex01_0000 <--- auto converted (file doesn't exist on disk)
            // Images/CardPack/SD/tcg/CardPackTex01_0000 <--- manually entered is OK (file exists on disk), but if you auto convert this, it will become...
            // Images/CardPack/HighEnd/tcg/CardPackTex01_0000 <--- this doesn't exist (SD gets converted to HighEnd)
            // Images/CardPack/HighEnd_HD/tcg/CardPackTex01_0000 <--- this exists
            // /<_PLATFORM_>/ gets converted to /PC/
            // /#/ gets converted to /en-US/
            path = FixupAssetPathResourceType(path, false);
            return path;
        }

        static string FixupAssetPathResourceType(string path, bool fixExistingConversion)
        {
            const string resourceTypeHeader = "<_RESOURCE_TYPE_>";
            ResourceType resourceType = GetResourceType();
            if (fixExistingConversion)
            {
                // NOTE: This resource path fixup code is a hack and might not apply to all files. Update the code if it breaks stuff.
                // This should be used in cases where a file doesn't exist but you're working with a string that the game converted as
                // the game will fall back to a "SD" file path on non-existing assets.
                switch (resourceType)
                {
                    case ResourceType.HighEnd_HD:
                        path = path.Replace("/SD/", "/HighEnd_HD/");
                        break;
                    case ResourceType.HighEnd:
                    case ResourceType.LowEnd:
                        path = path.Replace("/HighEnd_HD/", "/SD/");
                        break;
                }
            }
            if (path.Contains(resourceTypeHeader))
            {
                switch (resourceType)
                {
                    default:
                    case ResourceType.LowEnd:
                    case ResourceType.HighEnd:
                        path = path.Replace(resourceTypeHeader, "SD");
                        break;
                    case ResourceType.HighEnd_HD:
                        path = path.Replace(resourceTypeHeader, "HighEnd_HD");
                        break;
                }
            }

            path = path.Replace("/HighEnd/", "/HighEnd_HD/");// "/HighEnd/" doesn't exist (but the game will often put it in)
            return path;
        }

        public static ResourceType GetResourceType()
        {
            return (ResourceType)methodGetResourceType.Invoke().GetValueRef<int>();
        }

        /// <summary>
        /// Gets a file path on disk (CRCs the file path)
        /// Assumes the path has already been converted (#, _RESOURCE_TYPE_, _CARD_ILLUST_, etc)
        /// </summary>
        public static string GetAssetBundleOnDiskConverted(string path)
        {
            uint crc = YgomSystem.Hash.CRC32.GetStringCRC32(path);
            return Path.Combine(((int)((crc & 4278190080u) >> 24)).ToString("x2"), crc.ToString("x8"));
        }

        /// <summary>
        /// Does the path conversion and then CRCs that converted path
        /// </summary>
        public static string GetAssetBundleOnDisk(string path)
        {
            uint crc = methodGetCrc.Invoke(new IntPtr[] { new IL2String(path).ptr }).GetValueRef<uint>();
            return Path.Combine(((int)((crc & 4278190080u) >> 24)).ToString("x2"), crc.ToString("x8"));
        }
    }
}
