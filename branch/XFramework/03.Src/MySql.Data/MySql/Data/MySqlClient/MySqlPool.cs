namespace MySql.Data.MySqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class MySqlPool
    {
        private Queue<Driver> idlePool;
        private List<Driver> inUsePool;
        private object lockObject;
        private uint maxSize;
        private uint minSize;
        private Semaphore poolGate;
        private MySql.Data.MySqlClient.ProcedureCache procedureCache;
        private MySqlConnectionStringBuilder settings;

        public MySqlPool(MySqlConnectionStringBuilder settings)
        {
            this.minSize = settings.MinimumPoolSize;
            this.maxSize = settings.MaximumPoolSize;
            if (this.minSize > this.maxSize)
            {
                this.minSize = this.maxSize;
            }
            this.settings = settings;
            this.inUsePool = new List<Driver>((int) this.maxSize);
            this.idlePool = new Queue<Driver>((int) this.maxSize);
            for (int i = 0; i < this.minSize; i++)
            {
                this.idlePool.Enqueue(this.CreateNewPooledConnection());
            }
            this.procedureCache = new MySql.Data.MySqlClient.ProcedureCache((int) settings.ProcedureCacheSize);
            this.poolGate = new Semaphore((int) this.maxSize, (int) this.maxSize);
            this.lockObject = new object();
        }

        private Driver CheckoutConnection()
        {
            Driver driver = this.idlePool.Dequeue();
            if (!driver.Ping())
            {
                driver.Close();
                driver = this.CreateNewPooledConnection();
            }
            if (this.settings.ConnectionReset)
            {
                driver.Reset();
            }
            return driver;
        }

        private Driver CreateNewPooledConnection()
        {
            return Driver.Create(this.settings);
        }

        public Driver GetConnection()
        {
            Driver pooledConnection;
            object obj2;
            int millisecondsTimeout = (int) (this.settings.ConnectionTimeout * 0x3e8);
            if (!this.poolGate.WaitOne(millisecondsTimeout, false))
            {
                throw new MySqlException(Resources.TimeoutGettingConnection);
            }
            Monitor.Enter(obj2 = this.lockObject);
            try
            {
                pooledConnection = this.GetPooledConnection();
            }
            catch (Exception exception)
            {
                if (this.settings.Logging)
                {
                    Logger.LogException(exception);
                }
                this.poolGate.Release();
                throw;
            }
            finally
            {
                Monitor.Exit(obj2);
            }
            return pooledConnection;
        }

        private Driver GetPooledConnection()
        {
            Driver item = null;
            if (!this.HasIdleConnections)
            {
                item = this.CreateNewPooledConnection();
            }
            else
            {
                item = this.CheckoutConnection();
            }
            this.inUsePool.Add(item);
            return item;
        }

        public void ReleaseConnection(Driver driver)
        {
            lock (this.lockObject)
            {
                if (this.inUsePool.Contains(driver))
                {
                    this.inUsePool.Remove(driver);
                }
                if (driver.IsTooOld())
                {
                    driver.Close();
                }
                else
                {
                    this.idlePool.Enqueue(driver);
                }
                this.poolGate.Release();
            }
        }

        public void RemoveConnection(Driver driver)
        {
            lock (this.lockObject)
            {
                if (this.inUsePool.Contains(driver))
                {
                    this.inUsePool.Remove(driver);
                    this.poolGate.Release();
                }
            }
        }

        private bool HasIdleConnections
        {
            get
            {
                return (this.idlePool.Count > 0);
            }
        }

        public MySql.Data.MySqlClient.ProcedureCache ProcedureCache
        {
            get
            {
                return this.procedureCache;
            }
        }

        public MySqlConnectionStringBuilder Settings
        {
            get
            {
                return this.settings;
            }
            set
            {
                this.settings = value;
            }
        }
    }
}

