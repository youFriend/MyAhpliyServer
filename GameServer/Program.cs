using AhpilyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 子封装
            ServerPeer server = new ServerPeer();
            //
            server.SetApplication(new NetMsgCenter());
            server.Start(8888,10);
            Console.ReadKey();
        }
    }
}
