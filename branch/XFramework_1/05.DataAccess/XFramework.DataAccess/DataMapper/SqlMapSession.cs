using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using XFramework.Core;

namespace XFramework.DataAccess
{
    /// <summary>
    /// A template for a session in the ETL framwork.
    /// Holds the connection, the transaction 4 mssql ...
    /// </summary>
    public class SqlMapSession : IDalSession
    {
        #region 私有变量

        //Changes the vote to commit (true) or to abort (false) in transsaction
        private bool _consistent = false;
        //Holds value of connection
        private IDbConnection _conn = null;
        //Holds value of transaction
        private IDbTransaction _tran = null;
        //Holds value of dataSource
        protected IDataSource _dataSource = null;
        //indicate transaction was openned
        protected bool _isOpenTran = false;

        #endregion

        #region 构造函数

        public SqlMapSession(IDataSource dataSource)
        {
            this._dataSource = dataSource;
        }

        #endregion

        #region 接口实现
        
        /// <summary>
        /// The data source use by the session.
        /// </summary>
        public IDataSource DataSource
        {
            get { return _dataSource; }
        }

        /// <summary>
        /// The Connection use by the session.
        /// </summary>
        public IDbConnection Connection
        {
            get {
                if (_conn == null) this.CreateConnection();
                return _conn;
            }
        }

        /// <summary>
        /// The Transaction use by the session.
        /// </summary>
        public IDbTransaction Transaction
        {
            get { return _tran; }
        }

        /// <summary>
        /// Indicates if a transaction is open  on
        /// the session.
        /// </summary>
        public bool IsOpenTran
        {
            get { return _isOpenTran; }
        }

        /// <summary>
        /// Create the connection
        /// </summary>
        public void CreateConnection()
        {
            if (_conn == null)
            {
                _conn = _dataSource.DbProvider.CreateConnection();
                _conn.ConnectionString = _dataSource.ConnectionString;
            }
        }

        /// <summary>
        /// Open a connection.
        /// </summary>
        public void OpenConnection()
        {
            if (_conn == null) this.CreateConnection();
            if (_conn.State != ConnectionState.Open) _conn.Open();
        }

        /// <summary>
        /// close a connection
        /// </summary>
        public void CloseConnection()
        {
            if (_conn != null && _conn.State != ConnectionState.Closed)
            {
                _conn.Close();
                _conn.Dispose();
            }
            _conn = null;
        }

        /// <summary>
        /// Open a connection and begin a transaction
        /// </summary>
        public void BeginTransaction()
        {
            this.BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary>
        /// Open a connection and begin a transaction at the data source 
        /// with the specified IsolationLevel value.
        /// </summary>
        /// <param name="isolationLevel">The transaction isolation level for this connection.</param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_isOpenTran) return;

            this.OpenConnection();
            _tran = _conn.BeginTransaction(isolationLevel);
            _isOpenTran = true;
        }

        /// <summary>
        /// Commit a transaction and close the associated connection
        /// </summary>
        public void CommitTransaction()
        {
            this.CommitTransaction(true);
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <param name="closeConnection">Close the connection</param>
        public void CommitTransaction(bool closeConnection)
        {
            if (!_isOpenTran) return;

            if (_tran != null)
            {
                _tran.Commit();
                _tran.Dispose();
                _tran = null;
            }

            _isOpenTran = false;
            if (closeConnection) CloseConnection();
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public void RollBackTransaction()
        {
            this.RollBackTransaction(true);
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <param name="closeConnection">Close the connection</param>
        public void RollBackTransaction(bool closeConnection)
        {
            if (!_isOpenTran) return;

            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
                _tran = null;
            }

            _isOpenTran = false;
            if (closeConnection) CloseConnection();
        }

        /// <summary>
        /// Releasing, or resetting resources, for using
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_consistent)
                {
                    this.CommitTransaction();
                }
                else
                {
                    this.RollBackTransaction();
                }
            }
            finally
            {
                this.CloseConnection();
                _isOpenTran = false;
                _consistent = false;
            }
        }

        /// <summary>
        /// Complete (commit) a transsaction
        /// </summary>
        public void Complete()
        {
            _consistent = true;
        }

        #endregion
    }
}
