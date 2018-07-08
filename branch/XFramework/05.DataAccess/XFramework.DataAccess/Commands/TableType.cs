using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace XFramework.DataAccess
{
    [Serializable]
    public class TableType
    {
        
        //<TableType>
        //    <TableName><%= SourceTable.Name %></TableName>
        //    <TypeFullName><%= string.Concat(NameSpace,".",Common.GetEntityName(SourceTable)) %></TypeFullName>
        //</TableType>

        #region 私有变量

        #endregion

        #region 公开属性

        public string TableName { get; set; }

        public string TypeFullName { get; set; }

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
}
