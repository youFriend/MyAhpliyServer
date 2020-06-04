using AhpilyServer;
using Protocol;
using GameServer.Logic;
using GameServer.Logic.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class NetMsgCenter:IApplication
    {

        IHandler accountHandler = new AccountHandler();
        public  void onDisconnect(ClientPeer client)
        {
            accountHandler.onDisconnect(client);
        }
        public  void OnReceive(ClientPeer client, SocketMsg msg)
        {
            Console.WriteLine("收到的opcode" );
            switch (msg.OpCode) {
                case OpCode.ACCOUNT :
                    accountHandler.OnReceive(client, msg.SubCode, msg.Value);
                    break;
            }
        }
    }
}
