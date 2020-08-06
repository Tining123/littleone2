using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prepare.baidu;
using System.Threading;
using Prepare;
using System.Windows.Forms;
using Little_Net;


namespace Little_One
{
    class Program
    {
        static List<OnePrepare> oplist = new List<OnePrepare>();
        static List<bool> bllist = new List<bool>();

        static bool infofun = false;

        static void Main(string[] args)
        {
            Queue<KeyValuePair<String, String>> ls = new Queue<KeyValuePair<String, String>>();
            
            oplist.Add(new Prepare.baidu.FansPrepare());
            bllist.Add(true);
            oplist.Add(new Prepare.baidu.ZhidaoHashPrepare());
            bllist.Add(true);

            oplist.Add(new PassportPreapare());
            bllist.Add(false);

            oplist.Add(new UserPrepare());
            bllist.Add(true);

            Master mt = new Master();
            mt.oplist = oplist;
            mt.bllist = bllist;
            mt.startMatster();

            for (int i = 0; i < oplist.Count; i++)
                new Thread(new ParameterizedThreadStart(begin)).Start((new pack(oplist[i], bllist[i])));

            while (true)
            {
                control();
            }
            
        }

        //包
        class pack
        {
            public OnePrepare op;
            public bool manual;

            public pack(OnePrepare op, bool manual)
            {
                this.op = op;
                this.manual = manual;
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="obj"></param>
        static void begin(Object obj){
            pack pc = (pack)obj;
            OnePrepare op = pc.op;
            bool manual = pc.manual;
            if (manual)
                op.BuildMission(true);
            else op.BuildMission();
            op.BeginMission();
            new Thread(op.run).Start();
        }

        /// <summary>
        /// 控制
        /// </summary>
        static void control()
        {
            Console.WriteLine("输入H获得帮助");
            String get = "";
            get = Console.ReadLine();
            switch (get)
            {
                case "KP": foreach (OnePrepare op in oplist) op.running = false; break;
                case "KD": foreach (OnePrepare op in oplist) if(op.od != null) op.od.running = false; break;
                case "KDL": foreach (OnePrepare op in oplist) op.deal.running = false; break;

                case "BP": foreach (OnePrepare op in oplist) { op.running = true; new Thread(op.run).Start(); } break;
                case "BD": foreach (OnePrepare op in oplist) { if (op.od != null) { op.od.running = true; op.BeginDown(); } } break;
                case "BDL": foreach (OnePrepare op in oplist) { op.deal.running = true; op.BeginMission(); } break;

                case "O": Tools.Msg.fm = Tools.Msg.FilterMode.only; break;
                case "L": Tools.Msg.fm = Tools.Msg.FilterMode.level; break;
                case "S":
                    {
                        foreach (OnePrepare op in oplist)
                        {
                            Console.WriteLine("{0}#{1}速度:{2}", op.type_id, op.work_name, op.deal.CaculTime());
                        } break;
                    }
                case "I": 
                    {
                        //if (infofun) { infofun = false; }
                        //else { infofun = true; new Thread(inforun).Start(); }
                        int total = 0;
                        foreach (OnePrepare op in oplist)
                        {
                            Console.WriteLine("{0}#{1}任务量:{2}\t 是否完成初始化:{3}"
                                , op.type_id, op.work_name, op.mission.mission_queue.Count, op.finish);
                            total += op.mission.mission_queue.Count;
                        }
                        Console.WriteLine("总任务量:{0}", total);
                        Console.WriteLine("SQL等候量:{0}", Store.StoreQueue.sqllist.Count);
                        //foreach (OnePrepare op in oplist)
                        //{
                        //    if (op is PicPrepare)
                        //        Console.WriteLine("{0}#{1}三层开启情况:P:{2} D:{3} DL:{4}"
                        //            , op.type_id, op.work_name, op.running, op.od.running, op.deal.running);
                        //}
                    }break;
                case "R":
                    {
                        Tools.Msg.fm = Tools.Msg.FilterMode.level;
                        Tools.Msg.open_level = 0;
                    } break;
                case "H":
                    {
                        Console.WriteLine("输入I重置输出队列信息");
                        Console.WriteLine("输入O设置为选择输出");
                        Console.WriteLine("输入L设置为层级输出");
                        Console.WriteLine("输入R重置输出模式");
                        Console.WriteLine("输入S重置输出速度");
                        Console.WriteLine("输入数字控制层级");
                        Console.WriteLine("输入KP关闭准备层");
                        Console.WriteLine("输入KD关闭下载层");
                        Console.WriteLine("输入KDL关闭处理层");
                        Console.WriteLine("输入BP开启准备层");
                        Console.WriteLine("输入BD开启下载层");
                        Console.WriteLine("输入BDL开启处理层");
                    } break;
                default:
                    {
                        try
                        {
                            Tools.Msg.open_level = int.Parse(get);
                        }
                        catch { }
                    } break;

            }

        }

        public static void inforun()
        {
            while (infofun)
            {
                int total = 0;
                foreach (OnePrepare op in oplist)
                {
                    Console.WriteLine("{0}#{1}任务量:{2}\t 是否完成初始化:{3}"
                        , op.type_id, op.work_name, op.mission.mission_queue.Count, op.finish);
                    total += op.mission.mission_queue.Count;
                }
                Console.WriteLine("总任务量:{0}", total);
                Console.WriteLine("SQL等候量:{0}", Store.StoreQueue.sqllist.Count);
                Thread.Sleep(10);
            }
        }
    }
}
