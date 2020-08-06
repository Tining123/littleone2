using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tools;

namespace Deal.baidu
{
    public class UserDeal : NetDeal
    {

        /// <summary>
        /// 处理项
        /// </summary>
        /// <param name="obj"></param>
        protected override bool Deal(object obj)
        {
            Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}正在处理\t{2}", mission.work_name, mission.type_id, (String)obj));
            //要获取的值
            String sex = "";    //性别
            String year = "";   //吧龄
            String post = "";   //发贴数
            String ID = "";         //用户ID
            String fans_num = "";   //粉丝数
            String star_num = "";   //关注数
            String hash = "";       //哈希值

            List<String> outter = new List<string>();   //出口
            List<String> bar = new List<string>();      //贴吧

            String name = (String)obj;//用户名

            //构造HTML,并获取页面
            String url = @"http://tieba.baidu.com/home/main/?un=" + name;
            String html = "";
            try
            {
                html = Tools.Web.getHtml(url, "UTF8");
            }
            catch 
            { 
                //如果被拒绝，则暂停十秒
                mission.mission_queue.Enqueue(name);
                Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}对象'{2}'网页下载失败,休眠十秒", mission.work_name, mission.type_id,name));
                Tools.Msg.SendImportantMsg(mission.type_id,String.Format("网页地址{0}",url));
                String expSql = string.Format("insert into exp(type_id,msg) values('{0}','{1}')", mission.type_id, name + "#" + url);
                Tools.Log.log(String.Format("{0}#{1}任务失败,无法下载网页:{2},{3}",mission.work_name,mission.type_id,name,url),true);
                Thread.Sleep(10000);
                return false;
            }

            //进行第一次切割
            String Cut = Tools.Web.cutHtml(html, "j_userhead", "ihome");   //不确定
            //进行第一次匹配   
            List<String> temp = Tools.Web.matchHtml(Cut,"userinfo_sex ",">");  //获取
            sex = temp.Count != 0 ? temp[0].Trim().Replace("userinfo_sex_", "").Replace("\"","") : "";

            temp = Tools.Web.matchHtml(Cut, "吧龄:", "</span>");  //获取吧龄
            if (temp.Count != 0)
            {
                if (temp[0].Contains("年"))
                    temp[0] = temp[0].Replace("年","");
                year = temp[0].Trim();
            }

            temp = Tools.Web.matchHtml(Cut, "发贴:", "</span>");  //获取发帖数
            post = temp.Count != 0 ? (temp[0].Contains("万") ? double.Parse(temp[0].Replace("万","")) * 10000 + "" : temp[0])
                : "";
            temp = Tools.Web.matchHtml(Cut, "/im/pcmsg", "target");  //获取ID
            ID = temp.Count != 0 ? temp[0].Trim().Replace("?from=","").Replace("\"","") : "";

            //再次切割
            Cut = Tools.Web.cutHtml(html, "right_aside", "贴吧协议");
            //再次匹配
            temp = Tools.Web.matchHtml(Cut, "最近来访", "<span");  //获取最近来访作为出口
            if (temp.Count != 0)
            {
                temp = Tools.Web.matchHtml(temp[0], "un=", "&fr=home");
                foreach (String str in temp)
                    outter.Add(str);
            }

            temp = Tools.Web.matchHtml(Cut, "他关注的人<span", "<span");  //获取关注的人
            if(temp.Count == 0)
                temp = Tools.Web.matchHtml(Cut, "她关注的人<span", "<span");  //获取关注的人
            if(temp.Count != 0)
            {
                hash = Tools.Web.matchHtml(temp[0], "id=", "t=")[0].Replace("?", "");
                star_num = Tools.Web.matchHtml(temp[0], "_blank\">", "</a>")[0];

                temp = Tools.Web.matchHtml(temp[0], "un=", "&fr=home");
                foreach (String str in temp)
                    outter.Add(str);
            }

            if (Cut.Contains("关注他的人"))
            {
                String sub = Cut.Substring(Cut.IndexOf("关注他的人"), Cut.Length - Cut.IndexOf("关注他的人"));
                hash = Tools.Web.matchHtml(sub, "id=", "t=")[0].Replace("?", "");
                temp = Tools.Web.matchHtml(sub, "_blank\">", "</a>");
                if (temp.Count != 0)
                    fans_num = temp[0];

                temp = Tools.Web.matchHtml(sub, "un=", "&fr=home");
                foreach (String str in temp)
                    outter.Add(str);
            }
            if (Cut.Contains("关注她的人"))
            {
                String sub = Cut.Substring(Cut.IndexOf("关注她的人"), Cut.Length - Cut.IndexOf("关注她的人"));
                hash = Tools.Web.matchHtml(sub, "id=", "t=")[0].Replace("?", "");
                temp = Tools.Web.matchHtml(sub, "_blank\">", "</a>");
                if (temp.Count != 0)
                    fans_num = temp[0];

                temp = Tools.Web.matchHtml(sub, "un=", "&fr=home");
                foreach (String str in temp)
                    outter.Add(str);
            }

            //第三次切割
            Cut = Tools.Web.cutHtml(html, "ihome_title", "right_aside");
            //第三次匹配
            temp = Tools.Web.matchHtml(Cut, "unsign\"", "</span>"); //获取贴吧
            if (temp.Count != 0)
                foreach (String str in temp)
                {
                    if (str.Contains("img"))
                        bar.Add(Tools.Web.ReplaceHtmlTag(str));
                    else
                        bar.Add(Tools.FileTool.SQLFilter(str.Replace("<span>", "").Replace(">", "")));
                }
            //构造SQL
            String sql = "";
            //存储出口
            if (outter.Count != 0)
            {
                sql = "insert IGNORE into `baidu_user` (name) values";
                String tempStr = string.Format("('{0}')", outter[0]);
                for (int i = 1; i < outter.Count; i++)
                    tempStr += string.Format(" ,('{0}')", outter[i]);
                sql += tempStr;
                AddtoStore(sql);
            }
            //存储贴吧
            if(bar.Count != 0)
            {
                sql = "insert IGNORE into `bar` (user_name,bar_name) values";
                String tempStr = string.Format("('{0}', '{1}')", name, bar[0]);
                for (int i = 1; i < bar.Count; i++)
                    tempStr += string.Format(" ,('{0}','{1}')", name, bar[i]);
                sql += tempStr;
                AddtoStore(sql);
            }
            //存储人信息
            sql = string.Format(" REPLACE into `baidu_user` (ID, name , year, post_num,fans_num,star_num,sex,done )values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');",
                ID, name, year, post, fans_num, star_num, sex, "done");
            //压栈
            AddtoStore(sql);
            //如果hash为空，更新
            if (hash.Trim().Length != 0)
                sql = string.Format("update `baidu_user` set `hash` = '{0}' where ID = '{1}' and name = '{2}' and (hash is null or hash = '')", hash, ID, name);
            //删除在缓存中的自身
            //sql = "delete from wait where type_id = '" + type_id + "' and msg = '" + name + "' ";
            
            return true;
        }
    }
}
