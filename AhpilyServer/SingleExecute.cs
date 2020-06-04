using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AhpilyServer
{
    public delegate void ExceuteDelegate();
    /// <summary>
    /// 单线程池， 可以让用户只有单线程在执行，防止多线程出现并发问题
    /// </summary>
    class SingleExecute
    {
        /// <summary>
        /// 互斥锁，就相当于我们
        /// </summary>
        public Mutex mutex;


        public SingleExecute()
        {
            mutex = new Mutex();
        }
        /// <summary>
        /// 单线程池的执行逻辑
        /// </summary>
        /// <param name="executeDelegate"></param>
        public void Execute(ExceuteDelegate executeDelegate) {
            // 就比方说，很多人同时访问的时候，那么我们就直接锁住它
            lock (this)
            {
                // 互斥锁的概念
                mutex.WaitOne();
                executeDelegate();
                mutex.ReleaseMutex();
            }
        }


    }
}
