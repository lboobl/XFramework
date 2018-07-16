namespace MediaInfoNET
{
    using System;

    public class MediaInfo_Stream_Menu : MediaInfo_Stream
    {
        public override string Description
        {
            get
            {
                string str2 = "";
                if (this.Format != "")
                {
                    str2 = str2 + " | " + this.Format;
                }
                if (str2.Trim() != "")
                {
                    str2 = str2.Trim().Remove(0, 1).Trim();
                }
                return str2;
            }
        }

        public override string StreamType
        {
            get
            {
                return "Menu";
            }
        }
    }
}

