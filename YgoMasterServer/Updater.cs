using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;
using System.IO;

namespace YgoMaster
{
    // NOTE: Unfinished as this requires getting tokens prior to first User.entry / User.home which is annoying. Also Craft.get_card_route will take forever

    /// <summary>
    /// Obtains the following:
    /// - User.home - Master.CardRare (CardList.json)
    /// - User.home - Master.CardCr (CardCraftableList.json)
    /// - User.home - Master.Regulation (CardBanList.json)
    /// - User.home - Master.Structure (StructureDecks.json)
    /// - Solo.info (/Data/SoloDuels/) - Completes all solo duels sequentially using the loaner deck (or user deck if not available)
    /// - Shop.get_list (Shop.json)
    /// - Craft.get_card_route (Shop.json - secret packs)
    /// </summary>
    class Updater
    {
        Dictionary<int, int> cardRare;

        public Updater(Dictionary<int, int> cardRare)
        {
            this.cardRare = cardRare;
        }

        public void Run(string b64Token, string aToken)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;//Tls12

            //Send(b64Token, aToken, "User.entry", null);
            //Dictionary<string, object> response = Send(b64Token, aToken, "User.home", null);
            //if (response == null)
            //{
            //    Console.WriteLine("User.home is null");
            //    return;
            //}
            //Dictionary<string, object> masterData = Utils.GetDictionary(response, "Master");
            //if (masterData == null)
            //{
            //    Console.WriteLine("Master is null");
            //    return;
            //}
            //Console.WriteLine(MiniJSON.Json.Serialize(masterData));
            //Dictionary<string, object> cardRareData = Utils.GetDictionary(masterData, "CardRare");
            //if (cardRareData == null)
            //{
            //    Console.WriteLine("CardRare is null");
            //    return;
            //}
            //Console.WriteLine(MiniJSON.Json.Serialize(cardRareData));

            int cardIndex = 0;
            foreach (int cid in cardRare.Keys)
            {
                Dictionary<string, object> cardRouteData = Send(b64Token, aToken, "Craft.get_card_route", new Dictionary<string, object>()
                    {
                        { "card_id", cid }
                    });
                Console.WriteLine("CardRoute " + ++cardIndex + " / " + cardRare.Keys.Count);
                //Console.WriteLine(MiniJSON.Json.Serialize(cardRouteData));
                //Console.WriteLine(cid);
            }

            //Console.WriteLine(response);
            //System.Threading.Thread.Sleep(3000);
            //response = Send(b64Token, aToken, "EventNotify.get_list", null);
            //Console.WriteLine(MiniJSON.Json.Serialize(response));
        }

        Dictionary<string, object> Send(string b64Token, string aToken, string actName, Dictionary<string, object> actParams, RequestType requestType = RequestType.AykApi)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            List<object> acts = new List<object>();
            Dictionary<string, object> act = new Dictionary<string,object>();
            act["act"] = actName;
            act["id"] = 0;
            if (actParams != null)
            {
                act["params"] = actParams;
            }
            acts.Add(act);
            request["acts"] = acts;
            request["v"] = "1.1.1";
            request["ua"] = "Windows.Steam/6.1.0/All Series (ASUS)";
            request["h"] = 558161692;// TODO: Change this on each request? IIRC it's a crc of mouse movement / input
            return Send(b64Token, aToken, MiniJSON.Json.Serialize(request), requestType);
        }

        Dictionary<string, object> Send(string b64Token, string aToken, string request, RequestType requestType = RequestType.AykApi)
        {
            string url = null;
            switch (requestType)
            {
                case RequestType.AykApi:
                    url = "https://ayk-web.mo.konami.net/ayk/api/";
                    break;
            }
            if (string.IsNullOrEmpty(url))
            {
                Utils.LogWarning("Invalid url for request type " + requestType);
                return null;
            }
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                wc.Headers["atoken"] = aToken;
                List<string> acts = new List<string>();
                Dictionary<string, object> data = MiniJSON.Json.Deserialize(request) as Dictionary<string, object>;
                if (data != null)
                {
                    List<object> actsList;
                    if (Utils.TryGetValue(data, "acts", out actsList))
                    {
                        foreach (object actObj in actsList)
                        {
                            Dictionary<string, object> actData = actObj as Dictionary<string, object>;
                            string act;
                            if (actData != null && Utils.TryGetValue(actData, "act", out act))
                            {
                                acts.Add(act);
                            }
                        }
                    }
                }
                if (acts.Count != 1)
                {
                    Utils.LogWarning("Expected 1 act. Got " + acts.Count);
                    return null;
                }
                wc.Headers["x_acts"] = string.Join(",", acts);
                wc.Headers["X-Unity-Version"] = "2020.3.32f1";// TODO: Obtain this from an asset
                wc.Headers["User-Agent"] = "UnityPlayer/2020.3.32f1 (UnityWebRequest/1.0, libcurl/7.80.0-DEV)";
                url += acts[0];
                Console.WriteLine(url);
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        byte[] tokenData = Convert.FromBase64String(b64Token);
                        byte[] requestData = Encoding.UTF8.GetBytes(request);
                        bw.Write((ushort)tokenData.Length);
                        bw.Write((ushort)requestData.Length);
                        bw.Write(tokenData);
                        bw.Write(requestData);
                        byte[] buffer = wc.UploadData(url, ms.ToArray());
                        if (buffer.Length > 0)
                        {
                            Dictionary<string, object> dict = DeserializeResponseDictionary(buffer);
                            if (dict != null)
                            {
                                return Utils.GetResData(dict);
                            }
                        }
                        return null;
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            Console.WriteLine(reader.ReadToEnd());
                        }
                    }
                    throw;
                }
            }
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
                result = (MiniJSON.Json.Deserialize(str) as Dictionary<string, object>);
            }
            else
            {
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    memoryStream.ReadByte();
                    byte[] array = new byte[buffer.Length - 1];
                    memoryStream.Read(array, 0, buffer.Length - 1);
                    string str = Encoding.UTF8.GetString(array);
                    result = (MiniJSON.Json.Deserialize(str) as Dictionary<string, object>);
                }
            }
            return result;
        }

        enum RequestType
        {
            AykApi
        }
    }
}
