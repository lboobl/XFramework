using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ICS.XFramework.Data
{
    public static  class ExpressionExtensions
    {
        private static readonly string _anonymousName = "<>h__TransparentIdentifier";
        private static readonly string _groupingName = "IGrouping`2";

        private static Func<string, bool> _isGrouping = g => g == ExpressionExtensions._groupingName;
        private static Func<string, bool> _isAnonymous = name => !string.IsNullOrEmpty(name) && name.StartsWith(ExpressionExtensions._anonymousName, StringComparison.Ordinal);

        /// <summary>
        /// 判断属性访问表达式是否有系统动态生成前缀
        /// <code>
        /// h__TransparentIdentifier.a.CompanyName
        /// </code>
        /// </summary>
        public static bool IsAnonymous(this Expression node)
        {
            // <>h__TransparentIdentifier => h__TransparentIdentifier.a.CompanyName
            Expression exp = node;
            ParameterExpression paramExp = exp.NodeType == ExpressionType.Lambda 
                ? (node as LambdaExpression).Parameters[0]
                : exp as ParameterExpression;
            if (paramExp != null) return ExpressionExtensions._isAnonymous(paramExp.Name);

            if (exp.NodeType == ExpressionType.MemberAccess)    // <>h__TransparentIdentifier.a.CompanyName
            {
                MemberExpression memExp = exp as MemberExpression;
                if (ExpressionExtensions._isAnonymous(memExp.Member.Name)) return true;

                return ExpressionExtensions.IsAnonymous(memExp.Expression);
            }

            return false;
        }

        /// <summary>
        /// 判断是否是分组表达式
        /// </summary>
        public static bool IsGrouping(this Expression node)
        {
            //g.Key
            //g.Key.CompanyName
            //g.Max()
            //g=>g.xxx
            //g.Key.CompanyId.Length
            //g.Key.Length 

            // g | g=>g.xx
            Expression exp = node;
            ParameterExpression paramExp = exp.NodeType == ExpressionType.Lambda
                ? (node as LambdaExpression).Parameters[0]
                : exp as ParameterExpression;
            if (paramExp != null) return ExpressionExtensions._isGrouping(paramExp.Type.Name);

            // g.Max
            MethodCallExpression callExp = exp as MethodCallExpression;
            if (callExp != null) return ExpressionExtensions._isGrouping(callExp.Arguments[0].Type.Name);


            MemberExpression memExp = exp as MemberExpression;
            if (memExp != null)
            {
                // g.Key
                var g1 = memExp.Member.Name == "Key" && ExpressionExtensions._isGrouping(memExp.Expression.Type.Name);
                if (g1) return g1;

                // g.Key.Length | g.Key.Company | g.Key.CompanyId.Length
                memExp = memExp.Expression as MemberExpression;
                if (memExp != null)
                {
                    g1 = memExp.Member.Name == "Key" && ExpressionExtensions._isGrouping(memExp.Expression.Type.Name) && memExp.Type.Namespace == null; //匿名类没有命令空间
                    if (g1) return g1;
                }
            }

            return false;
        }

        /// <summary>
        /// 在递归访问 MemberAccess 表达式时，判定节点是否能够被继续递归访问
        /// </summary>
        public static bool IsArrivable(this Expression node)
        {
            // a 
            // <>h__TransparentIdentifier.a
            // <>h__TransparentIdentifier0.<>h__TransparentIdentifier1.a
            
            if (node.NodeType == ExpressionType.Parameter) return false;

            if (node.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression m = node as MemberExpression;
                if (m.Expression.NodeType == ExpressionType.Parameter)
                {
                    string name = (m.Expression as ParameterExpression).Name;
                    if (ExpressionExtensions._isAnonymous(name)) return false;
                }

                if (m.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    string name = (m.Expression as MemberExpression).Member.Name;
                    if (ExpressionExtensions._isAnonymous(name)) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 取剔除掉系统动态生成前缀后的表达式
        /// </summary>
        public static string GetKeyWidthoutAnonymous(this MemberExpression node)
        {
            List<string> chain = new List<string>();
            chain.Add(node.Member.Name);

            Expression expression = node.Expression;
            while (expression.IsArrivable())
            {
                chain.Add((expression as MemberExpression).Member.Name);
                expression = (expression as MemberExpression).Expression;
            }

            if (expression.NodeType == ExpressionType.Parameter) chain.Add((expression as ParameterExpression).Name);
            if (expression.NodeType == ExpressionType.MemberAccess) chain.Add((expression as MemberExpression).Member.Name);

            chain.Reverse();
            string result = string.Join(".", chain);
            return result;
        }
    }
}
