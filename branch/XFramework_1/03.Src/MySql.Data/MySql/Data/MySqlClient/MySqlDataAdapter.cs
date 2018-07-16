namespace MySql.Data.MySqlClient
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    [ToolboxBitmap(typeof(MySqlDataAdapter), "MySqlClient.resources.dataadapter.bmp"), DesignerCategory("Code"), Designer("MySql.Data.MySqlClient.Design.MySqlDataAdapterDesigner,MySqlClient.Design")]
    public sealed class MySqlDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
    {
        private bool loadingDefaults;

        public event MySqlRowUpdatedEventHandler RowUpdated;

        public event MySqlRowUpdatingEventHandler RowUpdating;

        public MySqlDataAdapter()
        {
            this.loadingDefaults = true;
        }

        public MySqlDataAdapter(MySqlCommand selectCommand) : this()
        {
            this.SelectCommand = selectCommand;
        }

        public MySqlDataAdapter(string selectCommandText, MySqlConnection connection) : this()
        {
            this.SelectCommand = new MySqlCommand(selectCommandText, connection);
        }

        public MySqlDataAdapter(string selectCommandText, string selectConnString) : this()
        {
            this.SelectCommand = new MySqlCommand(selectCommandText, new MySqlConnection(selectConnString));
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new MySqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new MySqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (this.RowUpdated != null)
            {
                this.RowUpdated(this, value as MySqlRowUpdatedEventArgs);
            }
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (this.RowUpdating != null)
            {
                this.RowUpdating(this, value as MySqlRowUpdatingEventArgs);
            }
        }

        [Description("Used during Update for deleted rows in Dataset.")]
        public MySqlCommand DeleteCommand
        {
            get
            {
                return (MySqlCommand) base.DeleteCommand;
            }
            set
            {
                base.DeleteCommand = value;
            }
        }

        [Description("Used during Update for new rows in Dataset.")]
        public MySqlCommand InsertCommand
        {
            get
            {
                return (MySqlCommand) base.InsertCommand;
            }
            set
            {
                base.InsertCommand = value;
            }
        }

        internal bool LoadDefaults
        {
            get
            {
                return this.loadingDefaults;
            }
            set
            {
                this.loadingDefaults = value;
            }
        }

        [Description("Used during Fill/FillSchema"), Category("Fill")]
        public MySqlCommand SelectCommand
        {
            get
            {
                return (MySqlCommand) base.SelectCommand;
            }
            set
            {
                base.SelectCommand = value;
            }
        }

        [Description("Used during Update for modified rows in Dataset.")]
        public MySqlCommand UpdateCommand
        {
            get
            {
                return (MySqlCommand) base.UpdateCommand;
            }
            set
            {
                base.UpdateCommand = value;
            }
        }
    }
}

