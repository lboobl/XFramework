using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using XFramework.DataAccess;
using XFramework.DataAccess.Dapper;
using XFramework.Model;
using XFramework.Core;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using EmitMapper;
using EmitMapper.MappingConfiguration;
using EmitMapper.MappingConfiguration.MappingOperations;
using System.Reflection;
using EmitMapper.Utils;
using System.Linq.Expressions;

using System.Data.Common.CommandTrees;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace XFramework.UnitTest
{
    [TestClass]
    public class Program
    {
        public BindingFlags BINDING_FLAGS_PROPERTY
            = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            ;

        //[TestMethod]
        public static void Main()
        {
            DapperDemo.Run();

            RepositoryBase rptBase = new RepositoryBase();
            //TestResolve(rptBase);

            return;

            //不带事务
            Test(rptBase);
            return;

            //带事务
            using (RepositoryBase rptBase1 = new RepositoryBase())
            {
                rptBase1.Session.BeginTransaction();
                Test(rptBase1);
                rptBase1.Session.Complete();
            }

            //带事务
            try
            {
                rptBase.Session.BeginTransaction();
                Test(rptBase);
                throw new NotSupportedException();
                rptBase.Session.CommitTransaction();
            }
            catch
            {
                rptBase.Session.RollBackTransaction();
            }
            finally
            {
                rptBase.Session.CloseConnection();
            }
        }

        public static void Test(RepositoryBase rptBase)
        {
            //查询
            IEnumerable<Bas_Company> query = null;
            DynamicParameters d = null;

            //LinQ 语法查询
            query = rptBase.Query<Bas_Company>();
            query = rptBase.Query<Bas_Company>(x => true);
            query = rptBase.Query<Bas_Company>(x => true && (x.CompanyID ?? null) != null && new[] { "1", "2" }.Contains(x.CompanyID) &&
                x.CompanyID.Substring(2, 5).TrimEnd() == "OK" && x.AllowUsed);
            
            //分页查询
            query = rptBase.Query<Bas_Company>(new PageInfo(3, 20));
            query = rptBase.Query<Bas_Company>(new PageInfo(3, 20), x => x.CompanyID == "FT");

            //自定义脚本查询
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司", DbType.String, null, 20);
            query = rptBase.Query<Bas_Company>("Select * From Bas_Company WHERE CompanyName = @CompanyName", d);
             
            //自定义参数查询
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司");
            query = rptBase.Query<Bas_Company>("selectByName", null, d);

            //带返回值查询
            d = new DynamicParameters();
            d.Add("Row", null);
            query = rptBase.Query<Bas_Company>("returnValue", x => x.CompanyID != "FT", d);
            query.Count(); //带返回值时必须执行查询，否则有异常
            int? eff = d.Get<int?>("Row");

            //查询自定义实体
            var query1 = rptBase.Query<ThinEntity>(typeof(Bas_Company).FullName, "thinEntity", "And CompanyID <> 'FT' ");
            
            //删除
            rptBase.Delete<Bas_Company>(x => x.CompanyID == "TH");

    //新增
    Bas_Company company = new Bas_Company();
    company.CompanyID = "TH";
    company.CompanyCode = "TH001";
    company.CompanyName = "TH001";
    company.Level = 0;
    company.IsDetail = false;
    company.IsAccount = false;
    company.IsHold = false;
    company.ParentID = "";
    company.FullName = "TH001";
    company.FullParentID = "";
    company.ModifyDTM = DateTime.Now;
    company.AllowUsed = true;
    rptBase.Insert(company);

    //修改
    company.CompanyCode = "TH00x";
    rptBase.Update(company);
    //批量修改
    rptBase.Update<Bas_Company>(x => new Bas_Company { CompanyCode = "TH003" }, x => x.CompanyID == "TH");

    //删除
    rptBase.Delete(company);

            DataTable table = null;
            table = rptBase.QueryDataTable<Bas_Company>();
            table = rptBase.QueryDataTable<Bas_Company>(x => true);
            table = rptBase.QueryDataTable<Bas_Company>(x => true && (x.CompanyID ?? null) != null && new[] { "1", "2" }.Contains(x.CompanyID) &&
                x.CompanyID.Substring(2, 5).TrimEnd() == "OK" && x.AllowUsed);
            table = rptBase.QueryDataTable<Bas_Company>(new PageInfo(3, 20));
            table = rptBase.QueryDataTable<Bas_Company>(new PageInfo(3, 20), x => x.CompanyID == "FT");

            //自定义脚本查询
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司", DbType.String, null, 20);
            table = rptBase.QueryDataTable("Select * From Bas_Company WHERE CompanyName = @CompanyName", d);

            //传递参数查询
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司");
            table = rptBase.QueryDataTable<Bas_Company>("selectByName", null, d);

            //带返回值查询
            d = new DynamicParameters();
            d.Add("Row", null);
            table = rptBase.QueryDataTable<Bas_Company>("returnValue", x => x.CompanyID != "FT", d);
            eff = d.Get<int?>("Row");
            
            //查询自定义实体
            table = rptBase.QueryDataTable(typeof(Bas_Company).FullName, "thinEntity", "And CompanyID <> 'FT' ");

            DataSet data = null;
            data = rptBase.QueryDataSet<Bas_Company>("Select",x => true);
            data = rptBase.QueryDataSet<Bas_Company>("Select", x => true && (x.CompanyID ?? null) != null && new[] { "1", "2" }.Contains(x.CompanyID) &&
                x.CompanyID.Substring(2, 5).TrimEnd() == "OK" && x.AllowUsed);
            data = rptBase.QueryDataSet(typeof(Bas_Company).FullName, "thinEntity", "And CompanyID <> 'FT' ");

            //自定义脚本查询
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司", DbType.String, null, 20);
            data = rptBase.QueryDataSet("Select * From Bas_Company WHERE CompanyName = @CompanyName", d);

            //传递参数查询
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司");
            data = rptBase.QueryDataSet<Bas_Company>("selectByName", null, d);

            //带返回值查询
            d = new DynamicParameters();
            d.Add("Row", null);
            data = rptBase.QueryDataSet<Bas_Company>("returnValue", x => x.CompanyID != "FT", d);
            eff = d.Get<int?>("Row");

            SqlMapper.GridReader reader = null;
            d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司", DbType.String, null, 20);
            d.Add("CompanyCode", "JDH", DbType.String, null, 20);
            reader = rptBase.QueryMultiple<Bas_Company>("selectMultiple", d);
            query = reader.Read<Bas_Company>();
            query = reader.Read<Bas_Company>();

            reader = rptBase.QueryMultiple(typeof(Bas_Company).FullName, "selectMultiple", null, d);
            query = reader.Read<Bas_Company>();
            query = reader.Read<Bas_Company>();
            
            d = new DynamicParameters();
            d.Add("x0", "JDH");
            d.Add("x1", "JDH");
            rptBase.Execute<Bas_Company>("updateName", x => true, d);


            //存储过程，单表用QueryDataTable，多结果集用QueryDataSet
            //若需要返回强类型也可用Query<T>或者QueryMultiple
            d = new DynamicParameters();
            d.Add("OrderNo", "N2",DbType.AnsiString,null,100);
            d.Add("CompanyID", "MZY", DbType.AnsiString, null, 10);
            data = rptBase.QueryDataSet("spSD_Barcode", d, CommandType.StoredProcedure);

            //有返回值的存储过程
            d = new DynamicParameters();
            d.Add("CompanyID", "MZY", DbType.AnsiString, null, 10);
            d.Add("TableName", "Bas_Company", DbType.AnsiString, null, 50);
            d.Add("KeyField", "CompanyID", DbType.AnsiString, null, 50);
            d.Add("Digits", 8, DbType.Int16, null, 4);
            d.Add("MaxID", null, DbType.AnsiString, ParameterDirection.Output, 40);
            rptBase.QueryDataSet("spSys_GetNextID", d, CommandType.StoredProcedure);

            string next = d.Get<string>("MaxID");


        }

        public static void TestResolve(RepositoryBase rptBase)
        {
            List<string> sqlList = new List<string>();
            string sql = null;

            sql = rptBase.Resolve<Bas_Company>("Select");
            sqlList.Add(sql);

            sql = rptBase.Resolve<Bas_Company>("Select",x => true && (x.CompanyID ?? null) != null && new[] { "1", "2" }.Contains(x.CompanyID) &&
                x.CompanyID.Substring(2, 5).TrimEnd() == "OK" && x.AllowUsed);
            sqlList.Add(sql);

            sql = rptBase.Resolve<Bas_Company>(new PageInfo(3, 20), x => x.CompanyID == "FT");
            sqlList.Add(sql);

            DynamicParameters d = new DynamicParameters();
            d.Add("CompanyName", "美之源科技有限公司");
            sql = rptBase.Resolve<Bas_Company>("selectByName", null, d);
            sqlList.Add(sql);

            d = new DynamicParameters();
            d.Add("Row", null);
            sql = rptBase.Resolve<Bas_Company>("returnValue", x => x.CompanyID != "FT", d);
            sqlList.Add(sql);

            d = new DynamicParameters();
            d.Add("Row", null);
            sql = rptBase.Resolve<Bas_Company>("returnValue", x => x.CompanyID != "FT", d);
            sqlList.Add(sql);

            //解析出脚本后一次性执行...
            rptBase.Execute(sqlList);
        }
    }

    public class ThinEntity
    {
        public string CompanyID { get; set; }

        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }
    }
}


