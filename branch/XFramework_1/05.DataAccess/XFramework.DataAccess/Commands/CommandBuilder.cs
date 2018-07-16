using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using XFramework.Core;
using EmitMapper.MappingConfiguration;

namespace XFramework.DataAccess
{
    /// <summary>
    /// 生成SQL脚本类
    /// </summary>
    public class CommandBuilder
    {
        #region 私有变量

        private string _parameterPrefix = "@";                      //参数前缀
        private readonly string _placeHolderWhere = "#WHERE#";      //Where 过滤条件占位符
        private readonly string _placeHolderSet = "#SET#";          //Set 占位符
        private readonly string _placeHolderBetween = "#BETWEEN#";  //Between 占位符

        #endregion

        #region 公开属性

        /// <summary>
        /// 删除键
        /// </summary>
        public static readonly string Delete = "Delete";

        /// <summary>
        /// 删除键
        /// </summary>
        public static readonly string DeleteByKey = "DeleteByKey";

        /// <summary>
        /// 更新键
        /// </summary>
        public static readonly string Update = "Update";

        /// <summary>
        /// 更新键
        /// </summary>
        public static readonly string UpdateByKey = "UpdateByKey";

        /// <summary>
        /// 更新键
        /// </summary>
        public static readonly string UpdateByExpr = "UpdateByExpr";

        /// <summary>
        /// 插入键
        /// </summary>
        public static readonly string Insert = "Insert";

        /// <summary>
        /// 选取键
        /// </summary>
        public static readonly string Select = "Select";

        /// <summary>
        /// 选取键
        /// </summary>
        public static readonly string SelectByKey = "SelectByKey";

        /// <summary>
        /// 选取键
        /// </summary>
        public static readonly string SelectByPaging = "SelectByPaging";

        /// <summary>
        /// 参数查询前缀
        /// </summary>
        public string ParameterPrefix
        {
            get { return _parameterPrefix; }
        }

        #endregion

        #region 构造函数

        public CommandBuilder()
            : this("@")
        { }

        public CommandBuilder(string parameterPrefix)
        {
            _parameterPrefix = parameterPrefix;
        }

        #endregion

        #region 重写方法

        #endregion

        #region 公开方法

        /// <summary>
        /// 生成命令（适用新增/按主键修改/按主键删除）
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="cmdName">脚本键值</param>
        /// <param name="TEntity">实体</param>
        /// <returns></returns>
        public Command Build<T>(string cmdName, T TEntity)
            where T : class
        {
            Command cmd = this.GetCommand(typeof(T), cmdName);
            foreach (Parameter parameter in cmd.Parameters)
            {
                object value = AccFacHelper.Get(TEntity, parameter.Name);
                parameter.Value = value;
            }

            return cmd;
        }

        /// <summary>
        /// 生成命令（适用查询/批量删除/批量修改）
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="cmdName">脚本键值</param>
        /// <param name="predicate">筛选谓词</param>
        /// <returns></returns>
        public Command Build<T>(string cmdName, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            //获取配置脚本
            Command cmd = this.GetCommand(typeof(T), cmdName);

            //解析过滤条件
            if (predicate == null) predicate = Common.True<T>();
            ConditionBuilder builder = new ConditionBuilder(_parameterPrefix);
            builder.Build(predicate);
            cmd.Text = Regex.Replace(cmd.Text, _placeHolderWhere, builder.Condition, RegexOptions.IgnoreCase);

            //添加脚本参数
            foreach (var bParameter in builder.Parameters)
            {
                //参数
                string parameterName = bParameter.Key;
                //字段
                string propertyName = builder.ParameterMembers[parameterName];
                Parameter parameter = cmd.Parameters.FirstOrDefault(x => x.Name == parameterName);

                if (parameter == null)
                {
                    parameter = new Parameter { Name = parameterName, Value = bParameter.Value };
                    this.SetupParameter(cmd.Mapper, parameter, propertyName);
                    cmd.Parameters.Add(parameter);
                }
                else
                {
                    parameter.Value = bParameter.Value;
                }
            }

            return cmd;
        }

        /// <summary>
        /// 生成命令（自定义脚本）
        /// </summary>
        /// <param name="typeFullName">命令键值</param>
        /// <param name="cmdName">查询脚本</param>
        /// <param name="conidtion">WHERE</param>
        /// <returns></returns>
        public Command Build(string typeFullName, string cmdName, string conidtion = null)
        {
            //if (!string.IsNullOrEmpty(conidtion) && ContainsKeyWord(conidtion))
            //    throw new NotSupportedException("condition contains sql keyword");

            Command cmd = this.GetCommand(typeFullName, cmdName);
            cmd.CommandType = cmd.CommandType ?? CommandType.Text;
            conidtion = conidtion ?? string.Empty;
            cmd.Text = Regex.Replace(cmd.Text, _placeHolderWhere, conidtion, RegexOptions.IgnoreCase);

            return cmd;
        }

        /// <summary>
        /// 生成命令（适用批量更新）
        /// </summary>
        /// <param name="updater">更新表达式</param>
        /// <param name="predicate">筛选谓词</param>
        internal Command Build<T>(Expression<Func<T, T>> updater, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            //UPDATE TABLE #SET# WHERE 1=1 And FieldName = @p0
            Command cmd = this.Build<T>(CommandBuilder.UpdateByExpr, predicate);

            MemberInitExpression updateExpr = (MemberInitExpression)updater.Body;
            StringBuilder sqlBuilder = new StringBuilder(Environment.NewLine);
            for (int i = 0; i < updateExpr.Bindings.Count; i++)
            {
                //SQL片断
                MemberAssignment member = (MemberAssignment)updateExpr.Bindings[i];
                sqlBuilder.AppendFormat("[{0}] = {1}{0}", member.Member.Name, _parameterPrefix);
                if (i < updateExpr.Bindings.Count - 1) sqlBuilder.Append(",");
                sqlBuilder.AppendLine();

                //SQL参数
                Parameter p = new Parameter { Name = member.Member.Name, Value = ((ConstantExpression)member.Expression).Value, Direction = ParameterDirection.Input };
                this.SetupParameter(cmd.Mapper, p);
                cmd.Parameters.Add(p);
            }

            cmd.Text = Regex.Replace(cmd.Text, _placeHolderSet, sqlBuilder.ToString(), RegexOptions.IgnoreCase);
            return cmd;
        }

        /// <summary>
        /// 生成命令（适用分页）
        /// </summary>
        /// <param name="page">分页对象</param>
        /// <param name="predicate">筛选谓词</param>
        internal Command Build<T>(PageInfo page, Expression<Func<T, bool>> predicate = null)
            where T : class
        {
            Command cmd = this.Build<T>(CommandBuilder.SelectByPaging, predicate);
            int beginPage = (page.CurrentPage - 1) * page.PageSize + 1;
            int endPage = page.CurrentPage * page.PageSize;
            cmd.Parameters.Add(new Parameter { Name = string.Format("p{0}", cmd.Parameters.Count), DbType = DbType.Int32, NativeType = "int", Value = beginPage });
            cmd.Parameters.Add(new Parameter { Name = string.Format("p{0}", cmd.Parameters.Count), DbType = DbType.Int32, NativeType = "int", Value = endPage });

            string between = string.Format("{0}{1} And {0}{2}",
                _parameterPrefix, cmd.Parameters[cmd.Parameters.Count - 2].Name, cmd.Parameters[cmd.Parameters.Count - 1].Name);
            cmd.Text = Regex.Replace(cmd.Text, _placeHolderBetween, between, RegexOptions.IgnoreCase);
            return cmd;
        }

        /// <summary>
        /// 生成命令
        /// </summary>
        /// <param name="cmd">脚本命令</param>
        /// <remarks>适用批量执行多个脚本</remarks>
        public string Resolve(Command cmd)
        {
            //Declare @xp int exec sp_executesql N'set @id = 2',N'@id int output',@id = @xp output

            StringBuilder sqlBuilder = new StringBuilder();     //最终产生的sql脚本
            StringBuilder varBuilder = new StringBuilder();     //如果有output类型参数，声明与output参数相绑定的本地参数
            StringBuilder typeBuilder = new StringBuilder();    //参数类型声明
            StringBuilder valBuilder = new StringBuilder();     //参数值声明
            StringBuilder returnBuilder = new StringBuilder();  //返回值声明

            for (int i = 0; i < cmd.Parameters.Count; i++)
            {
                Parameter p = cmd.Parameters[i];
                if (string.Compare(p.Name, "RETURN_VALUE") == 0) continue;

                string native = this.GetSqlType(p);
                string value = this.GetSqlValue(p);
                bool output = p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output;
                string outputName = string.Empty;

                if (output)
                {
                    outputName = string.Format("{0}p{1}_", _parameterPrefix, Guid.NewGuid());
                    outputName = outputName.Replace('-','_');
                    varBuilder.AppendFormat("declare {0} {1} {2}", outputName, native, Environment.NewLine);
                    returnBuilder.AppendFormat("SELECT {0},", outputName);
                }

                typeBuilder.AppendFormat("{0}{1} {2}", _parameterPrefix, p.Name, native);
                if (output) typeBuilder.Append(" output");
                if (i < cmd.Parameters.Count - 1) typeBuilder.Append(",");

                valBuilder.AppendFormat("{0}{1} = {2}", _parameterPrefix, p.Name, output ? outputName : value);
                if (output) valBuilder.Append(" output");
                if (i < cmd.Parameters.Count - 1) valBuilder.Append(",");
            }

            //组装最后的命令脚本
            sqlBuilder.Append(varBuilder);
            if (cmd.CommandType == CommandType.StoredProcedure)
            {
                //存储过程目前感觉没必要生成配置文件。。。
                sqlBuilder.Append("exec ");
                sqlBuilder.Append(cmd.Text);
                sqlBuilder.Append(" ");
            }
            else
            {
                sqlBuilder.Append("exec sp_executesql N'");
                sqlBuilder.Append(cmd.Text);
                sqlBuilder.Append("',N'");
                sqlBuilder.Append(typeBuilder);
                sqlBuilder.Append("',");
            }
            sqlBuilder.Append(valBuilder);
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine(returnBuilder.ToString().TrimEnd(','));

            return sqlBuilder.ToString();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 查找脚本命令
        /// </summary>
        protected Command GetCommand(Type tableType, string cmdName)
        {
            EntityMapper mapper = ConfigHelper.GetMapper(tableType);
            return this.GetCommand(mapper, cmdName);
        }

        /// <summary>
        /// 查找脚本命令
        /// </summary>
        protected Command GetCommand(string typeFullName, string cmdName)
        {
            EntityMapper mapper = ConfigHelper.GetMapper(typeFullName);
            return this.GetCommand(mapper, cmdName);
        }

        /// <summary>
        /// 查找脚本命令
        /// </summary>
        protected Command GetCommand(EntityMapper mapper, string cmdName)
        {
            if (string.IsNullOrEmpty(cmdName))
                throw new ArgumentNullException("cmdName");

            Command cmd = mapper.Commands.FirstOrDefault(x => x.Key.ToUpper() == cmdName.ToUpper());
            if (cmd == null) throw new KeyNotFoundException(string.Format("can't find command {0} from mapfile {1}", cmdName, mapper.TableType.TypeFullName));

            string pattern = "^p[0-9]+$";
            foreach (Parameter p in cmd.Parameters)
            {
                if (Regex.IsMatch(p.Name, pattern))
                    throw new NotSupportedException("custom parameter name should not be like p[0-9]");
            }

            //注：Parameter的Value属性是object类型，如果用深复制的话会拷贝不到值
            DefaultMapConfig conifg =
                new DefaultMapConfig()
                .DeepMap()
                .ShallowMap(typeof(object));
            Command cmdMap = Common.Map<Command, Command>(cmd, conifg);

            cmdMap.Mapper = mapper;
            return cmdMap;
        }

        //根据实体映射设置参数信息
        //注：从实体属性中读取类型和长度，避免有可能将varchar强制转成nvarchar
        private void SetupParameter(EntityMapper mapper, Parameter p, string propertyName = null)
        {
            propertyName = propertyName ?? p.Name;
            Property property = mapper.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (property != null)
            {
                p.DbType = property.DbType;
                p.Size = property.Size;
                p.NativeType = property.NativeType;
                p.Scale = property.Scale;
                p.Precision = property.Precision;
            }
            else if (p.Value != null)
            {
                Type type = p.Value.GetType();
                int vLength = p.Value.ToString().Length;

                p.DbType = this.GetDbType(type);
                p.NativeType = this.GetNativeType(p.DbType);

                if (type == typeof(string) ||
                    type == typeof(char) ||
                    type == typeof(char[])) p.Size = vLength;

                if (type == typeof(float) ||
                    type == typeof(double) ||
                    type == typeof(decimal))
                {
                    p.Precision = (byte)vLength;
                    string[] array = p.Value.ToString().Split('.');
                    p.Scale = array.Length == 1 ? 0 : array[1].Length;
                }
            }
        }

        //根据命令参数获取Sql参数值
        private string GetSqlValue(Parameter p)
        {
            if (p.Value == null) return "NULL";
            switch (p.DbType)
            {
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return string.Format("'{0}'", p.Value);

                case DbType.Date:
                case DbType.DateTime:
                    return string.Format("'{0}'", Convert.ToDateTime(p.Value).ToString("yyyy-MM-dd HH:mm:ss.fff"));

                case DbType.Boolean:
                    return Convert.ToBoolean(p.Value) ? "1" : "0";

                default:
                    return p.Value.ToString();
            }
        }

        //根据命令参数转Sql类型
        private string GetSqlType(Parameter p)
        {
            switch (p.DbType)
            {
                case DbType.Decimal:
                case DbType.Double:
                    return string.Format("{0}({1},{2})", p.NativeType, p.Precision, p.Scale);

                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return string.Format("{0}({1})", p.NativeType, p.Size > 0 ? p.Size.ToString() : (p.Value != null ? p.Value.ToString().Length.ToString() : "Max"));

                default:
                    return p.NativeType;
            }
        }

        //.Net类型转DbType
        private DbType GetDbType(Type type)
        {
            if (type == typeof(byte)) return DbType.Byte;
            if (type == typeof(sbyte)) return DbType.SByte;
            if (type == typeof(short)) return DbType.Int16;
            if (type == typeof(ushort)) return DbType.UInt16;
            if (type == typeof(int)) return DbType.Int32;
            if (type == typeof(uint)) return DbType.UInt32;
            if (type == typeof(long)) return DbType.Int64;
            if (type == typeof(ulong)) return DbType.UInt64;
            if (type == typeof(float)) return DbType.Single;
            if (type == typeof(double)) return DbType.Double;
            if (type == typeof(decimal)) return DbType.Decimal;
            if (type == typeof(bool)) return DbType.Boolean;
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(char[])) return DbType.String;
            if (type == typeof(char)) return DbType.StringFixedLength;
            if (type == typeof(Guid)) return DbType.Guid;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(DateTimeOffset)) return DbType.DateTimeOffset;
            if (type == typeof(TimeSpan)) return DbType.Time;
            if (type == typeof(byte[])) return DbType.Binary;

            if (type == typeof(byte?)) return DbType.Byte;
            if (type == typeof(sbyte?)) return DbType.SByte;
            if (type == typeof(short?)) return DbType.Int16;
            if (type == typeof(ushort?)) return DbType.UInt16;
            if (type == typeof(int?)) return DbType.Int32;
            if (type == typeof(uint?)) return DbType.UInt32;
            if (type == typeof(long?)) return DbType.Int64;
            if (type == typeof(ulong?)) return DbType.UInt64;
            if (type == typeof(float?)) return DbType.Single;
            if (type == typeof(double?)) return DbType.Double;
            if (type == typeof(decimal?)) return DbType.Decimal;
            if (type == typeof(bool?)) return DbType.Boolean;
            if (type == typeof(char?)) return DbType.StringFixedLength;
            if (type == typeof(Guid?)) return DbType.Guid;
            if (type == typeof(DateTime?)) return DbType.DateTime;
            if (type == typeof(DateTimeOffset?)) return DbType.DateTimeOffset;
            if (type == typeof(TimeSpan?)) return DbType.Time;
            if (type == typeof(object)) return DbType.Object;

            throw new NotSupportedException("unkown DbType __" + type.FullName);
        }

        //DbType转本地数据类型
        public string GetNativeType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int16:
                case DbType.UInt16:
                    return "smallint";
                case DbType.Int32:
                case DbType.UInt32:
                    return "int";
                case DbType.Int64:
                case DbType.UInt64:
                    return "bigint";
                case DbType.String:
                    return "nvarchar";
                case DbType.AnsiString:
                    return "varchar";
                case DbType.StringFixedLength:
                    return "nchar";
                case DbType.AnsiStringFixedLength:
                    return "char";
                case DbType.DateTime:
                    return "datetime";
                case DbType.Single:
                case DbType.Double:
                case DbType.Decimal:
                    return "numeric";
                case DbType.Boolean:
                    return "bit";
                case DbType.Xml:
                    return "xml";
                case DbType.Guid:
                    return "guid";

                default:
                    throw new NotSupportedException("unkown DbType __" + dbType);
            }
        }

        //sql过滤关键字   
        public bool ContainsKeyWord(string word)
        {
            //过滤关键字
            string strKeyWord = @"select|insert|delete|from|count\(|drop table|update|truncate|asc\(|mid\(|char\(|xp_cmdshell|exec master|netlocalgroup administrators|:|net user|""|or|and";
            //过滤关键字符
            string strRegex = @"[-|;|,|/|\(|\)|\[|\]|}|{|%|\@|*|!|']";
            if (Regex.IsMatch(word, strKeyWord, RegexOptions.IgnoreCase) || Regex.IsMatch(word, strRegex))
                return true;
            return false;
        }

        #endregion
    }
}
