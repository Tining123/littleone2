using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// 日志处理工具
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 写入日志并且自带时间格式
        /// </summary>
        /// <param name="info">日志信息</param>
        /// <param name="time">是否开启时间格式</param>
        public static void log(String info, Boolean time = true)
        {
            if (time)
                log(DateTime.Now.ToString() + ": " + info);
            else log(info);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="info">写入日志的信息</param>
        public static void log(String info)
        {
            //关闭日志系统
            //重要
            return;

            try
            {
                //如果没有log文件夹就创建
                if (Directory.Exists("log"))
                {
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo("log");
                    directoryInfo.Create();
                }

                //checkLog();
                FileStream fs = new FileStream("log/log.txt", FileMode.Append);
                //如果日志文件大于500k则建立一个新的并且把原来的备份
                if (fs.Length > 1024 * 1024 / 2)
                {
                    //生成随机防混码
                    Random ran = new Random();
                    int RandKey = ran.Next(10000, 99999);

                    String newName = "log/log" + ("" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day
                        + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + RandKey) + @".txt";

                    fs.Close();
                    System.IO.File.Copy("log/log.txt", newName);
                    System.IO.File.Delete("log/log.txt");

                    fs = new FileStream("log/log.txt", FileMode.Create);
                }
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(info);
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                try
                {
                    FileStream fs = new FileStream("log/loglog.txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(DateTime.Now.ToString() + "    日志写入错误     " + info);
                    sw.WriteLine(e.ToString());
                    sw.Close();
                    fs.Close();
                }
                catch { }
            }
        }
    }
}
