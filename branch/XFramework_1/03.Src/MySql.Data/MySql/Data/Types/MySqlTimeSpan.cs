﻿namespace MySql.Data.Types
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Data;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MySqlTimeSpan : IMySqlValue
    {
        private TimeSpan mValue;
        private bool isNull;
        public MySqlTimeSpan(bool isNull)
        {
            this.isNull = isNull;
            this.mValue = TimeSpan.MinValue;
        }

        public MySqlTimeSpan(TimeSpan val)
        {
            this.isNull = false;
            this.mValue = val;
        }

        public bool IsNull
        {
            get
            {
                return this.isNull;
            }
        }
        MySqlDbType IMySqlValue.MySqlDbType
        {
            get
            {
                return MySqlDbType.Time;
            }
        }
        DbType IMySqlValue.DbType
        {
            get
            {
                return DbType.Time;
            }
        }
        object IMySqlValue.Value
        {
            get
            {
                return this.mValue;
            }
        }
        public TimeSpan Value
        {
            get
            {
                return this.mValue;
            }
        }
        Type IMySqlValue.SystemType
        {
            get
            {
                return typeof(TimeSpan);
            }
        }
        string IMySqlValue.MySqlTypeName
        {
            get
            {
                return "TIME";
            }
        }
        void IMySqlValue.WriteValue(MySqlStream stream, bool binary, object val, int length)
        {
            if (!(val is TimeSpan))
            {
                throw new MySqlException("Only TimeSpan objects can be serialized by MySqlTimeSpan");
            }
            TimeSpan span = (TimeSpan) val;
            if (binary)
            {
                stream.WriteByte(8);
                stream.WriteByte((span.TotalSeconds < 0.0) ? ((byte) 1) : ((byte) 0));
                stream.WriteInteger((long) span.Days, 4);
                stream.WriteByte((byte) span.Hours);
                stream.WriteByte((byte) span.Minutes);
                stream.WriteByte((byte) span.Seconds);
            }
            else
            {
                stream.WriteStringNoNull(string.Format("'{0} {1:00}:{2:00}:{3:00}.{4}'", new object[] { span.Days, span.Hours, span.Minutes, span.Seconds, span.Milliseconds }));
            }
        }

        IMySqlValue IMySqlValue.ReadValue(MySqlStream stream, long length, bool nullVal)
        {
            if (nullVal)
            {
                return new MySqlTimeSpan(true);
            }
            if (length >= 0)
            {
                string s = stream.ReadString(length);
                this.ParseMySql(s, stream.Version.isAtLeast(4, 1, 0));
                return this;
            }
            long num = stream.ReadByte();
            int num2 = 0;
            if (num > 0)
            {
                num2 = stream.ReadByte();
            }
            this.isNull = false;
            switch (num)
            {
                case 0:
                    this.isNull = true;
                    break;

                case 5:
                    this.mValue = new TimeSpan(stream.ReadInteger(4), 0, 0, 0);
                    break;

                case 8:
                    this.mValue = new TimeSpan(stream.ReadInteger(4), stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
                    break;

                default:
                    this.mValue = new TimeSpan(stream.ReadInteger(4), stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadInteger(4) / 0xf4240);
                    break;
            }
            if (num2 == 1)
            {
                this.mValue = this.mValue.Negate();
            }
            return this;
        }

        void IMySqlValue.SkipValue(MySqlStream stream)
        {
            int len = stream.ReadByte();
            stream.SkipBytes(len);
        }

        internal static void SetDSInfo(DataTable dsTable)
        {
            DataRow row = dsTable.NewRow();
            row["TypeName"] = "TIME";
            row["ProviderDbType"] = MySqlDbType.Time;
            row["ColumnSize"] = 0;
            row["CreateFormat"] = "TIME";
            row["CreateParameters"] = null;
            row["DataType"] = "System.TimeSpan";
            row["IsAutoincrementable"] = false;
            row["IsBestMatch"] = true;
            row["IsCaseSensitive"] = false;
            row["IsFixedLength"] = true;
            row["IsFixedPrecisionScale"] = true;
            row["IsLong"] = false;
            row["IsNullable"] = true;
            row["IsSearchable"] = true;
            row["IsSearchableWithLike"] = false;
            row["IsUnsigned"] = false;
            row["MaximumScale"] = 0;
            row["MinimumScale"] = 0;
            row["IsConcurrencyType"] = DBNull.Value;
            row["IsLiteralsSupported"] = false;
            row["LiteralPrefix"] = null;
            row["LiteralSuffix"] = null;
            row["NativeDataType"] = null;
            dsTable.Rows.Add(row);
        }

        public override string ToString()
        {
            return string.Format("{0} {1:00}:{2:00}:{3:00}.{4}", new object[] { this.mValue.Days, this.mValue.Hours, this.mValue.Minutes, this.mValue.Seconds, this.mValue.Milliseconds });
        }

        private void ParseMySql(string s, bool is41)
        {
            string[] strArray = s.Split(new char[] { ':' });
            int hours = int.Parse(strArray[0]);
            int minutes = int.Parse(strArray[1]);
            int seconds = int.Parse(strArray[2]);
            if (hours < 0)
            {
                minutes *= -1;
                seconds *= -1;
            }
            int days = hours / 0x18;
            hours -= days * 0x18;
            this.mValue = new TimeSpan(days, hours, minutes, seconds, 0);
            this.isNull = false;
        }
    }
}

