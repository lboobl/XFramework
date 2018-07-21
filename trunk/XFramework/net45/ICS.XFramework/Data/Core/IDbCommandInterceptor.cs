using System.Data;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 数据库命令执行拦截器
    /// </summary>
    public interface IDbCommandInterceptor
    {
        /// <summary>
        /// 执行 SQL 命令后
        /// </summary>
        /// <param name="cmd">SQL 命令</param>
        /// <param name="e">事件参数</param>
        void DbCommandExecuted(System.Data.IDbCommand cmd, bool status, System.Exception e);
    }
}
