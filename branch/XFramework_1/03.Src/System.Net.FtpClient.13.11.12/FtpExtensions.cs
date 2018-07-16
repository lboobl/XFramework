using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;

namespace System.Net.FtpClient {
    /// <summary>
    /// Extension methods related to FTP tasks
    /// </summary>
    public static class FtpExtensions {
        /// <summary>
        /// Converts the specified path into a valid FTP file system path
        /// </summary>
        /// <param name="path">The file system path</param>
        /// <returns>A path formatted for FTP</returns>
        public static string GetFtpPath(this string path) {
            if (path == null || path.Length == 0)
                return "./";

            path = Regex.Replace(path.Replace('\\', '/'), "[/]+", "/").TrimEnd('/');
            if (path.Length == 0)
                path = "/";

            return path;
        }

        /// <summary>
        /// Creates a valid FTP path by appending the specified segments to this string
        /// </summary>
        /// <param name="path">This string</param>
        /// <param name="segments">The path segments to append</param>
        /// <returns>A valid FTP path</returns>
        public static string GetFtpPath(this string path, params string[] segments) {
            foreach (string part in segments) {
                if (part != null) {
                    if (path.Length > 0 && !path.EndsWith("/"))
                        path += "/";
                    path += Regex.Replace(part.Replace('\\', '/'), "[/]+", "/").TrimEnd('/');
                }
            }

            path = Regex.Replace(path.Replace('\\', '/'), "[/]+", "/").TrimEnd('/');
            if (path.Length == 0)
                path = "/";

            return path;
        }

        /// <summary>
        /// Gets the directory name of a path formatted for a FTP server
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The parent directory path</returns>
        public static string GetFtpDirectoryName(this string path) {
            if (path == null || path.Length == 0 || path.GetFtpPath() == "/")
                return "/";

            return System.IO.Path.GetDirectoryName(path).GetFtpPath();
        }

        /// <summary>
        /// Gets the file name from the path
        /// </summary>
        /// <param name="path">The full path to the file</param>
        /// <returns>The file name</returns>
        public static string GetFtpFileName(this string path) {
            return System.IO.Path.GetFileName(path).GetFtpPath();
        }

        /// <summary>
        /// Tries to convert the string FTP date representation  into a date time object
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="style">UTC/Local Time</param>
        /// <returns>A date time object representing the date, DateTime.MinValue if there was a problem</returns>
        public static DateTime GetFtpDate(this string date, DateTimeStyles style) {
            string[] formats = new string[] { 
                "yyyyMMddHHmmss", 
                "yyyyMMddHHmmss.fff",
                "MMM dd  yyyy", 
                "MMM dd HH:mm"
            };
            DateTime parsed;

            if (DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, style, out parsed)) {
                return parsed;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="client">Ftp客户端实例</param>
        /// <param name="sourceFileName">本地文件完全限定名</param>
        /// <param name="destPath">ftp目标路径</param>
        public static void OpenUpload(this FtpClient client, string sourceFileName, string destPath)
        {
            //检查上传的文件是否存在
            if (!File.Exists(sourceFileName)) throw new FileNotFoundException("File Not Found：" + sourceFileName);

            Stream stream = null;
            BinaryReader br = null;
            FileStream fs = null;
            
            destPath = Path.Combine(destPath, Path.GetFileName(sourceFileName));
            destPath = destPath.Replace(@"\", "/").Trim('/');
            destPath = "/" + destPath;

            try
            {
                FileInfo fi = new FileInfo(sourceFileName);         //打开本地文件
                byte[] buffer = new byte[4096];                     //上传缓冲区
                fs = fi.OpenRead();                                 //创建只读取文件流
                br = new BinaryReader(fs);                          //文件流阅读器
                stream = client.OpenWrite(destPath);                //打开Ftp写文件流
                int max = (int)Math.Ceiling(fi.Length / 4096.0);    //整个文件需要分块读取资料
                int start = 0;

                byte[] tmp = null;
                while (start < max)
                {
                    tmp = br.ReadBytes(buffer.Length);
                    stream.Write(tmp, 0, tmp.Length);
                    start += 1;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null) stream.Dispose();
                if (br != null) br.Close();
                if (fs != null) fs.Dispose();
            }
        }

        /// <summary>
        /// 从服务器上下载文件
        /// </summary>
        /// <param name="client">ftp客户端对象</param>
        /// <param name="sourceFileName">ftp服务器上的文件</param>
        /// <param name="destFileName">下载至本地的文件</param>
        public static void OpenDownload(this FtpClient client, string sourceFileName, string destFileName)
        {
           
            FileStream fs = null;
            Stream stream = null;

            try
            {
                stream = client.OpenRead(sourceFileName);
                //fs = new FileStream(Path.Combine(destFileName, Path.GetFileName(sourceFileName)), FileMode.Create);
                fs = new FileStream(destFileName, FileMode.Create);
                byte[] buffer = new byte[2048];

                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, read); //写入指定文件
                    if (read <= 0) break;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                //释放资源
                if (stream != null) stream.Dispose();
                if (fs != null) fs.Dispose();
            }
        }
    }
}
