﻿using System;
using System.Collections.Generic;
using System.Data;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 运行时类型工具类
    /// </summary>
    public class TypeUtils
    {
        static HashSet<Type> _primitiveTypes = new HashSet<Type>();
        static HashSet<Type> _numericTypes = new HashSet<Type>();

        static TypeUtils()
        {
            _primitiveTypes.Add(typeof(string));
            _primitiveTypes.Add(typeof(byte[]));
            _primitiveTypes.Add(typeof(bool));
            _primitiveTypes.Add(typeof(Nullable<bool>));
            _primitiveTypes.Add(typeof(byte));
            _primitiveTypes.Add(typeof(Nullable<byte>));
            _primitiveTypes.Add(typeof(DateTime));
            _primitiveTypes.Add(typeof(Nullable<DateTime>));
            _primitiveTypes.Add(typeof(decimal));
            _primitiveTypes.Add(typeof(Nullable<decimal>));
            _primitiveTypes.Add(typeof(double));
            _primitiveTypes.Add(typeof(Nullable<double>));
            _primitiveTypes.Add(typeof(Guid));
            _primitiveTypes.Add(typeof(Nullable<Guid>));
            _primitiveTypes.Add(typeof(short));
            _primitiveTypes.Add(typeof(Nullable<short>));
            _primitiveTypes.Add(typeof(int));
            _primitiveTypes.Add(typeof(Nullable<int>));
            _primitiveTypes.Add(typeof(long));
            _primitiveTypes.Add(typeof(Nullable<long>));
            _primitiveTypes.Add(typeof(sbyte));
            _primitiveTypes.Add(typeof(Nullable<sbyte>));
            _primitiveTypes.Add(typeof(float));
            _primitiveTypes.Add(typeof(Nullable<float>));
            _primitiveTypes.Add(typeof(ushort));
            _primitiveTypes.Add(typeof(Nullable<ushort>));
            _primitiveTypes.Add(typeof(uint));
            _primitiveTypes.Add(typeof(Nullable<uint>));
            _primitiveTypes.Add(typeof(ulong));
            _primitiveTypes.Add(typeof(Nullable<ulong>));


            _numericTypes.Add(typeof(byte));
            _numericTypes.Add(typeof(Nullable<byte>));
            _numericTypes.Add(typeof(decimal));
            _numericTypes.Add(typeof(Nullable<decimal>));
            _numericTypes.Add(typeof(double));
            _numericTypes.Add(typeof(Nullable<double>));
            _numericTypes.Add(typeof(short));
            _numericTypes.Add(typeof(Nullable<short>));
            _numericTypes.Add(typeof(int));
            _numericTypes.Add(typeof(Nullable<int>));
            _numericTypes.Add(typeof(long));
            _numericTypes.Add(typeof(Nullable<long>));
            _numericTypes.Add(typeof(sbyte));
            _numericTypes.Add(typeof(Nullable<sbyte>));
            _numericTypes.Add(typeof(float));
            _numericTypes.Add(typeof(Nullable<float>));
            _numericTypes.Add(typeof(ushort));
            _numericTypes.Add(typeof(Nullable<ushort>));
            _numericTypes.Add(typeof(uint));
            _numericTypes.Add(typeof(Nullable<uint>));
            _numericTypes.Add(typeof(ulong));
            _numericTypes.Add(typeof(Nullable<ulong>));
        }

        /// <summary>
        /// 判断给定类型是否是ORM支持的基元类型
        /// </summary>
        public static bool IsPrimitive(Type type)
        {
            return _primitiveTypes.Contains(type);
        }

        /// <summary>
        /// 判断给定类型是否是数值类型
        /// </summary>
        public static bool IsNumeric(Type type)
        {
            return _numericTypes.Contains(type);
        }

        /// <summary>
        /// CRL类型 转 DbType
        /// </summary>
        public static DbType ConvertCLRTypeToDbType(Type clrType)
        {
            switch (Type.GetTypeCode(clrType))
            {
                case TypeCode.Empty:
                    throw new ArgumentException(TypeCode.Empty.ToString());

                case TypeCode.Object:
                    if (clrType == typeof(Byte[]))
                    {
                        return DbType.Binary;
                    }
                    if (clrType == typeof(Char[]))
                    {
                        // Always treat char and char[] as string
                        return DbType.String;
                    }
                    else if (clrType == typeof(Guid))
                    {
                        return DbType.Guid;
                    }
                    else if (clrType == typeof(TimeSpan))
                    {
                        return DbType.Time;
                    }
                    else if (clrType == typeof(DateTimeOffset))
                    {
                        return DbType.DateTimeOffset;
                    }

                    return DbType.Object;

                case TypeCode.DBNull:
                    return DbType.Object;
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Char:
                    // Always treat char and char[] as string
                    return DbType.String;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.String:
                    return DbType.String;
                default:
                    throw new XFrameworkException("Unkown type ", clrType.FullName);
            }
        }

        /// <summary>
        /// 是否编译生成的类型
        /// </summary>
        public static  bool IsCompilerGenerated(Type t)
        {
            if (t == null)
                return false;

            return t.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                || IsCompilerGenerated(t.DeclaringType);
        }

        /// <summary>
        /// 返回给定类型的 NULL 值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetNullValue(Type type)
        {
            if (type.IsValueType)
            {
                if (type.IsEnum)
                {
                    return GetNullValue(Enum.GetUnderlyingType(type));
                }

                if (type.IsPrimitive)
                {
                    if (type == typeof(Int32)) { return 0; }
                    if (type == typeof(Double)) { return (Double)0; }
                    if (type == typeof(Int16)) { return (Int16)0; }
                    if (type == typeof(SByte)) { return (SByte)0; }
                    if (type == typeof(Int64)) { return (Int64)0; }
                    if (type == typeof(Byte)) { return (Byte)0; }
                    if (type == typeof(UInt16)) { return (UInt16)0; }
                    if (type == typeof(UInt32)) { return (UInt32)0; }
                    if (type == typeof(UInt64)) { return (UInt64)0; }
                    if (type == typeof(UInt64)) { return (UInt64)0; }
                    if (type == typeof(Single)) { return (Single)0; }
                    if (type == typeof(Boolean)) { return false; }
                    if (type == typeof(char)) { return '\0'; }
                }
                else
                {
                    //DateTime : 01/01/0001 00:00:00
                    //TimeSpan : 00:00:00
                    //Guid : 00000000-0000-0000-0000-000000000000
                    //Decimal : 0

                    if (type == typeof(DateTime)) { return DateTime.MinValue; }
                    if (type == typeof(Decimal)) { return 0m; }
                    if (type == typeof(Guid)) { return Guid.Empty; }
                    if (type == typeof(TimeSpan)) { return new TimeSpan(0, 0, 0); }
                }
            }

            return null;
        }
    }
}
