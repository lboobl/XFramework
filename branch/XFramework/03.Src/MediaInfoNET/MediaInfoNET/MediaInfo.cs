namespace MediaInfoNET
{
    using System;
    using System.Runtime.InteropServices;

    public class MediaInfo
    {
        private IntPtr Handle = MediaInfo_New();

        public void Close()
        {
            MediaInfo_Close(this.Handle);
        }

        public int Count_Get(StreamKind StreamKind, uint StreamNumber = 0xffffffff)
        {
            if (StreamNumber == uint.MaxValue)
            {
                return (int) ((uint) MediaInfo_Count_Get(this.Handle, (UIntPtr) StreamKind, (IntPtr) (-1)));
            }
            return (int) ((uint) MediaInfo_Count_Get(this.Handle, (UIntPtr) StreamKind, (IntPtr) StreamNumber));
        }

        ~MediaInfo()
        {
            MediaInfo_Delete(this.Handle);
        }

        public string Get_(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo = InfoKind.Text)
        {
            return Marshal.PtrToStringUni(MediaInfo_GetI(this.Handle, (UIntPtr) StreamKind, (UIntPtr) ((ulong) StreamNumber), (UIntPtr) ((ulong) Parameter), (UIntPtr) KindOfInfo));
        }

        public string Get_(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo = InfoKind.Text, InfoKind KindOfSearch = 0)
        {
            return Marshal.PtrToStringUni(MediaInfo_Get(this.Handle, (UIntPtr) StreamKind, (UIntPtr) ((ulong) StreamNumber), ref Parameter, (UIntPtr) KindOfInfo, (UIntPtr) KindOfSearch));
        }

        public string Inform()
        {
            return Marshal.PtrToStringUni(MediaInfo_Inform(this.Handle, (UIntPtr) 0L));
        }

        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern void MediaInfo_Close(IntPtr Handle);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern UIntPtr MediaInfo_Count_Get(IntPtr Handle, UIntPtr StreamKind, IntPtr StreamNumber);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern void MediaInfo_Delete(IntPtr Handle);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr MediaInfo_Get(IntPtr Handle, UIntPtr StreamKind, UIntPtr StreamNumber, [MarshalAs(UnmanagedType.VBByRefStr)] ref string Parameter, UIntPtr KindOfInfo, UIntPtr KindOfSearch);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr MediaInfo_GetI(IntPtr Handle, UIntPtr StreamKind, UIntPtr StreamNumber, UIntPtr Parameter, UIntPtr KindOfInfo);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr MediaInfo_Inform(IntPtr Handle, UIntPtr Reserved);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr MediaInfo_New();
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern UIntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.VBByRefStr)] ref string FileName);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.VBByRefStr)] ref string Option_, [MarshalAs(UnmanagedType.VBByRefStr)] ref string Value);
        [DllImport("MediaInfo.DLL", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern UIntPtr MediaInfo_State_Get(IntPtr Handle);
        public UIntPtr Open(string FileName)
        {
            return MediaInfo_Open(this.Handle, ref FileName);
        }

        public string Option_(string Option__, string Value = "")
        {
            return Marshal.PtrToStringUni(MediaInfo_Option(this.Handle, ref Option__, ref Value));
        }

        public int State_Get()
        {
            return (int) ((uint) MediaInfo_State_Get(this.Handle));
        }
    }
}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//// MediaInfoDLL - All info about media files, for DLL
//// Copyright (C) 2002-2009 Jerome Martinez, Zen@MediaArea.net
////
//// This library is free software; you can redistribute it and/or
//// modify it under the terms of the GNU Lesser General Public
//// License as published by the Free Software Foundation; either
//// version 2.1 of the License, or (at your option) any later version.
////
//// This library is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//// Lesser General Public License for more details.
////
//// MediaInfoDLL - All info about media files, for DLL
//// Copyright (C) 2002-2009 Jerome Martinez, Zen@MediaArea.net
////
//// This library is free software; you can redistribute it and/or
//// modify it under the terms of the GNU Lesser General Public
//// License as published by the Free Software Foundation; either
//// version 2.1 of the License, or (at your option) any later version.
////
//// This library is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//// Lesser General Public License for more details.
////
//// You should have received a copy of the GNU Lesser General Public
//// License along with this library; if not, write to the Free Software
//// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
////
////+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
////+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
////
//// Microsoft Visual C# wrapper for MediaInfo Library
//// See MediaInfo.h for help
////
//// To make it working, you must put MediaInfo.Dll
//// in the executable folder
////
////+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

//using System;
//using System.Runtime.InteropServices;

//namespace TXEnterprise.Common
//{

///// <summary>
///// 多媒体类型
///// </summary>
//public enum MediaType
//{
//    /// <summary>
//    /// 全局
//    /// </summary>
//    General,
//    /// <summary>
//    /// 视频
//    /// </summary>
//    Video,

//    /// <summary>
//    /// 音频
//    /// </summary>
//    Audio,

//    /// <summary>
//    /// 文本
//    /// </summary>
//    Text,

//    /// <summary>
//    /// 段?
//    /// </summary>
//    Chapters,

//    /// <summary>
//    /// 图片
//    /// </summary>
//    Image
//}


//    public enum InfoKind
//    {
//        Name,
//        Text,
//        Measure,
//        Options,
//        NameText,
//        MeasureText,
//        Info,
//        HowTo
//    }

//    public enum InfoOptions
//    {
//        ShowInInform,
//        Support,
//        ShowInSupported,
//        TypeOfValue
//    }

//    public enum InfoFileOptions
//    {
//        FileOption_Nothing = 0x00,
//        FileOption_NoRecursive = 0x01,
//        FileOption_CloseAll = 0x02,
//        FileOption_Max = 0x04
//    };


//    public class MediaInfo : IDisposable
//    {
//        #region DLL导入

//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_New();
//        [DllImport("MediaInfo.dll")]
//        private static extern void MediaInfo_Delete(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Open(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Open(IntPtr handle, IntPtr fileName);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Open_Buffer_Init(IntPtr handle, Int64 fileSize, Int64 fileOffset);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Open(IntPtr handle, Int64 fileSize, Int64 fileOffset);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Open_Buffer_Continue(IntPtr handle, IntPtr buffer, IntPtr bufferSize);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Open_Buffer_Continue(IntPtr handle, Int64 fileSize, byte[] buffer, IntPtr bufferSize);
//        [DllImport("MediaInfo.dll")]
//        private static extern Int64 MediaInfo_Open_Buffer_Continue_GoTo_Get(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern Int64 MediaInfoA_Open_Buffer_Continue_GoTo_Get(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Open_Buffer_Finalize(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Open_Buffer_Finalize(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern void MediaInfo_Close(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Inform(IntPtr handle, IntPtr reserved);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Inform(IntPtr handle, IntPtr reserved);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_GetI(IntPtr handle, IntPtr streamKind, IntPtr streamNumber, IntPtr parameter, IntPtr kindOfInfo);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_GetI(IntPtr handle, IntPtr streamKind, IntPtr streamNumber, IntPtr parameter, IntPtr kindOfInfo);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber, [MarshalAs(UnmanagedType.LPWStr)] string parameter, IntPtr kindOfInfo, IntPtr kindOfSearch);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber, IntPtr parameter, IntPtr kindOfInfo, IntPtr kindOfSearch);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Option(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string option, [MarshalAs(UnmanagedType.LPWStr)] string value);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoA_Option(IntPtr handle, IntPtr option, IntPtr value);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_State_Get(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfo_Count_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber);

//        #endregion

//        #region 字段

//        private IntPtr _Handle;         //文件句柄
//        private bool isAnsi;            //是否使用ansi格式?

//        #endregion

//        #region 属性

//        #endregion

//        #region 构造

//        /// <summary>
//        /// 创建MediaInfo实例
//        /// </summary>
//        public MediaInfo()
//        {
//            _Handle = MediaInfo_New();
//            isAnsi = Environment.OSVersion.ToString().IndexOf("Windows") == -1;
//        }

//        /// <summary>
//        /// 析构
//        /// </summary>
//        ~MediaInfo()
//        {
//            MediaInfo_Delete(_Handle);
//        }

//        #endregion

//        #region 公开方法

//        /// <summary>
//        /// 打开指定文件
//        /// </summary>
//        /// <param name="fullName"></param>
//        /// <returns></returns>
//        public int Open(string fullName)
//        {
//            if (!isAnsi) return (int)MediaInfo_Open(_Handle, fullName);

//            IntPtr ptr = Marshal.StringToHGlobalAnsi(fullName);
//            int value = (int)MediaInfoA_Open(_Handle, ptr);
//            Marshal.FreeHGlobal(ptr);
//            return value;
//        }

//        public int Open_Buffer_Init(Int64 fileSize, Int64 fileOffset)
//        {
//            return (int)MediaInfo_Open_Buffer_Init(_Handle, fileSize, fileOffset);
//        }

//        public int Open_Buffer_Continue(IntPtr buffer, IntPtr bufferSize)
//        {
//            return (int)MediaInfo_Open_Buffer_Continue(_Handle, buffer, bufferSize);
//        }

//        public Int64 Open_Buffer_Continue_GoTo_Get()
//        {
//            return (int)MediaInfo_Open_Buffer_Continue_GoTo_Get(_Handle);
//        }

//        public int Open_Buffer_Finalize()
//        {
//            return (int)MediaInfo_Open_Buffer_Finalize(_Handle);
//        }

//        /// <summary>
//        /// 返回一个概要性的信息
//        /// </summary>
//        /// <returns></returns>
//        public string Inform()
//        {
//            return isAnsi ? Marshal.PtrToStringAnsi(MediaInfoA_Inform(_Handle, (IntPtr)0)) : 
//                Marshal.PtrToStringUni(MediaInfo_Inform(_Handle, (IntPtr)0));
//        }


//        public string Get(MediaType mediaType, int streamNum, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
//        {
//            if(!isAnsi) return Marshal.PtrToStringUni(MediaInfo_Get(_Handle, (IntPtr)mediaType, (IntPtr)streamNum, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));

//            IntPtr ptr = Marshal.StringToHGlobalAnsi(parameter);
//            string value = Marshal.PtrToStringAnsi(MediaInfoA_Get(_Handle, (IntPtr)mediaType, (IntPtr)streamNum, ptr, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
//            Marshal.FreeHGlobal(ptr);
//            return value;
//        }


//        public string Get(MediaType mediaType, int streamNum, int parameter, InfoKind kindOfInfo)
//        {
//            return isAnsi ? Marshal.PtrToStringAnsi(MediaInfoA_GetI(_Handle, (IntPtr)mediaType, (IntPtr)streamNum, (IntPtr)parameter, (IntPtr)kindOfInfo)) : 
//                Marshal.PtrToStringUni(MediaInfo_GetI(_Handle, (IntPtr)mediaType, (IntPtr)streamNum, (IntPtr)parameter, (IntPtr)kindOfInfo));
//        }

//        public string Get(MediaType mediaType, int streamNum, string parameter, InfoKind kindOfInfo)
//        {
//            return Get(mediaType, streamNum, parameter, kindOfInfo, InfoKind.Name);
//        }


//        public string Get(MediaType mediaType, int streamNum, string parameter)
//        {
//            return Get(mediaType, streamNum, parameter, InfoKind.Text, InfoKind.Name);
//        }

//        public string Get(MediaType mediaType, int streamNum, int parameter)
//        {
//            return Get(mediaType, streamNum, parameter, InfoKind.Text);
//        }

//        public string Option(string opt, string Value)
//        {
//            if(!isAnsi) return Marshal.PtrToStringUni(MediaInfo_Option(_Handle, opt, Value));

//            IntPtr optPtr = Marshal.StringToHGlobalAnsi(opt);
//            IntPtr valuePtr = Marshal.StringToHGlobalAnsi(Value);

//            string value = Marshal.PtrToStringAnsi(MediaInfoA_Option(_Handle, optPtr, valuePtr));
//            Marshal.FreeHGlobal(optPtr);
//            Marshal.FreeHGlobal(valuePtr);
//            return value;

//        }

//        public string Option(string opt)
//        {
//            return Option(opt, "");
//        }

//        public int State_Get()
//        {
//            return (int)MediaInfo_State_Get(_Handle);
//        }

//        public int Count_Get(MediaType mediaType, int streamNum)
//        {
//            return (int)MediaInfo_Count_Get(_Handle, (IntPtr)mediaType, (IntPtr)streamNum);
//        }

//        public int Count_Get(MediaType mediaType)
//        {
//            return Count_Get(mediaType, -1);
//        }

//        #endregion

//        #region 辅助方法

//        /// <summary>
//        /// 释放叛逃
//        /// </summary>
//        protected virtual void Close()
//        {
//            MediaInfo_Close(_Handle);
//        }

//        #endregion

//        #region IDisposable成员

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        public void Dispose()
//        {
//            this.Close();
//        }

//        #endregion        
//    }

//    public class MediaInfoList : IDisposable
//    {
//        #region DLL导入

//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_New();
//        [DllImport("MediaInfo.dll")]
//        private static extern void MediaInfoList_Delete(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_Open(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName, IntPtr options);
//        [DllImport("MediaInfo.dll")]
//        private static extern void MediaInfoList_Close(IntPtr handle, IntPtr filePos);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_Inform(IntPtr handle, IntPtr filePos, IntPtr reserved);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_GetI(IntPtr handle, IntPtr filePos, IntPtr streamKind, IntPtr streamNum, IntPtr parameter, IntPtr kindOfInfo);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_Get(IntPtr handle, IntPtr filePos, IntPtr streamKind, IntPtr streamNum, [MarshalAs(UnmanagedType.LPWStr)] string parameter, IntPtr kindOfInfo, IntPtr kindOfSearch);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_Option(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string option, [MarshalAs(UnmanagedType.LPWStr)] string value);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_State_Get(IntPtr handle);
//        [DllImport("MediaInfo.dll")]
//        private static extern IntPtr MediaInfoList_Count_Get(IntPtr handle, IntPtr filePos, IntPtr streamKind, IntPtr streamNum);

//        #endregion

//        #region 字段

//        private IntPtr _Handle;

//        #endregion

//        #region 属性

//        #endregion

//        #region 构造

//        /// <summary>
//        /// 构造MediaInfoList实例
//        /// </summary>
//        public MediaInfoList()
//        {
//            _Handle = MediaInfoList_New();
//        }

//        /// <summary>
//        /// 析构
//        /// </summary>
//        ~MediaInfoList()
//        {
//            MediaInfoList_Delete(_Handle);
//        }

//        #endregion

//        #region 公开方法

//        /// <summary>
//        /// 打开指定文件
//        /// </summary>
//        public int Open(string fullName, InfoFileOptions options)
//        {
//            return (int)MediaInfoList_Open(_Handle, fullName, (IntPtr)options);
//        }

//        /// <summary>
//        /// 打开指定文件
//        /// </summary>
//        public void Open(String fullName)
//        {
//            this.Open(fullName, 0);
//        }

//        /// <summary>
//        /// 返回概要信息
//        /// </summary>
//        /// <param name="filePos"></param>
//        /// <returns></returns>
//        public string Inform(int filePos)
//        {
//            return Marshal.PtrToStringUni(MediaInfoList_Inform(_Handle, (IntPtr)filePos, (IntPtr)0));
//        }

//        public string Get(int filePos, MediaType mediaType, int streamNum, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
//        {
//            return Marshal.PtrToStringUni(MediaInfoList_Get(_Handle, (IntPtr)filePos, (IntPtr)mediaType, (IntPtr)streamNum, parameter, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
//        }

//        public String Get(int filePos, MediaType mediaType, int streamNum, int parameter, InfoKind infoKind)
//        {
//            return Marshal.PtrToStringUni(MediaInfoList_GetI(_Handle, (IntPtr)filePos, (IntPtr)mediaType, (IntPtr)streamNum, (IntPtr)parameter, (IntPtr)infoKind));
//        }

//        public String Get(int filePos, MediaType mediaType, int streamNum, string parameter, InfoKind kindOfInfo)
//        {
//            return Get(filePos, mediaType, streamNum, parameter, kindOfInfo, InfoKind.Name);
//        }

//        public String Get(int filePos, MediaType mediaType, int streamNum, string parameter)
//        {
//            return Get(filePos, mediaType, streamNum, parameter, InfoKind.Text, InfoKind.Name);
//        }

//        public String Get(int filePos, MediaType mediaType, int streamNum, int parameter)
//        {
//            return Get(filePos, mediaType, streamNum, parameter, InfoKind.Text);
//        }

//        public string Option(string option, string value)
//        {
//            return Marshal.PtrToStringUni(MediaInfoList_Option(_Handle, option, value));
//        }

//        public String Option(string option)
//        {
//            return Option(option, "");
//        }

//        public int State_Get()
//        {
//            return (int)MediaInfoList_State_Get(_Handle);
//        }

//        public int Count_Get(int filePos, MediaType mediaType, int streamNum)
//        {
//            return (int)MediaInfoList_Count_Get(_Handle, (IntPtr)filePos, (IntPtr)mediaType, (IntPtr)streamNum);
//        }

//        public int Count_Get(int filePos, MediaType mediaType)
//        {
//            return Count_Get(filePos, mediaType, -1);
//        }

//        #endregion

//        #region 辅助方法

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        protected void Close(int filePos)
//        {
//            MediaInfoList_Close(_Handle, (IntPtr)filePos);
//        }

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        protected void Close()
//        {
//            this.Close(-1);
//        }

//        #endregion

//        #region IDispose

//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        public void Dispose()
//        {
//            this.Close();
//        }

//        #endregion
//    }

//}

