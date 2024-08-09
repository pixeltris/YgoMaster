using IL2CPP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YgoMasterClient
{
    /// <summary>
    /// Tweaks to the home UI (HomeViewController / HeaderViewController)
    /// </summary>
    unsafe static class HomeViewTweaks
    {
        delegate void Del_UpdateDispPart(IntPtr thisPtr, int part);
        static Hook<Del_UpdateDispPart> hookUpdateDispPart;

        delegate void Del_UpdateHome(IntPtr thisPtr);
        static Hook<Del_UpdateHome> hookUpdateHome;

        static HomeViewTweaks()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");

            IL2Class headerClass = assembly.GetClass("HeaderViewController", "YgomGame.Menu");
            hookUpdateDispPart = new Hook<Del_UpdateDispPart>(UpdateDispPart, headerClass.GetMethod("UpdateDispPart"));

            IL2Class homeViewClass = assembly.GetClass("HomeViewController", "YgomGame.Menu");
            hookUpdateHome = new Hook<Del_UpdateHome>(UpdateHome, homeViewClass.GetMethod("UpdateHome"));
        }

        static void UpdateDispPart(IntPtr thisPtr, int part)
        {
            hookUpdateDispPart.Original(thisPtr, part);
            if (AssetHelper.IsQuitting)
            {
                return;
            }

            if (ClientSettings.HomeDisableUnusedHeaders)
            {
                IntPtr headerObj = Component.GetGameObject(thisPtr);
                DisableObject(headerObj, "HeaderUI(Clone).Root.RootTop.SafeArea.RootMenu.ButtonDuelPass");
                DisableObject(headerObj, "HeaderUI(Clone).Root.RootTop.SafeArea.RootMenu.ButtonNotice");
                DisableObject(headerObj, "HeaderUI(Clone).Root.RootTop.SafeArea.RootMenu.ButtonPresent");
                DisableObject(headerObj, "HeaderUI(Clone).Root.RootTop.SafeArea.RootMenu.ButtonDuelLive");
                if (string.IsNullOrEmpty(ClientSettings.MultiplayerToken))
                {
                    DisableObject(headerObj, "HeaderUI(Clone).Root.RootTop.SafeArea.RootMenu.ButtonFriend");
                }
            }
        }

        static void UpdateHome(IntPtr thisPtr)
        {
            hookUpdateHome.Original(thisPtr);
            if (AssetHelper.IsQuitting)
            {
                return;
            }

            if (ClientSettings.HomeDisableUnusedTopics || ClientSettings.HomeDisableUnusedBanners)
            {
                IntPtr obj = Component.GetGameObject(thisPtr);
                if (ClientSettings.HomeDisableUnusedTopics)
                {
                    DisableObject(obj, "HomeUI_Console(Clone).Root.SafeAreaTopics.RootTopics.Topics");
                }
                if (ClientSettings.HomeDisableUnusedBanners)
                {
                    DisableObject(obj, "HomeUI_Console(Clone).Root.SafeAreaTopics.RootTopics.MissionBanner");
                    DisableObject(obj, "HomeUI_Console(Clone).Root.SafeAreaMenu.RootMenu.BannerGroup.DuelShortcut");
                }
            }
        }

        static void DisableObject(IntPtr obj, string path)
        {
            IntPtr ptr = GameObject.FindGameObjectByPath(obj, path);
            if (ptr == IntPtr.Zero)
            {
                Console.WriteLine("[HomeViewTweaks] Failed to find '" + path + "'");
                return;
            }
            GameObject.SetActive(ptr, false);
        }
    }
}