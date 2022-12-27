using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using System.Windows.Forms;
using System.IO;
using MiniJSON;
using System.Drawing;
using System.Runtime.InteropServices;

// Required otherwise Fiddler doesn't load the extension
[assembly: Fiddler.RequiredVersion("2.3.5.0")]

namespace YgoMaster
{
    // Copy the output dll into the folder /Fiddler/Inspectors/

    // Roughly based on the following
    // https://github.com/chouex/FiddlerExtensionExample
    // https://github.com/waf/WCF-Binary-Message-Inspector/blob/master/BinaryMessageFiddlerExtension/BinaryResponseInspector.cs

    public class YgoMasterRequestInspector : Inspector2, IRequestInspector2
    {
        RichTextBox textControl;
        private byte[] binaryContent;

        public HTTPRequestHeaders headers
        {
            get { return null; }
            set { }
        }

        public byte[] body
        {
            get { return binaryContent; }
            set { binaryContent = value; }
        }

        public bool bDirty
        {
            get { return false; }
        }

        public bool bReadOnly
        {
            get { return false; }
            set { }
        }

        public override int GetOrder()
        {
            return 0;
        }

        public void Clear()
        {
            textControl.Clear();
        }

        public override void AssignSession(Session oS)
        {
            base.AssignSession(oS);
            if (binaryContent != null)
            {
                bool isPvp = oS.fullUrl.Contains("konami.net") && oS.fullUrl.Contains("/ayk/pvp");
                if (!string.IsNullOrEmpty(oS.oRequest["x_acts"]) || isPvp)
                {
                    string headers = oS.RequestHeaders != null ? oS.RequestHeaders.ToString(true, false, true) : null;
                    YgoMasterInspectorHelper.UpdateControl(textControl, binaryContent, headers, true, isPvp);
                }
                else
                {
                    textControl.Clear();
                }
            }
        }

        public override void AddToTab(TabPage o)
        {
            textControl = YgoMasterInspectorHelper.CreateControl(o);
        }
    }

    public class YgoMasterResponseInspector : Inspector2, IResponseInspector2
    {
        private RichTextBox textControl;
        private byte[] binaryContent;

        public HTTPResponseHeaders headers
        {
            get { return null; }
            set { }
        }

        public byte[] body
        {
            get { return binaryContent; }
            set { binaryContent = value; }
        }

        public bool bDirty
        {
            get { return false; }
        }

        public bool bReadOnly
        {
            get { return false; }
            set { }
        }

        public override int GetOrder()
        {
            return 0;
        }

        public void Clear()
        {
            textControl.Clear();
        }

        public override void AssignSession(Session oS)
        {
            base.AssignSession(oS);
            if (binaryContent != null)
            {
                // NOTE: For master duel changed this from oResponse to oRequest (as response doesnt have it)
                bool isPvp = oS.fullUrl.Contains("konami.net") && oS.fullUrl.Contains("/ayk/pvp");
                if (!string.IsNullOrEmpty(oS.oRequest["x_acts"]) || isPvp)
                {
                    string headers = oS.ResponseHeaders != null ? oS.ResponseHeaders.ToString(true, false) : null;
                    YgoMasterInspectorHelper.UpdateControl(textControl, binaryContent, headers, false, isPvp);
                }
                else
                {
                    textControl.Clear();
                }
            }
        }

        public override void AddToTab(TabPage o)
        {
            textControl = YgoMasterInspectorHelper.CreateControl(o);
        }
    }

    static class YgoMasterInspectorHelper
    {
        const string updateDir = "YgoMasterUpdate";
        
        static YgoMasterInspectorHelper()
        {
            try
            {
                Directory.CreateDirectory(updateDir);
            }
            catch {}
        }
    
        public static RichTextBox CreateControl(TabPage page)
        {
            RichTextBox5 control = new RichTextBox5();
            control.Height = page.Height;
            control.Multiline = true;
            control.WordWrap = false;
            //control.ScrollBars = ScrollBars.Both;
            control.Font = new Font("Lucida Console", Fiddler.CONFIG.flFontSize);
            control.BackColor = Fiddler.CONFIG.colorDisabledEdit;
            control.ReadOnly = true;
            control.WordWrap = false;
            control.MaxLength = 100000000;
            control.Dock = DockStyle.Fill;
            page.Text = "YgoMaster";
            page.Controls.Add(control);
            return control;
        }

        public static void UpdateControl(RichTextBox control, byte[] buffer, string headers, bool request, bool isPvp)
        {
            if (buffer != null)
            {
                string str = string.Empty;
                try
                {
                    if (isPvp)
                    {
                        if (request)
                        {
                            str = DeserializeRequestPvp(buffer);
                        }
                        else
                        {
                            str = DeserializeResponsePvp(buffer);
                        }
                    }
                    else
                    {

                        if (request)
                        {
                            str = DeserializeRequest(buffer);
                        }
                        else
                        {
                            str = DeserializeResponse(buffer);
                        }
                    }
                }
                catch (Exception e)
                {
                    str = e.ToString();
                }

                StringBuilder result = new StringBuilder();
                result.AppendLine(ToHex(buffer));
                result.AppendLine();
                result.AppendLine(headers);
                result.AppendLine();
                result.AppendLine(str);
                control.Text = result.ToString();

                if (Directory.Exists(updateDir))
                {
                    File.AppendAllText(Path.Combine(updateDir, "dllog.txt"), headers.TrimEnd('\r', '\n') + Environment.NewLine + str + Environment.NewLine + Environment.NewLine +
                        (request ? string.Empty : "---------------------------------------" + Environment.NewLine + Environment.NewLine));

                    //File.AppendAllText(Path.Combine(updateDir, "dllog.txt"), headers.TrimEnd('\r', '\n') + Environment.NewLine + str + Environment.NewLine + Environment.NewLine);
                }
            }
            else
            {
                control.Clear();
            }
        }

        private static string DeserializeRequestPvp(byte[] buffer)
        {
            if (buffer.Length > 0)
            {
                Dictionary<string, object> dict = MessagePack.Unpack(buffer) as Dictionary<string, object>;
                if (dict != null)
                {
                    Pvp.Command cmd = (Pvp.Command)0;
                    try
                    {
                        object cmdObj;
                        if (dict.TryGetValue("c", out cmdObj))
                        {
                            cmd = (Pvp.Command)(int)Convert.ChangeType(cmdObj, typeof(int));
                        }
                    }
                    catch
                    {
                    }
                    object bodyObj;
                    if (dict.TryGetValue("b", out bodyObj))
                    {
                        List<object> entries = bodyObj as List<object>;
                        if (entries != null)
                        {
                            List<byte> bodyBuffer = new List<byte>(entries.Select(x => (byte)Convert.ChangeType(x, typeof(byte))));
                            try
                            {
                                //Dictionary<string, object> bodyDict = MessagePack.Unpack(bodyBuffer.ToArray()) as Dictionary<string, object>;
                                object bodyUnpacked = MessagePack.Unpack(bodyBuffer.ToArray());
                                return Json.Serialize(dict) + Environment.NewLine + "cmd:" + cmd + Environment.NewLine + Json.Serialize(bodyUnpacked);
                            }
                            catch
                            {
                            }
                        }
                    }
                    return Json.Serialize(dict) + Environment.NewLine + "cmd:" + cmd;
                }
                else
                {
                    return "Deserialize failed!";
                }
            }
            return string.Empty;
        }

        private static string DeserializeResponsePvp(byte[] buffer)
        {
            // YgomSystem.Network.HTTP.Exec
            List<string> entries = new List<string>();
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                byte[] array = new byte[4];
                int num = 0;
                memoryStream.Read(array, 0, 4);
                num = BitConverter.ToInt32(array, 0);
                if (num > 16777216)
                {
                    return "Error: large packet size " + num;
                }
                else
                {
                    byte[] array2 = null;
                    try
                    {
                        array2 = new byte[num];
                    }
                    catch
                    {
                    }
                    if (array2 != null)
                    {
                        memoryStream.Read(array2, 0, array2.Length);
                        array2 = LZ4.Decompress(array2, 0, 0);
                    }
                    if (array2 == null)
                    {
                        return "Error: failed to decompress. Data len " + num;
                    }
                    using (MemoryStream memoryStream2 = new MemoryStream(array2))
                    {
                        List<KeyValuePair<Pvp.Command, byte[]>> eventList = new List<KeyValuePair<Pvp.Command, byte[]>>();
                        int num3 = memoryStream2.ReadByte();
                        for (int i = 0; i < num3; i++)
                        {
                            int cmd = memoryStream2.ReadByte();
                            memoryStream2.Read(array, 0, 4);
                            num = BitConverter.ToInt32(array, 0);
                            if (num > 1048576)
                            {
                                return "Error: large packet size (inner) " + num;
                            }
                            else
                            {
                                byte[] array3 = new byte[num];
                                if (num > 0)
                                {
                                    memoryStream2.Read(array3, 0, array3.Length);
                                }
                                //entries.Add("cmd:" + (Pvp.Command)cmd + " data:" + (array3.Length > 0 ? ToHex(array3) : "(none)"));
                                eventList.Add(new KeyValuePair<Pvp.Command, byte[]>((Pvp.Command)cmd, array3));
                            }
                        }
                        int receivedSerial = -1;
                        if (array2.Length - memoryStream2.Position >= 4)
                        {
                            memoryStream2.Read(array, 0, 4);
                            receivedSerial = BitConverter.ToInt32(array, 0);
                            entries.Add("serial:" + receivedSerial);
                        }
                        foreach (KeyValuePair<Pvp.Command, byte[]> ev in eventList)
                        {
                            HandleRecvData(ev.Key, ev.Value, entries);
                        }
                    }
                }
            }
            return string.Join(Environment.NewLine, entries);
        }

        public static void HandleRecvData(Pvp.Command cmd, byte[] buffer, List<string> entries)
        {
            entries.Add("cmd:" + cmd + " data:" + (buffer.Length > 0 ? ToHex(buffer) : "(none)"));
            entries.Add("{");
            switch (cmd)
            {
                case Pvp.Command.READY:
                case Pvp.Command.LEAVE:
                case Pvp.Command.RECOVERY:
                case Pvp.Command.TIMEUP:
                case Pvp.Command.FINISH:
                case Pvp.Command.ERROR:
                case Pvp.Command.FATAL:
                    {
                        // no data
                    }
                    break;
                case Pvp.Command.DATA:
                    {
                        Engine.ParseRecvData(buffer, entries);
                    }
                    break;
                case Pvp.Command.REPLAY:
                    {
                        // The data is the raw replay data
                        //Engine.s_instance.replayStream.Add(buffer)
                    }
                    break;
                case Pvp.Command.LATENCY:
                    {
                        Dictionary<string, object> dict = MessagePack.Unpack(buffer) as Dictionary<string, object>;
                        entries.Add("latencyData:" + (dict == null ? "(null)" : MiniJSON.Json.Serialize(dict)));
                        // "l" - latency (array of integers (2 ints))
                    }
                    break;
                case Pvp.Command.WATCH:
                    {
                        Dictionary<string, object> dict = MessagePack.Unpack(buffer) as Dictionary<string, object>;
                        entries.Add("watchData:" + (dict == null ? "(null)" : MiniJSON.Json.Serialize(dict)));
                        // "w" - watchers (integer, number of people watching / spectating)
                    }
                    break;
            }
            entries.Add("}");
        }

        private static string DeserializeRequest(byte[] buffer)
        {
            if (buffer.Length > 4)
            {
                short tokenLength = BitConverter.ToInt16(buffer, 0);
                short dataLength = BitConverter.ToInt16(buffer, 2);
                if (dataLength > 0)
                {
                    return Encoding.UTF8.GetString(buffer, tokenLength + 4, dataLength);
                }
            }
            return string.Empty;
        }

        private static string DeserializeResponse(byte[] buffer)
        {
            if (buffer.Length > 0)
            {
                Dictionary<string, object> dict = DeserializeResponseDictionary(buffer);
                if (dict != null)
                {
                    if (Directory.Exists(updateDir) && dict.ContainsKey("code") && dict.ContainsKey("res"))
                    {
                        Dictionary<string, object> dictTemp = new Dictionary<string, object>(dict);
                        List<object> resList = dictTemp["res"] as List<object>;
                        if (resList != null && resList.Count > 0)
                        {
                            List<object> resData = resList[0] as List<object>;
                            if (resData != null && resData.Count >= 3)
                            {
                                Dictionary<string, object> data = resData[1] as Dictionary<string, object>;
                                if (data != null)
                                {
                                    if (data.ContainsKey("Regulation") && data["Regulation"] as Dictionary<string, object> != null)
                                    {
                                        File.WriteAllText(Path.Combine(updateDir, "RegulationInfo.json"), FormatJson(MiniJSON.Json.Serialize(data["Regulation"])));
                                    }
                                    if (data.ContainsKey("Master") && data["Master"] as Dictionary<string, object> != null)
                                    {
                                        Dictionary<string, object> masterData = data["Master"] as Dictionary<string, object>;
                                        if (masterData.ContainsKey("CardRare") && masterData["CardRare"] as Dictionary<string, object> != null)
                                        {
                                            File.WriteAllText(Path.Combine(updateDir, "CardList.json"), FormatJson(MiniJSON.Json.Serialize(masterData["CardRare"])));
                                        }
                                        if (masterData.ContainsKey("Regulation") && masterData["Regulation"] as Dictionary<string, object> != null)
                                        {
                                            File.WriteAllText(Path.Combine(updateDir, "Regulation.json"), FormatJson(MiniJSON.Json.Serialize(masterData["Regulation"])));
                                        }
                                        if (masterData.ContainsKey("RegulationIcon") && masterData["RegulationIcon"] as Dictionary<string, object> != null)
                                        {
                                            File.WriteAllText(Path.Combine(updateDir, "RegulationIcon.json"), FormatJson(MiniJSON.Json.Serialize(masterData["RegulationIcon"])));
                                        }
                                        if (masterData.ContainsKey("CardCr") && masterData["CardCr"] as List<object> != null)
                                        {
                                            File.WriteAllText(Path.Combine(updateDir, "CardCraftableList.json"), FormatJson(MiniJSON.Json.Serialize(masterData["CardCr"])));
                                        }
                                        if (masterData.ContainsKey("Structure") && masterData["Structure"] as Dictionary<string, object> != null)
                                        {
                                            File.WriteAllText(Path.Combine(updateDir, "StructureDecks.json"), FormatJson(MiniJSON.Json.Serialize(masterData["Structure"])));
                                        }
                                        if (masterData.ContainsKey("Solo") && masterData["Solo"] as Dictionary<string, object> != null)
                                        {
                                            Dictionary<string, object> soloData = masterData["Solo"] as Dictionary<string, object>;
                                            if (soloData.ContainsKey("gate"))
                                            {
                                                data.Remove("Solo");
                                                resData[0] = 0;
                                                File.WriteAllText(Path.Combine(updateDir, "Solo.json"), FormatJson(MiniJSON.Json.Serialize(dictTemp)));
                                            }
                                        }
                                    }
                                    if (data.ContainsKey("Duel") && data["Duel"] as Dictionary<string, object> != null)
                                    {
                                        Dictionary<string, object> duelData = data["Duel"] as Dictionary<string, object>;
                                        if (duelData.ContainsKey("chapter") && (long)duelData["chapter"] > 0)
                                        {
                                            string soloDuelsDir = Path.Combine(updateDir, "SoloDuels");
                                            try
                                            {
                                                Directory.CreateDirectory(soloDuelsDir);
                                            }
                                            catch
                                            {
                                            }
                                            File.WriteAllText(Path.Combine(soloDuelsDir, (long)duelData["chapter"] + ".json"), MiniJSON.Json.Serialize(dictTemp));
                                        }
                                    }
                                    if (data.ContainsKey("Shop") && data["Shop"] as Dictionary<string, object> != null)
                                    {
                                        Dictionary<string, object> shopData = data["Shop"] as Dictionary<string, object>;
                                        if (shopData.ContainsKey("PackShop"))
                                        {
                                            File.WriteAllText(Path.Combine(updateDir, "Shop.json"), MiniJSON.Json.Serialize(dictTemp));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return Json.Serialize(dict);
                }
                else
                {
                    return "Deserialize failed!";
                }
            }
            return string.Empty;
        }

        private static Dictionary<string, object> DeserializeResponseDictionary(byte[] buffer)
        {
            Dictionary<string, object> result = null;
            byte b = buffer[0];
            if (b == 0 || b == 1)
            {
                // This was originally only used for type 0 with a int16 data length
                // v3.4.0 added type 1 to include int32 for length (likely due to issues with large packets causing disconnections)
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    memoryStream.ReadByte();
                    byte[] array2 = new byte[2];
                    byte[] array3 = new byte[4];
                    short num = -1;
                    while (num != 0)
                    {
                        memoryStream.Read(array2, 0, 2);
                        memoryStream.Read(array3, 0, 4);
                        num = BitConverter.ToInt16(array2, 0);
                        int num2 = b == 1 ? BitConverter.ToInt32(array3, 0) : BitConverter.ToInt16(array3, 0);
                        byte[] array4 = new byte[num2];
                        memoryStream.Read(array4, 0, num2);
                        if (num != 0)
                        {
                            if (num != 1)
                            {
                                if (num == 2)
                                {
                                    byte[] buf = LZ4.Decompress(array4, 0, 0);
                                    result = (MessagePack.Unpack(buf) as Dictionary<string, object>);
                                }
                            }
                        }
                    }
                }
            }
            else if (b == 64)
            {
                string str = Encoding.UTF8.GetString(buffer);
                result = (Json.Deserialize(str) as Dictionary<string, object>);
            }
            else
            {
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    memoryStream.ReadByte();
                    byte[] array = new byte[buffer.Length - 1];
                    memoryStream.Read(array, 0, buffer.Length - 1);
                    string str = Encoding.UTF8.GetString(array);
                    result = (Json.Deserialize(str) as Dictionary<string, object>);
                }
            }
            return result;
        }

        public static string ToHex(byte[] buffer)
        {
            StringBuilder hex = new StringBuilder(buffer.Length * 3);
            foreach (byte b in buffer)
            {
                hex.Append(b.ToString("X2") + " ");
            }
            return hex.ToString();
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            int numChars = hex.Length;
            byte[] buffer = new byte[numChars / 2];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return buffer;
        }

        static string FormatJson(string json)
        {
            // https://stackoverflow.com/questions/4580397/json-formatter-in-c/24782322#24782322
            const string INDENT_STRING = "  ";
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : (ch == ':' && quotes % 2 == 0 ? ": " : ch.ToString())
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null ? openChar.Length > 1 ? openChar : closeChar : lineBreak;
            return String.Concat(result);
        }
    }

    class RichTextBox5 : RichTextBox
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                if (LoadLibrary("msftedit.dll") != IntPtr.Zero)
                {
                    createParams.ClassName = "RICHEDIT50W";
                }
                return createParams;
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}
