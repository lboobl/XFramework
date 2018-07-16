
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Data.Common;
using System.Collections;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 数据查询提供者 提供一系列方法用以执行数据库操作
    /// </summary>
    public abstract partial class DbQueryProviderBase
    {
        /// <summary>
        /// 异步创建数据库连接
        /// </summary>
        /// <param name="isOpen">是否打开连接</param>
        /// <returns></returns>
        public async Task<IDbConnection> CreateConnectionAsync(bool isOpen = false)
        {
            DbConnection conn = this.DbProvider.CreateConnection();
            conn.ConnectionString = this.ConnectionString;
            if (isOpen) await conn.OpenAsync();

            return conn;
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteNonQueryAsync(cmd);
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(IDbCommand cmd)
        {
            return await this.DoExecuteAsync<int>(cmd, async p => await p.ExecuteNonQueryAsync(), cmd.Transaction == null);
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        public async Task<int> ExecuteNonQueryAsync(List<string> sqlList, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<int>(sqlList, ExecuteNonQueryAsync, trans);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<object> ExecuteScalarAsync(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteScalarAsync(cmd);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<object> ExecuteScalarAsync(IDbCommand cmd)
        {
            return await this.DoExecuteAsync<object>(cmd, async p => await p.ExecuteScalarAsync(), cmd.Transaction == null);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public async Task<object> ExecuteScalarAsync(List<string> sqlList, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<object>(sqlList, ExecuteScalarAsync, trans);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<IDataReader> ExecuteReaderAsync(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteReaderAsync(cmd);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd)
        {
            return await this.ExecuteReaderAsync((DbCommand)cmd);
        }

        // 异步执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        protected async Task<DbDataReader> ExecuteReaderAsync(DbCommand cmd)
        {
            return await this.DoExecuteAsync<DbDataReader>(cmd, async p => await p.ExecuteReaderAsync(CommandBehavior.SequentialAccess), false);
        }

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public async Task<IDataReader> ExecuteReaderAsync(List<string> sqlList, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<DbDataReader>(sqlList, ExecuteReaderAsync, trans);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="query">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>(IDbQueryable<T> query, IDbTransaction transaction = null)
        {
            //string commandText = this.Parse(query).CommandText;
            CommandDefine define = this.Parse(query);
            IDbCommand cmd = this.CreateCommand(define.CommandText, transaction, define.CommandType, define.Parameters);
            return await this.ExecuteAsync<T>(cmd, define);
        }

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteAsync<T>(cmd);
        }

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>(IDbCommand cmd, CommandDefine define = null)
        {
            IDataReader reader = null;
            T TResult = default(T);
            IDbConnection conn = null;

            try
            {
                reader = await this.ExecuteReaderAsync(cmd);
                conn = cmd != null ? cmd.Connection : null;
                TypeDeserializer<T> deserializer = new TypeDeserializer<T>(reader, define as CommandDefine_Select);
                if (await (reader as DbDataReader).ReadAsync()) TResult = deserializer.Deserialize();
                return TResult;
            }
            finally
            {
                Dispose(cmd, reader, conn);
            }
        }

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>(List<string> sqlList, CommandDefine define = null, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<T>(sqlList, async p => await this.ExecuteAsync<T>(p, define), trans);
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回两个实体集合
        /// </summary>
        /// <param name="query1">SQL 命令</param>
        /// <param name="query2">SQL 命令</param>
        /// <param name="transaction">事务</param>
        public async Task<Tuple<List<T1>, List<T2>>> ExecuteMultipleAsync<T1, T2>(IDbQueryable<T1> query1, IDbQueryable<T2> query2, IDbTransaction transaction = null)
        {
            CommandDefine[] defines = new[] { this.Parse(query1), this.Parse(query2) };
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(defines[0].CommandText)) builder.AppendLine(defines[0].CommandText);
            if (!string.IsNullOrEmpty(defines[1].CommandText)) builder.AppendLine(defines[1].CommandText);
            string commandText = builder.ToString();
            IDbCommand cmd = this.CreateCommand(commandText, transaction);

            var result = await this.ExecuteMultipleAsync<T1, T2, Null, Null, Null, Null, Null>(cmd, defines);
            return new Tuple<List<T1>, List<T2>>(result.Item1, result.Item2);
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回两个实体集合
        /// </summary>
        /// <param name="query1">SQL 命令</param>
        /// <param name="query2">SQL 命令</param>
        /// <param name="query3">SQL 命令</param>
        /// <param name="transaction">事务</param>
        public async Task<Tuple<List<T1>, List<T2>, List<T3>>> ExecuteMultipleAsync<T1, T2, T3>(IDbQueryable<T1> query1, IDbQueryable<T2> query2, IDbQueryable<T3> query3, IDbTransaction transaction = null)
        {
            CommandDefine[] defines = new[] { this.Parse(query1), this.Parse(query2), this.Parse(query3) };
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(defines[0].CommandText)) builder.AppendLine(defines[0].CommandText);
            if (!string.IsNullOrEmpty(defines[1].CommandText)) builder.AppendLine(defines[1].CommandText);
            if (!string.IsNullOrEmpty(defines[2].CommandText)) builder.AppendLine(defines[2].CommandText);
            string commandText = builder.ToString();
            IDbCommand cmd = this.CreateCommand(commandText, transaction);

            var result = await this.ExecuteMultipleAsync<T1, T2, T3, Null, Null, Null, Null>(cmd, defines);
            return new Tuple<List<T1>, List<T2>, List<T3>>(result.Item1, result.Item2, result.Item3);
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回多个实体集合
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        public async Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> ExecuteMultipleAsync<T1, T2, T3, T4, T5, T6, T7>(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteMultipleAsync<T1, T2, T3, T4, T5, T6, T7>(cmd);
        }

        /// <summary>
        /// 异步执行 SQL 语句，并返回多个实体集合
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="defines">命令定义对象，用于解析实体的外键</param>
        public async Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> ExecuteMultipleAsync<T1, T2, T3, T4, T5, T6, T7>(IDbCommand cmd, CommandDefine[] defines = null)
        {
            IDataReader reader = null;
            IDbConnection conn = null;
            List<T1> q1 = null;
            List<T2> q2 = null;
            List<T3> q3 = null;
            List<T4> q4 = null;
            List<T5> q5 = null;
            List<T6> q6 = null;
            List<T7> q7 = null;

            TypeDeserializer<T1> deserializer1 = null;
            TypeDeserializer<T2> deserializer2 = null;
            TypeDeserializer<T3> deserializer3 = null;
            TypeDeserializer<T4> deserializer4 = null;
            TypeDeserializer<T5> deserializer5 = null;
            TypeDeserializer<T6> deserializer6 = null;
            TypeDeserializer<T7> deserializer7 = null;

            try
            {
                int i = 0;
                reader = await this.ExecuteReaderAsync(cmd);
                conn = cmd != null ? cmd.Connection : null;

                do
                {
                    i += 1;
                    while (await (reader as DbDataReader).ReadAsync())
                    {
                        switch (i)
                        {
                            #region 元组赋值

                            case 1:
                                if (deserializer1 == null) deserializer1 = new TypeDeserializer<T1>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T1 TValue1 = deserializer1.Deserialize();
                                if (q1 == null) q1 = new List<T1>();
                                q1.Add(TValue1);

                                break;

                            case 2:
                                if (deserializer2 == null) deserializer2 = new TypeDeserializer<T2>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T2 TValue2 = deserializer2.Deserialize();
                                if (q2 == null) q2 = new List<T2>();
                                q2.Add(TValue2);

                                break;

                            case 3:
                                if (deserializer3 == null) deserializer3 = new TypeDeserializer<T3>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T3 TValue3 = deserializer3.Deserialize();
                                if (q3 == null) q3 = new List<T3>();
                                q3.Add(TValue3);

                                break;

                            case 4:
                                if (deserializer4 == null) deserializer4 = new TypeDeserializer<T4>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T4 TValue4 = deserializer4.Deserialize();
                                if (q4 == null) q4 = new List<T4>();
                                q4.Add(TValue4);

                                break;

                            case 5:
                                if (deserializer5 == null) deserializer5 = new TypeDeserializer<T5>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T5 TValue5 = deserializer5.Deserialize();
                                if (q5 == null) q5 = new List<T5>();
                                q5.Add(TValue5);

                                break;

                            case 6:
                                if (deserializer6 == null) deserializer6 = new TypeDeserializer<T6>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T6 TValue6 = deserializer6.Deserialize();
                                if (q6 == null) q6 = new List<T6>();
                                q6.Add(TValue6);

                                break;

                            case 7:
                                if (deserializer7 == null) deserializer7 = new TypeDeserializer<T7>(reader, defines != null ? defines[i - 1] as CommandDefine_Select : null);
                                T7 TValue7 = deserializer7.Deserialize();
                                if (q7 == null) q7 = new List<T7>();
                                q7.Add(TValue7);

                                break;

                            #endregion
                        }
                    }
                }
                while (reader.NextResult());
            }
            finally
            {
                Dispose(cmd, reader, conn);
            }

            return new Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>(q1 ?? new List<T1>(), q2 ?? new List<T2>(), q3 ?? new List<T3>(), q4 ?? new List<T4>(), q5 ?? new List<T5>(), q6 ?? new List<T6>(), q7 ?? new List<T7>());
        }


        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IEnumerable"/> 对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteListAsync<T>(IDbQueryable<T> query, IDbTransaction transaction = null)
        {
            CommandDefine define = this.Parse(query);
            IDbCommand cmd = this.CreateCommand(define.CommandText, transaction, define.CommandType, define.Parameters);
            return await this.ExecuteListAsync<T>(cmd, define);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IEnumerable"/> 对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteListAsync<T>(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteListAsync<T>(cmd);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IEnumerable"/> 对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteListAsync<T>(IDbCommand cmd, CommandDefine define = null)
        {
            IDataReader reader = null;
            IDbConnection conn = null;
            List<T> objList = new List<T>();

            try
            {
                reader = await this.ExecuteReaderAsync(cmd);
                conn = cmd != null ? cmd.Connection : null;
                DbDataReader dbReader = reader as DbDataReader;
                TypeDeserializer<T> deserializer = new TypeDeserializer<T>(reader, define as CommandDefine_Select);

                while (true)
                {
                    bool flag = await dbReader.ReadAsync();
                    if (flag)
                        objList.Add(deserializer.Deserialize());
                    else
                        break;
                }
            }
            finally
            {
                Dispose(cmd, reader, conn);
            }
            return objList;
        }

        /// <summary>
        /// 执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteListAsync<T>(List<string> sqlList, CommandDefine define = null, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<List<T>>(sqlList, async p => await this.ExecuteListAsync<T>(p, define), trans);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<DataTable> ExecuteDataTableAsync(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteDataTableAsync(cmd);
        }

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<DataTable> ExecuteDataTableAsync(IDbCommand cmd)
        {
            IDataReader reader = null;
            IDbConnection conn = null;
            DataTable table = null;

            try
            {
                reader = await this.ExecuteReaderAsync(cmd);
                conn = cmd != null ? cmd.Connection : null;
                table = new DataTable();
                table.Load(reader);
            }
            finally
            {
                Dispose(cmd, reader, conn);
            }
            return table;
        }

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public async Task<DataTable> ExecuteDataTableAsync(List<string> sqlList, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<DataTable>(sqlList, ExecuteDataTableAsync, trans);
        }

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteDataSetAsync(IDbCommand cmd)
        {
            IDataReader reader = null;
            IDbConnection conn = null;
            DataSet ds = null;

            try
            {
                reader = await this.ExecuteReaderAsync(cmd);
                conn = cmd != null ? cmd.Connection : null;
                ds = new XDataSet();
                ds.Load(reader, LoadOption.OverwriteChanges, null, new DataTable[] { });
            }
            finally
            {
                Dispose(cmd, reader, conn);
            }
            return ds;
        }
        
        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteDataSetAsync(string commandText, IDbTransaction transaction = null)
        {
            IDbCommand cmd = this.CreateCommand(commandText, transaction);
            return await this.ExecuteDataSetAsync(cmd);
        }

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteDataSetAsync(List<string> sqlList, IDbTransaction trans = null)
        {
            return await this.DoExecuteAsync<DataSet>(sqlList, ExecuteDataSetAsync, trans);
        }

        // 执行 SQL 命令
        internal async Task<T> DoExecuteAsync<T>(List<string> sqlList, Func<DbCommand, Task<T>> func, IDbTransaction trans)
        {
            if (sqlList == null || sqlList.Count == 0) return default(T);

            int pages = sqlList.Page(_executeSize);
            bool wasClosed = trans == null;
            T result = default(T);

            IDbConnection conn = null;
            IDbCommand cmd = null;

            try
            {
                conn = trans != null ? trans.Connection : await this.CreateConnectionAsync(true);
                if (sqlList.Count > 1 && trans == null) trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                for (int i = 1; i <= pages; i++)
                {
                    var sqls = sqlList
                        .Skip((i - 1) * _executeSize)
                        .Take(_executeSize);

                    string commandText = string.Join(Environment.NewLine, sqls);
                    cmd = this.CreateCommand(commandText, trans);
                    if (cmd.Connection == null) cmd.Connection = conn;

                    result = await this.DoExecuteAsync<T>(cmd, func, false);

                    // 释放当前的cmd
                    if (cmd != null) cmd.Dispose();
                }

                if (wasClosed && trans != null) trans.Commit();
                return result;
            }
            catch
            {
                if (wasClosed && trans != null) trans.Rollback();
                throw;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (wasClosed)
                {
                    Dispose(cmd, null, conn);
                    if (trans != null) trans.Dispose();
                    if (conn != null) conn.Close();
                    if (conn != null) conn.Dispose();
                }
            }
        }

        // 执行 SQL 命令
        protected async Task<T> DoExecuteAsync<T>(IDbCommand cmd, Func<DbCommand, Task<T>> func, bool wasClosed)
        {
            // 如果返回DataReader，则需要在关闭DataReader后再释放链接

            IDbConnection conn = cmd.Connection;
            T TResult = default(T);
            bool status = true;
            Exception exception = null;

            try
            {
                if (conn == null)
                {
                    conn = await this.CreateConnectionAsync();
                    cmd.Connection = conn;
                }
                if (conn.State != ConnectionState.Open) await (conn as DbConnection).OpenAsync();

                TResult = await func(cmd as DbCommand);
            }
            catch (Exception e)
            {
                status = false;
                exception = e;
                throw;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (wasClosed)
                {
                    if (conn != null) conn.Close();
                    if (conn != null) conn.Dispose();
                }

                // 附加拦截器处理
                DbInterception.Execute(cmd, status, exception);
            }

            return TResult;
        }
    }


}
