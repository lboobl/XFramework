using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ICS.XFramework.Data
{
    internal sealed class DbQueryParser
    {
        public static IDbQueryableInfo<TElement> Parse<TElement>(IDbQueryable<TElement> source, int start = 0)
        {
            // 目的：将query 转换成增/删/改/查
            // 1、from a in context.GetTable<T>() select a 此时query里面可能没有SELECT 表达式
            // 2、Take 视为一个查询的结束位，如有更多查询，应使用嵌套查询

            Type type = null;
            bool isDistinct = false;
            bool isAny = false;
            int? skip = null;
            int? take = null;
            int? outerIndex = null;
            List<Expression> where = new List<Expression>();                  // WHERE
            List<Expression> having = new List<Expression>();                 // HAVING
            List<DbExpression> join = new List<DbExpression>();               // JOIN
            List<DbExpression> orderBy = new List<DbExpression>();            // ORDER BY
            List<DbExpression> include = new List<DbExpression>();            // ORDER BY
            List<IDbQueryableInfo<TElement>> union = new List<IDbQueryableInfo<TElement>>();

            Expression select = null;       // SELECT #
            DbExpression insert = null;     // INSERT #
            DbExpression update = null;     // UPDATE #
            DbExpression delete = null;     // DELETE #
            DbExpression groupBy = null;    // GROUP BY #
            DbExpression statis = null;     // SUM/MAX  #

            for (int index = start; index < source.DbExpressions.Count; ++index)
            {
                DbExpression curExp = source.DbExpressions[index];

                // Take(n)
                if (take != null)
                {
                    outerIndex = index;
                    break;
                }

                if (skip != null && curExp.DbExpressionType != DbExpressionType.Take)
                {
                    outerIndex = index;
                    break;
                }

                switch (curExp.DbExpressionType)
                {
                    case DbExpressionType.None:
                    case DbExpressionType.All:
                        continue;

                    case DbExpressionType.Any:
                        isAny = true;
                        if (curExp.Expressions != null) where.Add(curExp.Expressions[0]);
                        break;

                    case DbExpressionType.Union:
                        var uQuery = (curExp.Expressions[0] as ConstantExpression).Value as IDbQueryable<TElement>;
                        var u = DbQueryParser.Parse(uQuery);
                        union.Add(u);
                        continue;
                    case DbExpressionType.Include:
                        include.Add(curExp);
                        continue;

                    case DbExpressionType.GroupBy:
                        groupBy = curExp;
                        continue;

                    case DbExpressionType.GetTable:
                        type = (curExp.Expressions[0] as ConstantExpression).Value as Type;
                        continue;

                    case DbExpressionType.Average:
                    case DbExpressionType.Min:
                    case DbExpressionType.Sum:
                    case DbExpressionType.Max:
                        statis = curExp;
                        continue;

                    case DbExpressionType.Count:
                        statis = curExp;
                        if (curExp.Expressions != null) where.Add(curExp.Expressions[0]);
                        continue;

                    case DbExpressionType.Distinct:
                        isDistinct = true;
                        continue;

                    case DbExpressionType.First:
                    case DbExpressionType.FirstOrDefault:
                        take = 1;
                        if (curExp.Expressions != null) where.Add(curExp.Expressions[0]);
                        continue;

                    case DbExpressionType.GroupJoin:
                    case DbExpressionType.Join:
                        select = curExp.Expressions[3];
                        join.Add(curExp);
                        continue;

                    case DbExpressionType.OrderBy:
                    case DbExpressionType.OrderByDescending:
                        orderBy.Add(curExp);
                        continue;
                    case DbExpressionType.Select:
                        select = curExp.Expressions[0];
                        continue;

                    case DbExpressionType.SelectMany:
                        select = curExp.Expressions[1];
                        if (!curExp.Expressions[0].IsAnonymous()) join.Add(curExp);
                        continue;

                    case DbExpressionType.Single:
                    case DbExpressionType.SingleOrDefault:
                        take = 1;
                        if (curExp.Expressions != null) where.Add(curExp.Expressions[0]);
                        continue;

                    case DbExpressionType.Skip:
                        skip = (int)(curExp.Expressions[0] as ConstantExpression).Value;
                        continue;

                    case DbExpressionType.Take:
                        take = (int)(curExp.Expressions[0] as ConstantExpression).Value;
                        continue;

                    case DbExpressionType.ThenBy:
                    case DbExpressionType.ThenByDescending:
                        orderBy.Add(curExp);
                        continue;

                    case DbExpressionType.Where:
                        var predicate = groupBy == null ? where : having;
                        if (curExp.Expressions != null) predicate.Add(curExp.Expressions[0]);
                        continue;

                    case DbExpressionType.Insert:
                        insert = curExp;
                        continue;

                    case DbExpressionType.Update:
                        update = curExp;
                        continue;

                    case DbExpressionType.Delete:
                        delete = curExp;
                        continue;

                    default:
                        throw new NotSupportedException(string.Format("{0} is not support.", curExp.DbExpressionType));
                }
            }

            // 没有解析到INSERT/DELETE/UPDATE/SELECT表达式，并且没有相关统计函数，则默认选择FromType的所有字段
            bool useAllColumn = insert == null && delete == null && update == null && select == null && statis == null;
            if (useAllColumn) select = Expression.Constant(type ?? typeof(TElement));

            var qQuery = new DbQueryableInfo_Select<TElement>();
            qQuery.FromType = type;
            qQuery.Select = new DbExpression(DbExpressionType.Select, select);
            qQuery.HaveDistinct = isDistinct;
            qQuery.HaveAny = isAny;
            qQuery.Join = join;
            qQuery.OrderBy = orderBy;
            qQuery.GroupBy = groupBy;
            qQuery.Skip = skip != null ? skip.Value : 0;
            qQuery.Take = take != null ? take.Value : 0;
            qQuery.Where = new DbExpression(DbExpressionType.Where, DbQueryParser.CombineWhere(where));
            qQuery.Having = new DbExpression(DbExpressionType.None, DbQueryParser.CombineWhere(having));
            qQuery.Statis = statis;
            qQuery.Union = union;
            qQuery.Include = include;

            if (update != null)
            {
                var qUpdate = new DbQueryableInfo_Update<TElement>();
                ConstantExpression expression2 = update.Expressions != null ? update.Expressions[0] as ConstantExpression : null;
                if (expression2 != null)
                    qUpdate.Entity = expression2.Value;
                else
                    qUpdate.Expression = update.Expressions[0];
                qUpdate.SelectInfo = qQuery;
                return qUpdate;
            }
            if (delete != null)
            {
                var qDelete = new DbQueryableInfo_Delete<TElement>();
                ConstantExpression expression2 = delete.Expressions != null ? delete.Expressions[0] as ConstantExpression : null;
                if (expression2 != null)
                    qDelete.Entity = expression2.Value;
                qDelete.SelectInfo = qQuery;
                return qDelete;
            }

            if (insert != null)
            {
                var qInsert = new DbQueryableInfo_Insert<TElement>();
                if (insert.Expressions != null) qInsert.Entity = (insert.Expressions[0] as ConstantExpression).Value;
                qInsert.SelectInfo = qQuery;
                source.DbQueryInfo = qInsert;
                qInsert.Bulk = source.Bulk;
                return qInsert;
            }

            // 如果有一对多的导航关系，则产生嵌套语义的查询
            if (select != null) qQuery = DbQueryParser.TryBuilOuter(qQuery);

            if (outerIndex != null)
            {
                // 解析嵌套查询
                var qOuter = DbQueryParser.Parse<TElement>(source, outerIndex.Value);
                var qInsert = qOuter as DbQueryableInfo_Insert<TElement>;
                if (qInsert != null)
                {
                    if (insert != null && insert.Expressions != null) qInsert.Entity = (insert.Expressions[0] as ConstantExpression).Value;
                    qInsert.SelectInfo = qQuery;
                    source.DbQueryInfo = qInsert;
                    qInsert.Bulk = source.Bulk;
                    return qInsert;
                }
                else
                {
                    qOuter.InnerQuery = qQuery;
                    return qOuter;
                }
            }

            // 查询表达式
            return qQuery;
        }

        // 构造由一对多关系产生的嵌套查询
        private static DbQueryableInfo_Select<TElement> TryBuilOuter<TElement>(DbQueryableInfo_Select<TElement> qQuery)
        {
            if (qQuery == null || qQuery.Select == null) return qQuery;

            Expression select = qQuery.Select.Expressions[0];
            List<DbExpression> include = qQuery.Include;
            Type type = qQuery.FromType;

            // 解析导航属性 如果有 一对多 的导航属性，那么查询的结果集的主记录将会有重复记录，这时就需要使用嵌套语义，先查主记录，再关联导航记录
            bool checkListNavgation = false;
            Expression expression = select;
            LambdaExpression lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression != null) expression = lambdaExpression.Body;
            MemberInitExpression initExpression = expression as MemberInitExpression;
            NewExpression newExpression = expression as NewExpression;

            foreach (DbExpression d in include)
            {
                Expression exp = d.Expressions[0];
                if (exp.NodeType == ExpressionType.Lambda) exp = (exp as LambdaExpression).Body;
                else if (exp.NodeType == ExpressionType.Call) exp = (exp as MethodCallExpression).Object;

                // Include 如果包含List<>泛型导航，则可以判定整个查询包含一对多的导航
                if (exp.Type.IsGenericType && exp.Type.GetGenericTypeDefinition() == typeof(List<>)) checkListNavgation = true;
                if (checkListNavgation) break;
            }
            if (!checkListNavgation) checkListNavgation = initExpression != null && CheckListNavigation<TElement>(initExpression);

            if (checkListNavgation)
            {
                NewExpression constructor = initExpression != null ? initExpression.NewExpression : newExpression;
                IEnumerable<MemberBinding> bindings = initExpression != null
                    ? initExpression
                      .Bindings
                      .Where(x => TypeUtils.IsPrimitive((x.Member as System.Reflection.PropertyInfo).PropertyType))
                    : new List<MemberBinding>();

                if (constructor != null || bindings.Count() > 0)
                {
                    initExpression = Expression.MemberInit(constructor, bindings);
                    lambdaExpression = Expression.Lambda(initExpression, lambdaExpression.Parameters);
                    // 简化内层选择器，只选择最小字段
                    qQuery.Select = new DbExpression(DbExpressionType.Select, lambdaExpression);
                }
                qQuery.IsListNavigationQuery = true;
                qQuery.Include = new List<DbExpression>();

                var qOuter = new DbQueryableInfo_Select<TElement>();
                qOuter.FromType = type;
                qOuter.Select = new DbExpression(DbExpressionType.Select, select);
                qOuter.InnerQuery = qQuery;
                qOuter.Join = new List<DbExpression>();
                qOuter.OrderBy = new List<DbExpression>();
                qOuter.Include = include;
                qOuter.HaveListNavigation = true;

                qQuery = qOuter;
            }

            return qQuery;
        }

        // 判定 MemberInit 绑定是否声明了一对多关系的导航
        private static bool CheckListNavigation<T>(MemberInitExpression node)
        {
            for (int i = 0; i < node.Bindings.Count; i++)
            {
                // primitive 类型
                Type type = (node.Bindings[i].Member as System.Reflection.PropertyInfo).PropertyType;
                if (TypeUtils.IsPrimitive(type)) continue;

                // complex 类型
                if (type.IsGenericType)
                {
                    TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(type);
                    if (typeRuntime.GenericTypeDefinition == typeof(List<>)) return true;
                } 

                MemberAssignment memberAssignment = node.Bindings[i] as MemberAssignment;
                if (memberAssignment != null && memberAssignment.Expression.NodeType == ExpressionType.MemberInit)
                {
                    MemberInitExpression initExpression = memberAssignment.Expression as MemberInitExpression;
                    bool checkListNavgation = CheckListNavigation<T>(initExpression);
                    if (checkListNavgation) return true;
                }
            }

            return false;
        }

        // 合并 'Where' 表达式语义
        private static Expression CombineWhere(IList<Expression> predicates)
        {
            if (predicates.Count == 0) return null;

            Expression body = ((LambdaExpression)predicates[0].ReduceUnary()).Body;
            for (int i = 1; i < predicates.Count; i++)
            {
                Expression expression = predicates[i];
                if (expression != null) body = Expression.And(body, ((LambdaExpression)expression.ReduceUnary()).Body);
            }
            return body;

        }
    }
}
