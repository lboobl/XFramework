
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

            //if (_define != null && _define.HaveListNavigation && _define.NavigationDescriptors != null && _define.NavigationDescriptors.Count > 1)
            //{
            //    // 剔除重复的外键记录
            //    var listNavigationDescriptors = 
            //        _define
            //        .NavigationDescriptors
            //        .Where(x => FilterListNavigation(x.Value.Member));
            //    if (listNavigationDescriptors.Count() > 1)
            //    {
            //        TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
            //        string typeName = typeRuntime.Type.Name;

            //        foreach (var model in collection)
            //        {
            //            foreach (var kvp in listNavigationDescriptors)
            //            {
            //                // 例：CRM_SaleOrder.Client.AccountList
            //                var descriptor = kvp.Value;
            //                string keyName = typeName + "." + descriptor.Name;
            //                if (keyName != kvp.Key) continue;

            //                var navWrapper = typeRuntime.GetWrapper(descriptor.Name);
            //                if (navWrapper == null) continue;

            //                string[] keys = keyName.Split('.');
            //                TypeRuntimeInfo curTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
            //                Reflection.MemberAccessWrapper curWrapper = null;
            //                object curModel = model;
            //                object prevList = null;
            //                TypeRuntimeInfo listRuntime = null;

            //                for (int i = 1; i < keys.Length; i++)
            //                {
            //                    curWrapper = curTypeRuntime.GetWrapper(keys[i]);
            //                    curModel = curWrapper.Get(curModel);
            //                    if (i == keys.Length - 1) prevList = curModel;
            //                    Type curType = curModel.GetType();

            //                    if (curType.IsGenericType)
            //                    {
            //                        // 取最后一个记录
            //                        curTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(curType);
            //                        if (i == keys.Length - 1) listRuntime = curTypeRuntime;

            //                        if (curTypeRuntime.GenericTypeDefinition == typeof(List<>))
            //                        {
            //                            var wrapper = curTypeRuntime.GetWrapper("get_Count");
            //                            int count = Convert.ToInt32(wrapper.Invoke(curModel));
            //                            if (count > 0)
            //                            {
            //                                var wrapper2 = curTypeRuntime.GetWrapper("get_Item");
            //                                curModel = wrapper2.Invoke(curModel, count - 1);
            //                            }
            //                            else
            //                            {
            //                                curModel = null;
            //                            }
            //                        }
            //                    }

            //                    // issue#上一个列表为空列表
            //                    if (curModel != null) curTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(curModel.GetType());
            //                }
            //                if (curModel != null)
            //                {
            //                    var wrapper = listRuntime.GetWrapper("get_Count");
            //                    int count = Convert.ToInt32(wrapper.Invoke(prevList));
            //                    if (count > 1)
            //                    {
            //                        // 取第1个
            //                        var wrapper2 = listRuntime.GetWrapper("get_Item");
            //                        var obj1 = wrapper2.Invoke(prevList, 0);
            //                        int index = 0;

            //                        for (int i = 1; i < count; i++)
            //                        {
            //                            var obj2 = wrapper2.Invoke(prevList, i);
            //                            bool duplicate = true;
            //                            foreach (var key in curTypeRuntime.KeyWrappers)
            //                            {
            //                                var wrapper3 = key.Value;
            //                                var key1 = wrapper3.Get(obj1);
            //                                var key2 = wrapper3.Get(obj2);
            //                                duplicate = duplicate && key1.Equals(key2);
            //                            }
            //                            if (duplicate)
            //                            {
            //                                index = i;
            //                                break;
            //                            }
            //                        }

            //                        if (index > 0)
            //                        {
            //                            // RemoveRange(int index, int count);
            //                            var myRemoveMethod = listRuntime.GetWrapper("RemoveRange");
            //                            myRemoveMethod.Invoke(prevList, index, count - index);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

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
