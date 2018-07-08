using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace XFramework.DataAccess
{
    [Serializable]
    public class Command
    {
        //<Command>
        //    <Key>Delete</Key>
        //    <Text>
        //    DELETE [SD_Pur_OrderMaster]
        //    WHERE 1=1 
        //    </Text>
        //</Command>

        #region 私有变量

        #endregion

        #region 公开属性

        /// <summary>
        /// SQL键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 指定如何解释命令字符串
        /// </summary>
        public Nullable<System.Data.CommandType> CommandType { get; set; }

        /// <summary>
        /// SQL脚本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// SQL参数
        /// </summary>
        public List<Parameter> Parameters { get; set; }

        ///// <summary>
        ///// 映射文件
        ///// </summary>
        //[XmlIgnore]
        //public IEnumerable<Parameter> Paramable { get; set; }

        /// <summary>
        /// 映射文件
        /// </summary>
        [XmlIgnore]
        public byte[] ByteArray { get; set; }
        
        ///// <summary>
        ///// 映射文件
        ///// </summary>
        //[XmlIgnore]
        //public System.Reflection.MemberInfo[] MemberArray { get; set; }

        /// <summary>
        /// 映射文件
        /// </summary>
        [XmlIgnore]
        public EntityMapper Mapper { get; set; }

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
