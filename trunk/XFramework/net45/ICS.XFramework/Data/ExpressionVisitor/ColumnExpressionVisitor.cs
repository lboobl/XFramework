﻿using System;
using System.Data;
using System.Linq.Expressions;
using System.Collections.Generic;
using TypeUtils = ICS.XFramework.Reflection.TypeUtils;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 选择列表达式解析器
    /// </summary>
    public class ColumnExpressionVisitor : ExpressionVisitorBase
    {
        // TODO 所有LEFT JOIN都应检查是否为空 ???
        // 若是，会否跟 Client = new Inte_CRM.Client(a.Client) 这种表达式表达的语义有冲突

        private DbQueryProviderBase _provider = null;
        private TableAliasCache _aliases = null;
        private DbExpression _groupBy = null;
        private List<DbExpression> _include = null;
        private static IDictionary<DbExpressionType, string> _statisMethods = null;

        private IDictionary<string, Column> _columns = null;
        private NavigationDescriptorCollection _navDescriptors = null;
        private List<string> _navChainHopper = null;
        private bool _mOnly = false;

        public static string NullFieldName = "NULL";

        static ColumnExpressionVisitor()
        {
            _statisMethods = new Dictionary<DbExpressionType, string>
            {
                { DbExpressionType.Count,"COUNT" },
                { DbExpressionType.Max,"MAX" },
                { DbExpressionType.Min,"MIN" },
                { DbExpressionType.Average,"AVG" },
                { DbExpressionType.Sum,"SUM" }
            };
        }

        /// <summary>
        /// 初始化 <see cref="ColumnExpressionVisitor"/> 类的新实例
        /// </summary>
        public ColumnExpressionVisitor(DbQueryProviderBase provider, TableAliasCache aliases, DbExpression exp, DbExpression groupBy = null, bool mOnly = false, IDictionary<string, Column> columns = null, List<DbExpression> include = null)
            : base(provider, aliases, exp.Expressions != null ? exp.Expressions[0] : null)
        {
            _provider = provider;
            _aliases = aliases;
            _groupBy = groupBy;
            _include = include;
            _mOnly = mOnly;

            _columns = columns;
            if (_columns == null) _columns = new Dictionary<string, Column>();
            _navDescriptors = new NavigationDescriptorCollection();
            _navChainHopper = new List<string>(10);
        }

        /// <summary>
        /// 将表达式所表示的SQL片断写入SQL构造器
        /// </summary>
        public override void Write(SqlBuilder builder)
        {
            if (base.Expression != null)
            {
                base._builder = builder;
                _builder.AppendNewLine();

                // SELECT 表达式解析
                if (base.Expression.NodeType == ExpressionType.Constant)
                {
                    // if have no select syntax
                    Type type = (base.Expression as ConstantExpression).Value as Type;
                    this.VisitAllMember(type, "t0");
                }
                else
                {
                    base.Write(builder);
                }

                // Include 表达式解析<导航属性>
                this.VisitInclude();

                // 去掉最后的空格和回车
                if (_builder[_builder.Length - 1].ToString() != _provider.EscCharRight)
                {
                    int space = Environment.NewLine.Length + 1;
                    int index = _builder.Length - 1;
                    while (_builder[index] == ' ')
                    {
                        space++;
                        index--;
                    }
                    _builder.Length -= space;
                }
            }
        }

        /// <summary>
        /// SELECT 字段
        /// Column 对应实体的原始属性
        /// </summary>
        public IDictionary<string, Column> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// 导航属性描述信息
        /// <para>
        /// 从 <see cref="IDataReader"/> 到实体的映射需要使用这些信息来给导航属性赋值
        /// </para>
        /// </summary>
        public NavigationDescriptorCollection NavigationDescriptors
        {
            get { return _navDescriptors; }
        }

        //p=>p
        //p=>p.t
        //p=>p.Id
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            LambdaExpression lambda = node as LambdaExpression;
            if (lambda.Body.NodeType == ExpressionType.Parameter)
            {
                // 例： a=> a
                Type type = lambda.Body.Type;
                string alias = _aliases.GetTableAlias(lambda);
                this.VisitAllMember(type, alias);
                return node;
            }

            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                // 例： t=> t.a
                // => SELECT a.ClientId
                Type type = lambda.Body.Type;
                return TypeUtils.IsPrimitive(type)
                    ? base.VisitLambda(node)
                    : this.VisitAllMember(type, _aliases.GetTableAlias(lambda.Body), node);
            }

            if (_mOnly) _mOnly = false;
            return base.VisitLambda(node);
        }

        //{new App() {Id = p.Id}} 
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (node.NewExpression != null) this.VisitMemberNew(node.NewExpression);
            if (_navChainHopper.Count == 0) _navChainHopper.Add(node.Type.Name);

            // TODO #对 Bindings 进行排序，保证导航属性的赋值一定要最后面#
            // 未实现，在书写表达式时人工保证 ##

            for (int i = 0; i < node.Bindings.Count; i++)
            {
                MemberAssignment binding = node.Bindings[i] as MemberAssignment;
                if (binding == null) throw new XFrameworkException("Only 'MemberAssignment' binding supported.");

                Type propertyType = (node.Bindings[i].Member as System.Reflection.PropertyInfo).PropertyType;
                bool isNavigation = !TypeUtils.IsPrimitive(propertyType);

                #region 一般属性

                // 非导航属性
                if (!isNavigation)
                {
                    base.VisitMemberBinding(node.Bindings[i]);

                    // 选择字段
                    string newName = ColumnExpressionVisitor.AddColumn(_columns, binding.Member.Name);
                    // 添加字段别名
                    _builder.AppendAs(newName);
                    _builder.Append(',');
                    _builder.AppendNewLine();

                }

                #endregion

                #region 导航属性

                else
                {
                    TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(binding.Member.DeclaringType);
                    var attribute = typeRuntime.GetWrapperAttribute<ForeignKeyAttribute>(binding.Member.Name);
                    if (attribute == null) throw new XFrameworkException("Complex property must mark 'ForeignKeyAttribute' ");

                    int n = _navChainHopper.Count;

                    // 生成导航属性描述集合，以类名.属性名做为键值
                    string keyName = _navChainHopper.Count > 0 ? _navChainHopper[_navChainHopper.Count - 1] : string.Empty;
                    keyName = !string.IsNullOrEmpty(keyName) ? keyName + "." + binding.Member.Name : binding.Member.Name;
                    NavigationDescriptor descriptor = new NavigationDescriptor(keyName, binding.Member);

                    if (!_navDescriptors.ContainsKey(keyName))
                    {
                        // fix issue# XC 列占一个位
                        descriptor.Start = _columns.Count;
                        descriptor.FieldCount = GetFieldCount(binding.Expression) + (binding.Expression.NodeType == ExpressionType.MemberAccess || binding.Expression.NodeType == ExpressionType.MemberInit ? 1 : 0);
                        _navDescriptors.Add(keyName, descriptor);
                        _navChainHopper.Add(keyName);
                    }

                    if (binding.Expression.NodeType == ExpressionType.MemberAccess) this.VisitMemberNavigation(binding.Expression as MemberExpression, true);
                    else if (binding.Expression.NodeType == ExpressionType.New) this.VisitMemberNew(binding.Expression as NewExpression);
                    else if (binding.Expression.NodeType == ExpressionType.MemberInit) this.VisitMemberInit(binding.Expression as MemberInitExpression);

                    // 恢复访问链
                    // 在访问导航属性时可能是 Client.CloudServer，这时要恢复为 Client，以保证能访问 Client 的下一个导航属性
                    if (_navChainHopper.Count != n) _navChainHopper.RemoveAt(_navChainHopper.Count - 1);
                }

                #endregion
            }

            return node;
        }

        private Expression VisitMemberNavigation(MemberExpression node, bool checkNull = false)
        {
            string alias = string.Empty;
            Type type = node.Type;

            if (node.IsArrivable())
            {
                // 例： Client = a.Client.CloudServer
                // fix issue# Join 表达式显式指定导航属性时时，alias 为空
                // fix issue# 多个导航属性时 AppendNullColumn 只解析当前表达式的
                int index = 0;
                int num = this.Navigations != null ? this.Navigations.Count : 0;
                alias = this.VisitNavigation(node);
                foreach (var kvp in Navigations)
                {
                    index += 1;
                    if (index < Navigations.Count && index > num)
                    {
                        alias = _aliases.GetNavigationTableAlias(kvp.Key);
                        if (checkNull) AppendNullColumn(kvp.Value.Member, alias);
                        continue;
                    }

                    alias = _aliases.GetNavigationTableAlias(kvp.Key);
                    type = kvp.Value.Type;
                }
            }
            else
            {
                // 例： Client = b
                alias = _aliases.GetTableAlias(node);
                type = node.Type;
            }


            if (type.IsGenericType) type = type.GetGenericArguments()[0];
            this.VisitAllMember(type, alias);
            if (checkNull) AppendNullColumn(node.Member, alias);

            return node;
        }

        //{new  {Id = p.Id}} 
        protected override Expression VisitNew(NewExpression node)
        {
            // TODO 未支持匿名类的导航属性
            if (node != null)
            {
                if (node.Arguments.Count == 0) throw new XFrameworkException("'NewExpression' do not have any arguments.");
                this.VisitMemberNew(node);
            }

            return node;
        }

        //遍历New表达式的参数集
        private Expression VisitMemberNew(NewExpression node)
        {
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                Expression exp = node.Arguments[i];
                Type pType = exp.Type;

                if (exp.NodeType == ExpressionType.Parameter)
                {
                    //例： new Client(a)
                    Type type = exp.Type;
                    string alias = _aliases.GetTableAlias(exp);
                    this.VisitAllMember(type, alias);
                    continue;
                }

                if (exp.NodeType == ExpressionType.MemberAccess || exp.NodeType == ExpressionType.Call)
                {
                    if (TypeUtils.IsPrimitive(pType))
                    {
                        // new Client(a.ClientId)
                        this.Visit(exp);
                        // 选择字段
                        string newName = ColumnExpressionVisitor.AddColumn(_columns, node.Members != null ? node.Members[i].Name : (exp as MemberExpression).Member.Name);
                        // 添加字段别名
                        _builder.AppendAs(newName);
                        _builder.Append(',');
                        _builder.AppendNewLine();
                    }
                    else
                    {
                        this.VisitMemberNavigation(exp as MemberExpression, true);
                    }

                    continue;
                }

                if (exp.CanEvaluate())
                {
                    //例： new Client(a)
                    this.Visit(exp);
                    // 选择字段
                    string newName = ColumnExpressionVisitor.AddColumn(_columns, node.Members != null ? node.Members[i].Name : (exp as MemberExpression).Member.Name);
                    // 添加字段别名
                    _builder.AppendAs(newName);
                    _builder.Append(',');
                    _builder.AppendNewLine();

                    continue;
                }

                throw new XFrameworkException("NodeType '{0}' not supported.", exp.NodeType);
            }

            return node;
        }

        // g.Key.CompanyName & g.Max(a)
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null) return node;

            // Group By 解析
            if (_groupBy != null && node.IsGrouping())
            {
                // CompanyName = g.Key.Name
                LambdaExpression keySelector = _groupBy.Expressions[0] as LambdaExpression;
                Expression exp = null;
                Expression body = keySelector.Body;


                if (body.NodeType == ExpressionType.MemberAccess)
                {
                    // group xx by a.CompanyName
                    exp = body;

                    //
                    //
                    //
                    //
                }
                else if (body.NodeType == ExpressionType.New)
                {
                    // group xx by new { Name = a.CompanyName  }

                    string memberName = node.Member.Name;
                    NewExpression newExp = body as NewExpression;
                    int index = newExp.Members.IndexOf(x => x.Name == memberName);
                    exp = newExp.Arguments[index];
                }

                return this.Visit(exp);
            }

            // 分组后再分页，子查询不能有OrderBy子句，此时要将OrderBy字段添加到选择字段中，抛给外层查询使用
            var newNode = base.VisitMember(node);
            if (_mOnly)
            {
                string newName = ColumnExpressionVisitor.AddColumn(_columns, node.Member.Name);
                _builder.AppendAs(newName);
            }
            return newNode;
        }

        // g.Max(a=>a.Level)
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (_groupBy != null && node.IsGrouping())
            {
                DbExpressionType dbExpressionType = DbExpressionType.None;
                Enum.TryParse(node.Method.Name, out dbExpressionType);
                Expression exp = dbExpressionType == DbExpressionType.Count
                    ? Expression.Constant(1)
                    : (node.Arguments.Count == 1 ? null : node.Arguments[1]);
                if (exp.NodeType == ExpressionType.Lambda) exp = (exp as LambdaExpression).Body;

                // 如果是 a=> a 这种表达式，那么一定会指定 elementSelector
                if (exp.NodeType == ExpressionType.Parameter) exp = _groupBy.Expressions[1];

                _builder.Append(_statisMethods[dbExpressionType]);
                _builder.Append("(");
                this.Visit(exp);
                _builder.Append(")");

                return node;
            }

            return base.VisitMethodCall(node);
        }

        // 选择所有的字段
        private Expression VisitAllMember(Type type, string alias, Expression node = null)
        {
            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(type);
            Dictionary<string, Reflection.MemberAccessWrapper> wrappers = typeRuntime.Wrappers;

            //Fixed issue# 匿名类的字段不能Set
            //runtimeInfo.IsAnonymousType
            //? type.GetProperties().ToDictionary(p => p.Name, p => new ICS.XFramework.Reflection.MemberAccessWrapper(p))
            //: runtimeInfo.Wrappers;

            foreach (var w in wrappers)
            {
                var wrapper = w.Value;// as ICS.XFramework.Reflection.MemberAccessWrapper;
                var mapper = wrapper as MemberAccessWrapper;
                if (mapper != null && mapper.Column != null && mapper.Column.NoMapped) continue;
                if (mapper != null && mapper.ForeignKey != null) continue; // 不加载导航属性
                if (wrapper.Member.MemberType == System.Reflection.MemberTypes.Method) continue;

                _builder.AppendMember(alias, wrapper.Member.Name);

                // 选择字段
                string newName = ColumnExpressionVisitor.AddColumn(_columns, wrapper.Member.Name);
                // 添加字段别名
                _builder.AppendAs(newName);
                _builder.Append(",");
                _builder.AppendNewLine();
            }

            return node;
        }

        // 遍历 Include 包含的导航属性
        private void VisitInclude()
        {
            if (_include == null || _include.Count == 0) return;

            foreach (var dbExpression in _include)
            {
                Expression exp = dbExpression.Expressions[0];
                if (exp == null) continue;

                if (exp.NodeType == ExpressionType.Lambda) exp = (exp as LambdaExpression).Body;
                MemberExpression memberExpression = exp as MemberExpression;
                if (memberExpression == null) throw new XFrameworkException("Include expression body must be MemberExpression.");

                // 解析导航属性链
                List<Expression> chain = new List<Expression>();
                while (memberExpression != null)
                {
                    // a.Client 要求 <Client> 必须标明 ForeignKeyAttribute
                    TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(memberExpression.Expression.Type);
                    ForeignKeyAttribute attribute = typeRuntime.GetWrapperAttribute<ForeignKeyAttribute>(memberExpression.Member.Name);
                    if (attribute == null) throw new XFrameworkException("Include member {{{0}}} must mark ForeignKeyAttribute.", memberExpression);

                    chain.Add(memberExpression);
                    var m = memberExpression.Expression as MemberExpression;
                    if (m == null) chain.Add(memberExpression.Expression);
                    memberExpression = m;
                }

                // 生成导航属性描述信息
                string keyName = string.Empty;
                for (int i = chain.Count - 1; i >= 0; i--)
                {
                    Expression expression = chain[i];
                    memberExpression = expression as MemberExpression;
                    if (memberExpression == null)
                    {
                        keyName = expression.Type.Name;
                        continue;
                    }

                    keyName = keyName + "." + memberExpression.Member.Name;
                    if (!_navDescriptors.ContainsKey(keyName))
                    {
                        // fix issue# XC 列占一个位
                        NavigationDescriptor descriptor = new NavigationDescriptor(keyName, memberExpression.Member);
                        descriptor.Start = _columns.Count; //i == 0 ? _columns.Count : -1;
                        descriptor.FieldCount = i == 0 ? (GetFieldCount(exp) + 1) : 1;//-1;
                        _navDescriptors.Add(keyName, descriptor);
                    }
                }

                this.VisitMemberNavigation(exp as MemberExpression, true);
            }
        }

        // 添加额外列，用来判断整个（左）连接记录是否为空
        private void AppendNullColumn(System.Reflection.MemberInfo member, string alias)
        {
            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(member.DeclaringType);
            var foreignKey = typeRuntime.GetWrapperAttribute<ForeignKeyAttribute>(member.Name);
            string keyName = foreignKey.OuterKeys[0];

            _builder.Append("CASE WHEN ");
            _builder.AppendMember(alias, keyName);
            _builder.Append(" IS NULL THEN NULL ELSE ");
            _builder.AppendMember(alias, keyName);
            _builder.Append(" END");

            // 选择字段
            string newName = ColumnExpressionVisitor.AddColumn(_columns, NullFieldName);
            // 添加字段别名
            _builder.AppendAs(newName);
            _builder.Append(',');
            _builder.AppendNewLine();
        }

        // 选择字段
        private static string AddColumn(IDictionary<string, Column> columns, string name)
        {
            // ATTENTION：此方法不能在 VisitMember 方法里调用
            // 因为 VisitMember 方法不一定是最后SELECT的字段
            // 返回最终确定的唯一的列名

            string newName = name;
            int dup = 0;
            if (columns.ContainsKey(newName))
            {
                var column = columns[newName];
                column.Duplicate += 1;

                newName = newName + column.Duplicate.ToString(); //string.Format("{0}{1}", newName, column.Dup.ToString());
                dup = column.Duplicate;
            }

            columns.Add(newName, new Column { Name = name, Duplicate = dup });
            return newName;
        }

        // 计算数据库字段数量 
        private static int GetFieldCount(Expression node)
        {
            int num = 0;

            switch (node.NodeType)
            {
                case ExpressionType.MemberInit:
                    MemberInitExpression m = node as MemberInitExpression;
                    foreach (var exp in m.NewExpression.Arguments) num += _typeFieldAggregator(exp);
                    foreach (MemberAssignment ma in m.Bindings) num += _primitiveAggregator((ma.Member as System.Reflection.PropertyInfo).PropertyType);

                    break;

                case ExpressionType.MemberAccess:
                    MemberExpression m1 = node as MemberExpression;
                    num += _typeFieldAggregator(m1);

                    break;

                case ExpressionType.New:
                    NewExpression m2 = node as NewExpression;
                    foreach (var exp in m2.Arguments) num += _typeFieldAggregator(exp);
                    if (m2.Members != null) foreach (var member in m2.Members) num += _primitiveAggregator((member as System.Reflection.PropertyInfo).PropertyType);

                    break;
            }

            return num;
        }

        static Func<Expression, int> _typeFieldAggregator = exp =>
            exp.NodeType == ExpressionType.MemberAccess && TypeUtils.IsPrimitive(exp.Type) ? 1 : TypeRuntimeInfoCache.GetRuntimeInfo(exp.Type.IsGenericType ? exp.Type.GetGenericArguments()[0] : exp.Type).FieldCount;
        static Func<Type, int> _primitiveAggregator = type => TypeUtils.IsPrimitive(type) ? 1 : 0;
    }
}
