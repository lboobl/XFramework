
using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

using XFramework.Core;

namespace XFramework.DataAccess
{
    /// <summary>
    /// 查询条件构造器，可解析WHERE查询条件，也可解析存储过程参数
    /// </summary>
    internal class ConditionBuilder : ExpressionVisitor
    {

        private List<object> _lstArguments;                             //解析的参数值列表
        private Stack<string> _stcConditions;                           //解析的筛选条件
        private XDictionary<string, object> _parameters = null;         //解析的参数键值对
        private XDictionary<string, string> _parameterMembers = null;   //参数名和字段名对应表
        private string _parameterPrefix = "@";                          //参数的前缀

        /// <summary>
        /// 过滤条件 
        /// 文本脚本解析成 MemberName = @p1 AND MemberName = @p2，
        /// 存储过程解析成 MemberName = @MemberName AND MemberName1 = @MemberName1
        /// </summary>
        public string Condition { get; private set; }

        /// <summary>
        /// 参数键值对
        /// 文本脚本解析成 >@p1,值>
        /// 存储过程解析成 >MemberName,值>
        /// </summary>
        public XDictionary<string, object> Parameters { get; private set; }

        /// <summary>
        /// 参数名和字段名对应表
        /// 参数名@p0
        /// 字段名MemberName
        /// </summary>
        public XDictionary<string, string> ParameterMembers { get { return _parameterMembers; } }

        /// <summary>
        /// 脚本参数的前缀
        /// </summary>
        public string ParameterPrefix
        {
            get { return _parameterPrefix; }
        }

        public ConditionBuilder()
            :this("@")
        {
        }

        public ConditionBuilder(string parameterPrefix)
        {
            _parameterPrefix = parameterPrefix;
        }

        /// <summary>
        /// 解析脚本条件
        /// </summary>
        /// <param name="expression">条件表达式</param>
        public void Build(Expression expression)
        {
            this._lstArguments = new List<object>();
            this._stcConditions = new Stack<string>();
            this._parameters = new XDictionary<string, object>();
            this._parameterMembers = new XDictionary<string, string>();
            string condition = string.Empty;

            //计算常量
            PartialEvaluator evaluator = new PartialEvaluator();
            Expression evalExpr = evaluator.Eval(expression);
            //x=>true 表达式，转成 1==1
            if (evalExpr.NodeType == ExpressionType.Constant) evalExpr = VisitBoolean(evalExpr);
            //遍历整个树节点
            this.Visit(evalExpr);

            if (this._stcConditions.Count > 0)
            {
                condition = string.Format(" AND {0}", _stcConditions.Pop());
                MatchCollection matches = Regex.Matches(condition, string.Format(@"{0}(?<Name>p(?<Index>[0-9]+))", _parameterPrefix));
                foreach (Match match in matches)
                {
                    if (!match.Success) continue;

                    string index = match.Groups["Index"].Value;
                    string parameterName = match.Groups["Name"].Value;
                    if (_parameters[parameterName] == null) _parameters.Add(parameterName, _lstArguments[Convert.ToInt32(index)]);
                }
            }

            this.Condition = condition;
            this.Parameters = _parameters;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b == null) return b;

            string opr;
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    opr = "=";
                    break;
                case ExpressionType.NotEqual:
                    opr = "<>";
                    break;
                case ExpressionType.GreaterThan:
                    opr = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    opr = ">=";
                    break;
                case ExpressionType.LessThan:
                    opr = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    opr = "<=";
                    break;
                case ExpressionType.AndAlso:
                    opr = "AND";
                    break;
                case ExpressionType.OrElse:
                    opr = "OR";
                    break;
                case ExpressionType.Add:
                    opr = "+";
                    break;
                case ExpressionType.Subtract:
                    opr = "-";
                    break;
                case ExpressionType.Multiply:
                    opr = "*";
                    break;
                case ExpressionType.Divide:
                    opr = "/";
                    break;
                case ExpressionType.Coalesce:
                    opr = "IsNull";
                    break;
                default:
                    throw new NotSupportedException(b.NodeType + "is not supported.");
            }

            //1、将 And 或者 OR 左右两边的True常量解析成1==1
            //2、将 And 或者 OR 左右两边的Flase常量解析成1==2
            //3、将 And 或者 OR 左右两边的x.MemberName(Boolean)解析成x.MemberName == true
            //4、将 And 或者 OR 左右两边的x.!MemberName(Boolean)解析成x.MemberName == false
            Expression leftExpr = b.Left;
            Expression rightExpr = b.Right;
            if (b.NodeType == ExpressionType.AndAlso || b.NodeType == ExpressionType.OrElse)
            {
                leftExpr = VisitBoolean(b.Left);
                rightExpr = VisitBoolean(b.Right);
            }

            base.Visit(leftExpr);
            base.Visit(rightExpr);

            //组合条件
            string right = this._stcConditions.Pop();
            string left = this._stcConditions.Pop();
            //处理Null值
            if (b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual)
            {
                string strNull = b.NodeType == ExpressionType.Equal ? "IS NULL" : "IS NOT NULL";
                if (b.Left.NodeType == ExpressionType.Constant && ((ConstantExpression)b.Left).Value == null)
                {
                    opr = string.Empty;
                    left = strNull;
                }
                if (b.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)b.Right).Value == null)
                {

                    opr = string.Empty;
                    right = strNull;
                }
            }

            string condition = b.NodeType == ExpressionType.Coalesce ? 
                string.Format("({0}({1},{2}))", opr, left, right) : 
                string.Format("({0} {1} {2})", left, opr, right);
            this._stcConditions.Push(condition);

            //解析 MemberName operator @p0 参数名与字段名的映射关系
            if ((b.Left.NodeType == ExpressionType.MemberAccess && b.Right.NodeType == ExpressionType.Constant) ||
                (b.Right.NodeType == ExpressionType.MemberAccess && b.Left.NodeType == ExpressionType.Constant))
            {
                string memberName = CleanMemberName(b.Left.NodeType == ExpressionType.MemberAccess ? left : right);
                string parameterName = CleanParaName(b.Left.NodeType == ExpressionType.MemberAccess ? right : left);
                this._parameterMembers.Add(parameterName, memberName);
            }

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            this._lstArguments.Add(c.Value);
            string paraName = string.Format("{0}p{1}", _parameterPrefix, this._lstArguments.Count - 1);
            this._stcConditions.Push(paraName);

            return base.VisitConstant(c);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m == null) return m;

            //注：IEnumrable.Contains时 m.Object == null
            Type visitType = m.Object != null ? m.Object.Type : m.Arguments[0].Type;
            string condition = string.Empty;

            base.Visit(m.Object);
            base.VisitExpressionList(m.Arguments);

            string condition1 = string.Empty;
            string condition2 = string.Empty;
            string condition3 = string.Empty;

            string memberName = string.Empty;
            string parameterName1 = string.Empty;
            string parameterName2 = string.Empty;

            #region 字符

            if (visitType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case "Contains":
                        condition2 = this._stcConditions.Pop();
                        condition1 = this._stcConditions.Pop();
                        if (m.Object.NodeType == ExpressionType.MemberAccess && 
                            m.Arguments[0].NodeType == ExpressionType.MemberAccess)
                        {
                            //x=>x.MemberName1.Contains(x.MemberName2)
                            condition = string.Format("(CHARINDEX({0},{1}) > 0)", condition2, condition1);
                        }
                        else
                        {
                            //"X".Contains(x.MemberName1)或者x.MemberName.Contains("x")
                            memberName = CleanMemberName(m.Object.NodeType == ExpressionType.MemberAccess ? condition1 : condition2);
                            parameterName1 = CleanParaName(m.Object.NodeType == ExpressionType.MemberAccess ? condition2 : condition1);

                            this._lstArguments[this._lstArguments.Count - 1] = string.Format("%{0}%", this._lstArguments[this._lstArguments.Count - 1]);
                            condition = string.Format("({0} Like {1})", memberName, parameterName1);
                            this._parameterMembers.Add(parameterName1, memberName);
                        }

                        break;

                    case "StartWidth":
                        //"X".StartWidth(x.MemberName1)或者x.MemberName.StartWidth("x")
                        condition2 = this._stcConditions.Pop();
                        condition1 = this._stcConditions.Pop();
                        memberName = CleanMemberName(m.Object.NodeType == ExpressionType.MemberAccess ? condition1 : condition2);
                        parameterName1 = CleanParaName(m.Object.NodeType == ExpressionType.MemberAccess ? condition2 : condition1);

                        this._lstArguments[this._lstArguments.Count - 1] = string.Format("{0}%", this._lstArguments[this._lstArguments.Count - 1]);
                        condition = string.Format("({0} LIKE {1})", memberName, parameterName1);
                        this._parameterMembers.Add(parameterName1, memberName);

                        break;

                    case "EndWidth":
                        //"X".EndWidth(x.MemberName1)或者x.MemberName.EndWidth("x")
                        condition2 = this._stcConditions.Pop();
                        condition1 = this._stcConditions.Pop();
                        memberName = CleanMemberName(m.Object.NodeType == ExpressionType.MemberAccess ? condition1 : condition2);
                        parameterName1 = CleanParaName(m.Object.NodeType == ExpressionType.MemberAccess ? condition2 : condition1);
                        
                        this._lstArguments[this._lstArguments.Count - 1] = string.Format("{0}%", this._lstArguments[this._lstArguments.Count - 1]);
                        condition = string.Format("({0} LIKE {1})", memberName, parameterName1);
                        this._parameterMembers.Add(parameterName1, memberName);

                        break;

                    case "TrimStart":
                        condition2 = this._stcConditions.Pop();
                        condition1 = this._stcConditions.Pop();
                        condition = string.Format("(LTRIM({0}))", condition1);
                        break;

                    case "TrimEnd":
                        condition2 = this._stcConditions.Pop();
                        condition1 = this._stcConditions.Pop();
                        condition = string.Format("(RTRIM({0}))", condition1);
                        break;

                    case "Trim":
                        condition1 = this._stcConditions.Pop();
                        condition = string.Format("(RTRIM(LTRIM({0})))", condition1);
                        break;

                    case "Substring":
                        //(SUBSTRING(x.[BankName], 1 + 1, 2)
                        condition3 = this._stcConditions.Pop();
                        condition2 = this._stcConditions.Pop();
                        condition1 = this._stcConditions.Pop();
                        this._lstArguments[this._lstArguments.Count - 2] = Convert.ToInt32(this._lstArguments[this._lstArguments.Count - 2]) + 1;
                        condition = string.Format("(SUBSTRING({0},{1},{2}))", condition1, condition2, condition3);
                        break;

                    default:
                        throw new NotSupportedException("expression not surpport:" + m.ToString());
                }
            }

            #endregion

            #region 迭代

            else if (visitType.IsImplFrom(typeof(IEnumerable)))
            {
                //FileName In()
                condition2 = this._stcConditions.Pop();
                condition1 = this._stcConditions.Pop();

                IEnumerable value = this._lstArguments[this._lstArguments.Count - 1] as IEnumerable;
                IEnumerator enumerator = value.GetEnumerator();
                List<string> scope = new List<string>();
                while (enumerator.MoveNext())
                {
                    _lstArguments.Add(enumerator.Current);
                    scope.Add(string.Format("{0}p{1}", _parameterPrefix, _lstArguments.Count - 1));
                    _parameterMembers.Add(string.Format("p{0}", _lstArguments.Count - 1), CleanMemberName(condition2));
                }

                condition = string.Format("({0} IN ({1}))", condition2, string.Join(",", scope));
            }

            #endregion

            if (!string.IsNullOrEmpty(condition)) this._stcConditions.Push(condition);
            return m;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m == null) return m;

            if (m.Expression.NodeType != ExpressionType.Parameter)
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        //x.MemberName.Length
                        //LEN函数各个类型的数据库不尽相同，需区别对待，此处仅处理MSSQL
                        base.Visit(m.Expression);
                        string memberName = this._stcConditions.Pop();
                        memberName = string.Format("LEN({0})", memberName);
                        this._stcConditions.Push(memberName);

                        break;

                    default:
                        throw new NotSupportedException("expression not surpport:" + m.ToString());
                }
            }
            else
            {
                //x.MemberName
                string fieldName = this.WrapMemberName(m.Member.Name);
                this._stcConditions.Push(fieldName);
            }

            return m;
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            //解析访问布尔型字段 x=>x.Allowused
            if (lambda.Body.Type == typeof(bool))
            {
                Expression expr = this.VisitBoolean(lambda.Body);
                if (expr != lambda.Body) lambda = Expression.Lambda(expr, lambda.Parameters);
            }

            return base.VisitLambda(lambda);
        }

        //解析二元表达式左右两边的Boolean表达式
        private Expression VisitBoolean(Expression expr)
        {
            if (expr.Type != typeof(bool)) return expr;

            Expression leftExpr = null;
            Expression rightExpr = null;

            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                    //True常量解析成1==1 Flase常量解析成1==2
                    bool value = Convert.ToBoolean(((ConstantExpression)expr).Value);
                    leftExpr = Expression.Constant(1);
                    rightExpr = Expression.Constant(value ? 1 : 2);

                    break;

                case ExpressionType.MemberAccess:
                    //x.MemberName(Boolean)
                    leftExpr = expr;
                    rightExpr = Expression.Constant(true);

                    break;

                case ExpressionType.Not:
                    //!x.MemberName(Boolean)
                    if (((UnaryExpression)expr).Operand.NodeType == ExpressionType.MemberAccess)
                    {
                        leftExpr = ((UnaryExpression)expr).Operand;
                        rightExpr = Expression.Constant(false);
                    }

                    break;
            }

            if (leftExpr != null && rightExpr != null) expr = Expression.MakeBinary(ExpressionType.Equal, leftExpr, rightExpr);

            return expr;
        }

        //去掉参数前缀
        private string CleanParaName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                switch (name[0])
                {
                    case '@':
                    case ':':
                    case '?':
                        return name.Substring(1);
                }
            }

            return name;
        }

        //去掉字段的中括号
        private string CleanMemberName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Match match = Regex.Match(name, @"^\[(?<MemberName>[^\]]+)\]$");
                if (match.Success) name = match.Groups["MemberName"].Value;
            }

            return name;
        }

        //用中括号把字段包起来
        private string WrapMemberName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return string.Format("[{0}]", name);
            }

            return name;
        }
    }

    //计算表达式，如 5*3
    public class PartialEvaluator : ExpressionVisitor
    {
        private Func<Expression, bool> m_fnCanBeEvaluated;
        private HashSet<Expression> m_candidates;

        public PartialEvaluator()
            : this(CanBeEvaluatedLocally)
        { }

        public PartialEvaluator(Func<Expression, bool> fnCanBeEvaluated)
        {
            this.m_fnCanBeEvaluated = fnCanBeEvaluated;
        }

        public Expression Eval(Expression exp)
        {
            this.m_candidates = new Nominator(this.m_fnCanBeEvaluated).Nominate(exp);

            return this.Visit(exp);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            if (this.m_candidates.Contains(exp))
            {
                return this.Evaluate(exp);
            }

            return base.Visit(exp);
        }

        private Expression Evaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant)
            {
                return e;
            }

            LambdaExpression lambda = e is LambdaExpression ? Expression.Lambda(((LambdaExpression)e).Body) : Expression.Lambda(e);
            Delegate fn = lambda.Compile();

            return Expression.Constant(fn.DynamicInvoke(null), e is LambdaExpression ? ((LambdaExpression)e).Body.Type : e.Type);
        }

        private static bool CanBeEvaluatedLocally(Expression exp)
        {
            return exp.NodeType != ExpressionType.Parameter;
        }
    }

    //提取计算式，如 5 * 3 
    internal class Nominator : ExpressionVisitor
    {
        private Func<Expression, bool> m_fnCanBeEvaluated;
        private HashSet<Expression> m_candidates;
        private bool m_cannotBeEvaluated;

        internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
        {
            this.m_fnCanBeEvaluated = fnCanBeEvaluated;
        }

        internal HashSet<Expression> Nominate(Expression expression)
        {
            this.m_candidates = new HashSet<Expression>();
            this.Visit(expression);
            return this.m_candidates;
        }

        protected override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = this.m_cannotBeEvaluated;
                this.m_cannotBeEvaluated = false;
                //methodCall
                base.Visit(expression);

                if (!this.m_cannotBeEvaluated)
                {
                    if (this.m_fnCanBeEvaluated(expression))
                    {
                        this.m_candidates.Add(expression);
                    }
                    else
                    {
                        this.m_cannotBeEvaluated = true;
                    }
                }

                this.m_cannotBeEvaluated |= saveCannotBeEvaluated;
            }

            return expression;
        }
    }

    //表达式树访问器
    public abstract class ExpressionVisitor
    {
        protected ExpressionVisitor() { }

        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = this.Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            Expression conversion = this.Visit(b.Conversion);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = this.Visit(b.Expression);
            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = this.Visit(c.Test);
            Expression ifTrue = this.Visit(c.IfTrue);
            Expression ifFalse = this.Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = this.Visit(m.Expression);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = this.Visit(m.Object);
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }
            return m;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = this.Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }
            return original;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = this.Visit(assignment.Expression);
            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = this.VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = this.VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {
                if (nex.Members != null)
                    return Expression.New(nex.Constructor, args, nex.Members);
                else
                    return Expression.New(nex.Constructor, args);
            }
            return nex;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }

        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
                }
            }
            return na;
        }

        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
            Expression expr = this.Visit(iv.Expression);
            if (expr is ConstantExpression) return expr;

            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }
            return iv;
        }
    }
}
