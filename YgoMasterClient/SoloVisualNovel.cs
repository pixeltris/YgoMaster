using IL2CPP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using YgoMaster;
using YgoMasterClient;

namespace YgoMasterClient
{
    static unsafe class SoloVisualNovel
    {
        static IntPtr rectMask2DType;
        static IntPtr canvasGroupType;
        static IntPtr rectTransformType;
        static IntPtr imageType;

        static IL2Method rectTransformSetAnchoredPosition;
        static IL2Method rectTransformSetAnchorMin;
        static IL2Method rectTransformSetAnchorMax;
        static IL2Method rectTransformSetOffsetMin;
        static IL2Method rectTransformSetOffsetMax;
        static IL2Method rectTransformSetPivot;
        static IL2Method rectTransformSetSizeDelta;

        static IntPtr commonImagesAtlas;
        static IL2Method spriteAtlasGetSprite;

        static IL2Property Image_type;
        static IL2Property Image_preserveAspect;
        static IL2Method Image_SetSprite;

        static IntPtr titleFont;
        static IntPtr bodyFont;
        static IntPtr ExtendedTextMeshProUGUI_Type;
        static IL2Property TMP_Text_font;
        static IL2Property TMP_Text_fontSize;
        static IL2Property TMP_Text_lineSpacing;
        static IL2Property TMP_Text_horizontalAlignment;
        static IL2Method TMP_FontAsset_CreateFontAsset;
        static IL2Class Font_Class;

        static IntPtr textTitleComponent;
        static IntPtr textBodyComponent;

        static IntPtr HorizontalOrVerticalLayoutGroup_Type;
        static IL2Property HorizontalOrVerticalLayoutGroup_spacing;

        static IntPtr SelectionButton_Type;
        static IL2Field SelectionButton_onClick;
        static IL2Field SelectionButton__soundLabelClick;
        static IL2Method UnityEvent_AddListener;
        static IntPtr Selector_Type;
        static IL2Method Selector_AddItem;
        static IL2Field SelectionItem__selector;

        static IL2Property Screen_width;
        static IL2Property Screen_height;

        static IL2Class Tween_Class;
        static IntPtr Tween_Type;
        static IL2Field Tween_label;
        static IL2Field Tween_duration;
        static IL2Field Tween_easing;
        static IL2Method Tween_PlayLabel;
        static IL2Method Tween_Reset;
        static IL2Method Tween_Play;
        static IL2Method Tween_Stop;
        static IL2Method Tween_IsPlaying;
        static IntPtr TweenAlpha_Type;
        static IL2Field TweenAlpha_from;
        static IL2Field TweenAlpha_to;
        static IntPtr TweenPosition_Type;
        static IL2Field TweenPosition_from;
        static IL2Field TweenPosition_to;
        static IL2Field TweenPosition_rtrans;
        static IntPtr TweenScale_Type;
        static IL2Field TweenScale_from;
        static IL2Field TweenScale_to;

        static Dictionary<string, int> tweenEasingValues = new Dictionary<string, int>();

        delegate void Del_TweenPositionOnSetValue(IntPtr thisPtr, float par);
        static Hook<Del_TweenPositionOnSetValue> hookTweenPositionOnSetValue;

        static IL2Property CanvasGroup_alpha;

        static ScriptEntry lastNonInfn8TalkingScriptEntry;
        static ScriptEntry currentScriptEntry;
        static List<ScriptEntry> scriptEntries = new List<ScriptEntry>();
        static Dictionary<string, object> uiSettings = null;
        static int screenWidth;
        static int screenHeight;
        static IntPtr bgImageComp;

        static float characterLeftFrontOffset;
        static float characterLeftBackOffset;
        static float characterRightFrontOffset;
        static float characterRightBackOffset;
        static float characterLeftInfn8Offset;
        static float characterOffsetY;
        static float characterInfn8OffsetY;
        static float characterTweenScaleSize;
        static float characterTweenScaleDuration;
        static float characterTweenAlphaDuration;
        static float characterTweenMoveDuration;

        static Dictionary<string, CharacterPosition> characterPositions = new Dictionary<string, CharacterPosition>();
        static Dictionary<string, string> characterImage = new Dictionary<string, string>();
        static Dictionary<string, IntPtr> characterGameObjects = new Dictionary<string, IntPtr>();
        static Dictionary<string, IntPtr> propGameObjects = new Dictionary<string, IntPtr>();
        static string lastPropOn;

        static Dictionary<string, string> characterNames = new Dictionary<string, string>();

        const string loadingSoundEffect = "SE_DUEL_ENTRY_LOOP";

        public const string baseDir = "LinkEvolution";
        static bool done;
        static bool finishedTweens;
        static bool hasLoadedTextures;
        static bool hasCreatedUI;
        static Dictionary<string, IntPtr> loadedSprites = new Dictionary<string, IntPtr>();
        static Dictionary<string, uint> loadedSpritesGC = new Dictionary<string, uint>();
        static Dictionary<string, AssetHelper.Vector2> spriteSizes = new Dictionary<string, AssetHelper.Vector2>();
        static Dictionary<IntPtr, CharTweenPos> tweenCharPos = new Dictionary<IntPtr, CharTweenPos>();

        public static bool IsRetryDuel;
        public static string IntroJsonName;
        public static string OutroJsonName;
        static bool isOutro;
        static IntPtr safeArea;
        static IntPtr containerObj;
        static IntPtr mainAreaObj;

        // NOTE: These are actually loading sprites
        static Queue<string> loadSpriteQueue = new Queue<string>();
        static List<string> loadingSprites = new List<string>();
        static IntPtr onLoadCallback;

        const string TweenFade = "Fade";
        const string TweenMove = "Move";
        const string TweenScale = "Scale";
        const string TweenScreenFadeIn = "FadeIn";
        const string TweenScreenFadeOut = "FadeOut";

        class ScriptEntry
        {
            public string Pos;
            public string Char;
            public string Image;
            public string Text;
        }

        class CharTweenPos
        {
            public float current;
            public float from;
            public float to;
            public float sizeY;
            public float offsetY;
        }

        enum CharacterPosition
        {
            None,
            Center,
            Left,
            LeftBack,
            Right,
            RightBack,
            LeftInfn8,
        }

        static SoloVisualNovel()
        {
            IL2Assembly uiAssembly = Assembler.GetAssembly("UnityEngine.UI");
            rectMask2DType = uiAssembly.GetClass("RectMask2D", "UnityEngine.UI").IL2Typeof();
            imageType = uiAssembly.GetClass("Image", "UnityEngine.UI").IL2Typeof();
            Image_SetSprite = uiAssembly.GetClass("Image", "UnityEngine.UI").GetProperty("sprite").GetSetMethod();

            HorizontalOrVerticalLayoutGroup_Type = uiAssembly.GetClass("HorizontalOrVerticalLayoutGroup", "UnityEngine.UI").IL2Typeof();
            HorizontalOrVerticalLayoutGroup_spacing = uiAssembly.GetClass("HorizontalOrVerticalLayoutGroup").GetProperty("spacing");

            IL2Class Image_Class = uiAssembly.GetClass("Image", "UnityEngine.UI");
            Image_type = Image_Class.GetProperty("type");
            Image_preserveAspect = Image_Class.GetProperty("preserveAspect");

            IL2Assembly uiModuleAssembly = Assembler.GetAssembly("UnityEngine.UIModule");
            IL2Class canvasGroupClass = uiModuleAssembly.GetClass("CanvasGroup", "UnityEngine");
            canvasGroupType = canvasGroupClass.IL2Typeof();
            CanvasGroup_alpha = canvasGroupClass.GetProperty("alpha");

            IL2Assembly coreModuleAssembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            rectTransformType = coreModuleAssembly.GetClass("RectTransform", "UnityEngine").IL2Typeof();

            IL2Class rectTransformClassInfo = coreModuleAssembly.GetClass("RectTransform", "UnityEngine");
            rectTransformSetAnchoredPosition = rectTransformClassInfo.GetProperty("anchoredPosition").GetSetMethod();
            rectTransformSetOffsetMin = rectTransformClassInfo.GetProperty("offsetMin").GetSetMethod();
            rectTransformSetOffsetMax = rectTransformClassInfo.GetProperty("offsetMax").GetSetMethod();
            rectTransformSetAnchorMin = rectTransformClassInfo.GetProperty("anchorMin").GetSetMethod();
            rectTransformSetAnchorMax = rectTransformClassInfo.GetProperty("anchorMax").GetSetMethod();
            rectTransformSetPivot = rectTransformClassInfo.GetProperty("pivot").GetSetMethod();
            rectTransformSetSizeDelta = rectTransformClassInfo.GetProperty("sizeDelta").GetSetMethod();

            spriteAtlasGetSprite = coreModuleAssembly.GetClass("SpriteAtlas", "UnityEngine.U2D").GetMethod("GetSprite");

            ExtendedTextMeshProUGUI_Type = CastUtils.IL2Typeof("ExtendedTextMeshProUGUI", "YgomSystem.YGomTMPro", "Assembly-CSharp");
            IL2Assembly textMeshAssembly = Assembler.GetAssembly("Unity.TextMeshPro");
            TMP_FontAsset_CreateFontAsset = textMeshAssembly.GetClass("TMP_FontAsset", "TMPro").GetMethod("CreateFontAsset", x => x.GetParameters().Length == 1);
            IL2Class TMP_Text_Class = textMeshAssembly.GetClass("TMP_Text", "TMPro");
            TMP_Text_font = TMP_Text_Class.GetProperty("font");
            TMP_Text_fontSize = TMP_Text_Class.GetProperty("fontSize");
            TMP_Text_lineSpacing = TMP_Text_Class.GetProperty("lineSpacing");
            TMP_Text_horizontalAlignment = TMP_Text_Class.GetProperty("horizontalAlignment");
            Font_Class = Assembler.GetAssembly("UnityEngine.TextRenderingModule").GetClass("Font", "UnityEngine");

            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class SelectionButton_Class = assembly.GetClass("SelectionButton", "YgomSystem.UI");
            SelectionButton_Type = SelectionButton_Class.IL2Typeof();
            SelectionButton_onClick = SelectionButton_Class.GetField("onClick");
            SelectionButton__soundLabelClick = SelectionButton_Class.GetField("_soundLabelClick");
            UnityEvent_AddListener = coreModuleAssembly.GetClass("UnityEvent", "UnityEngine.Events").GetMethod("AddListener");
            IL2Class Selector_Class = assembly.GetClass("Selector", "YgomSystem.UI");
            Selector_Type = Selector_Class.IL2Typeof();
            Selector_AddItem = Selector_Class.GetMethod("AddItem");
            SelectionItem__selector = assembly.GetClass("SelectionItem", "YgomSystem.UI").GetField("_selector");

            Screen_width = coreModuleAssembly.GetClass("Screen", "UnityEngine").GetProperty("width");
            Screen_height = coreModuleAssembly.GetClass("Screen", "UnityEngine").GetProperty("height");

            Tween_Class = assembly.GetClass("Tween", "YgomSystem.UI");
            Tween_Type = Tween_Class.IL2Typeof();
            Tween_label = Tween_Class.GetField("label");
            Tween_duration = Tween_Class.GetField("duration");
            Tween_easing = Tween_Class.GetField("easing");
            Tween_PlayLabel = Tween_Class.GetMethod("PlayLabel");
            Tween_Reset = Tween_Class.GetMethod("Reset");
            Tween_Play = Tween_Class.GetMethod("Play");
            Tween_Stop = Tween_Class.GetMethod("Stop");
            Tween_IsPlaying = Tween_Class.GetMethod("IsPlaying");

            IL2Class easingEnum = Tween_Class.GetNestedType("Easing");
            foreach (IL2Field field in easingEnum.GetFields())
            {
                if (field.Name != "value__")
                {
                    tweenEasingValues[field.Name] = field.GetValue().GetValueRef<int>();
                }
            }

            IL2Class TweenAlpha_Class = assembly.GetClass("TweenAlpha", "YgomSystem.UI");
            TweenAlpha_Type = TweenAlpha_Class.IL2Typeof();
            TweenAlpha_from = TweenAlpha_Class.GetField("from");
            TweenAlpha_to = TweenAlpha_Class.GetField("to");

            IL2Class TweenPosition_Class = assembly.GetClass("TweenPosition", "YgomSystem.UI");
            TweenPosition_Type = TweenPosition_Class.IL2Typeof();
            TweenPosition_from = TweenPosition_Class.GetField("from");
            TweenPosition_to = TweenPosition_Class.GetField("to");
            TweenPosition_rtrans = TweenPosition_Class.GetField("rtrans");
            hookTweenPositionOnSetValue = new Hook<Del_TweenPositionOnSetValue>(TweenPositionOnSetValue, TweenPosition_Class.GetMethod("OnSetValue"));

            IL2Class TweenScale_Class = assembly.GetClass("TweenScale", "YgomSystem.UI");
            TweenScale_Type = TweenScale_Class.IL2Typeof();
            TweenScale_from = TweenScale_Class.GetField("from");
            TweenScale_to = TweenScale_Class.GetField("to");
        }

        static Action NextOnClick = () =>
        {
            if (!done)
            {
                Next();
            }
        };

        static Action SkipOnClick = () =>
        {
            if (!done)
            {
                ClearLoadedSprites();
                done = true;
                if (isOutro)
                {
                    // The duel end UI / flying card screen looks weird when fading out the novel UI as the circle mask covers
                    // the screen directly after. Therefore we don't fade out the novel UI on the duel end UI
                }
                else
                {
                    PlayTween(containerObj, TweenScreenFadeOut);
                    SoundInterceptor.PlaySE(loadingSoundEffect);
                }
            }
        };

        static Action<IntPtr> OnLoad = (IntPtr pathPtr) =>
        {
            string path = new IL2String(pathPtr).ToString();
            loadedSprites[path] = AssetHelper.GetAsset(path);
            loadedSpritesGC[path] = Import.Handler.il2cpp_gchandle_new(loadedSprites[path], true);
            loadingSprites.Remove(path);
            if (loadSpriteQueue.Count > 0)
            {
                LoadSpriteAsync(loadSpriteQueue.Dequeue());
            }
            if (loadingSprites.Count == 0 && loadSpriteQueue.Count == 0)
            {
                hasLoadedTextures = true;
            }
        };

        static void LoadSpriteAsync(string path)
        {
            if (loadingSprites.Count < 5)
            {
                loadingSprites.Add(path);

                if (onLoadCallback == IntPtr.Zero)
                {
                    onLoadCallback = UnityEngine.Events._UnityAction.CreateAction(OnLoad);
                    Import.Handler.il2cpp_gchandle_new(onLoadCallback, true);
                }
                AssetHelper.Load(path, onLoadCallback);
            }
            else
            {
                loadSpriteQueue.Enqueue(path);
            }
        }

        static void ClearLoadedSprites()
        {
            foreach (KeyValuePair<string, uint> handle in loadedSpritesGC)
            {
                Import.Handler.il2cpp_gchandle_free(handle.Value);
                AssetHelper.Unload(handle.Key);
            }
            loadedSprites.Clear();
            loadedSpritesGC.Clear();
            spriteSizes.Clear();
        }

        public static void Init()
        {
            hasLoadedTextures = false;
            done = false;
            finishedTweens = false;
            safeArea = IntPtr.Zero;
            containerObj = IntPtr.Zero;
            mainAreaObj = IntPtr.Zero;
            ClearLoadedSprites();
            characterPositions.Clear();
            characterImage.Clear();
            characterGameObjects.Clear();
            propGameObjects.Clear();
            tweenCharPos.Clear();
            lastPropOn = null;
            hasCreatedUI = false;
        }

        public static void SetJsonFileNames()
        {
            if (ClientSettings.DisableSoloVisualNovel)
            {
                return;
            }
            IntroJsonName = YgomSystem.Utility.ClientWork.GetStringByJsonPath("Duel.dialog_intro");
            OutroJsonName = YgomSystem.Utility.ClientWork.GetStringByJsonPath("Duel.dialog_outro");
            SanitizeJsonFileName(ref IntroJsonName);
            SanitizeJsonFileName(ref OutroJsonName);
        }

        static void SanitizeJsonFileName(ref string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && !fileName.ToLower().EndsWith(".json"))
            {
                fileName += ".json";
            }
            if (!string.IsNullOrWhiteSpace(fileName) && !File.Exists(Path.Combine(Program.ClientDataDir, baseDir, "dialog", fileName)))
            {
                fileName = null;
            }
        }

        public static bool Run(IntPtr thisPtr, bool isFlyingCardView = false)
        {
            isOutro = isFlyingCardView;
            if ((isOutro && string.IsNullOrWhiteSpace(OutroJsonName)) || (!isOutro && string.IsNullOrWhiteSpace(IntroJsonName)))
            {
                return false;
            }
            if (!isOutro && IsRetryDuel)
            {
                return false;
            }

            if (safeArea == IntPtr.Zero)
            {
                // Prevent the loading sound from looping while the visual novel UI is being displayed
                // NOTE: Some of this sound will play as the effect is played a little before this function is called.
                //       To prevent the initial sound we'd need to intercept the Play function, see SoundInterceptor.cs
                SoundInterceptor.StopSE(loadingSoundEffect);

                done = false;

                if (characterNames.Count == 0)
                {
                    string charNamesJsonFile = Path.Combine(Program.ClientDataDir, baseDir, "CharNames.json");
                    if (File.Exists(charNamesJsonFile))
                    {
                        Dictionary<string, object> charNamesData = MiniJSON.Json.DeserializeStripped(File.ReadAllText(charNamesJsonFile)) as Dictionary<string, object>;
                        if (charNamesData != null)
                        {
                            foreach (KeyValuePair<string, object> entry in charNamesData)
                            {
                                characterNames[entry.Key] = entry.Value as string;
                            }
                        }
                    }
                }

                string targetJson = isOutro ? OutroJsonName : IntroJsonName;
                string jsonFile = Path.Combine(Program.ClientDataDir, baseDir, "dialog", targetJson);
                if (!File.Exists(jsonFile))
                {
                    return false;
                }

                Dictionary<string, object> dialogData = MiniJSON.Json.DeserializeStripped(File.ReadAllText(jsonFile)) as Dictionary<string, object>;
                if (dialogData == null)
                {
                    return false;
                }

                object scriptDataObj;
                if (!dialogData.TryGetValue("script", out scriptDataObj))
                {
                    return false;
                }
                List<object> scriptList = scriptDataObj as List<object>;
                if (scriptList == null)
                {
                    return false;
                }
                scriptEntries.Clear();
                foreach (object scriptEntryObj in scriptList)
                {
                    Dictionary<string, object> scriptEntryDict = scriptEntryObj as Dictionary<string, object>;
                    if (scriptEntryDict == null)
                    {
                        continue;
                    }
                    ScriptEntry scriptEntry = new ScriptEntry();
                    scriptEntry.Pos = Utils.GetValue<string>(scriptEntryDict, "pos");
                    scriptEntry.Char = Utils.GetValue<string>(scriptEntryDict, "char");
                    scriptEntry.Image = Utils.GetValue<string>(scriptEntryDict, "image");
                    // NOTE: Some letters aren't supported by the font we use / have large padding
                    scriptEntry.Text = Utils.GetValue<string>(scriptEntryDict, "text").Replace("’", "'").Replace("‘", "'");
                    scriptEntries.Add(scriptEntry);
                }

                if (string.IsNullOrEmpty(GetFirstBg()))
                {
                    Utils.LogWarning("Couldn't find first bg");
                    return false;
                }

                //if (uiSettings == null)
                {
                    string uiSettingsFile = Path.Combine(Program.ClientDataDir, baseDir, "UISettings.json");
                    if (!File.Exists(uiSettingsFile))
                    {
                        return false;
                    }
                    uiSettings = MiniJSON.Json.DeserializeStripped(File.ReadAllText(uiSettingsFile)) as Dictionary<string, object>;
                    if (uiSettings == null)
                    {
                        return false;
                    }
                    characterLeftFrontOffset = Utils.GetValue<float>(uiSettings, "CharacterLeftFrontOffset");
                    characterLeftBackOffset = Utils.GetValue<float>(uiSettings, "CharacterLeftBackOffset");
                    characterRightFrontOffset = Utils.GetValue<float>(uiSettings, "CharacterRightFrontOffset");
                    characterRightBackOffset = Utils.GetValue<float>(uiSettings, "CharacterRightBackOffset");
                    characterLeftInfn8Offset = Utils.GetValue<float>(uiSettings, "CharacterLeftInfn8Offset");
                    characterOffsetY = Utils.GetValue<float>(uiSettings, "CharacterOffsetY");
                    characterInfn8OffsetY = Utils.GetValue<float>(uiSettings, "Infn8OffsetY");
                    characterTweenScaleSize = Utils.GetValue<float>(uiSettings, "CharacterTweenScaleSize");
                    characterTweenScaleDuration = Utils.GetValue<float>(uiSettings, "CharacterTweenScaleDuration");
                    characterTweenAlphaDuration = Utils.GetValue<float>(uiSettings, "CharacterTweenAlphaDuration");
                    characterTweenMoveDuration = Utils.GetValue<float>(uiSettings, "CharacterTweenMoveDuration");
                }

                if (commonImagesAtlas == IntPtr.Zero)
                {
                    commonImagesAtlas = AssetHelper.LoadImmediateAsset("Images/Common/All/CommonImagesAtlasAll");
                    if (commonImagesAtlas == IntPtr.Zero)
                    {
                        Utils.LogWarning("Couldn't find CommonImagesAtlasAll");
                        return false;
                    }
                    Import.Handler.il2cpp_gchandle_new(commonImagesAtlas, true);// Keep the SpriteAtlas array alive
                }

                safeArea = GameObject.FindGameObjectByPath(Component.GetGameObject(thisPtr), isFlyingCardView ? "CardFlyingUI(Clone).Message.SafeArea" : "SoloStartProductionUI(Clone).SafeArea");

                HashSet<string> texturesToLoad = new HashSet<string>();
                foreach (ScriptEntry entry in scriptEntries)
                {
                    string path = null;
                    if (entry.Char == "command")
                    {
                        switch (entry.Pos)
                        {
                            case "PROP_ON":
                                path = baseDir + "/dialog_props/" + entry.Image;
                                break;
                            case "PROP_OFF":
                                break;
                            case "BG":
                                path = baseDir + "/dialog_bg/" + entry.Image;
                                break;
                            default:
                                Utils.LogWarning("Unhandled dialog command type '" + entry.Pos + "'");
                                break;
                        }
                    }
                    else
                    {
                        string subImage = entry.Image;
                        if (string.IsNullOrEmpty(subImage))
                        {
                            subImage = "neutral";
                        }
                        path = baseDir + "/dialog_chars/" + entry.Char + "_" + subImage;
                    }
                    if (!string.IsNullOrEmpty(path) && texturesToLoad.Add(path))
                    {
                        if (File.Exists(Path.Combine(Program.ClientDataDir, path + ".png")) ||
                            File.Exists(Path.Combine(Program.ClientDataDir, path + ".jpg")))
                        {
                            LoadSpriteAsync(path);
                        }
                        else
                        {
                            Utils.LogWarning("Couldn't find image '" + path + "'");
                        }
                    }
                }
            }
            if (safeArea != IntPtr.Zero && hasLoadedTextures && !hasCreatedUI)
            {
                hasCreatedUI = true;
                string titleFontString = Utils.GetValue<string>(uiSettings, "BodyFont");
                titleFont = LoadFont(titleFontString);
                if (titleFont == IntPtr.Zero)
                {
                    Utils.LogWarning("Couldn't find TitleFont " + titleFontString);
                    return false;
                }
                string bodyFontString = Utils.GetValue<string>(uiSettings, "BodyFont");
                bodyFont = LoadFont(bodyFontString);
                if (bodyFont == IntPtr.Zero)
                {
                    Utils.LogWarning("Couldn't find BodyFont " + bodyFontString);
                    return false;
                }

                IntPtr safeAreaTransform = GameObject.GetTranform(safeArea);

                containerObj = GameObject.New();
                UnityObject.SetName(containerObj, "SoloVisualNovel");
                GameObject.AddComponent(containerObj, rectTransformType);
                GameObject.AddComponent(containerObj, canvasGroupType);
                SetupTweenAlpha(GameObject.AddComponent(containerObj, TweenAlpha_Type), TweenScreenFadeOut, 1, 0, Utils.GetValue<float>(uiSettings, "FadeOutDuration"));
                SetupTweenAlpha(GameObject.AddComponent(containerObj, TweenAlpha_Type), TweenScreenFadeIn, 0, 1, Utils.GetValue<float>(uiSettings, "FadeInDuration"));
                IntPtr containerTransform = GameObject.GetTranform(containerObj);
                Transform.SetParent(containerTransform, safeAreaTransform);
                SetSize(containerTransform, "ContainerTransform");

                PlayTween(containerObj, TweenScreenFadeIn);

                mainAreaObj = GameObject.New();
                UnityObject.SetName(mainAreaObj, "Main");
                GameObject.AddComponent(mainAreaObj, rectMask2DType);
                IntPtr mainAreaTransform = GameObject.GetTranform(mainAreaObj);
                Transform.SetParent(mainAreaTransform, containerTransform);
                SetSize(mainAreaTransform, "MainTransform");

                IntPtr backgroundObj = GameObject.New();
                UnityObject.SetName(backgroundObj, "Background");
                bgImageComp = GameObject.AddComponent(backgroundObj, imageType);
                SetBgImage(GetFirstBg());
                Transform.SetParent(GameObject.GetTranform(backgroundObj), mainAreaTransform);
                SetSize(GameObject.GetTranform(backgroundObj), "BgTransform");

                // Need a container for infn8 otherwise it seems to get added above the text area
                IntPtr infn8ContainerObj = GameObject.New();
                UnityObject.SetName(infn8ContainerObj, "Infn8Container");
                GameObject.AddComponent(infn8ContainerObj, rectTransformType);
                Transform.SetParent(GameObject.GetTranform(infn8ContainerObj), containerTransform);
                SetFull(GameObject.GetTranform(infn8ContainerObj));

                IntPtr charactersObj = GameObject.New();
                UnityObject.SetName(charactersObj, "Characters");
                GameObject.AddComponent(charactersObj, rectTransformType);
                Transform.SetParent(GameObject.GetTranform(charactersObj), mainAreaTransform);
                SetFull(GameObject.GetTranform(charactersObj));
                CreateCharactersObjects(GameObject.GetTranform(infn8ContainerObj), GameObject.GetTranform(charactersObj));

                IntPtr bgFrameObj = GameObject.New();
                UnityObject.SetName(bgFrameObj, "BgFrame");
                string bgFrameImageString = Utils.GetValue<string>(uiSettings, "BgFrameImage");
                if (!string.IsNullOrEmpty(bgFrameImageString))
                {
                    IntPtr bgFrameSprite = GetAtlasSprite(bgFrameImageString);
                    if (bgFrameSprite != IntPtr.Zero)
                    {
                        IntPtr bgFrameImage = GameObject.AddComponent(bgFrameObj, imageType);
                        Image_SetSprite.Invoke(bgFrameImage, new IntPtr[] { bgFrameSprite });
                        int bgFrameImageType = 1;//sliced
                        Image_type.GetSetMethod().Invoke(bgFrameImage, new IntPtr[] { new IntPtr(&bgFrameImageType) });
                    }
                }
                Transform.SetParent(GameObject.GetTranform(bgFrameObj), mainAreaTransform);
                SetSize(GameObject.GetTranform(bgFrameObj), "BgFrameTransform");

                IntPtr textAreaObj = GameObject.New();
                UnityObject.SetName(textAreaObj, "TextArea");
                string textAreaBgImageString = Utils.GetValue<string>(uiSettings, "TextAreaBgImage");
                if (!string.IsNullOrEmpty(textAreaBgImageString))
                {
                    IntPtr textAreaBgSprite = GetAtlasSprite(textAreaBgImageString);
                    if (textAreaBgSprite != IntPtr.Zero)
                    {
                        IntPtr textAreaBgImage = GameObject.AddComponent(textAreaObj, imageType);
                        Image_SetSprite.Invoke(textAreaBgImage, new IntPtr[] { textAreaBgSprite });
                    }
                }
                IntPtr textAreaTransform = GameObject.GetTranform(textAreaObj);
                Transform.SetParent(textAreaTransform, containerTransform);
                SetSize(textAreaTransform, "TextAreaTransform");

                IntPtr textAreaLineObj = GameObject.New();
                UnityObject.SetName(textAreaLineObj, "Line");
                GameObject.AddComponent(textAreaLineObj, rectTransformType);
                string textAreaLineImageString = Utils.GetValue<string>(uiSettings, "TextAreaLineImage");
                if (!string.IsNullOrEmpty(textAreaLineImageString))
                {
                    IntPtr textAreaLineSprite = GetAtlasSprite(textAreaLineImageString);
                    if (textAreaLineSprite != IntPtr.Zero)
                    {
                        IntPtr textAreaLineImage = GameObject.AddComponent(textAreaLineObj, imageType);
                        Image_SetSprite.Invoke(textAreaLineImage, new IntPtr[] { textAreaLineSprite });
                        int textAreaLineImageType = 2;//tiled
                        Image_type.GetSetMethod().Invoke(textAreaLineImage, new IntPtr[] { new IntPtr(&textAreaLineImageType) });
                    }
                }
                Transform.SetParent(GameObject.GetTranform(textAreaLineObj), textAreaTransform);
                SetSize(GameObject.GetTranform(textAreaLineObj), "TextAreaLineTransform");

                IntPtr frameObj = GameObject.New();
                UnityObject.SetName(frameObj, "Frame");
                string textAreaFrameImageString = Utils.GetValue<string>(uiSettings, "TextAreaFrameImage");
                if (!string.IsNullOrEmpty(textAreaFrameImageString))
                {
                    IntPtr frameSprite = GetAtlasSprite(textAreaFrameImageString);
                    if (frameSprite != IntPtr.Zero)
                    {
                        IntPtr frameImage = GameObject.AddComponent(frameObj, imageType);
                        Image_SetSprite.Invoke(frameImage, new IntPtr[] { frameSprite });
                        int frameImageType = 1;//sliced
                        Image_type.GetSetMethod().Invoke(frameImage, new IntPtr[] { new IntPtr(&frameImageType) });
                    }
                }
                Transform.SetParent(GameObject.GetTranform(frameObj), textAreaTransform);
                SetSize(GameObject.GetTranform(frameObj), "TextAreaFrameTransform");

                IntPtr textTitleObj = GameObject.New();
                UnityObject.SetName(textTitleObj, "Title");
                textTitleComponent = GameObject.AddComponent(textTitleObj, ExtendedTextMeshProUGUI_Type);
                Transform.SetParent(GameObject.GetTranform(textTitleObj), textAreaTransform);
                TMP_Text_font.GetSetMethod().Invoke(textTitleComponent, new IntPtr[] { titleFont });
                SetSize(GameObject.GetTranform(textTitleObj), "TitleTransform");
                float titleFontSize = Utils.GetValue<float>(uiSettings, "TitleSize");
                TMP_Text_fontSize.GetSetMethod().Invoke(textTitleComponent, new IntPtr[] { new IntPtr(&titleFontSize) });

                IntPtr textBodyObj = GameObject.New();
                UnityObject.SetName(textBodyObj, "Body");
                textBodyComponent = GameObject.AddComponent(textBodyObj, ExtendedTextMeshProUGUI_Type);
                Transform.SetParent(GameObject.GetTranform(textBodyObj), textAreaTransform);
                TMP_Text_font.GetSetMethod().Invoke(textBodyComponent, new IntPtr[] { bodyFont });
                float fontSize = Utils.GetValue<float>(uiSettings, "BodySize");
                TMP_Text_fontSize.GetSetMethod().Invoke(textBodyComponent, new IntPtr[] { new IntPtr(&fontSize) });
                float lineSpacing = Utils.GetValue<float>(uiSettings, "BodyLineSpacing");
                TMP_Text_lineSpacing.GetSetMethod().Invoke(textBodyComponent, new IntPtr[] { new IntPtr(&lineSpacing) });
                SetSize(GameObject.GetTranform(textBodyObj), "BodyTransform");

                IntPtr selector = GameObject.GetComponent(GameObject.FindGameObjectByPath(Component.GetGameObject(thisPtr), isFlyingCardView ? "CardFlyingUI(Clone)" : "SoloStartProductionUI(Clone)"), Selector_Type);
                AddButtons(containerTransform, selector);

                Transform.SetLocalPosition(containerTransform, new Vector3(0, 0, 0));
                Transform.SetLocalScale(containerTransform, new Vector3(1, 1, 1));

                YgoMasterClient.AssetHelper.Vector2 anchorPos = new AssetHelper.Vector2(0, 0);
                rectTransformSetAnchoredPosition.Invoke(containerTransform, new IntPtr[] { new IntPtr(&anchorPos) });

                AddTweenScale(mainAreaObj, TweenScale);

                if (IsFirstTextInfn8())
                {
                    Transform.SetLocalScale(mainAreaTransform, new Vector3(0.5f, 0.5f, 0.5f));

                    IntPtr infn8CharObj;
                    if (characterGameObjects.TryGetValue("infn8", out infn8CharObj))
                    {
                        CharTeleTo(infn8CharObj, characterLeftInfn8Offset);
                        characterPositions["infn8"] = CharacterPosition.LeftInfn8;
                        GameObject.SetActive(infn8CharObj, true);
                    }
                }

                currentScriptEntry = null;
                lastNonInfn8TalkingScriptEntry = null;
                Next();
            }
            if (done)
            {
                if (!finishedTweens)
                {
                    IntPtr tween = GameObject.GetComponent(containerObj, Tween_Type);
                    if (tween != IntPtr.Zero)
                    {
                        bool isActive = false;
                        if (!Tween_IsPlaying.Invoke(tween, new IntPtr[] { new IL2String("").ptr, new IntPtr(&isActive) }).GetValueRef<csbool>())
                        {
                            finishedTweens = true;
                        }
                    }
                }
                return !finishedTweens;
            }
            else
            {
                return true;
            }
        }

        static void CreateCharactersObjects(IntPtr infn8ContainerTransform, IntPtr charactersTransform)
        {
            //screenWidth = Screen_width.GetGetMethod().Invoke().GetValueRef<int>();
            //screenHeight = Screen_height.GetGetMethod().Invoke().GetValueRef<int>();
            screenWidth = 1920;
            screenHeight = 1080;

            foreach (ScriptEntry scriptEntry in scriptEntries)
            {
                if (scriptEntry.Char == "command")
                {
                    if (scriptEntry.Pos == "PROP_ON" && !propGameObjects.ContainsKey(scriptEntry.Image))
                    {
                        string imagePath = baseDir + "/dialog_props/" + scriptEntry.Image;
                        propGameObjects[scriptEntry.Image] = CreatePropObject(charactersTransform, scriptEntry.Image, imagePath);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(scriptEntry.Char) && !characterGameObjects.ContainsKey(scriptEntry.Char))
                {
                    bool isInfn8 = scriptEntry.Char == "infn8";
                    string imagePath = baseDir + "/dialog_chars/" + scriptEntry.Char + "_" + (string.IsNullOrEmpty(scriptEntry.Image) ? "neutral" : scriptEntry.Image);
                    characterPositions[scriptEntry.Char] = CharacterPosition.None;
                    characterGameObjects[scriptEntry.Char] = CreateCharacterObject(isInfn8 ? infn8ContainerTransform : charactersTransform, scriptEntry.Char, imagePath);
                }
            }
        }

        static IntPtr CreateCharacterObject(IntPtr charactersTransform, string name, string image)
        {
            IntPtr characterObj = GameObject.New();
            GameObject.AddComponent(characterObj, rectTransformType);
            GameObject.AddComponent(characterObj, canvasGroupType);
            UnityObject.SetName(characterObj, name);
            Transform.SetParent(GameObject.GetTranform(characterObj), charactersTransform);

            IntPtr imageComp = GameObject.AddComponent(characterObj, imageType);
            AddCharacterTweens(characterObj);

            IntPtr sprite;
            if (loadedSprites.TryGetValue(image, out sprite))
            {
                Image_SetSprite.Invoke(imageComp, new IntPtr[] { sprite });
                bool preserveAspect = true;
                Image_preserveAspect.GetSetMethod().Invoke(imageComp, new IntPtr[] { new IntPtr(&preserveAspect) });

                AssetHelper.Vector2 spriteSize;
                if (!spriteSizes.TryGetValue(image, out spriteSize))
                {
                    int w, h;
                    AssetHelper.GetSpriteSize(sprite, out w, out h);
                    spriteSize.x = w;
                    spriteSize.y = h;
                    spriteSizes[image] = spriteSize;
                }

                float sizeX = (float)((double)spriteSize.x / (double)screenWidth);
                CharTweenPos tween = tweenCharPos[GameObject.GetTranform(characterObj)];
                tween.sizeY = (float)((double)spriteSize.y / (double)screenHeight);
                tween.offsetY = name == "infn8" ? characterInfn8OffsetY : characterOffsetY;
                // places the character slightly left of the screen (the 0.5 is the thing which sets it to left of screen, half width unsures half of the sprite isnt on screen)
                float currentPos = (0.5f + (sizeX / 2.0f));
                SetSize(GameObject.GetTranform(characterObj),
                    new AssetHelper.Vector2(0 - currentPos, 0), new AssetHelper.Vector2(1 - currentPos, tween.sizeY),
                    new AssetHelper.Vector2(0, tween.offsetY), new AssetHelper.Vector2(0, tween.offsetY));
                SetPivot(GameObject.GetTranform(characterObj), new AssetHelper.Vector2(0.5f, 0));
                tweenCharPos[GameObject.GetTranform(characterObj)].current = currentPos;
            }

            GameObject.SetActive(characterObj, false);

            return characterObj;
        }

        static IntPtr CreatePropObject(IntPtr charactersTransform, string name, string image)
        {
            IntPtr characterObj = GameObject.New();
            GameObject.AddComponent(characterObj, rectTransformType);
            UnityObject.SetName(characterObj, name);
            Transform.SetParent(GameObject.GetTranform(characterObj), charactersTransform);

            IntPtr imageComp = GameObject.AddComponent(characterObj, imageType);

            IntPtr sprite;
            if (loadedSprites.TryGetValue(image, out sprite))
            {
                Image_SetSprite.Invoke(imageComp, new IntPtr[] { sprite });
                bool preserveAspect = true;
                Image_preserveAspect.GetSetMethod().Invoke(imageComp, new IntPtr[] { new IntPtr(&preserveAspect) });

                AssetHelper.Vector2 spriteSize;
                if (!spriteSizes.TryGetValue(image, out spriteSize))
                {
                    int w, h;
                    AssetHelper.GetSpriteSize(sprite, out w, out h);
                    spriteSize.x = w;
                    spriteSize.y = h;
                    spriteSizes[image] = spriteSize;
                }

                // TODO: Stretch when screen resolution is greater than 1080p? (some images are created as 1920x1080 to fill the screen)
                float anchorWidth = (float)((double)spriteSize.x / (double)screenWidth);
                SetSize(GameObject.GetTranform(characterObj),
                    new AssetHelper.Vector2((0.5f - (anchorWidth / 2)), 0), new AssetHelper.Vector2((0.5f - (anchorWidth / 2)) + anchorWidth, 1),
                    new AssetHelper.Vector2(0, 0), new AssetHelper.Vector2(0, 0));
                SetPivot(GameObject.GetTranform(characterObj), new AssetHelper.Vector2(0.5f, 0.5f));
            }

            GameObject.SetActive(characterObj, false);

            return characterObj;
        }

        static void SetupTweenAlpha(IntPtr tween, string label, float from, float to, float duration)
        {
            Tween_label.SetValue(tween, new IL2String(label).ptr);
            Tween_duration.SetValue(tween, new IntPtr(&duration));
            TweenAlpha_from.SetValue(tween, new IntPtr(&from));
            TweenAlpha_to.SetValue(tween, new IntPtr(&to));
        }

        static void AddTweenAlpha(IntPtr obj, string label)
        {
            IntPtr tween = GameObject.AddComponent(obj, TweenAlpha_Type);
            Tween_label.SetValue(tween, new IL2String(label).ptr);
            Tween_Stop.Invoke(tween);
        }

        static void AddTweenPos(IntPtr obj, string label)
        {
            IntPtr tween = GameObject.AddComponent(obj, TweenPosition_Type);
            Tween_label.SetValue(tween, new IL2String(label).ptr);
            //SetTweenEasing(tween, "CubicIn");
            Tween_Stop.Invoke(tween);
        }

        static void AddTweenScale(IntPtr obj, string label)
        {
            IntPtr tween = GameObject.AddComponent(obj, TweenScale_Type);
            Tween_label.SetValue(tween, new IL2String(label).ptr);
            Tween_Stop.Invoke(tween);
        }

        static void SetTweenEasing(IntPtr tween, string easing)
        {
            int easingValue;
            if (tweenEasingValues.TryGetValue(easing, out easingValue))
            {
                Tween_easing.SetValue(tween, new IntPtr(&easingValue));
            }
        }

        static IntPtr GetTween(IntPtr obj, string label)
        {
            foreach (IntPtr comp in GameObject.GetComponents(obj, Tween_Class.ptr))
            {
                if (new IL2String(Tween_label.GetValue(comp).ptr).ToString() == label)
                {
                    return comp;
                }
            }
            return IntPtr.Zero;
        }

        static void PlayTweenAlphaTo(IntPtr obj, string label, float duration, float to)
        {
            IntPtr tween = GetTween(obj, label);
            if (tween == IntPtr.Zero)
            {
                return;
            }
            float from = GetAlpha(obj);
            Tween_Reset.Invoke(tween);
            Tween_duration.SetValue(tween, new IntPtr(&duration));
            TweenAlpha_from.SetValue(tween, new IntPtr(&from));
            TweenAlpha_to.SetValue(tween, new IntPtr(&to));
            Tween_Play.Invoke(tween);
        }

        static void PlayTweenPosition(IntPtr obj, string label, float duration)
        {
            IntPtr tween = GetTween(obj, label);
            if (tween == IntPtr.Zero)
            {
                return;
            }
            //Tween_Reset.Invoke(tween);// This crashes for some reason
            Tween_Stop.Invoke(tween);
            Tween_duration.SetValue(tween, new IntPtr(&duration));
            Tween_Play.Invoke(tween);
        }

        static void PlayTweenScale(IntPtr obj, string label, float duration, float scale)
        {
            IntPtr tween = GetTween(obj, label);
            if (tween == IntPtr.Zero)
            {
                return;
            }
            Vector3 from = Transform.GetLocalScale(GameObject.GetTranform(obj));
            Vector3 to = new Vector3(scale, scale, scale);
            Tween_Reset.Invoke(tween);
            Tween_duration.SetValue(tween, new IntPtr(&duration));
            TweenScale_from.SetValue(tween, new IntPtr(&from));
            TweenScale_to.SetValue(tween, new IntPtr(&to));
            Tween_Play.Invoke(tween);
        }

        static bool IsTweenPlaying(IntPtr obj, string label)
        {
            IntPtr tween = GetTween(obj, label);
            if (tween == IntPtr.Zero)
            {
                return false;
            }
            bool isActive = false;
            return Tween_IsPlaying.Invoke(tween, new IntPtr[] { new IL2String(label).ptr, new IntPtr(&isActive) }).GetValueRef<csbool>();
        }

        static float GetAlpha(IntPtr obj)
        {
            IntPtr canvas = GameObject.GetComponent(obj, canvasGroupType);
            return canvas != IntPtr.Zero ? CanvasGroup_alpha.GetGetMethod().Invoke(canvas).GetValueRef<float>() : 0;
        }

        static void SetAlpha(IntPtr obj, float alpha)
        {
            IntPtr canvas = GameObject.GetComponent(obj, canvasGroupType);
            if (canvas != IntPtr.Zero)
            {
                CanvasGroup_alpha.GetSetMethod().Invoke(canvas, new IntPtr[] { new IntPtr(&alpha) });
            }
        }
        
        static void PlayTween(IntPtr gameObject, string tweenLabel)
        {
            IntPtr tween = GameObject.GetComponent(gameObject, Tween_Type);
            if (tween != IntPtr.Zero)
            {
                Tween_PlayLabel.Invoke(tween, new IntPtr[] { new IL2String(tweenLabel).ptr });
            }
        }

        static void AddCharacterTweens(IntPtr characterObj)
        {
            AddTweenAlpha(characterObj, TweenFade);
            AddTweenPos(characterObj, TweenMove);
            AddTweenScale(characterObj, TweenScale);
            tweenCharPos[GameObject.GetTranform(characterObj)] = new CharTweenPos();
        }

        static void TweenPositionOnSetValue(IntPtr thisPtr, float par)
        {
            if (hasCreatedUI && (!done || !finishedTweens))
            {
                IntPtr transform = TweenPosition_rtrans.GetValue(thisPtr).ptr;
                CharTweenPos tweenPos;
                if (tweenCharPos.TryGetValue(transform, out tweenPos))
                {
                    // This is a hacky way of moving the chars but it's the best we have for how we are setting anchors
                    CharTeleTo(Component.GetGameObject(transform), tweenPos.from * (1f - par) + tweenPos.to * par);
                    return;
                }
            }
            hookTweenPositionOnSetValue.Original(thisPtr, par);
        }

        static void Next()
        {
            if (done)
            {
                return;
            }
            List<ScriptEntry> steps = new List<ScriptEntry>();
            ScriptEntry talkingScript = null;
            bool hasNextEntry = false;
            int currentScriptIndex = currentScriptEntry == null ? -1 : scriptEntries.IndexOf(currentScriptEntry);
            for (int i = currentScriptIndex + 1; i < scriptEntries.Count; i++)
            {
                ScriptEntry scriptEntry = scriptEntries[i];
                steps.Add(scriptEntry);
                hasNextEntry = true;
                if (!string.IsNullOrWhiteSpace(scriptEntry.Text))
                {
                    talkingScript = scriptEntry;
                    break;
                }
            }
            if (talkingScript == null && hasNextEntry)
            {
                Utils.LogWarning("NPC dialog doesn't finish with text. Skipping some movement of NPCs");
            }
            if (talkingScript == null)
            {
                SkipOnClick();
            }
            else
            {
                foreach (ScriptEntry entry in steps)
                {
                    if (entry.Char == "command")
                    {
                        switch (entry.Pos)
                        {
                            case "PROP_ON":
                                {
                                    IntPtr propObj;
                                    if (propGameObjects.TryGetValue(entry.Image, out propObj))
                                    {
                                        // TODO: Support FADEIN/FADEOUT for PROP_ON / PROP_OFF (though the regular game doesn't even have these)
                                        lastPropOn = entry.Image;
                                        GameObject.SetActive(propObj, true);
                                        Transform.SetAsLastSibling(GameObject.GetTranform(propObj));
                                    }
                                }
                                break;
                            case "PROP_OFF":
                                {
                                    string prop = !string.IsNullOrWhiteSpace(entry.Image) ? entry.Image : lastPropOn;
                                    IntPtr propObj;
                                    if (propGameObjects.TryGetValue(prop, out propObj))
                                    {
                                        GameObject.SetActive(propObj, false);
                                    }
                                }
                                break;
                            case "BG":
                                SetBgImage(entry.Image);
                                break;
                        }
                    }
                    else if (entry.Char == "infn8")
                    {
                        CharacterPosition infn8Pos;
                        characterPositions.TryGetValue(entry.Char, out infn8Pos);
                        if (infn8Pos == CharacterPosition.None)
                        {
                            CharMoveTo(entry.Char, CharacterPosition.LeftInfn8, false);
                        }
                    }
                    else
                    {
                        CharacterPosition infn8Pos;
                        characterPositions.TryGetValue("infn8", out infn8Pos);
                        if (infn8Pos != CharacterPosition.None)
                        {
                            CharMoveTo("infn8", CharacterPosition.None, false);
                        }

                        if (!string.IsNullOrWhiteSpace(entry.Image) || !string.IsNullOrWhiteSpace(entry.Text) || characterPositions[entry.Char] == CharacterPosition.None)
                        {
                            CharSetImage(entry.Char, entry.Image);
                        }
                        string[] pos = entry.Pos.Split(new char[] { ':', '_' }, StringSplitOptions.RemoveEmptyEntries);
                        bool fadeIn = pos.Any(x => x.ToUpper() == "FADEIN");
                        bool fadeOut = pos.Any(x => x.ToUpper() == "FADEOUT");
                        bool posBack = pos.Any(x => x.ToUpper() == "BACK");
                        bool posFront = pos.Any(x => x.ToUpper() == "FRONT");
                        bool posLeft = pos.Any(x => x.ToUpper() == "LEFT");
                        bool posRight = pos.Any(x => x.ToUpper() == "RIGHT");
                        bool posCenter = pos.Any(x => x.ToUpper() == "CENTER");
                        bool posNone = pos.Any(x => x.ToUpper() == "NONE");
                        if (!string.IsNullOrWhiteSpace(entry.Text))
                        {
                            posBack = false;
                        }
                        if (pos.Length > 0)
                        {
                            if (fadeOut)
                            {
                                CharFadeOut(entry.Char);
                            }
                            else if (posLeft || posRight)
                            {
                                CharacterPosition sideFront = posRight ? CharacterPosition.Right : CharacterPosition.Left;
                                CharacterPosition sideBack = posRight ? CharacterPosition.RightBack : CharacterPosition.LeftBack;
                                if (posBack)
                                {
                                    if (characterPositions.Values.Contains(sideBack))
                                    {
                                        CharMoveTo(characterPositions.Where(x => x.Value == sideBack).Select(x => x.Key).First(), CharacterPosition.None);
                                    }
                                    CharMoveTo(entry.Char, sideBack, fadeIn);
                                }
                                else
                                {
                                    if (characterPositions.Values.Contains(sideFront))
                                    {
                                        if (characterPositions.Values.Contains(sideBack))
                                        {
                                            CharMoveTo(characterPositions.Where(x => x.Value == sideBack).Select(x => x.Key).First(), CharacterPosition.None);
                                        }
                                        CharMoveTo(characterPositions.Where(x => x.Value == sideFront).Select(x => x.Key).First(), sideBack);
                                    }
                                    CharMoveTo(entry.Char, sideFront, fadeIn);
                                }
                            }
                            else if (posCenter)
                            {
                                CharMoveTo(entry.Char, CharacterPosition.Center, fadeIn);
                            }
                            else if (posNone)
                            {
                                CharMoveTo(entry.Char, CharacterPosition.None);
                            }
                            else
                            {
                                Utils.LogWarning("TODO: Unsupported pos '" + entry.Pos + "' on char '" + entry.Char + "'");
                            }
                        }
                        else
                        {
                            // If the target character doesn't have a position we need to set one
                            CharacterPosition charPos;
                            characterPositions.TryGetValue(entry.Char, out charPos);
                            if (charPos == CharacterPosition.None)
                            {
                                if (!characterPositions.Values.Contains(CharacterPosition.Left))
                                {
                                    CharMoveTo(entry.Char, CharacterPosition.Left);
                                }
                                else if (!characterPositions.Values.Contains(CharacterPosition.Right))
                                {
                                    CharMoveTo(entry.Char, CharacterPosition.Right);
                                }
                                else if (!characterPositions.Values.Contains(CharacterPosition.LeftBack))
                                {
                                    CharMoveTo(characterPositions.Where(x => x.Value == CharacterPosition.Left).Select(x => x.Key).First(), CharacterPosition.LeftBack);
                                    CharMoveTo(entry.Char, CharacterPosition.Left);
                                }
                                else if (!characterPositions.Values.Contains(CharacterPosition.RightBack))
                                {
                                    CharMoveTo(characterPositions.Where(x => x.Value == CharacterPosition.Right).Select(x => x.Key).First(), CharacterPosition.RightBack);
                                    CharMoveTo(entry.Char, CharacterPosition.Right);
                                }
                                else
                                {
                                    CharMoveTo(characterPositions.Where(x => x.Value == CharacterPosition.LeftBack).Select(x => x.Key).First(), CharacterPosition.None);
                                    CharMoveTo(characterPositions.Where(x => x.Value == CharacterPosition.Left).Select(x => x.Key).First(), CharacterPosition.LeftBack);
                                    CharMoveTo(entry.Char, CharacterPosition.Left);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(entry.Text))
                            {
                                // Always move the talking character to the front
                                if (charPos == CharacterPosition.LeftBack)
                                {
                                    CharMoveTo(characterPositions.Where(x => x.Value == CharacterPosition.Left).Select(x => x.Key).First(), CharacterPosition.LeftBack);
                                    CharMoveTo(entry.Char, CharacterPosition.Left);
                                }
                                else if (charPos == CharacterPosition.RightBack)
                                {
                                    CharMoveTo(characterPositions.Where(x => x.Value == CharacterPosition.Right).Select(x => x.Key).First(), CharacterPosition.RightBack);
                                    CharMoveTo(entry.Char, CharacterPosition.Right);
                                }
                            }
                        }
                    }
                }

                string[] sortedChars =
                {
                    characterPositions.Where(x => x.Value == CharacterPosition.RightBack).FirstOrDefault().Key,
                    characterPositions.Where(x => x.Value == CharacterPosition.LeftBack).FirstOrDefault().Key,
                    characterPositions.Where(x => x.Value == CharacterPosition.Right).FirstOrDefault().Key,
                    characterPositions.Where(x => x.Value == CharacterPosition.Left).FirstOrDefault().Key,
                    characterPositions.Where(x => x.Value == CharacterPosition.Center).FirstOrDefault().Key,
                    talkingScript != null ? talkingScript.Char : null
                };
                foreach (string character in sortedChars)
                {
                    IntPtr charObj;
                    if (character != null && characterGameObjects.TryGetValue(character, out charObj))
                    {
                        Transform.SetAsLastSibling(GameObject.GetTranform(charObj));
                    }
                }

                foreach (KeyValuePair<string, IntPtr> prop in propGameObjects)
                {
                    if (GameObject.IsActive(prop.Value))
                    {
                        Transform.SetAsLastSibling(GameObject.GetTranform(prop.Value));
                    }
                }

                CharacterPosition lastTalkingCharPos = CharacterPosition.None;
                if (lastNonInfn8TalkingScriptEntry != null)
                {
                    characterPositions.TryGetValue(lastNonInfn8TalkingScriptEntry.Char, out lastTalkingCharPos);
                }
                if (lastNonInfn8TalkingScriptEntry != null && ((lastNonInfn8TalkingScriptEntry.Char != talkingScript.Char && talkingScript.Char != "infn8") || talkingScript == null || lastTalkingCharPos == CharacterPosition.None))
                {
                    // Scale down when this is no longer the actively talking character (except if the talking character is infn8)
                    SetCharScale(lastNonInfn8TalkingScriptEntry.Char, 1f);
                }
                if (talkingScript != null)
                {
                    SetCharScale(talkingScript.Char, characterTweenScaleSize);
                }

                float screenScaleFrom = 0.5f;
                float screenScaleTo = 1;
                float screenScaleDelay = 0.5f;
                Vector3 screenScale = Transform.GetLocalScale(GameObject.GetTranform(mainAreaObj));
                if (talkingScript != null && talkingScript.Char == "infn8")
                {
                    if (screenScale.x != screenScaleFrom)
                    {
                        float percent = (screenScale.x - screenScaleFrom) / (screenScaleTo - screenScaleFrom);
                        PlayTweenScale(mainAreaObj, TweenScale, screenScaleDelay * percent, screenScaleFrom);
                    }
                }
                else
                {
                    if (screenScale.x != screenScaleTo)
                    {
                        float percent = (screenScaleTo - screenScale.x) / (screenScaleTo - screenScaleFrom);
                        PlayTweenScale(mainAreaObj, TweenScale, screenScaleDelay * percent, screenScaleTo);
                    }
                }

                string talkingCharName;
                if (!characterNames.TryGetValue(talkingScript.Char, out talkingCharName))
                {
                    talkingCharName = talkingScript.Char;
                }
                CharacterPosition talkingCharPos;
                characterPositions.TryGetValue(talkingScript.Char, out talkingCharPos);
                HorizontalAlignmentOptions titlePos = HorizontalAlignmentOptions.Left;
                switch (talkingCharPos)
                {
                    case CharacterPosition.Left:
                    case CharacterPosition.LeftBack:
                        titlePos = HorizontalAlignmentOptions.Left;
                        break;
                    case CharacterPosition.Center:
                        titlePos = HorizontalAlignmentOptions.Center;
                        break;
                    case CharacterPosition.Right:
                    case CharacterPosition.RightBack:
                        titlePos = HorizontalAlignmentOptions.Right;
                        break;
                }
                SetText(titlePos, talkingCharName, talkingScript.Text);
                currentScriptEntry = talkingScript;
                if (talkingScript.Char != "infn8")
                {
                    lastNonInfn8TalkingScriptEntry = talkingScript;
                }
            }
        }

        static void SetCharScale(string characterName, float scale)
        {
            if (characterName == "infn8")
            {
                return;
            }
            IntPtr charObj;
            if (characterGameObjects.TryGetValue(characterName, out charObj))
            {
                Vector3 currentScale = Transform.GetLocalScale(GameObject.GetTranform(charObj));
                if (currentScale.x != scale)// TODO: Use an epsilon here?
                {
                    PlayTweenScale(charObj, TweenScale, characterTweenScaleDuration, scale);
                }
            }
        }

        static void CharMoveTo(string characterName, CharacterPosition pos, bool fadeIn = false)
        {
            CharacterPosition oldPos;
            characterPositions.TryGetValue(characterName, out oldPos);

            IntPtr charObj;
            if (characterGameObjects.TryGetValue(characterName, out charObj))
            {
                CharSetPos(characterName, charObj, pos, fadeIn, oldPos);
                if (!GameObject.IsActive(charObj))
                {
                    GameObject.SetActive(charObj, true);
                }
            }
            characterPositions[characterName] = pos;
        }

        static void CharFadeOut(string characterName)
        {
            characterPositions[characterName] = CharacterPosition.None;
            IntPtr charObj;
            if (characterGameObjects.TryGetValue(characterName, out charObj))
            {
                PlayTweenAlphaTo(charObj, TweenFade, characterTweenAlphaDuration, 0);
            }
        }

        static bool IsCharVisible(IntPtr charObj, CharacterPosition oldPos)
        {
            if (!GameObject.IsActive(charObj))
            {
                return false;
            }
            if (GetAlpha(charObj) <= 0)
            {
                return false;
            }
            if (oldPos == CharacterPosition.None && !IsTweenPlaying(charObj, TweenFade) && !IsTweenPlaying(charObj, TweenMove))
            {
                return false;
            }
            return true;
        }

        static void CharTeleTo(IntPtr charObj, float offsetX)
        {
            CharTweenPos tween = tweenCharPos[GameObject.GetTranform(charObj)];
            tween.current = offsetX;
            string name = characterGameObjects.FirstOrDefault(x => x.Value == charObj).Key;
            SetSize(GameObject.GetTranform(charObj),
                new AssetHelper.Vector2(0 + offsetX, 0), new AssetHelper.Vector2(1 + offsetX, tween.sizeY),
                new AssetHelper.Vector2(0, tween.offsetY), new AssetHelper.Vector2(0, tween.offsetY));
        }

        static void CharTweenTo(IntPtr charObj, float offsetX)
        {
            tweenCharPos[GameObject.GetTranform(charObj)].from = tweenCharPos[GameObject.GetTranform(charObj)].current;
            tweenCharPos[GameObject.GetTranform(charObj)].to = offsetX;
            PlayTweenPosition(charObj, TweenMove, characterTweenMoveDuration);
        }

        static void CharSetPos(string charName, IntPtr charObj, CharacterPosition pos, bool fadeIn, CharacterPosition oldPos)
        {
            string charImage;
            if (!characterImage.TryGetValue(charName, out charImage))
            {
                charImage = "neutral";
            }

            string imagePath = baseDir + "/dialog_chars/" + charName + "_" + charImage;

            AssetHelper.Vector2 spriteSize;
            spriteSizes.TryGetValue(imagePath, out spriteSize);

            float sizeX = (float)((double)spriteSize.x / (double)screenWidth);

            float offsetX = 0;
            switch (pos)
            {
                case CharacterPosition.LeftInfn8:
                    offsetX = characterLeftInfn8Offset;
                    break;
                case CharacterPosition.Left:
                    offsetX = characterLeftFrontOffset;
                    break;
                case CharacterPosition.Right:
                    offsetX = characterRightFrontOffset;
                    break;
                case CharacterPosition.LeftBack:
                    offsetX = characterLeftBackOffset;
                    break;
                case CharacterPosition.RightBack:
                    offsetX = characterRightBackOffset;
                    break;
                case CharacterPosition.Center:
                    break;
                case CharacterPosition.None:
                    switch (oldPos)
                    {
                        case CharacterPosition.Left:
                        case CharacterPosition.LeftBack:
                        case CharacterPosition.Center:
                        case CharacterPosition.LeftInfn8:
                            offsetX = -(0.5f + (sizeX / 2.0f));
                            break;
                        case CharacterPosition.Right:
                        case CharacterPosition.RightBack:
                            offsetX = (0.5f + (sizeX / 2.0f));
                            break;
                        case CharacterPosition.None:
                            return;
                    }
                    break;
            }

            if (fadeIn)
            {
                if (IsCharVisible(charObj, oldPos))
                {
                    CharTweenTo(charObj, offsetX);
                }
                else
                {
                    CharTeleTo(charObj, offsetX);
                }
                if (!GameObject.IsActive(charObj))
                {
                    SetAlpha(charObj, 0);
                }
                if (GetAlpha(charObj) != 1)
                {
                    PlayTweenAlphaTo(charObj, TweenFade, characterTweenAlphaDuration, 1);
                }
            }
            else
            {
                if (!IsCharVisible(charObj, oldPos))
                {
                    if (GetAlpha(charObj) <= 0)
                    {
                        SetAlpha(charObj, 1);
                    }
                    switch (pos)
                    {
                        case CharacterPosition.Left:
                        case CharacterPosition.LeftBack:
                        case CharacterPosition.Center:
                        case CharacterPosition.LeftInfn8:
                            CharTeleTo(charObj, -(0.5f + (sizeX / 2.0f)));
                            break;
                        case CharacterPosition.Right:
                        case CharacterPosition.RightBack:
                            CharTeleTo(charObj, (0.5f + (sizeX / 2.0f)));
                            break;
                    }
                }
                CharTweenTo(charObj, offsetX);
                if (GetAlpha(charObj) != 1)
                {
                    PlayTweenAlphaTo(charObj, TweenFade, characterTweenAlphaDuration, 1);
                }
            }
        }

        static void CharSetImage(string charName, string charImage)
        {
            if (string.IsNullOrWhiteSpace(charImage))
            {
                charImage = "neutral";
            }
            string imagePath = baseDir + "/dialog_chars/" + charName + "_" + charImage;
            characterImage[charName] = charImage;
            IntPtr charObj;
            if (characterGameObjects.TryGetValue(charName, out charObj))
            {
                IntPtr imageComp = GameObject.GetComponent(charObj, imageType);
                if (imageComp != null)
                {
                    IntPtr sprite;
                    if (loadedSprites.TryGetValue(imagePath, out sprite))
                    {
                        Image_SetSprite.Invoke(imageComp, new IntPtr[] { sprite });

                        // TODO: Might need to offset the position if the image size is different?
                        AssetHelper.Vector2 spriteSize;
                        if (!spriteSizes.TryGetValue(imagePath, out spriteSize))
                        {
                            int w, h;
                            AssetHelper.GetSpriteSize(sprite, out w, out h);
                            spriteSize.x = w;
                            spriteSize.y = h;
                            spriteSizes[imagePath] = spriteSize;
                        }
                    }
                }
            }
        }

        static bool IsFirstTextInfn8()
        {
            foreach (ScriptEntry entry in scriptEntries)
            {
                if (!string.IsNullOrWhiteSpace(entry.Text))
                {
                    return entry.Char == "infn8";
                }
            }
            return false;
        }

        static string GetFirstBg()
        {
            foreach (ScriptEntry entry in scriptEntries)
            {
                if (entry.Char == "command" && entry.Pos == "BG")
                {
                    return entry.Image;
                }
            }
            return null;
        }

        static void SetBgImage(string bg)
        {
            IntPtr bgSprite;
            if (loadedSprites.TryGetValue(baseDir + "/dialog_bg/" + bg, out bgSprite))
            {
                Image_SetSprite.Invoke(bgImageComp, new IntPtr[] { bgSprite });
            }
        }

        static void AddButtons(IntPtr parentTransform, IntPtr selector)
        {
            IntPtr prefab = AssetHelper.LoadImmediateAsset("Scenarios/Player/Viewer/Prefabs/ScenarioUI");
            IntPtr clone = UnityObject.Instantiate(prefab);
            IntPtr obj = GameObject.FindGameObjectByPath(clone, "RootUI.SafeArea.MenuButtonAcordion.Root");
            IntPtr layoutGroupComp = GameObject.GetComponent(GameObject.FindGameObjectByPath(obj, "ChildButtonGroup"), HorizontalOrVerticalLayoutGroup_Type);
            float btnVerticalSpacing = 8;
            HorizontalOrVerticalLayoutGroup_spacing.GetSetMethod().Invoke(layoutGroupComp, new IntPtr[] { new IntPtr(&btnVerticalSpacing) });
            Transform.SetParent(GameObject.GetTranform(obj), parentTransform);

            AddButton(obj, selector, "ChildButtonGroup.ChildButtonSlot1.AutoButton", ClientSettings.CustomTextVisualNovelNext, NextOnClick, ClientSettings.DisableSoloVisualNovelNextButtonSound);
            AddButton(obj, selector, "ChildButtonGroup.ChildButtonSlot2.LogButton", ClientSettings.CustomTextVisualNovelSkip, SkipOnClick);

            UnityObject.Destroy(GameObject.FindGameObjectByPath(obj, "ChildButtonGroup.ChildButtonSlot0"));
            UnityObject.Destroy(GameObject.FindGameObjectByPath(obj, "ChildButtonGroup.ChildButtonSlot3"));
            UnityObject.Destroy(GameObject.FindGameObjectByPath(obj, "ChildButtonGroup.RootButton"));
            AssetHelper.Unload("Scenarios/Player/Viewer/Prefabs/ScenarioUI");
            SetSize(GameObject.GetTranform(obj), "ButtonsTransform");
        }

        static void AddButton(IntPtr buttonsObj, IntPtr selector, string buttonPath, string text, Delegate clickCallback, bool disableClickSound = false)
        {
            IntPtr buttonObj = GameObject.FindGameObjectByPath(buttonsObj, buttonPath);
            IntPtr selectionButtonComp = GameObject.GetComponent(buttonObj, SelectionButton_Type);
            Selector_AddItem.Invoke(selector, new IntPtr[] { selectionButtonComp });
            SelectionItem__selector.SetValue(selectionButtonComp, selector);
            if (disableClickSound)
            {
                // NOTE: An alternative might be to set "SE_MENU_OVERLAP_02" (or another SE_MENU with a slightly softer sound)
                SelectionButton__soundLabelClick.SetValue(selectionButtonComp, new IL2String("").ptr);
            }
            IntPtr textComp = GameObject.GetComponent(GameObject.FindGameObjectByPath(buttonObj, "TextTMP"), ExtendedTextMeshProUGUI_Type);
            TMPro.TMP_Text.SetText(textComp, text);

            IntPtr onClick = SelectionButton_onClick.GetValue(selectionButtonComp).ptr;
            IntPtr callback = UnityEngine.Events._UnityAction.CreateUnityAction(clickCallback);// NOTE: Leaks memory
            UnityEvent_AddListener.Invoke(onClick, new IntPtr[] { callback });
        }

        static IntPtr GetAtlasSprite(string name)
        {
            IL2Object result = spriteAtlasGetSprite.Invoke(commonImagesAtlas, new IntPtr[] { new IL2String(name).ptr });
            return result != null ? result.ptr : IntPtr.Zero;
        }

        static IntPtr LoadFont(string path)
        {
            IntPtr assets = AssetHelper.LoadImmediateAssets(path);
            if (assets != IntPtr.Zero)
            {
                IL2Array<IntPtr> assetsArray = new IL2Array<IntPtr>(assets);
                for (int i = 0; i < assetsArray.Length; i++)
                {
                    IntPtr asset = assetsArray[i];
                    if (Import.Object.il2cpp_object_get_class(asset) == Font_Class.ptr)
                    {
                        IL2Object result = TMP_FontAsset_CreateFontAsset.Invoke(new IntPtr[] { asset });
                        if (result != null)
                        {
                            return result.ptr;
                        }
                    }
                }
            }
            return IntPtr.Zero;
        }

        static void SetFull(IntPtr transform)
        {
            AssetHelper.Vector2 anchorMin = new AssetHelper.Vector2(0, 0);
            rectTransformSetAnchorMin.Invoke(transform, new IntPtr[] { new IntPtr(&anchorMin) });

            AssetHelper.Vector2 anchorMax = new AssetHelper.Vector2(1, 1);
            rectTransformSetAnchorMax.Invoke(transform, new IntPtr[] { new IntPtr(&anchorMax) });

            AssetHelper.Vector2 offsetMin = new AssetHelper.Vector2(0, 0);
            rectTransformSetOffsetMin.Invoke(transform, new IntPtr[] { new IntPtr(&offsetMin) });

            AssetHelper.Vector2 offsetMax = new AssetHelper.Vector2(0, 0);
            rectTransformSetOffsetMax.Invoke(transform, new IntPtr[] { new IntPtr(&offsetMax) });
        }

        static void SetSize(IntPtr transform,
            AssetHelper.Vector2 anchorMin, AssetHelper.Vector2 anchorMax,
            AssetHelper.Vector2 offsetMin, AssetHelper.Vector2 offsetMax)
        {
            rectTransformSetAnchorMin.Invoke(transform, new IntPtr[] { new IntPtr(&anchorMin) });
            rectTransformSetAnchorMax.Invoke(transform, new IntPtr[] { new IntPtr(&anchorMax) });
            rectTransformSetOffsetMin.Invoke(transform, new IntPtr[] { new IntPtr(&offsetMin) });
            rectTransformSetOffsetMax.Invoke(transform, new IntPtr[] { new IntPtr(&offsetMax) });
        }

        static void SetPivot(IntPtr transform, AssetHelper.Vector2 pivot)
        {
            rectTransformSetPivot.Invoke(transform, new IntPtr[] { new IntPtr(&pivot) });
        }

        static void SetSize(IntPtr transform, string jsonEntry)
        {
            bool found = false;
            string data = Utils.GetValue<string>(uiSettings, jsonEntry);
            if (!string.IsNullOrWhiteSpace(data))
            {
                string[] splitted = data.Split(',');
                if (splitted.Length >= 8)
                {
                    AssetHelper.Vector2 anchorMin = new AssetHelper.Vector2();
                    AssetHelper.Vector2 anchorMax = new AssetHelper.Vector2();
                    AssetHelper.Vector2 offsetMin = new AssetHelper.Vector2();
                    AssetHelper.Vector2 offsetMax = new AssetHelper.Vector2();
                    if (float.TryParse(splitted[0], out anchorMin.x) &&
                        float.TryParse(splitted[1], out anchorMin.y) &&
                        float.TryParse(splitted[2], out anchorMax.x) &&
                        float.TryParse(splitted[3], out anchorMax.y) &&
                        float.TryParse(splitted[4], out offsetMin.x) &&
                        float.TryParse(splitted[5], out offsetMin.y) &&
                        float.TryParse(splitted[6], out offsetMax.x) &&
                        float.TryParse(splitted[7], out offsetMax.y))
                    {
                        rectTransformSetAnchorMin.Invoke(transform, new IntPtr[] { new IntPtr(&anchorMin) });
                        rectTransformSetAnchorMax.Invoke(transform, new IntPtr[] { new IntPtr(&anchorMax) });
                        rectTransformSetOffsetMin.Invoke(transform, new IntPtr[] { new IntPtr(&offsetMin) });
                        rectTransformSetOffsetMax.Invoke(transform, new IntPtr[] { new IntPtr(&offsetMax) });
                        found = true;
                    }
                }
            }
            if (!found)
            {
                Utils.LogWarning("Couldn't find full data for '" + jsonEntry + "' in LinkEvolution UISettings.json");
            }
        }

        static void SetText(HorizontalAlignmentOptions align, string title, string body)
        {
            TMPro.TMP_Text.SetText(textTitleComponent, title);
            TMPro.TMP_Text.SetText(textBodyComponent, body);
            TMP_Text_horizontalAlignment.GetSetMethod().Invoke(textTitleComponent, new IntPtr[] { new IntPtr(&align) });
            TMP_Text_horizontalAlignment.GetSetMethod().Invoke(textBodyComponent, new IntPtr[] { new IntPtr(&align) });
        }

        enum HorizontalAlignmentOptions
        {
            Left = 1,
            Center = 2,
            Right = 4,
            Justified = 8,
            Flush = 16,
            Geometry = 32
        }
    }
}

namespace YgomGame.Tutorial
{
    static unsafe class CardFlyingViewController
    {
        static bool ignoreNextStopBGM;
        static IntPtr duelEndLabeledPlayableController;
        public static IntPtr duelClient;
        public static bool IsHacked;
        public static IntPtr hackedInstance;

        static IL2Field CardFlyingViewController__labelCtrl;

        static IntPtr LabeledPlayableController_Type;
        static IL2Field LabeledPlayableController_m_PlayLabel;
        static IL2Method LabeledPlayableController_PlayLabel;
        static IL2Method LabeledPlayableController_Create;

        static IL2Method DuelClient_InitTermStep;
        static IL2Field DuelClient_m_DuelEndTransTimeline;

        static IL2Property TimelineObject_playableDirector;

        static IL2Property Sound_Instance;
        static IL2Method Sound_PlayBGM;

        delegate void Del_StopBGM(IntPtr thisPtr, float delay);
        static Hook<Del_StopBGM> hookStopBGM;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;

        static CardFlyingViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("CardFlyingViewController", "YgomGame.Tutorial");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, classInfo.GetMethod("OnCreatedView"));
            CardFlyingViewController__labelCtrl = classInfo.GetField("_labelCtrl");

            LabeledPlayableController_PlayLabel = assembly.GetClass("LabeledPlayableController", "YgomSystem.Timeline").GetMethod("PlayLabel", x => x.GetParameters().Length == 1);

            // Also see DuelClientEndView (handles TransOut?)
            IL2Class DuelClient_Class = assembly.GetClass("DuelClient", "YgomGame.Duel");
            DuelClient_InitTermStep = DuelClient_Class.GetMethod("InitTermStep");
            DuelClient_m_DuelEndTransTimeline = DuelClient_Class.GetField("m_DuelEndTransTimeline");

            IL2Class LabeledPlayableController_Class = assembly.GetClass("LabeledPlayableController", "YgomSystem.Timeline");
            LabeledPlayableController_Type = LabeledPlayableController_Class.IL2Typeof();
            LabeledPlayableController_m_PlayLabel = LabeledPlayableController_Class.GetField("m_PlayLabel");
            LabeledPlayableController_Create = LabeledPlayableController_Class.GetMethod("Create");

            TimelineObject_playableDirector = assembly.GetClass("TimelineObject", "YgomSystem.Timeline").GetProperty("playableDirector");

            IL2Class Sound_Class = assembly.GetClass("Sound", "YgomSystem");
            Sound_Instance = Sound_Class.GetProperty("Instance");
            Sound_PlayBGM = Sound_Class.GetMethod("PlayBGM");
            hookStopBGM = new Hook<Del_StopBGM>(StopBGM, assembly.GetClass("Sound", "YgomSystem").GetMethod("StopBGM"));
        }

        static void StopBGM(IntPtr thisPtr, float fade)
        {
            if (ignoreNextStopBGM)
            {
                ignoreNextStopBGM = false;
                return;
            }
            hookStopBGM.Original(thisPtr, fade);
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            hookOnCreatedView.Original(thisPtr);
            if (IsHacked)
            {
                // See DuelClientUtils.cs EndStep which creates this hacked view controller ("Tutorial/CardFlying")

                duelEndLabeledPlayableController = IntPtr.Zero;
                hackedInstance = thisPtr;
                SoloVisualNovel.Init();

                // Play circle screen mask to reveal the novel UI / flying cards
                IntPtr timeline = DuelClient_m_DuelEndTransTimeline.GetValue(duelClient).ptr;
                IntPtr ptr = TimelineObject_playableDirector.GetGetMethod().Invoke(timeline).ptr;
                IntPtr lpc2 = LabeledPlayableController_Create.Invoke(new IntPtr[] { ptr }).ptr;
                LabeledPlayableController_PlayLabel.Invoke(lpc2, new IntPtr[] { new IL2String("TransOut").ptr });

                // BGM normally only starts on the duel result screen, force it to play now / ignore next stop of the BGM
                float delay = -1f;
                Sound_PlayBGM.Invoke(Sound_Instance.GetGetMethod().Invoke().ptr, new IntPtr[] { new IL2String("BGM_SOLO_GATE").ptr, new IntPtr(&delay) });
                ignoreNextStopBGM = true;
            }
        }

        public static void Update()
        {
            if (!IsHacked && hackedInstance != IntPtr.Zero)
            {
                string label = new IL2String(LabeledPlayableController_m_PlayLabel.GetValue(duelEndLabeledPlayableController).ptr).ToString();
                if (label == "Wait")
                {
                    // TransIn has finished (circle mask covering screen). Remove the flying cards view now that it's covered

                    // TransOut is required otherwise the cards/BG never goes away
                    LabeledPlayableController_PlayLabel.Invoke(CardFlyingViewController__labelCtrl.GetValue(hackedInstance).ptr, new IntPtr[] { new IL2String("TransOut").ptr });
                    IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                    YgomSystem.UI.ViewControllerManager.PopChildViewController(manager, hackedInstance);
                    hackedInstance = IntPtr.Zero;
                }
            }
            if (!IsHacked || hackedInstance == IntPtr.Zero)
            {
                return;
            }
            if (!SoloVisualNovel.Run(hackedInstance, true))
            {
                // Re-trigger the circle screen mask (TransIn/TransOut)
                DuelClient_InitTermStep.Invoke(duelClient);

                // Get the DuelClient's LabeledPlayableController to determine when TransIn ends (mask is covering the screen)
                IntPtr timeline = DuelClient_m_DuelEndTransTimeline.GetValue(duelClient).ptr;
                IntPtr duelEndPlayableDirector = TimelineObject_playableDirector.GetGetMethod().Invoke(timeline).ptr;
                duelEndLabeledPlayableController = GameObject.GetComponent(Component.GetGameObject(duelEndPlayableDirector), LabeledPlayableController_Type);

                IsHacked = false;
            }
        }
    }
}