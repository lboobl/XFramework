
using ICS.XFramework.Data;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

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

            for (int i = 0; i < 20; i++)
            {
                Task.Factory.StartNew(() => Demo.Run());
            }
            Console.ReadLine();
        }
    }
}
