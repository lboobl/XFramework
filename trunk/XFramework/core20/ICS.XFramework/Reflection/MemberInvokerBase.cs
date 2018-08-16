using System;
using System.Reflection;

namespace ICS.XFramework.Reflection
{
    /// <summary>
    /// 成员反射基类定义
    /// </summary>
    public abstract class MemberInvokerBase
    {
        private MemberInfo _member;

        /// <summary>
        /// 成员
        /// <para>
        /// 可能是方法，属性或者字段
        /// </para>
        /// </summary>
        public MemberInfo Member
        {
            get
            {
                return _member;
            }
        }

        /// <summary>
        /// 成员长名称
        /// </summary>
        public string FullName
        {
            get
            {
                return string.Concat(_member.ReflectedType, ".", _member.Name);
            }
        }

        /// <summary>
        /// 初始化 <see cref="MemberInvokerBase"/> 类的新实例
        /// </summary>
        /// <param name="member">成员元数据</param>
        public MemberInvokerBase(MemberInfo member)
        {
            XFrameworkException.Check.NotNull<MemberInfo>(member, "member");
            _member = member;
        }

        /// <summary>
        /// 动态访问成员
        /// </summary>
        /// <param name="target">拥有该成员的类实例</param>
        /// <param name="parameters">方法参数</param>
        /// <returns></returns>
        public virtual object Invoke(object target, params object[] parameters)
        {
            throw new XFrameworkException("{0}.Invoke not supported.", this.FullName);
        }

        ///// <summary>
        ///// Get 属性/字段 值
        ///// </summary>
        ///// <param name="target">拥有该成员的类实例</param>
        ///// <returns></returns>
        //public virtual object Get(object target)
        //{
        //    throw new XFrameworkException("{0}.Get not supported.", this.FullName);
        //}

        ///// <summary>
        ///// Set 属性/字段 值
        ///// </summary>
        ///// <param name="target">拥有该成员的类实例</param>
        ///// <param name="value">字段/属性值</param>
        //public virtual void Set(object target, object value)
        //{
        //    throw new XFrameworkException("{0}.Set not supported.", this.FullName);
        //}

        ///// <summary>
        ///// 动态调用方法
        ///// </summary>
        ///// <param name="target">拥有该成员的类实例</param>
        ///// <param name="parameters">方法参数</param>
        ///// <returns></returns>
        //public virtual object Invoke(object target, params object[] parameters)
        //{
        //    throw new XFrameworkException("{0}.Invoke not supported.", this.FullName);
        //}
    }
}
