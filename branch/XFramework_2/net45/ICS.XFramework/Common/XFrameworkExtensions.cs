using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ICS.XFramework
{
    public static class XfwExtensions
    {
        #region 表达式树

        /// <summary>
        /// 返回真表达式
        /// </summary>
        public static Expression<Func<T, bool>> True<T>()
            where T : class
        {
            return f => true;
        }

        /// <summary>
        /// 返回假表达式
        /// </summary>
        public static Expression<Func<T, bool>> False<T>()
            where T : class
        {
            return f => false;
        }

        /// <summary>
        /// 拼接真表达式
        /// </summary>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> TExp1,
            Expression<Func<T, bool>> TExp2) where T : class
        {
            if (TExp1 == null) return TExp2;
            if (TExp2 == null) return TExp1;

            var invokeExp = System.Linq.Expressions.Expression.Invoke(TExp2, TExp1.Parameters.Cast<System.Linq.Expressions.Expression>());
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>
                  (System.Linq.Expressions.Expression.AndAlso(TExp1.Body, invokeExp), TExp1.Parameters);
        }

        /// <summary>
        /// 拼接假表达式
        /// </summary>
        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> TExp1,
            Expression<Func<T, bool>> TExp2) where T : class
        {
            if (TExp1 == null) return TExp2;
            if (TExp2 == null) return TExp1;

            var invokeExp = System.Linq.Expressions.Expression.Invoke(TExp2, TExp1.Parameters.Cast<System.Linq.Expressions.Expression>());
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>
                  (System.Linq.Expressions.Expression.OrElse(TExp1.Body, invokeExp), TExp1.Parameters);
        }

        /// <summary>
        /// reduce unaryExpression
        /// </summary>
        /// <returns></returns>
        public static Expression ReduceUnary(this Expression exp)
        {
            UnaryExpression unaryExpression = exp as UnaryExpression;
            return unaryExpression != null
                ? unaryExpression.Operand.ReduceUnary()
                : exp;
        }

        /// <summary>
        /// 判断表达式链是否能通过动态计算，计算出它的值
        /// </summary>
        public static bool CanEvaluate(this Expression node)
        {
            // => 5
            // => a.ActiveDate == DateTime.Now
            // => a.State == (byte)state

            if (node == null) return false;
            if (node.NodeType == ExpressionType.Constant) return true;
            if (node.NodeType == ExpressionType.ArrayIndex) return true;
            if (node.NodeType == ExpressionType.Call)
            {
                // List<int>{0}[]
                var call = node as MethodCallExpression;
                if (call.Object != null && call.Method.Name == "get_Item" && call.Object.Type.Name == "List`1")
                {
                    return true;
                }
            }

            var member = node as MemberExpression;
            if (member == null) return false;
            if (member.Expression == null) return true;
            if (member.Expression.NodeType == ExpressionType.Constant) return true;
            return member.Expression.CanEvaluate();
        }

        /// <summary>
        /// 计算表达式的值
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static ConstantExpression Evaluate(this Expression e)
        {
            ConstantExpression node = null;
            if (e.NodeType == ExpressionType.Constant)
            {
                node = e as ConstantExpression;
            }
            else
            {
                LambdaExpression lambda = e is LambdaExpression ? Expression.Lambda(((LambdaExpression)e).Body) : Expression.Lambda(e);
                Delegate fn = lambda.Compile();

                node = Expression.Constant(fn.DynamicInvoke(null), e is LambdaExpression ? ((LambdaExpression)e).Body.Type : e.Type);
            }

            // 枚举要转成 INT
            if (node.Type.IsEnum) node = Expression.Constant(Convert.ToInt32(node.Value));

            // 返回最终处理的常量表达式s
            return node;
        }

        #endregion

        #region 列表扩展

        /// <summary>
        /// 取指定列表中符合条件的元素索引
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            int i = -1;
            foreach (T value in collection)
            {
                i++;
                if (predicate(value)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 创建一个集合
        /// </summary>
        public static List<TResult> ToList<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> selector)
        {
            return collection.Select(selector).ToList();
        }

        /// <summary>
        /// 根据页长计算总页码
        /// </summary>
        /// <param name="collection">数据集合</param>
        /// <param name="pageSize">页码</param>
        /// <returns></returns>
        public static int Page<T>(this IEnumerable<T> collection, int pageSize)
        {
            int count = 0;
            if ((collection as ICollection<T>) != null) count = (collection as ICollection<T>).Count;
            else if ((collection as T[]) != null) count = (collection as T[]).Length;
            else count = collection.Count();

            int page = count % pageSize == 0 ? count / pageSize : (count / pageSize + 1);
            return page;
        }

        #endregion
    }
}
