namespace MediaInfoNET
{
    using System;

    public class MediaInfo_Stream_Text : MediaInfo_Stream
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
                if (this.Language != "")
                {
                    str2 = str2 + ", " + this.Language;
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
                    string str3 = property;
                    return property.ToUpper();
                }
                property = this.GetProperty("Codec ID");
                if (property != "")
                {
                    return property.ToUpper();
                }
                return "";
            }
        }

        public string Language
        {
            get
            {
                return this.GetProperty("Language");
            }
        }

        public string MPlayerID
        {
            get
            {
                string str2 = null;
                if (base.Properties.TryGetValue("MPlayer -sid", out str2))
                {
                    return str2;
                }
                return "";
            }
        }

        public override string StreamType
        {
            get
            {
                return "Text";
            }
        }
    }
}

