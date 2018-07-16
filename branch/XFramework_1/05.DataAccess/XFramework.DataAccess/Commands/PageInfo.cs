using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace XFramework.DataAccess
{
    public class PageInfo
    {
        #region 私有变量

        private int _pageSize = 20;
        private int _currentPage = 1;

        #endregion

        #region 公开属性

        /// <summary>
        /// 页长
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value should large than zero");
                _pageSize = value;
            }
        }

        /// <summary>
        /// 当前面
        /// </summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value should large than zero");
                _currentPage = value;
            }
        }

        #endregion

        #region 构造函数

        public PageInfo(int curPage, int pageSize)
        {
            this.CurrentPage = curPage;
            this.PageSize = pageSize;
        }

        #endregion

        #region 重写方法

        #endregion

        #region 公开方法

        #endregion

        #region 辅助方法

        #endregion
    }
}
