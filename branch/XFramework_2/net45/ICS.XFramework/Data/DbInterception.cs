using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 拦截器
    /// </summary>
    public static class DbInterception
    {
        static List<IDbCommandInterceptor> _interceptors = new List<IDbCommandInterceptor>();
        /// <summary>
        /// 拦截器集合
        /// </summary>
        public static List<IDbCommandInterceptor> Interceptors
        {
            get { return _interceptors; }
        }

        /// <summary>
        /// 注册一个拦截器
        /// </summary>
        public static void Add(IDbCommandInterceptor interceptor)
        {
            _interceptors.Add(interceptor);
        }

        /// <summary>
        /// 移除一个拦截器
        /// </summary>
        public static void Remove(IDbCommandInterceptor interceptor)
        {
            _interceptors.Remove(interceptor);
        }

        /// <summary>
        /// 执行拦截器
        /// </summary>
        public static void Execute(System.Data.IDbCommand cmd, bool status, Exception e)
        {
            foreach (var interceptor in _interceptors) interceptor.DbCommandExecuted(cmd, status, e);
        }
    }
}
