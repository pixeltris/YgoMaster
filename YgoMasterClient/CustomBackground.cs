using IL2CPP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YgoMasterClient
{
    unsafe static class CustomBackground
    {
        static int baseBgSortingOrder;
        static bool transitionShow;
        static float transitionAlpha;
        static float transitionStartAlpha;
        static float transitionTargetAlpha;
        static IntPtr bg1Transform;
        static IntPtr bg1SpriteRenderer;
        static bool isTransitioning;
        static TimeSpan transitionTime;
        static Stopwatch transitionStopwatch = new Stopwatch();

        const string customBg1Name = "CustomBg1";

        static IL2Class bgManagerClassInfo;
        static IL2Field bgManagerBackRoot;

        static IntPtr spriteRendererType;
        static IL2Method spriteRendererSetSprite;
        static IL2Method spriteRendererSetColor;
        static IL2Method rendererGetSortingOrder;
        static IL2Method rendererSetSortingOrder;

        delegate void Del_Initialize(IntPtr thisPtr);
        static Hook<Del_Initialize> hookInitialize;

        static CustomBackground()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            bgManagerClassInfo = assembly.GetClass("OutGameBGManager", "YgomGame.Menu");
            hookInitialize = new Hook<Del_Initialize>(Initialize, bgManagerClassInfo.GetMethod("Initialize"));

            IL2Assembly coreModuleAssembly = Assembler.GetAssembly("UnityEngine.CoreModule");
            IL2Class rendererClass = coreModuleAssembly.GetClass("Renderer", "UnityEngine");
            rendererGetSortingOrder = rendererClass.GetProperty("sortingOrder").GetGetMethod();
            rendererSetSortingOrder = rendererClass.GetProperty("sortingOrder").GetSetMethod();
            IL2Class spriteRendererClass = coreModuleAssembly.GetClass("SpriteRenderer", "UnityEngine");
            spriteRendererSetColor = spriteRendererClass.GetProperty("color").GetSetMethod();
            spriteRendererSetSprite = spriteRendererClass.GetProperty("sprite").GetSetMethod();
            spriteRendererType = spriteRendererClass.IL2Typeof();

            bgManagerBackRoot = bgManagerClassInfo.GetField("m_BackRoot");
        }

        static void Initialize(IntPtr thisPtr)
        {
            hookInitialize.Original(thisPtr);
            Init();
        }

        public static void Init()
        {
            if (!ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            foreach (IntPtr obj in GetBgObjs())
            {
                foreach (IntPtr child in GameObject.GetChildren(obj))
                {
                    if (UnityObject.GetName(child).Contains("Back0001"))
                    {
                        CreateCustomBgObj(child, customBg1Name);
                        break;
                    }
                }
            }
        }

        static void CreateCustomBgObj(IntPtr targetObj, string name)
        {
            foreach (IntPtr child in GameObject.GetChildren(targetObj))
            {
                if (UnityObject.GetName(child) == name)
                {
                    return;
                }
            }

            if (baseBgSortingOrder == 0)
            {
                IntPtr baseBgRendererComp = GameObject.GetComponent(targetObj, spriteRendererType);
                if (baseBgRendererComp != IntPtr.Zero)
                {
                    baseBgSortingOrder = rendererGetSortingOrder.Invoke(baseBgRendererComp).GetValueRef<int>();
                }
            }

            IntPtr newObj = GameObject.New();
            UnityObject.SetName(newObj, name);
            IntPtr newObjTransform = GameObject.GetTranform(newObj);
            Transform.SetParent(newObjTransform, GameObject.GetTranform(targetObj));
            Transform.SetSiblingIndex(newObjTransform, 0);
            Transform.SetLocalPosition(newObjTransform, new Vector3(0, 0, 0));
            Transform.SetLocalScale(newObjTransform, new Vector3(0.5f, 0.5f, 1));
            IntPtr spriteRenderer = GameObject.AddComponent(newObj, spriteRendererType);
            SetBgBehindParticles(ClientSettings.CustomBackgroundBehindParticles, spriteRenderer);
            SetAlpha(spriteRenderer, 0);

            switch (name)
            {
                case customBg1Name:
                    bg1SpriteRenderer = spriteRenderer;
                    bg1Transform = newObjTransform;
                    if (ClientSettings.CustomBackgroundEnabled)
                    {
                        ReloadBg();
                    }
                    break;
            }
        }

        public static void Show()
        {
            if (!ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            SetBgBehindParticles(ClientSettings.CustomBackgroundBehindParticles);
            transitionShow = true;
            transitionTime = TimeSpan.FromSeconds(ClientSettings.CustomBackgroundFadeShowInSeconds);
            transitionStopwatch.Restart();
            isTransitioning = true;
            transitionTargetAlpha = 1.0f;
            transitionStartAlpha = transitionAlpha;
        }

        public static void Hide()
        {
            if (!ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            transitionShow = false;
            transitionTime = TimeSpan.FromSeconds(ClientSettings.CustomBackgroundFadeHideInSeconds);
            transitionStopwatch.Restart();
            isTransitioning = true;
            transitionTargetAlpha = 0.0f;
            transitionStartAlpha = transitionAlpha;
        }

        public static void HideImmediate()
        {
            if (!ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            SetAlpha(bg1SpriteRenderer, 0);
        }

        public static void Update()
        {
            if (!ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            bool updateAlpha = isTransitioning;
            float baseAlpha = transitionShow ? 1 : 0;
            if (isTransitioning)
            {
                baseAlpha = Lerp(transitionStartAlpha, transitionTargetAlpha, (float)(Math.Min(transitionStopwatch.Elapsed.TotalSeconds, transitionTime.TotalSeconds) / transitionTime.TotalSeconds));
                if (transitionStopwatch.Elapsed >= transitionTime)
                {
                    transitionStopwatch.Reset();
                    isTransitioning = false;
                }
            }
            transitionAlpha = baseAlpha;
            if (updateAlpha)
            {
                SetAlpha(bg1SpriteRenderer, baseAlpha);
            }
        }

        static float Lerp(float start, float end, float pos)
        {
            return start + (end - start) * pos;
        }

        static void SetAlpha(IntPtr spriteRenderer, float alpha)
        {
            if (spriteRenderer == IntPtr.Zero)
            {
                return;
            }
            Color col = new Color(1, 1, 1, alpha);
            spriteRendererSetColor.Invoke(spriteRenderer, new IntPtr[] { new IntPtr(&col) });
        }

        public static void ReloadBg()
        {
            if (!ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            string file = Path.Combine(Program.ClientDataDir, "CustomBackground.png");
            if (!File.Exists(file))
            {
                file = Path.ChangeExtension(file, ".jpg");
            }
            LoadBg(bg1SpriteRenderer, bg1Transform, file);
        }

        static void LoadBg(IntPtr spriteRenderer, IntPtr transform, string path)
        {
            if (spriteRenderer == IntPtr.Zero || transform == IntPtr.Zero || !ClientSettings.CustomBackgroundEnabled)
            {
                return;
            }
            if (File.Exists(path))
            {
                IntPtr sprite = AssetHelper.SpriteFromPNG(path, "bg", default(AssetHelper.Rect), 50);
                if (sprite != IntPtr.Zero)
                {
                    spriteRendererSetSprite.Invoke(spriteRenderer, new IntPtr[] { sprite });

                    int width, height;
                    IntPtr texture = AssetHelper.GetSpriteTexture(sprite);
                    AssetHelper.GetTextureSize(texture, out width, out height);
                    float widthMultiplier = 1920.0f / width;
                    float heightMultiplier = 1080.0f / height;
                    if (width == 1920)
                    {
                        widthMultiplier = 1;
                    }
                    if (height == 1080)
                    {
                        heightMultiplier = 1;
                    }
                    Transform.SetLocalScale(transform, new Vector3(0.5f * widthMultiplier, 0.5f * heightMultiplier, 1));
                }
            }
        }

        public static void SetBgBehindParticles(bool behindParticles)
        {
            SetBgBehindParticles(behindParticles, bg1SpriteRenderer);
        }

        static void SetBgBehindParticles(bool behindParticles, IntPtr spriteRenderer)
        {
            if (!ClientSettings.CustomBackgroundEnabled || spriteRenderer == IntPtr.Zero)
            {
                return;
            }
            int value = behindParticles ? baseBgSortingOrder + 1 : 0;
            rendererSetSortingOrder.Invoke(spriteRenderer, new IntPtr[] { new IntPtr(&value) });
        }

        static List<IntPtr> GetBgObjs()
        {
            IL2Object instance = bgManagerClassInfo.GetField("instance").GetValue();
            return instance != null ? GameObject.GetChildren(bgManagerBackRoot.GetValue(instance.ptr).ptr) : new List<IntPtr>();
        }

        struct Color
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public Color(float r, float g, float b, float a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
        }
    }
}
