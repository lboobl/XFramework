
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 定义用于解析和执行 <see cref="ICS.XFramework.Data.IDbQueryable"/> 对象所描述的查询方法
    /// </summary>
    public partial interface IDbQueryProvider
    {
        /// <summary>
        /// 异步创建数据库连接
        /// </summary>
        /// <param name="isOpen">是否打开连接</param>
        /// <returns></returns>
        Task<IDbConnection> CreateConnectionAsync(bool isOpen);

        /// <summary>
        /// 异步执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<int> ExecuteNonQueryAsync(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<int> ExecuteNonQueryAsync(IDbCommand cmd);

        /// <summary>
        /// 异步执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="transaction">事务</param>
        Task<int> ExecuteNonQueryAsync(List<string> sqlList, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<object> ExecuteScalarAsync(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<object> ExecuteScalarAsync(IDbCommand cmd);

        /// <summary>
        /// 执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<object> ExecuteScalarAsync(List<string> sqlList, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<IDataReader> ExecuteReaderAsync(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd);

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<IDataReader> ExecuteReaderAsync(List<string> sqlList, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="query">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(IDbQueryable<T> query, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(IDbCommand cmd, CommandBase define = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(List<string> sqlList, CommandBase define = null, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<List<T>> ExecuteListAsync<T>(IDbQueryable<T> query, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回单结果集集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<List<T>> ExecuteListAsync<T>(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <returns></returns>
        Task<List<T>> ExecuteListAsync<T>(IDbCommand cmd, CommandBase define = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<List<T>> ExecuteListAsync<T>(List<string> sqlList, CommandBase define = null, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行 SQL 语句，并返回两个实体集合
        /// </summary>
        /// <param name="query1">SQL 命令</param>
        /// <param name="query2">SQL 命令</param>
        /// <param name="trans">事务</param>
        Task<Tuple<List<T1>, List<T2>>> ExecuteMultipleAsync<T1, T2>(IDbQueryable<T1> query1, IDbQueryable<T2> query2, IDbTransaction trans = null);

        /// <summary>
        /// 异步执行 SQL 语句，并返回两个实体集合
        /// </summary>
        /// <param name="query1">SQL 命令</param>
        /// <param name="query2">SQL 命令</param>
        /// <param name="query3">SQL 命令</param>
        /// <param name="trans">事务</param>
        Task<Tuple<List<T1>, List<T2>, List<T3>>> ExecuteMultipleAsync<T1, T2, T3>(IDbQueryable<T1> query1, IDbQueryable<T2> query2, IDbQueryable<T3> query3, IDbTransaction trans = null);

        /// <summary>
        /// 异步执行 SQL 语句，并返回多个实体集合
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> ExecuteMultipleAsync<T1, T2, T3, T4, T5, T6, T7>(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 异步执行 SQL 语句，并返回多个实体集合
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="defines">命令定义对象，用于解析实体的外键</param>
        Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> ExecuteMultipleAsync<T1, T2, T3, T4, T5, T6, T7>(IDbCommand cmd, CommandBase[] defines = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<DataTable> ExecuteDataTableAsync(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<DataTable> ExecuteDataTableAsync(IDbCommand cmd);

        /// <summary>
        /// 异步执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<DataTable> ExecuteDataTableAsync(List<string> sqlList, IDbTransaction transaction = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<DataSet> ExecuteDataSetAsync(string commandText, IDbTransaction transaction = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<DataSet> ExecuteDataSetAsync(IDbCommand cmd);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        Task<DataSet> ExecuteDataSetAsync(List<string> sqlList, IDbTransaction transaction = null);
    }
}
