using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    class ClientPeerPool
    {
        /// <summary>
        /// 首先存好这么多个对象，要得时候直接出队列, z这个池得对象我们设置得时候就直接是max 连接得数量
        /// </summary>
        private Queue<ClientPeer> clientPeerQueue;

        public ClientPeerPool(int capacity) {
            clientPeerQueue = new Queue<ClientPeer>(capacity);
        }

        public void Enqueue(ClientPeer clientPeer) {
            clientPeerQueue.Enqueue(clientPeer);
        }

        public ClientPeer Dequeue()
        {
            //去除最前得对象
            return clientPeerQueue.Dequeue();
        }
    }
}
