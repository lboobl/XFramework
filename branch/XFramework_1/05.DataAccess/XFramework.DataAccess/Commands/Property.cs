using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace XFramework.DataAccess
{
    [Serializable]
    public class Property
    {
        //<Name>CompanyID</Name>
        //<DbType>AnsiString</DbType>
        //<NativeType>varchar</NativeType>
        //<Precision>0</Precision>
        //<Scale>0</Scale>
        //<Size>10</Size>

        #region 私有变量

        #endregion

        #region 公开属性

        public string Name { get; set; }

        public DbType DbType { get; set; }

        public string NativeType { get; set; }

        public byte Precision { get; set; }

        public int Scale { get; set; }

        public int Size { get; set; }

        #endregion

        #region 构造函数

        #endregion

        #region 重写方法

        public override string ToString()
        {
            return string.Format("Name:{0},DbType:{1},NativeType:{2},Precision:{3},Scale:{4},Size:{5}",
                Name, DbType, NativeType, Precision, Scale, Size);
        }

        #endregion

        #region 公开方法

        #endregion

        #region 辅助方法

        #endregion
    }
}
