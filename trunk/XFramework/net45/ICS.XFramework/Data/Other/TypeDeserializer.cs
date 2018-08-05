
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
        private IDataReader _reader = null;
        private CommandDefinition _define = null;
        private IDictionary<string, Func<IDataRecord, object>> _deserializers = null;

        public TypeDeserializer(IDataReader reader, CommandDefinition define)
        {
            _define = define;
            _reader = reader;
            _deserializers = new Dictionary<string, Func<IDataRecord, object>>(8);
        }

        /// <summary>
        /// 反序列化实体集合
        /// </summary>
        public List<T> Deserialize<T>()
        {
            //bool isTop = true;
            bool isLine = false;
            object prevLine = null;
            List<T> collection = new List<T>();
            TypeDeserializer<T> deserializer = new TypeDeserializer<T>(_reader, _define);
            while (_reader.Read())
            {
                T model = deserializer.Deserialize(prevLine, out isLine);
                if (!isLine)
                {
                    collection.Add(model);
                    prevLine = model;
                }
            }

            return collection;
        }

        static bool FilterListNavigation(MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            if (property == null) return false;
            if (!property.PropertyType.IsGenericType) return false;

            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(property.PropertyType);
            return typeRuntime.GenericTypeDefinition == typeof(List<>);
        }
    }
}
