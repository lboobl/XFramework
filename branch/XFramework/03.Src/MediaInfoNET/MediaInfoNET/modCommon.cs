namespace MediaInfoNET
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;

    [StandardModule]
    internal sealed class modCommon
    {
        public const long GB = 0x40000000L;
        public const int HR = 0xe10;
        public const long KB = 0x400L;
        public const long MB = 0x100000L;
        public const int MIN = 60;

        public static string FormatFileSize(long size)
        {
            double num2;
            if (size <= 0L)
            {
                return "";
            }
            long num = size;
            if ((num >= 0L) && (num <= 0x400L))
            {
                return (size.ToString() + " Bytes");
            }
            if ((num >= 0x400L) && (num <= 0x19000L))
            {
                num2 = ((double) size) / 1024.0;
                return (num2.ToString("#,###") + " KB");
            }
            if ((num >= 0x19000L) && (num <= 0x100000L))
            {
                num2 = ((double) size) / 1024.0;
                return (num2.ToString("#,###") + " KB");
            }
            if ((num >= 0x100000L) && (num <= 0x6400000L))
            {
                num2 = ((double) size) / 1048576.0;
                return (num2.ToString("#,###") + " MB");
            }
            if ((num >= 0x6400000L) && (num <= 0x40000000L))
            {
                num2 = ((double) size) / 1048576.0;
                return (num2.ToString("#,###") + " MB");
            }
            if ((num >= 0x40000000L) && (num <= 0x1900000000L))
            {
                num2 = ((double) size) / 1073741824.0;
                return (num2.ToString("#,###.#") + " GB");
            }
            num2 = ((double) size) / 1073741824.0;
            return (num2.ToString("#,###") + " GB");
        }

        public static long GetMillisFromString(string TimeString)
        {
            double num3;
            long num2 = 0L;
            if (TimeString.Contains(":"))
            {
                TimeSpan result = new TimeSpan();
                if (TimeSpan.TryParse(TimeString, out result))
                {
                    num2 = (long) Math.Round(Math.Floor(result.TotalMilliseconds));
                }
                return num2;
            }
            if (double.TryParse(TimeString, out num3))
            {
                long num4 = (long) Math.Round(Math.Floor((double) (num3 * 1000.0)));
                num2 = (long) Math.Round(Math.Floor((double) (num3 * 1000.0)));
            }
            return num2;
        }

        public static long ParseTimeSpan(string TimeString)
        {
            double num3;
            long num = 0L;
            if (TimeString.Contains(":"))
            {
                TimeSpan result = new TimeSpan();
                if (TimeSpan.TryParse(TimeString, out result))
                {
                    num = (long) Math.Round(Math.Floor(result.TotalMilliseconds));
                }
                return num;
            }
            if (double.TryParse(TimeString, out num3))
            {
                long num4 = (long) Math.Round(Math.Floor((double) (num3 * 1000.0)));
                num = (long) Math.Round(Math.Floor((double) (num3 * 1000.0)));
            }
            return num;
        }
    }
}

