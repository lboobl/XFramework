namespace MediaInfoNET
{
    using System;

    public class MediaInfo_Stream_Data : MediaInfo_Stream
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
                switch (property)
                {
                    case "AVC":
                        property = "mpeg4avc";
                        return "";

                    case "MPEG-4 Visual":
                    case "MS-MPEG4 v1":
                    case "MS-MPEG4 v2":
                    case "MS-MPEG4 v3":
                    case "S-Mpeg 4 v2":
                    case "S-Mpeg 4 v3":
                        return "mpeg4asp";

                    case "RealVideo 1":
                    case "RealVideo 2":
                    case "RealVideo 3":
                    case "RealVideo 4":
                        return "rv";

                    case "MPEG Video":
                        return "mpeg";

                    case "":
                        property = this.GetProperty("Codec ID");
                        switch (property)
                        {
                            case "rawvideo":
                                return "raw";

                            case "dvvideo":
                                return "dv";
                        }
                        return property.ToLower();
                }
                return property.ToLower();
            }
        }

        public override string StreamType
        {
            get
            {
                return "Data";
            }
        }
    }
}

