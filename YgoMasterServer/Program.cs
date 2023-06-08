using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace YgoMaster
{
    class Program
    {
        /*static byte[] b1(byte[] input)
        {
            using (var compressStream = new MemoryStream())
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress, true))
                {
                    inputStream.CopyTo(compressor);
                }
                compressStream.Flush();
                byte[] compressed = compressStream.ToArray();
                byte[] result = new byte[compressed.Length + 2];
                result[0] = 0x78;
                result[1] = 0x9C;
                Buffer.BlockCopy(compressed, 0, result, 2, compressed.Length);
                return result;
            }
        }*/

        static void LogLog(string file)
        {
            Func<byte[], byte[]> decompress = (byte[] buffer) =>
            {
                //Console.WriteLine(BitConverter.ToString(buffer));
                using (MemoryStream outputStream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(outputStream))
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    System.IO.Compression.DeflateStream zip = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);
                    int totalRead = 0;
                    int read = 0;
                    zip.BaseStream.Position += 2;
                    byte[] temp = new byte[65535];
                    while ((read = zip.Read(temp, 0, 1000)) > 0)
                    {
                        totalRead += read;
                        writer.Write(temp, 0, read);
                    }
                    return outputStream.ToArray();
                }
            };

            Dictionary<string, object> dat = MiniJSON.Json.Deserialize(File.ReadAllText(file)) as Dictionary<string, object>;
            string str = dat["replaym"] as string;

            byte[] buffer1 = Convert.FromBase64String(str);
            Dictionary<string, object> dictionary = MessagePack.Unpack(buffer1) as Dictionary<string, object>;
            byte[] b = (dictionary["b"] as byte[]);
            var v1 = decompress(b);
            //byte[] recompressed = b1(v1);
            Debug.WriteLine(file);
            for (int i = 0; i < v1.Length; i += 8)
            {
                Debug.WriteLine(BitConverter.ToString(v1, i, 8).Replace("-", " "));
            }
            Debug.WriteLine("");
        }

        static int Main(string[] args)
        {
            //LogLog(@"Data\Players\415899035\Replays\1686252553.json");

            //LogLog(@"Data\Players\415899035\Replays\1686253110.json");
            
            //LogLog(@"Data\Players\415899035\Replays\1686253704.json");
            //LogLog(@"Data\Players\415899035\Replays\1686253704.json");

            //Func<byte[], byte[]> decompress = (byte[] buffer) =>
            //{
            //    //Console.WriteLine(BitConverter.ToString(buffer));
            //    using (MemoryStream outputStream = new MemoryStream())
            //    using (BinaryWriter writer = new BinaryWriter(outputStream))
            //    using (MemoryStream ms = new MemoryStream(buffer))
            //    {
            //        System.IO.Compression.DeflateStream zip = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);
            //        int totalRead = 0;
            //        int read = 0;
            //        zip.BaseStream.Position += 2;
            //        byte[] temp = new byte[65535];
            //        while ((read = zip.Read(temp, 0, 1000)) > 0)
            //        {
            //            totalRead += read;
            //            writer.Write(temp, 0, read);
            //        }
            //        return outputStream.ToArray();
            //    }
            //};
            //
            //
            //int[] fin = new int[]
            //    {
            //        4,
            //        4
            //    };
            //Dictionary<string, object> replayData = new Dictionary<string, object>();
            //replayData["b"] = new byte[] { 1, 2, 3, 4, 5, 6, 7, };//Utils.ZLibCompress(new byte[] { 1, 2, 3, 4, 5, 6, 7, });
            //replayData["f"] = fin;
            //string str1 = Convert.ToBase64String(MessagePack.Pack(replayData));
            //
            //byte[] buffer1 = Convert.FromBase64String("gqFixH94nFNlcJB3kGdkCG/4/5+VgYEhnAFCMzJAAFMDhOaC0mYMvIyMIowM3FA+D5TObGAC00ENXdqSDqYMug08bF3apgwCDTwMpkDTBJHoLm3G/8JAtV3aDAwWQD4ICEHlNYG0MZAG2bPJAEwzdTgwMsLMV2pgBdOsQDUgFgAPtBhToWaSBAQ=");
            //Dictionary<string, object> dictionary = MessagePack.Unpack(buffer1) as Dictionary<string, object>;
            //byte[] b = (dictionary["b"] as byte[]);
            ////var v111 = LZ4.Decompress(b, 0, 0);
            ////var v1 = Zlib.Decompress(b);
            //var v1 = decompress(b);
            //byte[] recompressed = b1(v1);
            //for (int i = 0; i < v1.Length; i += 8)
            //{
            //    Console.WriteLine(BitConverter.ToString(v1, i, 8).Replace("-", " "));
            //}
            //
            //
            ////Debug.WriteLine(BitConverter.ToString(buffer1).Replace("-", " "));
            //
            ////byte[] buffer2 = Convert.FromBase64String("gqFi3ANYJQBAH0AfAQBXAMz/zP8FAAAAV8yAzP/M/wUAAAABAAAAAAAAAAIAAAAAAAAACgAAAAAAAAA2AA0BARQBAAsAAAAAAAAADAAAAAAAAABiACIAAAAADVEAPhMCQEUAKwA+EwEAAABEAAEAAAAAACkAAQAcAAEANgANAsy2QAEBNsyADQLM0jsBAQ8AAAAAAAAANQANAQEUAAADAAAAAAAAAAMAAQAAAAAABAAAAAAAAAACzIAAAAAAAAAKzIAAAAAAAAA2AA0BARQBAFfMgAAAAQAAADYADQI/JAEBNgANAszkQwEBC8yAAAAAAAAADMyAAAAAAAAAYsyAGwAAAAANUcyAzMUPBUA3ACvMgMzFDwIAAABEzIACAAAAAAApzIACABwAAQA2AA0CzNI7AQE2zIANAsy2QAEBDcyAAAAAAAAAFMyAAAAAAAAAF8yAAgAAAQIAGwABAAAAAAAbAAAAAgAAABXMgD4TzMUPAAAWzIBkAAAABgAgAAEAAQAAABsAAAAEAAAAJABkAAHMgMzFDzUADQLM/i8AADYADQLM/i8BARsAAAAFAAAAMQABABAABwA1AA0CzKs6AAA1AA0CfzEAADUADQJuRgAANgANAk8dAAE2AA0CEywBATYADQJ/MQEBNgANAmoyAQE2AA0CzL43AQE2AA0CzOE4AQE2AA0CzKs6AQE2AA0CzNRCAQE2AA0CbkYBATbMgA0CzOE4AQEtADAaPhMiABAAEAIiAAEAEQAQAiIAAQARABACPhMBzP9iACAAAAAFDywABEAgAAAARAACAAQAAAApAAIAHAABAFYAWjTM+cy9AAASABACIgABADUADQHMpR8AACkAAgAvAAAAHAAAAAAAAAAUzIAAAAAAAAAdzIAAAAAAAAAOzIAAAAAAAABSzIDM4BQTADcAIcyAOQABAAAAD8yAAAAAAAAANQANAQEUAAADzIAAAAAAAAADzIABAAAAAAAEzIAAAAAAAAACAAAAAAAAAAoAAAAAAAAANgANAQEUAQBXAAAAAQAAADbMgA0CPyQBATbMgA0CzORDAQELAAAAAAAAAAwAAAAAAAAAUgBmHA5ANAAtAAcGZhw0ABAABwA0AAEAEQAHADQAAQARAAcAZhwBzP84AAcAAAAAACkABwAcAAEAEgAHADQAAQApAAcAMwABADXMgA0CTEMAADYADQHMsjABADYADQLM8kUBATbMgA0CTEMBAaFmkgQE");
            //byte[] buffer2 = Convert.FromBase64String(str1);// "gqFi3AJBeMyczL3MlMzPTlNREMzGzL9zzNpKCi3Mrcy9zIVCzO5tzIAEzJM2zJVgJUXMoAlcLcyyzLEGzOzMpltLWMy5zOAVzJjM1zFsWMy5dMzrzN7MuGRjzMITzLh0zNE6zOfMzDTMlRtKzIhRzLvM+czlzPTMzsyZzPnM5szfecyCeCVeMczoYzTMygDM6MyTzNBAflY5zKvM3EbM3syYzLLMwcyczJ5zzMpTzKw6zOTM32E/SMzHL8zRYMy6zLvMh8zqzKHMzsyMzNgrzN/Mt8yfYmPMtilvzL/MtsyNKczozP3Mlsz3C8ykzPTMnFIFzOnMsQ5SHXRbR8yfRMyrzPN7zLDMzn7MmTcdY8zmzNQuzKc8zKVlzNFHV8yFTMy8zIUGzNPM6yPM1UcTfU7Ml8zTzOd0zObM9X5ZWXHM9szWIlR9zKHM1mjMgcz2zIMrTmbMkczOzPjM9MyIzKthzL1FzKg5zKzM4wzMhmNyzJ52zLgpesyHzJvMxsy4zO/MrszeTcy2L2LMxszVzMFezO7M+XrM2MyLzKbM8Mz8SMzszI/Mq8zwzPkFTyXMz8yLzKbM8MzDc8zhzOcXzMLM7zvMwszLPczhzLdXzMLM8yPMyczLfcyPcDvMr8yqcl7M2cyjzJtyzIBjzKzM0cyuzM/Mo0DMiT7M6TlFzNonPU/Mm8yXPsy0T8ykfSLM7cyTzNpNzOYoJ33MwiBKzMfMyzxHzIPM6CFzzNRDGMyUzOIaNlBfDMyDGszXMsyLGszbPcz+zI1hYEbMzyjMi2LM5xgtzL7M98yxBczszPDM/8ziN8zrzP3MlszUzN7MncytcsyLzOnM7My7HczgzIAtzN4DzOTM4sz/eC3M8czbNcyhzO/Mg8z3K8z8zLIrzOxyzJ7M+XHMvcyVFcznzP3MrjnM4nzM/RxpNVc1zO/M8RzM3TcvzNPM5szkT8zmYwPMxczZQcyUzOMYRcybzPPM9cybcBDMmVHMm8yszO/MwjTMlszUPkrM5F1VzM4rzNfMuMzDfsy+zJDMmC89zP/M/XdgQczmzIt3cSZuzPh3zMDMxTokzIlYZz7M5B1YzKLM8X4nzN8BQMzezIHM68yMUz3M2cz/zOvMzH3M+8yfzNzMxyVKzP1TzP93zO/M+8zbzP/MvszvPcy8CUrMiHkOJG4PP8yDMk7MmMzDShFdJMz3zOYXoWaSBAQ=");
            //
            //
            //
            //Dictionary<string, object> dictionary2 = MessagePack.Unpack(buffer2) as Dictionary<string, object>;
            //byte[] b2 = (dictionary2["b"] as byte[]);
            //var v2 = decompress(b2);
            //
            ////Debug.WriteLine(BitConverter.ToString(buffer2).Replace("-", " "));
            ////MessagePack.Pack(
            //
            ////DuelSimulator sim1 = new DuelSimulator("Data");
            ////sim1.Init();
            ////return;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant() == "--cpucontest-sim" && i < args.Length - 5)
                {
                    try
                    {
                        string deckFile1 = args[i + 1];
                        string deckFile2 = args[i + 2];
                        uint seed;
                        bool goFirst;
                        int iterationsBeforeIdle;
                        if (!uint.TryParse(args[i + 3], out seed) || !bool.TryParse(args[i + 4], out goFirst) ||
                            !int.TryParse(args[i + 5], out iterationsBeforeIdle) || !File.Exists(deckFile1) || !File.Exists(deckFile2))
                        {
                            return -1;
                        }
                        Process parentProcess = null;
                        int pid;
                        if (i < args.Length - 6 && int.TryParse(args[i + 6], out pid))
                        {
                            parentProcess = Process.GetProcessById(pid);
                        }
                        string dataDir = Utils.GetDataDirectory(true);
                        YdkHelper.LoadIdMap(dataDir);
                        DuelSimulator sim = new DuelSimulator(dataDir);
                        if (!sim.InitContent())
                        {
                            return -1;
                        }
                        return sim.RunCpuVsCpu(deckFile1, deckFile2, seed, goFirst, iterationsBeforeIdle, parentProcess);
                    }
                    catch
                    {
                        // NOTE: This doesn't catch duel.dll access violations... TODO: Add some native error handler
                        return -2;
                    }
                }
            }

            GameServer server = new GameServer();
            server.Start();
            System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
            return 0;
        }
    }
}
