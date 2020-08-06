using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Deal.baidu
{
    public class FansDeal:NetDeal
    {
        protected override bool Deal(object obj)
        {
            //0是hash，1是name
            String[] hashobj = null;
            try
            {
                hashobj = (String[])obj;
            }
            catch { return false; }
            String hash = hashobj[0];
            String name = hashobj[1];
            if (!dealnet("fans", hash, name))
                return false;
            if (!dealnet("concern", hash, name))
                return false;
            //标记完成
            String sql = "update `baidu_user` set fans_done = 'done' where name = '" + name + "'";
            AddtoStore(sql);
            return true;

        }

        private bool dealnet(String target, String hash, String name)
        {
            for (int i = 1; i < 26; i++)
            {
                //名字列表
                List<String> namelist = new List<string>();

                //构造HTML,并获取页面
                String url = @"http://tieba.baidu.com/i/i/" + target + "?u=" + hash + "&pn=" + i;
                String html = "";
                try
                {
                    html = Tools.Web.getHtml(url, "UTF8", "tieba.baidu.com", "BDUSS=VBTM3dJLUxoU35KVVA0RH5vR1Z3UDJuaXNjbFdnekIyflVLaTJZNXNnN3JpQlpaSVFBQUFBJCQAAAAAAAAAAAEAAABh~g0huMfKwNG8zfUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAOv77ljr--5Yc; STOKEN=be3bb3d08211cd0c18291ebca467ad032d5a4dd6ce4ff4b8c6645f5372f22fd8; PSINO=1; H_PS_PSSID=22162_1446_21126_17001_22175_20927");
            
                }
                catch
                {
                    //如果被拒绝，则暂停十秒
                    mission.mission_queue.Enqueue(name);
                    Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}对象'{2}'网页下载失败,休眠十秒", mission.work_name, mission.type_id, name));
                    Tools.Msg.SendImportantMsg(mission.type_id, String.Format("网页地址{0}", url));
                    String expSql = string.Format("insert into exp(type_id,msg) values('{0}','{1}')", mission.type_id, name + "#" + url);
                    Tools.Log.log(String.Format("{0}#{1}任务失败,无法下载网页:{2},{3}", mission.work_name, mission.type_id, name, url), true);
                    Thread.Sleep(10000);
                    return false;
                }
                String cuthml = Tools.Web.cutHtml(html, "main_wrapper", "main_aside");
                //如果不存在balnk既名字标签属性，则跳出
                if (!cuthml.Contains("name_show="))
                    break;
                //如果包含标签，则开始匹配
                namelist = Tools.Web.matchHtml(cuthml, @"name_show=""", @"""");
                if (namelist.Count == 0)
                    break;
                else
                {
                    //先添加到出口
                    String sql = "insert ignore into `baidu_user`(name) values ";
                    String tempsql = "('" + namelist[0] +"')";
                    for(int j = 1; j < namelist.Count;j++)
                        tempsql += ",('" + namelist[j] + "')";
                    sql += tempsql;
                    AddtoStore(sql);

                    //添加到好友关系
                    sql = "insert ignore into `fans_star`(fans_name,star_name) values ";
                    tempsql = "";
                    if(target == "fans")
                    {
                        tempsql += "('" + namelist[0] + "','" + name + "')";
                        for (int j = 1; j < namelist.Count; j++)
                        {
                            if(namelist[j] != name)
                                tempsql += ",('" + namelist[j] + "','" + name + "')";
                        }
                    }
                    else if (target == "concern")
                    {
                        tempsql += "('" + name +"','" + namelist[0] + "')";
                        for (int j = 1; j < namelist.Count; j++)
                        {
                            if (namelist[j] != name)
                                tempsql += ",('" + name + "','" + namelist[j] + "')";
                        }
                    }
                    sql += tempsql;
                    AddtoStore(sql);
                }
            }

            return true;
        }
    }
}
