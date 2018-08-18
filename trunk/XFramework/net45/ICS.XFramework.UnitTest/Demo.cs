using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

using ICS.XFramework;
using ICS.XFramework.Data;

namespace ICS.XFramework.UnitTest
{

    public class Demo
    {
        static string _demoName = "002F";
        static int[] _demoIdList = new int[] { 2, 3 };

        public static void Run()
        {
            //Query();
            //Join();
            //Other();
            Performance();
        }

        // 单表查询
        static void Query()
        {
            var context = new DataContext();

            // 查询表达式
            var query =
                from a in context.GetTable<Model.Demo>()
                where a.DemoByte_Nullable != null &&
                      a.DemoName.Contains("'--") &&
                      a.DemoName.StartsWith("'--") &&
                      a.DemoName.EndsWith("'--") &&
                      a.DemoByte == (byte)State.Complete &&
                      a.DemoId == new List<int> { 1 }[0]
                select a;
            var r1 = query.ToList();
            // 点标记
            query = context.GetTable<Model.Demo>();
            r1 = query.ToList();
            //SQL=> 
            //SELECT
            //t0.[DemoId] AS[DemoId],
            //t0.[DemoCode] AS[DemoCode],
            //t0.[DemoName] AS[DemoName],
            //...
            //FROM[Sys_Demo] t0
            //WHERE t0.[DemoByte_Nullable] IS NOT NULL AND t0.[DemoName] LIKE N'%''--%' AND t0.[DemoName] LIKE N'''--%' AND t0.[DemoName] LIKE N'%''--' AND t0.[DemoByte] = 1 AND t0.[DemoId] = 1

            // 构造函数
            query =
                 from a in context.GetTable<Model.Demo>()
                 select new Model.Demo(a);
            r1 = query.ToList();
            query =
               from a in context.GetTable<Model.Demo>()
               select new Model.Demo(a.DemoId, a.DemoName);
            r1 = query.ToList();
            //SQL=> 
            //SELECT 
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoName] AS [DemoName]
            //FROM [Sys_Demo] t0 

            // 指定字段
            query = from a in context.GetTable<Model.Demo>()
                    select new Model.Demo
                    {
                        DemoCode = a.DemoCode,
                        DemoDateTime_Nullable = a.DemoDateTime_Nullable
                    };
            r1 = query.ToList();
            // 点标记
            query = context
                .GetTable<Model.Demo>()
                .Select(a => new Model.Demo
                {
                    DemoCode = a.DemoCode.ToString(),
                    DemoDateTime_Nullable = a.DemoDateTime_Nullable
                });
            r1 = query.ToList();
            //SQL=> 
            //SELECT 
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoDateTime_Nullable] AS [DemoDateTime_Nullable]
            //FROM [Sys_Demo] t0 

            // 匿名类
            var query_dynamic = from a in context.GetTable<Model.Demo>()
                                select new
                                {
                                    DemoCode = a.DemoCode,
                                    DemoDateTime_Nullable = a.DemoDateTime_Nullable
                                };
            var r2 = query_dynamic.ToList();
            // 点标记
            query_dynamic = context
                .GetTable<Model.Demo>()
                .Select(a => new
                {
                    DemoCode = a.DemoCode,
                    DemoDateTime_Nullable = a.DemoDateTime_Nullable
                });
            r2 = query_dynamic.ToList();
            //SQL=> 
            //SELECT 
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoDateTime_Nullable] AS [DemoDateTime_Nullable]
            //FROM [Sys_Demo] t0 

            //分页查询（非微软api）
            query = from a in context.GetTable<Model.Demo>()
                    select a;
            var r3 = query.ToPagedList(1, 20);
            //SQL=>
            //SELECT TOP(20)
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoName] AS [DemoName],
            //...
            //t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //FROM [Sys_Demo] t0 
            query = context.GetTable<Model.Demo>();
            r3 = query.OrderBy(a => a.DemoDecimal).ToPagedList(2, 1);
            //SQL=>
            //SELECT
            //t0.[DemoId] AS[DemoId],
            //t0.[DemoCode] AS[DemoCode],
            //t0.[DemoName] AS[DemoName],
            //...
            //FROM[Sys_Demo] t0
            //ORDER BY t0.[DemoDecimal]
            //OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY

            // 分页查询
            // 1.不是查询第一页的内容时，必须先OrderBy再分页，OFFSET ... Fetch Next 分页语句要求有 OrderBy
            // 2.OrderBy表达式里边的参数必须跟query里边的变量名一致，如此例里的 a。SQL解析时根据此变更生成表别名
            query = from a in context.GetTable<Model.Demo>()
                    orderby a.DemoCode
                    select a;
            query = query.Skip(1).Take(18);
            r1 = query.ToList();
            // 点标记
            query = context
                .GetTable<Model.Demo>()
                .OrderBy(a => a.DemoCode)
                .Skip(1)
                .Take(18);
            r1 = query.ToList();
            //SQL=>
            //SELECT 
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoName] AS [DemoName],
            //...
            //t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //FROM [Sys_Demo] t0 
            //ORDER BY t0.[DemoCode]
            //OFFSET 1 ROWS FETCH NEXT 18 ROWS ONLY

            query =
                from a in context.GetTable<Model.Demo>()
                orderby a.DemoCode
                select a;
            query = query.Skip(1);
            r1 = query.ToList();
            //SQL=>
            //SELECT 
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoName] AS [DemoName],
            //...
            //t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //FROM [Sys_Demo] t0 
            //ORDER BY t0.[DemoCode]
            //OFFSET 1 ROWS

            query =
                from a in context.GetTable<Model.Demo>()
                orderby a.DemoCode
                select a;
            query = query.Take(1);
            r1 = query.ToList();
            //SQL=>
            //SELECT TOP(1)
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoName] AS [DemoName],
            //...
            //t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //FROM [Sys_Demo] t0 
            //ORDER BY t0.[DemoCode]

            // 分页后查查询，结果会产生嵌套查询
            query =
                from a in context.GetTable<Model.Demo>()
                orderby a.DemoCode
                select a;
            query = query.Skip(1);
            query = query.Where(a => a.DemoId > 0);
            query = query.OrderBy(a => a.DemoCode).Skip(1).Take(1);
            r1 = query.ToList();
            //SQL=>
            //SELECT 
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoName] AS [DemoName],
            //...
            //t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //FROM (
            //    SELECT 
            //    t0.[DemoId] AS [DemoId],
            //    t0.[DemoCode] AS [DemoCode],
            //    t0.[DemoName] AS [DemoName],
            //    ...
            //    t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //    FROM [Sys_Demo] t0 
            //    ORDER BY t0.[DemoCode]
            //    OFFSET 1 ROWS
            //) t0 
            //WHERE t0.[DemoId] > 0
            //ORDER BY t0.[DemoCode]
            //OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY 

            // 过滤条件
            query = from a in context.GetTable<Model.Demo>()
                    where a.DemoName == "002"
                    select a;
            r1 = query.ToList();
            // 点标记
            query = context.GetTable<Model.Demo>().Where(a => a.DemoName == "002");
            r1 = query.ToList();
            //SQL=>
            //SELECT 
            //t0.[DemoId] AS [DemoId],
            //t0.[DemoCode] AS [DemoCode],
            //t0.[DemoName] AS [DemoName],
            //...
            //t0.[DemoLong_Nullable] AS [DemoLong_Nullable]
            //FROM [Sys_Demo] t0 
            //WHERE t0.[DemoName] = N'002'

            // 支持的查询条件
            int m_byte = 9;
            query = from a in context.GetTable<Model.Demo>()
                    where
                        a.DemoName == "002" &&
                        a.DemoCode.Contains("TAN") &&                                   // LIKE '%%'
                        a.DemoCode.StartsWith("TAN") &&                                 // LIKE 'K%'
                        a.DemoCode.EndsWith("TAN") &&                                   // LIKE '%K' 
                        a.DemoCode.Length == 12 &&
                        //支持的字符串操作=> Trim | TrimStart | TrimEnd | ToString | Length
                        a.DemoDateTime == DateTime.Now &&
                        a.DemoName == (
                            a.DemoDateTime_Nullable == null ? "NULL" : "NOT NULL") &&   // 三元表达式
                        a.DemoName == (a.DemoName ?? a.DemoCode) &&                     // 二元表达式
                        new[] { 1, 2, 3 }.Contains(a.DemoId) &&
                        Demo._demoIdList.Contains(a.DemoId) &&
                        a.DemoName == Demo._demoName &&
                        a.DemoByte == (byte)m_byte &&
                        a.DemoByte == (byte)State.Complete ||                                 // IN(1,2,3)
                        (a.DemoName == "STATE" && a.DemoName == "REMARK")               // OR 查询
                    select a;
            r1 = query.ToList();
            // 点标记
            query = context.GetTable<Model.Demo>().Where(a =>
                        a.DemoName == "002" &&
                        a.DemoCode.Contains("TAN") &&                                   // LIKE '%%'
                        a.DemoCode.StartsWith("TAN") &&                                 // LIKE 'K%'
                        a.DemoCode.EndsWith("TAN") &&                                   // LIKE '%K' 
                        a.DemoCode.Length == 12 &&
                        //支持的字符串操作=> Trim | TrimStart | TrimEnd | ToString | Length | Substring
                        a.DemoDateTime == DateTime.Now &&
                        a.DemoName == (
                            a.DemoDateTime_Nullable == null ? "NULL" : "NOT NULL") &&   // 三元表达式
                        a.DemoName == (a.DemoName ?? a.DemoCode) &&                     // 二元表达式
                        new[] { 1, 2, 3 }.Contains(a.DemoId) &&                         // IN(1,2,3)
                        Demo._demoIdList.Contains(a.DemoId) &&
                        a.DemoName == Demo._demoName &&
                        a.DemoByte == (byte)m_byte &&
                        a.DemoByte == (byte)State.Complete ||
                        (a.DemoName == "STATE" && a.DemoName == "'--REMARK'-")          // OR 查询
                );
            //SQL=>            
            //SELECT
            //t0.[DemoId] AS[DemoId],
            //t0.[DemoCode] AS[DemoCode],
            //t0.[DemoName] AS[DemoName],
            //...
            //FROM[Sys_Demo] t0
            //WHERE (t0.[DemoName] = N'002' AND t0.[DemoCode] LIKE N'%TAN%' AND t0.[DemoCode] LIKE N'TAN%' AND t0.[DemoCode] LIKE N'%TAN' 
            //        AND LEN(t0.[DemoCode]) = 12 AND t0.[DemoDateTime] = '2018-08-13 17:48:47.427' 
            //        AND t0.[DemoName] = (CASE WHEN t0.[DemoDateTime_Nullable] IS NULL THEN N'NULL' ELSE N'NOT NULL' END) 
            //        AND t0.[DemoName] = ISNULL(t0.[DemoName], t0.[DemoCode]) AND t0.[DemoId] IN(1,2,3) AND t0.[DemoId] IN(2,3) 
            //AND t0.[DemoName] = N'002F' AND t0.[DemoByte] = 9 AND t0.[DemoByte] = 1) 
            //OR(t0.[DemoName] = N'STATE' AND t0.[DemoName] = N'''--REMARK''-')

        }

        // 多表查询
        static void Join()
        {
            var context = new DataContext();            

            // INNER JOIN
            var query =
                from a in context.GetTable<Model.Client>()
                join b in context.GetTable<Model.CloudServer>() on a.CloudServerId equals b.CloudServerId
                where a.ClientId > 0
                select a;
            var result = query.ToList();
            // 点标记
            query = context
                .GetTable<Model.Client>()
                .Join(context.GetTable<Model.CloudServer>(), a => a.CloudServerId, b => b.CloudServerId, (a, b) => a)
                .Where(a => a.ClientId > 0);
            result = query.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[Remark] AS[Remark],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId]
            //FROM[Bas_Client] t0
            //INNER JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //WHERE t0.[ClientId] > 0
            query =
                context
                .GetTable<Model.Client>()
                .Include(a => a.CloudServer);
            query =
                from a in query
                join b in context.GetTable<Model.CloudServer>() on a.CloudServerId equals b.CloudServerId
                orderby a.ClientId
                select new Model.Client (a)
                {
                    CloudServer = a.CloudServer
                };
            result = query.ToList();



            // 更简单的赋值方式 
            // 适用场景：在显示列表时只想显示外键表的一两个字段
            query =
                from a in context.GetTable<Model.Client>()
                select new Model.Client(a)
                {
                    CloudServer = a.CloudServer,
                    LocalServer = new Model.CloudServer
                    {
                        CloudServerId = a.CloudServerId,
                        CloudServerName = a.LocalServer.CloudServerName
                    }
                };
            result = query.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[Remark] AS[Remark],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId],
            //t1.[CloudServerId] AS[CloudServerId1],
            //t1.[CloudServerCode] AS[CloudServerCode],
            //t1.[CloudServerName] AS[CloudServerName],
            //CASE WHEN t1.[CloudServerId] IS NULL THEN NULL ELSE t1.[CloudServerId] END AS[NULL],
            //t0.[CloudServerId] AS[CloudServerId2],
            //t2.[CloudServerName] AS[CloudServerName1]
            //FROM[Bas_Client] t0
            //LEFT JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //LEFT JOIN[Sys_CloudServer] t2 ON t0.[CloudServerId] = t2.[CloudServerId]

            // 1：1关系，1：n关系
            query =
                from a in context
                    .GetTable<Model.Client>()
                where a.ClientId > 0
                orderby a.ClientId
                select new Model.Client(a)
                {
                    CloudServer = a.CloudServer,
                    Accounts = a.Accounts
                };
            result = query.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[Remark] AS[Remark],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId],
            //t1.[CloudServerId] AS[CloudServerId1],
            //t1.[CloudServerCode] AS[CloudServerCode],
            //t1.[CloudServerName] AS[CloudServerName],
            //CASE WHEN t1.[CloudServerId] IS NULL THEN NULL ELSE t1.[CloudServerId]
            //        END AS[NULL],
            //t2.[ClientId] AS[ClientId1],
            //t2.[AccountId] AS[AccountId],
            //t2.[AccountCode] AS[AccountCode],
            //t2.[AccountName] AS[AccountName],
            //CASE WHEN t2.[ClientId] IS NULL THEN NULL ELSE t2.[ClientId] END AS [NULL1]
            //FROM (
            //    SELECT
            //    t0.[ClientId] AS[ClientId],
            //    t0.[ClientCode] AS [ClientCode],
            //    t0.[ClientName] AS [ClientName],
            //    t0.[Remark] AS [Remark],
            //    t0.[State] AS [State],
            //    t0.[ActiveDate] AS [ActiveDate],
            //    t0.[CloudServerId] AS [CloudServerId]
            //    FROM [Bas_Client] t0
            //    WHERE t0.[ClientId] > 0
            //) t0
            //LEFT JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //LEFT JOIN [Bas_ClientAccount] t2 ON t0.[ClientId] = t2.[ClientId]
            //ORDER BY t0.[ClientId]

            // Include
            query =
                from a in context
                    .GetTable<Model.Client>()
                    .Include(a => a.Accounts)
                    .Include(a => a.Accounts[0].Markets)
                    .Include(a => a.Accounts[0].Markets[0].Client)
                where a.ClientId > 0
                orderby a.ClientId
                select a;
            result = query.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[Remark] AS[Remark],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId],
            //t1.[ClientId] AS[ClientId1],
            //t1.[AccountId] AS[AccountId],
            //t1.[AccountCode] AS[AccountCode],
            //t1.[AccountName] AS[AccountName],
            //CASE WHEN t1.[ClientId] IS NULL THEN NULL ELSE t1.[ClientId]
            //        END AS[NULL],
            //t2.[ClientId] AS[ClientId2],
            //t2.[AccountId] AS[AccountId1],
            //t2.[MarketId] AS[MarketId],
            //t2.[MarketCode] AS[MarketCode],
            //t2.[MarketName] AS[MarketName],
            //CASE WHEN t2.[ClientId] IS NULL THEN NULL ELSE t2.[ClientId] END AS[NULL1],
            //t3.[ClientId] AS[ClientId3],
            //t3.[ClientCode] AS[ClientCode1],
            //t3.[ClientName] AS[ClientName1],
            //t3.[Remark] AS[Remark1],
            //t3.[State] AS[State1],
            //t3.[ActiveDate] AS[ActiveDate1],
            //t3.[CloudServerId] AS[CloudServerId1],
            //CASE WHEN t3.[ClientId] IS NULL THEN NULL ELSE t3.[ClientId] END AS [NULL2]
            //FROM (
            //    SELECT
            //    t0.[ClientId] AS[ClientId],
            //    t0.[ClientCode] AS [ClientCode],
            //    t0.[ClientName] AS [ClientName],
            //    t0.[Remark] AS [Remark],
            //    t0.[State] AS [State],
            //    t0.[ActiveDate] AS [ActiveDate],
            //    t0.[CloudServerId] AS [CloudServerId]
            //    FROM [Bas_Client] t0
            //    WHERE t0.[ClientId] > 0
            //) t0
            //LEFT JOIN[Bas_ClientAccount] t1 ON t0.[ClientId] = t1.[ClientId]
            //LEFT JOIN [Bas_ClientAccountMarket] t2 ON t1.[ClientId] = t2.[ClientId] AND t1.[AccountId] = t2.[AccountId]
            //LEFT JOIN [Bas_Client] t3 ON t2.[ClientId] = t3.[ClientId]
            //ORDER BY t0.[ClientId]

            query =
            from a in context
                .GetTable<Model.Client>()
                .Include(a => a.Accounts)
                .Include(a => a.Accounts[0].Markets)
                .Include(a => a.Accounts[0].Markets[0].Client)
            where a.ClientId > 0
            orderby a.ClientId
            select a;
            query = query
                .Where(a => a.ClientId > 0 && a.CloudServer.CloudServerId > 0)
                .Skip(10)
                .Take(20);
            result = query.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[Remark] AS[Remark],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId],
            //t1.[ClientId] AS[ClientId1],
            //t1.[AccountId] AS[AccountId],
            //t1.[AccountCode] AS[AccountCode],
            //t1.[AccountName] AS[AccountName],
            //CASE WHEN t1.[ClientId] IS NULL THEN NULL ELSE t1.[ClientId]
            //        END AS[NULL],
            //t2.[ClientId] AS[ClientId2],
            //t2.[AccountId] AS[AccountId1],
            //t2.[MarketId] AS[MarketId],
            //t2.[MarketCode] AS[MarketCode],
            //t2.[MarketName] AS[MarketName],
            //CASE WHEN t2.[ClientId] IS NULL THEN NULL ELSE t2.[ClientId] END AS[NULL1],
            //t3.[ClientId] AS[ClientId3],
            //t3.[ClientCode] AS[ClientCode1],
            //t3.[ClientName] AS[ClientName1],
            //t3.[Remark] AS[Remark1],
            //t3.[State] AS[State1],
            //t3.[ActiveDate] AS[ActiveDate1],
            //t3.[CloudServerId] AS[CloudServerId1],
            //CASE WHEN t3.[ClientId] IS NULL THEN NULL ELSE t3.[ClientId] END AS [NULL2]
            //FROM (
            //    SELECT
            //    t0.[ClientId] AS[ClientId],
            //    t0.[ClientCode] AS [ClientCode],
            //    t0.[ClientName] AS [ClientName],
            //    t0.[Remark] AS [Remark],
            //    t0.[State] AS [State],
            //    t0.[ActiveDate] AS [ActiveDate],
            //    t0.[CloudServerId] AS [CloudServerId]
            //    FROM [Bas_Client] t0
            //    LEFT JOIN [Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //    WHERE t0.[ClientId] > 0 AND t0.[ClientId] > 0 AND t1.[CloudServerId] > 0
            //    ORDER BY t0.[ClientId]
            //    OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY
            //) t0
            //LEFT JOIN[Bas_ClientAccount] t1 ON t0.[ClientId] = t1.[ClientId]
            //LEFT JOIN [Bas_ClientAccountMarket] t2 ON t1.[ClientId] = t2.[ClientId] AND t1.[AccountId] = t2.[AccountId]
            //LEFT JOIN [Bas_Client] t3 ON t2.[ClientId] = t3.[ClientId]

            // Include 语法查询 主 从 孙 关系
            var query1 =
                from a in
                    context
                    .GetTable<Model.Client>()
                    .Include(a => a.Accounts)
                    .Include(a => a.Accounts[0].Markets)
                    .Include(a => a.Accounts[0].Markets[0].Client)
                join b in context.GetTable<Model.CloudServer>() on a.CloudServerId equals b.CloudServerId
                group a by new { a.ClientId, a.ClientCode, a.ClientName, b.CloudServerName } into g
                select new
                {
                    ClientId = g.Key.ClientId,
                    ClientCode = g.Key.ClientCode,
                    ClientName = g.Key.ClientName,
                    CloudServerName = g.Key.CloudServerName
                };
            query1 = query1
                .Where(a => a.ClientId > 0)
                .OrderBy(a => a.ClientId)
                .Skip(10)
                .Take(20)
                ;
            var result1 = query1.ToList();
            var max1 = query1.Max(a => a.ClientId);

            // goup by
            var query2 =
                from a in context.GetTable<Model.Client>()
                group a by a.ClientId into g
                select new
                {
                    ClientId = g.Key
                };
            query2 = query2.OrderBy(a => a.ClientId);
            var result2 = query2.Max(a => a.ClientId);
            //SQL=>
            //SELECT
            //MAX(t0.[ClientId])
            //FROM(
            //    SELECT
            //    t0.[ClientId] AS[ClientId],
            //    t0.[ClientId] AS[ClientId1]
            //    FROM[Bas_Client] t0
            //    GROUP BY t0.[ClientId]
            // ) t0

            var query3 =
                from a in context.GetTable<Model.Client>()
                join b in context.GetTable<Model.Account>() on a.ClientId equals b.ClientId
                group new { a.ClientId, b.AccountId } by new { a.ClientId, b.AccountId } into g
                select new
                {
                    ClientId = g.Key.ClientId,
                    AccountId = g.Key.AccountId,
                    Max = g.Max(b => b.AccountId)
                };
            var result3 = query3.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t1.[AccountId] AS[AccountId],
            //MAX(t1.[AccountId]) AS[Max]
            //FROM[Bas_Client] t0
            //INNER JOIN[Bas_ClientAccount] t1 ON t0.[ClientId] = t1.[ClientId]
            //GROUP BY t0.[ClientId],t1.[AccountId]


            // CROSS JOIN
            var query4 =
                context
                .GetTable<Model.Demo>()
                .SelectMany(a => context.GetTable<Model.Demo>(), (a, b) => new
                {
                    a.DemoId,
                    b.DemoName
                });
            var result4 = query4.ToList();
            //SQL=>
            //SELECT
            //t0.[DemoId] AS[DemoId],
            //t1.[DemoName] AS[DemoName]
            //FROM[Sys_Demo] t0
            //CROSS JOIN[Sys_Demo] t1

            // LEFT JOIN
            query =
                  from a in context.GetTable<Model.Client>()
                  join b in context.GetTable<Model.CloudServer>() on a.CloudServerId equals b.CloudServerId into u_b
                  from b in u_b
                  select a;
            query = query.Where(a => a.CloudServer.CloudServerName != null);
            result = query.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[Remark] AS[Remark],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId]
            //FROM[Bas_Client] t0
            //LEFT JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //WHERE t1.[CloudServerName] IS NOT NULL


            query =
                 from a in context.GetTable<Model.Client>()
                 join b in context.GetTable<Model.CloudServer>() on a.CloudServerId equals b.CloudServerId into u_c
                 from b in u_c.DefaultIfEmpty()
                 select a;
            var query5 =
                query.SelectMany(c => context.GetTable<Model.CloudServer>(), (a, c) => new
                {
                    ClientId = a.ClientId,
                    CloudServerName = a.CloudServer.CloudServerName,
                    CloudServerCode = c.CloudServerCode
                });
            var result5 = query5.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t1.[CloudServerName] AS[CloudServerName],
            //t2.[CloudServerCode] AS[CloudServerCode]
            //FROM[Bas_Client] t0
            //LEFT JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //CROSS JOIN[Sys_CloudServer] t2

            // UNION 注意UNION语法不支持OrderBy
            var q1 = context.GetTable<Model.Client>().Where(x => x.ClientId == 0);
            var q2 = context.GetTable<Model.Client>().Where(x => x.ClientId == 0);
            var q3 = context.GetTable<Model.Client>().Where(x => x.ClientId == 0);
            var query6 = q1.Union(q2).Union(q3);
            var result6 = query6.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //...
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientId] = 1
            //UNION ALL
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //...
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientId] = 1
            //UNION ALL
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //...
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientId] = 1

            // Any
            var isAny = context.GetTable<Model.Client>().Any(a => a.ActiveDate == DateTime.Now);
            //SQL=> 
            //IF EXISTS(
            //    SELECT TOP 1 1
            //    FROM[Bas_Client] t0
            //   WHERE t0.[ActiveDate] = '2018-08-15 14:07:09.784'
            //) SELECT 1 ELSE SELECT 0

            // FirstOrDefault
            var f = context.GetTable<Model.Client>().FirstOrDefault();
            //SQL=> 
            //SELECT TOP(1)
            //t0.[ClientId] AS[ClientId],
            //t0.[ClientCode] AS[ClientCode],
            //t0.[ClientName] AS[ClientName],
            //t0.[State] AS[State],
            //t0.[ActiveDate] AS[ActiveDate],
            //t0.[CloudServerId] AS[CloudServerId]
            //FROM[Bas_Client] t0

            // Max
            var max = context.GetTable<Model.Client>().Where(a => a.ClientId < -9).Max(a => a.ClientId);
            //SQL=> 
            //SELECT
            //MAX(t0.[ClientId])
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientId] < -9

            // GROUP BY
            var query7 =
                 from a in context.GetTable<Model.Client>()
                 where a.ClientName == "TAN"
                 group a by new { a.ClientId, a.ClientName } into g
                 where g.Key.ClientId > 0
                 orderby g.Key.ClientName
                 select new
                 {
                     Id = g.Key.ClientId,
                     Name = g.Min(a => a.ClientId)
                 };
            var result7 = query7.ToList();
            //SQL=> 
            //SELECT
            //t0.[ClientId] AS[Id],
            //MIN(t0.[ClientId]) AS[Name]
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientName] = N'TAN'
            //GROUP BY t0.[ClientId],t0.[ClientName]
            //Having t0.[ClientId] > 0
            //ORDER BY t0.[ClientName]

            // 分组后再分页
            var query8 =
                 from a in context.GetTable<Model.Client>()
                 where a.ClientName == "TAN"
                 group a by new { a.ClientId, a.ClientName } into g
                 where g.Key.ClientId > 0
                 orderby new { g.Key.ClientName, g.Key.ClientId }
                 select new
                 {
                     Id = g.Key.ClientId,
                     Name = g.Min(a => a.ClientId)
                 };
            query8 = query8.Skip(2).Take(3);
            var result8 = query8.ToList();
            //SQL=> 
            //SELECT
            //t0.[ClientId] AS[Id],
            //MIN(t0.[ClientId]) AS[Name]
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientName] = N'TAN'
            //GROUP BY t0.[ClientId],t0.[ClientName]
            //Having t0.[ClientId] > 0
            //ORDER BY t0.[ClientName],t0.[ClientId]
            //OFFSET 2 ROWS FETCH NEXT 3 ROWS ONLY

            // DISTINCT 分组
            query =
                context
                .GetTable<Model.Client>()
                .Distinct()
                .Select(a => new Model.Client
                {
                    ClientId = a.ClientId,
                    ClientName = a.ClientName
                });
            var min = query.Min(a => a.ClientId);
            //SQL=> 
            //SELECT
            //MIN(t0.[ClientId])
            //FROM(
            //    SELECT DISTINCT
            //    t0.[ClientId] AS[ClientId],
            //    t0.[ClientName] AS[ClientName],
            //    t0.[ClientId] AS[ClientId1],
            //    t0.[ClientName] AS[ClientName1]
            //    FROM[Bas_Client] t0
            // ) t0
        }

        // 其它说明
        static void Other()
        {
            var context = new DataContext();

            var demo = new Model.Demo { DemoId = 1, DemoCode = "D1", DemoName = "D1", DemoChar = "FF", DemoDateTime = DateTime.Now };
            // 删除记录
            context.Delete(demo);
            context.SubmitChanges();

            context.Delete<Model.Demo>(a => a.DemoId == 2 || a.DemoId == 3 || a.DemoName == "XF");
            var qeury =
                context
                .GetTable<Model.Demo>()
                .Where(a => a.DemoId == 2 || a.DemoId == 3);
            context.Delete<Model.Demo>(qeury);
            // 一次性提交
            context.SubmitChanges();
            //SQL=> 
            //DELETE t0 FROM[Sys_Demo] t0
            //WHERE((t0.[DemoId] = 2) OR(t0.[DemoId] = 3)) OR(t0.[DemoName] = N'XF')
            //DELETE t0 FROM[Sys_Demo] t0
            //WHERE(t0.[DemoId] = 2) OR(t0.[DemoId] = 3)

            // 增加记录
            context.Insert(demo);
            context.SubmitChanges();
            //SQL=> 
            //INSERT INTO[Sys_Demo]
            //([DemoCode],[DemoName],[DemoChar],[DemoChar_Nullable],[DemoByte],[DemoByte_Nullable],[DemoDateTime],[DemoDateTime_Nullable],[DemoDecimal],[DemoDecimal_Nullable],[DemoFloat],[DemoFloat_Nullable],[DemoReal],[Demo_Nullable],[DemoGuid],[DemoGuid_Nullable],[DemoShort],[DemoShort_Nullable],[DemoInt],[DemoInt_Nullable],[DemoLong],[DemoLong_Nullable])
            //VALUES
            //(N'D1', N'D1', N'FF', NULL, 0, NULL, '2018-08-15 14:25:52.957', NULL, 0, NULL, 0, NULL, 0, NULL, '00000000-0000-0000-0000-000000000000', NULL, 0, NULL, 0, NULL, 0, NULL)
            //SELECT CAST(SCOPE_IDENTITY() AS INT)

            demo.DemoName = "001.N";
            context.Update(demo);

            var client = context.GetTable<Model.Client>().FirstOrDefault();
            context.Update(client);

            context.SubmitChanges();
            ////SQL=> 
            //UPDATE t0 SET
            //t0.[DemoCode] = N'D1',
            //t0.[DemoName] = N'001.N',
            //t0.[DemoChar] = N'FF',
            //t0.[DemoChar_Nullable] = NULL,
            //t0.[DemoByte] = 0,
            //t0.[DemoByte_Nullable] = NULL,
            //t0.[DemoDateTime] = '2018-08-15 14:43:53.993',
            //t0.[DemoDateTime_Nullable] = NULL,
            //t0.[DemoDecimal] = 0,
            //t0.[DemoDecimal_Nullable] = NULL,
            //t0.[DemoFloat] = 0,
            //t0.[DemoFloat_Nullable] = NULL,
            //t0.[DemoReal] = 0,
            //t0.[Demo_Nullable] = NULL,
            //t0.[DemoGuid] = '00000000-0000-0000-0000-000000000000',
            //t0.[DemoGuid_Nullable] = NULL,
            //t0.[DemoShort] = 0,
            //t0.[DemoShort_Nullable] = NULL,
            //t0.[DemoInt] = 0,
            //t0.[DemoInt_Nullable] = NULL,
            //t0.[DemoLong] = 0,
            //t0.[DemoLong_Nullable] = NULL
            //FROM[Sys_Demo] t0
            //WHERE t0.[DemoId] = 19
            //UPDATE t0 SET
            //t0.[ClientId] = 81,
            //t0.[ClientCode] = N'1188@qq.com',
            //t0.[ClientName] = N'广州大哥大技术有限公司',
            //t0.[Remark] = NULL,
            //t0.[State] = 8,
            //t0.[ActiveDate] = NULL,
            //t0.[CloudServerId] = 0
            //FROM[Bas_Client] t0
            //WHERE t0.[ClientId] = 81

            List<Model.Client> collection = context.GetTable<Model.Client>().Take(205).ToList();
            for (int i = 0; i < collection.Count; i++) context.Update(collection[i]);
            context.SubmitChanges();

            // 更新记录
            context.Update<Model.Demo>(a => new Model.Demo { DemoName = "'--001.TAN" }, a => a.DemoId != 3);
            context.SubmitChanges();
            //SQL=> 
            //UPDATE t0 SET
            //t0.[DemoName] = N'''--001.TAN'
            //FROM[Sys_Demo] AS[t0]
            //WHERE t0.[DemoId] <> 3

            var query3 =
                from a in context.GetTable<Model.Client>()
                where a.CloudServer.CloudServerId != 0
                select a;
            context.Update<Model.Client>(a => new Model.Client
            {
                Remark = "001.TAN"
            }, query3);
            context.SubmitChanges();
            //SQL=> 
            //UPDATE t0 SET
            //t0.[Remark] = N'001.TAN'
            //FROM[Bas_Client] AS[t0]
            //LEFT JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //WHERE t1.[CloudServerId] <> 0

            // 批量增加
            // 产生 INSERT INTO VALUES(),(),()... 语法。注意这种批量增加的方法并不能给自增列自动赋值
            context.Delete<Model.Demo>(x => x.DemoId > 0);
            List<Model.Demo> demos = new List<Model.Demo>();
            for (int i = 0; i < 1002; i++)
            {
                Model.Demo d = new Model.Demo
                {
                    DemoId = 1,
                    DemoCode = "D1",
                    DemoName = "D1",
                    DemoChar = "FF",
                    DemoDateTime = DateTime.Now
                };
                demos.Add(d);
            }
            context.Insert<Model.Demo>(demos);
            context.SubmitChanges();
            //SQL=>
            //INSERT INTO[Sys_Demo]
            //([DemoCode],[DemoName],[DemoChar],[DemoChar_Nullable],[DemoByte],[DemoByte_Nullable],[DemoDateTime],[DemoDateTime_Nullable],[DemoDecimal],[DemoDecimal_Nullable],[DemoFloat],[DemoFloat_Nullable],[DemoReal],[Demo_Nullable],[DemoGuid],[DemoGuid_Nullable],[DemoShort],[DemoShort_Nullable],[DemoInt],[DemoInt_Nullable],[DemoLong],[DemoLong_Nullable])
            //VALUES(...),(),()...
        }

        // 性能测试
        static void Performance()
        {
            Stopwatch stop = new Stopwatch();
            string connString = "Server=.;Database=Inte_CRM;uid=sa;pwd=123456";
            var context = new DataContext(new ICS.XFramework.Data.SqlClient.DbQueryProvider(connString));

            stop = new Stopwatch();
            stop.Start();
            for (int i = 0; i < 10; i++)
            {
                DateTime beginDate = DateTime.Now;
                var result = context
                    .GetTable<Model.DemoPerformance>()
                    .ToList();
                Console.WriteLine(string.Format("第 {0} 次，用时：{1}", 1, (DateTime.Now - beginDate).TotalMilliseconds / 1000.0));
            }

            stop.Stop();
            Console.WriteLine(string.Format("运行 10 次 100w 行单表数据，用时：{0}", stop.Elapsed));
            Console.ReadLine();

            stop = new Stopwatch();
            stop.Start();
            for (int i = 0; i < 5000; i++)
            {
                var result = context
                    .GetTable<Model.Client>()
                    .Include(a => a.Accounts)
                    .ToList();
            }
            stop.Stop();
            Console.WriteLine(string.Format("运行 5000 次 2000 行主从数据，用时：{0}", stop.Elapsed));

            stop = new Stopwatch();
            stop.Start();
            for (int i = 0; i < 5000; i++)
            {
                var result = context
                    .GetTable<Model.Client>()
                    .Include(a => a.Accounts)
                    .Include(a => a.Accounts[0].Markets)
                    .ToList();
            }
            stop.Stop();
            Console.WriteLine(string.Format("运行 5000 次 2000 行主从孙数据，用时：{0}", stop.Elapsed));
            Console.ReadLine();
        }
    }
}
