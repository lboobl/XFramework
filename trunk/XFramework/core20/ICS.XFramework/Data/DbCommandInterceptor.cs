using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 数据库命令执行拦截器
    /// </summary>
    public class DbCommandInterceptor : IDbCommandInterceptor
    {
        Action<System.Data.IDbCommand, bool, Exception> _action = null;

        /// <summary>
        /// 实例化 <see cref="DbCommandInterceptor"/> 类的新实例
        /// </summary>
        /// <param name="action">拦截动作</param>
        public DbCommandInterceptor(Action<System.Data.IDbCommand, bool, Exception> action)
        {
            _action = action;
        }

        /// <summary>
        /// 执行 SQL 命令后
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="status">执行状态</param>
        /// <param name="e">异常</param>
        public virtual void DbCommandExecuted(System.Data.IDbCommand cmd, bool status, Exception e)
        {
            if (_action != null) _action(cmd, status, e);
        }
    }
}
