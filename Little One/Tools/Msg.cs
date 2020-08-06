using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class Msg
    {
        /// <summary>
        /// 开放等级
        /// </summary>
        public static int open_level = 0;

        /// <summary>
        /// 是否开启内输出时间
        /// </summary>
        public static bool time_on = true;

        /// <summary>
        /// 是否开启外输出时间
        /// </summary>
        public static bool out_time_on = true;

        /// <summary>
        /// 缓存持有量
        /// </summary>
        public static int keep = 5000;

        /// <summary>
        /// 输出模式
        /// </summary>
        static public FilterMode fm = FilterMode.level;

        /// <summary>
        /// 消息输出委托
        /// </summary>
        public static MsgSendOut mso;

        /// <summary>
        /// 消息列表
        /// 前缀为层数
        /// 次前缀为所属任务号
        /// 1级.任务正在做的事件类型
        /// 2级.任务所做事件信息
        /// 3级.任务所做事件执行过程
        /// </summary>
        static Queue<MsgPack> list = new Queue<MsgPack>();

        /// <summary>
        /// 增加一个消息
        /// </summary>
        /// <param name="level"></param>
        /// <param name="type_id"></param>
        /// <param name="msg"></param>
        public static void SendMsg(int level, String type_id, String msg)
        {
            MsgPack mp = new MsgPack(level,type_id,msg);
            lock (list)
            {
                if (list.Count > keep)
                    list.Dequeue();
                list.Enqueue(mp);
            }

            String msg_temp = msg;
            String out_msg_temp = msg;

            if (time_on)
                msg_temp = DateTime.Now.ToString() + ": " + msg;
            if (out_time_on)
                out_msg_temp = DateTime.Now.ToString() + ": " + msg;

            if (CheckOut(level))
                Console.WriteLine(msg_temp);

            //调用消息发送委托
            if (null != mso)
                mso(level, type_id, out_msg_temp);
        }

        /// <summary>
        /// 检测允许输出
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        static bool CheckOut(int level)
        {
            return fm == FilterMode.level? (level<=open_level? true:false) 
                : (fm == FilterMode.only? (level == open_level? true :false)
                :false); 
        }

        /// <summary>
        /// 发送普通消息
        /// </summary>
        /// <param name="type_id"></param>
        /// <param name="msg"></param>
        public static void SendNormalMsg(String type_id, String msg)
        { SendMsg(1, type_id, string.Format("{0}\t{1}", msg, DateTime.Now)); }
        /// <summary>
        /// 发送重要消息
        /// </summary>
        /// <param name="type_id"></param>
        /// <param name="msg"></param>
        public static void SendImportantMsg(String type_id, String msg)
            { SendMsg(2,type_id,msg); }
        /// <summary>
        /// 发送深层消息
        /// </summary>
        /// <param name="type_id"></param>
        /// <param name="msg"></param>
        public static void SendDeepMsg(String type_id, String msg)
            { SendMsg(3, type_id, msg); }

        /// <summary>
        /// 输出模式枚举类
        /// </summary>
        public enum FilterMode
        {
            /// <summary>
            /// 层级模式,选择后将多层输出
            /// </summary>
            level,
            /// <summary>
            /// 专属模式,选择后将单层输出
            /// </summary>
            only
        }

        /// <summary>
        /// 输出委托模版
        /// </summary>
        /// <param name="level"></param>
        /// <param name="type_id"></param>
        /// <param name="msg"></param>
        public delegate void MsgSendOut(int level, String type_id, String msg);

        /// <summary>
        /// 消息包
        /// </summary>
        public class MsgPack
        {
            int level;          //消息等级
            String type_id;     //工作类型
            String msg;         //消息本体
            DateTime dt = DateTime.Now;//时间

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="level"></param>
            /// <param name="type_id"></param>
            /// <param name="msg"></param>
            public MsgPack(int level, String type_id, String msg)
            {
                this.level = level;
                this.type_id = type_id;
                this.msg = msg;
            }
        }
    }
}
