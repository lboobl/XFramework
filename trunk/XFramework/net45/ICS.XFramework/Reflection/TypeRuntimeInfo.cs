
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using ICS.XFramework.Reflection.Emit;

namespace ICS.XFramework.Reflection
{
    /// <summary>
    /// 类型运行时元数据
    /// </summary>
    public class TypeRuntimeInfo
    {
        private Type _type = null;
        private bool _isAnonymousType = false;

        private object[] _attributes;
        private ConstructorInvoker _ctorInvoker = null;
        private Dictionary<string, MemberInvokerWrapper> _wrappers = null;
        private Type[] _genericArguments = null;
        private Type _genericTypeDefinition = null;
        private bool? _lazyIsCompilerGenerated = null;
        private bool? _lazyIsPrimitive = null;

        /// <summary>
        /// 类型声明
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>
        /// 泛型参数列表
        /// </summary>
        public Type[] GenericArguments
        {
            get
            {
                if (_genericArguments == null && _type.IsGenericType) _genericArguments = _type.GetGenericArguments();
                return _genericArguments;
            }
        }

        /// <summary>
        /// 泛型类型的类型
        /// </summary>
        public Type GenericTypeDefinition
        {
            get
            {
                if (_type.IsGenericType && _genericTypeDefinition == null) _genericTypeDefinition = _type.GetGenericTypeDefinition();
                return _genericTypeDefinition;
            }
        }

        /// <summary>
        /// 是否为匿名类
        /// </summary>
        public bool IsAnonymousType
        {
            get { return _isAnonymousType; }
        }

        /// <summary>
        ///  获取一个值，该值指示当前类型是否是泛型类型。
        /// </summary>
        public bool IsGenericType
        {
            get { return _type.IsGenericType; }
        }

        /// <summary>
        /// 是否编译生成的类型
        /// </summary>
        public bool IsCompilerGenerated
        {
            get
            {
                if (_lazyIsCompilerGenerated == null) _lazyIsCompilerGenerated = TypeUtils.IsCompilerGenerated(_type);
                return _lazyIsCompilerGenerated.Value;
            }
        }

        /// <summary>
        /// 判断给定类型是否是ORM支持的基元类型
        /// </summary>
        public bool IsPrimitive
        {
            get
            {
                if (_lazyIsPrimitive == null) _lazyIsPrimitive = TypeUtils.IsPrimitive(_type);
                return _lazyIsPrimitive.Value;
            }
        }

        /// <summary>
        /// 成员包装器集合
        /// </summary>
        public Dictionary<string, MemberInvokerWrapper> Wrappers
        {
            get { return (_wrappers = _wrappers ?? this.InitializeWrapper(_type)); }
        }

        /// <summary>
        /// 构造函数调用器
        /// </summary>
        public ConstructorInvoker ConstructInvoker
        {
            get
            {
                if (_ctorInvoker == null)
                {
                    var ctor = this.GetConstructor();
                    _ctorInvoker = new ConstructorInvoker(ctor);

                }
                return _ctorInvoker;
            }
        }

        /// <summary>
        /// 初始化 <see cref="TypeRuntimeInfo"/> 类的新实例
        /// </summary>
        /// <param name="type">类型声明</param>
        internal TypeRuntimeInfo(Type type)
        {
            _type = type;
            _isAnonymousType = type != null && type.Name.Length > 18 && type.Name.IndexOf("AnonymousType", 5, StringComparison.InvariantCulture) == 5;
        }

        /// <summary>
        /// 取指定的成员包装器
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public MemberInvokerWrapper GetWrapper(string memberName)
        {
            MemberInvokerWrapper wrapper = null;
            this.Wrappers.TryGetValue(memberName, out wrapper);

            //if (wrapper == null) throw new XfwException("member [{0}.{1}] doesn't exists", _type.Name, memberName);

            return wrapper;
        }

        /// <summary>
        /// 访问成员
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public object Invoke(string memberName, object target, params object[] parameters)
        {
            MemberInvokerWrapper wrapper = null;
            this.Wrappers.TryGetValue(memberName, out wrapper);

            if (wrapper == null) throw new XFrameworkException("[{0}.{1}] doesn't exists", _type.Name, memberName);
            return wrapper.Invoke(target, parameters);
        }

        /// <summary>
        /// 取指定的成员包装器自定义特性。
        /// </summary>
        /// <typeparam name="TAttribute">自定义特性</typeparam>
        /// <returns></returns>
        public TAttribute GetWrapperAttribute<TAttribute>(string memberName) where TAttribute : Attribute
        {
            MemberInvokerWrapper wrapper = this.GetWrapper(memberName);
            return wrapper != null ? wrapper.GetCustomAttribute<TAttribute>() : null;
        }

        /// <summary>
        /// 获取指定的自定义特性。
        /// </summary>
        /// <typeparam name="TAttribute">自定义特性</typeparam>
        /// <returns></returns>
        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
        {
            if (_attributes == null) _attributes = _type.GetCustomAttributes(false);
            return _attributes.FirstOrDefault(x => x is TAttribute) as TAttribute;
        }

        /// <summary>
        /// 初始化成员包装器集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, MemberInvokerWrapper> InitializeWrapper(Type type)
        {
            // 静态/实例 私有/公有
            Func<MemberInfo, bool> predicate = x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Method;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var wrappers = 
                type
                .GetMembers(flags)
                .Where(predicate)
                .Select(x => new MemberInvokerWrapper(x));

            // fix issue # overide method
            Dictionary<string, MemberInvokerWrapper> result = new Dictionary<string, MemberInvokerWrapper>();
            foreach (var w in wrappers)
            {
                if (!result.ContainsKey(w.Member.Name)) result.Add(w.Member.Name, w);
            }

            return result;
        }

        /// <summary>
        /// 获取构造函数
        /// 优先顺序与参数数量成反比
        /// </summary>
        /// <returns></returns>
        protected virtual ConstructorInfo GetConstructor()
        {
            ConstructorInfo[] ctors = _type.GetConstructors();
            if (_isAnonymousType) return ctors[0];

            for (int i = 0; i < 10; i++)
            {
                ConstructorInfo ctor = ctors.FirstOrDefault(x => x.GetParameters().Length == i);
                if (ctor != null) return ctor;
            }

            return ctors[0];
        }

        /// <summary>
        /// 获取构造函数
        /// 优先顺序与参数数量成反比
        /// </summary>
        /// <returns></returns>
        public ConstructorInfo GetConstructor(Type[] types)
        {
            ConstructorInfo[] ctors = _type.GetConstructors();
            if (_isAnonymousType) return ctors[0];

            if (types != null && types.Length > 0)
            {
                foreach (var ctor in ctors)
                {
                    var parameters = ctor.GetParameters();
                    if (parameters != null && parameters.Length == types.Length)
                    {
                        bool match = true;
                        for (int i = 0; i < parameters.Length; i++) match = parameters[i].ParameterType == types[i];
                        if (match) return ctor;
                    }
                }
            }

            throw new XFrameworkException("not such constructor.");
        }
    }
}
