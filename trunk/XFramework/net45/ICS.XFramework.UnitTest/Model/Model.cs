
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICS.XFramework.Data;

namespace ICS.XFramework.UnitTest
{
    // 说明：
    // 1.如果类有 TableAttribute，则用 TableAttribute 指定的名称做为表名，否则用类名称做为表名
    // 2.删除/更新时如果传递的参数是一个实体，必须使用 [Column(IsKey = true)] 指定实体的主键
    // 3.ForeignKeyAttribute 指定外键，一对多关系未实现，但可以用其它方法变通，示例中会给出
    // 4.支持原汁原味的LINQ语法，Lambda表达式

    // [Table]特性说明
    // 若类指定 TableAttribute，则表名取 TableAttribute.Name，否则表名取 类名称

    // [Column]特性说明
    // 若属性指定 ColumnAttribute.NoMapped，则在生成 INSERT 和 UPDATE 语句时会忽略这个字段

    // 代码生成模板说明

    public enum State
    {
        None = 0,
        OK = 1
    }
    public class Inte_CRM
    {
        /// <summary>
        /// sys_demo
        /// </summary>
        [Table(Name = "Sys_Demo")]
        public partial class Demo
        {
            #region 构造函数

            public Demo()
            {

            }

            public Demo(int demoId, string demoName)
            {

            }

            public Demo(Demo model)
            {

            }

            #endregion

            /// <summary>
            /// demoid
            /// </summary>
            [Column(IsKey = true)]
            public int DemoId { get; set; }

            [Column(NoMapped = true)]
            public bool Editable { get; set; }

            /// <summary>
            /// democode
            /// </summary>
            public string DemoCode
            {
                get;
                set;
            }

            /// <summary>
            /// demoname
            /// </summary>
            public string DemoName
            {
                get;
                set;
            }

            /// <summary>
            /// demochar
            /// </summary>
            public string DemoChar
            {
                get;
                set;
            }

            /// <summary>
            /// demochar_nullable
            /// </summary>
            public string DemoChar_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demobyte
            /// </summary>
            public byte DemoByte
            {
                get;
                set;
            }

            /// <summary>
            /// demobyte_nullable
            /// </summary>
            public Nullable<byte> DemoByte_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demodatetime
            /// </summary>
            public DateTime DemoDateTime
            {
                get;
                set;
            }

            /// <summary>
            /// demodatetime_nullable
            /// </summary>
            public Nullable<DateTime> DemoDateTime_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demodecimal
            /// </summary>
            public decimal DemoDecimal
            {
                get;
                set;
            }

            /// <summary>
            /// demodecimal_nullable
            /// </summary>
            public Nullable<decimal> DemoDecimal_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demofloat
            /// </summary>
            public double DemoFloat
            {
                get;
                set;
            }

            /// <summary>
            /// demofloat_nullable
            /// </summary>
            public Nullable<double> DemoFloat_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demoreal
            /// </summary>
            public float DemoReal
            {
                get;
                set;
            }

            /// <summary>
            /// demo_nullable
            /// </summary>
            public Nullable<float> Demo_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demoguid
            /// </summary>
            public Guid DemoGuid
            {
                get;
                set;
            }

            /// <summary>
            /// demoguid_nullable
            /// </summary>
            public Nullable<Guid> DemoGuid_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demoshort
            /// </summary>
            public short DemoShort
            {
                get;
                set;
            }

            /// <summary>
            /// demoshort_nullable
            /// </summary>
            public Nullable<short> DemoShort_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demoint
            /// </summary>
            public int DemoInt
            {
                get;
                set;
            }

            /// <summary>
            /// demoint_nullable
            /// </summary>
            public Nullable<int> DemoInt_Nullable
            {
                get;
                set;
            }

            /// <summary>
            /// demolong
            /// </summary>
            public long DemoLong
            {
                get;
                set;
            }

            /// <summary>
            /// demolong_nullable
            /// </summary>
            public Nullable<long> DemoLong_Nullable
            {
                get;
                set;
            }
            [ForeignKey("ClientId")]
            public ICollection<Client> Buyers { get; set; }
        }

        public class CRM_SaleOrder
        {
            #region 构造函数

            public CRM_SaleOrder()
            {

            }

            public CRM_SaleOrder(int orderId, string orderNo)
            {

            }

            public CRM_SaleOrder(CRM_SaleOrder model)
            {

            }

            #endregion

            /// <summary>
            /// orderid
            /// </summary>
            [Column(IsKey = true)]
            public int OrderId
            {
                get;
                set;
            }

            /// <summary>
            /// orderno
            /// </summary>
            public string OrderNo
            {
                get;
                set;
            }

            public int ClientId { get; set; }

            [ForeignKey("ClientId")]
            public Client Client { get; set; }

            [ForeignKey("ClientId")]
            public Client HeavyBuyer { get; set; }

        }

        [Table(Name = "Bas_Client")]
        public class Client
        {
            #region 构造函数

            public Client()
            {

            }

            public Client(Client model)
            {

            }

            #endregion

            [Column(IsKey = true)]
            public int ClientId { get; set; }

            public string ClientCode { get; set; }

            public string ClientName { get; set; }

            public string Remark { get; set; }

            public byte State { get; set; }

            public DateTime? ActiveDate { get; set; }

            public int CloudServerId { get; set; }

            [ForeignKey("CloudServerId")]
            public CloudServer CloudServer { get; set; }

            [ForeignKey("ClientId")]
            public List<Account> AccountList { get; set; }
        }

        [Table(Name = "Bas_ClientAccount")]
        public class Account
        {
            public Account()
            {

            }

            public Account(Account model)
            {

            }

            [Column(IsKey = true)]
            public int ClientId { get; set; }

            [ForeignKey("ClientId")]
            public Client Client { get; set; }

            [Column(IsKey = true)]
            public string AccountId { get; set; }

            public string AccountCode { get; set; }

            public string AccountName { get; set; }
        }

        [Table(Name = "Sys_CloudServer")]
        public class CloudServer
        {
            [Column(IsKey = true)]
            public int CloudServerId { get; set; }

            public string CloudServerCode { get; set; }

            public string CloudServerName { get; set; }
        }


        [Table(Name = "Sys_Thin")]
        public class Thin
        {
            [Column(IsKey = true)]
            public int ThinId { get; set; }

            public string ThinName { get; set; }
        }

        [Table(Name = "Sys_ThinIdentity")]
        public class ThinIdentity
        {
            [Column(IsKey = true, IsIdentity = true)]
            public int ThinId { get; set; }

            public string ThinName { get; set; }
        }

        [Table(Name = "Sys_User")]
        public class User
        {
            [Column(IsKey = true)]
            public int UserId { get; set; }

            public string UserName { get; set; }

            [ForeignKey("UserId")]
            public List<UserRole> Roles { get; set; }


            [ForeignKey("UserId")]
            public List<UserModule> Modules { get; set; }
        }

        [Table(Name = "Sys_UserRole")]
        public class UserRole
        {
            [Column(IsKey = true)]
            public int UserId { get; set; }

            [Column(IsKey = true)]
            public int RoleId { get; set; }

            [ForeignKey(new string[] { "UserId", "RoleId" })]
            public List<UserRoleModule> RoleModules { get; set; }
        }


        [Table(Name = "Sys_UserRoleModule")]
        public class UserRoleModule
        {
            [Column(IsKey = true)]
            public int UserId { get; set; }

            [Column(IsKey = true)]
            public int RoleId { get; set; }

            [Column(IsKey = true)]
            public int ModuleId { get; set; }
        }

        [Table(Name = "Sys_UserModule")]
        public class UserModule
        {
            [Column(IsKey = true)]
            public int UserId { get; set; }

            [Column(IsKey = true)]
            public int ModuleId { get; set; }
        }
    }


    public class Prd_Center
    {
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

            [ForeignKey("ClientID")]
            public virtual Client Client { get; set; }
        }

        public class Client
        {
            public Client()
            {

            }

            public Client(Client model)
            {

            }

            public int ClientID { get; set; }

            public string ClientCode { get; set; }

            public string ClientName { get; set; }
        }
    }

    /// <summary>
    /// 单据状态
    /// </summary>
    public enum OrderState
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None = 0,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 1,

        /// <summary>
        /// 未支付
        /// </summary>
        NoPay = 2,

        /// <summary>
        /// 未激活
        /// </summary>
        WaitActive = 10,

        /// <summary>
        /// 未使用
        /// </summary>
        Ready = 20,

        /// <summary>
        /// 使用中
        /// </summary>
        Inuse = 25,

        /// <summary>
        /// 已过期
        /// </summary>
        Expired = 30,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancel = 35
    }

    public enum AllowusedState
    {
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// 启用
        /// </summary>
        Enabled = 1,

        /// <summary>
        /// 删除
        /// </summary>
        Delete = 2
    }
}
