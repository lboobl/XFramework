namespace MediaInfoNET
{
    using System;
    using System.Text.RegularExpressions;

    public class MediaInfo_Stream_Video : MediaInfo_Stream
    {
        public override string Description
        {
            get
            {
                string str2 = "";
                if (this.FormatID != "")
                {
                    str2 = str2 + ", " + this.FormatID;
                }
                if (this.Bitrate != 0)
                {
                    str2 = str2 + ", " + this.Bitrate.ToString() + " kbps";
                }
                if (this.FrameRate != 0.0)
                {
                    str2 = str2 + ", " + this.FrameRate.ToString() + " fps";
                }
                if (this.FrameSize != "")
                {
                    str2 = str2 + ", " + this.FrameSize;
                }
                if (str2.Trim() != "")
                {
                    str2 = str2.Trim().Remove(0, 1).Trim();
                }
                return str2;
            }
        }

        public override string FormatID
        {
            get
            {
                string property = this.GetProperty("Format").Trim();
                if (property != "")
                {
                    switch (property.ToLower())
                    {
                        case "avc":
                        case "h264":
                            return "H264";

                        case "mpeg-4":
                        case "mpeg-4 visual":
                        case "ms-mpeg v1":
                        case "ms-mpeg v2":
                        case "ms-mpeg v3":
                        case "s-mpeg 4 v2":
                        case "s-mpeg 4 v3":
                            return "MPEG4";

                        case "realvideo 1":
                        case "realvideo 2":
                        case "realvideo 3":
                        case "realvideo 4":
                        case "rv10":
                        case "rv20":
                        case "rv30":
                        case "rv40":
                            return "RV";

                        case "mpeg video":
                        case "mpeg2video":
                            return "MPEG";

                        case "wmv2":
                            return "WMV";
                    }
                    return property.ToUpper();
                }
                property = this.GetProperty("Codec ID");
                switch (property.ToLower())
                {
                    case "rawvideo":
                        return "RAW";

                    case "dvvideo":
                        return "DV";
                }
                return property.ToUpper();
            }
        }

        public double FrameRate
        {
            get
            {
                string str = null;
                if (base.Properties.TryGetValue("Frame rate", out str) && (str != null))
                {
                    double result = 0.0;
                    base.exp = new Regex("([ 0-9.,]+)[fps]*");
                    base.exp_matches = base.exp.Matches(str);
                    if (base.exp_matches.Count > 0)
                    {
                        str = base.exp_matches[0].Value;
                        if (double.TryParse(base.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim(), out result))
                        {
                            if ((result >= 24.97) & (result <= 25.2))
                            {
                                result = 25.0;
                            }
                            return result;
                        }
                    }
                }
                return 0.0;
            }
        }

        public string FrameSize
        {
            get
            {
                return (this.Width.ToString() + "x" + this.Height.ToString());
            }
        }

        public int Height
        {
            get
            {
                string str = null;
                if (base.Properties.TryGetValue("Height", out str) && (str != null))
                {
                    int result = 0;
                    base.exp = new Regex("([ 0-9,]+)[pixels]*");
                    base.exp_matches = base.exp.Matches(str);
                    if (base.exp_matches.Count > 0)
                    {
                        str = base.exp_matches[0].Value;
                        if (int.TryParse(base.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim(), out result))
                        {
                            return result;
                        }
                    }
                }
                return 0;
            }
        }

        public bool IsInterlaced
        {
            get
            {
                string str = null;
                return (base.Properties.TryGetValue("Scan type", out str) && (str == "Interlaced"));
            }
        }

        public string MPlayerID
        {
            get
            {
                string str2 = null;
                if (base.Properties.TryGetValue("MPlayer -vid", out str2))
                {
                    return str2;
                }
                return "";
            }
        }

        public string PixelFormat
        {
            get
            {
                return this.GetProperty("Pixel format");
            }
        }

        public long Resolution
        {
            get
            {
                return (long) (this.Width * this.Height);
            }
        }

        public override string StreamType
        {
            get
            {
                return "Video";
            }
        }

        public int Width
        {
            get
            {
                string str = null;
                if (base.Properties.TryGetValue("Width", out str) && (str != null))
                {
                    int result = 0;
                    base.exp = new Regex("([ 0-9,]+)[pixels]*");
                    base.exp_matches = base.exp.Matches(str);
                    if (base.exp_matches.Count > 0)
                    {
                        str = base.exp_matches[0].Value;
                        if (int.TryParse(base.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim(), out result))
                        {
                            return result;
                        }
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 视频截图
        /// </summary>
        /// <param name="vImageFullName">输出图片路径</param>
        /// <param name="vffmpeg">ffmpeg.exe完全路径</param>
        /// <param name="size">输出尺寸</param>
        /// <returns></returns>
        public void Capture(string vFile, string vImageFullName, string vffmpeg, string size)
        {
            //echo # 制作者：黄峰
            //echo # 功能：使用FFmpeg 对FLV进行批量截图
            //ffmpeg -i D:\video.flv -y -f image2 -ss 8 -sameq -t 0.001 -s 640*480 D:\video.jpg
            //ffmpeg -i test.asf -y -f image2 -t 0.001 -s 352x240 a.jpg
            //" -i " + fileName + " -y -f image2 -ss 2 -vframes 1 -s " + FlvImgSize + " " + flv_img;
            //" -i " + vFileName + " -y -f image2 -t 0.001 -s " + FlvImgSize + " " + flv_img_p ;

            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(vffmpeg);
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.Arguments = " -i " + vFile + " -y -f image2 -ss 08.010 -t 0.001 -s " + vsize + " " + vImageFullName;
            //System.Diagnostics.Process.Start(startInfo);
            System.Diagnostics.Process process = null;
            try
            {
                //初始化进程实例，并设置相关基本属性
                process = new System.Diagnostics.Process();
                process.StartInfo.FileName = vffmpeg;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                //设置进程的工作路径，这样可以省去要模拟输入CD 命令
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.Arguments = " -i " + vFile + " -y -f image2 -ss 08.010 -t 0.001 -s " + size + " " + vImageFullName;
                //启动cmd进程
                process.Start();
                //process.StandardIn
                //退出进程
                process.WaitForExit();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (process != null) process.Close();
            }
        }

        ///// <summary>
        ///// 视频截图[带异常捕捉]
        ///// </summary>
        ///// <param name="vImageFullName">输出图片路径</param>
        ///// <param name="vffmpeg">ffmpeg.exe完全路径</param>
        ///// <param name="size">输出尺寸</param>
        ///// <returns></returns>
        //public string Capture1(string vFile, string vImageFullName, string vffmpeg, string size)
        //{
        //    //echo # 制作者：黄峰
        //    //echo # 功能：使用FFmpeg 对FLV进行批量截图
        //    //ffmpeg -i D:\video.flv -y -f image2 -ss 8 -sameq -t 0.001 -s 640*480 D:\video.jpg
        //    //ffmpeg -i test.asf -y -f image2 -t 0.001 -s 352x240 a.jpg
        //    //" -i " + fileName + " -y -f image2 -ss 2 -vframes 1 -s " + FlvImgSize + " " + flv_img;
        //    //" -i " + vFileName + " -y -f image2 -t 0.001 -s " + FlvImgSize + " " + flv_img_p ;

        //    //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(vffmpeg);
        //    //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //    //startInfo.Arguments = " -i " + vFile + " -y -f image2 -ss 08.010 -t 0.001 -s " + vsize + " " + vImageFullName;
        //    //System.Diagnostics.Process.Start(startInfo);
        //    System.Diagnostics.Process process = null;
        //    try
        //    {
        //        //初始化进程实例，并设置相关基本属性
        //        process = new System.Diagnostics.Process();
        //        process.StartInfo.FileName = vffmpeg;
        //        process.StartInfo.CreateNoWindow = true;
        //        process.StartInfo.UseShellExecute = false;
        //        //设置进程的工作路径，这样可以省去要模拟输入CD 命令
        //        process.StartInfo.RedirectStandardInput = true;
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.Arguments = " -i " + vFile + " -y -f image2 -ss 08.010 -t 0.001 -s " + size + " " + vImageFullName;
        //        //启动cmd进程
        //        process.Start();
        //        //process.StandardIn
        //        //退出进程
        //        process.WaitForExit();

        //        return vImageFullName;
        //    }
        //    catch
        //    {
        //        //如果截图不成功，返回空值
        //        return string.Empty;
        //    }
        //    finally
        //    {
        //        if (process != null) process.Close();
        //    }
        //}
    }
}

