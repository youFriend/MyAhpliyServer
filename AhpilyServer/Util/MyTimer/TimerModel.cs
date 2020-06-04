using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer.Util.MyTimer
{
    /// <summary>
    /// 定时器到达时间后定时发生
    /// </summary>
    public delegate void TimeDelegate();

    /// <summary>
    /// 定时任务模型
    /// </summary>
    class TimerModel
    {
        //唯一标识
        public int id;
        // 任务执行的事件
        public long Time;
        //定时任务触发
        public TimeDelegate timeDelegate;
        public TimerModel(int id, long time ,TimeDelegate td) {
            this.id = id;
            this.Time = time;
            this.timeDelegate = td;
        }

        /// <summary>
        /// 触发任务
        /// </summary>
        public void Run() {
            timeDelegate();
        }


    }
}
