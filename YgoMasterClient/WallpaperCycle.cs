using IL2CPP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using YgoMaster;

namespace YgoMasterClient
{
    /// <summary>
    /// Cycles between all owned wallpapers
    /// </summary>
    unsafe static class WallpaperCycle
    {
        static Stopwatch stopwatch = new Stopwatch();
        static Stopwatch fadeStopwatch = new Stopwatch();
        static TimeSpan extraTime;

        static IL2Field fieldCurrentWallpaperGo;
        static IL2Method methodTargetPlayLabel;

        delegate void Del_Update(IntPtr thisPtr);
        static Hook<Del_Update> hookUpdate;

        delegate void Del_UpdateWallpaper(IntPtr thisPtr);
        static Hook<Del_UpdateWallpaper> hookUpdateWallpaper;

        delegate void Del_OnTransitionStart(IntPtr thisPtr, int transitionType);
        static Hook<Del_OnTransitionStart> hookOnTransitionStart;

        delegate int Del_GetUserWallpaper(int defaultValue);
        static Hook<Del_GetUserWallpaper> hookGetUserWallpaper;

        delegate void Del_NotificationStackRemove(IntPtr thisPtr);
        static Hook<Del_NotificationStackRemove> hookNotificationStackRemove;

        static List<int> ownedWallpaperIds = new List<int>();
        static bool hasObtainedWallpaperIds;
        static int currentWallpaperIndex;
        static int currentWallpaperId;
        static bool visible;
        static IntPtr lastInstance;

        public static bool Enabled
        {
            get { return ClientSettings.WallpaperCycleEnabled; }
            set
            {
                if (value)
                {
                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Restart();
                        fadeStopwatch.Reset();
                    }
                }
                ClientSettings.WallpaperCycleEnabled = value;
            }
        }

        static WallpaperCycle()
        {
            stopwatch.Start();

            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("HomeViewController", "YgomGame.Menu");
            hookUpdate = new Hook<Del_Update>(Update, classInfo.GetMethod("Update"));
            hookOnTransitionStart = new Hook<Del_OnTransitionStart>(OnTransitionStart, classInfo.GetMethod("OnTransitionStart"));
            hookNotificationStackRemove = new Hook<Del_NotificationStackRemove>(NotificationStackRemove, classInfo.GetMethod("NotificationStackRemove"));
            hookUpdateWallpaper = new Hook<Del_UpdateWallpaper>(UpdateWallpaper, classInfo.GetMethod("UpdateWallpaper"));
            fieldCurrentWallpaperGo = classInfo.GetField("currentWallpaperGo");

            IL2Class clientWorkUtilClassInfo = assembly.GetClass("ClientWorkUtil", "YgomSystem.Utility");
            hookGetUserWallpaper = new Hook<Del_GetUserWallpaper>(GetUserWallpaper, clientWorkUtilClassInfo.GetMethod("GetUserWallpaper"));

            IL2Class tweenClassInfo = assembly.GetClass("Tween", "YgomSystem.UI");
            methodTargetPlayLabel = tweenClassInfo.GetMethod("TargetPlayLabel");
        }

        // NOTE: We have 3 hooks which are called per frame (here, TradeUtils, Win32Hooks)
        static void Update(IntPtr thisPtr)
        {
            hookUpdate.Original(thisPtr);

            if (AssetHelper.IsQuitting)
            {
                return;
            }

            CustomBackground.Update();

            if (lastInstance != thisPtr)
            {
                lastInstance = thisPtr;
                stopwatch.Restart();
                fadeStopwatch.Reset();
                UpdateOwnedWallpaperIds();
                if (ClientSettings.WallpaperCycleOnEveryHomeVisit)
                {
                    UpdateWallpaperId();
                }
            }

            if (!visible || !ClientSettings.WallpaperCycleEnabled || ClientSettings.WallpaperCycleDelayInSeconds == 0 || ClientSettings.WallpaperDisabled)
            {
                return;
            }

            if (stopwatch.Elapsed.TotalSeconds - extraTime.TotalSeconds > ClientSettings.WallpaperCycleDelayInSeconds)
            {
                extraTime = TimeSpan.Zero;
                stopwatch.Reset();
                IL2Object gameObject = fieldCurrentWallpaperGo.GetValue(thisPtr);
                if (gameObject != null)
                {
                    PlayTween(gameObject.ptr, "Pop");
                    fadeStopwatch.Restart();
                }
            }
            if (fadeStopwatch.Elapsed.TotalSeconds > ClientSettings.WallpaperCycleFadeWaitInSeconds)
            {
                fadeStopwatch.Reset();
                stopwatch.Restart();
                UpdateWallpaperId();
                hookUpdateWallpaper.Original(thisPtr);

                IL2Object gameObject = fieldCurrentWallpaperGo.GetValue(thisPtr);
                if (gameObject != null)
                {
                    PlayTween(gameObject.ptr, "Push");
                }
            }
        }

        static void PlayTween(IntPtr gameObject, string label)
        {
            IntPtr parent = Component.GetGameObject(Transform.GetParent(Transform.GetParent(GameObject.GetTransform(gameObject))));
            bool includeChildren = false;
            bool wakeup = false;
            methodTargetPlayLabel.Invoke(new IntPtr[] { parent, new IL2String(label).ptr, new IntPtr(&includeChildren), new IntPtr(&wakeup) });
        }

        /// <summary>
        /// YgomSystem.UI.ViewController.TransitionType
        /// </summary>
        enum TransitionType
        {
            Push,
            Pop,
            Cover,
            Uncover,
            SwapIn,
            SwapOut,
            Max
        }

        static void OnTransitionStart(IntPtr thisPtr, int type)
        {
            if (ClientSettings.WallpaperCycleOnEveryHomeVisit)
            {
                stopwatch.Restart();
            }
            fadeStopwatch.Reset();
            switch ((TransitionType)type)
            {
                case TransitionType.Pop:
                case TransitionType.SwapOut:
                case TransitionType.Cover:
                    visible = false;
                    CustomBackground.Hide();
                    break;
                default:
                    CustomBackground.Show();
                    UpdateOwnedWallpaperIds();
                    visible = true;
                    if (!ClientSettings.WallpaperCycleOnEveryHomeVisit)
                    {
                        if (stopwatch.Elapsed.TotalSeconds > ClientSettings.WallpaperCycleDelayInSeconds)
                        {
                            UpdateWallpaperId();
                        }
                        else
                        {
                            extraTime = TimeSpan.FromSeconds(5);
                        }
                    }
                    break;
            }
            hookOnTransitionStart.Original(thisPtr, type);
        }

        static void NotificationStackRemove(IntPtr thisPtr)
        {
            CustomBackground.HideImmediate();
            hookNotificationStackRemove.Original(thisPtr);
        }

        static void UpdateOwnedWallpaperIds()
        {
            if (!ClientSettings.WallpaperCycleEnabled)
            {
                return;
            }
            hasObtainedWallpaperIds = true;
            bool hasNewIds = false;
            List<int> updatedIds = new List<int>();
            Dictionary<string, object> items = YgomSystem.Utility.ClientWork.GetDict("Item.have");
            foreach (KeyValuePair<string, object> item in items)
            {
                int itemId;
                if (int.TryParse(item.Key, out itemId) && ItemID.GetCategoryFromID(itemId) == ItemID.Category.WALLPAPER)
                {
                    updatedIds.Add(itemId);
                    if (!ownedWallpaperIds.Contains(itemId))
                    {
                        hasNewIds = true;
                    }
                }
            }
            if (hasNewIds)
            {
                ownedWallpaperIds = Utils.Shuffle(Utils.Rand, updatedIds);
            }
        }

        static void UpdateWallpaperId()
        {
            if (!AssetHelper.IsQuitting && ClientSettings.WallpaperCycleEnabled && ClientSettings.WallpaperCycleDelayInSeconds > 0 && !ClientSettings.WallpaperDisabled)
            {
                if (!hasObtainedWallpaperIds || ownedWallpaperIds.Count == 0)
                {
                    UpdateOwnedWallpaperIds();
                }
                if (ownedWallpaperIds.Count > 0)
                {
                    currentWallpaperIndex++;
                    if (currentWallpaperIndex >= ownedWallpaperIds.Count)
                    {
                        currentWallpaperIndex = 0;
                        ownedWallpaperIds = Utils.Shuffle(Utils.Rand, ownedWallpaperIds);
                        if (ownedWallpaperIds[0] == currentWallpaperId)
                        {
                            ownedWallpaperIds.RemoveAt(0);
                            ownedWallpaperIds.Add(currentWallpaperId);
                        }
                    }
                    currentWallpaperId = ownedWallpaperIds[currentWallpaperIndex];
                }
            }
        }

        static void UpdateWallpaper(IntPtr thisPtr)
        {
            if (ClientSettings.WallpaperCycleOnEveryHomeVisit)
            {
                UpdateWallpaperId();
            }
            hookUpdateWallpaper.Original(thisPtr);
        }

        static int GetUserWallpaper(int defaultValue)
        {
            if (ClientSettings.WallpaperDisabled)
            {
                return 0;
            }
            if (Enabled)
            {
                if (currentWallpaperId == 0)
                {
                    UpdateWallpaperId();
                }
                return currentWallpaperId != 0 ? currentWallpaperId : hookGetUserWallpaper.Original(defaultValue);
            }
            else
            {
                return hookGetUserWallpaper.Original(defaultValue);
            }
        }
    }
}
