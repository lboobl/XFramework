namespace MediaInfoNET
{
    using System;
    using System.IO;

    public class File
    {
        public string Extension;
        public string FullName;
        public string Name;
        public string ParentFolder;
        public string Title;

        public File(string SourceFile)
        {
            if (SourceFile != "")
            {
                this.FullName = SourceFile;
                this.Name = Path.GetFileName(SourceFile);
                this.Title = Path.GetFileNameWithoutExtension(SourceFile);
                this.Extension = Path.GetExtension(SourceFile).ToLower();
                this.ParentFolder = Path.GetDirectoryName(SourceFile);
            }
        }
    }
}

