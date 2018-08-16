
using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using ICS.XFramework.Reflection.Emit;
using System.Text;
using ICS.XFramework.Caching;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// <see cref="IDataReader"/> 转实体映射
    /// </summary>
    public partial class TypeDeserializer
    {
        private IDataReader _reader = null;
        private CommandDefinition _define = null;
        static ICache<string, object> _deserializers = new ReaderWriterCache<string, object>();

        public TypeDeserializer(IDataReader reader, CommandDefinition define)
        {
            _define = define;
            _reader = reader;
        }

        /// <summary>
        /// 反序列化实体集合
        /// </summary>
        public List<T> Deserialize<T>()
        {
            bool isLine = false;
            object prevLine = null;
            List<T> collection = new List<T>();

            object obj = null;
            string key = GetDeserializerKey<T>(_reader, _define);
            _deserializers.TryGet(key, out obj);
            if(obj == null) obj = new TypeDeserializer<T>(_define);
            TypeDeserializer<T> deserializer = (TypeDeserializer<T>)obj;

            while (_reader.Read())
            {
                T model = deserializer.Deserialize(_reader, prevLine, out isLine);
                if (!isLine)
                {
                    collection.Add(model);
                    prevLine = model;
                }
            }

            // 添加映射器到缓存
            _deserializers.GetOrAdd(key, x => obj);

            // 返回结果
            return collection;
        }
        
        static  string GetDeserializerKey<T>(IDataReader reader, CommandDefinition define)
        {
            var keyBuilder = new StringBuilder(typeof(T).FullName);
            if (define != null)
            {
                if (define.Columns != null) foreach (var kv in define.Columns) keyBuilder.AppendFormat("_{0}", kv.Key);
                if (define.NavigationDescriptors != null) foreach (var kv in define.NavigationDescriptors) keyBuilder.AppendFormat("_{0}<{1},{2}>", kv.Key, kv.Value.Start, kv.Value.Start + kv.Value.FieldCount);
            }
            else
            {
                for (int i = 0; i < reader.FieldCount; i++) keyBuilder.AppendFormat("_{0}", reader.GetName(i));
            }

            string key = keyBuilder.ToString();
            return key;
        }
    }
}
