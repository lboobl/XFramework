
using System;
using System.Data;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 数据上下文，表示 Xfw 框架的主入口点
    /// </summary>
    public partial class DataContext : IDisposable
    {
        #region 私有字段

        private readonly List<object> _dbQueryables = new List<object>();
        private readonly object _oLock = new object();
        private IDbQueryProvider _provider;

        #endregion

        #region 公开属性

        /// <summary>
        /// <see cref="IDbQueryable"/> 的解析执行提供程序
        /// </summary>
        public IDbQueryProvider Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        /// <summary>
        /// 执行命令超时时间
        /// </summary>
        public int CommandTimeout
        {
            get { return _provider.CommandTimeout ?? 0; }
            set { _provider.CommandTimeout = value; }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化 <see cref="DataContext"/> 类的新实例
        /// </summary>
        public DataContext()
            : this(XfwContainer.Default.Resolve<IDbQueryProvider>())
        {
        }

        /// <summary>
        /// 使用提供程序初始化 <see cref="DataContext"/> 类的新实例
        /// </summary>
        public DataContext(IDbQueryProvider provider)
        {
            _provider = provider;
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 新增记录
        /// </summary>
        public virtual void Insert<T>(T TEntity)
        {
            IDbQueryable<T> table = this.GetTable<T>();
            table.DbExpressions.Add(new DbExpression
            {
                DbExpressionType = DbExpressionType.Insert,
                Expressions = new[] { Expression.Constant(TEntity) }
            });

            lock (this._oLock)
                _dbQueryables.Add(table);
        }

        /// <summary>
        /// 批量新增记录
        /// </summary>
        public virtual void Insert<T>(IEnumerable<T> collection)
        {
            List<IDbQueryable> bulkList = new List<IDbQueryable>();
            foreach (T value in collection)
            {
                IDbQueryable<T> table = this.GetTable<T>();
                table.DbExpressions.Add(new DbExpression
                {
                    DbExpressionType = DbExpressionType.Insert,
                    Expressions = new[] { Expression.Constant(value) }
                });

                bulkList.Add(table);
            }

            lock (this._oLock)
                _dbQueryables.Add(bulkList);
        }

        /// <summary>
        /// 批量新增记录
        /// </summary>
        public virtual void Insert<T>(IDbQueryable<T> query)
        {
            //IDbQueryable<T> source = query;
            //source.DbExpressions.Add(new DbExpression(DbExpressionType.Insert));
            query = query.CreateQuery<T>(new DbExpression(DbExpressionType.Insert));
            lock (this._oLock)
                _dbQueryables.Add(query);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        public void Delete<T>(T TEntity)
        {
            IDbQueryable<T> table = this.GetTable<T>();
            table.DbExpressions.Add(new DbExpression
            {
                DbExpressionType = DbExpressionType.Delete,
                Expressions = new[] { Expression.Constant(TEntity) }
            });
            lock (this._oLock)
                _dbQueryables.Add(table);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        public void Delete<T>(Expression<Func<T, bool>> predicate)
        {
            this.Delete<T>(p => p.Where(predicate));
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        public void Delete<T>(IDbQueryable<T> query)
        {
            query = query.CreateQuery<T>(new DbExpression(DbExpressionType.Delete));
            lock (this._oLock)
                _dbQueryables.Add(query);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        public virtual void Update<T>(T TEntity)
        {
            IDbQueryable<T> table = this.GetTable<T>();
            table.DbExpressions.Add(new DbExpression
            {
                DbExpressionType = DbExpressionType.Update,
                Expressions = new[] { Expression.Constant(TEntity) }
            });
            lock (this._oLock)
                _dbQueryables.Add(table);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        public virtual void Update<T>(Expression<Func<T, T>> action, Expression<Func<T, bool>> predicate)
        {
            this.Update<T>(action, p => p.Where(predicate));
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        public virtual void Update<T>(Expression<Func<T, T>> action, IDbQueryable<T> query)
        {
            query = query.CreateQuery<T>(new DbExpression
            {
                DbExpressionType = DbExpressionType.Update,
                Expressions = new[] { action }
            });
            lock (this._oLock)
                _dbQueryables.Add(query);
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        public virtual void Update<T, TFrom>(Expression<Func<T, TFrom, T>> action, IDbQueryable<T> query)
        {
            query = query.CreateQuery<T>(new DbExpression
            {
                DbExpressionType = DbExpressionType.Update,
                Expressions = new[] { action }
            });
            lock (this._oLock)
                _dbQueryables.Add(query);
        }

        /// <summary>
        /// 附加查询项
        /// </summary>
        public void AddQuery(string query, params object[] args)
        {
            if (args != null && !string.IsNullOrEmpty(query))
            {
                for (int i = 0; i < args.Length; i++) args[i] = _provider.GetSqlSeg(args[i]);
                query = string.Format(query, args);
            }
            lock (this._oLock)
                if (!string.IsNullOrEmpty(query)) _dbQueryables.Add(query);
        }

        /// <summary>
        /// 附加查询项
        /// </summary>
        public void AddQuery(IDbQueryable query)
        {
            lock (this._oLock)
                _dbQueryables.Add(query);
        }

        /// <summary>
        /// 计算要插入、更新或删除的已修改对象的集，并执行相应命令以实现对数据库的更改
        /// </summary>
        /// <returns></returns>
        public virtual int SubmitChanges()
        {
            int count = _dbQueryables.Count;
            if (count == 0) return 0;

            List<string> sqlList = this.Resolve(true);
            List<int> identitys = new List<int>();
            IDataReader reader = null;

            try
            {
                DbQueryProviderBase provider = _provider as DbQueryProviderBase;
                Func<IDbCommand, object> func = p =>
                {
                    reader = provider.ExecuteReader(p);
                    TypeDeserializer<int> deserializer = new TypeDeserializer<int>(reader, null);
                    do
                    {
                        if (reader.Read()) identitys.Add(deserializer.Deserialize());
                    }
                    while (reader.NextResult());

                    // 释放当前的reader
                    if (reader != null) reader.Dispose();

                    return null;
                };

                provider.DoExecute<object>(sqlList, func, null);
                // 回写自增列的ID值
                this.SetAutoIncrementValue(identitys);
            }
            finally
            {
                if (reader != null) reader.Dispose();
            }

            return count;
        }

        /// <summary>
        /// 计算要插入、更新或删除的已修改对象的集，并执行相应命令以实现对数据库的更改
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="result">提交更改并查询数据</param>
        /// <returns></returns>
        public virtual int SubmitChanges<T>(out List<T> result, IDbTransaction trans = null)
        {
            result = new List<T>();
            int count = _dbQueryables.Count;
            if (count == 0) return 0;

            List<CommandDefine> sqlList = this.InnerResolve(true);
            CommandDefine define = sqlList.FirstOrDefault(x => (x as CommandDefine_Select) != null);
            result = _provider.ExecuteList<T>(sqlList.ToList(x => x.CommandText), define, trans);
            return count;
        }

        /// <summary>
        /// 计算要插入、更新或删除的已修改对象的集，并执行相应命令以实现对数据库的更改
        /// </summary>
        /// <param name="result">提交更改并查询数据</param>
        /// <returns></returns>
        public virtual int SubmitChanges(out DataTable result, IDbTransaction trans = null)
        {
            result = new DataTable();
            int count = _dbQueryables.Count;
            if (count == 0) return 0;

            List<CommandDefine> sqlList = this.InnerResolve(true);
            result = _provider.ExecuteDataTable(sqlList.ToList(x => x.CommandText), trans);
            return count;
        }

        /// <summary>
        /// 计算要插入、更新或删除的已修改对象的集，并执行相应命令以实现对数据库的更改
        /// </summary>
        /// <param name="singleRowEffect">每一个 <see cref="IDbQueryable"/> 只影响一行</param>
        /// <returns></returns>
        public virtual int SubmitChanges(out bool singleRowEffect)
        {
            singleRowEffect = true;

            int count = _dbQueryables.Count;
            if (count == 0) return 0;

            IDbConnection conn = null;
            IDbTransaction trans = null;

            try
            {
                conn = _provider.CreateConnection(true);
                trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                for (int i = 0; i < count; i++)
                {
                    string cmd = _dbQueryables[i].ToString();
                    int line = _provider.ExecuteNonQuery(cmd, trans);
                    if (line != 1)
                    {
                        singleRowEffect = false;
                        if (trans != null) trans.Rollback();
                        return 0;
                    }
                }

                if (trans != null) trans.Commit();
                return count;
            }
            catch
            {
                if (trans != null) trans.Rollback();
                throw;
            }
            finally
            {
                if (trans != null) trans.Dispose();
                if (conn != null) conn.Close();
                if (conn != null) conn.Dispose();
                this.Dispose();
            }
        }

        ///// <summary>
        ///// 每一个 <see cref="IDbQueryable"/> 只影响一行，否则返回false
        ///// </summary>
        //public bool ExecuteLineQuery()
        //{
        //    int count = _dbQueryables.Count;
        //    if (count == 0) return false;

        //    IDbConnection conn = null;
        //    IDbTransaction trans = null;

        //    try
        //    {
        //        conn = _provider.CreateConnection(true);
        //        if (count > 1) trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

        //        for (int i = 0; i < count; i++)
        //        {
        //            string cmd = _dbQueryables[i].ToString();
        //            int line = _provider.ExecuteNonQuery(cmd, trans);
        //            if (line != 1)
        //            {
        //                if (trans != null) trans.Rollback();
        //                return false;
        //            }
        //        }

        //        if (trans != null) trans.Commit();
        //        return true;
        //    }
        //    catch
        //    {
        //        if (trans != null) trans.Rollback();
        //        throw;
        //    }
        //    finally
        //    {
        //        this.Dispose(null, null, trans, conn);
        //    }
        //}

        /// <summary>
        /// 返回特定类型的对象的集合，其中类型由 T 参数定义
        /// </summary>
        public IDbQueryable<T> GetTable<T>()
        {
            DbQueryable<T> queryable = new DbQueryable<T> { Provider = this.Provider };
            queryable.DbExpressions = new List<DbExpression> { new DbExpression
            {
                DbExpressionType = DbExpressionType.GetTable,
                Expressions = new[] { Expression.Constant(typeof(T)) }
            } };
            return queryable;
        }

        /// <summary>
        /// 释放由 <see cref="DataContext"/> 类的当前实例占用的所有资源
        /// </summary>
        public void Dispose()
        {
            lock (this._oLock)
                this._dbQueryables.Clear();
        }

        //// 尝试释放资源
        //private void Dispose(IDbCommand cmd = null, IDataReader reader = null, IDbTransaction trans = null, IDbConnection conn = null)
        //{
        //    if (cmd != null) cmd.Dispose();
        //    if (reader != null) reader.Dispose();
        //    if (trans != null) trans.Dispose();
        //    if (conn != null) conn.Close();
        //    if (conn != null) conn.Dispose();

        //    // 清空查询语义
        //    this.Dispose();
        //}

        /// <summary>
        /// 将 IDbQueryable&lt;T&gt;对象解析成 SQL 脚本
        /// </summary>
        /// <returns></returns>
        public virtual List<string> Resolve(bool clear = true)
        {
            return this
                .InnerResolve(clear)
                .ToList(x => x.CommandText);
        }

        #endregion

        #region 私有函数

        //更新记录
        private void Update<T>(Expression<Func<T, T>> action, Func<IDbQueryable<T>, IDbQueryable<T>> predicate)
        {
            IDbQueryable<T> table = this.GetTable<T>();
            IDbQueryable<T> query = predicate(table);
            this.Update<T>(action, query);
        }

        // 删除记录
        private void Delete<T>(Func<IDbQueryable<T>, IDbQueryable<T>> predicate)
        {
            IDbQueryable<T> table = this.GetTable<T>();
            this.Delete<T>(predicate(table));
        }

        // 更新自增列
        private void SetAutoIncrementValue(List<int> identitys)
        {
            if (identitys == null || identitys.Count == 0) return;

            int index = -1;
            foreach (var obj in _dbQueryables)
            {
                IDbQueryable query = obj as IDbQueryable;
                if (query == null) continue;

                var info = query.DbQueryInfo as IDbQueryableInfo_Insert;
                if (info != null && info.Entity != null && info.AutoIncrement != null)
                {
                    index += 1;
                    int identity = identitys[index];
                    info.AutoIncrement.Set(info.Entity, identity);
                }
            }
        }

        // 将查询语义解析成 SQL 语句
        protected List<CommandDefine> InnerResolve(bool clear = true)
        {
            List<CommandDefine> sqlList = new List<CommandDefine>();
            int count = _dbQueryables.Count;

            try
            {
                if (count != 0)
                {

                    for (int i = 0; i < count; i++)
                    {
                        object obj = _dbQueryables[i];
                        if (obj is IDbQueryable)
                        {
                            string cmd = _dbQueryables[i].ToString();
                            sqlList.Add((_dbQueryables[i] as IDbQueryable).Command);
                        }
                        else if (obj is string)
                        {
                            string cmd = _dbQueryables[i].ToString();
                            CommandDefine d = new CommandDefine(cmd);
                            sqlList.Add(d);
                        }
                        else
                        {
                            // 解析批量插入操作
                            List<IDbQueryable> bulkList = obj as List<IDbQueryable>;
                            this.ResolveBulk(sqlList, bulkList);
                        }
                    }
                }
            }
            finally
            {
                if (clear) this.Dispose();
            }

            return sqlList;
        }

        // 解析批量 INSERT 语句
        private void ResolveBulk(List<CommandDefine> sqlList, List<IDbQueryable> bulkList)
        {
            // SQL 只能接收1000个
            int pageSize = 1000;
            int pages = bulkList.Page(pageSize);
            for (int pageIndex = 1; pageIndex <= pages; pageIndex++)
            {
                var dbQueryables = bulkList.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                int i = 0;
                int t = dbQueryables.Count();
                var builder = new System.Text.StringBuilder(128);

                foreach (IDbQueryable q in dbQueryables)
                {
                    i += 1;
                    q.Bulk = new BulkInfo { OnlyValue = i != 1, IsOver = i == t };

                    string cmd = q.ToString();
                    builder.Append(cmd);
                }

                if (builder.Length > 0) sqlList.Add(new CommandDefine(builder.ToString()));
            }
        }

        #endregion
    }
}
