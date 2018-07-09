using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// SELECT 命令定义
    /// </summary>
    public class CommandDefinition : CommandBase
    {
        private CommandBuilder _builder = null;

        /// <summary>
        /// 针对数据源运行的文本命令
        /// </summary>
        public override string CommandText { get { return _builder.Command; } }

        /// <summary>
        /// 选择字段范围
        /// </summary>
        /// <remarks>INSERT 表达式可能用这些字段</remarks>
        public IDictionary<string, Column> Columns { get; set; }

        /// <summary>
        /// 导航属性描述集合
        /// <para>
        /// 用于实体与 <see cref="IDataRecord"/> 做映射
        /// </para>
        /// </summary>
        public NavigationDescriptorCollection NavigationDescriptors { get; set; }

        /// <summary>
        /// JOIN（含） 之前的片断
        /// </summary>
        public SqlBuilder JoinFragment { get { return _builder.JoinFragment; } }

        /// <summary>
        /// Where 之后的片断
        /// </summary>
        public SqlBuilder WhereFragment { get { return _builder.WhereFragment; } }

        /// <summary>
        /// 实例化 <see cref="CommandDefinition"/> 类的新实例
        /// </summary>
        /// <param name="escCharLeft">如 [</param>
        /// <param name="escCharRight">如 ]</param>
        /// <param name="aliases">别名</param>
        public CommandDefinition(IDbQueryProvider provider, TableAliasCache aliases)
            : base(string.Empty, null, System.Data.CommandType.Text)
        {
            _builder = new CommandBuilder(provider, aliases);
        }

        /// <summary>
        /// 合并外键
        /// </summary>
        internal void AddNavigation(IDictionary<string, MemberExpression> navigations)
        {
            _builder.AddNavigation(navigations);
        }

        /// <summary>
        /// SQL命令构造器
        /// </summary>
        public class CommandBuilder
        {
            private string _command;
            private IDictionary<string, MemberExpression> _navigations = null;
            private string _escCharLeft;
            private string _escCharRight;
            private SqlBuilder _joinFragment = null;
            private SqlBuilder _whereFragment = null;
            private TableAliasCache _aliases = null;
            private IDbQueryProvider _provider = null;

            /// <summary>
            /// SQL 命令
            /// </summary>
            public string Command
            {
                get
                {
                    if (string.IsNullOrEmpty(_command))
                    {
                        this.AppendNavigation();
                        _joinFragment.Append(_whereFragment);
                    }

                    _command = _joinFragment.ToString();
                    return _command;
                }
            }

            /// <summary>
            /// JOIN（含） 之前的片断
            /// </summary>
            public SqlBuilder JoinFragment { get { return _joinFragment; } }

            /// <summary>
            /// Where 之后的片断
            /// </summary>
            public SqlBuilder WhereFragment { get { return _whereFragment; } }

            ///// <summary>
            ///// 附加外键
            ///// <para>
            ///// 用来构造额外的表达式没有显式指定的外键 Left Join 语句
            ///// </para>
            ///// </summary>
            //public IDictionary<string, MemberExpression> AdditionNavigations { get { return _navigations; } }

            /// <summary>
            /// 实例化 <see cref="CommandBuilder" /> 的新实例
            /// </summary>
            public CommandBuilder(IDbQueryProvider provider, TableAliasCache aliases)
            {
                _provider = provider;
                _escCharLeft = _provider.EscCharLeft;
                _escCharRight = _provider.EscCharRight;
                _aliases = aliases;
                _navigations = new Dictionary<string, MemberExpression>();

                _joinFragment = new SqlBuilder(_provider);
                _whereFragment = new SqlBuilder(_provider);
            }

            /// <summary>
            /// 合并外键
            /// </summary>
            public void AddNavigation(IDictionary<string, MemberExpression> navigations)
            {
                if (navigations != null && navigations.Count > 0)
                {
                    foreach (var kvp in navigations)
                    {
                        if (!_navigations.ContainsKey(kvp.Key)) _navigations.Add(kvp);
                    }
                }
            }

            // 添加导航属性关联
            protected virtual void AppendNavigation()
            {
                if (this._navigations == null || this._navigations.Count == 0) return;

                //开始产生LEFT JOIN 子句
                SqlBuilder builder = this.JoinFragment;
                foreach (var kvp in _navigations)
                {
                    string key = kvp.Key;
                    MemberExpression m = kvp.Value;
                    TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(m.Expression.Type);
                    ForeignKeyAttribute attribute = typeRuntime.GetWrapperAttribute<ForeignKeyAttribute>(m.Member.Name);

                    string innerKey = string.Empty;
                    string outerKey = key;
                    string innerAlias = string.Empty;

                    if (!m.Expression.IsArrivable())
                    {
                        innerKey = m.Expression.NodeType == ExpressionType.Parameter
                            ? (m.Expression as ParameterExpression).Name
                            : (m.Expression as MemberExpression).Member.Name;
                    }
                    else
                    {
                        MemberExpression mLeft = m.Expression as MemberExpression;
                        string name = TypeRuntimeInfoCache.GetRuntimeInfo(mLeft.Type).TableName;
                        innerAlias = _aliases.GetJoinTableAlias(name);

                        if (string.IsNullOrEmpty(innerAlias))
                        {
                            string keyLeft = mLeft.GetKeyWidthoutAnonymous();
                            if (_navigations.ContainsKey(keyLeft)) innerKey = keyLeft;
                        }
                    }

                    string alias1 = !string.IsNullOrEmpty(innerAlias) ? innerAlias : _aliases.GetTableAlias(innerKey);
                    string alias2 = _aliases.GetTableAlias(outerKey);


                    builder.AppendNewLine();
                    builder.Append("LEFT JOIN ");
                    Type type = m.Type;
                    if (type.IsGenericType) type = type.GetGenericArguments()[0];
                    builder.AppendMember(TypeRuntimeInfoCache.GetRuntimeInfo(type).TableName);
                    builder.Append(" ");
                    builder.Append(alias2);
                    builder.Append(" ON ");
                    for (int i = 0; i < attribute.InnerKeys.Length; i++)
                    {                        
                        builder.Append(alias1);
                        builder.Append('.');
                        builder.AppendMember(attribute.InnerKeys[i]);
                        builder.Append(" = ");
                        builder.Append(alias2);
                        builder.Append('.');
                        builder.AppendMember(attribute.OuterKeys[i]);

                        if (i < attribute.InnerKeys.Length - 1) builder.Append(" AND ");
                    }
                }
            }
        }
    }
}
