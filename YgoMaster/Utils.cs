using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    static class Utils
    {
        static readonly bool disableInfoLogging = false;

        public static string FixIdString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (str.StartsWith("IDS_"))
            {
                return str;
            }
            return "IDS_SYS_IDHACK:" + str;
        }

        public static void LogInfo(string str)
        {
            if (!disableInfoLogging)
            {
                Console.WriteLine(str);
            }
        }

        public static void LogWarning(string str)
        {
            Console.WriteLine("[WARNING] " + str);
        }

        public static long GetEpochTime(DateTime time = default(DateTime))
        {
            time = (time == default(DateTime) ? DateTime.UtcNow : time);
            return (long)(time - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime ConvertEpochTime(long time)
        {
            return (new DateTime(1970, 1, 1)).AddSeconds(time).ToLocalTime();
        }

        public static bool TryCreateDirectory(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static List<int> Shuffle(Random rng, List<int> values)
        {
            List<int> array = new List<int>(values);
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }

        public static Dictionary<int, Dictionary<string, object>> GetIntDictDict(Dictionary<string, object> values, string key)
        {
            Dictionary<int, Dictionary<string, object>> result = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<string, object> dict = GetDictionary(values, key);
            foreach (KeyValuePair<string, object> entry in dict)
            {
                int intKey;
                Dictionary<string, object> dictValue = entry.Value as Dictionary<string, object>;
                if (int.TryParse(entry.Key, out intKey) && intKey > 0 && dictValue != null)
                {
                    result[intKey] = dictValue;
                }
            }
            return result;
        }

        public static Dictionary<string, object> GetDictionary(Dictionary<string, object> values, string key)
        {
            return GetValue(values, key, default(Dictionary<string, object>));
        }

        public static List<int> GetIntList(Dictionary<string, object> values, string key, bool ignoreZero = false)
        {
            List<int> result = new List<int>();
            GetIntList(values, key, result, ignoreZero);
            return result;
        }

        public static void GetIntList(Dictionary<string, object> values, string key, List<int> result, bool ignoreZero = false)
        {
            List<object> items;
            if (TryGetValue(values, key, out items))
            {
                foreach (object item in items)
                {
                    int intVal = (int)Convert.ChangeType(item, typeof(int));
                    if (intVal != 0)
                    {
                        result.Add(intVal);
                    }
                }
            }
        }

        public static void GetIntHashSet(Dictionary<string, object> values, string key, HashSet<int> result, bool ignoreZero = false)
        {
            List<object> items;
            if (TryGetValue(values, key, out items))
            {
                foreach (object item in items)
                {
                    int intVal = (int)Convert.ChangeType(item, typeof(int));
                    if (intVal != 0)
                    {
                        result.Add(intVal);
                    }
                }
            }
        }

        public static T GetValue<T>(Dictionary<string, object> values, string key, T defaultValue = default(T))
        {
            T result;
            if (TryGetValue(values, key, out result))
            {
                return result;
            }
            return defaultValue;
        }

        public static bool TryGetValue<T>(Dictionary<string, object> values, string key, out T result)
        {
            object obj;
            if (values.TryGetValue(key, out obj) && obj != null)
            {
                if (typeof(T).IsValueType)
                {
                    if (obj != null)
                    {
                        try
                        {
                            if (typeof(T).IsEnum)
                            {
                                if (obj is string)
                                {
                                    result = (T)Enum.Parse(typeof(T), obj as string, false);
                                }
                                else
                                {
                                    result = (T)Convert.ChangeType(obj, typeof(T).GetEnumUnderlyingType());
                                }
                            }
                            else
                            {
                                result = (T)Convert.ChangeType(obj, typeof(T));
                            }
                            return true;
                        }
                        catch
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }
                    result = default(T);
                    return false;
                }

                try
                {
                    result = (T)obj;
                    return true;
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            result = default(T);
            return false;
        }

        public static List<Dictionary<string, object>> GetDictionaryCollection(Dictionary<string, object> data, string key)
        {
            // Accepts the following formats:
            /*
             * { "MyData": [
             *      {
             *          { "val1": 0 },
             *          { "val2": 3 },
             *      }
             * ]}
             * 
             * { "MyData": {
             *      { "1": {
             *           { "val1": 0 },
             *           { "val2": 3 },
             *      }
             * }
             */

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            object obj = GetValue<object>(data, key);
            if (obj is List<object>)
            {
                List<object> collectionData = obj as List<object>;
                foreach (object collectionItemData in collectionData)
                {
                    Dictionary<string, object> item = collectionItemData as Dictionary<string, object>;
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
            }
            else if (obj is Dictionary<string, object>)
            {
                Dictionary<string, object> collectionData = obj as Dictionary<string, object>;
                foreach (object collectionItemData in collectionData.Values)
                {
                    Dictionary<string, object> item = collectionItemData as Dictionary<string, object>;
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, object> GetOrCreateDictionary(Dictionary<string, object> data, string name)
        {
            object obj;
            Dictionary<string, object> result;
            if (!data.TryGetValue(name, out obj) || (result = obj as Dictionary<string, object>) == null)
            {
                result = new Dictionary<string, object>();
                data[name] = result;
            }
            return result;
        }

        public static List<object> GetOrCreateList(Dictionary<string, object> data, string name)
        {
            object obj;
            List<object> result;
            if (!data.TryGetValue(name, out obj) || (result = obj as List<object>) == null)
            {
                result = new List<object>();
                data[name] = result;
            }
            return result;
        }

        public static Dictionary<string, object> GetResData(Dictionary<string, object> data)
        {
            if (data != null && data.ContainsKey("code") && data.ContainsKey("res"))
            {
                List<object> resList = GetValue(data, "res", default(List<object>));
                if (resList != null && resList.Count > 0)
                {
                    List<object> resData = resList[0] as List<object>;
                    data = resData[1] as Dictionary<string, object>;
                }
            }
            return data;
        }
    }
}
