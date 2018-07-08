namespace MediaInfoNET
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class MediaInfo_Stream
    {
        protected Regex exp;
        protected MatchCollection exp_matches;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public MediaFile SourceFile;
        public int StreamIndex;
        public int StreamTypeIndex;

        public string GetProperty(string Name)
        {
            if (this.Properties.ContainsKey(Name))
            {
                return this.Properties[Name];
            }
            return "";
        }

        public override string ToString()
        {
            return this.Description;
        }

        public virtual int Bitrate
        {
            get
            {
                string str = null;
                if (this.Properties.TryGetValue("Bit rate", out str) && (str != null))
                {
                    int result = 0;
                    this.exp = new Regex("([ 0-9.,]+)[Kbps]*");
                    this.exp_matches = this.exp.Matches(str);
                    if (this.exp_matches.Count > 0)
                    {
                        str = this.exp_matches[0].Value;
                        if (int.TryParse(this.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim(), out result))
                        {
                            return result;
                        }
                    }
                }
                return 0;
            }
        }

        public virtual string CodecID
        {
            get
            {
                return this.GetProperty("Codec ID");
            }
        }

        public virtual string Description
        {
            get
            {
                return "";
            }
        }

        public long DurationMillis
        {
            get
            {
                string str = null;
                if (!this.Properties.TryGetValue("Duration", out str) || (str == null))
                {
                    return 0L;
                }
                if (str.Contains(":"))
                {
                    return modCommon.GetMillisFromString(str);
                }
                long num2 = 0L;
                foreach (string str2 in Strings.Split(str, " ", -1, CompareMethod.Binary))
                {
                    if (str2.Contains("ms"))
                    {
                        num2 += Conversions.ToLong(Strings.Trim(str2.Replace("ms", "")));
                    }
                    else if (str2.Contains("s"))
                    {
                        num2 += Conversions.ToLong(Strings.Trim(str2.Replace("s", ""))) * 0x3e8L;
                    }
                    else if (str2.Contains("mn"))
                    {
                        num2 += (Conversions.ToLong(Strings.Trim(str2.Replace("mn", ""))) * 60L) * 0x3e8L;
                    }
                    else if (str2.Contains("h"))
                    {
                        num2 += (Conversions.ToLong(Strings.Trim(str2.Replace("h", ""))) * 0xe10L) * 0x3e8L;
                    }
                }
                return num2;
            }
        }

        public string DurationString
        {
            get
            {
                long durationMillis = this.DurationMillis;
                if (durationMillis > 0L)
                {
                    return TimeSpan.FromSeconds(Math.Floor((double) (((double) durationMillis) / 1000.0))).ToString();
                }
                return "00:00:00";
            }
        }

        public string DurationStringAccurate
        {
            get
            {
                long durationMillis = this.DurationMillis;
                if (durationMillis > 0L)
                {
                    long num2 = durationMillis % 0x3e8L;
                    return (TimeSpan.FromSeconds(Math.Floor((double) (((double) durationMillis) / 1000.0))).ToString() + "." + num2.ToString("000"));
                }
                return "00:00:00.000";
            }
        }

        public virtual string Format
        {
            get
            {
                return this.GetProperty("Format");
            }
        }

        public virtual string FormatID
        {
            get
            {
                return this.GetProperty("Format");
            }
        }

        public int ID
        {
            get
            {
                string str = null;
                int result = 0;
                if (this.Properties.TryGetValue("ID", out str))
                {
                    str = str.Trim();
                    if (str.Contains(" "))
                    {
                        str = str.Split(new char[] { ' ' })[0];
                    }
                    if (int.TryParse(str, out result))
                    {
                        return result;
                    }
                }
                return -1;
            }
        }

        public long StreamSize
        {
            get
            {
                string str = null;
                if (!this.Properties.TryGetValue("StreamSize", out str))
                {
                    return 0L;
                }
                long num2 = 0L;
                if (str.Contains(" KiB"))
                {
                    return (long) Math.Round((double) (Conversions.ToDouble(str.Replace(" KiB", "").Replace(" ", "").Replace(",", "").Trim()) * 1024.0));
                }
                if (str.Contains(" MiB"))
                {
                    return (long) Math.Round((double) (Conversions.ToDouble(str.Replace(" MiB", "").Replace(" ", "").Replace(",", "").Trim()) * 1048576.0));
                }
                if (str.Contains(" GiB"))
                {
                    num2 = (long) Math.Round((double) (Conversions.ToDouble(str.Replace(" GiB", "").Replace(" ", "").Replace(",", "").Trim()) * 1073741824.0));
                }
                return num2;
            }
        }

        public virtual string StreamType
        {
            get
            {
                return "";
            }
        }
    }
}

