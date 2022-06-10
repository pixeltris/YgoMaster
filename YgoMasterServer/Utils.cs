using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace YgoMaster
{
    static class Utils
    {
        static readonly bool disableInfoLogging = false;

        public static string GetDataDirectory(bool isServer, string currentDir = "")
        {
            string dataDir = null;
            try
            {
                string overrideDataDirFile = Path.Combine(currentDir, isServer ? "DataDirServer.txt" : "DataDirClient.txt");
                if (!File.Exists(overrideDataDirFile))
                {
                    overrideDataDirFile = Path.Combine(currentDir, "DataDir.txt");
                }
                if (File.Exists(overrideDataDirFile))
                {
                    string newDir = Path.Combine(currentDir, File.ReadAllLines(overrideDataDirFile)[0].Trim());
                    if (Directory.Exists(newDir))
                    {
                        dataDir = newDir;
                    }
                }
            }
            catch
            {
            }
            if (string.IsNullOrEmpty(dataDir))
            {
                dataDir = Path.Combine(currentDir, "Data");
            }
            return dataDir;
        }

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

        public static string GetInnerText(string html)
        {
            html = html.Trim();
            if ((html.StartsWith("<") && html.EndsWith(">")) || (html.Contains("<a") && html.Contains("</a>")))
            {
                string[] nameEntries = FindAllContentBetween(html, 0, html.Length, ">", "<");
                if (nameEntries.Length > 0)
                {
                    return nameEntries.Last();
                }
                else
                {
                    return string.Empty;
                }
            }
            return html;
        }

        public static string FindFirstContentBetween(string html, int startIndex, int endIndex, string str1, string str2)
        {
            string[] result = FindAllContentBetween(html, startIndex, endIndex, str1, str2, 1);
            return result.Length > 0 ? result[0] : null;
        }

        public static string[] FindAllContentBetween(string html, int startIndex, int endIndex, string str1, string str2)
        {
            return FindAllContentBetween(html, startIndex, endIndex, str1, str2, int.MaxValue);
        }

        public static string[] FindAllContentBetween(string html, int startIndex, int endIndex, string str1, string str2, int limit, bool trimEmpty = false)
        {
            if (startIndex < 0 && endIndex >= 0)
            {
                throw new Exception();
            }
            if (endIndex < 0)
            {
                endIndex = int.MaxValue;
            }

            List<string> result = new List<string>();
            int index = startIndex;
            int foundCount = 0;
            while ((index = html.IndexOf(str1, index)) >= 0 &&
                    index < endIndex)
            {
                int firstItemEndIndex = html.IndexOf(str2, index + str1.Length);
                if (firstItemEndIndex >= 0)
                {
                    int offset = index + str1.Length;
                    string entry = html.Substring(offset, firstItemEndIndex - offset);
                    if (!trimEmpty || !string.IsNullOrEmpty(entry.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim()))
                    {
                        result.Add(entry);
                    }
                }
                index++;
                foundCount++;
                if (foundCount >= limit)
                {
                    break;
                }
            }
            return result.ToArray();
        }

        public static string RemoveAllBracePairs(string str, Dictionary<char, char> bracePairs = null)
        {
            if (bracePairs == null)
            {
                bracePairs = new Dictionary<char, char>() { { '{', '}' }, { '[', ']' }, { '(', ')' } };
            }
            StringBuilder sb = new StringBuilder();
            int braceDepth = 0;
            char currentBraceChar = (char)0;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (currentBraceChar != (char)0)
                {
                    if (c == currentBraceChar)
                    {
                        braceDepth++;
                    }
                    else if (c == bracePairs[currentBraceChar])
                    {
                        braceDepth--;
                        if (braceDepth == 0)
                        {
                            currentBraceChar = (char)0;
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<char, char> bracePair in bracePairs)
                    {
                        if (c == bracePair.Key)
                        {
                            currentBraceChar = c;
                            braceDepth++;
                            break;
                        }
                    }
                    if (braceDepth == 0)
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a list of card names (tolower) and card count from a textual card list
        /// </summary>
        public static Dictionary<string, int> GetCardNamesLowerAndCount(string text)
        {
            Dictionary<string, int> result = new Dictionary<string,int>();
            string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string tempLine = RemoveAllBracePairs(line);
                tempLine = tempLine.Trim(new char[] { ':', '-', '|', ' ', '\t' });
                bool ignoreLine = false;
                bool end = false;
                switch (tempLine.ToLowerInvariant().TrimEnd('s'))
                {
                    case "monster":
                    case "magic":
                    case "spell":
                    case "trap":
                    case "toon monster":
                    case "spirit monster":
                    case "gemini monster":
                    case "union monster":
                    case "xyz monster":
                    case "flip monster":
                    case "non-tribute monster":
                    case "tribute monster":
                    case "ritual monster":
                    case "normal monster":
                    case "effect monster":
                    case "tuner monster":
                    case "dark tuner monster":
                    case "dark synchro monster":
                    case "fusion monster":
                    case "synchro monster":
                    case "pendulum monster":
                    case "monster card":
                    case "spell card":
                    case "magic card":
                    case "trap card":
                    case "extra deck card":
                    case "main deck":
                    case "extra deck":
                    case "fusion deck":
                    case "":
                        ignoreLine = true;
                        break;
                    case "side deck":
                    case "side deck card":
                        end = true;
                        break;
                }
                if (ignoreLine)
                {
                    continue;
                }
                if (end)
                {
                    break;
                }
                int count = 1;
                string name = tempLine.Trim(new char[] { ':', '-', '|', ' ', '\t' }).ToLowerInvariant();
                string strip2 = "\"x\" can not be assigned to a declared";
                if (name.Contains(strip2))
                {
                    name = name.Substring(0, name.IndexOf(strip2)).Trim();
                }
                string[] stripStrings = { "(favorite)", "(d)" };
                foreach (string stripString in stripStrings)
                {
                    if (name.EndsWith(stripString))
                    {
                        name = name.Substring(0, name.Length - stripString.Length).Trim();
                    }
                }
                string[] nameSplitted = name.Split();
                if (nameSplitted.Length > 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        string element = i == 0 ? nameSplitted.First() : nameSplitted.Last();
                        bool hadBrace = element.Contains("(");
                        bool hadX = element.ToLowerInvariant().Count(x => x == 'x') == 1 ||
                            element.ToLowerInvariant().Count(x => x == '*') == 1 ||
                            element.ToLowerInvariant().Count(x => x == '×') == 1;
                        element = element.Replace("(", "").Replace(")", "").Replace("x", "").Replace("*", "").Replace("×", "");
                        int num;
                        if (int.TryParse(element, out num) && num >= 1)
                        {
                            if (hadX || hadBrace)
                            {
                                name = string.Join(" ", nameSplitted, i == 0 ? 1 : 0, nameSplitted.Length - 1);
                                count = num;
                                break;
                            }
                        }
                    }
                }
                name = name.Trim();
                if (!result.ContainsKey(name))
                {
                    result[name] = count;
                }
                else
                {
                    result[name] += count;
                }
            }
            return result;
        }

        public static string DownloadString(string url)
        {
            return DownloadString(url, null);
        }

        public static string DownloadString(string url, string referer, int downloadAttempts = 5, int downloadFailWaitMilliseconds = 200)
        {
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                client.Encoding = Encoding.UTF8;

                if (!string.IsNullOrEmpty(referer))
                {
                    client.Headers[HttpRequestHeader.Referer] = referer;
                }

                for (int i = 0; i < downloadAttempts; i++)
                {
                    try
                    {
                        return client.DownloadString(url);
                    }
                    catch (WebException e)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        if (response.StatusCode == HttpStatusCode.NotFound ||
                            response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        if (downloadFailWaitMilliseconds > 0 && i < downloadAttempts - 1)
                        {
                            System.Threading.Thread.Sleep(downloadFailWaitMilliseconds);
                        }
                    }
                }

                return null;
            }
        }

        public static bool DownloadFile(string url, string filename)
        {
            return DownloadFile(url, null, filename);
        }

        public static bool DownloadFile(string url, string referer, string filename, int downloadAttempts = 5, int downloadFailWaitMilliseconds = 200)
        {
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                client.Encoding = Encoding.UTF8;

                if (!string.IsNullOrEmpty(referer))
                {
                    client.Headers[HttpRequestHeader.Referer] = referer;
                }

                for (int i = 0; i < downloadAttempts; i++)
                {
                    try
                    {
                        client.DownloadFile(url, filename);
                        return true;
                    }
                    catch (WebException e)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        if (response != null &&
                            (response.StatusCode == HttpStatusCode.NotFound ||
                            response.StatusCode == HttpStatusCode.Forbidden))
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        if (downloadFailWaitMilliseconds > 0 && i < downloadAttempts - 1)
                        {
                            System.Threading.Thread.Sleep(downloadFailWaitMilliseconds);
                        }
                    }
                }

                return false;
            }
        }
    }
}
