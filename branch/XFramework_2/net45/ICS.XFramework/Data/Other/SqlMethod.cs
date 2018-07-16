using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 定义
    /// </summary>
    public class SqlMethod
    {
        /// <summary>
        /// 行号
        /// </summary>
        public static int RowNumber(Expression<Func<object, object>> order)
        {
            return -1;
        }
    }
}
