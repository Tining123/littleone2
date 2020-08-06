using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools.web
{
    /// <summary>
    /// 检测图片类型类
    /// </summary>
    public class ImageTypeCheck
    {
        static ImageTypeCheck()
        {
            _imageTag = InitImageTag();
        }
        private static SortedDictionary<int, ImageType> _imageTag ;

        public static readonly string ErrType = ImageType.None.ToString();

        private static SortedDictionary<int, ImageType> InitImageTag()
        {
            SortedDictionary<int, ImageType> list = new SortedDictionary<int, ImageType>();

            list.Add((int)ImageType.BMP, ImageType.BMP);
            list.Add((int)ImageType.JPG, ImageType.JPG);
            list.Add((int)ImageType.GIF, ImageType.GIF);
            list.Add((int)ImageType.PCX, ImageType.PCX);
            list.Add((int)ImageType.PNG, ImageType.PNG);
            list.Add((int)ImageType.PSD, ImageType.PSD);
            list.Add((int)ImageType.RAS, ImageType.RAS);
            list.Add((int)ImageType.SGI, ImageType.SGI);
            list.Add((int)ImageType.TIFF, ImageType.TIFF);
            return list;

        }

        /// <summary>  
        /// 通过文件头判断图像文件的类型  
        /// </summary>  
        /// <param name="path"></param>  
        /// <returns></returns>  
        public static string CheckImageTypeName(string path)
        {
            return CheckImageType(path).ToString();
        }

        /// <summary>  
        /// 通过文件头判断网络图像文件的类型  
        /// </summary>  
        /// <param name="path"></param>  
        /// <returns></returns>  
        public static string CheckImageTypeName(string path,bool mode = true)
        {
            return CheckImageType(path,true).ToString();
        }

        /// <summary>  
        /// 通过文件头判断图像文件的类型  
        /// </summary>  
        /// <param name="path"></param>  
        /// <returns></returns>  
        public static ImageType CheckImageType(string path)
        {
            byte[] buf = new byte[2];
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    int i = sr.BaseStream.Read(buf, 0, buf.Length);
                    if (i != buf.Length)
                    {
                        return ImageType.None;
                    }
                }
            }
            catch 
            {
                //Debug.Print(exc.ToString());
                return ImageType.None;
            }
            return CheckImageType(buf);
        }

        /// <summary>  
        /// 通过文件头判断网络图像文件的类型  
        /// </summary>  
        /// <param name="path"></param>  
        /// <returns></returns>  
        public static ImageType CheckImageType(string path,bool mode = true)
        {
            path = path.Trim();
            if (!path.Contains("http:") && !path.Contains("https:"))
                if (!path.Contains("//"))
                    path = "http://" + path;
                else path = "http:" + path;
            while (path.Contains("///"))
                path = path.Replace("///", "//");
            while (path.Contains(" "))
                path = path.Replace(" ", "");
            Uri httpURL = new Uri(path);
            ///HttpWebRequest类继承于WebRequest，并没有自己的构造函数，需通过WebRequest的Creat方法 建立，并进行强制的类型转换 
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(httpURL);
            httpReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            HttpWebResponse httpResp = null;
            try
            {
                httpResp = (HttpWebResponse)httpReq.GetResponse();
            }
            catch { return ImageType.None; }
            byte[] buf = new byte[2];
            try
            {
                using (StreamReader sr = new StreamReader(httpResp.GetResponseStream()))
                {
                    int i = sr.BaseStream.Read(buf, 0, buf.Length);
                    if (i != buf.Length)
                    {
                        return ImageType.None;
                    }
                }
            }
            catch 
            {
                //Debug.Print(exc.ToString());
                return ImageType.None;
            }
            return CheckImageType(buf);
        }

        /// <summary>  
        /// 通过文件的前两个自己判断图像类型  
        /// </summary>  
        /// <param name="buf">至少2个字节</param>  
        /// <returns></returns>  
        public static ImageType CheckImageType(byte[] buf)
        {
            if (buf == null || buf.Length < 2)
            {
                return ImageType.None;
            }

            int key = (buf[1] << 8) + buf[0];
            ImageType s;  
            if (_imageTag.TryGetValue(key, out s))
            {
                return s;
            }  
            return ImageType.None;
        }

        /// <summary>  
        /// 图像文件的类型  
        /// </summary>  
        public enum ImageType
        {
            None = 0,
            BMP = 0x4D42,
            JPG = 0xD8FF,
            GIF = 0x4947,
            PCX = 0x050A,
            PNG = 0x5089,
            PSD = 0x4238,
            RAS = 0xA659,
            SGI = 0xDA01,
            TIFF = 0x4949
        }

    }
}
