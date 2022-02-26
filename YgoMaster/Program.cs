using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace YgoMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            GameServer server = new GameServer();
            server.Start();
            System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
        }
    }
}
