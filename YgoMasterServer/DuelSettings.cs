using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace YgoMaster
{
    class DuelSettingsCustomArrayAttribute : Attribute
    {
    }

    // TODO: Maybe just change this code to be Dictionary<string, object> rather than listing all entries (but keep Deck)
    class DuelSettings
    {
        public static string DefaultNamePlayer = "Duelist";
        public static string DefaultNameCPU = "CPU";
        public static Dictionary<int, List<string>> MatIdToBgmId = new Dictionary<int, List<string>>();

#pragma warning disable 0169
#pragma warning disable 0649
        public const int PlayerIndex = 0;
        public const int CpuIndex = 1;
        public const int MaxPlayers = 4;
        public const string ExpectedDuelDataKey = "icon";
        public const int MaxBgmId = 16;// Update based on the largest number shown in the custom duel starter UI

        public bool IsCustomDuel;

        public bool HasSavedReplay;
        public long DuelBeginTime;// Time of Duel.begin
        public long DuelEndTime;// Time of Duel.end

        // Custom (used for custom duels) - these are 1/2 instead of 0/1 so that 0 can denote defauling to CPU (which is actually 1)
        public int OpponentType;
        public int OpponentPartnerType;

        // Custom (used to prevent certain BGMs from playing)
        public bool SingleBgm;
        public bool DoubleBgm;
        public bool NoKeycardBgm;
        public bool OverrideUserBgm;

        // Normally not stored here (it comes from Solo.detail) - this links to IDS_DECKRECIPE
        public int npc_deck_id;

        // Used for injecting duel commands
        [DuelSettingsCustomArray]
        public List<int>[] cmds { get; private set; }

        // Used by custom duel starter to select random field / synced parts
        public int SharedField;

        public List<string> PreserveP1ForLoanerDeck { get; private set; }
        public List<string> PreserveP1ForMyDeck { get; private set; }

        // Used by SoloVisualNovel.cs (LinkEvolution dialogs)
        public string dialog_intro;
        public string dialog_outro;

        static string[] ignoreZero = { "life", "hnum" };

        // Use the same names as in the packet (using reflection here to reduce the amount of manual work)
        public uint RandSeed;
        public int FirstPlayer;
        public bool noshuffle;
        public bool tag;
        public bool dlginfo;
        public int MyID;
        public int MyType;
        public int Type;
        public int MyPartnerType;
        public int PlayableTagPartner;
        public int regulation_id;
        public long duel_start_timestamp;
        public bool surrender;
        public int Limit;
        public int GameMode;// GameMode
        public int cpu;
        public string cpuflag;// None,Def,Fool,Light,MyTurnOnly,AttackOnly,Simple (YgomGame.Duel.Engine.CpuParam)
        public int LeftTimeMax;
        public int TurnTimeMax;
        public int TotalTimeMax;
        public int Auto;
        public bool rec;
        public bool recf;
        public long did;
        public string duel;
        public bool is_pvp;
        public int chapter;
        public string replaym;
        public bool view_mydeck;//v2.1.0
        public List<string> bgms { get; private set; }
        public DeckInfo[] Deck { get; private set; }
        public int[] life { get; private set; }
        public int[] hnum { get; private set; }
        public int[] reg { get; private set; }// ?
        public int[] level { get; private set; }
        public int[] follow_num { get; private set; }
        public int[] follower_num { get; private set; }
        public int[] pcode { get; private set; }
        public int[] rank { get; private set; }
        public int[] rate { get; private set; }
        public int[] DuelistLv { get; private set; }
        public string[] name { get; private set; }
        public int[] avatar { get; private set; }
        public int[] avatar_home { get; private set; }
        public int[] icon { get; private set; }
        public int[] icon_frame { get; private set; }
        public int[] sleeve { get; private set; }
        public int[] mat { get; private set; }
        public int[] duel_object { get; private set; }
        public int[] wallpaper { get; private set; }
        public int[] coin { get; private set; }
        public List<int>[] profile_tag { get; private set; }
        public int[] story_deck_id { get; private set; }

        // Replay settings
        public int res;
        public int finish;
        public int turn;
        public int MaxTurn;
        public bool open;// Is public / private duel replay
        public int PvpChoice;// 1.9.0 - Shows which player won the coin flip (NOTE: Name is actually "Choice" but we only want it on relays / spectator duels)
        public int[] xuid { get; private set; }
        public string[] online_id { get; private set; }
        public bool[] is_same_os { get; private set; }
        public bool IsReplayLocked;//v2.1.0 - we can't call this "lock" due to C#'s lock keyword (see fixup in GetFieldName)
        public List<int> tags { get; private set; }//v2.1.0
#pragma warning restore 0169
#pragma warning restore 0649

        public DuelSettings()
        {
            FirstPlayer = -1;
            Auto = -1;
            surrender = true;
            MyPartnerType = 1;
            cpu = int.MaxValue;
            view_mydeck = true;
            PvpChoice = -1;

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (property.PropertyType.IsArray)
                {
                    if (!IsCustomArray(property))
                    {
                        property.SetValue(this, Array.CreateInstance(property.PropertyType.GetElementType(), MaxPlayers), null);
                        if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType.GetElementType()))
                        {
                            Array array = property.GetValue(this, null) as Array;
                            for (int i = 0; i < MaxPlayers; i++)
                            {
                                array.SetValue(Activator.CreateInstance(property.PropertyType.GetElementType()), i);
                            }
                        }
                    }
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType))
                {
                    property.SetValue(this, Activator.CreateInstance(property.PropertyType), null);
                }
            }

            Deck = new DeckInfo[MaxPlayers];
            for (int i = 0; i < Deck.Length; i++)
            {
                Deck[i] = new DeckInfo();
            }
        }

        public void SetP1Name(bool isMyDeck, string value)
        {
            List<string> preserve = isMyDeck ? PreserveP1ForMyDeck : PreserveP1ForLoanerDeck;
            if (preserve != null && preserve.Contains("NAME"))
            {
                return;
            }
            name[PlayerIndex] = value;
        }

        public void SetP1ItemValue(bool isMyDeck, ItemID.Category itemCategory, int value)
        {
            List<string> preserve = isMyDeck ? PreserveP1ForMyDeck : PreserveP1ForLoanerDeck;
            if (preserve != null && preserve.Contains(itemCategory.ToString()))
            {
                return;
            }
            // TODO: Support DECK_CASE (this involves modifying the Deck accessory entries)
            switch (itemCategory)
            {
                case ItemID.Category.AVATAR: avatar[PlayerIndex] = value; break;
                case ItemID.Category.ICON: icon[PlayerIndex] = value; break;
                case ItemID.Category.ICON_FRAME: icon_frame[PlayerIndex] = value; break;
                case ItemID.Category.PROTECTOR: sleeve[PlayerIndex] = value; break;
                case ItemID.Category.FIELD: mat[PlayerIndex] = value; break;
                case ItemID.Category.FIELD_OBJ: duel_object[PlayerIndex] = value; break;
                case ItemID.Category.AVATAR_HOME: avatar_home[PlayerIndex] = value; break;
                case ItemID.Category.WALLPAPER: wallpaper[PlayerIndex] = value; break;
                case ItemID.Category.COIN: coin[PlayerIndex] = value; break;
            }
        }

        public int GetBgmValue()
        {
            if (bgms != null && bgms.Count > 0)
            {
                int value;
                int.TryParse(bgms[0].Trim().Split('_').Last(), out value);
                return value;
            }
            return 0;
        }

        public void BgmsFromValue(int value)
        {
            bgms.Clear();
            if (value > 0)
            {
                bgms.Add("BGM_DUEL_NORMAL_" + value.ToString().PadLeft(2, '0'));
                bgms.Add("BGM_DUEL_KEYCARD_" + value.ToString().PadLeft(2, '0'));
                bgms.Add("BGM_DUEL_CLIMAX_" + value.ToString().PadLeft(2, '0'));
            }
        }

        public void SetRandomBgm()
        {
            BgmsFromValue(Utils.Rand.Next(1, (MaxBgmId + 1)));
        }

        public void SetBgm(DuelBgmMode bgmMode)
        {
            int myMatId = MyID == 0 ? mat[0] : mat[1];
            int rivalMatId = MyID == 0 ? mat[1] : mat[0];
            List<string> targetBgms;
            if (MatIdToBgmId.TryGetValue(bgmMode == DuelBgmMode.Myself ? myMatId : rivalMatId, out targetBgms) && targetBgms.Count == 3)
            {
                bgms.Clear();
                bgms.AddRange(targetBgms);
            }
        }

        public void SetBgm(DuelBgmMode bgmMode, int index)
        {
            int myMatId = MyID == 0 ? mat[0] : mat[1];
            int rivalMatId = MyID == 0 ? mat[1] : mat[0];
            List<string> targetBgms;
            if (MatIdToBgmId.TryGetValue(bgmMode == DuelBgmMode.Myself ? myMatId : rivalMatId, out targetBgms) &&
                targetBgms.Count == 3 && bgms.Count > index && targetBgms.Count > index)
            {
                bgms[index] = targetBgms[index];
            }
        }

        public static int GetRandomBgmValue()
        {
            return Utils.Rand.Next(1, (MaxBgmId + 1));
        }

        public void LoadRandomDecks()
        {
            for (int i = 0; i < Deck.Length; i++)
            {
                if (Deck[i].IsRandomDeckPath)
                {
                    Deck[i].Load();
                }
            }
        }

        public void ClearRandomDeckPaths(bool setCpuNameFromDeckName)
        {
            for (int i = 0; i < Deck.Length; i++)
            {
                if (Deck[i].IsRandomDeckPath)
                {
                    if (setCpuNameFromDeckName && i > 0 && !string.IsNullOrEmpty(Deck[i].Name))
                    {
                        name[i] = Deck[i].Name;
                    }
                    Deck[i].File = null;
                }
            }
        }

        public void SetRequiredDefaults()
        {
            // cpu int.MaxValue is used by DuelStarter to state the default value should be used
            // TODO: Maybe set 100 to the default in DuelStarter and go from 100 to -100 instead?
            if (cpu == int.MaxValue)
            {
                cpu = 100;
            }
            if (Type == 2)// tag duel (they are bugged, the decks don't load resulting in an instant victory / defeat)
            {
                tag = true;
            }

            if (SharedField != -1 && SharedField != 0)
            {
                int field = SharedField;
                if (field == -2)
                {
                    field = ItemID.GetRandomId(Utils.Rand, ItemID.Category.FIELD);
                }
                int fieldPart = ItemID.GetFieldObjFromField(field);
                for (int i = 0; i < MaxPlayers; i++)
                {
                    mat[i] = field;
                    if (duel_object[i] <= 0)
                    {
                        duel_object[i] = fieldPart;
                    }
                }
#if YGO_MASTER_CLIENT
                //Console.WriteLine("Random field: " + field);
#endif
            }

            for (int i = 0; i < MaxPlayers; i++)
            {
                if (hnum[i] == -1) hnum[i] = 0;
                if (life[i] == -1) life[i] = 0;

                if (avatar[i] == -3) avatar[i] = 0;
                if (avatar_home[i] == -3) avatar_home[i] = 0;

                if (avatar[i] == -2) avatar[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.AVATAR);
                if (mat[i] == -2) mat[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.FIELD);
                if (sleeve[i] == -2) sleeve[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.PROTECTOR);
                if (avatar_home[i] == -2) avatar_home[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.AVATAR_HOME);
                if (duel_object[i] == -2) duel_object[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.FIELD_OBJ);
                if (icon[i] == -2) icon[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.ICON);
                if (icon_frame[i] == -2) icon_frame[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.ICON_FRAME);
                if (coin[i] == -2) coin[i] = ItemID.GetRandomId(Utils.Rand, ItemID.Category.COIN);

                if (mat[i] == -1) mat[i] = Deck[i].Accessory.Field;
                if (sleeve[i] == -1) sleeve[i] = Deck[i].Accessory.Sleeve;
                if (avatar_home[i] == -1) avatar_home[i] = Deck[i].Accessory.AvBase;
                if (duel_object[i] == -1) duel_object[i] = Deck[i].Accessory.FieldObj;
                if (avatar[i] == -1) avatar[i] = Deck[i].Accessory.AvatarId;
                if (coin[i] == -1) coin[i] = Deck[i].Accessory.Coin;

                if (avatar[i] < 0) avatar[i] = 0;//avatar[i] = 1000001;
                if (mat[i] <= 0) mat[i] = (int)ItemID.Value.DefaultField;
                if (sleeve[i] <= 0) sleeve[i] = (int)ItemID.Value.DefaultProtector;
                if (icon[i] <= 0) icon[i] = (int)ItemID.Value.DefaultIcon;
                if (icon_frame[i] <= 0) icon_frame[i] = (int)ItemID.Value.DefaultIconFrame;
                if (duel_object[i] <= 0) duel_object[i] = ItemID.GetFieldObjFromField(mat[i]);//duel_object[i] = 1100001;
                if (wallpaper[i] <= 0) wallpaper[i] = (int)ItemID.Value.DefaultWallpaper;
                if (coin[i] <= 0) coin[i] = (int)ItemID.Value.DefaultCoin;

                if (i > 0 && string.IsNullOrEmpty(name[i]))
                {
                    bool isCPU = false;
                    switch (i)
                    {
                        case 0: isCPU = MyType == 1; break;
                        case 1: isCPU = OpponentType == 0 || OpponentType == 2; break;
                        case 2: isCPU = MyPartnerType == 1; break;
                        case 3: isCPU = OpponentPartnerType == 0 || OpponentPartnerType == 2; break;
                    }
                    name[i] = isCPU ? DefaultNameCPU : DefaultNamePlayer;
                }
            }

            /*if (Bgms.Count == 0)
            {
                Bgms.Add("BGM_DUEL_NORMAL_03");
                Bgms.Add("BGM_DUEL_KEYCARD_03");
                Bgms.Add("BGM_DUEL_CLIMAX_03");
            }*/
        }

        public void CopyDecksFrom(DuelSettings other)
        {
            for (int i = 0; i < Deck.Length && i < other.Deck.Length; i++)
            {
                Deck[i].CopyFrom(other.Deck[i]);
            }
        }

        public void CopyFrom(DuelSettings other)
        {
            foreach (FieldInfo field in GetType().GetFields())
            {
                if (field.IsStatic)
                {
                    continue;
                }
                field.SetValue(this, field.GetValue(other));
            }
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (property.PropertyType.IsArray)
                {
                    if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType.GetElementType()))
                    {
                        Array srcArray = property.GetValue(other, null) as Array;
                        Array dstArray = property.GetValue(this, null) as Array;
                        if (IsCustomArray(property))
                        {
                            if (srcArray == null)
                            {
                                property.SetValue(this, null, null);
                                continue;
                            }
                            else if (dstArray == null || dstArray.Length != srcArray.Length)
                            {
                                property.SetValue(this, dstArray = Array.CreateInstance(property.PropertyType.GetElementType(), srcArray.Length), null);
                                for (int i = 0; i < srcArray.Length; i++)
                                {
                                    dstArray.SetValue(Activator.CreateInstance(property.PropertyType.GetElementType()), i);
                                }
                            }
                        }
                        for (int i = 0; i < srcArray.Length && i < dstArray.Length; i++)
                        {
                            System.Collections.IList srcList = srcArray.GetValue(i) as System.Collections.IList;
                            System.Collections.IList dstList = dstArray.GetValue(i) as System.Collections.IList;
                            if (srcList != null && dstList != null)
                            {
                                dstList.Clear();
                                for (int j = 0; j < srcList.Count; j++)
                                {
                                    dstList.Add(srcList[j]);
                                }
                            }
                        }
                    }
                    else if (property.PropertyType.GetElementType() != typeof(DeckInfo))
                    {
                        Array srcArray = property.GetValue(other, null) as Array;
                        Array dstArray = property.GetValue(this, null) as Array;
                        srcArray.CopyTo(dstArray, 0);
                    }
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType))
                {
                    System.Collections.IList srcList = (System.Collections.IList)property.GetValue(other, null);
                    System.Collections.IList dstList = (System.Collections.IList)property.GetValue(this, null);
                    dstList.Clear();
                    for (int i = 0; i < srcList.Count; i++)
                    {
                        dstList.Add(srcList[i]);
                    }
                }
                else
                {
                    Utils.LogWarning("Failed to copy duel settings property " + property.Name);
                }
            }
            for (int i = 0; i < Deck.Length && i < other.Deck.Length; i++)
            {
                //Deck[i] = other.Deck[i];
                Deck[i].CopyFrom(other.Deck[i]);
            }
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            foreach (FieldInfo field in GetType().GetFields())
            {
                if (field.IsStatic)
                {
                    continue;
                }
                object obj;
                if (data.TryGetValue(field.Name, out obj))
                {
                    object value = obj == null && field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : Convert.ChangeType(obj, field.FieldType);
                    field.SetValue(this, value);
                }
            }

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                object obj;
                if (!data.TryGetValue(property.Name, out obj))
                {
                    if (property.PropertyType.IsArray && IsCustomArray(property))
                    {
                        property.SetValue(this, null, null);
                    }
                    continue;
                }
                List<object> objList = obj as List<object>;
                if (objList == null)
                {
                    if (property.PropertyType.IsArray && IsCustomArray(property))
                    {
                        property.SetValue(this, null, null);
                    }
                    continue;
                }
                if (property.PropertyType.IsArray)
                {
                    if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType.GetElementType()))
                    {
                        Type elementType = property.PropertyType.GetElementType().GetGenericArguments()[0];
                        Array dstArray = property.GetValue(this, null) as Array;
                        if (IsCustomArray(property))
                        {
                            property.SetValue(this, dstArray = Array.CreateInstance(property.PropertyType.GetElementType(), objList.Count), null);
                        }
                        for (int i = 0; i < objList.Count && i < dstArray.Length; i++)
                        {
                            System.Collections.IList dstList = dstArray.GetValue(i) as System.Collections.IList;
                            if (dstList == null)
                            {
                                dstArray.SetValue(dstList = Activator.CreateInstance(property.PropertyType.GetElementType()) as System.Collections.IList, i);
                            }
                            dstList.Clear();
                            List<object> innerList = objList[i] as List<object>;
                            if (innerList != null)
                            {
                                for (int j = 0; j < innerList.Count; j++)
                                {
                                    dstList.Add(Convert.ChangeType(innerList[j], elementType));
                                }
                            }
                        }
                    }
                    else if (property.PropertyType.GetElementType() == typeof(DeckInfo))
                    {
                        for (int i = 0; i < objList.Count && i < Deck.Length; i++)
                        {
                            // Changed to Ex for additional info for the duel starter (additional info not used by client)
                            Deck[i].FromDictionaryEx(objList[i] as Dictionary<string, object>, true);
                            //Deck[i].FromDictionary(objList[i] as Dictionary<string, object>, true);
                        }
                    }
                    else
                    {
                        Type elementType = property.PropertyType.GetElementType();
                        Array dstArray = property.GetValue(this, null) as Array;
                        if (objList.Count > 0 && objList[0] is Dictionary<string, object> && property.Name == "rank")
                        {
                            continue;
                        }
                        for (int i = 0; i < objList.Count && i < dstArray.Length; i++)
                        {
                            dstArray.SetValue(Convert.ChangeType(objList[i], elementType), i);
                        }
                    }
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType))
                {
                    Type elementType = property.PropertyType.GetGenericArguments()[0];
                    System.Collections.IList dstList = (System.Collections.IList)property.GetValue(this, null);
                    dstList.Clear();
                    for (int i = 0; i < objList.Count; i++)
                    {
                        dstList.Add(Convert.ChangeType(objList[i], elementType));
                    }
                }
            }

            // NOTE: This will always be set when logging solo duel packets. Change it based on "Solo.detail"
            FirstPlayer = Utils.GetValue(data, "FirstPlayer", FirstPlayer);

            if (DuelBeginTime == 0 && duel_start_timestamp != 0)
            {
                DuelBeginTime = duel_start_timestamp;
            }
            if (duel_start_timestamp == 0)
            {
                duel_start_timestamp = DuelBeginTime;
            }
            if (turn != 0)
            {
                MaxTurn = turn;
            }
            else if (MaxTurn != 0)
            {
                turn = MaxTurn;
            }

            if (data.ContainsKey("PickCards"))
            {
                List<object> pickCards = data["PickCards"] as List<object>;
                if (pickCards != null && pickCards.Count >= 2)
                {
                    Deck[0].DisplayCards.FromIndexedDictionary(pickCards[0] as Dictionary<string, object>);
                    Deck[1].DisplayCards.FromIndexedDictionary(pickCards[1] as Dictionary<string, object>);
                }
            }

            PvpChoice = Utils.GetValue(data, "choice", PvpChoice);
            PvpChoice = Utils.GetValue(data, "Choice", PvpChoice);
            FirstPlayer = Utils.GetValue(data, "first_player", FirstPlayer);
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (FieldInfo field in GetType().GetFields())
            {
                if (field.IsStatic)
                {
                    continue;
                }
                result[field.Name] = field.GetValue(this);
            }
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (ignoreZero.Contains(property.Name))
                {
                    bool isZero = true;
                    Array array = property.GetValue(this, null) as Array;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if ((int)array.GetValue(i) != 0)
                        {
                            isZero = false;
                            break;
                        }
                    }
                    if (isZero)
                    {
                        continue;
                    }
                }
                if (property.PropertyType.IsArray)
                {
                    if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType.GetElementType()))
                    {
                        List<object> objListList = new List<object>();
                        Type elementType = property.PropertyType.GetElementType().GetGenericArguments()[0];
                        Array array = property.GetValue(this, null) as Array;
                        if (array != null)
                        {
                            bool isCmds = property.Name == "cmds";
                            List<object> acsl1 = null;
                            List<object> acsl2 = null;
                            if (isCmds)
                            {
                                List<object> acsl = isCmds ? Utils.GetOrCreateList(result, "acsl") : null;
                                acsl.Add(acsl1 = new List<object>());
                                acsl.Add(acsl2 = new List<object>());
                            }
                            int len = IsCustomArray(property) ? array.Length : Math.Min(array.Length, MaxPlayers);
                            for (int i = 0; i < len; i++)
                            {
                                List<object> objList = new List<object>();
                                System.Collections.IList list = array.GetValue(i) as System.Collections.IList;
                                if (list != null)
                                {
                                    for (int j = 0; j < list.Count; j++)
                                    {
                                        objList.Add(list[j]);
                                    }
                                }
                                if (isCmds && objList.Count >= 7)
                                {
                                    int cmd = (int)Convert.ChangeType(objList[0], typeof(int));
                                    if (cmd == 0)
                                    {
                                        int player = (int)Convert.ChangeType(objList[1], typeof(int));
                                        int pos = (int)Convert.ChangeType(objList[2], typeof(int));
                                        //int unk = (int)Convert.ChangeType(objList[3], typeof(int));
                                        int cid = (int)Convert.ChangeType(objList[4], typeof(int));
                                        int prm = (int)Convert.ChangeType(objList[5], typeof(int));
                                        int df = (int)Convert.ChangeType(objList[6], typeof(int));
                                        List<object> acslArray = player == 0 ? acsl1 : acsl2;
                                        acslArray.Add(new Dictionary<string, object>()
                                        {
                                            {  "cid", cid },
                                            {  "pos", pos },
                                            {  "prm", prm },
                                            {  "df", df },
                                        });
                                    }
                                }
                                objListList.Add(objList);
                            }
                            result[property.Name] = objListList;
                        }
                    }
                    else if (property.PropertyType.GetElementType() == typeof(DeckInfo))
                    {
                        List<object> objList = new List<object>();
                        for (int i = 0; i < Deck.Length && i < MaxPlayers; i++)
                        {
                            // Changed to Ex for additional info for the duel starter (additional info not used by client)
                            objList.Add(Deck[i].ToDictionaryEx(true));
                            //objList.Add(Deck[i].ToDictionary(true));
                        }
                        result[property.Name] = objList;
                    }
                    else
                    {
                        List<object> objList = new List<object>();
                        Type elementType = property.PropertyType.GetElementType();
                        Array array = property.GetValue(this, null) as Array;
                        int len = IsCustomArray(property) ? array.Length : Math.Min(array.Length, MaxPlayers);
                        for (int i = 0; i < len; i++)
                        {
                            objList.Add(array.GetValue(i));
                        }
                        result[property.Name] = objList;
                    }
                }
                else if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType))
                {
                    List<object> objList = new List<object>();
                    Type elementType = property.PropertyType.GetGenericArguments()[0];
                    System.Collections.IList list = (System.Collections.IList)property.GetValue(this, null);
                    for (int i = 0; i < list.Count; i++)
                    {
                        objList.Add(list[i]);
                    }
                    result[property.Name] = objList;
                }
            }
            return result;
        }

        private bool IsCustomArray(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(DuelSettingsCustomArrayAttribute), false).Length > 0;
        }

        public Dictionary<string, object> ToDictionaryForSoloStart()
        {
            Dictionary<string, object> result = new Dictionary<string, object>()
            {
                { "avatar", avatar },
                { "icon", icon },
                { "icon_frame", icon_frame },
                { "sleeve", sleeve },
                { "mat", mat },
                { "duel_object", duel_object },
                { "avatar_home", avatar_home },
                { "coin", coin },
                { "dialog_intro", dialog_intro },
                { "dialog_outro", dialog_outro }
            };
            return result;
        }

#if YGO_MASTER_CLIENT
        public Dictionary<string, object> ToDictionaryForReplayList()
#else
        public Dictionary<string, object> ToDictionaryForReplayList(Player[] players)
#endif
        {
            Dictionary<string, object> replayData = new Dictionary<string, object>();
            replayData["mode"] = GameMode == 0 ? (int)YgoMaster.GameMode.SoloSingle : GameMode;
            replayData["did"] = did;
            replayData["time"] = DuelBeginTime > 0 ? DuelBeginTime : duel_start_timestamp;
            replayData["myid"] = MyID;
            replayData["deck"] = new object[]
            {
                Deck[0].ToDictionary(true),
                Deck[1].ToDictionary(true)
            };
            replayData["icon"] = icon;
            replayData["icon_frame"] = icon_frame;
            replayData["avatar"] = avatar;
            replayData["res"] = res;
            replayData["turn"] = turn;
            replayData["finish"] = finish;

            List<Dictionary<string, object>> allPlayerProfileData = new List<Dictionary<string, object>>();
            replayData["player"] = allPlayerProfileData;
            for (int i = 0; i < 2; i++)
            {
                Dictionary<string, object> playerProfileData = new Dictionary<string, object>();
                allPlayerProfileData.Add(playerProfileData);
#if !YGO_MASTER_CLIENT
                Player player = players.Length < i ? players[i] : null;
                if (player != null)
                {
                    playerProfileData["pcode"] = player.Code;
                    playerProfileData["name"] = player.Name;
                    playerProfileData["cid"] = 0;//?
                    playerProfileData["mat"] = mat[i];
                    playerProfileData["sleeve"] = sleeve[i];
                    playerProfileData["avatar_home"] = avatar_home[i];
                    playerProfileData["duel_object"] = duel_object[i];
                    playerProfileData["deck_case"] = Deck[i].Accessory.Box;
                    playerProfileData["level"] = player.Level;
                    playerProfileData["wallpaper"] = player.Wallpaper;
                    playerProfileData["profile_tag"] = player.TitleTags.ToArray();
                    playerProfileData["follow_num"] = player.Friends.Count(x => x.Value.HasFlag(FriendState.Following));
                    playerProfileData["follower_num"] = player.Friends.Count(x => x.Value.HasFlag(FriendState.Follower));
                    playerProfileData["country"] = 0;
                    playerProfileData["rank_str"] = null;//?
                    playerProfileData["rank_sub"] = 0;//?
                    playerProfileData["rank_id"] = 0;//?
                    playerProfileData["rank"] = Math.Max(1, player.Rank);
                    playerProfileData["lv"] = null;//?
                    playerProfileData["rate"] = Math.Max(1, player.Rate);
                    playerProfileData["os"] = (int)PlatformID.Steam;
                    playerProfileData["is_same_os"] = null;
                    playerProfileData["online_id"] = null;
                    playerProfileData["icon"] = player.IconId;
                    playerProfileData["icon_frame"] = player.IconFrameId;
                    playerProfileData["avatar"] = player.AvatarId;
                    playerProfileData["coin"] = Deck[i].Accessory.Coin;
                }
                else
                {
#endif
                    playerProfileData["pcode"] = pcode[i];
                    playerProfileData["name"] = name[i];
                    playerProfileData["cid"] = 0;//?
                    playerProfileData["mat"] = mat[i];
                    playerProfileData["sleeve"] = sleeve[i];
                    playerProfileData["avatar_home"] = avatar_home[i];
                    playerProfileData["duel_object"] = duel_object[i];
                    playerProfileData["deck_case"] = 0;
                    playerProfileData["level"] = level[i];
                    playerProfileData["wallpaper"] = wallpaper[i];
                    playerProfileData["profile_tag"] = profile_tag[i];
                    playerProfileData["follow_num"] = follow_num[i];
                    playerProfileData["follower_num"] = follower_num[i];
                    playerProfileData["country"] = 0;
                    playerProfileData["rank_str"] = null;//?
                    playerProfileData["rank_sub"] = 0;//?
                    playerProfileData["rank_id"] = 0;//?
                    playerProfileData["rank"] = Math.Max(1, rank[i]);
                    playerProfileData["lv"] = null;//?
                    playerProfileData["rate"] = Math.Max(1, rate[i]);
                    playerProfileData["os"] = (int)PlatformID.Steam;
                    playerProfileData["is_same_os"] = null;
                    playerProfileData["online_id"] = null;
                    playerProfileData["icon"] = icon[i];
                    playerProfileData["icon"] = icon[i];
                    playerProfileData["icon_frame"] = icon_frame[i];
                    playerProfileData["avatar"] = avatar[i];
                    playerProfileData["coin"] = coin[i];
#if !YGO_MASTER_CLIENT
                }
#endif
            }

            replayData["room_id"] = 0;
            replayData["invalid"] = false;
            replayData["open"] = open;
            replayData["season_id"] = 1;
            replayData["date"] = Utils.FormatDateTime(Utils.ConvertEpochTime(DuelBeginTime));

            replayData["first_player"] = FirstPlayer;
            if (PvpChoice >= 0 && GameMode > 0 && GameMode != (int)YgoMaster.GameMode.SoloSingle)
            {
                replayData["choice"] = PvpChoice;
            }
            replayData["lock"] = IsReplayLocked;
            replayData["tags"] = tags;
            replayData["pcard"] = new List<object>()
            {
                Deck[0].DisplayCards.ToIndexDictionary(),
                Deck[1].DisplayCards.ToIndexDictionary()
            };

            return replayData;
        }

        public static Dictionary<string, object> FixupReplayRequirements(DuelSettings duelSettings, Dictionary<string, object> data)
        {
            data["view_mydeck"] = false;
            data["rank"] = new List<object>()
            {
                new Dictionary<string, object>()
                {
                    { "rank", duelSettings.rank[0] },
                    { "rate", duelSettings.rate[0] }
                },
                new Dictionary<string, object>()
                {
                    { "rank", duelSettings.rank[1] },
                    { "rate", duelSettings.rate[1] }
                }
            };
            if (!data.ContainsKey("xuid")) data["xuid"] = new int[2];
            if (!data.ContainsKey("online_id")) data["online_id"] = new int[2];
            if (!data.ContainsKey("is_same_os")) data["is_same_os"] = new bool[2];
            if (duelSettings.PvpChoice >= 0)
            {
                data["Choice"] = duelSettings.PvpChoice;
            }
            else
            {
                data["Choice"] = null;
            }
            return data;
        }

        public bool AreAllEqual(int[] values)
        {
            if (values.Length <= 0)
            {
                return true;
            }
            int firstVal = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] != firstVal)
                {
                    return false;
                }
            }
            return true;
        }

        public static void LoadBgmInfo(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            MatIdToBgmId.Clear();
            List<object> entries = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as List<object>;
            foreach (object entry in entries)
            {
                KeyValuePair<string, object> data = (entry as Dictionary<string, object>).First();
                int matId;
                if (int.TryParse(data.Key, out matId))
                {
                    List<string> bgms = new List<string>();
                    if (data.Value is List<object>)
                    {
                        List<object> bgmList = data.Value as List<object>;
                        foreach (object bgm in bgmList)
                        {
                            bgms.Add((string)bgm);
                        }

                    }
                    else
                    {
                        int bgm = (int)Convert.ChangeType(data.Value, typeof(int));
                        bgms.Add("BGM_DUEL_NORMAL_" + bgm.ToString().PadLeft(2, '0'));
                        bgms.Add("BGM_DUEL_KEYCARD_" + bgm.ToString().PadLeft(2, '0'));
                        bgms.Add("BGM_DUEL_CLIMAX_" + bgm.ToString().PadLeft(2, '0'));
                    }
                    MatIdToBgmId[matId] = bgms;
                }
            }
        }
    }
}
