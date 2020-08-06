using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// 文件处理工具
    /// </summary>
    public static class FileTool
    {
        /// <summary>
        /// 打开文件并且读取所有字符
        /// 读取失败则返回空null
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>读取字符</returns>
        public static String read(String path)
        {
            StreamReader sr;
            try
            {
                //尝试是否能够打开，否则返回，用于放置对话框临时关闭
                sr = new StreamReader(path, System.Text.Encoding.UTF8);
                String fileStr = sr.ReadToEnd();
                sr.Close();
                return fileStr;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 把一个字符串覆盖进入文件
        /// 如果路径不存在就创建文件夹
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="str">文件内容</param>
        public static void write(String path,String str)
        {
            StreamWriter sr;
            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                {
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(path));
                    directoryInfo.Create();
                }
                //尝试是否能够打开，否则返回，用于放置对话框临时关闭
                sr = new StreamWriter(path,false);
                sr.Write(str);
                sr.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 把一个字符串追加进入文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="str">文件内容</param>
        /// /// <param name="str">是否追加,追加为true,否则该参数无效</param>
        public static void write(String path, String str , bool append)
        {
            StreamWriter sw;
            try
            {
                //尝试是否能够打开，否则返回，用于放置对话框临时关闭
                if(append)
                    sw = new StreamWriter(path, true);
                else sw = new StreamWriter(path, false);
                sw.Write(str);
                sw.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 将一段字符转换为字符串列表
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <returns>转换后的字符串列表</returns>
        public static List<String> toList(String str)
        {
            if (str == null || str.Length == 0)
                return null;

            List<String> back = new List<string>();
            StringReader sr = new StringReader(str);
            String nextList = "";
            while ((nextList = sr.ReadLine()) != null)
            {
                back.Add(nextList);
            }
            sr.Close();
            return back;
        }

        /// <summary>
        /// 把一个字符串列表转化为一段字符
        /// </summary>
        /// <param name="list">字符串列表</param>
        /// <returns>转换后的字符</returns>
        public static String toLine(List<String> list)
        {
            String temp = "";
            StringWriter sw = new StringWriter(new StringBuilder(temp));
            for (int i = 0; i < list.Count;i++ )
            {
                sw.WriteLine(list[i]);
            }
            temp = sw.ToString();
            sw.Close();
            return temp;
        }

        /// <summary>
        /// 获取目录机器所有子目录下的文件
        /// </summary>
        /// <param name="diroot">目录路径</param>
        public static void GetFileList(DirectoryInfo diroot,List<String> listFile)
        {
            foreach (FileInfo fileName in diroot.GetFiles())
            {
                listFile.Add(fileName.FullName);
            }
            foreach (DirectoryInfo dirSub in diroot.GetDirectories())
            {
                GetFileList(dirSub, listFile);
            }
        }

        /// <summary>
        /// 取出可能干扰SQL的特殊字符
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static String SQLFilter(String param)
        {
            return param.Replace("'", "''").Replace(@"\","");
        }

        /// <summary>
        /// 判断一个字符是否为GB2312
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static bool IsGBCode(string word)
        {
            byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(word);
            if (bytes.Length <= 1) // if there is only one byte, it is ASCII code or other code
                return false;
            else
            {
                byte byte1 = bytes[0];
                byte byte2 = bytes[1];
                if (byte1 >= 176 && byte1 <= 247 && byte2 >= 160 && byte2 <= 254)    //判断是否是GB2312
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// GB2312转换成UTF8
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string gb2312_to_utf8(string text)
        {
            //声明字符集   
            System.Text.Encoding utf8, gb2312;
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            //utf8   
            utf8 = System.Text.Encoding.GetEncoding("utf-8");
            byte[] gb;
            gb = gb2312.GetBytes(text);
            gb = System.Text.Encoding.Convert(gb2312, utf8, gb);
            //返回转换后的字符   
            return utf8.GetString(gb);
        }

    }
}
