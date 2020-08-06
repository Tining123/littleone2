using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Down
{
    public class PicDown:OneDown
    {
        /// <summary>
        /// 图片下载的根目录
        /// </summary>
        String PicRoot = "Pic";

        /// <summary>
        /// 网站
        /// </summary>
        public String website;

        /// <summary>
        /// 工作类型
        /// </summary>
        public String type_id;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="website">网站名</param>
        /// <param name="type_id">类型ID</param>
        public PicDown(String website, String type_id) { this.website = website; this.type_id = type_id; }

        /// <summary>
        /// 添加到队列
        /// </summary>
        protected override void AddtoQueue()
        {
            String sql = string.Format("select id , type_id from pic_info where website = '{0}' and done = 'done' and empty is null and pic_down_done is null ORDER BY RAND() limit 0,500", website);
            DataSet ds = Store.StoreQueue.Query(type_id,sql);
            if (ds.Tables[0].Rows.Count != 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    queue.Enqueue(new PicDownMission(dr["id"].ToString(), dr["type_id"].ToString()));
                }
            }
        }

        /// <summary>
        /// 检查路径是否存在
        /// </summary>
        /// <param name="obj"></param>
        protected override void checkPath(object obj)
        {
            PicDownMission pdm = (PicDownMission)obj;
            String path = string.Format("{0}/{1}/{2}/{3}", root, PicRoot, pdm.type_id, pdm.id);
            if (Directory.Exists(path) == false)//如果不存在就创建file文件夹
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 获取子任务组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override List<object> GetTarget(object obj)
        {
            PicDownMission pdm = (PicDownMission)obj;
            List<object> list = new List<object>();
            String sql = string.Format("select done,url,id from pic_down where pic_info_id = '{0}' order by id", pdm.id);
            DataSet ds = Store.StoreQueue.Query(type_id, sql);
            if (ds.Tables[0].Rows.Count != 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    list.Add(new PicInfo(dr["done"].ToString(), dr["url"].ToString(),dr["id"].ToString()));
                }
            }
            else
            {
                // 如果没有结果标记为空
                String emptysql = string.Format("update pic_info set empty = 'yes' where id = '{0}'", pdm.id);
                AddtoStore(emptysql);
            }
            return list;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="title"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected override bool Down(object obj, object title, List<object> list)
        {
            PicInfo pi = (PicInfo)obj;
            //防爆措施
            if (pi.url == null || pi.url.Trim().Length == 0)
            {
                String sql = "update pic_down set,empty = 'yes' where id = '" + pi.id + "'";
                AddtoStore(sql);
                return true;
            }
            PicDownMission pdm = (PicDownMission)title;
            if (pi.done != "done")
            {
                //如果是未完成的子任务才进行一系列处理
                String path = string.Format("{0}/{1}/{2}/{3}", root, PicRoot, pdm.type_id, pdm.id); //求路径
                String type = Tools.web.ImageTypeCheck.CheckImageTypeName(pi.url, true);            //求类型   
                String name = string.Format("{0}/{1}.{2}", path, list.IndexOf(obj), type);          //拼合
                Tools.Msg.SendImportantMsg(type_id, String.Format("正在下载{0}", path));
                Stopwatch sw = new Stopwatch(); sw.Start();
                if (type != "None" && Tools.Web.DoGetImage(pi.url, name) == "done")
                {
                    //如果完成下载，计量下载速度
                    sw.Stop(); System.IO.FileInfo pic = new FileInfo(name);
                    Tools.Msg.SendImportantMsg(type_id, String.Format("{0}\t下载完成，下载速度{1} k/s", name,
                        Math.Round(pic.Length / 1024 / sw.Elapsed.TotalSeconds)));
                    //在图片处写入done
                    String sql = "update pic_down set done = 'done' where id = '" + pi.id + "'";
                    AddtoStore(sql);
                    return true;
                }
                else
                {
                    //检测网络
                    if (!Tools.web.CheckNet.IsConnectedInternet())
                    {
                        Tools.Msg.SendNormalMsg(pdm.type_id, String.Format("网络中断！，终止于{0}号{1}型地址为{2}的下载任务，当前下载线程退出", pdm.id, pdm.type_id, pi.url));
                        Tools.Log.log(String.Format("网络中断！，终止于{0}号{1}型地址为{2}的下载任务，当前下载线程退出", pdm.id, pdm.type_id, pi.url), true);
                        this.running = false; return false;
                    }
                    //如果失败，给予三次机会，并检测网络，如果网络错误，直接记录日志并退出程序，如果三次失败，标记失败
                    for (int i = 0; i < 3; i++)
                    {
                        if (type == "None")
                        { type = Tools.web.ImageTypeCheck.CheckImageTypeName(pi.url, true); name = string.Format("{0}/{1}.{2}", path, list.IndexOf(obj), type); }
                        if (type != "None" && Tools.Web.DoGetImage(pi.url, name) == "done")
                        {
                            String sql = "update pic_down set done = 'done' where id = '" + pi.id + "'";
                            AddtoStore(sql);
                            return true;
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 标记完成
        /// </summary>
        /// <param name="title"></param>
        protected override void SignIn(object title)
        {
            PicDownMission pdm = (PicDownMission)title;
            String sql = string.Format("update pic_info set pic_down_done = 'done' where id = '{0}'", pdm.id);
            AddtoStore(sql);
        }

        /// <summary>
        /// 存储到待执行的SQL队列
        /// </summary>
        /// <param name="sql"></param>
        protected void AddtoStore(String sql)
        {
            Store.StoreQueue.AddtoList(type_id, sql);
        }

        /// <summary>
        /// 图片任务包
        /// </summary>
        public class PicDownMission
        {
            /// <summary>
            /// 题目id
            /// </summary>
            public String id;      
 
            /// <summary>
            /// 题目类型id
            /// </summary>
            public String type_id;  

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="id">题目id</param>
            /// <param name="type_id">题目类型id</param>
            public PicDownMission(String id, String type_id)
            { this.id = id; this.type_id = type_id; }
        }

        /// <summary>
        /// 图片信息
        /// </summary>
        public class PicInfo
        {
            /// <summary>
            /// 完成标志
            /// </summary>
            public String done;

            /// <summary>
            /// 地址
            /// </summary>
            public String url;

            /// <summary>
            /// 图片id
            /// </summary>
            public String id;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="done">完成标志</param>
            /// <param name="url">图片地址</param>
            /// <param name="id">图片id</param>
            public PicInfo(String done, String url, String id)
            { this.done = done; this.url = url; this.id = id; }
        }

    }
}
