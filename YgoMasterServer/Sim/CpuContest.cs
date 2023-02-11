using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

// TODO:
// - Add a matchmaking system. Currently it picks opponents at random which probably doesn't produce great rank distribution
// - Add support for adding a new deck to a pool of already ranked decks

namespace YgoMaster
{
    class CpuContest
    {
        public string DataDir { get; private set; }
        public string ContestDir { get; private set; }
        public string DeckStatsDir { get; private set; }
        List<DuelEngineInstance> engines = new List<DuelEngineInstance>();
        Dictionary<DeckInfo, DeckStats> decks = new Dictionary<DeckInfo, DeckStats>();
        List<DeckInfo> decksRemaining = new List<DeckInfo>();
        Random rand = new Random();
        int numIterationsBeforeIdle;
        int numEnginesFinished;
        int numDuelsPerDeck;
        int numDuelsTotal;
        int numDuelsStarted;
        int numDuelsComplete;

        public CpuContest(string dataDir)
        {
            DataDir = dataDir;
        }

        public void Run()
        {
            ContestDir = Path.Combine(DataDir, "CpuContest");
            if (!Directory.Exists(ContestDir))
            {
                Console.WriteLine("Failed to find folder '" + ContestDir + "'");
                return;
            }
            string decksDir = Path.Combine(ContestDir, "Decks");
            if (!Directory.Exists(decksDir))
            {
                Console.WriteLine("Failed to find folder '" + decksDir + "'");
                return;
            }
            string cardDataDir = Path.Combine(DataDir, "CardData");
            if (!Directory.Exists(cardDataDir))
            {
                Console.WriteLine("Failed to find folder '" + cardDataDir + "'");
                return;
            }
            string cardDataPropsFile = Path.Combine(cardDataDir, "#", "CARD_Prop.bytes");
            if (!File.Exists(cardDataPropsFile))
            {
                Console.Write("Failed to find '" + cardDataPropsFile + "'");
                return;
            }
            string dllFile = Path.Combine(DataDir, "..", "..", "masterduel_Data", "Plugins", "x86_64", "duel.dll");
            if (!File.Exists(dllFile))
            {
                Console.Write("Failed to find '" + dllFile + "'");
                return;
            }
            string contestSettingsFile = Path.Combine(ContestDir, "ContestSettings.json");
            if (!File.Exists(contestSettingsFile))
            {
                Console.WriteLine("Failed to find file '" + contestSettingsFile + "'");
                return;
            }
            DeckStatsDir = Path.Combine(ContestDir, "DeckStats");
            Utils.TryCreateDirectory(DeckStatsDir);
            Dictionary<string, object> settings = MiniJSON.Json.DeserializeStripped(File.ReadAllText(contestSettingsFile)) as Dictionary<string, object>;
            int numInstances = Utils.GetValue(settings, "instances", 1);
            numIterationsBeforeIdle = Utils.GetValue(settings, "iterationsBeforeIdle", 1);
            numDuelsPerDeck = Utils.GetValue(settings, "duelsPerDeck", 1);
            foreach (string deckFile in Directory.GetFiles(decksDir))
            {
                try
                {
                    DeckInfo deck = new DeckInfo();
                    deck.File = deckFile;
                    deck.Load();
                    if (deck.GetAllCards().Count > 0)
                    {
                        DeckStats stats = new DeckStats(this, deck);
                        stats.Load();
                        decks[deck] = stats;
                    }
                }
                catch
                {
                }
            }
            if (decks.Count < 2)
            {
                Console.WriteLine("Not enough decks. Found " + decks.Count);
                return;
            }
            foreach (DeckStats stats in decks.Values)
            {
                int numDuels = stats.GetNumGoFirstDuels();
                numDuelsStarted += numDuels;
                numDuelsComplete += numDuels;
            }
            numDuelsTotal = ((decks.Count + (decks.Count % 2)) / 2) * numDuelsPerDeck;
            UpdateProgressBar();
            for (int i = 0; i < numInstances; i++)
            {
                engines.Add(new DuelEngineInstance(i + 1, this));
            }
            numEnginesFinished = 0;
            foreach (DuelEngineInstance engine in engines)
            {
                engine.Start();
            }
            while (true)
            {
                lock (engines)
                {
                    if (numEnginesFinished == engines.Count)
                    {
                        foreach (DuelEngineInstance otherEngine in engines)
                        {
                            otherEngine.Stop();
                        }
                        string decksByRatingDir = Path.Combine(ContestDir, "DecksByRating");
                        Utils.TryCreateDirectory(decksByRatingDir);
                        List<object> statsData = new List<object>();
                        foreach (DeckStats stats in decks.Values.OrderByDescending(x => x.Rating))
                        {
                            statsData.Add(new Dictionary<string, object>()
                            {
                                { "rating", (int)stats.Rating },
                                { "deck", Path.GetFileName(stats.Deck.File) },
                            });
                            File.Copy(stats.Deck.File, Path.Combine(decksByRatingDir, ((int)stats.Rating) + " - " + Path.GetFileName(stats.Deck.File)), true);
                        }
                        File.WriteAllText(Path.Combine(ContestDir, "Results.json"), MiniJSON.Json.Format(MiniJSON.Json.Serialize(statsData)));
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public uint GetNextSeed()
        {
            lock (engines)
            {
                return (uint)rand.Next();
            }
        }

        void Shuffle<T>(IList<T> list)
        {
            lock (engines)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    int randIndex = rand.Next(i + 1);
                    T temp = list[randIndex];
                    list[randIndex] = list[i];
                    list[i] = temp;
                }
            }
        }

        DeckInfo GetNextDeck()
        {
            lock (engines)
            {
                if (decksRemaining.Count == 0)
                {
                    foreach (KeyValuePair<DeckInfo, DeckStats> deck in decks)
                    {
                        decksRemaining.Add(deck.Key);
                    }
                    Shuffle(decksRemaining);
                }
                int lastIndex = decksRemaining.Count - 1;
                DeckInfo result = decksRemaining[lastIndex];
                decksRemaining.RemoveAt(lastIndex);
                return result;
            }
        }

        DeckStats GetNextDeck(out DeckStats opponent, out bool goFirst)
        {
            lock (engines)
            {
                opponent = null;
                goFirst = false;
                if (numDuelsStarted == numDuelsTotal)
                {
                    return null;
                }
                DeckInfo deck = GetNextDeck();
                opponent = decks[GetNextDeck()];
                DeckStats result = decks[deck];
                goFirst = result.GetNumGoFirstDuels() <= opponent.GetNumGoSecondDuels();
                numDuelsStarted++;
                return result;
            }
        }

        void UpdateProgressBar()
        {
            Console.Title = numDuelsComplete + " / " + numDuelsTotal;
        }

        void OnDuelResult(DeckStats stats, DeckStats opponentStats, bool goFirst, DuelResultType result, TimeSpan duration)
        {
            //Debug.WriteLine("Duration:" + duration + " result:" + result + " deck1:" + stats.Deck.Name + " deck2:" + opponentStats.Deck.Name);
            lock (engines)
            {
                numDuelsComplete++;
                UpdateProgressBar();

                double rating = stats.Rating;
                stats.Update(opponentStats.Rating, result, goFirst);
                switch (result)
                {
                    case DuelResultType.Win:
                        opponentStats.Update(rating, DuelResultType.Lose, !goFirst);
                        break;
                    case DuelResultType.Lose:
                        opponentStats.Update(rating, DuelResultType.Win, !goFirst);
                        break;
                    case DuelResultType.Draw:
                        opponentStats.Update(rating, DuelResultType.Draw, !goFirst);
                        break;
                }

                // TODO: Maybe save periodically instead of after every duel?
                stats.Save();
                opponentStats.Save();
            }
        }

        void OnEngineFinished(DuelEngineInstance engine)
        {
            lock (engines)
            {
                if (numEnginesFinished < engines.Count)
                {
                    numEnginesFinished++;
                }
            }
        }

        class DeckStats
        {
            public CpuContest Contest;
            public DeckInfo Deck;
            public double Rating;
            public bool IsPro;
            public Dictionary<DuelResultType, int> Results { get; private set; }
            public Dictionary<DuelResultType, int> GoFirstResults { get; private set; }
            public Dictionary<DuelResultType, int> GoSecondResults { get; private set; }

            public DeckStats(CpuContest contest, DeckInfo deck)
            {
                Contest = contest;
                Deck = deck;
                Results = new Dictionary<DuelResultType, int>();
                GoFirstResults = new Dictionary<DuelResultType, int>();
                GoSecondResults = new Dictionary<DuelResultType, int>();
                Dictionary<DuelResultType, int>[] collections = { Results, GoFirstResults, GoSecondResults };
                foreach (Dictionary<DuelResultType, int> collection in collections)
                {
                    collection[DuelResultType.Win] = 0;
                    collection[DuelResultType.Lose] = 0;
                    collection[DuelResultType.Draw] = 0;
                }
                SetDefaultRating();
            }

            string GetFilePath()
            {
                string fileName = Path.GetFileName(Deck.File);
                if (Deck.IsYdkDeck)
                {
                    fileName += ".json";
                }
                return Path.Combine(Contest.DeckStatsDir, fileName);
            }

            public void Load()
            {
                string path = GetFilePath();
                if (File.Exists(path))
                {
                    Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(path)) as Dictionary<string, object>;
                    if (data == null)
                    {
                        return;
                    }
                    double rating = Utils.GetValue<double>(data, "rating");
                    if (rating < 100)
                    {
                        return;
                    }
                    Rating = rating;
                    IsPro = Utils.GetValue<bool>(data, "pro");
                    ResultsFromDictionary(Utils.GetValue(data, "results", default(Dictionary<string, object>)), Results);
                    ResultsFromDictionary(Utils.GetValue(data, "resultsGoFirst", default(Dictionary<string, object>)), GoFirstResults);
                    ResultsFromDictionary(Utils.GetValue(data, "resultsGoSecond", default(Dictionary<string, object>)), GoSecondResults);
                }
            }

            public void Save()
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["rating"] = Rating;
                data["pro"] = IsPro;
                data["results"] = ResultsToDictionary(Results);
                data["resultsGoFirst"] = ResultsToDictionary(GoFirstResults);
                data["resultsGoSecond"] = ResultsToDictionary(GoSecondResults);
                File.WriteAllText(GetFilePath(), MiniJSON.Json.Format(MiniJSON.Json.Serialize(data)));
            }

            void ResultsFromDictionary(Dictionary<string, object> data, Dictionary<DuelResultType, int> collection)
            {
                if (data == null)
                {
                    return;
                }
                collection[DuelResultType.Win] = Utils.GetValue<int>(data, "win");
                collection[DuelResultType.Lose] = Utils.GetValue<int>(data, "lose");
                collection[DuelResultType.Draw] = Utils.GetValue<int>(data, "draw");
            }

            Dictionary<string, object> ResultsToDictionary(Dictionary<DuelResultType, int> collection)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                result["win"] = collection[DuelResultType.Win];
                result["lose"] = collection[DuelResultType.Lose];
                result["draw"] = collection[DuelResultType.Draw];
                return result;
            }

            public int GetNumDuels()
            {
                return GetNumDuels(Results);
            }

            public int GetNumGoFirstDuels()
            {
                return GetNumDuels(GoFirstResults);
            }

            public int GetNumGoSecondDuels()
            {
                return GetNumDuels(GoSecondResults);
            }

            int GetNumDuels(Dictionary<DuelResultType, int> collection)
            {
                int num = 0;
                foreach (KeyValuePair<DuelResultType, int> result in collection)
                {
                    num += result.Value;
                }
                return num;
            }

            public void Update(double opponentRating, DuelResultType result, bool goFirst)
            {
                double score = 0;
                switch (result)
                {
                    case DuelResultType.Win:
                        score = 1;
                        break;
                    case DuelResultType.Lose:
                        score = 0;
                        break;
                    case DuelResultType.Draw:
                        score = 0.5;
                        break;
                    default:
                        return;
                }

                Results[result]++;
                if (goFirst)
                {
                    GoFirstResults[result]++;
                }
                else
                {
                    GoSecondResults[result]++;
                }

                // NOTE: Probably not going to run enough duels for this config. Maybe just default to 15?
                // FIDE (chess) config
                double kFactor = 25;
                if (IsPro)
                {
                    kFactor = 10;
                }
                else if (GetNumDuels() >= 30)
                {
                    kFactor = 15;
                }
                if (!IsPro && Rating >= 2400)
                {
                    IsPro = true;
                }

                double expectedRating = 1 / (1 + (Math.Pow(10, (opponentRating - Rating) / 400.0)));
                double newRating = Rating + (kFactor * (score - expectedRating));
                //Console.WriteLine("Rating from " + Rating + " to " + newRating + " (" + result + ") " + Deck.Name);
                Rating = Math.Max(100, newRating);
            }

            public void SetDefaultRating()
            {
                Rating = 1200;
                IsPro = false;
            }
        }

        class DuelEngineInstance
        {
            public int Id { get; private set; }
            public CpuContest Contest { get; private set; }
            Thread thread;

            public DuelEngineInstance(int id, CpuContest contest)
            {
                Id = id;
                Contest = contest;
            }

            public void Start()
            {
                Stop();
                thread = new Thread(delegate()
                    {
                        while (thread == Thread.CurrentThread)
                        {
                            bool goFirst;
                            DeckStats opponentStats;
                            DeckStats stats = Contest.GetNextDeck(out opponentStats, out goFirst);
                            if (stats != null)
                            {
                                //Debug.WriteLine("Start duel deck1:" + duel.Deck.Name + " deck2:" + opponentDeck.Name);

                                using (Process process = new Process())
                                using (Process currentProcess = Process.GetCurrentProcess())
                                {
                                    Stopwatch stopwatch = new Stopwatch();
                                    stopwatch.Start();
                                    process.StartInfo.Arguments = "--cpucontest-sim \"" + stats.Deck.File + "\" \"" + opponentStats.Deck.File + "\" " +
                                        Contest.GetNextSeed() + " " + goFirst + " " + Contest.numIterationsBeforeIdle + " " + currentProcess.Id;
                                    process.StartInfo.FileName = "YgoMaster.exe";
                                    process.StartInfo.CreateNoWindow = true;
                                    process.StartInfo.UseShellExecute = false;
                                    process.Start();
                                    process.WaitForExit();
                                    if (process.ExitCode <= 0)
                                    {
                                        Console.WriteLine("Error occurred when running duel sim deck1:" + stats.Deck.Name + " deck2:" + opponentStats.Deck.Name);
                                    }
                                    DuelResultType result = (DuelResultType)process.ExitCode;
                                    switch (result)
                                    {
                                        case DuelResultType.Win:
                                        case DuelResultType.Lose:
                                            break;
                                        default:
                                            result = DuelResultType.Draw;
                                            break;
                                    }
                                    if (thread == Thread.CurrentThread)
                                    {
                                        stopwatch.Stop();
                                        Contest.OnDuelResult(stats, opponentStats, goFirst, result, stopwatch.Elapsed);
                                    }
                                }
                            }
                            else
                            {
                                Contest.OnEngineFinished(this);
                                break;
                            }
                            Thread.Sleep(1);
                        }
                    });
                thread.Start();
            }

            public void Stop()
            {
                try
                {
                    if (thread != null)
                    {
                        thread.Abort();
                    }
                }
                catch
                {
                }
                thread = null;
            }
        }
    }
}
