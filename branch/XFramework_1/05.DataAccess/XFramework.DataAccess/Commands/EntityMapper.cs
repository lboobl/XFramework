using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace XFramework.DataAccess
{
    [Serializable]
    public class EntityMapper
    {
        #region 私有变量

        #endregion

        #region 公开属性

        /// <summary>
        /// 表
        /// </summary>
        public TableType TableType { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public List<Property> Properties { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public List<Property> Keys { get; set; }

        /// <summary>
        /// 自增列
        /// </summary>
        public Property Identity { get; set; }

        /// <summary>
        /// SQL
        /// </summary>
        public List<Command> Commands { get; set; }

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
