using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 定义 ORM 所支持的 SQL 方法
    /// </summary>
    public class SqlMethod
    {
        /// <summary>
        /// 解析成行号
        /// </summary>
        public static int RowNumber(Expression<Func<object, object>> order)
        {
            return -1;
        }
    }
}
