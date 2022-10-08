using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace YgoMaster
{
    class DuelSettingsCustomArrayAttribute : Attribute
    {
    }

    // TODO: Maybe just change this code to be Dictionary<string, object> rather than listing all entries (but keep Deck)
    class DuelSettings
    {
#pragma warning disable 0169
#pragma warning disable 0649
        public const int PlayerIndex = 0;
        public const int CpuIndex = 1;
        public const int MaxPlayers = 4;

        public bool IsCustomDuel;

        static string[] ignoreZero = { "life", "hnum" };
        
        // Custom (used for custom duels) - these are 1/2 instead of 0/1 so that 0 can denote defauling to CPU (which is actually 1)
        public int OpponentType;
        public int OpponentPartnerType;

        // Used for injecting duel commands
        [DuelSettingsCustomArray]
        public List<int>[] cmds { get; private set; }

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
        public List<string> bgms { get; private set; }
        public DeckInfo[] Deck { get; private set; }
        public int[] life { get; private set; }
        public int[] hnum { get; private set; }
        public int[] reg { get; private set; }// ?
        public int[] level { get; private set; }
        public int[] follow_num { get; private set; }
        public int[] pcode { get; private set; }
        public int[] rank { get; private set; }
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
        public List<int>[] profile_tag { get; private set; }
        public int[] story_deck_id { get; private set; }
#pragma warning restore 0169
#pragma warning restore 0649

        public DuelSettings()
        {
            FirstPlayer = -1;
            Auto = -1;
            surrender = true;
            MyPartnerType = 1;
            cpu = int.MaxValue;

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

        public void SetRandomBgm(Random rand)
        {
            BgmsFromValue(rand.Next(1, (8 + 1)));
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

            for (int i = 0; i < MaxPlayers; i++)
            {
                if (hnum[i] == -1) hnum[i] = 0;
                if (life[i] == -1) life[i] = 0;

                if (mat[i] == -1) mat[i] = Deck[i].Accessory.Field;
                if (sleeve[i] == -1) sleeve[i] = Deck[i].Accessory.Sleeve;
                if (avatar_home[i] == -1) avatar_home[i] = Deck[i].Accessory.AvBase;
                if (duel_object[i] == -1) duel_object[i] = Deck[i].Accessory.FieldObj;

                if (avatar[i] < 0) avatar[i] = 0;//avatar[i] = 1000001;
                if (mat[i] <= 0) mat[i] = 1090001;
                if (sleeve[i] <= 0) sleeve[i] = 1070001;
                if (icon[i] <= 0) icon[i] = 1010001;
                if (icon_frame[i] <= 0) icon_frame[i] = 1030001;
                if (duel_object[i] <= 0) duel_object[i] = ItemID.GetFieldObjFromField(mat[i]);//duel_object[i] = 1100001;
                if (wallpaper[i] <= 0) wallpaper[i] = 1130001;

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
                    name[i] = isCPU ? "CPU" : "Duelist";
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
                    field.SetValue(this, Convert.ChangeType(obj, field.FieldType));
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
            int firstPlayer;
            if (Utils.TryGetValue(data, "FirstPlayer", out firstPlayer))
            {
                FirstPlayer = firstPlayer;
            }
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
                { "avatar_home", avatar_home }
            };
            return result;
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
    }
}
