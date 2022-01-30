using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();
            server.Start();
            System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
        }
    }
}
