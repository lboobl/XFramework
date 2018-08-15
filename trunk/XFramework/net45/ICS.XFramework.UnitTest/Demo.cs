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
            Query();
            Join();
            //Other();
            //Horse();
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
                .Take(20);

            var result1 = query1.ToList();
            var max1 = query1.Max(a => a.ClientId);

            // CROSS JOIN
            var query2 =
                context
                .GetTable<Model.Demo>()
                .SelectMany(a => context.GetTable<Model.Demo>(), (a, b) => new
                {
                    a.DemoId,
                    b.DemoName
                });
            var result2 = query2.ToList();
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


            var query3 =
                 from a in context.GetTable<Model.Client>()
                 join b in context.GetTable<Model.CloudServer>() on a.CloudServerId equals b.CloudServerId into u_c
                 from b in u_c.DefaultIfEmpty()
                 select a;
            var query4=
                query3.SelectMany(c => context.GetTable<Model.CloudServer>(), (a, c) => new
                {
                    ClientId = a.ClientId,
                    CloudServerName = a.CloudServer.CloudServerName,
                    CloudServerCode = c.CloudServerCode
                });
            var result4 = query4.ToList();
            //SQL=>
            //SELECT
            //t0.[ClientId] AS[ClientId],
            //t1.[CloudServerName] AS[CloudServerName],
            //t2.[CloudServerCode] AS[CloudServerCode]
            //FROM[Bas_Client] t0
            //LEFT JOIN[Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
            //CROSS JOIN[Sys_CloudServer] t2

            // UNION 注意UNION语法不支持OrderBy
            var q1 = context.GetTable<Model.Client>().Where(x => x.ClientId == 1);
            var q2 = context.GetTable<Model.Client>().Where(x => x.ClientId == 1);
            var q3 = context.GetTable<Model.Client>().Where(x => x.ClientId == 1);
            var q5 = q1.Union(q2).Union(q3);
            var result5 = q5.ToList();
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
           

            //var q1 =
            //    from a in context.GetTable<Model.Demo>()
            //    select new { top = 1 };
            //var rrr = q1.ToList();

            //var q2 =
            //from a in context.GetTable<Model.Client>()
            //join b in context.GetTable<Model.Account>() on a.ClientId equals b.ClientId
            //select new Model.Account(b)
            //{
            //    ClientId = 99,
            //    Client = new Model.Client
            //    {
            //        ClientName = a.ClientName
            //    }
            //};
            //var q2r2 = q2.ToList();

            //var q4 = context
            //    .GetTable<Model.User>()
            //    .Include(a => a.Roles)
            //    .Include(a => a.Modules)
            //    .Where(a => a.UserId == 1);
            //var r4 = q4.ToList();

            //var q5 = context
            //    .GetTable<Model.User>()
            //    .Include(a => a.Roles)
            //    .Include(a => a.Roles[0].RoleModules)
            //    .Include(a => a.Modules)
            //    .Where(a => a.UserId > 0);
            //            var r5 = q5.ToList();
        }

        //// 其它说明
        //static void Other()
        //{
        //    var context = new DataContext();
        //var q10=
        //    from a in context.GetTable<Model.CRM_SaleOrder>()
        //    group a by a.ClientId into g
        //    select new { ClientId = g.Key };
        //q10 = q10.OrderBy(a => a.ClientId);
        //var result10 = q10.Max(a => a.ClientId);

        //var query0 = context.GetTable<Model.CRM_SaleOrder>();
        //var result01 = query0.ToList();

        //var query2 =
        //    from a in context.GetTable<Model.CRM_SaleOrder>()
        //    group a by a.ClientId into g
        //    select new { ClientId= g.Key};
        //var m2 = query2.Max(a=>a.ClientId);
        //    // Any
        //    var any = context.GetTable<Model.Client>().Any(a => a.ActiveDate == DateTime.Now);
        //    //SQL=> 
        //    //IF EXISTS(
        //    //    SELECT TOP 1 1
        //    //    FROM[Bas_Client] t0
        //    //    WHERE t0.[ActiveDate] = '2017/11/26 22:43:52'
        //    //) SELECT 1 ELSE SELECT 0

        //    // FirstOrDefault
        //    var f = context.GetTable<Model.Client>().FirstOrDefault();
        //    //SQL=> 
        //    //SELECT TOP(1)
        //    //t0.[ClientId] AS[ClientId],
        //    //t0.[ClientCode] AS[ClientCode],
        //    //t0.[ClientName] AS[ClientName],
        //    //t0.[State] AS[State],
        //    //t0.[ActiveDate] AS[ActiveDate],
        //    //t0.[CloudServerId] AS[CloudServerId]
        //    //FROM[Bas_Client] t0

        //    // Max
        //    var max = context.GetTable<Model.Client>().Where(a => a.ClientId < -9).Max(a => a.ClientId);
        //    //SQL=> 
        //    //SELECT
        //    //MAX(t0.[ClientId])
        //    //FROM[Bas_Client] t0
        //    //WHERE t0.[ClientId] < -9

        //    // GROUP BY
        //    var query =
        //        from a in context.GetTable<Model.Client>()
        //        where a.ClientName == "TAN"
        //        group a by new { a.ClientId, a.ClientName } into g
        //        where g.Key.ClientId > 0
        //        orderby g.Key.ClientName
        //        select new
        //        {
        //            Id = g.Key.ClientId,
        //            Name = g.Min(a => a.ClientId)
        //        };
        //    var r1 = query.ToList();
        //    //SQL=> 
        //    //SELECT
        //    //t0.[ClientId] AS[Id],
        //    //MIN(t0.[ClientId]) AS[Name]
        //    //FROM[Bas_Client] t0
        //    //WHERE t0.[ClientName] = N'TAN'
        //    //GROUP BY t0.[ClientId],t0.[ClientName]
        //    //Having t0.[ClientId] > 0
        //    //ORDER BY t0.[ClientName]

        //    // 分组后再分页
        //    query =
        //        from a in context.GetTable<Model.Client>()
        //        where a.ClientName == "TAN"
        //        group a by new { a.ClientId, a.ClientName } into g
        //        where g.Key.ClientId > 0
        //        orderby new { g.Key.ClientName, g.Key.ClientId }
        //        select new
        //        {
        //            Id = g.Key.ClientId,
        //            Name = g.Min(a => a.ClientId)
        //        };
        //    query = query.Skip(2).Take(3);
        //    r1 = query.ToList();
        //    //SQL=> 
        //    //SELECT 
        //    //t0.[Id],
        //    //t0.[Name]
        //    //FROM ( 
        //    //    SELECT 
        //    //    t0.[ClientId] AS [Id],
        //    //    MIN(t0.[ClientId]) AS [Name],
        //    //    t0.[ClientName] AS [ClientName]
        //    //    FROM [Bas_Client] t0 
        //    //    WHERE t0.[ClientName] = N'TAN'
        //    //    GROUP BY t0.[ClientId],t0.[ClientName]
        //    //    Having t0.[ClientId] > 0
        //    // ) t0
        //    //ORDER BY t0.[ClientName]
        //    //OFFSET 2 ROWS FETCH NEXT 3 ROWS ONLY 

        //    // DISTINCT 分组
        //    var query2 =
        //        context.GetTable<Model.Client>().Distinct().Select(a => new Model.Client { ClientId = a.ClientId, ClientName = a.ClientName });
        //    var min = query2.Min(a => a.CloudServerId);
        //    //SQL=> 
        //    //SELECT 
        //    //MIN(t0.[CloudServerId])
        //    //FROM ( 
        //    //    SELECT DISTINCT 
        //    //    t0.[ClientId] AS [ClientId],
        //    //    t0.[ClientName] AS [ClientName],
        //    //    t0.[CloudServerId] AS [CloudServerId]
        //    //    FROM [Bas_Client] t0 
        //    // ) t0

        //    var thin = new Model.Thin { ThinId = 1, ThinName = "001" };
        //    var thinIdentity = new Model.ThinIdentity { ThinName = "001" };
        //    List<Model.Thin> collection = new List<Model.Thin>
        //    {
        //         new Model.Thin { ThinId = 2, ThinName = "002" },
        //         new Model.Thin { ThinId = 3, ThinName = "003" }
        //    };
        //    List<Model.ThinIdentity> identirys = new List<Model.ThinIdentity>
        //    {
        //         new Model.ThinIdentity { ThinName = "002" },
        //         new Model.ThinIdentity { ThinName = "003" }
        //    };

        //    // 删除记录
        //    context.Delete(thin);
        //    context.Delete(thinIdentity);
        //    ///context.SubmitChanges();

        //    context.Delete<Model.Thin>(a => a.ThinId == 2 || a.ThinId == 3);
        //    context.Delete<Model.ThinIdentity>(a => a.ThinName == "001" || a.ThinName == "002" || a.ThinName == "003");
        //    ///context.SubmitChanges();
        //    var qeury4 = context.GetTable<Model.Thin>().Where(a => a.ThinId == 2 || a.ThinId == 3);
        //    context.Delete<Model.Thin>(qeury4);
        //    context.SubmitChanges(); // 一次性提交
        //    //SQL=> 
        //    //DELETE t0 FROM [Sys_Thin] t0 
        //    //WHERE t0.[ThinId] = 1
        //    //DELETE t0 FROM [Sys_Thin] t0 
        //    //WHERE (t0.[ThinId] = 2) OR (t0.[ThinId] = 3)
        //    //DELETE t0 FROM [Sys_Thin] t0 
        //    //WHERE (t0.[ThinId] = 2) OR (t0.[ThinId] = 3)

        //    // 增加记录
        //    context.Insert(thin);
        //    context.SubmitChanges();
        //    //SQL=> 
        //    //INSERT INTO [Sys_Thin]
        //    //([ThinId],[ThinName])
        //    //VALUES
        //    //(1,N'001')


        //    context.Insert<Model.Thin>(collection);
        //    context.SubmitChanges();

        //    // 自增列添加记录
        //    context.Insert(thinIdentity);
        //    context.Insert<Model.ThinIdentity>(identirys);
        //    context.Insert(thinIdentity);
        //    context.Update<Model.Thin>(a => new Model.Thin { ThinName = "'--001.TAN" }, a => a.ThinId != 3);
        //    context.Insert<Model.ThinIdentity>(identirys);
        //    context.Insert(thinIdentity);
        //    context.SubmitChanges();
        //    //SQL=> 
        //    //INSERT INTO [Sys_ThinIdentity]
        //    //([ThinName])
        //    //VALUES
        //    //(N'001')
        //    //SELECT CAST(SCOPE_IDENTITY() AS INT)
        //    //...


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();


        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    thin.ThinName = "001.N";
        //    context.Update(thin);
        //    context.Update(thin);
        //    context.SubmitChanges();

        //    //SQL=> 
        //    //UPDATE t0 SET
        //    //t0.[ThinId] = 1,
        //    //t0.[ThinName] = N'001.N'
        //    //FROM [Sys_Thin] t0
        //    //WHERE t0.[ThinId] = 1

        //    // 更新记录
        //    context.Update<Model.Thin>(a => new Model.Thin { ThinName = "'--001.TAN" }, a => a.ThinId != 3);
        //    context.SubmitChanges();
        //    //SQL=> 
        //    //UPDATE t0 SET
        //    //t0.[ThinName] = N'001.TAN'
        //    //FROM [Sys_Thin] AS [t0]
        //    //WHERE t0.[ThinId] <> 3

        //    var query3 =
        //        from a in context.GetTable<Model.Client>()
        //        where a.CloudServer.CloudServerId != 0
        //        select a;
        //    context.Update<Model.Client>(a => new Model.Client { Remark = "001.TAN" }, query3);
        //    context.SubmitChanges();
        //    //SQL=> 
        //    //UPDATE t0 SET
        //    //t0.[Remark] = N'001.TAN'
        //    //FROM [Bas_Client] AS [t0]
        //    //LEFT JOIN [Sys_CloudServer] t1 ON t0.[CloudServerId] = t1.[CloudServerId]
        //    //WHERE t1.[CloudServerId] <> 0 


        // 性能测试
        static void Horse()
        {
            string connString = "Server=192.168.34.170;Database=ProductCenter;uid=testuser;pwd=123456";
            var context = new DataContext(new ICS.XFramework.Data.SqlClient.DbQueryProvider(connString));


            Stopwatch stop = new Stopwatch();
            //stop.Start();

            //for (int i = 0; i < 100; i++)
            //{
            //    var result = context
            //        .GetTable<Prd_Center.Product>()
            //        .Include(a => a.Client)
            //        .ToList();
            //}
            //Console.WriteLine(stop.ElapsedMilliseconds);
            //Console.WriteLine(stop.Elapsed);
            //Console.WriteLine("Enter to Begin");
            //Console.ReadLine();

            connString = "Server=.;Database=Inte_CRM;uid=sa;pwd=123456";
            context = new DataContext(new ICS.XFramework.Data.SqlClient.DbQueryProvider(connString));
            stop = new Stopwatch();
            stop.Start();
            //for (int i = 0; i < 2000; i++)
            //{
            //    var result = context
            //        .GetTable<Inte_CRM.Client>()
            //        .Include(a => a.AccountList)
            //        .ToList();
            //}
            var beginDate = DateTime.Now;
            for (int i = 0; i < 1; i++)
            {
                var result = context
                    .GetTable<Model.Test>()
                    //.Include(a => a.AccountList)
                    .ToList();
            }
            Console.Write("总计：" + (DateTime.Now - beginDate).TotalMilliseconds / 1000.0);
            stop.Stop();
            Console.WriteLine(stop.ElapsedMilliseconds);
            Console.WriteLine(stop.Elapsed);
            Console.ReadLine();

            for (int i = 0; i < 1000; i++)
            {
                var query =
                    from a in context.GetTable<Prd_Center.Product>()
                    where a.Client.ClientID == 1
                    select new Prd_Center.Product(a)
                    {
                        Client = a.Client
                    };
                //var query =
                //    from a in context.GetTable<Prd_Center.Product>()
                //    where a.ClientID == 1
                //    select new Prd_Center.Product
                //    {
                //        ClientID = a.ClientID,
                //        ProductID = a.ProductID,
                //        Title = a.Title
                //    };
                var result = query.ToList();
            }

            for (int i = 0; i < 1000; i++)
            {
                var query =
                    from a in context.GetTable<Prd_Center.Product>()
                    where a.Client.ClientID == 1
                    select new Prd_Center.Product(a)
                    {
                        ClientID = SqlMethod.RowNumber(x => a.ClientID),
                        Client = a.Client
                    };
                //var query =
                //    from a in context.GetTable<Prd_Center.Product>()
                //    where a.ClientID == 1
                //    select new Prd_Center.Product
                //    {
                //        ClientID = a.ClientID,
                //        ProductID = a.ProductID,
                //        Title = a.Title
                //    };
                var result = query.ToList();
            }


            //stop.Stop();
            //Console.WriteLine(stop.ElapsedMilliseconds);
            //Console.WriteLine(stop.Elapsed);
            //Console.ReadLine();
            // Elapsed = {00:04:52.2977968}
            // Elapsed = {00:06:30.9521988} EF

            context = new DataContext();
            stop = new Stopwatch();
            stop.Start();
            for (int i = 0; i < 10000; i++)
            {
                var query =
                    from a in context.GetTable<Model.Account>()
                    select new Model.Account(a)
                    {
                        Client = new Model.Client
                        {
                            ClientId = a.Client.ClientId,
                            ClientCode = a.Client.ClientCode,
                            ClientName = a.Client.ClientName
                        }
                    };
                var result = query.ToList();
            }

            stop.Stop();
            Console.WriteLine(stop.ElapsedMilliseconds);
            Console.WriteLine(stop.Elapsed);
            Console.ReadLine();

        }

        public int Run2(int r)
        {
            return r;
        }

    }
}
