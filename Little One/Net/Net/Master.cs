using Prepare;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Script.Serialization;  

namespace Little_Net
{
    public class Master
    {
        private static byte[] result = new byte[10240];
        private static int myProt = 8885;   //端口  
        static Socket serverSocket;

        /// <summary>
        /// 是否开启上代理服务器
        /// </summary>
        public bool proxy_open = false;
        //上游服务器端口
        public int proxy_port = 8885;
        //上游服务器地址
        public String proxy_add = "";
        //上游服务器socket
        public Socket proxy_socket = null;

        public List<OnePrepare> oplist = new List<OnePrepare>();
        public List<bool> bllist = new List<bool>();

        public Master() { }

        public void startMatster()
        {
            try
            {
                //如果启动代理服务器模式，先链接上游服务器
                if(proxy_open)
                {
                    try
                    {
                        IPAddress proxy_ip = IPAddress.Parse(proxy_add);
                        proxy_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        proxy_socket.Connect(new IPEndPoint(proxy_ip,proxy_port));
                    }
                    catch { }
                }

                IPAddress ip = IPAddress.Parse("192.168.18.7");
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
                serverSocket.Listen(1000);    //设定最多10个排队连接请求  
                Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
                //通过Clientsoket发送数据  
                Thread myThread = new Thread(ListenClientConnect);
                myThread.Start();
            }
            catch
            {
                startMatster();
            }
        }


        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnect()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept();
                    Thread receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start(clientSocket);
                }
                catch {
                    try
                    {
                        serverSocket.Close();
                        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress ip = IPAddress.Parse("192.168.18.7"); 
                        serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
                        serverSocket.Listen(1000);    //设定最多10个排队连接请求
                        ListenClientConnect();
                    }
                    catch { break; }
                    break;
                }
            }
            ListenClientConnect();
        }

        /// <summary>  
        /// 接收消息  
        /// </summary>  
        /// <param name="clientSocket"></param>  
        private void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    Tools.Msg.SendImportantMsg("0",String.Format("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber)));
                    String get = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    //如果开启了代理服务器，向上游询问
                    if (proxy_open)
                    {
                        proxy_socket.Send(result);
                        byte[] proxy_result = new byte[10240];
                        proxy_socket.Receive(proxy_result);
                        myClientSocket.Send(proxy_result);
                    }
                    else DealMsg(myClientSocket, get);
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="myClientSocket"></param>
        /// <param name="flag"></param>
        private void DealMsg(Socket myClientSocket , String flag)
        {
            Queue<object> list = new Queue<object>();
            Object obj = new object();
            switch(flag)
            {
                case "15": list = oplist[0].mission.mission_queue;break;
                case "14": list = oplist[1].mission.mission_queue;break;
                case "9": list = oplist[2].mission.mission_queue;break;
                case "1": list = oplist[3].mission.mission_queue;break;
            }
            String line = "";
            if (list.Count != 0)
            {
                switch (flag)
                {
                    case "15": line = ObjectToJson<String[]>(list.Dequeue(),"15"); break;
                    case "14": line = ObjectToJson<String[]>(list.Dequeue(),"14"); break;
                    case "9": line = ObjectToJson<String>(list.Dequeue(),"9"); break;
                    case "1": line = ObjectToJson<String>(list.Dequeue(),"1"); break;
                }
                myClientSocket.Send(Encoding.ASCII.GetBytes(line));
            }
            else myClientSocket.Send(Encoding.ASCII.GetBytes("NO"));
        }

        // 从一个对象信息生成Json串
        public string ObjectToJson<T>(object obj,String num)
        {
            try
            {
                Console.WriteLine((String)obj + ";" + num);
            }
            catch { }
            return JsonConvert.SerializeObject((T)obj, new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });

            //try
            //{
            //    Console.WriteLine((String)obj + ";" + num);
            //}
            //catch { }

            //return new JavaScriptSerializer().Serialize(obj);
            
            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //MemoryStream stream = new MemoryStream();
            //serializer.WriteObject(stream, obj);
            //byte[] dataBytes = new byte[stream.Length];
            //stream.Position = 0;
            //stream.Read(dataBytes, 0, (int)stream.Length);
            //return Encoding.UTF8.GetString(dataBytes);
        }


    }
}
