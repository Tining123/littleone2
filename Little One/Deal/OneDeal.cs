using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mission_Pack;
using System.Threading;
using System.Diagnostics;

namespace Deal
{
    /// <summary>
    /// 处理类模板
    /// </summary>
    public class OneDeal
    {
        #region 域

        /// <summary>
        /// 休眠时间，毫秒
        /// </summary>
        public int rest_time = 60000;

        /// <summary>
        /// 任务类实体
        /// </summary>
        public OneMission mission = new OneMission();

        /// <summary>
        /// 线程队列
        /// </summary>
        public List<Thread> list = new List<Thread>();

        /// <summary>
        /// 运行标量
        /// </summary>
        public bool running = true;

        /// <summary>
        /// 运算时间持有量
        /// </summary>
        public int keep = 50;

        /// <summary>
        /// 任务消耗时间队列
        /// </summary>
        public Queue<double> time_list = new Queue<double>();

        #endregion

        #region 方法

        /// <summary>
        /// 开始轮询
        /// </summary>
        public void BeginRun()
        {
            //启动初始线程
            run();
            while(running)
            {
                //如果有新线程要求，增加
                if (list.Count < mission.thread_num)
                {
                    Thread newThread = new Thread(run) { Name = getThreadName() };
                }
                else Thread.Sleep(rest_time);
            }
        }

        /// <summary>
        /// 构造一个新的线程名字
        /// </summary>
        /// <returns></returns>
        public String getThreadName()
        {
            for (int i = 0; ; i++)
            {
                bool flag = false;
                foreach (Thread td in list)
                {
                    if (int.Parse(td.Name.Replace("*", "")) == i)
                    {
                        flag = true; break;
                    }
                }
                if(!flag)
                    return i + "*";
            }
        }

        /// <summary>
        /// 轮询
        /// </summary>
        void run()
        {
            //任务量计量
            long count = 0;
            //如果任务队列有任务，取出并进行处理
            while (running)
            {
                if (mission.mission_queue.Count != 0)
                {
                    count++;
                    //处理任务
                    Tools.Msg.SendNormalMsg(mission.type_id, String.Format("{0}#{1}开始处理新条目,本次运行第{2}个...", mission.work_name, mission.type_id,count));
                    Stopwatch sw = new Stopwatch(); sw.Start();
                    if(!Deal(mission.mission_queue.Dequeue()))
                        Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}新条目处理失败", mission.work_name, mission.type_id));
                    AddtoTimeList(sw.Elapsed.TotalMilliseconds);
                    Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}新条目处理完毕，耗时:{2}毫秒", mission.work_name, mission.type_id, sw.Elapsed.TotalMilliseconds));
                    Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}当前处理速度\t{2}毫秒/个", mission.work_name, mission.type_id, CaculTime()));                    
                    //如果当前ID超过线程量，自杀
                    if (Thread.CurrentThread.ManagedThreadId.ToString().Contains("*"))
                    {
                        int nowId = int.Parse(Thread.CurrentThread.ManagedThreadId.ToString().Replace("*", ""));
                        if (nowId > mission.thread_num)
                        {
                            foreach (Thread td in list)
                                if (td.Name == nowId + "*")
                                {
                                    list.Remove(td);
                                    break;
                                }
                            break;
                        }
                    }
                }
                else
                {
                    //Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}暂无任务，休眠{2}秒", mission.work_name, mission.type_id, rest_time / 1000));
                    Thread.Sleep(rest_time);    //如果没有任务休眠5秒
                    //Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}暂无任务，休眠", mission.work_name, mission.type_id));
                    //running = false;
                    
                }
            }
        }

        /// <summary>
        /// 存储到待执行的SQL队列
        /// </summary>
        /// <param name="sql"></param>
        protected void AddtoStore(String sql)
        {
            Store.StoreQueue.AddtoList(mission.type_id,sql);
        }

        /// <summary>
        /// 增加到时间队列
        /// </summary>
        /// <param name="num"></param>
        void AddtoTimeList(double num)
        {
            time_list.Enqueue(num);
            if (time_list.Count > keep)
                time_list.Dequeue();
        }

        /// <summary>
        /// 计算平均运算时间
        /// </summary>
        /// <returns></returns>
        public double CaculTime()
        {
            if (time_list.Count == 0)
                return 0;
            else return time_list.ToList().Average();
        }

        /// <summary>
        /// 处理函数
        /// </summary>
        /// <param name="obj"></param>
        protected virtual bool Deal(object obj)
        {
            return true;
        }

        #endregion
    }
}
