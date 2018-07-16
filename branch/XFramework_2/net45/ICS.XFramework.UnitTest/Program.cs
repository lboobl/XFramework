
using ICS.XFramework.Data;

using System.Data.SqlClient;

namespace ICS.XFramework.UnitTest
{
    public class Program
    {
        public static void Main()
        {
            string connString = XCommon.GetConnString("XFrameworkConnString");
            XfwContainer.Default.Register<IDbQueryProvider>(() => new ICS.XFramework.Data.SqlClient.DbQueryProvider(connString), true);
            //DbInterception.Add(new DbCommandInterceptor((cmd,e)=> 
            //{
            //    var a = cmd;
            //}));
            Demo.Run();
        }
    }
}
