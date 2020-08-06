using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace Deal.baidu
{
    public class ZhidaoHashDeal : NetDeal
    {
        /// <summary>
        /// 处理项
        /// </summary>
        /// <param name="obj"></param>
        protected override bool Deal(object obj)
        {
            //0是hash，1是name
            String[] hashobj = (String[])obj;
            String url = "https://zhidao.baidu.com/usercenter/ajax/getrelationuserlist?type=1&status=2&pn5&rn=99999999999&uid=" + hashobj[0];
            if (!dealjson(url)) return false; 
            url = "https://zhidao.baidu.com/usercenter/ajax/getrelationuserlist?type=1&status=1&pn5&rn=99999999999&uid=" + hashobj[0];
            if (!dealjson(url)) return false;
            //标记处理完毕
            String sql = "update `baidu_user` set zhidao_hash_done = 'done' where name = '" + hashobj[1] + "'";
            AddtoStore(sql);

            return true;
        }

        private bool dealjson(String url)
        {
            List<String> namelist = new List<string>();
            List<String> hashlist = new List<string>();
            List<String> tag = new List<string>();

            String html = "";
            //下载页面
            try
            {
                html = Tools.Web.getHtml(url,"UTF8");
            }
            catch
            {
                //如果被拒绝，则暂停十秒
                //mission.mission_queue.Enqueue(name);
                Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}对象'{2}'网页下载失败,休眠十秒", mission.work_name, mission.type_id, url));
                Tools.Msg.SendImportantMsg(mission.type_id, String.Format("网页地址{0}", url));
                String expSql = string.Format("insert into exp(type_id,msg) values('{0}','{1}')", mission.type_id, url + "#" + url);
                Tools.Log.log(String.Format("{0}#{1}任务失败,无法下载网页:{2},{3}", mission.work_name, mission.type_id, url, url), true);
                Thread.Sleep(10000);
                return false;
            }
            String cuthtml = Tools.Web.cutHtml(html,@"list",@"viewer");
            namelist = Tools.Web.matchHtml(cuthtml, @"uname", @",");
            hashlist = Tools.Web.matchHtml(cuthtml, @"imId", @",");
            tag = Tools.Web.matchHtml(cuthtml, @"tags", "]");
            //过滤
            for (int i = 0; i < tag.Count; i++)
            {
                namelist[i] = Tools.Web.unicode_to_chinese(namelist[i]).Replace(":\"", "").Replace("\"", "");
                //namelist[i] = namelist[i].Substring(1,namelist[i].Length - 1);
                hashlist[i] = Tools.Web.unicode_to_chinese(hashlist[i]).Replace(":\"", "").Replace("\"", "");
                tag[i] = tag[i].Replace("\"", "").Replace(",", " ").Replace("\n", "").Replace("\r", "").Replace("\r\n", "").Replace("[","");
            }
            String sql = "replace into `baidu_user`(name,hash) values";
            if (namelist.Count != 0)
            {
                String temp = "";
                temp += ("('" + namelist[0] + "','" + hashlist[0] + "')");
                for (int i = 1; i < namelist.Count; i++)
                    temp += (",('" + namelist[i] + "','" + hashlist[i] + "')");
                sql += temp;
                AddtoStore(sql);
            }

            return true;
        }

    
        
    }
}
