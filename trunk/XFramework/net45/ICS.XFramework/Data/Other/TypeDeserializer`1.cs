
using System;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using ICS.XFramework.Reflection.Emit;

namespace ICS.XFramework.Data
{
    // TODO Include 语法带来的分页问题
    /// <summary>
    /// 类型序列化器
    /// </summary>
    public class TypeDeserializer
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

        public TypeDeserializer(IDataReader reader, CommandDefinition define)
        {
            _define = define;
            _reader = reader;
            _deserializers = new Dictionary<string, Func<IDataRecord, object>>(8);
        }

        /// <summary>
        /// 将 <see cref="IDataRecord"/> 上的当前行反序列化为实体
        /// </summary>
        /// <returns></returns>
        public T Deserialize<T>()
        {
            T model = default(T);
            while (_reader.Read())
            {
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

                    // 递归导航属性
                    this.Deserialize_Navigation(model, string.Empty);
                }

                return model;

                #endregion
            }

            return model;
        }

        T Deserialize<T>(T model)
        {

            return default(T);
        }

        // 导航属性
        private void Deserialize_Navigation(object model, string typeName)
        {
            // CRM_SaleOrder.Client
            Type pType = model.GetType();
            TypeRuntimeInfo runtime = TypeRuntimeInfoCache.GetRuntimeInfo(pType);
            if (string.IsNullOrEmpty(typeName)) typeName = pType.Name;

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

                var navWrapper = runtime.GetWrapper(descriptor.Name);
                if (navWrapper == null) continue;

                Type navType = navWrapper.DataType;
                Func<IDataRecord, object> deserializer = null;
                if (!_deserializers.TryGetValue(keyName, out deserializer))
                {
                    deserializer = GetDeserializer(navType, _reader, _define.Columns, start, end);
                    _deserializers[keyName] = deserializer;
                }

                object navModel = deserializer(_reader);
                navWrapper.Set(model, navModel);

                TypeRuntimeInfo navRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(navType);
                if (navRuntime.NavWrappers.Count > 0) Deserialize_Navigation(navModel, keyName);
            }
        }

        private static Func<IDataRecord, object> GetDeserializer(Type modelType, IDataRecord reader, IDictionary<string, Column> columns = null, int start = 0, int? end = null)
        {
            string methodName = Guid.NewGuid().ToString();
            DynamicMethod dm = new DynamicMethod(methodName, typeof(object), new[] { typeof(IDataRecord) }, true);
            ILGenerator g = dm.GetILGenerator();
            TypeRuntimeInfo runtime = TypeRuntimeInfoCache.GetRuntimeInfo(modelType);

            var model = g.DeclareLocal(modelType);
            g.Emit(OpCodes.Newobj, runtime.ConstructInvoker.Constructor);
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

                var wrapper = runtime.GetWrapper(keyName);// as MemberAccessWrapper;
                if (wrapper == null) continue;

                var isDBNullLabel = g.DefineLabel();
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, index);
                g.Emit(OpCodes.Callvirt, _isDBNull);
                g.Emit(OpCodes.Brtrue, isDBNullLabel);

                // member的类型可能与数据库中查出来的数据类型不一样
                // 如 boolean，数据库类型 是int
                var m_method = GetReaderMethod(reader.GetFieldType(index));
                g.Emit(OpCodes.Ldloc, model);
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, index);
                g.Emit(OpCodes.Callvirt, m_method);

                Type memberType = wrapper.DataType;
                Type nullUnderlyingType = memberType.IsGenericType ? Nullable.GetUnderlyingType(memberType) : null;
                Type unboxType = nullUnderlyingType != null ? nullUnderlyingType : memberType;

                if (unboxType == typeof(byte[]) || (m_method == _getValue && memberType != typeof(object)))
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

            g.Emit(OpCodes.Ldloc, model);
            g.Emit(OpCodes.Ret);

            var func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
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

    /// <summary>
    /// 类型序列化器
    /// </summary>
    public class TypeDeserializer<T>
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

        public TypeDeserializer(IDataReader reader, CommandDefinition define)
        {
            _define = define;
            _reader = reader;
            _deserializers = new Dictionary<string, Func<IDataRecord, object>>(8);
        }

        /// <summary>
        /// 将 <see cref="IDataRecord"/> 上的当前行反序列化为实体
        /// </summary>
        /// <returns></returns>
        public T Deserialize()
        {
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

                // 递归导航属性
                this.Deserialize_Navigation(model, string.Empty);
            }

            return model;

            #endregion
        }

        // 导航属性
        private void Deserialize_Navigation(object model, string typeName)
        {
            // CRM_SaleOrder.Client 
            // Client.AccountList
            Type pType = model.GetType();
            TypeRuntimeInfo runtime = TypeRuntimeInfoCache.GetRuntimeInfo(pType);
            if (string.IsNullOrEmpty(typeName)) typeName = pType.Name;

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

                var navWrapper = runtime.GetWrapper(descriptor.Name);
                if (navWrapper == null) continue;

                Type navType = navWrapper.DataType;
                Func<IDataRecord, object> deserializer = null;
                TypeRuntimeInfo navRuntime = TypeRuntimeInfoCache.GetRuntimeInfo(navType);
                object list = null;
                if (navType.IsGenericType && navType.Name == "List`1")
                {
                    // 1：n关系，导航属性为 List<T>
                    list = navWrapper.Get(model);
                    if (list == null)
                    {
                        list = navRuntime.ConstructInvoker.Invoke();
                        navWrapper.Set(model, list);
                    }
                }

                if (!_deserializers.TryGetValue(keyName, out deserializer))
                {
                    deserializer = GetDeserializer(navType.IsGenericType ? navType.GetGenericArguments()[0] : navType, _reader, _define.Columns, start, end);
                    _deserializers[keyName] = deserializer;
                }

                object navModel = deserializer(_reader);
                if (list == null)
                {
                    navWrapper.Set(model, navModel);
                }
                else
                {
                    var addWrapper = navRuntime.GetWrapper("Add");
                    if (addWrapper == null)
                    {
                        //navRuntime.Wrappers.Add();
                    }

                    addWrapper.Invoke(list, navModel);
                }

                if (navRuntime.NavWrappers.Count > 0) Deserialize_Navigation(navModel, keyName);
            }
        }

        private static Func<IDataRecord, object> GetDeserializer(Type modelType, IDataRecord reader, IDictionary<string, Column> columns = null, int start = 0, int? end = null)
        {
            string methodName = Guid.NewGuid().ToString();
            DynamicMethod dm = new DynamicMethod(methodName, typeof(object), new[] { typeof(IDataRecord) }, true);
            ILGenerator g = dm.GetILGenerator();
            TypeRuntimeInfo runtime = TypeRuntimeInfoCache.GetRuntimeInfo(modelType);

            var model = g.DeclareLocal(modelType);
            g.Emit(OpCodes.Newobj, runtime.ConstructInvoker.Constructor);
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

                var wrapper = runtime.GetWrapper(keyName);// as MemberAccessWrapper;
                if (wrapper == null) continue;

                var isDBNullLabel = g.DefineLabel();
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, index);
                g.Emit(OpCodes.Callvirt, _isDBNull);
                g.Emit(OpCodes.Brtrue, isDBNullLabel);

                // member的类型可能与数据库中查出来的数据类型不一样
                // 如 boolean，数据库类型 是int
                var m_method = GetReaderMethod(reader.GetFieldType(index));
                g.Emit(OpCodes.Ldloc, model);
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, index);
                g.Emit(OpCodes.Callvirt, m_method);

                Type memberType = wrapper.DataType;
                Type nullUnderlyingType = memberType.IsGenericType ? Nullable.GetUnderlyingType(memberType) : null;
                Type unboxType = nullUnderlyingType != null ? nullUnderlyingType : memberType;

                if (unboxType == typeof(byte[]) || (m_method == _getValue && memberType != typeof(object)))
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

            g.Emit(OpCodes.Ldloc, model);
            g.Emit(OpCodes.Ret);

            var func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
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
