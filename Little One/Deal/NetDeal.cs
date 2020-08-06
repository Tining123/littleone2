using Mission_Pack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Newtonsoft;
using Newtonsoft.Json;

namespace Deal
{
    public class NetDeal
    {
        #region 域

        /// <summary>
        /// 休眠时间，毫秒
        /// </summary>
        public int rest_time = 60000;

        /// <summary>
        /// 任务类实体
        /// </summary>
        public OneMission mission = new OneMission();

        /// <summary>
        /// 线程队列
        /// </summary>
        public List<Thread> list = new List<Thread>();

        /// <summary>
        /// 运行标量
        /// </summary>
        public bool running = true;

        /// <summary>
        /// 运算时间持有量
        /// </summary>
        public int keep = 50;

        /// <summary>
        /// 任务消耗时间队列
        /// </summary>
        public Queue<double> time_list = new Queue<double>();

        #endregion

        #region 方法

        /// <summary>
        /// 开始轮询
        /// </summary>
        public void BeginRun()
        {
            //启动初始线程
            try
            {
                run();
            }
            catch (Exception e)
            {
                ding(e); 
            }
        }

        void ding(Exception e)
        {
            BeginRun();
        }

        /// <summary>
        /// 构造一个新的线程名字
        /// </summary>
        /// <returns></returns>
        public String getThreadName()
        {
            for (int i = 0; ; i++)
            {
                bool flag = false;
                foreach (Thread td in list)
                {
                    if (int.Parse(td.Name.Replace("*", "")) == i)
                    {
                        flag = true; break;
                    }
                }
                if (!flag)
                    return i + "*";
            }
        }


        private  byte[] result = new byte[10240]; 

        public void run()
        {
            //设定服务器IP地址  
            IPAddress ip = IPAddress.Parse("192.168.18.7");
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 8885)); //配置服务器IP与端口  
                Console.WriteLine("连接服务器成功");
            }
            catch
            {
                clientSocket.Close();
                Console.WriteLine("连接服务器失败，请按回车键退出！");
                throw;
            }
            try
            {
                //任务量计量
                long count = 0;
                //如果任务队列有任务，取出并进行处理
                while (running)
                {
                    clientSocket.Send(Encoding.ASCII.GetBytes(mission.type_id));
                    int receiveLength = clientSocket.Receive(result);
                    if (receiveLength == 0)
                        continue;
                    String line = Encoding.ASCII.GetString(result, 0, receiveLength);

                    if (line != "NO")
                    {
                        count++;
                        //处理任务
                        Tools.Msg.SendNormalMsg(mission.type_id, String.Format("{0}#{1}开始处理新条目,本次运行第{2}个...", mission.work_name, mission.type_id, count));
                        Stopwatch sw = new Stopwatch(); sw.Start();
                        bool dealResult = false;
                        switch (mission.type_id)
                        {
                            case "15": dealResult = Deal(JsonToObject<String[]>(line)); break;
                            case "14": dealResult = Deal(JsonToObject<String[]>(line)); break;
                            case "9": dealResult = Deal(JsonToObject<String>(line)); break;
                            case "1": dealResult = Deal(JsonToObject<String>(line)); break;
                        }
                        if (!dealResult)
                            Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}新条目处理失败", mission.work_name, mission.type_id));
                        AddtoTimeList(sw.Elapsed.TotalMilliseconds);
                        Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}新条目处理完毕，耗时:{2}毫秒", mission.work_name, mission.type_id, sw.Elapsed.TotalMilliseconds));
                        Tools.Msg.SendImportantMsg(mission.type_id, String.Format("{0}#{1}当前处理速度\t{2}毫秒/个", mission.work_name, mission.type_id, CaculTime()));
                        //如果当前ID超过线程量，自杀
                        if (Thread.CurrentThread.ManagedThreadId.ToString().Contains("*"))
                        {
                            int nowId = int.Parse(Thread.CurrentThread.ManagedThreadId.ToString().Replace("*", ""));
                            if (nowId > mission.thread_num)
                            {
                                foreach (Thread td in list)
                                    if (td.Name == nowId + "*")
                                    {
                                        list.Remove(td);
                                        break;
                                    }
                                break;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(rest_time);    //如果没有任务休眠5秒
                    }
                }
            }
            catch { throw; }
        }

        // 从一个Json串生成对象信息
        public object JsonToObject<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);  


            //JavaScriptSerializer jss = new JavaScriptSerializer();
            //T ss = jss.Deserialize<T>(jsonString);

            //return ss;

            //T obj = Activator.CreateInstance<T>();
            //using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            //{
            //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //    return (T)serializer.ReadObject(ms);
            //}  

            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            //Object objback = serializer.ReadObject(mStream);
            //return objback;
        }

        /// <summary>
        /// 存储到待执行的SQL队列
        /// </summary>
        /// <param name="sql"></param>
        protected void AddtoStore(String sql)
        {
            Store.StoreQueue.AddtoList(mission.type_id, sql);
        }

        /// <summary>
        /// 增加到时间队列
        /// </summary>
        /// <param name="num"></param>
        protected void AddtoTimeList(double num)
        {
            time_list.Enqueue(num);
            if (time_list.Count > keep)
                time_list.Dequeue();
        }

        /// <summary>
        /// 计算平均运算时间
        /// </summary>
        /// <returns></returns>
        public double CaculTime()
        {
            if (time_list.Count == 0)
                return 0;
            else return time_list.ToList().Average();
        }

        /// <summary>
        /// 处理函数
        /// </summary>
        /// <param name="obj"></param>
        protected virtual bool Deal(object obj)
        {
            return true;
        }

        #endregion
    }
}
