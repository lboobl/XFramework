
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace ICS.XFramework.Data.SqlClient
{
    /// <summary>
    /// 数据查询提供者
    /// </summary>
    /// <remarks>
    /// 2、表别名根据Lambda表达式的参数取得
    /// </remarks>
    public sealed class DbQueryProvider : DbQueryProviderBase
    {
        private static readonly string WITHNOLOCK = string.Empty;
        private MethodCallExressionVisitor _methodVisitor = null;

        /// <summary>
        /// 方法表达式访问器
        /// </summary>
        public override MethodCallExressionVisitorBase MethodVisitor
        {
            get
            {
                _methodVisitor = _methodVisitor ?? new MethodCallExressionVisitor();
                return _methodVisitor;
            }
        }

        /// <summary>
        /// 数据库安全字符 左
        /// </summary>
        public override string EscCharLeft
        {
            get
            {
                return "[";
            }
        }

        /// <summary>
        /// 数据库安全字符 右
        /// </summary>
        public override string EscCharRight
        {
            get
            {
                return "]";
            }
        }

        /// <summary>
        /// 字符串引号
        /// </summary>
        public override string EscCharQuote
        {
            get
            {
                return "'";
            }
        }

        /// <summary>
        /// 数据查询提供者 名称
        /// </summary>
        public override string ProviderName
        {
            get
            {
                return "SqlClient";
            }
        }

        /// <summary>
        /// 命令参数前缀
        /// </summary>
        public override string ParameterPrefix
        {
            get
            {
                return "@";
            }
        }

        /// <summary>
        /// 初始化 <see cref="DbQueryProvider"/> 类的新实例
        /// </summary>
        public DbQueryProvider()
            : this(XCommon.GetConnString("XFrameworkConnString"))
        { }

        /// <summary>
        /// 初始化 <see cref="DbQueryProvider"/> 类的新实例
        /// </summary>
        /// <param name="connString">数据库连接字符串</param>
        public DbQueryProvider(string connString)
            : base(SqlClientFactory.Instance, connString)
        {
        }

        // 创建 SELECT 命令
        protected override CommandBase ParseSelectCommand<T>(DbQueryableInfo_Select<T> qQuery, int indent = 0)
        {
            // 说明：
            // 1.OFFSET 前必须要有 'ORDER BY'，即 'Skip' 子句前必须使用 'OrderBy' 子句
            // 2.在有统计函数的<MAX,MIN...>情况下，如果有 'Distinct' 'GroupBy' 'Skip' 'Take' 子句，则需要使用嵌套查询
            // 3.'Any' 子句将翻译成 IF EXISTS...
            // 4.分组再分页时需要使用嵌套查询，此时子查询不需要 'OrderBy' 子句，但最外层则需要
            // 5.'Skip' 'Take' 子句视为语义结束符，在其之后的子句将使用嵌套查询
            // 6.导航属性中有 1:n 关系的，需要使用嵌套查询，否则分页查询会有问题
            // 7.todo 越来越复杂化了 :(


            // 导航属性中有1:n关系，只统计主表
            // 例：AccountList = a.Client.AccountList,
            DbQueryableInfo_Select<T> innerQuery = qQuery.InnerQuery as DbQueryableInfo_Select<T>;
            if (qQuery.HaveListNavigation && innerQuery != null && innerQuery.Statis != null) qQuery = innerQuery;

            bool willNest = qQuery.HaveDistinct || qQuery.GroupBy != null || qQuery.Skip > 0 || qQuery.Take > 0;
            bool useStatis = qQuery.Statis != null;
            // 分组分页   
            //bool groupByPaging = qQuery.GroupBy != null && qQuery.Skip > 0;
            // 没有统计函数或者使用 'Skip' 子句，则解析OrderBy
            // 导航属性如果使用嵌套，除非有 TOP 或者 OFFSET 子句，否则不能用ORDER BY
            bool useOrderBy = (!useStatis || qQuery.Skip > 0) && !qQuery.HaveAny && (!qQuery.IsListNavigationQuery || (qQuery.Skip > 0 || qQuery.Take > 0));

            //ExpressionVisitorBase visitor = null;
            TableAliasCache aliases = this.PrepareAlias<T>(qQuery);
            string sColumnName = string.Empty;

            CommandDefinition cd = new CommandDefinition(this, aliases);
            cd.HaveListNavigation = qQuery.HaveListNavigation;
            SqlBuilder jf = cd.JoinFragment;
            SqlBuilder wf = cd.WhereFragment;

            //if (groupByPaging) indent = indent + 1;
            jf.Indent = indent;

            #region 嵌套查询

            if (useStatis && willNest)
            {
                // SELECT
                jf.Append("SELECT ");
                jf.AppendNewLine();

                // SELECT COUNT(1)
                StatisExpressionVisitor visitor = new StatisExpressionVisitor(this, aliases, qQuery.Statis, qQuery.GroupBy, "t0");
                visitor.Write(jf);
                sColumnName = visitor.ColumnName;
                cd.AddNavigation(visitor.Navigations);

                // SELECT COUNT(1) FROM
                jf.AppendNewLine();
                jf.Append("FROM ( ");

                indent += 1;
                jf.Indent = indent;
            }

            #endregion

            // SELECT 子句
            if (jf.Indent > 0) jf.AppendNewLine();

            if (qQuery.HaveAny)
            {
                jf.Append("IF EXISTS(");
                indent += 1;
                jf.Indent = indent;
                jf.AppendNewLine();
            }

            jf.Append("SELECT ");

            if (useStatis && !willNest)
            {
                // 如果有统计函数，并且不是嵌套的话，则直接使用SELECT <MAX,MIN...>，不需要解析选择的字段
                jf.AppendNewLine();
                StatisExpressionVisitor visitor = new StatisExpressionVisitor(this, aliases, qQuery.Statis, qQuery.GroupBy);
                visitor.Write(jf);
                cd.AddNavigation(visitor.Navigations);
            }
            else
            {

                // DISTINCT 子句
                if (qQuery.HaveDistinct) jf.Append("DISTINCT ");

                // TOP 子句
                if (qQuery.Take > 0 && qQuery.Skip == 0) jf.AppendFormat("TOP({0})", qQuery.Take);

                // Any 
                if (qQuery.HaveAny) jf.Append("TOP 1 1");

                #region 选择字段

                if (!qQuery.HaveAny)
                {
                    // SELECT 范围
                    ColumnExpressionVisitor visitor = new ColumnExpressionVisitor(this, aliases, qQuery);
                    visitor.Write(jf);

                    cd.Columns = visitor.Columns;
                    cd.NavigationDescriptors = visitor.NavigationDescriptors;
                    cd.AddNavigation(visitor.Navigations);

                    // 如果有统计，选择列中还要追加统计的列
                    if (useStatis && willNest && !string.IsNullOrEmpty(sColumnName))
                    {
                        if (cd.Columns.Count > 0) jf.Append(",");
                        visitor = new ColumnExpressionVisitor(this, aliases, qQuery, true, cd.Columns);
                        visitor.Write(jf);

                        cd.AddNavigation(visitor.Navigations);
                    }

                    //// 如果分组后再分页，此时需要在原先的选择字段上再加上 'OrderBy' 子句指定的字段，外层的分页时需要用到这些排序字段
                    //if (qQuery.OrderBy.Count > 0 && useOrderBy && groupByPaging)
                    //{
                    //    if (cd.Columns.Count > 0) jf.Append(",");
                    //    for (int i = 0; i < qQuery.OrderBy.Count; i++)
                    //    {
                    //        visitor = new ColumnExpressionVisitor(this, aliases, qQuery.OrderBy[i], qQuery.GroupBy, true, cd.Columns);
                    //        visitor.HaveListNavigation = qQuery.HaveListNavigation;
                    //        visitor.Write(jf);

                    //        cd.AddNavigation(visitor.Navigations);
                    //        if (i < qQuery.OrderBy.Count - 1) jf.AppendNewLine(",");
                    //    }
                    //}
                }

                #endregion
            }

            // FROM 子句
            jf.AppendNewLine();
            jf.Append("FROM ");
            if (qQuery.InnerQuery != null)
            {
                // 子查询
                jf.Append("(");
                CommandBase define = this.ParseSelectCommand<T>(qQuery.InnerQuery as DbQueryableInfo_Select<T>, indent + 1);
                jf.Append(define.CommandText);
                jf.AppendNewLine();
                jf.Append(")");
            }
            else
            {
                jf.AppendMember(TypeRuntimeInfoCache.GetRuntimeInfo(qQuery.FromType).TableName);
            }
            jf.Append(" t0 ");
            if (!string.IsNullOrEmpty(DbQueryProvider.WITHNOLOCK)) jf.Append(DbQueryProvider.WITHNOLOCK);

            // LEFT<INNER> JOIN 子句
            ExpressionVisitorBase  visitorBase = new JoinExpressionVisitor(this, aliases, qQuery.Join);
            visitorBase.Write(jf);

            wf.Indent = jf.Indent;

            // WHERE 子句
            visitorBase = new WhereExpressionVisitor(this, aliases, qQuery.Where);
            visitorBase.Write(wf);
            cd.AddNavigation(visitorBase.Navigations);

            // GROUP BY 子句
            visitorBase = new GroupByExpressionVisitor(this, aliases, qQuery.GroupBy);
            visitorBase.Write(wf);
            cd.AddNavigation(visitorBase.Navigations);

            // HAVING 子句
            visitorBase = new HavingExpressionVisitor(this, aliases, qQuery.Having, qQuery.GroupBy);
            visitorBase.Write(wf);
            cd.AddNavigation(visitorBase.Navigations);

            // ORDER 子句
            if (qQuery.OrderBy.Count > 0 && useOrderBy)// && !groupByPaging)
            {
                visitorBase = new OrderByExpressionVisitor(this, aliases, qQuery.OrderBy, qQuery.GroupBy);
                visitorBase.Write(wf);
                cd.AddNavigation(visitorBase.Navigations);
            }

            #region 分页查询

            if (qQuery.Skip > 0)// && !groupByPaging)
            {
                if (qQuery.OrderBy.Count == 0) throw new XFrameworkException("The method 'OrderBy' must be called before the method 'Skip'.");
                wf.AppendNewLine();
                wf.Append("OFFSET ");
                wf.Append(qQuery.Skip);
                wf.Append(" ROWS");

                if (qQuery.Take > 0)
                {
                    wf.Append(" FETCH NEXT ");
                    wf.Append(qQuery.Take);
                    wf.Append(" ROWS ONLY ");
                }
            }

            #endregion

            #region 嵌套查询

            if (useStatis && willNest)
            {
                string inner = cd.CommandText;
                indent -= 1;
                jf.Indent = indent;
                jf.AppendNewLine();
                jf.Append(" ) t0");
            }

            #endregion

            #region 分组分页

            //if (groupByPaging)
            //{
            //    SqlBuilder builder = new SqlBuilder(this);

            //    // SELECT
            //    int index = -1;
            //    builder.Append("SELECT ");
            //    foreach (var kvp in cd.Columns)
            //    {
            //        index += 1;
            //        builder.AppendNewLine();
            //        builder.AppendMember("t0", kvp.Key);
            //        if (index < cd.Columns.Count - 1) builder.Append(",");
            //    }

            //    builder.AppendNewLine();
            //    builder.Append("FROM ( ");

            //    string inner = cd.CommandText;
            //    //jf.Replace(Environment.NewLine, Environment.NewLine + SqlBuilder.TAB);
            //    jf.Insert(0, builder);


            //    indent -= 1;
            //    jf.Indent = indent;
            //    jf.AppendNewLine();
            //    jf.Append(" ) t0");

            //    // 排序
            //    if (qQuery.OrderBy.Count > 0 && useOrderBy)
            //    {
            //        visitorBase = new OrderByExpressionVisitor(this, aliases, qQuery.OrderBy, null, "t0");
            //        visitorBase.Write(jf);
            //    }

            //    // 分页
            //    if (qQuery.Skip > 0)
            //    {
            //        if (qQuery.OrderBy.Count == 0) throw new XFrameworkException("'OrderBy' must be called before the method 'Skip'.");
            //        jf.AppendNewLine();
            //        jf.Append("OFFSET ");
            //        jf.Append(qQuery.Skip);
            //        jf.Append(" ROWS");

            //        if (qQuery.Take > 0)
            //        {
            //            jf.Append(" FETCH NEXT ");
            //            jf.Append(qQuery.Take);
            //            jf.Append(" ROWS ONLY ");
            //        }
            //    }
            //}

            #endregion

            #region 嵌套导航

            if (qQuery.HaveListNavigation && innerQuery != null && innerQuery.OrderBy.Count > 0 && innerQuery.Statis == null && !(innerQuery.Skip > 0 || innerQuery.Take > 0))
            //if (qQuery.HaveListNavigation && innerQuery != null && innerQuery.OrderBy.Count > 0 && innerQuery.Statis == null && qQuery.Statis == null)
            {
                string sql = cd.CommandText;
                visitorBase = new OrderByExpressionVisitor(this, aliases, innerQuery.OrderBy, null, "t0");
                visitorBase.Write(jf);
            }

            #endregion

            // 'Any' 子句
            if (qQuery.HaveAny)
            {
                string inner = cd.CommandText;
                indent -= 1;
                jf.Indent = indent;
                jf.AppendNewLine();
                jf.Append(") SELECT 1 ELSE SELECT 0");
            }

            // UNION 子句
            if (qQuery.Union != null && qQuery.Union.Count > 0)
            {
                string inner = cd.CommandText;
                for (int index = 0; index < qQuery.Union.Count; index++)
                {
                    jf.AppendNewLine();
                    jf.AppendNewLine("UNION ALL");
                    CommandBase define = this.ParseSelectCommand<T>(qQuery.Union[index] as DbQueryableInfo_Select<T>);
                    jf.Append(define.CommandText);
                }

            }

            return cd;
        }

        // 创建 INSRT 命令
        protected override CommandBase ParseInsertCommand<T>(DbQueryableInfo_Insert<T> qInsert)
        {
            SqlBuilder builder = new SqlBuilder(this);
            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
            TableAliasCache aliases = new TableAliasCache();

            if (qInsert.Entity != null)
            {
                object entity = qInsert.Entity;
                SqlBuilder columns = new SqlBuilder(this);
                SqlBuilder values = new SqlBuilder(this);

                foreach (var kv in typeRuntime.Invokers)
                {
                    MemberInvokerBase invoker = kv.Value;
                    var column = invoker.Column;
                    if (column != null && column.NoMapped) continue;
                    if (invoker.ForeignKey != null) continue;
                    if (invoker.Member.MemberType == System.Reflection.MemberTypes.Method) continue;

                    if (invoker != qInsert.AutoIncrement)
                    {
                        columns.AppendMember(invoker.Member.Name);
                        columns.Append(',');

                        var value = invoker.Invoke(entity);
                        string seg = this.GetSqlSeg(value);
                        values.Append(seg);
                        values.Append(',');
                    }
                }
                columns.Length -= 1;
                values.Length -= 1;

                if (qInsert.Bulk == null || !qInsert.Bulk.OnlyValue)
                {
                    builder.Append("INSERT INTO ");
                    builder.AppendMember(typeRuntime.TableName);
                    builder.AppendNewLine();
                    builder.Append('(');
                    builder.Append(columns);
                    builder.Append(')');
                    builder.AppendNewLine();
                    builder.Append("VALUES");
                    builder.AppendNewLine();
                }

                builder.Append('(');
                builder.Append(values);
                builder.Append(')');
                if (qInsert.Bulk != null && !qInsert.Bulk.IsOver) builder.Append(",");

                if (qInsert.Bulk == null && qInsert.AutoIncrement != null)
                {
                    builder.AppendNewLine();
                    builder.Append("SELECT CAST(SCOPE_IDENTITY() AS INT)");
                }
            }
            else if (qInsert.SelectInfo != null)
            {
                builder.Append("INSERT INTO ");
                builder.AppendMember(typeRuntime.TableName);
                builder.Append('(');

                CommandDefinition sc = this.ParseSelectCommand(qInsert.SelectInfo) as CommandDefinition;
                //for (int i = 0; i < seg.Columns.Count; i++)
                int i = 0;
                foreach (var kvp in sc.Columns)
                {
                    builder.AppendMember(kvp.Key);
                    if (i < sc.Columns.Count - 1) builder.Append(',');
                    //{
                    //    builder.Append(',');
                    //    builder.AppendNewLine();
                    //}

                    i++;
                }

                builder.Append(')');
                builder.AppendNewLine();
                builder.Append(sc.CommandText);
            }

            return new CommandBase(builder.ToString(), null, System.Data.CommandType.Text); //builder.ToString();
        }

        // 创建 DELETE 命令
        protected override CommandBase ParseDeleteCommand<T>(DbQueryableInfo_Delete<T> qDelete)
        {
            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
            SqlBuilder builder = new SqlBuilder(this);
            bool useKey = false;

            builder.Append("DELETE t0 FROM ");
            builder.AppendMember(typeRuntime.TableName);
            builder.Append(" t0 ");

            if (qDelete.Entity != null)
            {
                object entity = qDelete.Entity;

                builder.AppendNewLine();
                builder.Append("WHERE ");

                foreach (var kv in typeRuntime.Invokers)
                {
                    MemberInvokerBase invoker = kv.Value;
                    var column = invoker.Column;

                    if (column != null && column.IsKey)
                    {
                        useKey = true;
                        var value = invoker.Invoke(entity);
                        var seg = this.GetSqlSeg(value);
                        builder.AppendMember("t0", invoker.Member.Name);
                        builder.Append(" = ");
                        builder.Append(seg);
                        builder.Append(" AND ");
                    };
                }
                builder.Length -= 5;

                if (!useKey) throw new XFrameworkException("Delete<T>(T value) require T must have key column.");
            }
            else if (qDelete.SelectInfo != null)
            {
                TableAliasCache aliases = this.PrepareAlias<T>(qDelete.SelectInfo);
                var sc = new CommandDefinition.CommandBuilder(this, aliases);

                ExpressionVisitorBase visitor = new JoinExpressionVisitor(this, aliases, qDelete.SelectInfo.Join);
                visitor.Write(sc.JoinFragment);

                visitor = new WhereExpressionVisitor(this, aliases, qDelete.SelectInfo.Where);
                visitor.Write(sc.WhereFragment);
                sc.AddNavigation(visitor.Navigations);

                builder.Append(sc.Command);
            }

            return new CommandBase(builder.ToString(), null, System.Data.CommandType.Text); //builder.ToString();
        }

        // 创建 UPDATE 命令
        protected override CommandBase ParseUpdateCommand<T>(DbQueryableInfo_Update<T> qUpdate)
        {
            SqlBuilder builder = new SqlBuilder(this);
            var typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();

            builder.Append("UPDATE t0 SET");
            builder.AppendNewLine();

            if (qUpdate.Entity != null)
            {
                object entity = qUpdate.Entity;
                SqlBuilder whereBuilder = new SqlBuilder(this);
                bool useKey = false;
                int length = 0;

                foreach (var kv in typeRuntime.Invokers)
                {
                    MemberInvokerBase invoker = kv.Value;
                    var column = invoker.Column;
                    if (column != null && column.IsIdentity) goto gotoLabel; // fix issue# 自增列同时又是主键
                    if (column != null && column.NoMapped) continue;
                    if (invoker.ForeignKey != null) continue;
                    if (invoker.Member.MemberType == System.Reflection.MemberTypes.Method) continue;

                    builder.AppendMember("t0", invoker.Member.Name);
                    builder.Append(" = ");

                    gotoLabel:
                    var value = invoker.Invoke(entity);
                    var seg = this.GetSqlSeg(value);

                    if (column == null || !column.IsIdentity)
                    {
                        builder.Append(seg);
                        length = builder.Length;
                        builder.Append(',');
                        builder.AppendNewLine();
                    }

                    if (column != null && column.IsKey)
                    {
                        useKey = true;
                        whereBuilder.AppendMember("t0", invoker.Member.Name);
                        whereBuilder.Append(" = ");
                        whereBuilder.Append(seg);
                        whereBuilder.Append(" AND ");
                    }
                }

                if (!useKey) throw new XFrameworkException("Update<T>(T value) require T must have key column.");

                builder.Length = length;
                whereBuilder.Length -= 5;

                builder.AppendNewLine();
                builder.Append("FROM ");
                builder.AppendMember(typeRuntime.TableName);
                builder.Append(" t0");


                builder.AppendNewLine();
                builder.Append("WHERE ");
                builder.Append(whereBuilder);

            }
            else if (qUpdate.Expression != null)
            {
                TableAliasCache aliases = this.PrepareAlias<T>(qUpdate.SelectInfo);
                ExpressionVisitorBase visitor = null;
                visitor = new UpdateExpressionVisitor(this, aliases, qUpdate.Expression);
                visitor.Write(builder);

                builder.AppendNewLine();
                builder.Append("FROM ");
                builder.AppendMember(typeRuntime.TableName);
                builder.AppendAs("t0");

                var sc = new CommandDefinition.CommandBuilder(this, aliases);

                visitor = new JoinExpressionVisitor(this, aliases, qUpdate.SelectInfo.Join);
                visitor.Write(sc.JoinFragment);

                visitor = new WhereExpressionVisitor(this, aliases, qUpdate.SelectInfo.Where);
                visitor.Write(sc.WhereFragment);
                sc.AddNavigation(visitor.Navigations);

                builder.Append(sc.Command);
            }

            return new CommandBase(builder.ToString(), null, System.Data.CommandType.Text); //builder.ToString();
        }
    }
}
