using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFramework.DataAccess
{
    [Serializable]
    public class Parameter : Property
    {
        //<Parameter>
        //    <Name>ID</Name>
        //    <DbType>Int32</DbType>
        //    <NativeType>int</NativeType>
        //    <Precision>10</Precision>
        //    <Scale>0</Scale>
        //    <Size>4</Size>
        //</Parameter>
        //string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null

        #region 私有变量

        #endregion

        #region 公开属性

        public object Value { get; set; }

        public Nullable<System.Data.ParameterDirection> Direction { get; set; }

        #endregion

        #region 构造函数

        #endregion

        #region 重写方法

        #endregion

        #region 公开方法

        #endregion

        #region 辅助方法

        #endregion
    }

    public class ParameterComparer : IEqualityComparer<Parameter>
    {

        public bool Equals(Parameter x, Parameter y)
        {
            return x != null && y != null && string.Compare(x.Name, y.Name, true) == 0;
        }

        public int GetHashCode(Parameter obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
