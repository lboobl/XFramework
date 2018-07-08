using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace XFramework.DataAccess
{
    [Serializable]
    public class Sequence
    {
        #region 私有变量

        private int _value;

        #endregion

        #region 公开属性

        public int Value
        {
            get { return _value; }
            set { if (value < 0) throw new ArgumentOutOfRangeException("value should equals or large than zero");
            _value = value;
            }
        }

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
