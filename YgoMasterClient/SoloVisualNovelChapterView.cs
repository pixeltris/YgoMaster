using IL2CPP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using YgoMaster;

namespace YgoMasterClient
{
    static unsafe class SoloVisualNovelChapterView
    {
        static IL2Field SoloSelectChapterViewController_chapterMap;
        static IL2Field ChapterMap_chapterDataDic;
        static IL2Field ChapterMap_gateID;
        static IL2Class Chapter_Class;
        static IL2Field Chapter_go;
        static IntPtr Image_Type;
        static IL2Method Image_SetSprite;
        static IL2Property Sprite_rect;
        static IL2Property Texture_height;
        static IntPtr CanvasGroup_Type;
        static IntPtr RectTransform_Type;
        static IL2Property RectTransform_anchoredPosition3D;
        static IL2Property RectTransform_anchorMin;
        static IL2Property RectTransform_anchorMax;
        static IL2Property RectTransform_offsetMin;
        static IL2Property RectTransform_offsetMax;
        static IL2Property RectTransform_pivot;
        static IL2Property RectTransform_sizeDelta;
        static IntPtr Mask_Type;
        static IL2Property Mask_showMaskGraphic;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;

        static bool loadedCharacterImages;
        static Dictionary<string, IntPtr> characterSprites = new Dictionary<string, IntPtr>();
        static IntPtr maskSprite;

        static int charsOffsetY = 13;
        static bool disableWallpaper;

        static SoloVisualNovelChapterView()
        {
            IL2Assembly uiAssembly = Assembler.GetAssembly("UnityEngine.UI");
            IL2Class Image_Class = uiAssembly.GetClass("Image", "UnityEngine.UI");
            Image_Type = Image_Class.IL2Typeof();
            Image_SetSprite = Image_Class.GetProperty("sprite").GetSetMethod();
            IL2Class Mask_Class = uiAssembly.GetClass("Mask", "UnityEngine.UI");
            Mask_Type = Mask_Class.IL2Typeof();
            Mask_showMaskGraphic = Mask_Class.GetProperty("showMaskGraphic");

            IL2Assembly coreModuleAssembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class Texture_Class = coreModuleAssembly.GetClass("Texture");
            Texture_height = Texture_Class.GetProperty("height");

            IL2Class RectTransform_Class = coreModuleAssembly.GetClass("RectTransform", "UnityEngine");
            RectTransform_Type = RectTransform_Class.IL2Typeof();
            RectTransform_anchoredPosition3D = RectTransform_Class.GetProperty("anchoredPosition3D");
            RectTransform_anchorMin = RectTransform_Class.GetProperty("anchorMin");
            RectTransform_anchorMax = RectTransform_Class.GetProperty("anchorMax");
            RectTransform_offsetMin = RectTransform_Class.GetProperty("offsetMin");
            RectTransform_offsetMax = RectTransform_Class.GetProperty("offsetMax");
            RectTransform_pivot = RectTransform_Class.GetProperty("pivot");
            RectTransform_sizeDelta = RectTransform_Class.GetProperty("sizeDelta");

            IL2Class Sprite_Class = coreModuleAssembly.GetClass("Sprite", "UnityEngine");
            Sprite_rect = Sprite_Class.GetProperty("rect");

            IL2Assembly uiModuleAssembly = Assembler.GetAssembly("UnityEngine.UIModule");
            IL2Class CanvasGroup_Class = uiModuleAssembly.GetClass("CanvasGroup", "UnityEngine");
            CanvasGroup_Type = CanvasGroup_Class.IL2Typeof();

            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class SoloSelectChapterViewController_Class = assembly.GetClass("SoloSelectChapterViewController", "YgomGame.Solo");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, SoloSelectChapterViewController_Class.GetMethod("OnCreatedView"));
            SoloSelectChapterViewController_chapterMap = SoloSelectChapterViewController_Class.GetField("chapterMap");

            IL2Class ChapterMap_Class = SoloSelectChapterViewController_Class.GetNestedType("ChapterMap");
            ChapterMap_chapterDataDic = ChapterMap_Class.GetField("chapterDataDic");
            ChapterMap_gateID = ChapterMap_Class.GetField("gateID");

            Chapter_Class = SoloSelectChapterViewController_Class.GetNestedType("Chapter");
            Chapter_go = Chapter_Class.GetField("go");
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            hookOnCreatedView.Original(thisPtr);

            if (!loadedCharacterImages)
            {
                loadedCharacterImages = true;
                string dir = Path.Combine(Program.ClientDataDir, SoloVisualNovel.baseDir);
                string imageFile = Path.Combine(dir, "chars.png");
                if (!File.Exists(imageFile))
                {
                    return;
                }
                string maskImageFile = Path.Combine(dir, "chars_mask.png");
                if (!File.Exists(maskImageFile))
                {
                    return;
                }
                Dfymoo dfymoo = new Dfymoo();
                if (!dfymoo.Load(Path.Combine(dir, "chars.dfymoo")))
                {
                    return;
                }
                // NOTE: We create a global object with all texture/sprites to prevent unity/masterduel from destroying them
                IntPtr charsDontDestroy = GameObject.New();
                UnityObject.SetName(charsDontDestroy, "CharsDontDestroy");
                UnityObject.DontDestroyOnLoad(charsDontDestroy);
                // TODO: Load this async?
                IntPtr texture = AssetHelper.TextureFromPNG(imageFile, "chars", false);
                int textureHeight = Texture_height.GetGetMethod().Invoke(texture).GetValueRef<int>();
                Import.Handler.il2cpp_gchandle_new(texture, true);
                foreach (Dfymoo.Item item in dfymoo.Items.Values)
                {
                    IntPtr sprite = AssetHelper.SpriteFromTexture(texture, "chars_" + item.Name, new AssetHelper.Rect(item.X, textureHeight - item.Y - item.Height, item.Width, item.Height), 50);
                    if (sprite != IntPtr.Zero)
                    {
                        characterSprites[item.Name.ToLower()] = sprite;
                        Import.Handler.il2cpp_gchandle_new(sprite, true);

                        IntPtr spriteKeepAlive = GameObject.New();
                        UnityObject.SetName(spriteKeepAlive, item.Name);
                        IntPtr imgComp = GameObject.AddComponent(spriteKeepAlive, Image_Type);
                        Image_SetSprite.Invoke(imgComp, new IntPtr[] { sprite });
                        Transform.SetParent(GameObject.GetTransform(spriteKeepAlive), GameObject.GetTransform(charsDontDestroy));
                    }
                }
                maskSprite = AssetHelper.SpriteFromPNG(maskImageFile, "chars_mask");
                Import.Handler.il2cpp_gchandle_new(maskSprite, true);
                UnityObject.DontDestroyOnLoad(maskSprite);

                IntPtr maskSpriteKeepAlive = GameObject.New();
                UnityObject.SetName(maskSpriteKeepAlive, "chars_mask");
                IntPtr maskImgComp = GameObject.AddComponent(maskSpriteKeepAlive, Image_Type);
                Image_SetSprite.Invoke(maskImgComp, new IntPtr[] { maskSprite });
                Transform.SetParent(GameObject.GetTransform(maskSpriteKeepAlive), GameObject.GetTransform(charsDontDestroy));

                string uiSettingsFile = Path.Combine(Program.ClientDataDir, SoloVisualNovel.baseDir, "UISettings.json");
                if (File.Exists(uiSettingsFile))
                {
                    Dictionary<string, object> uiSettings = MiniJSON.Json.DeserializeStripped(File.ReadAllText(uiSettingsFile)) as Dictionary<string, object>;
                    charsOffsetY = Utils.GetValue<int>(uiSettings, "ChapterSelectCharsOffsetY");
                    disableWallpaper = Utils.GetValue<bool>(uiSettings, "ChapterSelectDisableWallpaper");
                }
            }

            if (characterSprites.Count == 0)
            {
                return;
            }

            int numChaptersWithCustomImages = 0;

            IntPtr chapterMap = SoloSelectChapterViewController_chapterMap.GetValue(thisPtr).ptr;
            int gateId = ChapterMap_gateID.GetValue(chapterMap).GetValueRef<int>();

            Dictionary<string, object> chaptersData = YgomSystem.Utility.ClientWork.GetDict("$.Master.Solo.chapter." + gateId);
            IL2DictionaryExplicit chapterDataDict = new IL2DictionaryExplicit(ChapterMap_chapterDataDic.GetValue(chapterMap).ptr, IL2SystemClass.Int32, Chapter_Class);
            foreach (KeyValuePair<string, object> chapter in chaptersData)
            {
                Dictionary<string, object> chapterData = chapter.Value as Dictionary<string, object>;
                int chapterId;
                string p1Img, p2Img;
                IntPtr p1ImgSprite, p2ImgSprite;
                if (int.TryParse(chapter.Key, out chapterId) && chapterDataDict.ContainsKey(chapterId) && chapterData != null &&
                    Utils.TryGetValue(chapterData, "p1_img", out p1Img) && Utils.TryGetValue(chapterData, "p2_img", out p2Img) &&
                    characterSprites.TryGetValue(p1Img.ToLower(), out p1ImgSprite) && characterSprites.TryGetValue(p2Img.ToLower(), out p2ImgSprite))
                {
                    IntPtr chapterObj = chapterDataDict[chapterId];
                    IL2Object goObj = Chapter_go.GetValue(chapterObj);
                    if (goObj != null)
                    {
                        IntPtr go = goObj.ptr;
                        IntPtr btnObj = GameObject.FindGameObjectByName(go, "Button", false, false);
                        if (btnObj == IntPtr.Zero)
                        {
                            return;
                        }

                        IntPtr iconObj = GameObject.FindGameObjectByName(btnObj, "Icon", false, false);
                        if (iconObj != IntPtr.Zero)
                        {
                            GameObject.SetActive(iconObj, false);
                        }

                        IntPtr charsObj = GameObject.New();
                        UnityObject.SetName(charsObj, "Chars");
                        GameObject.AddComponent(charsObj, CanvasGroup_Type);
                        GameObject.AddComponent(charsObj, RectTransform_Type);
                        IntPtr charsTransform = GameObject.GetTransform(charsObj);
                        Transform.SetParent(charsTransform, GameObject.GetTransform(btnObj));
                        Transform.SetSiblingIndex(charsTransform, 1);
                        IntPtr maskComp = GameObject.AddComponent(charsObj, Mask_Type);
                        csbool showMask = false;
                        Mask_showMaskGraphic.GetSetMethod().Invoke(maskComp, new IntPtr[] { new IntPtr(&showMask) });
                        IntPtr imgComp = GameObject.AddComponent(charsObj, Image_Type);
                        Image_SetSprite.Invoke(imgComp, new IntPtr[] { maskSprite });
                        SetFull(charsTransform);
                        SetSizeDelta(charsTransform, new AssetHelper.Vector2(-8, -8));

                        CreateCharObj(charsObj, p2ImgSprite, "P2", false);
                        CreateCharObj(charsObj, p1ImgSprite, "P1", true);

                        Vector3 anchorPos = new Vector3(0, 0, 0);
                        RectTransform_anchoredPosition3D.GetSetMethod().Invoke(charsTransform, new IntPtr[] { new IntPtr(&anchorPos) });
                        Transform.SetLocalScale(charsTransform, new Vector3(1, 1, 1));

                        numChaptersWithCustomImages++;
                    }
                }
            }

            if (disableWallpaper && numChaptersWithCustomImages > 0)
            {
                IntPtr obj = Component.GetGameObject(thisPtr);
                IntPtr wallpaper = GameObject.FindGameObjectByPath(obj, "SoloSelectChapterUI(Clone).Wallpaper");
                if (wallpaper != IntPtr.Zero)
                {
                    GameObject.SetActive(wallpaper, false);
                }
            }
        }

        static void CreateCharObj(IntPtr parentObj, IntPtr sprite, string name, bool left)
        {
            IntPtr imgObj = GameObject.New();
            GameObject.AddComponent(imgObj, CanvasGroup_Type);
            GameObject.AddComponent(imgObj, RectTransform_Type);
            IntPtr imgTransform = GameObject.GetTransform(imgObj);
            Transform.SetParent(imgTransform, GameObject.GetTransform(parentObj));
            UnityObject.SetName(imgObj, name);
            IntPtr imgComp = GameObject.AddComponent(imgObj, Image_Type);
            Image_SetSprite.Invoke(imgComp, new IntPtr[] { sprite });

            AssetHelper.Rect spriteRect = Sprite_rect.GetGetMethod().Invoke(sprite).GetValueRef<AssetHelper.Rect>();
            int w = (int)spriteRect.m_Width;
            int h = (int)spriteRect.m_Height;
            int w2 = w / 3;
            int offsetX = left ? -w2 : w2;
            int offsetY = charsOffsetY;//13;//0;//4;

            SetPivot(imgTransform, new AssetHelper.Vector2(0.5f, 0.0f));
            SetSize(imgTransform,
                new AssetHelper.Vector2(0.5f, 0.0f), new AssetHelper.Vector2(0.5f, 0.0f),
                new AssetHelper.Vector2(0, 0), new AssetHelper.Vector2(0, 0));
            SetSizeDelta(imgTransform, new AssetHelper.Vector2(w, h));

            Transform.SetLocalScale(imgTransform, new Vector3(1, 1, 1));

            Vector3 anchorPos = new Vector3(offsetX, offsetY, 0);
            RectTransform_anchoredPosition3D.GetSetMethod().Invoke(imgTransform, new IntPtr[] { new IntPtr(&anchorPos) });
        }

        static void SetFull(IntPtr transform)
        {
            SetSize(transform,
                new AssetHelper.Vector2(0, 0), new AssetHelper.Vector2(1, 1),
                new AssetHelper.Vector2(0, 0), new AssetHelper.Vector2(0, 0));
        }

        static void SetSize(IntPtr transform,
            AssetHelper.Vector2 anchorMin, AssetHelper.Vector2 anchorMax,
            AssetHelper.Vector2 offsetMin, AssetHelper.Vector2 offsetMax)
        {
            RectTransform_anchorMin.GetSetMethod().Invoke(transform, new IntPtr[] { new IntPtr(&anchorMin) });
            RectTransform_anchorMax.GetSetMethod().Invoke(transform, new IntPtr[] { new IntPtr(&anchorMax) });
            RectTransform_offsetMin.GetSetMethod().Invoke(transform, new IntPtr[] { new IntPtr(&offsetMin) });
            RectTransform_offsetMax.GetSetMethod().Invoke(transform, new IntPtr[] { new IntPtr(&offsetMax) });
        }

        static void SetSizeDelta(IntPtr transform, AssetHelper.Vector2 sizeDelta)
        {
            RectTransform_sizeDelta.GetSetMethod().Invoke(transform, new IntPtr[] { new IntPtr(&sizeDelta) });
        }

        static void SetPivot(IntPtr transform, AssetHelper.Vector2 pivot)
        {
            RectTransform_pivot.GetSetMethod().Invoke(transform, new IntPtr[] { new IntPtr(&pivot) });
        }

        public class Dfymoo
        {
            public string ImageType { get; set; }

            public bool HasWidth { get; set; }
            public int Width { get; set; }

            public bool HasHeight { get; set; }
            public int Height { get; set; }

            public Dictionary<string, Item> Items = new Dictionary<string, Item>();

            public bool Load(string path)
            {
                Items.Clear();

                if (!File.Exists(path))
                {
                    return false;
                }

                string n = null;
                string s = null;
                string o = null;

                string i = null;
                string w = null;
                string h = null;

                foreach (string line in File.ReadAllLines(path))
                {
                    if (line.Trim() == "~")
                    {
                        if (n != null)
                        {
                            Item item = new Item(n);

                            if (s != null)
                            {
                                string[] splitted = s.Split();
                                item.X = int.Parse(splitted[0]);
                                item.Y = int.Parse(splitted[1]);
                                item.Width = int.Parse(splitted[2]);
                                item.Height = int.Parse(splitted[3]);
                                item.HasSourceRect = true;
                            }

                            if (o != null)
                            {
                                string[] splitted = o.Split();
                                item.OffsetSizeX = int.Parse(splitted[0]);
                                item.OffsetSizeY = int.Parse(splitted[1]);
                                item.OffsetSizeWidth = int.Parse(splitted[2]);
                                item.OffsetSizeHeight = int.Parse(splitted[3]);
                                item.HasOffsetSize = true;
                            }

                            Items.Add(item.Name, item);
                        }
                        else if (i != null)
                        {
                            ImageType = i;
                            if (w != null)
                            {
                                Width = int.Parse(w);
                                HasWidth = true;
                            }
                            if (h != null)
                            {
                                Height = int.Parse(h);
                                HasHeight = true;
                            }
                        }

                        n = s = o = i = w = h = null;
                    }
                    else if (line.Length > 1)
                    {
                        char firstChar = line[0];
                        string data = line.Substring(2);
                        switch (firstChar)
                        {
                            case 'n': n = data; break;
                            case 'o': o = data; break;
                            case 's': s = data; break;

                            case 'i': i = data; break;
                            case 'w': w = data; break;
                            case 'h': h = data; break;
                        }
                    }
                }

                return true;
            }

            public class Item
            {
                public string Name { get; set; }

                public bool HasSourceRect { get; set; }
                public int X { get; set; }
                public int Y { get; set; }
                public int Width { get; set; }
                public int Height { get; set; }

                // The desired size of the new rect is different than the source rect.
                // OffsetSizeX/Y = image offset into the new rect size
                // OffsetSizeWidth/Height = new rect size
                public bool HasOffsetSize { get; set; }
                public int OffsetSizeX { get; set; }
                public int OffsetSizeY { get; set; }
                public int OffsetSizeWidth { get; set; }
                public int OffsetSizeHeight { get; set; }

                public Item(string name)
                {
                    Name = name;
                }
            }
        }
    }
}
