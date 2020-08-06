using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mission_Pack;
using Deal;
using Down;
using System.Threading;

namespace Prepare
{
    /// <summary>
    /// 预备层模版
    /// </summary>
    public class OnePrepare
    {
        #region 域

        /// <summary>
        /// 任务包
        /// </summary>
        public OneMission mission = new OneMission();

        /// <summary>
        /// 任务包完成标量
        /// </summary>
        public bool finish = false;

        /// <summary>
        /// 起点
        /// </summary>
        public Object start = new object();

        /// <summary>
        /// 运行标量
        /// </summary>
        public bool running = true;

        /// <summary>
        /// 缓存持有量
        /// </summary>
        public int keep = 500;

        /// <summary>
        /// 处理类持有实体
        /// </summary>
        public OneDeal deal;

        /// <summary>
        /// 工作名称
        /// </summary>
        public String work_name = "";

        /// <summary>
        /// 工作类型名
        /// </summary>
        public String type_id = "";

        /// <summary>
        /// 下载类实体
        /// </summary>
        public OneDown od;

        #endregion

        #region 方法

        /// <summary>
        /// 开始构建任务包
        /// </summary>
        public void BuildMission()
        {
            //为任务包设置工名工类
            mission.type_id = type_id;
            mission.work_name = work_name;

            Tools.Msg.SendNormalMsg(type_id,String.Format("正在构建{0}#{1}任务包...",work_name,type_id));

            //执行未完成任务加载，如果失败则加入默认起点
            if (!LoadWaiting())
                InitialMission();   
            //完成后将完成标量点亮
            finish = true;

            Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包构建完成", work_name, type_id));
        }

        /// <summary>
        /// 采用手动模式构建任务包
        /// </summary>
        /// <param name="mode"></param>
        public void BuildMission(bool mode = true)
        {
            //为任务包设置工名工类
            mission.type_id = type_id;
            mission.work_name = work_name;

            Tools.Msg.SendNormalMsg(type_id, String.Format("正在构建{0}#{1}任务包...", work_name, type_id));

            ManualInitialMission();
            //完成后将完成标量点亮
            finish = true;

            Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包构建完成", work_name, type_id));
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        public void BeginMission()
        {
            Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包开始执行任务", work_name, type_id));
            deal.mission = this.mission;
            Thread newThread = new Thread(deal.BeginRun);
            newThread.Start();
        }

        /// <summary>
        /// 运行准备包
        /// </summary>
        public void run()
        {
            Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包启动", work_name, type_id));
            while (running)
            {
                Tools.Msg.SendImportantMsg(type_id, String.Format("{0}#{1}任务包缓存检查...", work_name, type_id));
                //如果储存量小于缓存持有量，从数据库获取
                if (mission.mission_queue.Count < keep)
                {
                    //Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包缓存过低，从数据库中调集...", work_name, type_id));
                    LoadWaiting();
                }
                else Thread.Sleep(60000);
            }   
        }

        /// <summary>
        /// 开始下载进程
        /// </summary>
        public virtual void BeginDown()
        {
            new Thread(od.Begin).Start();
        }

        /// <summary>
        /// 加载未完成任务
        /// </summary>
        /// <returns>如果存在未完成任务,则返回true表示加载成功，否则返回false</returns>
        protected virtual bool LoadWaiting()
        {
            return false;
        }

        /// <summary>
        /// 手动封装任务包
        /// </summary>
        protected virtual void ManualInitialMission()
        {
            mission.mission_queue.Enqueue(start);
            Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包构建启用手动模式...", work_name, type_id));
        }

        /// <summary>
        /// 初始化任务包
        /// </summary>
        /// <returns></returns>
        protected virtual bool InitialMission()
        {
            mission.mission_queue.Enqueue(start);
            Tools.Msg.SendNormalMsg(type_id, String.Format("{0}#{1}任务包构建启用初始模式...", work_name, type_id));
            return true;
        }

        #endregion
    }
}
