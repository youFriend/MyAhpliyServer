using AhpilyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    /// <summary>
    /// 就是所有的逻辑层都实现该方法
    /// </summary>
    interface IHandler
    {
        /// <summary>
        /// 所有的逻辑层都有接收的方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        void OnReceive(ClientPeer client, int subCode, object value);
        
        /// <summary>
        ///  断开连接时候回调
        /// </summary>
        /// <param name="client"></param>
        void onDisconnect(ClientPeer client);

    }
}
