using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading;

namespace Store
{
    public static class StoreQueue
    {
        #region 域

        /// <summary>
        /// SQL队列
        /// </summary>
        public static Queue<KeyValuePair<String,String>> sqllist = new Queue<KeyValuePair<string,string>>();
        
        /// <summary>
        /// 运行状态
        /// </summary>
        public static bool running = false;

        /// <summary>
        /// 数据库连接串
        /// </summary>
        private static String sqlstr = "Database=little;Data Source=localhost;User Id=root;Password=123456;Connection Timeout=9999999";

        /// <summary>
        /// 数据库同时运行数
        /// </summary>
        public static int runnum = 1;

        /// <summary>
        /// 保持sql上线
        /// </summary>
        public static int keep = 20000;

        /// <summary>
        /// 等待延时
        /// 0则是死询
        /// </summary>
        public static int wait = 0; 

        /// <summary>
        /// 链接实体
        /// </summary>
        public static MySqlConnection mc = new MySqlConnection(sqlstr);

        #endregion

        #region 方法 

        /// <summary>
        /// 添加到处理列表
        /// </summary>
        /// <param name="sqlstr"></param>
        public static void AddtoList(String type_id,String sqlstr)
        {
            if (running == false) 
            {
                running = true;
                for (int i = 0; i < runnum; i++)
                {
                    Thread newThread = new Thread(run);
                    newThread.Start();
                }
            }
            //尝试向第一段队列加入
            try
            {
                lock (sqllist)
                {
                    if (sqllist.Count >= keep)
                        throw new Exception();
                    sqllist.Enqueue(new KeyValuePair<String, String>(type_id, sqlstr));
                }
            }
            catch { 
                Tools.Msg.SendNormalMsg(type_id, string.Format("SQL队列饱和，{0}#进入30秒休眠后重试",type_id));
                //Tools.Log.log(string.Format("SQL队列饱和，{0}#进入30秒休眠后重试", type_id), true);
                while (sqllist.Count >= keep)
                    Thread.Sleep(30000);
                lock (sqllist)
                {
                    sqllist.Enqueue(new KeyValuePair<String, String>(type_id, sqlstr));
                }
            }
        }

        /// <summary>
        /// 运行轮询
        /// </summary>
        public static void run()
        {
            while (running)
            {
                bool work = false;
                String dealstr = null;
                lock (sqllist)
                {
                    if (sqllist.Count != 0)
                    {
                        lock (sqllist)
                        {
                            dealstr = sqllist.Dequeue().Value;
                        }
                        work = true;
                    }
                }
                ////
                //**
                //**    有时候会出现莫名的kvp双空现象，并且是跳过生成阶段，暂时没有解决
                //**
                ////
                if (work && dealstr != null)
                    Deal(dealstr);
                else
                  Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 执行数据操作
        /// </summary>
        /// <param name="sql"></param>
        public static void Deal(String sql)
        {
            Tools.Msg.SendDeepMsg("0",String.Format("正在执行SQL: {0}",sql));
            if (mc.State == ConnectionState.Closed)
                mc.Open();
            MySqlCommand cmd = new MySqlCommand(sql, mc); cmd.CommandTimeout = wait;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch {
                //
                Tools.Log.log("SQL执行失败: " + sql, true);
                //Queue<KeyValuePair<String, String>> templist = new Queue<KeyValuePair<string, string>>();
                //templist.Enqueue(new KeyValuePair<string, string>("-1", sql));
                //lock (sqllist)
                //{
                //    foreach (KeyValuePair<string, string> kvp in sqllist)
                //        templist.Enqueue(kvp);
                //}
                //sqllist = templist;
            }
            cmd.Dispose();
        }

        /// <summary>
        /// 查询事物
        /// </summary>
        /// <returns></returns>
        public static DataSet Query(String type_id,String sql)
        {

            //Tools.Msg.SendDeepMsg(type_id, String.Format("正在查询SQL: {0}", sql));
            DataSet temp = new DataSet();
            MySqlConnection mc = getConn();
            MySqlCommand cmd = new MySqlCommand(sql, mc); cmd.CommandTimeout = wait;
            MySqlDataAdapter msd = new MySqlDataAdapter(sql, mc);
            try
            {
                msd.Fill(temp);
            }
            catch { temp.Tables.Add(new DataTable()); Thread.Sleep(3000); return temp; }
            msd.Dispose();
            cmd.Dispose();
            mc.Close();
            return temp;
        }

        private static MySqlConnection getConn()
        {
            //return (MySqlConnection)pool.GetConnectionFormPool(new Object()); 

            MySqlConnection mc = new MySqlConnection(sqlstr);
            mc.Open();
            return mc;
        }

        #endregion
    }
}
