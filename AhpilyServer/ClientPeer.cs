using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    public class ClientPeer
    {
        public Socket ClientSocket{
            get;set;
        }

        // 我们的数据也在list 中不需要老是 new 
        private List<byte> dataCache = new List<byte>();
        //
        public SocketAsyncEventArgs ReceiveArgs;
        //
        private bool isReceiveProcess = false;
        //定义一个委托
        public  delegate void ReceiveCompleted(ClientPeer client,SocketMsg value);
        //声明一个委托 （消息完成的回调）
        public ReceiveCompleted receiveCompeleted;
        // 发送消息的队列，如果说，很多消息，那么就慢慢发送
        // 这里就可以扩展到我们client 端的方式
        private Queue<byte[]> sendQueue = new Queue<byte[]>();
        //是否发送
        private bool isSendProcess = false;
        //发送的异步套接字，可以new 的
        private SocketAsyncEventArgs SendArgs;
        // 给应用层调用，当客户端断开连接的时候
        public   delegate void SendDisConnect(ClientPeer client,string reason);
        // 给应用层调用，当客户端断开连接的时候 ,这个就是实体类
        public   SendDisConnect sendDisConnect;

        public ClientPeer()
        {
            //把new 的内容都统一管理起来
            this.ReceiveArgs = new SocketAsyncEventArgs();
            this.ReceiveArgs.UserToken = this; // 后面要使用可以反查找到拥有者
            this.ReceiveArgs.SetBuffer(new byte[1024], 0, 1024);
            this.SendArgs = new SocketAsyncEventArgs();
            this.SendArgs.Completed += SendArgs_Completed;
        }

 
        public ClientPeer(Socket socket) {
            setSocket(socket);
        }
        /// <summary>
        /// 开始处理数据包
        /// </summary>
        /// <param name="packet"></param>
        public void StartReceive(byte [] packet) {
            // 添加一个集合
            dataCache.AddRange(packet);
            //防止并发
            if (!isReceiveProcess)
                processReceive();
            //Console.WriteLine(dataCache.Count + "缓存区有数据了");
        }
        /// <summary>
        /// 处理接收数据，如果处理完成就直接回调函数给应用层
        /// </summary>
        private void processReceive() {
            isReceiveProcess = true;
            //处理数据
            byte [] data = EncodeTool.DecodePacket(ref dataCache);
            //Console.WriteLine("接收数据成功"+ data == null);
            if (data == null){
                isReceiveProcess = false;
                return;
            }
            //TODO 需要再次转成一个具体的类型功我们使用
            SocketMsg value = EncodeTool.DecodeMsg(data);
            //Console.WriteLine((value.Value as string));
            if (value != null) {
                //这样子调用函数
                receiveCompeleted(this, value);
            }
            //
            //Console.WriteLine("接收数据成功");
            //回调上层,尾递归，继续处理，直到为空
            processReceive();
        }



        public void setSocket(Socket socket) {
            this.ClientSocket = socket; 

        }
        #region 断开连接
        /// <summary>
        /// 断开连接
        /// </summary>
        public void disConnect()
        {
            try
            {
                dataCache.Clear();
                isReceiveProcess = false;
                //TODO 给发送数据
                sendQueue.Clear();
                isSendProcess = false;
                //两个都断开
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
                ClientSocket = null;
                // ClientSocket.Disconnect(true);
            }
            catch (Exception e)
            {
                
                throw new Exception(e.Message);
            }
        }

        #endregion

        #region 发送数据
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        public void Send(int opCode ,int subCode,object value) {
            SocketMsg msg = new SocketMsg(opCode, subCode, value);
            // 先变成 socketMsg ,再变成 包头 + 包尾 的形式
            byte[] data = EncodeTool.EncodeMsg(msg);
            //
            byte[] packet = EncodeTool.EncodePacket(data);
            //现存好数据，然后再发送
            sendQueue.Enqueue(packet);

            if (!isSendProcess) {
                // 处理一下发送消息
                send();
            }
        }

        private void send() {
            //
            isSendProcess = true;
            if (sendQueue.Count == 0) {
                isSendProcess = false;
                // 如果没有数据，那么就不发送了
                return;
            }
            byte[] packet = sendQueue.Dequeue();
            SendArgs.SetBuffer(packet, 0, packet.Length);
            //
            bool result = ClientSocket.SendAsync(SendArgs);
            if (result == false) { 
                //如果发送错误，重新发送
                sendProcess();
            }
        }
        /// <summary>
        ///  异步发送完成的时候调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Console.WriteLine("接收数据完成");
            sendProcess();
        }
        /// <summary>
        /// 完成的时候调用
        /// </summary>
        private void sendProcess() {
            //
            if (SendArgs.SocketError == SocketError.Success)
            {
                //如果一条信息发送成功，那么就发送另外一条信息
                send();
            }
            else {
                // 如果失败
                //就是客户端连接失败了
                // 直接暴露外面的接口，让他来调用就好
                sendDisConnect(this, SendArgs.SocketError.ToString());
            }
        }
        #endregion
    }
}
