using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    class EncodeTool
    {
        #region 封装一个又规定的数据包
        public static byte[] EncodePacket(byte[] data)
        {
            //内存流对象


            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //我们封装的内容就直接是  长度 +  data
                    bw.Write(data.Length);
                    bw.Write(data);
                    byte[] byteArray = new byte[(int)ms.Length]; // 字节流的长度
                    //效率最高的转存方式
                    //参数说明 ： 要复制的数组，开始的位置，到达的数组， 开始的位置， 复制的长度
                    Buffer.BlockCopy(ms.GetBuffer(), 0, byteArray, 0, (int)ms.Length);
                    return byteArray;
                }

            }

            //使用后自动释放 ，使用using 关键字
            // ms.Close();
            // bw.Close();

        }
        #endregion

        #region 解释消息体
        public static byte[] DecodePacket(ref List<byte> dataCache)
        {
            if (dataCache.Count < 4)
            {
                Console.WriteLine("DecodePacket收到一个长度小于4的包，包错误");
                return null;
                //throw new Exception("数组的缓存长度不大于4，构不成一个int");

            }
            using (MemoryStream ms = new MemoryStream(dataCache.ToArray()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    int length = br.ReadInt32();
                    int dataRemainLength = (int)(ms.Length - ms.Position);
                    // 判断字节流的长度，是否和我们预定写好的长度一致
                    if (length > dataRemainLength)
                    {
                        Console.WriteLine("DecodePacket收到一个长度过于大，验证错误");
                        return null;
                        //throw new Exception("数据的长度和我们预定的长度不一致，校验失败");
                    }
                    byte[] data = br.ReadBytes(length);
                    //注意我们要清空一下数据的缓存，因为我们的数据cache 对象是一直存在的
                    dataCache.Clear();
                    //虽然以前的位置清空了，但是我们把现在的内容读取后又写在回了里面
                    dataCache.AddRange(br.ReadBytes(dataRemainLength));
                    //
                    return data;
                }
            }
        }
        #endregion


        #region 封装发送SocketMsg 转换工具
        /// <summary>
        /// 把socketMsg 转换成为
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] EncodeMsg(SocketMsg msg)
        {
            byte[] data = null;
            MemoryStream ms = new MemoryStream();

            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(msg.OpCode);
            bw.Write(msg.SubCode);
            if (msg.Value != null)
            {
                //序列化
                byte[] values = EncodeObj(msg.Value);
                bw.Write(values);
                data = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);

            }
            //把所有的数据复制，（把数据写入）
            return data;

        }

        public static SocketMsg DecodeMsg(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);
            //
            SocketMsg msg = new SocketMsg();
            msg.OpCode = br.ReadInt32();
            // 如果还有数据
            msg.SubCode = br.ReadInt32();
            if (ms.Length > ms.Position)
            {
                byte[] valueBytes = br.ReadBytes((int)(ms.Length - ms.Position));
                //Todo 反序列化
                object value = DecodeObj(valueBytes); //valueBytes
                msg.Value = value;
            }
            ms.Close();
            br.Close();
            return msg;

        }
        #endregion


        #region 把一个object 转换成为byte[] 等等, 序列化
        public static byte[] EncodeObj(object value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // 新建序列化
                BinaryFormatter bf = new BinaryFormatter();
                // c# 内置序列化 函数
                bf.Serialize(ms, value);
                byte[] valueBytes = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, valueBytes, 0, (int)ms.Length);
                return valueBytes;
            }
        }
        /// <summary>
        /// 反序序列化对象
        /// </summary>
        /// <param name="valueBytes"></param>
        /// <returns></returns>
        public static object DecodeObj(byte[] valueBytes)
        {
            using (MemoryStream ms = new MemoryStream(valueBytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object value = bf.Deserialize(ms);
                return value;
            }
        }



        #endregion
    }
}
