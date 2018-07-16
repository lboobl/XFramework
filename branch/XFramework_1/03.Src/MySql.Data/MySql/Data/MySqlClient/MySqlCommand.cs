﻿namespace MySql.Data.MySqlClient
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    [DesignerCategory("Code"), ToolboxBitmap(typeof(MySqlCommand), "MySqlClient.resources.command.bmp")]
    public sealed class MySqlCommand : DbCommand, ICloneable
    {
        private IAsyncResult asyncResult;
        private bool canceled;
        private string cmdText;
        private System.Data.CommandType cmdType;
        private int commandTimeout;
        private MySqlConnection connection;
        private int cursorPageSize;
        private MySqlTransaction curTransaction;
        private bool designTimeVisible;
        internal long lastInsertedId;
        private MySqlParameterCollection parameters;
        private bool resetSqlSelect;
        private PreparableStatement statement;
        internal Exception thrownException;
        private bool timedOut;
        private long updatedRowCount;
        private UpdateRowSource updatedRowSource;

        public MySqlCommand()
        {
            this.designTimeVisible = true;
            this.cmdType = System.Data.CommandType.Text;
            this.parameters = new MySqlParameterCollection(this);
            this.updatedRowSource = UpdateRowSource.Both;
            this.cursorPageSize = 0;
            this.cmdText = string.Empty;
            this.timedOut = false;
        }

        public MySqlCommand(string cmdText) : this()
        {
            this.CommandText = cmdText;
        }

        public MySqlCommand(string cmdText, MySqlConnection connection) : this(cmdText)
        {
            this.Connection = connection;
        }

        public MySqlCommand(string cmdText, MySqlConnection connection, MySqlTransaction transaction) : this(cmdText, connection)
        {
            this.curTransaction = transaction;
        }

        internal void AsyncExecuteWrapper(int type, CommandBehavior behavior)
        {
            this.thrownException = null;
            try
            {
                if (type == 1)
                {
                    this.ExecuteReader(behavior);
                }
                else
                {
                    this.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                this.thrownException = exception;
            }
        }

        public IAsyncResult BeginExecuteNonQuery()
        {
            this.asyncResult = new AsyncDelegate(this.AsyncExecuteWrapper).BeginInvoke(2, CommandBehavior.Default, null, null);
            return this.asyncResult;
        }

        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback, object stateObject)
        {
            this.asyncResult = new AsyncDelegate(this.AsyncExecuteWrapper).BeginInvoke(2, CommandBehavior.Default, callback, stateObject);
            return this.asyncResult;
        }

        public IAsyncResult BeginExecuteReader()
        {
            return this.BeginExecuteReader(CommandBehavior.Default);
        }

        public IAsyncResult BeginExecuteReader(CommandBehavior behavior)
        {
            this.asyncResult = new AsyncDelegate(this.AsyncExecuteWrapper).BeginInvoke(1, behavior, null, null);
            return this.asyncResult;
        }

        public override void Cancel()
        {
            if (!this.connection.driver.Version.isAtLeast(5, 0, 0))
            {
                throw new NotSupportedException(Resources.CancelNotSupported);
            }
            using (MySqlConnection connection = new MySqlConnection(this.connection.Settings.GetConnectionString(true)))
            {
                connection.Settings.Pooling = false;
                connection.Open();
                new MySqlCommand(string.Format("KILL QUERY {0}", this.connection.ServerThread), connection).ExecuteNonQuery();
                this.canceled = true;
            }
        }

        private void CheckState()
        {
            if (this.connection == null)
            {
                throw new InvalidOperationException("Connection must be valid and open.");
            }
            if ((this.connection.State != ConnectionState.Open) && !this.connection.SoftClosed)
            {
                throw new InvalidOperationException("Connection must be valid and open.");
            }
            if ((this.connection.Reader != null) && (this.cursorPageSize == 0))
            {
                throw new MySqlException("There is already an open DataReader associated with this Connection which must be closed first.");
            }
            if ((this.CommandType == System.Data.CommandType.StoredProcedure) && !this.connection.driver.Version.isAtLeast(5, 0, 0))
            {
                throw new MySqlException("Stored procedures are not supported on this version of MySQL");
            }
        }

        internal void Close()
        {
            if (this.statement != null)
            {
                this.statement.Close();
            }
            if (this.resetSqlSelect)
            {
                new MySqlCommand("SET SQL_SELECT_LIMIT=-1", this.connection).ExecuteNonQuery();
            }
            this.resetSqlSelect = false;
        }

        protected override DbParameter CreateDbParameter()
        {
            return new MySqlParameter();
        }

        public MySqlParameter CreateParameter()
        {
            return (MySqlParameter) this.CreateDbParameter();
        }

        protected override void Dispose(bool disposing)
        {
            if ((disposing && (this.statement != null)) && this.statement.IsPrepared)
            {
                this.statement.CloseStatement();
            }
            base.Dispose(disposing);
        }

        public int EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            asyncResult.AsyncWaitHandle.WaitOne();
            if (this.thrownException != null)
            {
                throw this.thrownException;
            }
            return (int) this.updatedRowCount;
        }

        public MySqlDataReader EndExecuteReader(IAsyncResult result)
        {
            result.AsyncWaitHandle.WaitOne();
            if (this.thrownException != null)
            {
                throw this.thrownException;
            }
            return this.connection.Reader;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            this.lastInsertedId = -1;
            this.updatedRowCount = -1;
            MySqlDataReader reader = this.ExecuteReader();
            if (reader != null)
            {
                reader.Close();
                this.lastInsertedId = reader.InsertedId;
                this.updatedRowCount = reader.RecordsAffected;
            }
            return (int) this.updatedRowCount;
        }

        public MySqlDataReader ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        public MySqlDataReader ExecuteReader(CommandBehavior behavior)
        {
            MySqlDataReader reader2;
            this.lastInsertedId = -1;
            this.CheckState();
            if ((this.cmdText == null) || (this.cmdText.Trim().Length == 0))
            {
                throw new InvalidOperationException(Resources.CommandTextNotInitialized);
            }
            string text = TrimSemicolons(this.cmdText);
            this.connection.IsExecutingBuggyQuery = false;
            if (!this.connection.driver.Version.isAtLeast(5, 0, 0) && this.connection.driver.Version.isAtLeast(4, 1, 0))
            {
                string str2 = text;
                if (str2.Length > 0x11)
                {
                    str2 = text.Substring(0, 0x11);
                }
                str2 = str2.ToLower(CultureInfo.InvariantCulture);
                this.connection.IsExecutingBuggyQuery = str2.StartsWith("describe") || str2.StartsWith("show table status");
            }
            if ((this.statement == null) || !this.statement.IsPrepared)
            {
                if (this.CommandType == System.Data.CommandType.StoredProcedure)
                {
                    this.statement = new StoredProcedure(this, text);
                }
                else
                {
                    this.statement = new PreparableStatement(this, text);
                }
            }
            this.statement.Resolve();
            this.HandleCommandBehaviors(behavior);
            this.updatedRowCount = -1;
            Timer timer = null;
            try
            {
                MySqlDataReader reader = new MySqlDataReader(this, this.statement, behavior);
                this.timedOut = false;
                this.canceled = false;
                this.statement.Execute();
                if (this.connection.driver.Version.isAtLeast(5, 0, 0) && (this.commandTimeout > 0))
                {
                    TimerCallback callback = new TimerCallback(this.TimeoutExpired);
                    timer = new Timer(callback, this, this.CommandTimeout * 0x3e8, -1);
                }
                reader.NextResult();
                this.connection.Reader = reader;
                reader2 = reader;
            }
            catch (MySqlException exception)
            {
                if (exception.Number == 0x525)
                {
                    if (this.TimedOut)
                    {
                        throw new MySqlException(Resources.Timeout);
                    }
                    return null;
                }
                if (exception.IsFatal)
                {
                    this.Connection.Close();
                }
                throw;
            }
            finally
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            return reader2;
        }

        public override object ExecuteScalar()
        {
            this.lastInsertedId = -1;
            object obj2 = null;
            MySqlDataReader reader = this.ExecuteReader();
            if (reader == null)
            {
                return null;
            }
            try
            {
                if (reader.Read())
                {
                    obj2 = reader.GetValue(0);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    this.lastInsertedId = reader.InsertedId;
                }
                reader = null;
            }
            return obj2;
        }

        private void HandleCommandBehaviors(CommandBehavior behavior)
        {
            if ((behavior & CommandBehavior.SchemaOnly) != CommandBehavior.Default)
            {
                new MySqlCommand("SET SQL_SELECT_LIMIT=0", this.connection).ExecuteNonQuery();
                this.resetSqlSelect = true;
            }
            else if ((behavior & CommandBehavior.SingleRow) != CommandBehavior.Default)
            {
                new MySqlCommand("SET SQL_SELECT_LIMIT=1", this.connection).ExecuteNonQuery();
                this.resetSqlSelect = true;
            }
        }

        public override void Prepare()
        {
            if (this.connection == null)
            {
                throw new InvalidOperationException("The connection property has not been set.");
            }
            if (this.connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open.");
            }
            if (this.connection.driver.Version.isAtLeast(4, 1, 0) && !this.connection.Settings.IgnorePrepare)
            {
                this.Prepare(0);
            }
        }

        private void Prepare(int cursorPageSize)
        {
            if (!this.connection.driver.Version.isAtLeast(5, 0, 0) && (cursorPageSize > 0))
            {
                throw new InvalidOperationException("Nested commands are only supported on MySQL 5.0 and later");
            }
            string commandText = this.CommandText;
            if ((commandText != null) && (commandText.Trim().Length != 0))
            {
                if (this.CommandType == System.Data.CommandType.StoredProcedure)
                {
                    this.statement = new StoredProcedure(this, this.CommandText);
                }
                else
                {
                    this.statement = new PreparableStatement(this, this.CommandText);
                }
                this.statement.Prepare();
            }
        }

        object ICloneable.Clone()
        {
            MySqlCommand command = new MySqlCommand(this.cmdText, this.connection, this.curTransaction) {
                CommandType = this.CommandType,
                CommandTimeout = this.CommandTimeout
            };
            foreach (MySqlParameter parameter in this.parameters)
            {
                command.Parameters.Add(((ICloneable) parameter).Clone());
            }
            return command;
        }

        private void TimeoutExpired(object commandObject)
        {
            MySqlCommand command = commandObject as MySqlCommand;
            command.timedOut = true;
            try
            {
                command.Cancel();
            }
            catch (Exception exception)
            {
                if (this.connection.Settings.Logging)
                {
                    Logger.LogException(exception);
                }
            }
        }

        private static string TrimSemicolons(string sql)
        {
            StringBuilder builder = new StringBuilder(sql);
            int startIndex = 0;
            while (builder[startIndex] == ';')
            {
                startIndex++;
            }
            int num2 = builder.Length - 1;
            while (builder[num2] == ';')
            {
                num2--;
            }
            return builder.ToString(startIndex, (num2 - startIndex) + 1);
        }

        [Description("Command text to execute"), Editor("MySql.Data.Common.Design.SqlCommandTextEditor,MySqlClient.Design", typeof(UITypeEditor)), Category("Data")]
        public override string CommandText
        {
            get
            {
                return this.cmdText;
            }
            set
            {
                this.cmdText = value;
                this.statement = null;
                if (this.cmdText.EndsWith("DEFAULT VALUES"))
                {
                    this.cmdText = this.cmdText.Substring(0, this.cmdText.Length - 14);
                    this.cmdText = this.cmdText + "() VALUES ()";
                }
            }
        }

        [Description("Time to wait for command to execute"), DefaultValue(30), Category("Misc")]
        public override int CommandTimeout
        {
            get
            {
                if (this.commandTimeout != 0)
                {
                    return this.commandTimeout;
                }
                return 30;
            }
            set
            {
                this.commandTimeout = value;
            }
        }

        [Category("Data")]
        public override System.Data.CommandType CommandType
        {
            get
            {
                return this.cmdType;
            }
            set
            {
                this.cmdType = value;
            }
        }

        [Category("Behavior"), Description("Connection used by the command")]
        public MySqlConnection Connection
        {
            get
            {
                return this.connection;
            }
            set
            {
                if (this.connection != value)
                {
                    this.Transaction = null;
                }
                this.connection = value;
                if ((this.connection != null) && (this.commandTimeout == 0))
                {
                    this.commandTimeout = (int) this.connection.Settings.DefaultCommandTimeout;
                }
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
            set
            {
                this.Connection = (MySqlConnection) value;
            }
        }

        protected override System.Data.Common.DbParameterCollection DbParameterCollection
        {
            get
            {
                return this.Parameters;
            }
        }

        protected override System.Data.Common.DbTransaction DbTransaction
        {
            get
            {
                return this.Transaction;
            }
            set
            {
                this.Transaction = (MySqlTransaction) value;
            }
        }

        [Browsable(false)]
        public override bool DesignTimeVisible
        {
            get
            {
                return this.designTimeVisible;
            }
            set
            {
                this.designTimeVisible = value;
            }
        }

        [Browsable(false)]
        public bool IsPrepared
        {
            get
            {
                return ((this.statement != null) && this.statement.IsPrepared);
            }
        }

        [Browsable(false)]
        public long LastInsertedId
        {
            get
            {
                return this.lastInsertedId;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("The parameters collection"), Category("Data")]
        public MySqlParameterCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal bool TimedOut
        {
            get
            {
                return this.timedOut;
            }
        }

        [Browsable(false)]
        public MySqlTransaction Transaction
        {
            get
            {
                return this.curTransaction;
            }
            set
            {
                this.curTransaction = value;
            }
        }

        internal int UpdateCount
        {
            get
            {
                return (int) this.updatedRowCount;
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this.updatedRowSource;
            }
            set
            {
                this.updatedRowSource = value;
            }
        }

        internal delegate void AsyncDelegate(int type, CommandBehavior behavior);
    }
}

