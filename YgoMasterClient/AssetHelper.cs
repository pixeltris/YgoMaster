using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using System.Runtime.InteropServices;
using System.IO;

namespace YgoMasterClient
{
    unsafe static class AssetHelper
    {
        public static bool ShouldDumpData = false;

        // YgomSystem.Utility.DeviceInfo
        static IL2Method methodGetResourceType;

        // YgomSystem.ResourceSystem.ResourceUtility
        static IL2Class resourceUtilityClassInfo;
        static IL2Method methodConvertAutoPath;

        // YgomSystem.ResourceManager
        static IL2Class resourceManagerClassInfo;
        static IL2Method methodExists;
        static IL2Method methodGetResource;

        delegate IntPtr Del_GetResource(IntPtr thisPtr, IntPtr pathPtr, IntPtr workPathPtr);
        static Hook<Del_GetResource> hookGetResource;
        delegate uint Del_Load(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInterface, IntPtr completeHandler, bool disableErrorNotify);
        static Hook<Del_Load> hookLoad;
        delegate uint Del_LoadImmediate(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInterface, IntPtr completeHandler, bool disableErrorNotify);
        static Hook<Del_LoadImmediate> hookLoadImmediate;

        // YgomSystem.ResourceSystem.Resource
        static IL2Class resourceClassInfo;
        static IL2Method methodGetAssets;
        static IL2Method methodSetAssets;
        static IL2Method methodGetBytes;
        static IL2Method methodSetBytes;
        static IL2Method methodSetError;
        static IL2Method methodGetPath;
        static IL2Method methodGetLoadPath;

        // UnityEngine.ImageConversionModule.ImageConversion
        static IL2Class imageConversionClassInfo;
        static IL2Method methodLoadImage;
        static IL2Method methodEncodeToPNG;

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
        static IL2Class rectClassInfo;// Rect
        static IL2Class vector2ClassInfo;// Vector2
        static IL2Class objectClassInfo;// Object (UnityEngine)
        static IL2Method methodGetName;
        static IL2Method methodSetName;

        // mscorlib
        static IL2Method methodReadAllBytes;// File

        [StructLayout(LayoutKind.Sequential)]
        struct Vector2
        {
            public float x;
            public float y;

            public Vector2(float x, float y)
            {
                this.x = x;
                this.y = y;
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

        static AssetHelper()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class deviceInfoClassInfo = assembly.GetClass("DeviceInfo", "YgomSystem.Utility");
            methodGetResourceType = deviceInfoClassInfo.GetMethod("GetResourceType");

            resourceUtilityClassInfo = assembly.GetClass("ResourceUtility", "YgomSystem.ResourceSystem");
            methodConvertAutoPath = resourceUtilityClassInfo.GetMethod("ConvertAutoPath");

            resourceManagerClassInfo = assembly.GetClass("ResourceManager", "YgomSystem");
            methodExists = resourceManagerClassInfo.GetMethod("Exists");
            methodGetResource = resourceManagerClassInfo.GetMethod("getResource", x => x.GetParameters().Length == 2 && x.GetParameters()[0].Name == "path");
            hookGetResource = new Hook<Del_GetResource>(GetResource, methodGetResource);
            hookLoad = new Hook<Del_Load>(Load, resourceManagerClassInfo.GetMethod("load"));
            hookLoadImmediate = new Hook<Del_LoadImmediate>(LoadImmediate, resourceManagerClassInfo.GetMethod("loadImmediate"));

            resourceClassInfo = assembly.GetClass("Resource", "YgomSystem.ResourceSystem");
            methodGetAssets = resourceClassInfo.GetProperty("Assets").GetGetMethod();
            methodSetAssets = resourceClassInfo.GetProperty("Assets").GetSetMethod();
            methodGetBytes = resourceClassInfo.GetProperty("Bytes").GetGetMethod();
            methodSetBytes = resourceClassInfo.GetProperty("Bytes").GetSetMethod();
            methodSetError = resourceClassInfo.GetProperty("Error").GetSetMethod();
            methodGetPath = resourceClassInfo.GetProperty("Path").GetGetMethod();
            methodGetLoadPath = resourceClassInfo.GetProperty("LoadPath").GetGetMethod();

            IL2Assembly imageConversionAssembly = Assembler.GetAssembly("UnityEngine.ImageConversionModule");
            imageConversionClassInfo = imageConversionAssembly.GetClass("ImageConversion");
            methodLoadImage = imageConversionClassInfo.GetMethod("LoadImage", x => x.GetParameters().Length == 2);
            methodEncodeToPNG = imageConversionClassInfo.GetMethod("EncodeToPNG");

            IL2Assembly coreModuleAssembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            texture2DClassInfo = coreModuleAssembly.GetClass("Texture2D");// Texture2D
            methodTexture2DCtor = texture2DClassInfo.GetMethod(".ctor", x => x.GetParameters().Length == 2);
            methodGetIsReadable = texture2DClassInfo.GetProperty("isReadable").GetGetMethod();
            methodGetFormat = texture2DClassInfo.GetProperty("format").GetGetMethod();
            methodReadPixels = texture2DClassInfo.GetMethod("ReadPixels", x => x.GetParameters().Length == 3);
            methodApply = texture2DClassInfo.GetMethod("Apply", x => x.GetParameters().Length == 2);
            IL2Class textureClassInfo = coreModuleAssembly.GetClass("Texture");// Texture (putting class here as this has bitten me using it by mistake)
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
            rectClassInfo = coreModuleAssembly.GetClass("Rect");// Rect
            vector2ClassInfo = coreModuleAssembly.GetClass("Vector2");// Vector2
            objectClassInfo = coreModuleAssembly.GetClass("Object");
            methodGetName = objectClassInfo.GetProperty("name").GetGetMethod();
            methodSetName = objectClassInfo.GetProperty("name").GetSetMethod();

            IL2Assembly mscorlibAssembly = Assembler.GetAssembly("mscorlib");
            IL2Class fileClassInfo = mscorlibAssembly.GetClass("File");
            methodReadAllBytes = fileClassInfo.GetMethod("ReadAllBytes");
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

                IL2Object textureData = methodEncodeToPNG.Invoke(new IntPtr[] { texture });
                if (textureData != null)
                {
                    byte[] buffer = new IL2Array<byte>(textureData.ptr).ToByteArray();
                    if (buffer.Length > 0)
                    {
                        return buffer;
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

        static IntPtr TextureToSprite(IntPtr texture)
        {
            return IntPtr.Zero;
        }

        static string GetObjectName(IntPtr obj)
        {
            string name = null;
            IL2Object nameObj = methodGetName.Invoke(obj);
            if (nameObj != null)
            {
                name = nameObj.GetValueObj<string>();
            }
            //Console.WriteLine("Name '" + name + "'");
            return name;
        }

        static bool IsCustomFile(IntPtr path)
        {
            if (path != IntPtr.Zero)
            {
                // NOTE: Might be potential issue with resolving the correct resource type (SD/HD)?
                string loadPath = ConvertAssetPath(new IL2String(path).ToString());
                string customTexturePath = Path.Combine(Program.ClientDataDir, loadPath + ".png");
                if (File.Exists(customTexturePath))
                {
                    //Console.WriteLine("Custom file '" + loadPath + "'");
                    return true;
                }
            }
            return false;
        }

        static uint Load(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify)
        {
            if (IsCustomFile(path))
            {
                disableErrorNotify = true;
            }
            return hookLoad.Original(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify);
        }

        static uint LoadImmediate(IntPtr thisPtr, IntPtr path, IntPtr systemTypeInstance, IntPtr completeHandler, bool disableErrorNotify)
        {
            if (IsCustomFile(path))
            {
                disableErrorNotify = true;
            }
            return hookLoadImmediate.Original(thisPtr, path, systemTypeInstance, completeHandler, disableErrorNotify);
        }

        static IntPtr GetResource(IntPtr thisPtr, IntPtr pathPtr, IntPtr workPathPtr)
        {
            // TODO: Handle unloading and caching?

            IntPtr resourcePtr = hookGetResource.Original(thisPtr, pathPtr, workPathPtr);
            if (resourcePtr == IntPtr.Zero)
            {
                Console.WriteLine("TODO: Handle null resource (" + new IL2String(pathPtr).ToString() + ")");
                return resourcePtr;
            }

            /*string path = null;// The path prior to any conversion (<_CARD_ILLUST_>, <_RESOURCE_TYPE_>, etc)
            IL2Object pathObj = methodGetPath.Invoke(resourcePtr);
            if (pathObj != null)
            {
                path = pathObj.GetValueObj<string>();
            }*/

            string loadPath = null;// The target path after conversion
            IL2Object loadPathObj = methodGetLoadPath.Invoke(resourcePtr);
            if (loadPathObj != null)
            {
                loadPath = loadPathObj.GetValueObj<string>();
            }

            if (string.IsNullOrEmpty(loadPath))
            {
                return resourcePtr;
            }

            IL2Object assetsArrayObj = methodGetAssets.Invoke(resourcePtr);
            IL2Array<IntPtr> assetsArray = null;
            if (assetsArrayObj != null)
            {
                bool hasDumpedTexture = false;
                int spriteAssetIndex = -1;
                IntPtr newTextureAsset = IntPtr.Zero;
                assetsArray = new IL2Array<IntPtr>(assetsArrayObj.ptr);
                for (int i = 0; i < assetsArray.Length; i++)
                {
                    IntPtr obj = assetsArray[i];
                    if (obj == IntPtr.Zero) continue;
                    IntPtr type = Import.Object.il2cpp_object_get_class(obj);
                    if (type == IntPtr.Zero) continue;
                    string typeName = Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_name(type));
                    Console.WriteLine(typeName + " - " + loadPath);

                    if (type == spriteClassInfo.ptr)
                    {
                        spriteAssetIndex = i;
                    }
                    if (type == texture2DClassInfo.ptr)
                    {
                        if (ShouldDumpData && !hasDumpedTexture)
                        {
                            hasDumpedTexture = true;
                            // TODO: Sanitize the path (remove any bad chars)
                            string fullPath = Path.Combine(Program.ClientDataDumpDir, loadPath + ".png");
                            if (!File.Exists(fullPath))
                            {
                                byte[] buffer = TextureToPNG(obj);
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
                        string customTexturePath = Path.Combine(Program.ClientDataDir, loadPath + ".png");
                        if (newTextureAsset == IntPtr.Zero && File.Exists(customTexturePath))
                        {
                            newTextureAsset = TextureFromPNG(customTexturePath, GetObjectName(obj));
                            if (newTextureAsset != IntPtr.Zero)
                            {
                                assetsArray[i] = newTextureAsset;
                                //Console.WriteLine("Texture swapped");
                            }
                        }
                    }
                }
                if (newTextureAsset != IntPtr.Zero && spriteAssetIndex >= 0)
                {
                    IntPtr oldSpriteAsset = assetsArray[spriteAssetIndex];
                    Rect rect = methodGetRect.Invoke(oldSpriteAsset).GetValueRef<Rect>();
                    float pixelsPerUnit = methodGetPixelsPerUnit.Invoke(oldSpriteAsset).GetValueRef<float>();
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    IL2Object newSpriteAsset = methodSpriteCreate.Invoke(
                        new IntPtr[] { newTextureAsset, new IntPtr(&rect), new IntPtr(&pivot), new IntPtr(&pixelsPerUnit) });
                    if (newSpriteAsset != null)
                    {
                        assetsArray[spriteAssetIndex] = newSpriteAsset.ptr;
                        string name = GetObjectName(oldSpriteAsset);
                        if (!string.IsNullOrEmpty(name))
                        {
                            methodSetName.Invoke(newSpriteAsset.ptr, new IntPtr[] { new IL2String(name).ptr });
                        }
                        //Console.WriteLine("Sprite swapped");
                    }
                }
            }

            if (assetsArray == null || assetsArray.Length == 0)
            {
                // NOTE: This resource path fixup code is a hack and might not apply to all images. Update the code if it breaks stuff
                //       - It's done because non-existing files seem to fall back to SD
                ResourceType resourceType = GetResourceType();
                switch (resourceType)
                {
                    case ResourceType.HighEnd_HD:
                        loadPath = loadPath.Replace("SD", "HighEnd_HD");
                        break;
                    case ResourceType.HighEnd:
                    case ResourceType.LowEnd:
                        loadPath = loadPath.Replace("HighEnd_HD", "SD");
                        break;
                }
                string customTexturePath = Path.Combine(Program.ClientDataDir, loadPath + ".png");
                if (File.Exists(customTexturePath))
                {
                    string assetName = Path.GetFileNameWithoutExtension(customTexturePath);
                    IntPtr newTextureAsset = TextureFromPNG(customTexturePath, assetName);
                    assetsArray = new IL2Array<IntPtr>(2, objectClassInfo);
                    assetsArray[0] = newTextureAsset;
                    bool error = false;
                    methodSetError.Invoke(resourcePtr, new IntPtr[] { new IntPtr(&error) });

                    int width = methodGetWidth.Invoke(newTextureAsset).GetValueRef<int>();
                    int height = methodGetHeight.Invoke(newTextureAsset).GetValueRef<int>();
                    Rect rect = new Rect(0, 0, width, height);
                    float pixelsPerUnit = resourceType == ResourceType.HighEnd_HD ? 100 : 50;
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    IL2Object newSpriteAsset = methodSpriteCreate.Invoke(
                        new IntPtr[] { newTextureAsset, new IntPtr(&rect), new IntPtr(&pivot), new IntPtr(&pixelsPerUnit) });
                    if (newSpriteAsset != null)
                    {
                        assetsArray[1] = newSpriteAsset.ptr;
                        methodSetName.Invoke(newSpriteAsset.ptr, new IntPtr[] { new IL2String(assetName).ptr });
                    }

                    methodSetAssets.Invoke(resourcePtr, new IntPtr[] { assetsArray.ptr });
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
            const string resourceTypeHeader = "<_RESOURCE_TYPE_>";
            if (path.Contains(resourceTypeHeader))
            {
                switch (GetResourceType())
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
            path = path.Replace("/HighEnd/", "/HighEnd_HD/");
            return path;
        }

        public static ResourceType GetResourceType()
        {
            return (ResourceType)methodGetResourceType.Invoke().GetValueRef<int>();
        }

        /// <summary>
        /// Converts a virtual file path to a path on disk (which is the file path CRCed)
        /// Assumes the path has already been converted (#, _RESOURCE_TYPE_, _CARD_ILLUST_, etc)
        /// </summary>
        public static string GetAssetBundleOnDisk(string path)
        {
            uint crc = YgomSystem.Hash.CRC32.GetStringCRC32(path);
            return Path.Combine(((int)((crc & 4278190080u) >> 24)).ToString("x2"), crc.ToString("x8"));
        }
    }
}
