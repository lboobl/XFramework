using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using XFramework.DataAccess.Dapper;

namespace XFramework.UnitTest
{
    public class DapperDemo
    {
        public static void Run()
        {
            IDbConnection conn = new SqlConnection("Server=192.168.32.170;Database=ICSDB;uid=testuser;pwd=123456");
            List<CRM_SaleOrder> sales = new List<CRM_SaleOrder>();
            using (conn)
            {
                string sqlCommandText = @"
SELECT 
t0.[ProductID] AS [ProductID],
t0.[SKU] AS [SKU],
t0.[Title] AS [Title],
t0.[ClientID] AS [ClientID]
FROM [Product] t0 
WHERE t0.[ClientID] = 1";

                sqlCommandText = @"
SELECT 
t0.[OrderId] AS [OrderId],
t0.[ClientID] AS [ClientID],
0 as Editable
FROM [CRM_SaleOrder] t0";

                System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                stop.Start();
                for (int i = 0; i < 1000; i++)
                {
                  var v= conn.Query<CRM_SaleOrder>(sqlCommandText);//.ToList();
                    //sales = conn.Query<CRM_SaleOrder, Bas_Client, CRM_SaleOrder>(
                    //    sqlCommandText, (sale, client) => { sale.Client = client; return sale; }, null, null, true, "ClientId", null, null).ToList();
                }
                stop.Stop();
            }
        }

        public class CRM_SaleOrder
        {
            public CRM_SaleOrder()
            { }
            public CRM_SaleOrder(CRM_SaleOrder a)
            { }

            public int? ClientId { get; set; }

            public int OrderId { get; set; }
            public bool Editable { get; set; }

            public string OrderNo { get; set; }

            public string Remark { get; set; }

            public string ClientName { get; set; }

            //public Bas_Client Client { get; set; }
        }

        public class Client
        {
            public Client()
            {

            }

            public Client(Client model)
            {

            }

            //[Column(IsKey = true)]
            public int ClientID { get; set; }

            public string ClientCode { get; set; }

            public string ClientName { get; set; }

            //public byte State { get; set; }

            //public DateTime? ActiveDate { get; set; }

            //public int CloudServerId { get; set; }

            //[ForeignKey("CloudServerId")]
            //public CloudServer CloudServer { get; set; }

            //[ForeignKey("ClientId")]
            //public virtual ICollection<Account> Accounts { get; set; }
        }


        public class Product
        {
            public Product()
            {

            }

            public Product(Product model)
            {

            }

            public int ProductID { get; set; }

            public string SKU { get; set; }

            public string Title { get; set; }

            public int ClientID { get; set; }

            //[ForeignKey("ClientId")]
            //public Client Client { get; set; }
        }

        public class Sys_CloudServer
        {
            public int CloudServerId { get; set; }

            public string CloudServerName { get; set; }
        }
    }
}
