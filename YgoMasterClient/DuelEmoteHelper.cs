using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IL2CPP;
using YgoMaster;
using YgoMaster.Net.Message;
using UnityEngine;

namespace YgoMasterClient
{
    unsafe static class DuelEmoteHelper
    {
        static FileInfo fileInfo;
        static DateTime fileInfoLastWrite;
        static Vector3 nearPos;
        static Vector3 farPos;
        static List<string> emotesList = new List<string>();
        static List<string> emotesListWithSounds = new List<string>();
        static IntPtr putNearInstance;
        static IntPtr putFarInstance;
        static bool isNearVisible;
        static bool isFarVisible;
        static DateTime lastShowNear;
        static DateTime lastShowFar;

        delegate void Del_ShowProfileCard(IntPtr parent, IntPtr buttonParent, int player, bool isMyself, bool force, IntPtr profileData);
        static Hook<Del_ShowProfileCard> hookShowProfileCard;

        // PopUpTextManager
        static IntPtr putManagerInstance;
        static IL2Field fieldFreeInstanceQueue;
        static IL2Method methodGetPutInstance;
        delegate IntPtr Del_Create(IntPtr onFinish);
        static Hook<Del_Create> hookCreate;
        delegate void Del_ReturnInstance(IntPtr thisPtr, IntPtr instance);
        static Hook<Del_ReturnInstance> hookReturnInstance;
        delegate void Del_Terminate(IntPtr thisPtr);
        static Hook<Del_Terminate> hookTerminate;

        // PopUpText
        static IL2Method methodShowText;
        static IL2Method methodUpdateText;
        static IL2Field fieldShowing;
        delegate void Del_HideText(IntPtr thisPtr);
        static Hook<Del_HideText> hookHideText;

        // Sound
        static IL2Method methodPlaySE;

        static DuelEmoteHelper()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class utilClassInfo = assembly.GetClass("Util", "YgomGame.Duel");
            hookShowProfileCard = new Hook<Del_ShowProfileCard>(ShowProfileCard, utilClassInfo.GetMethod("ShowProfileCard", x => x.GetParameters().Length == 6));

            IL2Class putManagerClassInfo = assembly.GetClass("PopUpTextManager", "YgomGame");
            fieldFreeInstanceQueue = putManagerClassInfo.GetField("m_FreeInstanceQueue");
            methodGetPutInstance = putManagerClassInfo.GetMethod("GetPutInstance");
            hookCreate = new Hook<Del_Create>(Create, putManagerClassInfo.GetMethod("Create"));
            hookReturnInstance = new Hook<Del_ReturnInstance>(ReturnInstance, putManagerClassInfo.GetMethod("ReturnInstance"));
            hookTerminate = new Hook<Del_Terminate>(Terminate, putManagerClassInfo.GetMethod("Terminate"));

            IL2Class putClassInfo = assembly.GetClass("PopUpText", "YgomGame");
            methodShowText = putClassInfo.GetMethod("ShowText");
            methodUpdateText = putClassInfo.GetMethod("UpdateText");
            fieldShowing = putClassInfo.GetField("m_Showing");
            hookHideText = new Hook<Del_HideText>(HideText, putClassInfo.GetMethod("HideText"));

            IL2Class soundClassInfo = assembly.GetClass("Sound", "YgomGame.Duel");
            methodPlaySE = soundClassInfo.GetMethod("PlaySE");
        }

        static bool TryInit()
        {
            if (putNearInstance != IntPtr.Zero && putFarInstance != IntPtr.Zero)
            {
                return true;
            }
            if (ClientSettings.EmoteDurationInSeconds <= 0 || !DuelDll.HasDuelStart || DuelDll.HasDuelEnd ||
                (!DuelDll.IsPvpDuel && !DuelDll.IsPvpSpectator))
            {
                return false;
            }
            if (fieldFreeInstanceQueue.GetValue(putManagerInstance).ptr == IntPtr.Zero)
            {
                return false;
            }
            putNearInstance = methodGetPutInstance.Invoke(putManagerInstance).ptr;
            putFarInstance = methodGetPutInstance.Invoke(putManagerInstance).ptr;
            return true;
        }

        public static void OnEndDuel()
        {
            if (putNearInstance != IntPtr.Zero)
            {
                HideText(true);
            }
            if (putFarInstance != IntPtr.Zero)
            {
                HideText(false);
            }
            putNearInstance = IntPtr.Zero;
            putFarInstance = IntPtr.Zero;
            isNearVisible = false;
            isFarVisible = false;
        }

        public static void Load(bool emoteListOnly = false)
        {
            try
            {
                bool modified = fileInfo == null || fileInfo.LastWriteTimeUtc > fileInfoLastWrite;

                if (fileInfo == null)
                {
                    fileInfo = new FileInfo(Path.Combine(Program.ClientDataDir, "Text", "Emotes.json"));
                }
                fileInfoLastWrite = fileInfo.LastWriteTimeUtc;

                Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(fileInfo.FullName)) as Dictionary<string, object>;
                List<string> emotes = Utils.GetValueTypeList<string>(data, "EmoteList");
                if (emotes != null)
                {
                    emotesList.Clear();
                    emotesListWithSounds.Clear();
                    emotesListWithSounds.AddRange(emotes);
                    foreach (string str in emotesListWithSounds)
                    {
                        string strWithoutSoundText = str;
                        GetSounds(ref strWithoutSoundText);
                        emotesList.Add(strWithoutSoundText);
                    }
                }
                Dictionary<string, object> nearPosData = Utils.GetDictionary(data, "NearPos");
                if (nearPosData != null)
                {
                    nearPos.x = Utils.GetValue<float>(nearPosData, "X");
                    nearPos.y = Utils.GetValue<float>(nearPosData, "Y");
                    nearPos.z = Utils.GetValue<float>(nearPosData, "Z");
                }
                Dictionary<string, object> farPosData = Utils.GetDictionary(data, "FarPos");
                if (nearPosData != null)
                {
                    farPos.x = Utils.GetValue<float>(farPosData, "X");
                    farPos.y = Utils.GetValue<float>(farPosData, "Y");
                    farPos.z = Utils.GetValue<float>(farPosData, "Z");
                }
                if (emoteListOnly)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Utils.LogWarning("Failed to load emote file. Exception: " + e);
            }
        }

        static IntPtr Create(IntPtr onFinish)
        {
            putManagerInstance = hookCreate.Original(onFinish);
            return putManagerInstance;
        }

        static void Terminate(IntPtr thisPtr)
        {
            OnEndDuel();
            hookTerminate.Original(thisPtr);
        }

        static void ReturnInstance(IntPtr thisPtr, IntPtr instance)
        {
            if (instance == putNearInstance || instance == putFarInstance)
            {
                return;
            }
            hookReturnInstance.Original(thisPtr, instance);
        }

        static void ShowProfileCard(IntPtr parent, IntPtr buttonParent, int player, bool isMyself, bool force, IntPtr profileData)
        {
            if (ClientSettings.EmoteDurationInSeconds > 0 && DuelDll.IsPvpDuel && DuelDll.MyID == player && DuelDll.HasDuelStart && !DuelDll.HasDuelEnd)
            {
                Load(true);
                YgomGame.Menu.ActionSheetViewController.Open(ClientSettings.CustomTextEmotesListHeader, emotesList.ToArray(), OnClickRoomMatchMenuItem);
                return;
            }
            hookShowProfileCard.Original(parent, buttonParent, player, isMyself, force, profileData);
        }

        static Action<IntPtr, int> OnClickRoomMatchMenuItem = OnClickEmoteMenuItemImpl;
        static void OnClickEmoteMenuItemImpl(IntPtr ctx, int index)
        {
            if (!TryInit() || index < 0 || index >= emotesListWithSounds.Count)
            {
                return;
            }
            string emote = emotesListWithSounds[index];
            Program.NetClient.Send(new DuelEmoteMessage()
            {
                Text = emote
            });
        }

        static void HideText(IntPtr thisPtr)
        {
            if (thisPtr == putNearInstance || thisPtr == putFarInstance)
            {
                return;
            }
            hookHideText.Original(thisPtr);
        }

        public static void HideText(bool near)
        {
            if (!TryInit() || putNearInstance == IntPtr.Zero || putFarInstance == IntPtr.Zero)
            {
                return;
            }
            if (near)
            {
                isNearVisible = false;
            }
            else
            {
                isFarVisible = false;
            }
            hookHideText.Original(near ? putNearInstance : putFarInstance);
        }

        public static void ShowText(bool near, string text)
        {
            if (!TryInit() || putNearInstance == IntPtr.Zero || putFarInstance == IntPtr.Zero)
            {
                return;
            }

            Vector3 pos;
            IntPtr putInstance;
            if (near)
            {
                isNearVisible = true;
                lastShowNear = DateTime.UtcNow;
                putInstance = putNearInstance;
                pos = nearPos;
            }
            else
            {
                isFarVisible = true;
                lastShowFar = DateTime.UtcNow;
                putInstance = putFarInstance;
                pos = farPos;
            }

            if (putInstance == IntPtr.Zero)
            {
                return;
            }

            List<string> sounds = GetSounds(ref text);
            if (sounds != null)
            {
                foreach (string sound in sounds)
                {
                    methodPlaySE.Invoke(new IntPtr[] { new IL2String(sound).ptr });
                }
            }

            if (fieldShowing.GetValue(putInstance).GetValueRef<bool>())
            {
                methodUpdateText.Invoke(putInstance, new IntPtr[] { new IL2String(text).ptr });
            }
            else
            {
                IntPtr gameObj = GameObject.New();
                IntPtr transform = GameObject.GetTranform(gameObj);
                UnityEngine.Transform.SetPosition(transform, pos);

                bool isforui = false;
                bool followTarget = false;
                methodShowText.Invoke(putInstance, new IntPtr[] { new IL2String(text).ptr, transform, new IntPtr(&isforui), new IntPtr(&followTarget) });
            }
        }

        static List<string> GetSounds(ref string text)
        {
            List<string> sounds = null;
            int maxSounds = 3;
            int numSounds = 0;
            string soundPrefix = "plsd:";
            int soundIndex;
            while ((soundIndex = text.IndexOf(soundPrefix)) >= 0)
            {
                int len = 0;
                int endIndex = -1;
                for (int i = soundIndex + soundPrefix.Length; i < text.Length; i++)
                {
                    if (char.IsWhiteSpace(text[i]))
                    {
                        endIndex = i;
                        break;
                    }
                    len++;
                }
                string sound = string.Empty;
                if (endIndex >= 0 && endIndex < text.Length)
                {
                    sound = text.Substring(soundIndex + soundPrefix.Length, len);
                    text = text.Substring(0, soundIndex) + text.Substring(endIndex).TrimStart();
                }
                else
                {
                    sound = text.Substring(soundIndex + soundPrefix.Length);
                    text = text.Substring(0, soundIndex);
                }
                if (!string.IsNullOrWhiteSpace(sound) && ++numSounds <= maxSounds)
                {
                    // TODO: Maybe sanitize the sound against known sounds
                    if (sounds == null)
                    {
                        sounds = new List<string>();
                    }
                    sounds.Add(sound);
                }
            }
            return sounds;
        }

        public static void OnDuelEmote(DuelEmoteMessage message)
        {
            if (ClientSettings.EmoteDurationInSeconds <= 0)
            {
                return;
            }
            TradeUtils.AddAction(() =>
            {
                ShowText(message.Near, message.Text);
            });
        }

        public static void Update()
        {
            if (isNearVisible && lastShowNear < DateTime.UtcNow - TimeSpan.FromSeconds(ClientSettings.EmoteDurationInSeconds))
            {
                HideText(true);
            }
            if (isFarVisible && lastShowFar < DateTime.UtcNow - TimeSpan.FromSeconds(ClientSettings.EmoteDurationInSeconds))
            {
                HideText(false);
            }
        }

        public static void OnRunEffect(DuelViewType id, int param1, int param2, int param3)
        {
            // TODO: Send an automated emote based on id/params and a .txt/.json config file. Examples:
            // - Tribute summoning a monster
            // - Summoning specific monster ids (i.e. Dark Magician)
            // - Taking X amounts of damage
            // - Direct attacks
            // - Drawing cards
            // Also allow a % chance to trigger and provide alternative texts / pick a random one from a list
        }
    }
}