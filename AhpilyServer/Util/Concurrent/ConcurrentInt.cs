using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer.Util.Concurrent
{
    /// <summary>
    /// 线程安全的 int 类型
    /// </summary>
    public class ConcurrentInt
    {
        private int value;

        public ConcurrentInt(int value) {
            this.value = value;
        }

        /// <summary>
        /// 添加并且得到
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public int Add(int number) {
            lock (this) {
                this.value += number;
                return this.value;
            }
        }

        /// <summary>
        /// 默认直接加1
        /// </summary>
        /// <returns></returns>
        public int Add() {
            return this.Add(1);
        }

        public int Get() {
            return this.value;
        }
    }
}
