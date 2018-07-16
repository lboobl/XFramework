
using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using ICS.XFramework.Reflection.Emit;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// <see cref="IDataReader"/> 转实体反序列化器
    /// </summary>
    public partial class TypeDeserializer
    {
        /// <summary>
        /// 异步反序列化实体集合
        /// </summary>
        public async Task<List<T>> DeserializeAsync<T>()
        {
            bool isTop = true;
            bool isLine = false;
            object prevLine = null;
            List<T> collection = new List<T>();
            TypeDeserializer<T> deserializer = new TypeDeserializer<T>(_reader, _define as CommandDefinition);
            while (await (_reader as DbDataReader).ReadAsync())
            {
                T model = deserializer.Deserialize(prevLine, isLine);

                // 若有 1:n 的导航属性，判断当前行数据与上一行数据是否相同
                if (prevLine != null && _define.HaveListNavigation)
                {
                    isLine = true;
                    TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
                    Func<KeyValuePair<string, Reflection.MemberAccessWrapper>, bool> predicate =
                        x => (x.Value as MemberAccessWrapper) != null && (x.Value as MemberAccessWrapper).Column != null && (x.Value as MemberAccessWrapper).Column.IsKey;
                    var keys =
                        typeRuntime
                        .Wrappers
                        .Where(predicate)
                        .Select(x => x.Value);
                    foreach (var wrapper in keys)
                    {
                        var key1 = wrapper.Get(prevLine);
                        var key2 = wrapper.Get(model);
                        isLine = isLine && key1.Equals(key2);
                        if (!isLine) break;
                    }
                }

                if (!isLine) collection.Add(model);
                if (prevLine == null) prevLine = model;
                if (!isLine && !isTop) prevLine = null;
                isTop = false;
            }
            return collection;
        }
    }
}
