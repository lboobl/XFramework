
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            bool isLine = false;
            object prevLine = null;
            List<T> collection = new List<T>();

            object obj = null;
            string key = GetDeserializerKey<T>(_reader, _define);
            _deserializers.TryGet(key, out obj);
            if (obj == null) obj = new TypeDeserializer<T>();
            TypeDeserializer<T> deserializer = (TypeDeserializer<T>)obj;
            deserializer.Reader = _reader;
            deserializer.CommandDefinition = _define;

            while (await (_reader as DbDataReader).ReadAsync())
            {
                T model = ((TypeDeserializer<T>)deserializer).Deserialize(prevLine, out isLine);
                if (!isLine)
                {
                    collection.Add(model);
                    prevLine = model;
                }
            }

            // 添加映射器到缓存
            _deserializers.GetOrAdd(key, x => deserializer);

            // 返回结果
            return collection;
        }
    }
}
