using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mission_Pack
{
    /// <summary>
    /// 任务包层模板
    /// </summary>
    public class OneMission
    {
        #region 域

        /// <summary>
        /// 额外进程数
        /// </summary>
        public int thread_num = 0;

        /// <summary>
        /// 任务队列
        /// </summary>
        public Queue<Object> mission_queue = new Queue<Object>();

        /// <summary>
        /// 工作名称
        /// </summary>
        public String work_name = "";

        /// <summary>
        /// 工作类型名
        /// </summary>
        public String type_id = "";

        #endregion
    }
}
