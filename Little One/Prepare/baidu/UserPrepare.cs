using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Store;
using System.Data;

namespace Prepare.baidu
{
    /// <summary>
    /// 百度用户名称爬虫，i贴吧开始
    /// </summary>
    public class UserPrepare:OnePrepare
    {
        /// <summary>
        /// 设置初始值
        /// </summary>
        public UserPrepare()
        {
            this.start = "李彦宏";
            //分配DEAL类
            this.deal = new Deal.baidu.UserDeal();
            //分配任务信息
            this.work_name = "贴吧用户";
            this.type_id = 1 + "";
        }

        
        /// <summary>
        /// 加载未完成
        /// </summary>
        /// <returns></returns>
        protected override bool LoadWaiting()
        {
            //进行未完成任务加载
            String sql = "select name from baidu_user where done is null Limit 0," + keep;
            DataSet ds = Store.StoreQueue.Query(type_id,sql);
            if (ds.Tables[0].Rows.Count == 0)
                return false;
            else
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    this.mission.mission_queue.Enqueue(dr["name"]);
                }
                mission.mission_queue.Distinct();
                return true;
            }
        }
    }
}
