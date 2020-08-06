using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Deal.baidu
{
    public class PassportDeal : NetDeal
    {
        protected override bool Deal(object obj)
        {
            Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}正在处理\t{2}", mission.work_name, mission.type_id, (String)obj));
            //要获取的值

            String birth = "";  //生日
            String blood = "";  //血型
            String born_place = ""; //出生地
            String live_place = ""; //居住地
            String intro = "";  //介绍
            String body = "";   //体型
            String marry = "";  //婚姻
            String hobby = "";    //爱好
            String character = "";   //性格
            String edu = "";   //教育程度
            String work = "";         //职业
            String conservation = "";   //联系方式
            String edu_background = "";   //教育背景
            String work_info = "";       //工作信息

            String name = (String)obj;//用户名
            //构造HTML,并获取页面
            String url = @"https://www.baidu.com/p/"+ name + "/detail";
            String html = "";
            try
            {
                html = Tools.Web.getHtml(url, "UTF8");
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

            if (html.Contains("生日")) birth = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "生日</span>", "/span>"), "cnt>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("血型")) blood = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "血型</span>", "/span>"), "cnt>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("出生地")) born_place = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "出生地</span>", "/span>"), "cnt>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("居住地")) live_place = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "居住地</span>", "/span>"), "cnt>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("个人简介")) intro = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "个人简介</span>", "/span>"), "cnt>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("体型")) body = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "体型</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("婚姻状态")) marry = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "婚姻状态</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("个人习惯")) hobby = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "个人习惯</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("性格")) character = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "性格</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("教育程度")) edu = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "教育程度</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("当前职业")) work = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "当前职业</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());
            if (html.Contains("联系方式")) conservation = Tools.FileTool.SQLFilter(Tools.Web.cutHtml(Tools.Web.cutHtml(html, "联系方式</span>", "/span>"), "<span>", "<").Replace("&nbsp", "").Replace(" ", "").Replace("\n", " ").Trim());

            if(html.Contains("教育背景"))
                edu_background = Tools.FileTool.SQLFilter(Tools.Web.ReplaceHtmlTag(Tools.Web.cutHtml(html, "教育背景", "</dl>")).Replace("&nbsp", "").Replace(" ", "").Replace("\n", " "));
            if (html.Contains("工作信息"))
                work_info = Tools.FileTool.SQLFilter(Tools.Web.ReplaceHtmlTag(Tools.Web.cutHtml(html, "工作信息", "</dl>")).Replace("&nbsp", "").Replace(" ", "").Replace("\n", " "));

            //存储人信息,这项处理太快，需要权值压缩
            string sql = string.Format("update `baidu_user` set birth = '{0}',blood = '{1}',born_place='{2}',live_place = '{3}',intro = '{4}' , body = '{5}', marry = '{6}',hobby = '{7}', `character` = '{8}',edu= '{9}',`work`='{10}',conservation = '{11}',edu_background = '{12}', work_info = '{13}' , passport_done = 'done' where name = '{14}'"
                , birth, blood, born_place, live_place, intro, body, marry, hobby, character, edu, work, conservation, edu_background, work_info ,name);
            //压栈
            AddtoStore(sql);
            return true;
        }
    }
}
