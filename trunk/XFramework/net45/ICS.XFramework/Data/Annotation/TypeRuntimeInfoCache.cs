using System;
using System.Collections.Generic;
using ICS.XFramework.Caching;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 类型运行时元数据缓存
    /// </summary>
    /// <remarks>适用Data命名空间</remarks>
    public static class TypeRuntimeInfoCache
    {
        static readonly ICache<Type, TypeRuntimeInfo> _typeRuntimeCache = new ReaderWriterCache<Type, TypeRuntimeInfo>(MemberInfoComparer<Type>.Default);

        /// <summary>
        /// 取指定类型的运行时元数据
        /// </summary>
        /// <param name="type">类型实例</param>
        /// <returns></returns>
        public static TypeRuntimeInfo GetRuntimeInfo(Type type)
        {
            return _typeRuntimeCache.GetOrAdd(type, p => new TypeRuntimeInfo(p));
        }

        /// <summary>
        /// 取指定类型的运行时元数据
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns></returns>
        public static TypeRuntimeInfo GetRuntimeInfo<T>()
        {
            return TypeRuntimeInfoCache.GetRuntimeInfo(typeof(T));
        }
    }

}
