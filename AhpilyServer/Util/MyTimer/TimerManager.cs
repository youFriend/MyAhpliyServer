using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Concurrent;
using AhpilyServer.Util.Concurrent;

namespace AhpilyServer.Util.MyTimer
{
    /// <summary>
    /// 正在的计时器
    /// </summary>
    class TimerManager
    {
        private static TimerManager _instance = null;

        //定时器
        private Timer timer;
        //线程安全的字典来使用
        //id 和 模型的任务
        private ConcurrentDictionary<int, TimerModel> idModelDict = new ConcurrentDictionary<int, TimerModel>(); 
        //移除的任务列表ID
        private List<int> remove_list = new List<int>();
        //当前添加的 id 
        private ConcurrentInt id = new ConcurrentInt(-1);

        public static TimerManager Instance {
            get {
                //考虑到可能有多个线程同时访问，那么就把这个给锁了
                lock (Instance) {
                    if (_instance == null)
                        _instance = new TimerManager();
                    return _instance;
                }
            }
        }

        public TimerManager() {
            // 服务器每隔一秒就会回调该函数一次
            timer = new Timer(10);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 先把以前的任务给清空掉
            // 但是我们不能够对字典动手
            TimerModel tempModel = null;
            foreach (var id in remove_list)
            {
                idModelDict.TryRemove(id, out tempModel);

            }
            remove_list.Clear();

            // 直接获取内容，通过Values
            foreach (var model in idModelDict.Values)
            {
                if (model.Time <= DateTime.Now.Ticks) {
                    //当当前的时间已经比 我们设定的时间大了，就执行
                    model.Run();
                }
            }
        }

        /// <summary>
        /// 指定时间定时任务，添加定时任务
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeDelegate"></param>
        public void AddTimeEvent(DateTime dateTime, TimeDelegate timeDelegate)
        {
            long delayTime = dateTime.Ticks - DateTime.Now.Ticks;
            if (delayTime <= 0)
            {
                return;
            }
            AddTimeEvent(delayTime, timeDelegate);
        }
        /// <summary>
        /// 延迟执行 定时任务
        /// </summary>
        /// <param name="delayTime">毫秒(必须大于10)</param>
        /// <param name="timeDelegate"></param>
        public void AddTimeEvent(long delayTime, TimeDelegate timeDelegate) {
            //TODO id 锁
            TimerModel timeModel = new TimerModel(id.Add(),DateTime.Now.Ticks+delayTime,timeDelegate);
            //
            idModelDict.TryAdd(timeModel.id, timeModel);
        }
        //

    }
}
