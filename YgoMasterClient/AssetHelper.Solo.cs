using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using System.IO;

namespace YgoMasterClient
{
    unsafe static partial class AssetHelper
    {
        static Dictionary<int, SoloGateCardEntry> SoloGates = new Dictionary<int, SoloGateCardEntry>();
        static Dictionary<int, string> SoloGateBackgroundPaths = new Dictionary<int, string>();

        // YgomGame.Solo.SoloCardThumbSettings
        static IL2Class soloCardThumbSettingsClassInfo;
        static IL2Field fieldGateMap;
        static IL2Field fieldChapterMap;

        // YgomGame.Solo.SoloCardThumbSettings.SettingMap
        static IL2Class settingMapClassInfo;
        static IL2Field fieldThumbMapSettings;
        static IL2Field fieldThumbMapSettingsMap;

        // YgomGame.Solo.SoloCardThumbSettings.ThumbSetting
        static IL2Class thumbSettingClassInfo;
        static IL2Method thumbSettingCtor;
        static IL2Field fieldThumbSettingId;
        static IL2Field fieldThumbSettingMrk;
        static IL2Field fieldThumbSettingUvRectPos;
        static IL2Field fieldThumbSettingUvRectSize;
        static IL2Field fieldThumbSettingUvRectPosOther;
        static IL2Field fieldThumbSettingUvRectSizeOther;

        // The thumb data only gets loaded once, store the pointer so that we can reload our custom data as needed
        static IntPtr soloThumbsPtr;

        /// <summary>
        /// YgomGame.Solo.SoloCardThumbSettings.ThumbSetting
        /// </summary>
        class SoloGateCardEntry
        {
            public int Id;
            public int CardId;
            public Vector2 UvRectPos;
            public Vector2 UvRectSize;
            public Vector2 UvRectPosOther;
            public Vector2 UvRectSizeOther;
        }

        public static void LoadSoloData()
        {
            SoloGates.Clear();
            SoloGateBackgroundPaths.Clear();

            string backgroundsDir = Path.Combine(Program.ClientDataDir, "SoloGateBackgrounds");
            if (Directory.Exists(backgroundsDir))
            {
                foreach (string backgroundFile in Directory.GetFiles(backgroundsDir))
                {
                    int id;
                    if (int.TryParse(Path.GetFileNameWithoutExtension(backgroundFile), out id))
                    {
                        SoloGateBackgroundPaths[id] = backgroundFile;
                    }
                }
            }

            string file = Path.Combine(Program.ClientDataDir, "SoloGateCards.txt");
            if (File.Exists(file))
            {
                char[] splitChars = { ',', '|' };
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string[] splitted = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (splitted[0].Trim().StartsWith("//"))
                    {
                        continue;
                    }
                    if (splitted.Length >= 4)
                    {
                        SoloGateCardEntry entry = new SoloGateCardEntry();
                        if (int.TryParse(splitted[0], out entry.Id) &&
                            int.TryParse(splitted[1], out entry.CardId))
                        {
                            if (splitted.Length >= 10)
                            {
                                if (float.TryParse(splitted[2], out entry.UvRectPos.x) &&
                                    float.TryParse(splitted[3], out entry.UvRectPos.y) &&
                                    float.TryParse(splitted[4], out entry.UvRectSize.x) &&
                                    float.TryParse(splitted[5], out entry.UvRectSize.y) &&
                                    float.TryParse(splitted[6], out entry.UvRectPosOther.x) &&
                                    float.TryParse(splitted[7], out entry.UvRectPosOther.y) &&
                                    float.TryParse(splitted[8], out entry.UvRectSizeOther.x) &&
                                    float.TryParse(splitted[9], out entry.UvRectSizeOther.y))
                                {
                                    SoloGates[entry.Id] = entry;
                                }
                            }
                            else
                            {
                                if (float.TryParse(splitted[2], out entry.UvRectPos.y) &&
                                    float.TryParse(splitted[3], out entry.UvRectPosOther.y))
                                {
                                    entry.UvRectPos.x = 0;
                                    entry.UvRectPosOther.x = 0;
                                    entry.UvRectSize = new Vector2(1, 1);
                                    entry.UvRectSizeOther = new Vector2(1, 1);
                                    SoloGates[entry.Id] = entry;
                                }
                            }
                        }
                    }
                }
            }
            if (soloThumbsPtr != IntPtr.Zero)
            {
                InjectSoloThumbs(soloThumbsPtr);
            }
        }

        static void InitSolo()
        {
            LoadSoloData();

            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            soloCardThumbSettingsClassInfo = assembly.GetClass("SoloCardThumbSettings");
            fieldGateMap = soloCardThumbSettingsClassInfo.GetField("m_GateMap");
            fieldChapterMap = soloCardThumbSettingsClassInfo.GetField("m_ChapterMap");
            settingMapClassInfo = soloCardThumbSettingsClassInfo.GetNestedType("SettingMap");
            fieldThumbMapSettings = settingMapClassInfo.GetField("m_Settings");
            fieldThumbMapSettingsMap = settingMapClassInfo.GetField("m_SettingsMap");
            thumbSettingClassInfo = soloCardThumbSettingsClassInfo.GetNestedType("ThumbSetting");
            thumbSettingCtor = thumbSettingClassInfo.GetMethod(".ctor");
            fieldThumbSettingId = thumbSettingClassInfo.GetField("id");
            fieldThumbSettingMrk = thumbSettingClassInfo.GetField("mrk");
            fieldThumbSettingUvRectPos = thumbSettingClassInfo.GetField("uvRectPos");
            fieldThumbSettingUvRectSize = thumbSettingClassInfo.GetField("uvRectSize");
            fieldThumbSettingUvRectPosOther = thumbSettingClassInfo.GetField("uvRectPosOther");
            fieldThumbSettingUvRectSizeOther = thumbSettingClassInfo.GetField("uvRectSizeOther");
        }

        static void InjectSoloThumb(IntPtr thumb, SoloGateCardEntry data)
        {
            int mrk = data.CardId;
            Vector2 rectPos = data.UvRectPos;
            Vector2 rectSize = data.UvRectSize;
            Vector2 rectPosOther = data.UvRectPosOther;
            Vector2 rectSizeOther = data.UvRectSizeOther;
            fieldThumbSettingMrk.SetValue(thumb, new IntPtr(&mrk));
            fieldThumbSettingUvRectPos.SetValue(thumb, new IntPtr(&rectPos));
            fieldThumbSettingUvRectSize.SetValue(thumb, new IntPtr(&rectSize));
            fieldThumbSettingUvRectPosOther.SetValue(thumb, new IntPtr(&rectPosOther));
            fieldThumbSettingUvRectSizeOther.SetValue(thumb, new IntPtr(&rectSizeOther));
        }

        static void InjectSoloThumbs(IntPtr obj)
        {
            soloThumbsPtr = obj;
            // NOTE: Not modifying m_ChapterMap as those card images are only used for scenario chapters

            // NOTE: When loading the m_SettingsMap seems to be empty. It's probably filled in later on
            IL2Object gateMapObj = fieldGateMap.GetValue(obj);
            if (gateMapObj != null)
            {
                IL2Object settingsObj = fieldThumbMapSettings.GetValue(gateMapObj.ptr);
                IL2Object settingsMapObj = fieldThumbMapSettingsMap.GetValue(gateMapObj.ptr);
                if (settingsObj != null && settingsMapObj != null)
                {
                    StringBuilder dataDump = null;
                    IL2ListExplicit settingsList = new IL2ListExplicit(settingsObj.ptr, thumbSettingClassInfo);
                    /*IL2DictionaryExplicit settingsMap = new IL2DictionaryExplicit(settingsMapObj.ptr,
                        Assembler.GetAssembly("mscorlib").GetClass(typeof(int).Name, typeof(int).Namespace), thumbSettingClassInfo);*/
                    HashSet<int> seenIds = new HashSet<int>();
                    for (int j = 0; j < settingsList.Count; j++)
                    {
                        IntPtr setting = settingsList[j];
                        int id = fieldThumbSettingId.GetValue(setting).GetValueRef<int>();
                        seenIds.Add(id);
                        SoloGateCardEntry customSetting;
                        if (SoloGates.TryGetValue(id, out customSetting))
                        {
                            InjectSoloThumb(setting, customSetting);
                        }
                        if (ClientSettings.AssetHelperDump)
                        {
                            if (dataDump == null)
                            {
                                dataDump = new StringBuilder();
                            }
                            int mrk = fieldThumbSettingMrk.GetValue(setting).GetValueRef<int>();
                            Vector2 rectPos = fieldThumbSettingUvRectPos.GetValue(setting).GetValueRef<Vector2>();
                            //Vector2 rectSize = fieldThumbSettingUvRectSize.GetValue(setting).GetValueRef<Vector2>();
                            Vector2 rectPosOther = fieldThumbSettingUvRectPosOther.GetValue(setting).GetValueRef<Vector2>();
                            //Vector2 rectSizeOther = fieldThumbSettingUvRectSizeOther.GetValue(setting).GetValueRef<Vector2>();
                            /*dataDump.AppendLine(id + "," + mrk + "," +
                                rectPos.x + "," + rectPos.y + "," +
                                rectSize.x + "," + rectSize.y + "," +
                                rectPosOther.x + "," + rectPosOther.y + "," +
                                rectSizeOther.x + "," + rectSizeOther.y + ",");*/
                            dataDump.AppendLine(id + "," + mrk + "," + rectPos.y + "," + rectPosOther.y);// This is all that's really needed
                        }
                    }
                    if (ClientSettings.AssetHelperDump && dataDump != null)
                    {
                        string dumpPath = Path.Combine(Program.ClientDataDumpDir, "SoloGateCards.txt");
                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(dumpPath));
                        }
                        catch { }
                        try
                        {
                            File.WriteAllText(dumpPath, dataDump.ToString());
                        }
                        catch { }
                    }
                    foreach (SoloGateCardEntry customSetting in SoloGates.Values)
                    {
                        if (!seenIds.Contains(customSetting.Id))
                        {
                            IntPtr newSetting = Import.Object.il2cpp_object_new(thumbSettingClassInfo.ptr);
                            if (newSetting == IntPtr.Zero)
                            {
                                continue;
                            }
                            int id = customSetting.Id;
                            thumbSettingCtor.Invoke(newSetting, new IntPtr[] { new IntPtr(&id) });
                            InjectSoloThumb(newSetting, customSetting);
                            settingsList.Add(newSetting);
                        }
                    }
                }
            }
        }

        static void InjectSoloBackground(IntPtr gameObject, int gateId)
        {
            if (ClientSettings.AssetHelperDump)
            {
                DumpSoloBackground(gameObject, gateId);
            }

            string filePath;
            if (SoloGateBackgroundPaths.TryGetValue(gateId, out filePath) && File.Exists(filePath))
            {
                bool includeInactive = true;
                IL2Object behavior = methodGetComponentInChildren.Invoke(gameObject, new IntPtr[] {
                    monoBehaviourClassInfo.IL2Typeof(), new IntPtr(&includeInactive) });
                if (behavior != null)
                {
                    IntPtr behaviorClass = Import.Object.il2cpp_object_get_class(behavior.ptr);
                    if (behaviorClass == imageClassInfo.ptr)
                    {
                        IntPtr sprite = SpriteFromPNG(filePath, Path.GetFileNameWithoutExtension(filePath));
                        if (sprite != IntPtr.Zero)
                        {
                            methodSetSprite.Invoke(behavior.ptr, new IntPtr[] { sprite });
                        }
                    }
                }
            }
        }

        static void DumpSoloBackground(IntPtr gameObject, int gateId)
        {
            string targetPath = Path.Combine(Program.ClientDataDumpDir, "SoloGateBackgrounds", gateId + ".png");
            if (!File.Exists(targetPath))
            {
                bool includeInactive = true;
                IL2Object behavior = methodGetComponentInChildren.Invoke(gameObject, new IntPtr[] {
                        monoBehaviourClassInfo.IL2Typeof(), new IntPtr(&includeInactive) });
                if (behavior != null)
                {
                    IntPtr behaviorClass = Import.Object.il2cpp_object_get_class(behavior.ptr);
                    if (behaviorClass == imageClassInfo.ptr)
                    {
                        IL2Object sprite = methodGetSprite.Invoke(behavior.ptr);
                        if (sprite != null)
                        {
                            IL2Object texture = methodGetTexture.Invoke(sprite.ptr);
                            if (texture != null)
                            {
                                byte[] buffer = TextureToPNG(texture.ptr);
                                if (buffer != null)
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                                    }
                                    catch { }
                                    try
                                    {
                                        File.WriteAllBytes(targetPath, buffer);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
