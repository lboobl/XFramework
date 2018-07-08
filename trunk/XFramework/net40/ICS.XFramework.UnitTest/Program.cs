
using ICS.XFramework.Data;

namespace ICS.XFramework.UnitTest
{
    public class Program
    {
        public static void Main()
        {
            string connString = XCommon.GetConnString("XFrameworkConnString");
            XfwContainer.Default.Register<IDbQueryProvider>(() => new ICS.XFramework.Data.SqlClient.DbQueryProvider(connString), true);
            Demo.Run();
        }
    }
}
