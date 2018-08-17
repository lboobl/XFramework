
using System;
using System.Linq;
using System.Reflection;
using ICS.XFramework.Reflection.Emit;
using System.Collections.Generic;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 成员访问包装器 Facade
    /// </summary>
    public class MemberInvokerWrapper
    {
        private MemberInvokerBase _invoker = null;
        private object[] _attributes = null;
        private MemberInfo _member = null;
        private Type _dataType = null;
        private MethodInfo _setMethod = null;

        /// <summary>
        /// 成员元数据
        /// </summary>
        public MemberInfo Member
        {
            get
            {
                return _member;
            }
        }

        /// <summary>
        /// 字段
        /// </summary>
        public FieldInfo FieldInfo { get { return _member as FieldInfo; } }

        /// <summary>
        /// 属性
        /// </summary>
        public PropertyInfo PropertyInfo { get { return _member as PropertyInfo; } }

        /// <summary>
        /// 方法
        /// </summary>
        public MethodInfo MethodInfo { get { return _member as MethodInfo; } }

        /// <summary>
        /// 成员数据类型
        /// </summary>
        public Type DataType
        {
            get
            {
                if (_dataType == null)
                {
                    _dataType = _member.MemberType == MemberTypes.Property
                        ? ((PropertyInfo)_member).PropertyType
                        : ((FieldInfo)_member).FieldType;
                }

                return _dataType;
            }
        }

        /// <summary>
        /// Set 访问器
        /// </summary>
        public MethodInfo SetMethod
        {
            get
            {
                if (_setMethod == null && _member.MemberType == MemberTypes.Property) _setMethod = (_invoker as PropertyInvoker).SetMethod;
                return _setMethod;
            }
        }

        /// <summary>
        /// 成员完全限定名
        /// </summary>
        public string FullName
        {
            get
            {
                return string.Concat(_member.ReflectedType, ".", _member.Name);
            }
        }

        /// <summary>
        /// 初始化 <see cref="MemberInvokerWrapper"/> 类的新实例
        /// </summary>
        /// <param name="member">成员元数据</param>
        public MemberInvokerWrapper(MemberInfo member)
        {
            _member = member;
            if (_member.MemberType == MemberTypes.Property) _invoker = new PropertyInvoker((PropertyInfo)_member);
            else if (_member.MemberType == MemberTypes.Field) _invoker = new FieldInvoker((FieldInfo)_member);
            else if (_member.MemberType == MemberTypes.Method) _invoker = new MethodInvoker((MethodInfo)_member);

            if (_invoker == null) throw new XFrameworkException("member {0} not supported", member.ToString());
        }

        /// <summary>
        /// 动态调用方法
        /// </summary>
        /// <param name="target">拥有该成员的类实例</param>
        /// <param name="parameters">方法参数</param>
        /// <returns></returns>
        public virtual object Invoke(object target, params object[] parameters)
        {
            return _invoker.Invoke(target, parameters);
        }

        /// <summary>
        /// 获取指定的自定义特性。
        /// </summary>
        /// <typeparam name="TAttribute">自定义特性</typeparam>
        /// <returns></returns>
        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
        {
            if (_attributes == null) _attributes = _member.GetCustomAttributes(false);
            return _attributes.FirstOrDefault(x => x is TAttribute) as TAttribute;
        }

        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        public override string ToString()
        {
            return this.FullName;
        }
    }
}
