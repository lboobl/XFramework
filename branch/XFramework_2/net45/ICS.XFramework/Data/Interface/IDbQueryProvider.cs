
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
        /// 名称左括号
        /// </summary>
        string EscCharLeft { get; }

        /// <summary>
        /// 名称右括号
        /// </summary>
        string EscCharRight { get; }

        /// <summary>
        /// 字符串引号
        /// </summary>
        string EscCharQuote { get; }

        /// <summary>
        /// 数据源类的提供程序
        /// </summary>
        DbProviderFactory DbProvider { get; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// <see cref="IDbQueryProvider"/> 实例的名称
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// 命令参数前缀
        /// </summary>
        string ParameterPrefix { get; }

        /// <summary>
        /// 执行命令超时时间
        /// </summary>
        int? CommandTimeout { get; set; }

        /// <summary>
        /// 批次执行的SQL查询语句数量
        /// </summary>
        int ExecuteSize { get; }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="isOpen">是否打开连接</param>
        /// <returns></returns>
        IDbConnection CreateConnection(bool isOpen);

        /// <summary>
        /// 生成 value 对应的 SQL 片断
        /// </summary>
        /// <param name="value">值</param>
        string GetSqlSeg(object value, System.Linq.Expressions.MemberExpression node = null);

        /// <summary>
        /// 创建 SQL 命令
        /// </summary>
        /// <param name="query">查询 语句</param>
        /// <returns></returns>
        CommandDefine Parse<T>(IDbQueryable<T> query);

        /// <summary>
        /// 创建 SQL 命令
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="trans">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        IDbCommand CreateCommand(string commandText, IDbTransaction trans = null, CommandType? commandType = null, IEnumerable<IDataParameter> parameters = null);

        /// <summary>
        /// 创建命令参数
        /// </summary>
        /// <returns></returns>
        IDbDataParameter CreateParameter();

        /// <summary>
        /// 创建命令参数
        /// </summary>
        /// <param name="parameterName">存储过程名称</param>
        /// <param name="value">参数值</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="direction">参数方向</param>
        IDbDataParameter CreateParameter(string parameterName, object value, DbType? dbType = null, int? size = null, ParameterDirection? direction = null);

        /// <summary>
        /// 执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        int ExecuteNonQuery(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        int ExecuteNonQuery(IDbCommand cmd);

        /// <summary>
        /// 执行 SQL 语句，并返回受影响的行数
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        int ExecuteNonQuery(List<string> sqlList, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        object ExecuteScalar(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        object ExecuteScalar(IDbCommand cmd);

        /// <summary>
        /// 执行SQL 语句，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        object ExecuteScalar(List<string> sqlList, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        IDataReader ExecuteReader(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        IDataReader ExecuteReader(IDbCommand cmd);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="IDataReader"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        IDataReader ExecuteReader(List<string> sqlList, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="query">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        T Execute<T>(IDbQueryable<T> query, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        T Execute<T>(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <returns></returns>
        T Execute<T>(IDbCommand cmd, CommandDefine define = null);

        /// <summary>
        /// 执行SQL 语句，并返回单个实体对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        T Execute<T>(List<string> sqlList, CommandDefine define = null, IDbTransaction trans = null);

        /// <summary>
        /// 执行 SQL 语句，并返回两个实体集合
        /// </summary>
        /// <param name="query1">SQL 命令</param>
        /// <param name="query2">SQL 命令</param>
        /// <param name="trans">事务</param>
        Tuple<List<T1>, List<T2>> ExecuteMultiple<T1, T2>(IDbQueryable<T1> query1, IDbQueryable<T2> query2, IDbTransaction trans = null);

        /// <summary>
        /// 执行 SQL 语句，并返回两个实体集合
        /// </summary>
        /// <param name="query1">SQL 命令</param>
        /// <param name="query2">SQL 命令</param>
        /// <param name="query3">SQL 命令</param>
        /// <param name="trans">事务</param>
        Tuple<List<T1>, List<T2>, List<T3>> ExecuteMultiple<T1, T2, T3>(IDbQueryable<T1> query1, IDbQueryable<T2> query2, IDbQueryable<T3> query3, IDbTransaction trans = null);

        /// <summary>
        /// 执行 SQL 语句，并返回多个实体集合
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>> ExecuteMultiple<T1, T2, T3, T4, T5, T6, T7>(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行 SQL 语句，并返回多个实体集合
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="defines">命令定义对象，用于解析实体的外键</param>
        Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>> ExecuteMultiple<T1, T2, T3, T4, T5, T6, T7>(IDbCommand cmd, CommandDefine[] defines = null);

        /// <summary>
        /// 执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        List<T> ExecuteList<T>(IDbQueryable<T> query, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        List<T> ExecuteList<T>(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <returns></returns>
        List<T> ExecuteList<T>(IDbCommand cmd, CommandDefine define = null);

        /// <summary>
        /// 执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="define">命令定义对象，用于解析实体的外键</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        List<T> ExecuteList<T>(List<string> sqlList, CommandDefine define = null, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回并返回单结果集集合
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(IDbCommand cmd);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataTable"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(List<string> sqlList, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(string commandText, IDbTransaction trans = null);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(IDbCommand cmd);

        /// <summary>
        /// 执行SQL 语句，并返回 <see cref="DataSet"/> 对象
        /// </summary>
        /// <param name="sqlList">SQL 命令</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        DataSet ExecuteDataSet(List<string> sqlList, IDbTransaction trans = null);
    }
}
