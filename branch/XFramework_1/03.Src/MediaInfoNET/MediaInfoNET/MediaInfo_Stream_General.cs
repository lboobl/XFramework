namespace MediaInfoNET
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public class MediaInfo_Stream_General : MediaInfo_Stream
    {
        public override string ToString()
        {
            return this.Description;
        }

        public override int Bitrate
        {
            get
            {
                string str = null;
                if (base.Properties.TryGetValue("Overall bit rate", out str) && (str != null))
                {
                    int result = 0;
                    base.exp = new Regex("([ 0-9.,]+)[Kbps]*");
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

        public string Extension
        {
            get
            {
                if (this.GetProperty("Complete name") != "")
                {
                    return Path.GetExtension(this.GetProperty("Complete name")).Replace(".", "").Trim().ToUpper();
                }
                return "";
            }
        }

        public override string StreamType
        {
            get
            {
                return "General";
            }
        }
    }
}

