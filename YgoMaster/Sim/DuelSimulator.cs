using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace YgoMaster
{
    partial class DuelSimulator
    {
        public string DataDir { get; private set; }

        public DuelSimulator(string dataDir)
        {
            DataDir = dataDir;
        }

        RunEffect DoRunEffect = (int id, int param1, int param2, int param3) =>
        {
            DuelViewType viewType = (DuelViewType)id;
            switch (viewType)
            {
                case DuelViewType.DuelEnd:
                    Console.WriteLine("DoRun " + viewType + " resultType:" + (DuelResultType)param1 + " finishType:" + (DuelFinishType)param2 + " p3:" + param3);
                    break;
                default:
                    Console.WriteLine("DoRun " + viewType + " p1:" + param1 + " p2:" + param2 + " p3:" + param3 + " | " +
                        DLL_DuelGetLP(0) + " " + DLL_DuelGetCardNum(0, 15) + " " + DLL_DuelGetLP(1) + " " + DLL_DuelGetCardNum(1, 15));
                    break;
            }
            return 0;
        };

        IsBusyEffect DoIsBusyEffect = (int id) =>
        {
            DuelViewType viewType = (DuelViewType)id;
            //Console.WriteLine("DoIsBusyEffect " + viewType);
            return 0;
        };

        void SetDeck(int player, DeckInfo deck)
        {
            int[] main = deck.MainDeckCards.GetIds().ToArray();
            int[] extra = deck.ExtraDeckCards.GetIds().ToArray();
            int[] side = deck.SideDeckCards.GetIds().ToArray();
            DLL_DuelSysSetDeck2(player, main, main.Length, extra, extra.Length, side, side.Length);
        }

        uint GetCpuParam(int val, DuelCpuParam param = DuelCpuParam.None)
        {
            val = Math.Min(100, Math.Max(-100, val));
            if (val < 0)
            {
                param |= DuelCpuParam.Def;
                val = -val;
            }
            return (uint)(val | (int)param);
        }

        public void Init()
        {
            if (string.IsNullOrEmpty(DataDir) || !Directory.Exists(DataDir))
            {
                return;
            }
            InitContent();

            //Console.WriteLine("Version: " + DLL_GetRevision());

            DeckInfo deck1 = new DeckInfo();
            deck1.File = "241244.ydk";
            YdkHelper.LoadDeck(deck1);

            DeckInfo deck2 = new DeckInfo();
            deck2.File = "241310.ydk";
            YdkHelper.LoadDeck(deck2);

            const int myPlayerNum = 1;
            uint seed = 0;
            Random rand = new Random();
            seed = (uint)rand.Next();

            int num = DLL_SetWorkMemory(IntPtr.Zero);
            IntPtr engineWork = Marshal.AllocHGlobal(num);
            Debug.Assert(DLL_SetWorkMemory(engineWork) == 0);

            DLL_SetEffectDelegate(DoRunEffect, DoIsBusyEffect);
            DLL_DuelSysClearWork();
            DLL_DuelSetMyPlayerNum(myPlayerNum);
            DLL_DuelSetRandomSeed(seed);
            SetDeck(0, deck1);
            SetDeck(1, deck2);
            DLL_DuelSetPlayerType(0, (int)DuelPlayerType.CPU);//Human);
            DLL_DuelSetPlayerType(1, (int)DuelPlayerType.CPU);//Human);
            DLL_DuelSetCpuParam(0, GetCpuParam(100));
            DLL_DuelSetCpuParam(1, GetCpuParam(100));
            DLL_DuelSetFirstPlayer(0);
            DLL_DuelSetDuelLimitedType((uint)DuelLimitedType.None);
            DLL_DuelSysInitCustom((int)DllDuelType.Normal, false, 8000, 8000, 5, 5, false);
            //DLL_DuelSysInitRush(); <--- does this just auto push Rush,4000,4000?
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                //System.Threading.Thread.Sleep(1000);
                if (DLL_DuelSysAct() > 0)
                {
                    Console.WriteLine("!!!");
                    break;
                }
                System.Threading.Thread.Sleep(1);
            }
            
            Console.WriteLine(sw.Elapsed);
        }

        enum DllDuelType
        {
            Normal = 0,
            Speed = 1,
            Rush = 2
        }
    }
}
