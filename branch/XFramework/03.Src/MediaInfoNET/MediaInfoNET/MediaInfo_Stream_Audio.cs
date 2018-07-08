namespace MediaInfoNET
{
    using System;
    using System.Text.RegularExpressions;

    public class MediaInfo_Stream_Audio : MediaInfo_Stream
    {
        public int Channels
        {
            get
            {
                string str = null;
                if (base.Properties.TryGetValue("Channel(s)", out str) && (str != null))
                {
                    double result = 0.0;
                    base.exp = new Regex("([ 0-9.]+)[channels]*");
                    base.exp_matches = base.exp.Matches(str);
                    if (base.exp_matches.Count > 0)
                    {
                        str = base.exp_matches[0].Value;
                        if (double.TryParse(base.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim(), out result))
                        {
                            return (int) Math.Round(Math.Ceiling(result));
                        }
                    }
                }
                return 0;
            }
        }

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
                if (this.Channels != 0)
                {
                    str2 = str2 + ", " + this.Channels.ToString() + " ch";
                }
                if (this.SamplingRate != 0)
                {
                    str2 = str2 + ", " + this.SamplingRate.ToString() + " hz";
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
                string property = this.GetProperty("Format");
                if (property != "")
                {
                    string str3 = property.ToLower();
                    switch (str3)
                    {
                        case "mpeg audio":
                        {
                            string str4 = this.GetProperty("Format profile").ToLower();
                            if (str4 == "layer 2")
                            {
                                return "MP2";
                            }
                            if (str4 != "layer 3")
                            {
                                return "";
                            }
                            return "MP3";
                        }
                        case "2048":
                            return "SONIC";

                        case "ac-3":
                            return "AC3";
                    }
                    if (((str3 != "wma1") && (str3 != "wma2")) && ((str3 != "wmav1") && (str3 != "wmav2")))
                    {
                        return property.ToUpper();
                    }
                    return "WMA";
                }
                property = this.GetProperty("Codec ID");
                if (property.Contains("pcm"))
                {
                    return "PCM";
                }
                return property.ToUpper();
            }
        }

        public string MPlayerID
        {
            get
            {
                string str2 = null;
                if (base.Properties.TryGetValue("MPlayer -aid", out str2))
                {
                    return str2;
                }
                return "";
            }
        }

        public int SamplingRate
        {
            get
            {
                string str = null;
                if (base.Properties.TryGetValue("Sampling rate", out str) && (str != null))
                {
                    double result = 0.0;
                    base.exp = new Regex("([ 0-9.,]+)KHz*");
                    base.exp_matches = base.exp.Matches(str);
                    if (base.exp_matches.Count > 0)
                    {
                        str = base.exp_matches[0].Value;
                        str = base.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim();
                        if (double.TryParse(str, out result))
                        {
                            return (int) Math.Round((double) (result * 1000.0));
                        }
                    }
                    base.exp = new Regex("([ 0-9.,]+)Hz*");
                    base.exp_matches = base.exp.Matches(str);
                    if (base.exp_matches.Count > 0)
                    {
                        str = base.exp_matches[0].Value;
                        if (double.TryParse(base.exp.Replace(str, "$1").Replace(" ", "").Replace(",", "").Trim(), out result))
                        {
                            return (int) Math.Round(result);
                        }
                    }
                }
                return 0;
            }
        }

        public override string StreamType
        {
            get
            {
                return "Audio";
            }
        }
    }
}

