using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Down
{
    /// <summary>
    /// 下载模板类
    /// </summary>
    public class OneDown
    {
        /// <summary>
        /// 下载根目录
        /// </summary>
        public String root = "Down";

        /// <summary>
        /// 运行状态
        /// </summary>
        public bool running = true;

        /// <summary>
        /// 任务队列
        /// </summary>
        public Queue<Object> queue = new Queue<Object>();

        /// <summary>
        /// 开始下载
        /// </summary>
        public void Begin()
        {
            //进入下载循环
            while (running)
            {
                //初始化题目和子任务组
                Object title = null;
                List<Object> targetlist = new List<object>();

                //如果任务队列数量为0，注入任务
                lock (queue)
                {
                    if (queue.Count == 0)
                        AddtoQueue();
                    if(queue.Count != 0)
                        title = queue.Dequeue();
                }
                if (title == null)
                    { Thread.Sleep(5000); continue; }   //如果没有题目休眠5秒
                targetlist = GetTarget(title);  //获取子任务组

                //检查路径
                checkPath(title);
                //矫正错误的地址或剔除异常地址
                targetlist = CorrectUrl(targetlist);

                //对每一个子任务
                bool success = true;
                foreach(object obj in targetlist)
                    //下载,如果其中某项失败，跳出并放弃计划，拒绝签名
                    if (!Down(obj,title,targetlist))
                        { success = false; break; }

                //如果全员下载成功，标记完成
                if(success)
                    SignIn(title);

            }
        }

        /// <summary>
        /// 加载到队列
        /// </summary>
        protected virtual void AddtoQueue() { }

        /// <summary>
        /// 找到题目下的任务组
        /// </summary>
        /// <param name="obj">题目</param>
        /// <returns>任务组</returns>
        protected virtual List<Object> GetTarget(Object obj) { return null; }

        /// <summary>
        /// 检查路径是否合法
        /// 如果不存在则创建
        /// </summary>
        /// <param name="obj">题目</param>
        protected virtual void checkPath(object obj) { }

        /// <summary>
        /// 纠正有可能错误地址，并删除异常地址
        /// </summary>
        /// <param name="targetlist">子任务组</param>
        /// <returns>如果当前地址可以下载返回true，如果是错误地址返回false</returns>
        protected virtual List<Object> CorrectUrl(List<Object> targetlist) { return targetlist; }

        /// <summary>
        /// 下载任务
        /// </summary>
        /// <param name="obj">子任务</param>
        /// <param name="title">任务包</param>
        /// <param name="list">子任务列表</param>
        /// <returns></returns>
        protected virtual bool Down(object obj,object title,List<object> list) { return true; }

        /// <summary>
        /// 签名确认完成函数
        /// </summary>
        /// <param name="title">题目</param>
        protected virtual void SignIn(object targetlist) { }

    }
}
