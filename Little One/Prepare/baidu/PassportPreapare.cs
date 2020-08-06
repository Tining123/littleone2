using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prepare.baidu
{
    public class PassportPreapare:OnePrepare
    {
        public PassportPreapare()
        {
            work_name = "百度个人";
            type_id = "9";
            this.start = "李彦宏";

            deal = new Deal.baidu.PassportDeal();
        }

        /// <summary>
        /// 加载未完成
        /// </summary>
        /// <returns></returns>
        protected override bool LoadWaiting()
        {
            //进行未完成任务加载
            String sql = "select name from baidu_user where passport_done is null  Limit 0," + keep;
            DataSet ds = Store.StoreQueue.Query(type_id, sql);
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
