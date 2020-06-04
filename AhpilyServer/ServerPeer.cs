using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace AhpilyServer
{
    /// <summary>
    /// 代表服务器端
    /// </summary>
    public class ServerPeer
    {
        private Socket serverSocket = null;
        //限制客户端连接的数量
        private Semaphore acceptSemaphore;
        //
        private ClientPeerPool clientPool;
        //应用层
        private IApplication iApplication;


        public ServerPeer()
        {
            //
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        #region 开启服务器，准备连接
        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="maxCount">最大连接数量</param>
        public void Start(int port, int maxCount)
        {
            try
            {
                //修复bug ，设置最大的线程数，  参数：现在可用的线程数量， 最大可用的线程数量
                acceptSemaphore = new Semaphore(maxCount, maxCount);
                //创建线程池
                clientPool = new ClientPeerPool(maxCount);
                ClientPeer tempClient = null;
                for (int i = 0; i < maxCount; i++)
                {
                    tempClient = new ClientPeer();
                    tempClient.ReceiveArgs.Completed += processReceiveCompeleted;
                    //用委托来为client 提供函数
                    tempClient.receiveCompeleted += receiveCompleted;
                    //发送的时候发生异常了,就直接断开连接，简单粗暴
                    tempClient.sendDisConnect += DisConnect;

                    clientPool.Enqueue(tempClient);
                    //
                }
                //任何人都可以接通的
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                //把 10 个线程至于监听状态，不用每一次访问都得重新创建
                serverSocket.Listen(10);
                //服务器启动
                startAccept(null);
            }
            catch (Exception e)
            {
                //报错
                Console.WriteLine(e.Message);
                throw;
            }
        }


        private void startAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                // += 添加事件
                e.Completed += accept_completed;
            }

            //通过异步开启(效率高一点)，开启异步接收
            bool result = serverSocket.AcceptAsync(e);
            // Console.WriteLine("服务器启动" + e == null);

            if (result == false)
            {
                Console.WriteLine("当服务器发生异常异步启动不了的时候的就会调用这里");
                // result = false 的时候表示,获取客户端的信息成功
                processAccept(e);

            }
            else
            {
                // 如果还在接收
                //TODO
                //Console.WriteLine("异步启动服务器成功");
            }
        }
        /// <summary>
        /// 接收完成时候调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void accept_completed(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("当前客户端连接成功的时候回调,继续等待下一个连接接收...");
            processAccept(e);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 处理接收
        /// </summary>
        private void processAccept(SocketAsyncEventArgs e)
        {
            //把可以用得线程减少一个,先开启减少一个 ,当我们取内容的时候就调用
            acceptSemaphore.WaitOne();
            //通过异步来调用是这样子调用的
            Socket clientSocket = e.AcceptSocket;
            //
            ClientPeer clientPeer = clientPool.Dequeue();
            clientPeer.ClientSocket = clientSocket;
            //
            startReceive(clientPeer);
            //然后就可以一直接收客户端得到得数据            
            //这里是，完成一个，传递e 的可以减少new e 的开销
            e.AcceptSocket = null;

            // 主要是回调 socket.AcceptAynsc
            startAccept(e);

        }

        #endregion

        #region 发送数据


        #endregion

        #region 断开连接
        /// <summary>
        ///  断开连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reason"></param>
        public void DisConnect(ClientPeer client, string reason)
        {
            try
            {
                //客户端断开的时候处理的数据，
                //1. 清空client 端的缓存
                //2. 清空client 对象
                if (client == null)
                {
                    throw new Exception("当前的客户端为 null, 无法断开连接");
                }
                Console.WriteLine("ahpilyServer ,disconnect  断开原因" + reason);
                //只有下一次连接的时候才会出现打印，这个是Unity 的Bug
                //TODO 通知应用层，断开连接
                iApplication.onDisconnect(client);
                //

                //
                client.disConnect();
                //回收 直接出来
                clientPool.Enqueue(client);
                // 默然退出一个连接数量
                acceptSemaphore.Release();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        #endregion


        #region 接收数据
        /// <summary>
        /// 开始接收数据
        /// </summary>
        /// <param name="client"></param>
        private void startReceive(ClientPeer client)
        {
            try
            {
                // 异步接收对象，这个接收的相的异步数据对象是连接的时候得到的
                // 把 client 的ui想
                bool result = client.ClientSocket.ReceiveAsync(client.ReceiveArgs);
                if (result == false)
                {
                    //防止 客户端异步接收不成功，然后手动启动接收
                    processReceive(client.ReceiveArgs);
                }
                else
                {
                    // 如果接收完成了的话，就处理完成，一般没有的
                    //因为我们是强连接
                    ///processReceiveCompeleted(client.ReceiveArgs);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 处理接收的请求
        /// </summary>
        /// <param name="e"></param>
        private void processReceive(SocketAsyncEventArgs e)
        {
            //反方向获得对象，因为这里是Compeleted 获得的对象，那个方法又被指定了对象，所以就限制了方向

            ClientPeer client = e.UserToken as ClientPeer;
            //
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                //Console.WriteLine("数据符合逻辑");
                //获取收据的长度
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                //拷贝到数组中
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);
                //客户端自身解释
                client.StartReceive(packet);
                //解释完成以后，又得开启接收 （C# 可能就是这样，就收后流可能要重新设置）
                startReceive(client);
            }
            else
            {
                //如果是进入else ，那么就是网络失败了，
                //如果传输的数据为0
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    //
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        // 如果是正常，那么就是 Socket Client 主动断开连接
                        DisConnect(client, "客户端主动断开连接");
                    }
                    else
                    {
                        //否则就是网络异常断开连接
                        DisConnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// Client 端 异步接收到一条消息完成时候的回调
        /// </summary>
        /// <param name="e"></param>
        private void processReceiveCompeleted(object sender, SocketAsyncEventArgs e)
        {
            //到最后的完成的时候还是一样的处理的，我们就写在一起就好了
            processReceive(e);
        }

        /// <summary>
        /// 这里是一个Client 的自定义委托函数
        /// 当客户端得到一条数据解释完成的处理
        /// </summary>
        /// <param name="client">对应的连接对象</param>
        /// <param name="value">得到的数据</param>
        private void receiveCompleted(ClientPeer client, SocketMsg value)
        {
            //
            //Console.WriteLine("服务器收到某某某Client 的信息");
            //TODO 留给应用层处理
            iApplication.OnReceive(client, value);
        }
        #endregion

        #region 设置应用层

        public void SetApplication(IApplication app)
        {
            this.iApplication = app;
        }
        #endregion
    }
}
