using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using System.Runtime.InteropServices;
using System.IO;

namespace YgoMasterClient
{
    unsafe static partial class AssetHelper
    {
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
        static IL2Field fieldResourceDictionary;
        static IL2Field fieldResourceManagerInstance;

        delegate IntPtr Del_GetResource(IntPtr thisPtr, IntPtr pathPtr, IntPtr workPathPtr);
        static Hook<Del_GetResource> hookGetResource;
        delegate uint Del_Load(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInterface, IntPtr completeHandler, bool disableErrorNotify);
        static Hook<Del_Load> hookLoad;
        delegate uint Del_LoadImmediate(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInterface, IntPtr completeHandler, bool disableErrorNotify);
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
        static IL2Class texture2DClassInfo;// Texture2D
        static IL2Method methodTexture2DCtor;
        static IL2Method methodGetIsReadable;
        static IL2Method methodGetFormat;
        static IL2Method methodReadPixels;
        static IL2Method methodApply;
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
        static IL2Class imageClassInfo;// Image
        static IL2Method methodGetSprite;
        static IL2Method methodSetSprite;
        static IL2Class rectClassInfo;// Rect
        static IL2Class vector2ClassInfo;// Vector2
        static IL2Class objectClassInfo;// Object (UnityEngine)
        static IL2Method methodGetName;
        static IL2Method methodSetName;

        // mscorlib
        static IL2Method methodReadAllBytes;// File

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

        struct Rect
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
            methodGetIsReadable = texture2DClassInfo.GetProperty("isReadable").GetGetMethod();
            methodGetFormat = texture2DClassInfo.GetProperty("format").GetGetMethod();
            methodReadPixels = texture2DClassInfo.GetMethod("ReadPixels", x => x.GetParameters().Length == 3);
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
            imageClassInfo = uiAssembly.GetClass("Image");// Image
            methodGetSprite = imageClassInfo.GetProperty("sprite").GetGetMethod();
            methodSetSprite = imageClassInfo.GetProperty("sprite").GetSetMethod();
            rectClassInfo = coreModuleAssembly.GetClass("Rect");// Rect
            vector2ClassInfo = coreModuleAssembly.GetClass("Vector2");// Vector2
            objectClassInfo = coreModuleAssembly.GetClass("Object");
            methodGetName = objectClassInfo.GetProperty("name").GetGetMethod();
            methodSetName = objectClassInfo.GetProperty("name").GetSetMethod();

            IL2Assembly mscorlibAssembly = Assembler.GetAssembly("mscorlib");
            IL2Class fileClassInfo = mscorlibAssembly.GetClass("File");
            methodReadAllBytes = fileClassInfo.GetMethod("ReadAllBytes");

            InitSolo();
        }

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

        static IntPtr TextureFromPNG(string filePath, string assetName)
        {
            IL2Object bytes = methodReadAllBytes.Invoke(new IntPtr[] { new IL2String(filePath).ptr });
            if (bytes != null)
            {
                IntPtr newTexture = Import.Object.il2cpp_object_new(texture2DClassInfo.ptr);
                if (newTexture != IntPtr.Zero)
                {
                    int textureWidth = 2, textureHeight = 2;
                    methodTexture2DCtor.Invoke(newTexture, new IntPtr[] { new IntPtr(&textureWidth), new IntPtr(&textureHeight) });
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

        static IntPtr SpriteFromTexture(IntPtr texture, string assetName)
        {
            if (texture == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            int width = methodGetWidth.Invoke(texture).GetValueRef<int>();
            int height = methodGetHeight.Invoke(texture).GetValueRef<int>();
            Rect rect = new Rect(0, 0, width, height);
            float pixelsPerUnit = GetResourceType() == ResourceType.HighEnd_HD ? 100 : 50;
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

        static IntPtr SpriteFromPNG(string filePath, string assetName)
        {
            IntPtr newTextureAsset = TextureFromPNG(filePath, assetName);
            if (newTextureAsset != IntPtr.Zero)
            {
                return SpriteFromTexture(newTextureAsset, assetName);
            }
            return IntPtr.Zero;
        }

        static bool TryLoadCustomFile(IntPtr thisPtr, IntPtr pathPtr, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify, out uint result)
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
            string customTexturePath = Path.Combine(Program.ClientDataDir, loadPath + ".png");
            if (File.Exists(customTexturePath))
            {
                uint crc = methodGetCrc.Invoke(new IntPtr[] { pathPtr }).GetValueRef<uint>();
                if (resourceDictionary.ContainsKey((int)crc))
                {
                    IntPtr resourcePtr = resourceDictionary[(int)crc];
                    int refCount = methodGetRefCount.Invoke(resourcePtr).GetValueRef<int>();
                    refCount++;
                    methodSetRefCount.Invoke(resourcePtr, new IntPtr[] { new IntPtr(&refCount) });
                    result = crc;
                    if (completeHandler != IntPtr.Zero)
                    {
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

                    ResourceType resourceType = GetResourceType();
                    IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(2, objectClassInfo);
                    if (assetsArray.ptr == IntPtr.Zero)
                    {
                        Console.WriteLine("Array alloc failed");
                        return false;
                    }

                    string assetName = Path.GetFileNameWithoutExtension(customTexturePath);
                    IntPtr newTextureAsset = TextureFromPNG(customTexturePath, assetName);
                    assetsArray[0] = newTextureAsset;

                    IntPtr newSpriteAsset = SpriteFromTexture(newTextureAsset, assetName);
                    if (newSpriteAsset != IntPtr.Zero)
                    {
                        assetsArray[1] = newSpriteAsset;
                    }

                    // TODO: Remove what is not required (and/or directly set them via k__BackingField entries)
                    bool done = true;
                    int resType = (int)ResourceManager_Resource_Type.LocalFile;
                    int queueId = (int)ResourceManager_ReqType.Default;
                    int refCount = 1;
                    IntPtr[] arg = new IntPtr[1];
                    arg[0] = new IntPtr(&resType); methodSetRefCount.Invoke(resourcePtr, arg);
                    arg[0] = new IntPtr(&refCount); methodSetResType.Invoke(resourcePtr, arg);
                    arg[0] = systemTypeInstance; methodSetSystemType.Invoke(resourcePtr, arg);
                    arg[0] = new IntPtr(&queueId); methodSetQueueId.Invoke(resourcePtr, arg);
                    arg[0] = pathPtr; methodSetPath.Invoke(resourcePtr, arg);
                    arg[0] = new IL2String(loadPath).ptr; methodSetLoadPath.Invoke(resourcePtr, arg);
                    arg[0] = new IntPtr(&done); methodSetDone.Invoke(resourcePtr, arg);
                    arg[0] = assetsArray.ptr; methodSetAssets.Invoke(resourcePtr, arg);

                    resourceDictionary.Add((int)crc, resourcePtr);
                    result = crc;
                    if (completeHandler != IntPtr.Zero)
                    {
                        methodInvoke.Invoke(completeHandler, new IntPtr[] { pathPtr });
                    }
                    return true;
                }
            }
            return false;
        }

        static uint Load(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify)
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
            if (TryLoadCustomFile(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify, out result))
            {
                return result;
            }
            return hookLoad.Original(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify);
        }

        static uint LoadImmediate(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify)
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
            if (TryLoadCustomFile(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify, out result))
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
                                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                }
                                catch { }
                                try
                                {
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
                                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                        }
                                        catch { }
                                        try
                                        {
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
