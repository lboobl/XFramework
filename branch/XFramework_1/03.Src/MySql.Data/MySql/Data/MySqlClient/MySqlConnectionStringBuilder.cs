namespace MySql.Data.MySqlClient
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    public sealed class MySqlConnectionStringBuilder : DbConnectionStringBuilder
    {
        private bool allowBatch;
        private bool allowZeroDatetime;
        private bool autoEnlist;
        private string blobAsUtf8ExcludePattern;
        private Regex blobAsUtf8ExcludeRegex;
        private string blobAsUtf8IncludePattern;
        private Regex blobAsUtf8IncludeRegex;
        private string charSet;
        private bool compress;
        private uint connectionLifetime;
        private bool connectionReset;
        private uint connectionTimeout;
        private bool convertZeroDatetime;
        private string database;
        private uint defaultCommandTimeout;
        private static Dictionary<Keyword, object> defaultValues = new Dictionary<Keyword, object>();
        private MySqlDriverType driverType;
        private bool ignorePrepare;
        private bool logging;
        private uint maxPoolSize;
        private uint minPoolSize;
        private bool oldSyntax;
        private string originalConnectionString;
        private string password;
        private StringBuilder persistConnString;
        private bool persistSI;
        private string pipeName;
        private bool pooling;
        private uint port;
        private uint procCacheSize;
        private MySqlConnectionProtocol protocol;
        private bool respectBinaryFlags;
        private string server;
        private string sharedMemName;
        private bool treatBlobsAsUTF8;
        private bool treatTinyAsBoolean;
        private bool usePerfMon;
        private bool useProcedureBodies;
        private string userId;
        private bool useSSL;
        private bool useUsageAdvisor;

        static MySqlConnectionStringBuilder()
        {
            defaultValues.Add(Keyword.ConnectionTimeout, 15);
            defaultValues.Add(Keyword.Pooling, true);
            defaultValues.Add(Keyword.Port, 0xcea);
            defaultValues.Add(Keyword.Server, "");
            defaultValues.Add(Keyword.PersistSecurityInfo, false);
            defaultValues.Add(Keyword.ConnectionLifetime, 0);
            defaultValues.Add(Keyword.ConnectionReset, false);
            defaultValues.Add(Keyword.MinimumPoolSize, 0);
            defaultValues.Add(Keyword.MaximumPoolSize, 100);
            defaultValues.Add(Keyword.UserID, "");
            defaultValues.Add(Keyword.Password, "");
            defaultValues.Add(Keyword.UseUsageAdvisor, false);
            defaultValues.Add(Keyword.CharacterSet, "");
            defaultValues.Add(Keyword.Compress, false);
            defaultValues.Add(Keyword.PipeName, "MYSQL");
            defaultValues.Add(Keyword.Logging, false);
            defaultValues.Add(Keyword.OldSyntax, false);
            defaultValues.Add(Keyword.SharedMemoryName, "MYSQL");
            defaultValues.Add(Keyword.AllowBatch, true);
            defaultValues.Add(Keyword.ConvertZeroDatetime, false);
            defaultValues.Add(Keyword.Database, "");
            defaultValues.Add(Keyword.DriverType, MySqlDriverType.Native);
            defaultValues.Add(Keyword.Protocol, MySqlConnectionProtocol.Sockets);
            defaultValues.Add(Keyword.AllowZeroDatetime, false);
            defaultValues.Add(Keyword.UsePerformanceMonitor, false);
            defaultValues.Add(Keyword.ProcedureCacheSize, 0x19);
            defaultValues.Add(Keyword.UseSSL, false);
            defaultValues.Add(Keyword.IgnorePrepare, true);
            defaultValues.Add(Keyword.UseProcedureBodies, true);
            defaultValues.Add(Keyword.AutoEnlist, true);
            defaultValues.Add(Keyword.RespectBinaryFlags, true);
            defaultValues.Add(Keyword.BlobAsUTF8ExcludePattern, null);
            defaultValues.Add(Keyword.BlobAsUTF8IncludePattern, null);
            defaultValues.Add(Keyword.TreatBlobsAsUTF8, false);
            defaultValues.Add(Keyword.DefaultCommandTimeout, 30);
            defaultValues.Add(Keyword.TreatTinyAsBoolean, true);
        }

        public MySqlConnectionStringBuilder()
        {
            this.persistConnString = new StringBuilder();
            this.Clear();
        }

        public MySqlConnectionStringBuilder(string connectionString) : this()
        {
            this.originalConnectionString = connectionString;
            this.persistConnString = new StringBuilder();
            base.ConnectionString = connectionString;
        }

        public override void Clear()
        {
            base.Clear();
            this.persistConnString.Remove(0, this.persistConnString.Length);
            foreach (KeyValuePair<Keyword, object> pair in defaultValues)
            {
                this.SetValue(pair.Key, pair.Value);
            }
        }

        private static bool ConvertToBool(object value)
        {
            bool flag;
            if (value is string)
            {
                string str = value.ToString().ToLower();
                switch (str)
                {
                    case "yes":
                    case "true":
                        return true;
                }
                if (!(str == "no") && !(str == "false"))
                {
                    throw new ArgumentException(Resources.ImproperValueFormat, (string) value);
                }
                return false;
            }
            try
            {
                flag = (value as IConvertible).ToBoolean(CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Resources.ImproperValueFormat, value.ToString());
            }
            return flag;
        }

        private static MySqlDriverType ConvertToDriverType(object value)
        {
            if (value is MySqlDriverType)
            {
                return (MySqlDriverType) value;
            }
            return (MySqlDriverType) Enum.Parse(typeof(MySqlDriverType), value.ToString(), true);
        }

        private static MySqlConnectionProtocol ConvertToProtocol(object value)
        {
            try
            {
                if (value is MySqlConnectionProtocol)
                {
                    return (MySqlConnectionProtocol) value;
                }
                return (MySqlConnectionProtocol) Enum.Parse(typeof(MySqlConnectionProtocol), value.ToString(), true);
            }
            catch (Exception)
            {
                if (value is string)
                {
                    switch ((value as string).ToLower())
                    {
                        case "socket":
                        case "tcp":
                            return MySqlConnectionProtocol.Sockets;

                        case "pipe":
                            return MySqlConnectionProtocol.NamedPipe;

                        case "unix":
                            return MySqlConnectionProtocol.UnixSocket;

                        case "memory":
                            return MySqlConnectionProtocol.SharedMemory;
                    }
                }
            }
            throw new ArgumentException(Resources.ImproperValueFormat, value.ToString());
        }

        private static uint ConvertToUInt(object value)
        {
            uint num2;
            try
            {
                num2 = (value as IConvertible).ToUInt32(CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Resources.ImproperValueFormat, value.ToString());
            }
            return num2;
        }

        public string GetConnectionString(bool includePass)
        {
            if (includePass)
            {
                return this.originalConnectionString;
            }
            string str = this.persistConnString.ToString();
            return str.Remove(str.Length - 1, 1);
        }

        private static Keyword GetKey(string key)
        {
            switch (key.ToLower(CultureInfo.InvariantCulture))
            {
                case "uid":
                case "username":
                case "user id":
                case "user name":
                case "userid":
                case "user":
                    return Keyword.UserID;

                case "host":
                case "server":
                case "data source":
                case "datasource":
                case "address":
                case "addr":
                case "network address":
                    return Keyword.Server;

                case "password":
                case "pwd":
                    return Keyword.Password;

                case "useusageadvisor":
                case "usage advisor":
                case "use usage advisor":
                    return Keyword.UseUsageAdvisor;

                case "character set":
                case "charset":
                    return Keyword.CharacterSet;

                case "use compression":
                case "compress":
                    return Keyword.Compress;

                case "pipe name":
                case "pipe":
                    return Keyword.PipeName;

                case "logging":
                    return Keyword.Logging;

                case "use old syntax":
                case "old syntax":
                case "oldsyntax":
                    return Keyword.OldSyntax;

                case "shared memory name":
                    return Keyword.SharedMemoryName;

                case "allow batch":
                    return Keyword.AllowBatch;

                case "convert zero datetime":
                case "convertzerodatetime":
                    return Keyword.ConvertZeroDatetime;

                case "persist security info":
                    return Keyword.PersistSecurityInfo;

                case "initial catalog":
                case "database":
                    return Keyword.Database;

                case "connection timeout":
                case "connect timeout":
                    return Keyword.ConnectionTimeout;

                case "port":
                    return Keyword.Port;

                case "pooling":
                    return Keyword.Pooling;

                case "min pool size":
                case "minimum pool size":
                    return Keyword.MinimumPoolSize;

                case "max pool size":
                case "maximum pool size":
                    return Keyword.MaximumPoolSize;

                case "connection lifetime":
                    return Keyword.ConnectionLifetime;

                case "driver":
                    return Keyword.DriverType;

                case "protocol":
                case "connection protocol":
                    return Keyword.Protocol;

                case "allow zero datetime":
                case "allowzerodatetime":
                    return Keyword.AllowZeroDatetime;

                case "useperformancemonitor":
                case "use performance monitor":
                    return Keyword.UsePerformanceMonitor;

                case "procedure cache size":
                case "procedurecachesize":
                case "procedure cache":
                case "procedurecache":
                    return Keyword.ProcedureCacheSize;

                case "connection reset":
                    return Keyword.ConnectionReset;

                case "ignore prepare":
                    return Keyword.IgnorePrepare;

                case "encrypt":
                    return Keyword.UseSSL;

                case "procedure bodies":
                case "use procedure bodies":
                    return Keyword.UseProcedureBodies;

                case "auto enlist":
                    return Keyword.AutoEnlist;

                case "respect binary flags":
                    return Keyword.RespectBinaryFlags;

                case "blobasutf8excludepattern":
                    return Keyword.BlobAsUTF8ExcludePattern;

                case "blobasutf8includepattern":
                    return Keyword.BlobAsUTF8IncludePattern;

                case "treatblobsasutf8":
                case "treat blobs as utf8":
                    return Keyword.TreatBlobsAsUTF8;

                case "default command timeout":
                    return Keyword.DefaultCommandTimeout;

                case "treat tiny as boolean":
                    return Keyword.TreatTinyAsBoolean;
            }
            throw new ArgumentException(Resources.KeywordNotSupported, key);
        }

        protected override void GetProperties(Hashtable propertyDescriptors)
        {
            base.GetProperties(propertyDescriptors);
            PropertyDescriptor descriptor = (PropertyDescriptor) propertyDescriptors["Connection Protocol"];
            Attribute[] array = new Attribute[descriptor.Attributes.Count];
            descriptor.Attributes.CopyTo(array, 0);
            ConnectionProtocolDescriptor descriptor2 = new ConnectionProtocolDescriptor(descriptor.Name, array);
            propertyDescriptors["Connection Protocol"] = descriptor2;
        }

        private object GetValue(Keyword kw)
        {
            switch (kw)
            {
                case Keyword.UserID:
                    return this.UserID;

                case Keyword.Password:
                    return this.Password;

                case Keyword.Server:
                    return this.Server;

                case Keyword.Port:
                    return this.Port;

                case Keyword.UseUsageAdvisor:
                    return this.UseUsageAdvisor;

                case Keyword.CharacterSet:
                    return this.CharacterSet;

                case Keyword.Compress:
                    return this.UseCompression;

                case Keyword.PipeName:
                    return this.PipeName;

                case Keyword.Logging:
                    return this.Logging;

                case Keyword.OldSyntax:
                    return this.UseOldSyntax;

                case Keyword.SharedMemoryName:
                    return this.SharedMemoryName;

                case Keyword.AllowBatch:
                    return this.AllowBatch;

                case Keyword.ConvertZeroDatetime:
                    return this.ConvertZeroDateTime;

                case Keyword.PersistSecurityInfo:
                    return this.PersistSecurityInfo;

                case Keyword.Database:
                    return this.Database;

                case Keyword.ConnectionTimeout:
                    return this.ConnectionTimeout;

                case Keyword.Pooling:
                    return this.Pooling;

                case Keyword.MinimumPoolSize:
                    return this.MinimumPoolSize;

                case Keyword.MaximumPoolSize:
                    return this.MaximumPoolSize;

                case Keyword.ConnectionLifetime:
                    return this.ConnectionLifeTime;

                case Keyword.DriverType:
                    return this.DriverType;

                case Keyword.Protocol:
                    return this.ConnectionProtocol;

                case Keyword.ConnectionReset:
                    return this.ConnectionReset;

                case Keyword.AllowZeroDatetime:
                    return this.AllowZeroDateTime;

                case Keyword.UsePerformanceMonitor:
                    return this.UsePerformanceMonitor;

                case Keyword.ProcedureCacheSize:
                    return this.ProcedureCacheSize;

                case Keyword.IgnorePrepare:
                    return this.IgnorePrepare;

                case Keyword.UseSSL:
                    return this.UseSSL;

                case Keyword.UseProcedureBodies:
                    return this.UseProcedureBodies;

                case Keyword.AutoEnlist:
                    return this.AutoEnlist;

                case Keyword.RespectBinaryFlags:
                    return this.RespectBinaryFlags;

                case Keyword.TreatBlobsAsUTF8:
                    return this.TreatBlobsAsUTF8;

                case Keyword.BlobAsUTF8IncludePattern:
                    return this.blobAsUtf8IncludePattern;

                case Keyword.BlobAsUTF8ExcludePattern:
                    return this.blobAsUtf8ExcludePattern;

                case Keyword.DefaultCommandTimeout:
                    return this.defaultCommandTimeout;

                case Keyword.TreatTinyAsBoolean:
                    return this.treatTinyAsBoolean;
            }
            return null;
        }

        public override bool Remove(string keyword)
        {
            Keyword key = GetKey(keyword);
            this.SetValue(key, defaultValues[key]);
            return base.Remove(keyword);
        }

        private void SetValue(Keyword kw, object value)
        {
            switch (kw)
            {
                case Keyword.UserID:
                    this.userId = (string) value;
                    return;

                case Keyword.Password:
                    this.password = (string) value;
                    return;

                case Keyword.Server:
                    this.server = (string) value;
                    return;

                case Keyword.Port:
                    this.port = ConvertToUInt(value);
                    return;

                case Keyword.UseUsageAdvisor:
                    this.useUsageAdvisor = ConvertToBool(value);
                    return;

                case Keyword.CharacterSet:
                    this.charSet = (string) value;
                    return;

                case Keyword.Compress:
                    this.compress = ConvertToBool(value);
                    return;

                case Keyword.PipeName:
                    this.pipeName = (string) value;
                    return;

                case Keyword.Logging:
                    this.logging = ConvertToBool(value);
                    return;

                case Keyword.OldSyntax:
                    this.oldSyntax = ConvertToBool(value);
                    return;

                case Keyword.SharedMemoryName:
                    this.sharedMemName = (string) value;
                    return;

                case Keyword.AllowBatch:
                    this.allowBatch = ConvertToBool(value);
                    return;

                case Keyword.ConvertZeroDatetime:
                    this.convertZeroDatetime = ConvertToBool(value);
                    return;

                case Keyword.PersistSecurityInfo:
                    this.persistSI = ConvertToBool(value);
                    return;

                case Keyword.Database:
                    this.database = (string) value;
                    return;

                case Keyword.ConnectionTimeout:
                    this.connectionTimeout = ConvertToUInt(value);
                    return;

                case Keyword.Pooling:
                    this.pooling = ConvertToBool(value);
                    return;

                case Keyword.MinimumPoolSize:
                    this.minPoolSize = ConvertToUInt(value);
                    return;

                case Keyword.MaximumPoolSize:
                    this.maxPoolSize = ConvertToUInt(value);
                    return;

                case Keyword.ConnectionLifetime:
                    this.connectionLifetime = ConvertToUInt(value);
                    return;

                case Keyword.DriverType:
                    this.driverType = ConvertToDriverType(value);
                    return;

                case Keyword.Protocol:
                    this.protocol = ConvertToProtocol(value);
                    return;

                case Keyword.ConnectionReset:
                    this.connectionReset = ConvertToBool(value);
                    return;

                case Keyword.AllowZeroDatetime:
                    this.allowZeroDatetime = ConvertToBool(value);
                    return;

                case Keyword.UsePerformanceMonitor:
                    this.usePerfMon = ConvertToBool(value);
                    return;

                case Keyword.ProcedureCacheSize:
                    this.procCacheSize = ConvertToUInt(value);
                    return;

                case Keyword.IgnorePrepare:
                    this.ignorePrepare = ConvertToBool(value);
                    return;

                case Keyword.UseSSL:
                    this.useSSL = ConvertToBool(value);
                    return;

                case Keyword.UseProcedureBodies:
                    this.useProcedureBodies = ConvertToBool(value);
                    return;

                case Keyword.AutoEnlist:
                    this.autoEnlist = ConvertToBool(value);
                    return;

                case Keyword.RespectBinaryFlags:
                    this.respectBinaryFlags = ConvertToBool(value);
                    return;

                case Keyword.TreatBlobsAsUTF8:
                    this.treatBlobsAsUTF8 = ConvertToBool(value);
                    return;

                case Keyword.BlobAsUTF8IncludePattern:
                    this.blobAsUtf8IncludePattern = (string) value;
                    return;

                case Keyword.BlobAsUTF8ExcludePattern:
                    this.blobAsUtf8ExcludePattern = (string) value;
                    return;

                case Keyword.DefaultCommandTimeout:
                    this.defaultCommandTimeout = ConvertToUInt(value);
                    return;

                case Keyword.TreatTinyAsBoolean:
                    this.treatTinyAsBoolean = ConvertToBool(value);
                    return;
            }
        }

        private void SetValue(string keyword, object value)
        {
            object obj2;
            if (value == null)
            {
                throw new ArgumentException(Resources.KeywordNoNull, keyword);
            }
            this.TryGetValue(keyword, out obj2);
            Keyword key = GetKey(keyword);
            this.SetValue(key, value);
            base[keyword] = value;
            if (key != Keyword.Password)
            {
                this.persistConnString.Replace(string.Concat(new object[] { keyword, "=", obj2, ";" }), "");
                this.persistConnString.AppendFormat(CultureInfo.InvariantCulture, "{0}={1};", new object[] { keyword, value });
            }
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            try
            {
                Keyword key = GetKey(keyword);
                value = this.GetValue(key);
                return true;
            }
            catch (ArgumentException)
            {
            }
            value = null;
            return false;
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(true), Category("Connection"), DisplayName("Allow Batch"), Description("Allows execution of multiple SQL commands in a single statement")]
        public bool AllowBatch
        {
            get
            {
                return this.allowBatch;
            }
            set
            {
                this.SetValue("Allow Batch", value);
                this.allowBatch = value;
            }
        }

        [DefaultValue(false), RefreshProperties(RefreshProperties.All), Category("Advanced"), DisplayName("Allow Zero Datetime"), Description("Should zero datetimes be supported")]
        public bool AllowZeroDateTime
        {
            get
            {
                return this.allowZeroDatetime;
            }
            set
            {
                this.SetValue("Allow Zero Datetime", value);
                this.allowZeroDatetime = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), Description("Should the connetion automatically enlist in the active connection, if there are any."), DefaultValue(true), Category("Advanced"), DisplayName("Auto Enlist")]
        public bool AutoEnlist
        {
            get
            {
                return this.autoEnlist;
            }
            set
            {
                this.SetValue("Auto Enlist", value);
                this.autoEnlist = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("BlobAsUTF8ExcludePattern"), Category("Advanced"), Description("Pattern that matches columns that should not be treated as UTF8")]
        public string BlobAsUTF8ExcludePattern
        {
            get
            {
                return this.blobAsUtf8ExcludePattern;
            }
            set
            {
                this.SetValue("BlobAsUTF8ExcludePattern", value);
                this.blobAsUtf8ExcludePattern = value;
                this.blobAsUtf8ExcludeRegex = null;
            }
        }

        internal Regex BlobAsUTF8ExcludeRegex
        {
            get
            {
                if (this.blobAsUtf8ExcludePattern == null)
                {
                    return null;
                }
                if (this.blobAsUtf8ExcludeRegex == null)
                {
                    this.blobAsUtf8ExcludeRegex = new Regex(this.blobAsUtf8ExcludePattern);
                }
                return this.blobAsUtf8ExcludeRegex;
            }
        }

        [DisplayName("BlobAsUTF8IncludePattern"), Description("Pattern that matches columns that should be treated as UTF8"), RefreshProperties(RefreshProperties.All), Category("Advanced")]
        public string BlobAsUTF8IncludePattern
        {
            get
            {
                return this.blobAsUtf8IncludePattern;
            }
            set
            {
                this.SetValue("BlobAsUTF8IncludePattern", value);
                this.blobAsUtf8IncludePattern = value;
                this.blobAsUtf8IncludeRegex = null;
            }
        }

        internal Regex BlobAsUTF8IncludeRegex
        {
            get
            {
                if (this.blobAsUtf8IncludePattern == null)
                {
                    return null;
                }
                if (this.blobAsUtf8IncludeRegex == null)
                {
                    this.blobAsUtf8IncludeRegex = new Regex(this.blobAsUtf8IncludePattern);
                }
                return this.blobAsUtf8IncludeRegex;
            }
        }

        [RefreshProperties(RefreshProperties.All), Description("Character set this connection should use"), DisplayName("Character Set"), Category("Advanced")]
        public string CharacterSet
        {
            get
            {
                return this.charSet;
            }
            set
            {
                this.SetValue("Character Set", value);
                this.charSet = value;
            }
        }

        [DefaultValue(0), Description("The minimum amount of time (in seconds) for this connection to live in the pool before being destroyed."), Category("Pooling"), RefreshProperties(RefreshProperties.All), DisplayName("Connection Lifetime")]
        public uint ConnectionLifeTime
        {
            get
            {
                return this.connectionLifetime;
            }
            set
            {
                this.SetValue("Connection Lifetime", value);
                this.connectionLifetime = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DisplayName("Connection Protocol"), DefaultValue(0), Category("Connection"), Description("Protocol to use for connection to MySQL")]
        public MySqlConnectionProtocol ConnectionProtocol
        {
            get
            {
                return this.protocol;
            }
            set
            {
                this.SetValue("Connection Protocol", value);
                this.protocol = value;
            }
        }

        [DefaultValue(true), DisplayName("Connection Reset"), Description("When true, indicates the connection state is reset when removed from the pool."), Category("Pooling"), RefreshProperties(RefreshProperties.All)]
        public bool ConnectionReset
        {
            get
            {
                return this.connectionReset;
            }
            set
            {
                this.SetValue("Connection Reset", value);
                this.connectionReset = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), Category("Connection"), DisplayName("Connect Timeout"), Description("The length of time (in seconds) to wait for a connection to the server before terminating the attempt and generating an error."), DefaultValue(15)]
        public uint ConnectionTimeout
        {
            get
            {
                return this.connectionTimeout;
            }
            set
            {
                this.SetValue("Connect Timeout", value);
                this.connectionTimeout = value;
            }
        }

        [Category("Advanced"), DefaultValue(false), RefreshProperties(RefreshProperties.All), DisplayName("Convert Zero Datetime"), Description("Should illegal datetime values be converted to DateTime.MinValue")]
        public bool ConvertZeroDateTime
        {
            get
            {
                return this.convertZeroDatetime;
            }
            set
            {
                this.SetValue("Convert Zero Datetime", value);
                this.convertZeroDatetime = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), Description("Database to use initially"), Category("Connection")]
        public string Database
        {
            get
            {
                return this.database;
            }
            set
            {
                this.SetValue("Database", value);
                this.database = value;
            }
        }

        [Category("Connection"), DisplayName("Default Command Timeout"), Description("The default timeout that MySqlCommand objects will use\r\n                     unless changed."), DefaultValue(30), RefreshProperties(RefreshProperties.All)]
        public uint DefaultCommandTimeout
        {
            get
            {
                return this.defaultCommandTimeout;
            }
            set
            {
                this.SetValue("Default Command Timeout", value);
                this.defaultCommandTimeout = value;
            }
        }

        [Category("Connection"), DefaultValue(0), RefreshProperties(RefreshProperties.All), Browsable(false), DisplayName("Driver Type"), Description("Specifies the type of driver to use for this connection")]
        public MySqlDriverType DriverType
        {
            get
            {
                return this.driverType;
            }
            set
            {
                this.SetValue("Driver Type", value);
                this.driverType = value;
            }
        }

        [Description("Instructs the provider to ignore any attempts to prepare a command."), Category("Advanced"), RefreshProperties(RefreshProperties.All), DefaultValue(true), DisplayName("Ignore Prepare")]
        public bool IgnorePrepare
        {
            get
            {
                return this.ignorePrepare;
            }
            set
            {
                this.SetValue("Ignore Prepare", value);
                this.ignorePrepare = value;
            }
        }

        public override object this[string keyword]
        {
            get
            {
                Keyword key = GetKey(keyword);
                return this.GetValue(key);
            }
            set
            {
                if (value == null)
                {
                    this.Remove(keyword);
                }
                else
                {
                    this.SetValue(keyword, value);
                }
            }
        }

        [Description("Enables output of diagnostic messages"), RefreshProperties(RefreshProperties.All), Category("Connection"), DefaultValue(false)]
        public bool Logging
        {
            get
            {
                return this.logging;
            }
            set
            {
                this.SetValue("Logging", value);
                this.logging = value;
            }
        }

        [DefaultValue(100), RefreshProperties(RefreshProperties.All), DisplayName("Maximum Pool Size"), Description("The maximum number of connections allowed in the pool."), Category("Pooling")]
        public uint MaximumPoolSize
        {
            get
            {
                return this.maxPoolSize;
            }
            set
            {
                this.SetValue("Maximum Pool Size", value);
                this.maxPoolSize = value;
            }
        }

        [DefaultValue(0), Category("Pooling"), RefreshProperties(RefreshProperties.All), DisplayName("Minimum Pool Size"), Description("The minimum number of connections allowed in the pool.")]
        public uint MinimumPoolSize
        {
            get
            {
                return this.minPoolSize;
            }
            set
            {
                this.SetValue("Minimum Pool Size", value);
                this.minPoolSize = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), PasswordPropertyText(true), Category("Security"), Description("Indicates the password to be used when connecting to the data source.")]
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.SetValue("Password", value);
                this.password = value;
            }
        }

        [Description("When false, security-sensitive information, such as the password, is not returned as part of the connection if the connection is open or has ever been in an open state."), RefreshProperties(RefreshProperties.All), DisplayName("Persist Security Info"), Category("Security")]
        public bool PersistSecurityInfo
        {
            get
            {
                return this.persistSI;
            }
            set
            {
                this.SetValue("Persist Security Info", value);
                this.persistSI = value;
            }
        }

        [DisplayName("Pipe Name"), Description("Name of pipe to use when connecting with named pipes (Win32 only)"), DefaultValue("MYSQL"), RefreshProperties(RefreshProperties.All), Category("Connection")]
        public string PipeName
        {
            get
            {
                return this.pipeName;
            }
            set
            {
                this.SetValue("Pipe Name", value);
                this.pipeName = value;
            }
        }

        [DefaultValue(true), Description("When true, the connection object is drawn from the appropriate pool, or if necessary, is created and added to the appropriate pool."), Category("Pooling"), RefreshProperties(RefreshProperties.All)]
        public bool Pooling
        {
            get
            {
                return this.pooling;
            }
            set
            {
                this.SetValue("Pooling", value);
                this.pooling = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(0xcea), Category("Connection"), Description("Port to use for TCP/IP connections")]
        public uint Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.SetValue("Port", value);
                this.port = value;
            }
        }

        [DisplayName("Procedure Cache Size"), Category("Advanced"), DefaultValue(0x19), RefreshProperties(RefreshProperties.All), Description("Indicates how many stored procedures can be cached at one time. A value of 0 effectively disables the procedure cache.")]
        public uint ProcedureCacheSize
        {
            get
            {
                return this.procCacheSize;
            }
            set
            {
                this.SetValue("Procedure Cache Size", value);
                this.procCacheSize = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(true), Category("Advanced"), DisplayName("Respect Binary Flags"), Description("Should binary flags on column metadata be respected.")]
        public bool RespectBinaryFlags
        {
            get
            {
                return this.respectBinaryFlags;
            }
            set
            {
                this.SetValue("Respect Binary Flags", value);
                this.respectBinaryFlags = value;
            }
        }

        [Category("Connection"), RefreshProperties(RefreshProperties.All), Description("Server to connect to")]
        public string Server
        {
            get
            {
                return this.server;
            }
            set
            {
                this.SetValue("Server", value);
                this.server = value;
            }
        }

        [Category("Connection"), Description("Name of the shared memory object to use"), RefreshProperties(RefreshProperties.All), DisplayName("Shared Memory Name"), DefaultValue("MYSQL")]
        public string SharedMemoryName
        {
            get
            {
                return this.sharedMemName;
            }
            set
            {
                this.SetValue("Shared Memory Name", value);
                this.sharedMemName = value;
            }
        }

        [Description("Should binary blobs be treated as UTF8"), DisplayName("Treat Blobs As UTF8"), Category("Advanced"), RefreshProperties(RefreshProperties.All)]
        public bool TreatBlobsAsUTF8
        {
            get
            {
                return this.treatBlobsAsUTF8;
            }
            set
            {
                this.SetValue("TreatBlobsAsUTF8", value);
                this.treatBlobsAsUTF8 = value;
            }
        }

        [DefaultValue(true), RefreshProperties(RefreshProperties.All), DisplayName("Treat Tiny As Boolean"), Description("Should the provider treat TINYINT(1) columns as boolean."), Category("Advanced")]
        public bool TreatTinyAsBoolean
        {
            get
            {
                return this.treatTinyAsBoolean;
            }
            set
            {
                this.SetValue("Treat Tiny As Boolean", value);
                this.treatTinyAsBoolean = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), Category("Connection"), DisplayName("Use Compression"), Description("Should the connection ues compression"), DefaultValue(false)]
        public bool UseCompression
        {
            get
            {
                return this.compress;
            }
            set
            {
                this.SetValue("Use Compression", value);
                this.compress = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(false), Description("Allows the use of old style @ syntax for parameters"), Category("Connection"), DisplayName("Use Old Syntax")]
        public bool UseOldSyntax
        {
            get
            {
                return this.oldSyntax;
            }
            set
            {
                this.SetValue("Use Old Syntax", value);
                this.oldSyntax = value;
            }
        }

        [Description("Indicates that performance counters should be updated during execution."), DefaultValue(false), RefreshProperties(RefreshProperties.All), DisplayName("Use Performance Monitor"), Category("Advanced")]
        public bool UsePerformanceMonitor
        {
            get
            {
                return this.usePerfMon;
            }
            set
            {
                this.SetValue("Use Performance Monitor", value);
                this.usePerfMon = value;
            }
        }

        [Description("Indicates if stored procedure bodies will be available for parameter detection."), DefaultValue(true), RefreshProperties(RefreshProperties.All), DisplayName("Use Procedure Bodies"), Category("Advanced")]
        public bool UseProcedureBodies
        {
            get
            {
                return this.useProcedureBodies;
            }
            set
            {
                this.SetValue("Use Procedure Bodies", value);
                this.useProcedureBodies = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), Category("Security"), DisplayName("User Id"), Description("Indicates the user ID to be used when connecting to the data source.")]
        public string UserID
        {
            get
            {
                return this.userId;
            }
            set
            {
                this.SetValue("User Id", value);
                this.userId = value;
            }
        }

        [Description("Should the connection use SSL.  This currently has no effect."), RefreshProperties(RefreshProperties.All), Category("Authentication"), DefaultValue(false)]
        internal bool UseSSL
        {
            get
            {
                return this.useSSL;
            }
            set
            {
                this.SetValue("UseSSL", value);
                this.useSSL = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(false), Category("Advanced"), DisplayName("Use Usage Advisor"), Description("Logs inefficient database operations")]
        public bool UseUsageAdvisor
        {
            get
            {
                return this.useUsageAdvisor;
            }
            set
            {
                this.SetValue("Use Usage Advisor", value);
                this.useUsageAdvisor = value;
            }
        }
    }
}

