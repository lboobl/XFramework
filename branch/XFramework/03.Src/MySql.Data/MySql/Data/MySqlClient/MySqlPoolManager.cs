namespace MySql.Data.MySqlClient
{
    using System;
    using System.Collections;

    internal class MySqlPoolManager
    {
        private static Hashtable pools = new Hashtable();

        public static MySqlPool GetPool(MySqlConnectionStringBuilder settings)
        {
            string connectionString = settings.GetConnectionString(true);
            lock (pools.SyncRoot)
            {
                MySqlPool pool = pools[connectionString] as MySqlPool;
                if (pool == null)
                {
                    pool = new MySqlPool(settings);
                    pools.Add(connectionString, pool);
                }
                else
                {
                    pool.Settings = settings;
                }
                return pool;
            }
        }

        public static void ReleaseConnection(Driver driver)
        {
            string connectionString = driver.Settings.GetConnectionString(true);
            MySqlPool pool = (MySqlPool) pools[connectionString];
            if (pool == null)
            {
                if (driver.ThreadID != -1)
                {
                    throw new MySqlException("Pooling exception: Unable to find original pool for connection");
                }
            }
            else
            {
                pool.ReleaseConnection(driver);
            }
        }

        public static void RemoveConnection(Driver driver)
        {
            string connectionString = driver.Settings.GetConnectionString(true);
            MySqlPool pool = (MySqlPool) pools[connectionString];
            if (pool == null)
            {
                if (driver.ThreadID != -1)
                {
                    throw new MySqlException("Pooling exception: Unable to find original pool for connection");
                }
            }
            else
            {
                pool.RemoveConnection(driver);
            }
        }
    }
}

