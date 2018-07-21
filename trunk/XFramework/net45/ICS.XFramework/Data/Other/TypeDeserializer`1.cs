﻿
using System;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using ICS.XFramework.Reflection.Emit;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 单个实体反序列化
    /// </summary>
    internal class TypeDeserializer<T>
    {
        static MethodInfo _isDBNull = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        static MethodInfo _getFieldType = typeof(IDataRecord).GetMethod("GetFieldType", new Type[] { typeof(int) });
        static MethodInfo _getBoolean = typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) });
        static MethodInfo _getByte = typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) });
        static MethodInfo _getChar = typeof(IDataRecord).GetMethod("GetChar", new Type[] { typeof(int) });
        static MethodInfo _getDateTime = typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) });
        static MethodInfo _getDecimal = typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) });
        static MethodInfo _getDouble = typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) });
        static MethodInfo _getFloat = typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) });
        static MethodInfo _getGuid = typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) });
        static MethodInfo _getInt16 = typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) });
        static MethodInfo _getInt32 = typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) });
        static MethodInfo _getInt64 = typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) });
        static MethodInfo _getString = typeof(IDataRecord).GetMethod("GetString", new Type[] { typeof(int) });
        static MethodInfo _getValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });

        private IDataReader _reader = null;
        private CommandDefinition _define = null;
        private IDictionary<string, Func<IDataRecord, object>> _deserializers = null;
        private Func<IDataRecord, object> _modelDeserializer = null;

        internal TypeDeserializer(IDataReader reader, CommandDefinition define)
        {
            _reader = reader;
            _define = define;
            _deserializers = new Dictionary<string, Func<IDataRecord, object>>(8);
        }

        /// <summary>
        /// 将 <see cref="IDataRecord"/> 上的当前行反序列化为实体
        /// </summary>
        /// <param name="prevModel">前一行数据</param>
        /// <param name="isLine">是否同行数据</param>
        internal T Deserialize(object prevModel, out bool isLine)
        {
            isLine = false;

            #region 基元类型

            if (Reflection.TypeUtils.IsPrimitive(typeof(T)))
            {
                if (_reader.IsDBNull(0)) return default(T);

                var obj = _reader.GetValue(0);
                if (obj.GetType() != typeof(T))
                {
                    // fix#Nullable<T> issue
                    if (!typeof(T).IsGenericType)
                    {
                        obj = Convert.ChangeType(obj, typeof(T));
                    }
                    else
                    {
                        Type g = typeof(T).GetGenericTypeDefinition();
                        if (g != typeof(Nullable<>)) throw new NotSupportedException(string.Format("type {0} not suppored.", g.FullName));

                        obj = Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeof(T)));
                    }
                }

                return (T)obj;
            }

            #endregion

            #region 匿名类型

            TypeRuntimeInfo runtime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
            ICS.XFramework.Reflection.Emit.ConstructorInvoker ctor = runtime.ConstructInvoker;
            if (runtime.IsAnonymousType)
            {
                object[] values = new object[_reader.FieldCount];
                _reader.GetValues(values);
                for (int index = 0; index < values.Length; ++index)
                {
                    if (values[index] is DBNull) values[index] = null;
                }
                return (T)ctor.Invoke(values);
            }

            #endregion

            #region 实体类型

            T model = default(T);
            if (_define == null)
            {
                // 直接跑SQL,则不解析导航属性
                if (_modelDeserializer == null) _modelDeserializer = GetDeserializer(typeof(T), _reader);
                model = (T)_modelDeserializer(_reader);
            }
            else if (_define.NavigationDescriptors != null && _define.NavigationDescriptors.Count == 0)
            {
                // 直接跑SQL,则不解析导航属性
                if (_modelDeserializer == null) _modelDeserializer = GetDeserializer(typeof(T), _reader, _define.Columns, 0);
                model = (T)_modelDeserializer(_reader);
            }
            else
            {
                // 第一层
                if (_modelDeserializer == null) _modelDeserializer = GetDeserializer(typeof(T), _reader, _define.Columns, 0, _define.NavigationDescriptors.MinIndex);
                model = (T)_modelDeserializer(_reader);
                // 若有 1:n 的导航属性，判断当前行数据与上一行数据是否相同
                if (prevModel != null && _define.HaveListNavigation)
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
                        var key1 = wrapper.Get(prevModel);
                        var key2 = wrapper.Get(model);
                        isLine = isLine && key1.Equals(key2);
                        if (!isLine) break;
                    }
                }

                // 递归导航属性
                this.Deserialize_Navigation(isLine ? prevModel : null, model, string.Empty, isLine);
            }

            return model;

            #endregion
        }

        // 导航属性
        // @prevLine 前一行数据
        // @isLine   是否同一行数据<同一父级>
        private void Deserialize_Navigation(object prevModel, object model, string typeName, bool isLine)
        {
            // CRM_SaleOrder.Client 
            // CRM_SaleOrder.Client.AccountList
            Type type = model.GetType();
            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(type);
            if (string.IsNullOrEmpty(typeName)) typeName = type.Name;

            foreach (var kvp in _define.NavigationDescriptors)
            {
                int start = -1;
                int end = -1;
                var descriptor = kvp.Value;
                if (descriptor.FieldCount > 0)
                {
                    start = descriptor.Start;
                    end = descriptor.Start + descriptor.FieldCount;
                }

                string keyName = typeName + "." + descriptor.Name;
                if (keyName != kvp.Key) continue;

                var navWrapper = typeRuntime.GetWrapper(descriptor.Name);
                if (navWrapper == null) continue;

                Type navType = navWrapper.DataType;
                Func<IDataRecord, object> deserializer = null;
                TypeRuntimeInfo navTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(navType);
                object list = null;
                if (navType.IsGenericType && navTypeRuntime.GenericTypeDefinition == typeof(List<>))
                {
                    // 1：n关系，导航属性为 List<T>
                    list = navWrapper.Get(model);
                    if (list == null)
                    {
                        list = navTypeRuntime.ConstructInvoker.Invoke();
                        navWrapper.Set(model, list);
                    }
                }

                if (!_deserializers.TryGetValue(keyName, out deserializer))
                {
                    deserializer = GetDeserializer(navType.IsGenericType ? navType.GetGenericArguments()[0] : navType, _reader, _define.Columns, start, end);
                    _deserializers[keyName] = deserializer;
                }

                // 如果整个导航链中某一个导航属性为空，则跳出递归
                object navModel = deserializer(_reader);
                if (navModel != null)
                {
                    if (list == null)
                    {
                        navWrapper.Set(model, navModel);
                        //
                        //
                        // 
                    }
                    else
                    {
                        var method = navTypeRuntime.GetWrapper("Add");
                        method.Invoke(list, navModel);

                        #region 合并列表

                        // 合并列表 
                        if (prevModel != null && isLine)
                        {
                            // 例：CRM_SaleOrder.Client.AccountList
                            string[] keys = keyName.Split('.');
                            TypeRuntimeInfo pTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo<T>();
                            Reflection.MemberAccessWrapper pWrapper = null;
                            object pModel = prevModel;
                            object prevList = null;

                            for (int i = 1; i < keys.Length; i++)
                            {
                                pWrapper = pTypeRuntime.GetWrapper(keys[i]);
                                pModel = pWrapper.Get(pModel);
                                if (i == keys.Length - 1) prevList = pModel;

                                Type objType = pModel.GetType();
                                if(objType.IsGenericType)// && objType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    // 取最后一个记录
                                    TypeRuntimeInfo objTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(objType);
                                    if (objTypeRuntime.GenericTypeDefinition == typeof(List<>))
                                    {
                                        var wrapper = objTypeRuntime.GetWrapper("get_Count");
                                        int count = Convert.ToInt32(wrapper.Invoke(pModel));
                                        if (count > 0)
                                        {
                                            var wrapper2 = navTypeRuntime.GetWrapper("get_Item");
                                            pModel = wrapper2.Invoke(pModel, count - 1);
                                        }
                                        else
                                        {
                                            pModel = null;
                                        }
                                    }
                                }

                                // issue#上一个列表为空列表
                                if (pModel != null) pTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(pModel.GetType());
                            }
                            if (pModel != null)
                            {
                                // 则添加到上一行的相同导航列表中去
                                method = navTypeRuntime.GetWrapper("Add");
                                method.Invoke(prevList, navModel);
                            }
                        }

                        #endregion
                    }

                    // TODO 1:n:n 关系未实现
                    if (navTypeRuntime.GenericTypeDefinition == typeof(List<>)) navTypeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(navTypeRuntime.GenericArguments[0]);
                    if (navTypeRuntime.NavWrappers.Count > 0) Deserialize_Navigation(prevModel, navModel, keyName, isLine);
                }
            }
        }

        static Func<IDataRecord, object> GetDeserializer(Type type, IDataRecord reader, IDictionary<string, Column> columns = null, int start = 0, int? end = null)
        {
            string methodName = Guid.NewGuid().ToString();
            DynamicMethod dynamicMethod = new DynamicMethod(methodName, typeof(object), new[] { typeof(IDataRecord) }, true);
            ILGenerator g = dynamicMethod.GetILGenerator();
            TypeRuntimeInfo typeRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(type);

            LocalBuilder model = g.DeclareLocal(type);
            Label exitLabel = g.DefineLabel();
            Label loadNullLabel = g.DefineLabel();
            g.Emit(OpCodes.Newobj, typeRuntime.ConstructInvoker.Constructor);
            g.Emit(OpCodes.Stloc, model);

            if (end == null) end = reader.FieldCount;
            for (int index = start; index < end; index++)
            {
                string keyName = reader.GetName(index);
                if (columns != null)
                {
                    Column column = null;
                    columns.TryGetValue(keyName, out column);
                    keyName = column != null ? column.Name : string.Empty;
                }

                if (keyName == ColumnExpressionVisitor.NullFieldName)
                {
                    g.Emit(OpCodes.Ldarg_0);
                    g.Emit(OpCodes.Ldc_I4, index);
                    g.Emit(OpCodes.Callvirt, _isDBNull);
                    g.Emit(OpCodes.Brtrue, loadNullLabel);
                }

                var wrapper = typeRuntime.GetWrapper(keyName);
                if (wrapper == null) continue;

                var isDBNullLabel = g.DefineLabel();
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, index);
                g.Emit(OpCodes.Callvirt, _isDBNull);
                g.Emit(OpCodes.Brtrue, isDBNullLabel);

                // member的类型可能与数据库中查出来的数据类型不一样
                // 如 boolean，数据库类型 是int
                var method = GetReaderMethod(reader.GetFieldType(index));
                g.Emit(OpCodes.Ldloc, model);
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, index);
                g.Emit(OpCodes.Callvirt, method);

                Type memberType = wrapper.DataType;
                Type nullUnderlyingType = memberType.IsGenericType ? Nullable.GetUnderlyingType(memberType) : null;
                Type unboxType = nullUnderlyingType != null ? nullUnderlyingType : memberType;

                if (unboxType == typeof(byte[]) || (method == _getValue && memberType != typeof(object)))
                {
                    g.Emit(OpCodes.Castclass, memberType);
                }

                if (nullUnderlyingType != null)
                {
                    g.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType }));
                }

                if (wrapper.Member.MemberType == MemberTypes.Field)
                {
                    g.Emit(OpCodes.Stfld, wrapper.FieldInfo);
                }
                else
                {
                    g.Emit(OpCodes.Callvirt, wrapper.SetMethod);
                }

                g.MarkLabel(isDBNullLabel);
            }

            // 直接跳到结束标签返回实体
            g.Emit(OpCodes.Br, exitLabel);

            // 将 null 赋值给实体
            g.MarkLabel(loadNullLabel);
            g.Emit(OpCodes.Ldnull);
            g.Emit(OpCodes.Stloc, model);

            // 结束
            g.MarkLabel(exitLabel);
            g.Emit(OpCodes.Ldloc, model);
            g.Emit(OpCodes.Ret);

            var func = (Func<IDataRecord, object>)dynamicMethod.CreateDelegate(typeof(Func<IDataRecord, object>));
            return func;
        }

        private static MethodInfo GetReaderMethod(Type fieldType)
        {
            if (fieldType == typeof(char)) return _getChar;
            if (fieldType == typeof(string)) return _getString;
            if (fieldType == typeof(bool) || fieldType == typeof(bool?)) return _getBoolean;
            if (fieldType == typeof(byte) || fieldType == typeof(byte?)) return _getByte;
            if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?)) return _getDateTime;
            if (fieldType == typeof(decimal) || fieldType == typeof(decimal?)) return _getDecimal;
            if (fieldType == typeof(double) || fieldType == typeof(double?)) return _getDouble;
            if (fieldType == typeof(float) || fieldType == typeof(float?)) return _getFloat;
            if (fieldType == typeof(Guid) || fieldType == typeof(Guid?)) return _getGuid;
            if (fieldType == typeof(short) || fieldType == typeof(short?)) return _getInt16;
            if (fieldType == typeof(int) || fieldType == typeof(int?)) return _getInt32;
            if (fieldType == typeof(long) || fieldType == typeof(long?)) return _getInt64;

            return _getValue;

            //bit	Boolean
            //tinyint	Byte
            //smallint	Int16
            //int	Int32
            //bigint	Int64
            //smallmoney	Decimal
            //money	Decimal
            //numeric	Decimal
            //decimal	Decimal
            //float	Double
            //real	Single
            //smalldatetime	DateTime
            //datetime	DateTime
            //timestamp	DateTime
            //char	String
            //text	String
            //varchar	String
            //nchar	String
            //ntext	String
            //nvarchar	String
            //binary	Byte[]
            //varbinary	Byte[]
            //image	Byte[]
            //uniqueidentifier	Guid
            //Variant	Object
        }
    }
}
