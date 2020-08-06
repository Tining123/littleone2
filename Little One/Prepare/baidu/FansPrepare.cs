using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Prepare.baidu
{
    public class FansPrepare:OnePrepare
    {
        /// <summary>
        /// 设置初始值
        /// </summary>
        public FansPrepare()
        {
            String[] arr = new string[2];
            arr[0] = "4f00e69d8ee5bda6e5ae8f0000";
            arr[1] = "李彦宏";
            this.start = arr;
            //分配DEAL类
            this.deal = new Deal.baidu.FansDeal();
            //分配任务信息
            this.work_name = "贴吧粉丝";
            this.type_id = 15 + "";
        }

        
        /// <summary>
        /// 加载未完成
        /// </summary>
        /// <returns></returns>
        protected override bool LoadWaiting()
        {
            //进行未完成任务加载
            String sql = "select `hash`,name from baidu_user where fans_done is null and hash is not null and hash != '' Limit 0," + keep;
            DataSet ds = Store.StoreQueue.Query(type_id,sql);
            if (ds.Tables[0].Rows.Count == 0)
                return false;
            else
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    String[] arr = new string[2];
                    arr[0] = dr["hash"].ToString().Split('&')[0];
                    arr[1] = dr["name"].ToString();
                    this.mission.mission_queue.Enqueue(arr);
                }
                mission.mission_queue.Distinct();
                return true;
            }
        }
    }
}
