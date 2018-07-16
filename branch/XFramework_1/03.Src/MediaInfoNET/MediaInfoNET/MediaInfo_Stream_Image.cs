namespace MediaInfoNET
{
    using System;
    using System.Text.RegularExpressions;

    public class MediaInfo_Stream_Image : MediaInfo_Stream
    {
        public override string Description
        {
            get
            {
                string str2 = "";
                if (this.Format != "")
                {
                    str2 = str2 + ", " + this.Format;
                }
                else if (this.CodecID != "")
                {
                    str2 = str2 + ", " + this.CodecID;
                }
                if (this.PixelFormat != "")
                {
                    str2 = str2 + ", " + this.PixelFormat;
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
                return "Image";
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
    }
}

