using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Linq.Expressions;
using System.Collections.Generic;

using XFramework.Core;
using XFramework.DataAccess.Dapper;

namespace XFramework.DataAccess
{
    /// <summary>
    /// 仓储模式基类，提供单个实体的操作
    /// </summary>
    public class RepositoryBase : IDisposable
    {
        #region 私有变量

        protected IDalSession _session = null;
        protected CommandBuilder _builder = null;

        #endregion

        #region 公开属性

        /// <summary>
        /// 当前服务器时间
        /// </summary>
        public DateTime SrvDateTime
        {
            get { return this.Query<DateTime>("SELECT GETDATE()", null, CommandType.Text).First<DateTime>(); }
        }

        /// <summary>
        /// Sql会话
        /// </summary>
        public IDalSession Session
        {
            get { return _session; }
        }

        #endregion

        #region 构造函数

        public RepositoryBase()
            : this(ConfigHelper.DataSource)
        {
        }

        public RepositoryBase(IDataSource dataSource)
        {
            this._session = new SqlMapSession(dataSource);
            this._builder = new CommandBuilder(_session.DataSource.DbProvider.ParameterPrefix);
        }

        #endregion

        #region 重写方法

        #endregion

        #region 公开方法

        /// <summary>
        /// 插入记录
        /// </summary>
        /// <param name="TEntity">实体</param>
        /// <returns></returns>
        public void Insert<T>(T TEntity) where T : class
        {
            Command cmd = _builder.Build<T>(CommandBuilder.Insert, TEntity);
            DynamicParameters dynParameters = this.SetupParameter(cmd.Parameters);
            this.Execute(cmd.Text, dynParameters, cmd.CommandType);

            //处理自增列
            if (cmd.Mapper.Identity != null)
            {
                string fieldName = cmd.Mapper.Identity.Name;
                AccFacHelper.Set(TEntity, fieldName, dynParameters.Get<int>(fieldName));
            }
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="TEntity">实体</param>
        /// <returns></returns>
        public int Update<T>(T TEntity) where T : class
        {
            return this.Execute<T>(CommandBuilder.UpdateByKey, TEntity);
        }

        /// <summary>
        /// 更新记录 x=>new T{}
        /// </summary>
        /// <param name="updater">更新表达式</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, T>> updater, Expression<Func<T, bool>> predicate) where T : class
        {
            Command cmd = _builder.Build<T>(updater, predicate);
            DynamicParameters dynParameters = this.SetupParameter(cmd.Parameters);
            return this.Execute(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="TEntity">实体</param>
        public int Delete<T>(T TEntity) where T : class
        {
            return this.Execute<T>(CommandBuilder.DeleteByKey, TEntity);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="predicate">筛选谓词</param>
        public int Delete<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return this.Execute<T>(CommandBuilder.Delete, predicate);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> predicate = null) where T : class
        {
            return this.Query<T>(CommandBuilder.Select, predicate);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="page">分页信息</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(PageInfo page, Expression<Func<T, bool>> predicate = null) where T : class
        {
            Command cmd = _builder.Build<T>(page, predicate);
            DynamicParameters dynParameters = this.SetupParameter(cmd.Parameters);
            return this.Query<T>(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="predicate">筛选谓词</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string cmdName, Expression<Func<T, bool>> predicate = null, DynamicParameters dynParameters = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, predicate);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.Query<T>(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="condition">查询条件 FieldName = @p1</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string typeFullName, string cmdName, string condition = null, DynamicParameters dynParameters = null)
        {
            Command cmd = _builder.Build(typeFullName, cmdName, condition);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.Query<T>(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <param name="command">查询脚本</param>
        /// <param name="dynParameters">命令参数</param>
        /// <param name="commandType">指定如何解释命令字符串</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string command, DynamicParameters dynParameters = null, CommandType? commandType = null)
        {
            return _session.Connection.Query<T>(command, dynParameters,
                _session.Transaction, false, _session.DataSource.CommandTimeout, commandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="dynParameters">命令参数</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public SqlMapper.GridReader QueryMultiple<T>(string cmdName, DynamicParameters dynParameters = null, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, predicate);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.QueryMultiple(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="condition">查询条件 FieldName = @p1</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public SqlMapper.GridReader QueryMultiple(string typeFullName, string cmdName, string condition = null, DynamicParameters dynParameters = null)
        {
            Command cmd = _builder.Build(typeFullName, cmdName, condition);
            return this.QueryMultiple(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="command">查询脚本</param>
        /// <param name="dynParameters">命令参数</param>
        /// <param name="commandType">指定如何解释命令字符串</param>
        /// <returns></returns>
        public SqlMapper.GridReader QueryMultiple(string command, DynamicParameters dynParameters = null, CommandType? commandType = null)
        {
            return _session.Connection.QueryMultiple(command, dynParameters,
                _session.Transaction, _session.DataSource.CommandTimeout, commandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="predicate">筛选谓词</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public DataSet QueryDataSet<T>(string cmdName, Expression<Func<T, bool>> predicate = null, DynamicParameters dynParameters = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, predicate);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.QueryDataSet(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="condition">查询条件 FieldName = @p1</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public DataSet QueryDataSet(string typeFullName, string cmdName, string condition = null, DynamicParameters dynParameters = null)
        {
            Command cmd = _builder.Build(typeFullName, cmdName, condition);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.QueryDataSet(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="command">查询脚本</param>
        /// <param name="dynParameters">命令参数</param>
        /// <param name="commandType">指定如何解释命令字符串</param>
        /// <returns></returns>
        public DataSet QueryDataSet(string command, DynamicParameters dynParameters = null, CommandType? commandType = null)
        {
            IDataReader reader = _session.Connection.ExecuteReader(command, dynParameters,
                _session.Transaction, _session.DataSource.CommandTimeout, commandType);
            DataSet ds = new XDataSet();
            ds.Load(reader, LoadOption.OverwriteChanges, null, new DataTable[] { });
            return ds;
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public DataTable QueryDataTable<T>(Expression<Func<T, bool>> predicate = null) where T : class
        {
            return this.QueryDataTable<T>(CommandBuilder.Select, predicate);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="page">分页信息</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public DataTable QueryDataTable<T>(PageInfo page, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(page, predicate);
            DynamicParameters dynParameters = this.SetupParameter(cmd.Parameters);
            return this.QueryDataTable(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="predicate">筛选谓词</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public DataTable QueryDataTable<T>(string cmdName, Expression<Func<T, bool>> predicate = null, DynamicParameters dynParameters = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, predicate);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.QueryDataTable(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="condition">查询条件 FieldName = @p1</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public DataTable QueryDataTable(string typeFullName, string cmdName, string condition = null, DynamicParameters dynParameters = null)
        {
            Command cmd = _builder.Build(typeFullName, cmdName, condition);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.QueryDataTable(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="command">查询脚本</param>
        /// <param name="dynParameters">脚本参数</param>
        /// <param name="commandType">脚本类型</param>
        /// <returns></returns>
        public DataTable QueryDataTable(string command, DynamicParameters dynParameters = null, CommandType? commandType = null)
        {
            IDataReader reader = _session.Connection.ExecuteReader(command, dynParameters,
                _session.Transaction, _session.DataSource.CommandTimeout, commandType);
            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="TEntity">实体</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public int Execute<T>(string cmdName, T TEntity, DynamicParameters dynParameters = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, TEntity);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.Execute(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="predicate">筛选谓词</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public int Execute<T>(string cmdName, Expression<Func<T, bool>> predicate = null, DynamicParameters dynParameters = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, predicate);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.Execute(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="condition">查询条件 FieldName = @p1</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns></returns>
        public int Execute(string typeFullName, string cmdName, string condition = null, DynamicParameters dynParameters = null)
        {
            Command cmd = _builder.Build(typeFullName, cmdName, condition);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return this.Execute(cmd.Text, dynParameters, cmd.CommandType);
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <param name="command">查询脚本</param>
        /// <param name="dynParameters">命令参数</param>
        /// <param name="commandType">指定如何解释命令字符串</param>
        /// <returns></returns>
        public int Execute(string command, DynamicParameters dynParameters = null, CommandType? commandType = null)
        {
            return _session.Connection.Execute(command, dynParameters,
                _session.Transaction, _session.DataSource.CommandTimeout, commandType);
        }

        /// <summary>
        /// 执行脚本，返回影响行数
        /// </summary>
        /// <param name="sqlList">查询脚本</param>
        /// <returns></returns>
        public int Execute(IEnumerable<string> sqlList)
        {
            //分批执行脚本，每批200行
            if (sqlList == null) throw new ArgumentNullException("sqlList");

            int count = sqlList.Count();
            int size = 200;
            int time = count % 200 == 0 ? count / 200 : count / 200 + 1;

            int effect = 0;
            bool isOpen = _session.IsOpenTran;
            if (!isOpen) _session.BeginTransaction();

            for (int i = 0; i < time; i++)
            {
                IEnumerable<string> curList = sqlList.Skip(i * size).Take(size);
                string sql = string.Join(Environment.NewLine, curList);
                                
                effect += _session.Connection.Execute(sql, null, _session.Transaction, _session.DataSource.CommandTimeout);
            }

            if (!isOpen) _session.CommitTransaction();

            return effect;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_session != null) _session.Dispose();
        }

        /// <summary>
        /// 生成Dapper脚本，
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="TEntity">实体</param>
        /// <returns>脚本字符串</returns>
        public string Resolve<T>(string cmdName, T TEntity)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, TEntity);
            return _builder.Resolve(cmd);
        }

        /// <summary>
        /// 生成Dapper脚本，
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="predicate">筛选谓词</param>
        /// <param name="dynParameters">命令参数</param>
        /// <returns>脚本字符串</returns>
        public string Resolve<T>(string cmdName, Expression<Func<T, bool>> predicate = null, DynamicParameters dynParameters = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(cmdName, predicate);
            this.SetupParameter(ref dynParameters, cmd.Parameters);
            return _builder.Resolve(cmd);
        }

        /// <summary>
        /// 生成Dapper脚本，
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="page">分页信息</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns>脚本字符串</returns>
        public string Resolve<T>(PageInfo page, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(page, predicate);
            return _builder.Resolve(cmd);
        }

        /// <summary>
        /// 生成Dapper脚本，
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="updater">更新表达式</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns>脚本字符串</returns>
        public string Resolve<T>(Expression<Func<T, T>> updater, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            Command cmd = _builder.Build<T>(updater, predicate);
            return _builder.Resolve(cmd);
        }

        /// <summary>
        /// 生成Dapper脚本，
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="condition">查询条件 FieldName = @p1</param>
        /// <returns></returns>
        public string Resolve(string typeFullName, string cmdName, string condition = null)
        {
            Command cmd = _builder.Build(typeFullName, cmdName, condition);
            return _builder.Resolve(cmd);
        }

        #endregion

        #region 辅助方法

        //将Command的参数转化成Dapper执行参数
        protected void SetupParameter(ref DynamicParameters dynParameters, IEnumerable<Parameter> lstParameters)
        {
            if (dynParameters == null) dynParameters = new DynamicParameters();
            foreach (Parameter p in lstParameters)
            {
                if (!dynParameters.Parameters.ContainsKey(p.Name))
                {
                    //添加参数
                    dynParameters.Add(p.Name, p.Value, p.DbType, p.Direction, p.Size);
                }
                else
                {
                    //指定参数配置
                    dynParameters.Parameters[p.Name].DbType = p.DbType;
                    dynParameters.Parameters[p.Name].ParameterDirection = p.Direction ?? ParameterDirection.Input;
                    dynParameters.Parameters[p.Name].Size = p.Size;

                    //接收外部传递的值
                    p.Value = dynParameters.Parameters[p.Name].Value;
                }
            }
        }

        //转换Dapper执行参数
        private DynamicParameters SetupParameter(IEnumerable<Parameter> lstParameters)
        {
            DynamicParameters dynParameters = new DynamicParameters();
            foreach (Parameter p in lstParameters)
            {
                dynParameters.Add(p.Name, p.Value, p.DbType, p.Direction, p.Size);
            }

            return dynParameters;
        }

        #endregion
    }
}
