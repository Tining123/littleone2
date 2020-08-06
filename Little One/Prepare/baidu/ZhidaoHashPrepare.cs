using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Prepare.baidu
{
    public class ZhidaoHashPrepare:OnePrepare
    {
        /// <summary>
        /// 设置初始值
        /// </summary>
        public ZhidaoHashPrepare()
        {
            String[] arr = new string[2];
            arr[0] = "4f00e69d8ee5bda6e5ae8f0000";
            arr[1] = "李彦宏";
            this.start = arr;
            //分配DEAL类
            this.deal = new Deal.baidu.ZhidaoHashDeal();
            //分配任务信息
            this.work_name = "知道HASH";
            this.type_id = 14 + "";
        }

        
        /// <summary>
        /// 加载未完成
        /// </summary>
        /// <returns></returns>
        protected override bool LoadWaiting()
        {
            //进行未完成任务加载
            String sql = "select `name`,`hash` from baidu_user where `hash` is not null and zhidao_hash_done is null and `hash` != '' Limit 0," + keep;
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
