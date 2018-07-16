namespace MediaInfoNET
{
    using MediaInfoNET.My;
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MediaFile : File
    {
        public List<MediaInfo_Stream> AllStreams;
        public List<MediaInfo_Stream_Audio> Audio;
        public List<MediaInfo_Stream_Chapters> Chapters;
        public List<MediaInfo_Stream_Data> Data;
        public string Description;
        public MediaInfo_Stream_General General;
        public List<MediaInfo_Stream_Image> Image;
        public string Info_HTML;
        public string Info_Text;
        public bool MediaInfo_Available;
        public string MediaInfo_HTML;
        public string MediaInfo_Text;
        public List<MediaInfo_Stream_Menu> Menu;
        public List<MediaInfo_Stream_Text> Text;
        public List<MediaInfo_Stream_Video> Video;

        public MediaFile(string SourceFile) : base(SourceFile)
        {
            this.Description = "";
            this.Info_Text = "";
            this.Info_HTML = "";
            this.MediaInfo_Text = "";
            this.MediaInfo_HTML = "";
            this.MediaInfo_Available = false;
            this.AllStreams = new List<MediaInfo_Stream>();
            this.General = new MediaInfo_Stream_General();
            this.Audio = new List<MediaInfo_Stream_Audio>();
            this.Video = new List<MediaInfo_Stream_Video>();
            this.Text = new List<MediaInfo_Stream_Text>();
            this.Image = new List<MediaInfo_Stream_Image>();
            this.Menu = new List<MediaInfo_Stream_Menu>();
            this.Chapters = new List<MediaInfo_Stream_Chapters>();
            this.Data = new List<MediaInfo_Stream_Data>();
            if (SourceFile != "")
            {
                this.GetMediaInfo(true);
                if (this.InfoAvailable)
                {
                    this.getHTML(ref this.Info_HTML, ref this.General, ref this.AllStreams);
                    this.getInfoText();
                    this.getDescription();
                }
            }
        }

        private void addProperties(MediaInfo_Stream_General m_General, List<MediaInfo_Stream> m_AllStreams, bool IsMediaInfo = false)
        {
            Dictionary<string, string> properties;
            if (this.General == null)
            {
                this.General = m_General;
            }
            else
            {
                properties = m_General.Properties;
                foreach (string str in properties.Keys)
                {
                    if (!this.General.Properties.ContainsKey(str))
                    {
                        this.General.Properties.Add(str, properties[str]);
                    }
                }
            }
            int num = -1;
            if (this.AllStreams.Count == 0)
            {
                foreach (MediaInfo_Stream stream in m_AllStreams)
                {
                    num++;
                    this.AllStreams.Add(stream);
                    string str3 = stream.StreamType.ToString();
                    if (str3 == "Data")
                    {
                        this.Data.Add((MediaInfo_Stream_Data) stream);
                        stream.StreamTypeIndex = this.Data.IndexOf((MediaInfo_Stream_Data) stream);
                    }
                    else
                    {
                        if (str3 == "Audio")
                        {
                            this.Audio.Add((MediaInfo_Stream_Audio) stream);
                            stream.StreamTypeIndex = this.Audio.IndexOf((MediaInfo_Stream_Audio) stream);
                            continue;
                        }
                        if (str3 == "Video")
                        {
                            this.Video.Add((MediaInfo_Stream_Video) stream);
                            stream.StreamTypeIndex = this.Video.IndexOf((MediaInfo_Stream_Video) stream);
                            continue;
                        }
                        if (str3 == "Text")
                        {
                            this.Text.Add((MediaInfo_Stream_Text) stream);
                            stream.StreamTypeIndex = this.Text.IndexOf((MediaInfo_Stream_Text) stream);
                            continue;
                        }
                        if (str3 == "Image")
                        {
                            this.Image.Add((MediaInfo_Stream_Image) stream);
                            stream.StreamTypeIndex = this.Image.IndexOf((MediaInfo_Stream_Image) stream);
                            continue;
                        }
                        if (str3 == "Menu")
                        {
                            this.Menu.Add((MediaInfo_Stream_Menu) stream);
                            stream.StreamTypeIndex = this.Menu.IndexOf((MediaInfo_Stream_Menu) stream);
                            continue;
                        }
                        if (str3 == "Chapters")
                        {
                            this.Chapters.Add((MediaInfo_Stream_Chapters) stream);
                            stream.StreamTypeIndex = this.Chapters.IndexOf((MediaInfo_Stream_Chapters) stream);
                        }
                    }
                }
            }
            else
            {
                foreach (MediaInfo_Stream stream2 in m_AllStreams)
                {
                    List<MediaInfo_Stream>.Enumerator enumerator = new List<MediaInfo_Stream>.Enumerator();
                    try
                    {
                        enumerator = this.AllStreams.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            MediaInfo_Stream current = enumerator.Current;
                            if ((current.StreamType.Equals(stream2.StreamType) & (current.StreamTypeIndex == stream2.StreamTypeIndex)) & ((current.FormatID == stream2.FormatID) | (current.FormatID == "")))
                            {
                                Dictionary<string, string>.KeyCollection.Enumerator enumerator5 = new Dictionary<string,string>.KeyCollection.Enumerator();
                                properties = stream2.Properties;
                                if (IsMediaInfo)
                                {
                                    if (current.Properties.ContainsKey("Codec ID") & stream2.Properties.ContainsKey("Codec ID"))
                                    {
                                        current.Properties["Codec ID"] = stream2.Properties["Codec ID"];
                                    }
                                    if (current.Properties.ContainsKey("Format") & stream2.Properties.ContainsKey("Format"))
                                    {
                                        current.Properties["Format"] = stream2.Properties["Format"];
                                    }
                                }
                                try
                                {
                                    enumerator5 = properties.Keys.GetEnumerator();
                                    while (enumerator5.MoveNext())
                                    {
                                        string key = enumerator5.Current;
                                        if (!current.Properties.ContainsKey(key))
                                        {
                                            current.Properties.Add(key, properties[key]);
                                        }
                                    }
                                    continue;
                                }
                                finally
                                {
                                    enumerator5.Dispose();
                                }
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
            }
        }

        public void DeleteProperties()
        {
            this.General = null;
            this.AllStreams.Clear();
            this.Audio.Clear();
            this.Video.Clear();
            this.Text.Clear();
            this.Image.Clear();
            this.Menu.Clear();
            this.Chapters.Clear();
            this.Info_Text = "";
            this.Info_HTML = "";
        }

        private void getDescription()
        {
            string str = "";
            if (this.General.FormatID != "")
            {
                str = str + ", " + this.General.Extension + " File";
            }
            if (this.FileSize != 0L)
            {
                str = str + ", " + modCommon.FormatFileSize(this.FileSize);
            }
            if (this.StreamCount >= 0)
            {
                str = str + ", " + this.StreamCount.ToString() + " streams";
            }
            if (this.General.Bitrate != 0)
            {
                str = str + ", " + this.General.Bitrate.ToString() + " kbps";
            }
            if (this.General.DurationString != "")
            {
                str = str + ", " + this.General.DurationString;
            }
            if (str.Trim() != "")
            {
                str = str.Trim().Remove(0, 1).Trim();
            }
            str = str + "\r\n\r\n";
            foreach (MediaInfo_Stream stream in this.AllStreams)
            {
                string streamType = stream.StreamType;
                switch (streamType)
                {
                    case "Audio":
                    case "Video":
                    case "Text":
                    {
                        str = str + "#" + stream.StreamIndex.ToString() + " | " + stream.StreamType + " [ " + stream.ToString() + " ]\r\n";
                        continue;
                    }
                }
                if ((streamType != "Menu") && (streamType == "Chapter"))
                {
                }
            }
            str = str.Trim();
            if (str.EndsWith("\r\n"))
            {
                str = str.Remove(str.Length, 1);
            }
            this.Description = str;
        }

        private void getHTML(ref string StringHTML, ref MediaInfo_Stream_General m_General, ref List<MediaInfo_Stream> m_AllStreams)
        {
            string str = "";
            bool flag = false;
            Dictionary<string, string> properties = m_General.Properties;
            str = ((str + "<table width='100%'>\r\n") + "<thead><tr>\r\n" + "<th colspan=2>&nbsp;General</th>\r\n") + "</tr></thead>\r\n" + "<tbody>\r\n";
            foreach (string str2 in properties.Keys)
            {
                flag = !flag;
                if (flag)
                {
                    str = str + "<tr class='odd'>";
                }
                else
                {
                    str = str + "<tr>";
                }
                str = str + "<td nowrap>" + str2 + "</td><td>" + properties[str2].ToString() + "</td>\r\n";
                str = str + "</tr>";
            }
            str = str + "</tbody>\r\n" + "</table>\r\n\r\n";
            foreach (MediaInfo_Stream stream in m_AllStreams)
            {
                if (stream.Properties.Count != 0)
                {
                    properties = stream.Properties;
                    str = str + "<table width='100%'>\r\n";
                    str = str + "<thead><tr>\r\n";
                    str = str + "<th colspan=2>&nbsp;" + stream.StreamType + " #" + stream.StreamTypeIndex.ToString() + "</th>\r\n";
                    str = str + "</tr></thead>\r\n";
                    str = str + "<tbody>\r\n";
                    foreach (string str3 in properties.Keys)
                    {
                        if (!flag)
                        {
                            str = str + "<tr class='odd'>";
                        }
                        else
                        {
                            str = str + "<tr>";
                        }
                        str = str + "<td nowrap>" + str3 + "</td><td>" + properties[str3].ToString() + "</td>\r\n";
                        str = str + "</tr>";
                    }
                    str = str + "</tbody>\r\n";
                    str = str + "</table>\r\n\r\n";
                }
            }
            StringHTML = str;
        }

        private void getInfoText()
        {
            string str = "";
            Dictionary<string, string> properties = this.General.Properties;
            str = str + "General\r\n";
            foreach (string str2 in properties.Keys)
            {
                str = str + str2 + " : " + properties[str2].ToString() + "\r\n";
            }
            str = str + "\r\n";
            foreach (MediaInfo_Stream stream in this.AllStreams)
            {
                properties = stream.Properties;
                str = str + stream.StreamType + " #" + stream.StreamTypeIndex.ToString() + "\r\n";
                foreach (string str3 in properties.Keys)
                {
                    str = str + str3 + " : " + properties[str3].ToString() + "\r\n";
                }
                str = str + "\r\n";
            }
            this.Info_Text = str;
        }

        public void GetMediaInfo(bool AppendInfo)
        {
            if (!this.MediaInfo_Available && MyProject.Computer.FileSystem.FileExists(base.FullName))
            {
                MediaInfo info = new MediaInfo();
                info.Open(base.FullName);
                this.MediaInfo_Text = Strings.Trim(info.Inform());
                MediaInfo_Stream_General general = new MediaInfo_Stream_General();
                List<MediaInfo_Stream> streamList = new List<MediaInfo_Stream>();
                string[] strArray2 = Strings.Split(this.MediaInfo_Text, "\r\n\r\n", -1, CompareMethod.Binary);
                int num = -1;
                foreach (string str4 in strArray2)
                {
                    MediaInfo_Stream item = null;
                    string[] strArray = Strings.Split(str4, "\r\n", -1, CompareMethod.Binary);
                    string str3 = strArray[0];
                    if (str3 != "")
                    {
                        if (str3.Contains("General"))
                        {
                            item = new MediaInfo_Stream_General();
                            general = (MediaInfo_Stream_General) item;
                            if (base.Extension == ".avs")
                            {
                                item.Properties.Add("Format", "Avisynth Script");
                            }
                        }
                        else if (str3.Contains("Audio"))
                        {
                            item = new MediaInfo_Stream_Audio();
                            streamList.Add(item);
                            num++;
                            item.StreamIndex = num;
                        }
                        else if (str3.Contains("Video"))
                        {
                            item = new MediaInfo_Stream_Video();
                            streamList.Add(item);
                            num++;
                            item.StreamIndex = num;
                        }
                        else if (str3.Contains("Text"))
                        {
                            item = new MediaInfo_Stream_Text();
                            streamList.Add(item);
                            num++;
                            item.StreamIndex = num;
                        }
                        else if (str3.Contains("Image"))
                        {
                            item = new MediaInfo_Stream_Image();
                            streamList.Add(item);
                            num++;
                            item.StreamIndex = num;
                        }
                        else if (str3.Contains("Menu"))
                        {
                            item = new MediaInfo_Stream_Menu();
                            streamList.Add(item);
                            num++;
                            item.StreamIndex = num;
                        }
                        else if (str3.Contains("Chapters"))
                        {
                            item = new MediaInfo_Stream_Chapters();
                            streamList.Add(item);
                            num++;
                            item.StreamIndex = num;
                        }
                    }
                    if (item != null)
                    {
                        foreach (string str5 in strArray)
                        {
                            string[] strArray3 = Strings.Split(str5, " : ", -1, CompareMethod.Binary);
                            if (strArray3.Length > 1)
                            {
                                string key = strArray3[0].Trim();
                                string str2 = strArray3[1].Trim();
                                if (!item.Properties.ContainsKey(key))
                                {
                                    item.Properties.Add(key, str2);
                                }
                            }
                        }
                    }
                }
                foreach (MediaInfo_Stream stream3 in streamList)
                {
                    stream3.SourceFile = this;
                }
                bool flag = true;
                int count = streamList.Count;
                for (int i = 1; i <= count; i++)
                {
                    if (!flag)
                    {
                        break;
                    }
                    int num7 = streamList.Count - 2;
                    for (int j = 0; j <= num7; j++)
                    {
                        if (((streamList[j].ID != -1) & (streamList[j + 1].ID != -1)) && (streamList[j].ID > streamList[j + 1].ID))
                        {
                            MediaInfo_Stream stream = streamList[j];
                            streamList[j] = streamList[j + 1];
                            streamList[j + 1] = stream;
                            flag = true;
                        }
                    }
                }
                foreach (MediaInfo_Stream stream4 in streamList)
                {
                    stream4.StreamIndex = this.AllStreams.IndexOf(stream4);
                }
                this.setStreamTypeIndices(ref streamList);
                this.MediaInfo_Available = true;
                this.getHTML(ref this.MediaInfo_HTML, ref general, ref streamList);
                if (AppendInfo)
                {
                    this.addProperties(general, streamList, true);
                }
                info.Close();
            }
        }

        private void setStreamTypeIndices(ref List<MediaInfo_Stream> streamList)
        {
            List<MediaInfo_Stream>.Enumerator enumerator = new List<MediaInfo_Stream>.Enumerator();
            int num = -1;
            int num6 = -1;
            int num3 = -1;
            int num5 = -1;
            int num2 = -1;
            int num4 = -1;
            try
            {
                enumerator = streamList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    MediaInfo_Stream current = enumerator.Current;
                    current.StreamIndex = streamList.IndexOf(current);
                    if (current is MediaInfo_Stream_Video)
                    {
                        num6++;
                        current.StreamTypeIndex = num6;
                    }
                    else
                    {
                        if (current is MediaInfo_Stream_Audio)
                        {
                            num++;
                            current.StreamTypeIndex = num;
                            continue;
                        }
                        if (current is MediaInfo_Stream_Image)
                        {
                            num3++;
                            current.StreamTypeIndex = num3;
                            continue;
                        }
                        if (current is MediaInfo_Stream_Text)
                        {
                            num5++;
                            current.StreamTypeIndex = num5;
                            continue;
                        }
                        if (current is MediaInfo_Stream_Chapters)
                        {
                            num2++;
                            current.StreamTypeIndex = num2;
                            continue;
                        }
                        if (current is MediaInfo_Stream_Menu)
                        {
                            num4++;
                            current.StreamTypeIndex = num4;
                        }
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        public long FileSize
        {
            get
            {
                if (MyProject.Computer.FileSystem.FileExists(base.FullName))
                {
                    return MyProject.Computer.FileSystem.GetFileInfo(base.FullName).Length;
                }
                return 0L;
            }
        }

        public int FrameCount
        {
            get
            {
                if ((this.Video.Count > 0) && (!(this.Video[0].FrameRate == 0.0) & (this.General.DurationMillis != 0L)))
                {
                    return (int) Math.Round(Math.Ceiling((double) ((this.Video[0].FrameRate * this.General.DurationMillis) / 1000.0)));
                }
                return 0;
            }
        }

        public bool InfoAvailable
        {
            get
            {
                return ((this.Audio.Count > 0) | (this.Video.Count > 0));
            }
        }

        public int StreamCount
        {
            get
            {
                return ((this.Audio.Count + this.Video.Count) + this.Text.Count);
            }
        }
    }
}

