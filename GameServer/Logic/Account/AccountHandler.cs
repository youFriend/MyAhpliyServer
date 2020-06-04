using AhpilyServer;
using Protocol;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic.Account
{
    class AccountHandler : IHandler
    {
        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            AccountDto accountDto = (AccountDto)value ;
            //string aa = value as string;
            switch (subCode)
            {
                case AcountCode.REGIST_CREO:
                    //注册
                    Console.WriteLine((accountDto==null) +"注册账号：" +(accountDto.Account) ); // accountDto.Account.Length
                    Console.WriteLine("密码：" + accountDto.Password);
                    client.Send(OpCode.ACCOUNT,AcountCode.REGIST_SRES,"注册账号：" + (accountDto.Account));
                    break;
                case AcountCode.LOGIN_CREO:
                    //登录

                    Console.WriteLine(accountDto.Account);
                    break;
                default:
                    break;
            }
        }
        public void onDisconnect(ClientPeer client)
        {
            Console.WriteLine("AccountHandle +  客户端断开连接");

        }
    }
}
