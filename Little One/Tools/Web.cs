using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// 网页处理工具
    /// </summary>
    public class Web
    {  
        /// <summary>
        /// 获取html内容
        /// 扒取个人中心时用default
        /// 扒取用户列表型时用utf-8
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <returns>网页内容</returns>
        public static String getHtml(String url,String encoding)
        {
            string htmlStr = "";
            if (!String.IsNullOrEmpty(url))
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
                //request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Win64; x64; Trident/5.0)";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

                //Accept-Encoding:gzip, deflate, sdch, br
                //Accept-Language:zh-CN,zh;q=0.8
                //Cache-Control:no-cache
                //Connection:Upgrade
                //Host:gc.kis.scr.kaspersky-labs.com
                //Origin:https://www.zhihu.com
                //Pragma:no-cache
                //Sec-WebSocket-Extensions:permessage-deflate; client_max_window_bits
                //Sec-WebSocket-Key:JmWCL3k7uQn8lGXo1x02Bg==
                //Sec-WebSocket-Version:13
                //Upgrade:websocket
                //User-Agent:Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36

                WebResponse response = null;
                try
                {
                    response = request.GetResponse();           //创建WebResponse对象  
                }
                catch { throw; }
                Stream datastream = response.GetResponseStream();       //创建流对象  
                Encoding ec = Encoding.Default;
                if (encoding == "UTF8")
                {
                    ec = Encoding.UTF8;
                }
                else if (encoding == "Default")
                {
                    ec = Encoding.Default;
                }
                else if (encoding == "GB2312")
                {
                    ec = Encoding.GetEncoding("gb2312"); ;
                }
                else if (encoding == "unicode")
                {
                    ec = Encoding.GetEncoding("Unicode"); ;
                }
                else if (encoding == "Big5")
                {
                    ec = Encoding.GetEncoding("Big5"); ;
                }
                
                StreamReader reader = new StreamReader(datastream, ec);
                htmlStr = reader.ReadToEnd();                           //读取数据  
                reader.Close();
                datastream.Close();
                response.Close();
            }
            //延时
            //Thread.Sleep(1000);
            return htmlStr;
        }

        /// <summary>
        /// 获取html内容
        /// 扒取个人中心时用default
        /// 扒取用户列表型时用utf-8
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <returns>网页内容</returns>
        /// <param name="host">主机域名</param>
        /// <param name="cookie">cookie串</param>
        /// <returns></returns>
        public static String getHtml(String url, String encoding,String host,String cookie)
        {
            //@"z_c0=Mi4wQUJDQTBuMnhCZ29BWUVEU0xoQ0pDaVlBQUFCZ0FsVk50MnNUV1FEcTlQUUVBbjh4Qmc0b2JWbDZjYlVSZjVycHJ3|1491853762|b270151febe389900cc962dc76fa98d53c69ce84"
            string htmlStr = "";
            if (!String.IsNullOrEmpty(url))
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
                //request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Win64; x64; Trident/5.0)";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
                request.Host = host;
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(new Uri("http://" + host), cookie);
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

                WebResponse response = null;
                try
                {
                    response = request.GetResponse();           //创建WebResponse对象  
                }
                catch { throw; }
                Stream datastream = response.GetResponseStream();       //创建流对象  
                Encoding ec = Encoding.Default;
                if (encoding == "UTF8")
                {
                    ec = Encoding.UTF8;
                }
                else if (encoding == "Default")
                {
                    ec = Encoding.Default;
                }
                else if (encoding == "GB2312")
                {
                    ec = Encoding.GetEncoding("gb2312"); ;
                }
                StreamReader reader = new StreamReader(datastream, ec);
                htmlStr = reader.ReadToEnd();                           //读取数据  
                reader.Close();
                datastream.Close();
                response.Close();
            }
            //延时
            //Thread.Sleep(1000);
            return htmlStr;
        }

        /// <summary>
        /// 分割html
        /// </summary>
        /// <param name="html">html原文</param>
        /// <param name="begin">开头</param>
        /// <param name="end">结尾</param>
        /// <returns>分割后的html</returns>
        public static String cutHtml(String htmlStr, String begin, String end)
        {
            //掐头
            int index = htmlStr.IndexOf(begin);
            if (index == -1)
                return htmlStr;
            String cutB = htmlStr.Substring(index + begin.Length, htmlStr.Length - index - begin.Length);

            //去尾
            index = cutB.IndexOf(end);
            if (index == -1)
                return htmlStr;
            String cutBE = cutB.Substring(0, index);    //获取去尾后的文段

            return cutBE;
        }

        /// <summary>
        /// 获得两个字符串之间的值
        /// 将在给予的文本中进行多次匹配
        /// </summary>
        /// <param name="html">查询文本</param>
        /// <param name="begin">起始字符</param>
        /// <param name="end">结束字符</param>
        /// <returns>查询结果</returns>
        public static List<String> matchHtml(String html, String begin, String end)
        {
            string pattern = string.Format(@"{0}(.*?){1}", begin, end);        //正则匹配
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(html);

            List<String> back = new List<String>();               //初始化返回列表

            //将匹配结果导入列表
            for (int i = 0; i < matches.Count; i++)
            {
                back.Add(matches[i].Groups[1].Value.ToString());
            }
            return back;
        }

        /// <summary>
        /// 取出HTML标签
        /// </summary>
        /// <param name="html"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }

        /// <summary>
        /// 获取一个图片
        /// </summary>
        /// <param name="url">目标路径</param>
        /// <param name="path">存储路径</param>
        /// <returns>结果码</returns>
        public static string DoGetImage(string url, string path)
        {
            //如果文件存在且大小不为0，返回
            //if (File.Exists(path))
            //{
            //    System.IO.FileInfo fileInfo = null;
            //    fileInfo = new System.IO.FileInfo(path);
            //    if (fileInfo.Length != 0)
            //    {
            //        Tools.Log.log(path + "已存在", true);
            //        return "already exits";
            //    }
            //}
            url = url.Trim();
            if (!url.Contains("http:") && !url.Contains("https:"))
                if (!url.Contains("//"))
                    url = "http://" + url;
                else url = "http:" + url;
            while (url.Contains("///"))
                url = url.Replace("///", "//");
            while (url.Contains(" "))
                url = url.Replace(" ","");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            String host = url.Replace("http://", "").Split('/')[0];
            req.Host = host.Trim();
            req.Headers.Add("Accept-Encoding", "gzip");
            //req.Connection = "keep-alive";
            try
            {

                WebResponse res = req.GetResponse();
            
                Stream resStream = res.GetResponseStream();
                int count = (int)res.ContentLength;
                int offset = 0;
                byte[] buf = new byte[count];
                while (count > 0)
                {
                    int n = resStream.Read(buf, offset, count);
                    if (n == 0) break;
                    count -= n;
                    offset += n;
                }
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
                fs.Close();
                //延时
                //Thread.Sleep(3000);
                return "done";
            }
            catch (Exception e)
            { 
                Tools.Log.log(String.Format("{0}\n下载失败,原因为\n{1}", url,e.ToString()), true); 
                return "fail"; 
            }
            
        }


        /// <summary>
        /// 将JSON字符串生成类，需自行提供类模版
        /// 使用子类时建议将变量名命名为和类名一样
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="jsonString">json</param>
        /// <returns></returns>
        public static T JsonParse<T>(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }
        }


        /// <summary>
        /// 中文转unicode（符合js规则的）
        /// </summary>
        /// <returns></returns>
        public static string unicode_to_chinese(string str)
        {
            Regex reUnicode = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);
            return reUnicode.Replace(str, m =>
            {
                short c;
                if (short.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out c))
                {
                    return "" + (char)c;
                }
                return m.Value;
            });  
        }

        /// <summary>
        /// 字符串编码转换
        /// </summary>
        /// <param name="srcEncoding">原编码</param>
        /// <param name="dstEncoding">目标编码</param>
        /// <param name="srcBytes">原字符串</param>
        /// <returns>字符串</returns>
        public static string TransferEncoding(Encoding srcEncoding, Encoding dstEncoding, string srcStr)
        {
            byte[] srcBytes = srcEncoding.GetBytes(srcStr);
            byte[] bytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(bytes);
        }

    }
}
