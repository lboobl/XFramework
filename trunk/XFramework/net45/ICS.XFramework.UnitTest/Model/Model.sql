
 
If Not Exists (Select Top 1 1  From sys.objects Where [object_id] = OBJECT_ID('Bas_Client','U'))
CREATE TABLE [dbo].[Bas_Client](
	[ClientId] [int] NOT NULL,
	[ClientCode] [nvarchar](200) NOT NULL,
	[ClientName] [nvarchar](200) NULL,
	[CloudServerId] [int] NOT NULL,
	[ActiveDate] [datetime] NULL,
	[State] [tinyint] NOT NULL,
	[Remark] [nvarchar](250) NULL,
 CONSTRAINT [PK__Bas_Clie__E67E1A24ABD4030C] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Bas_Client] ADD  CONSTRAINT [DF__Bas_Clien__Cloud__5070F446]  DEFAULT ((0)) FOR [CloudServerId]
GO

ALTER TABLE [dbo].[Bas_Client] ADD  CONSTRAINT [DF__Bas_Clien__State__2A164134]  DEFAULT ((0)) FOR [State]
GO

If Not Exists (Select Top 1 1  From sys.objects Where [object_id] = OBJECT_ID('Bas_ClientAccount','U'))
CREATE TABLE [dbo].[Bas_ClientAccount](
	[ClientId] [int] NOT NULL,
	[AccountId] [nvarchar](100) NOT NULL,
	[AccountCode] [nvarchar](200) NOT NULL,
	[AccountName] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Bas_ClientAccount] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC,
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

If Not Exists (Select Top 1 1  From sys.objects Where [object_id] = OBJECT_ID('Bas_ClientAccountMarket','U'))
CREATE TABLE [dbo].[Bas_ClientAccountMarket](
	[ClientId] [int] NOT NULL,
	[AccountId] [nvarchar](100) NOT NULL,
	[MarketId] [int] NOT NULL,
	[MarketCode] [nvarchar](50) NOT NULL,
	[MarketName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Bas_ClientAccountMarket] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC,
	[AccountId] ASC,
	[MarketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
Delete Bas_ClientAccountMarket 
Go



Insert Into [Bas_ClientAccountMarket] ([ClientId],[AccountId],[MarketId],[MarketCode],[MarketName]) Values (81,N'1188@qq.com',1,N'Amazon',N'亚马逊')
Insert Into [Bas_ClientAccountMarket] ([ClientId],[AccountId],[MarketId],[MarketCode],[MarketName]) Values (81,N'1188@qq.com',2,N'AliExpress',N'速卖通')
Go


If Not Exists (Select Top 1 1  From sys.objects Where [object_id] = OBJECT_ID('Sys_Demo','U'))
CREATE TABLE [dbo].[Sys_Demo](
	[DemoId] [int] IDENTITY(1,1) NOT NULL,
	[DemoCode] [nvarchar](32) NULL,
	[DemoName] [nvarchar](32) NOT NULL,
	[DemoChar] [char](2) NOT NULL,
	[DemoChar_Nullable] [char](2) NULL,
	[DemoByte] [tinyint] NOT NULL,
	[DemoByte_Nullable] [tinyint] NULL,
	[DemoDateTime] [datetime] NOT NULL,
	[DemoDateTime_Nullable] [datetime] NULL,
	[DemoDecimal] [decimal](18, 2) NOT NULL,
	[DemoDecimal_Nullable] [decimal](18, 2) NULL,
	[DemoFloat] [float] NOT NULL,
	[DemoFloat_Nullable] [float] NULL,
	[DemoReal] [real] NOT NULL,
	[Demo_Nullable] [real] NULL,
	[DemoGuid] [uniqueidentifier] NOT NULL,
	[DemoGuid_Nullable] [uniqueidentifier] NULL,
	[DemoShort] [smallint] NOT NULL,
	[DemoShort_Nullable] [smallint] NULL,
	[DemoInt] [int] NOT NULL,
	[DemoInt_Nullable] [int] NULL,
	[DemoLong] [bigint] NOT NULL,
	[DemoLong_Nullable] [bigint] NULL,
 CONSTRAINT [PK_Sys_Demo_1] PRIMARY KEY CLUSTERED 
(
	[DemoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

If Not Exists (Select Top 1 1  From sys.objects Where [object_id] = OBJECT_ID('Sys_CloudServer','U'))
CREATE TABLE [dbo].[Sys_CloudServer](
	[CloudServerId] [int] NOT NULL,
	[CloudServerCode] [nvarchar](50) NOT NULL,
	[CloudServerName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CloudServerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
Delete Sys_CloudServer 
Go
Insert Into [Sys_CloudServer] ([CloudServerId],[CloudServerCode],[CloudServerName]) Values (8,N'0181',N'181服务器')
Insert Into [Sys_CloudServer] ([CloudServerId],[CloudServerCode],[CloudServerName]) Values (16,N'0182',N'182服务器')
Insert Into [Sys_CloudServer] ([CloudServerId],[CloudServerCode],[CloudServerName]) Values (17,N'0183',N'183服务器')
Go


Delete Sys_Demo 
Go
Insert Into [Sys_Demo] ([DemoCode],[DemoName],[DemoChar],[DemoChar_Nullable],[DemoByte],[DemoByte_Nullable],[DemoDateTime],[DemoDateTime_Nullable],[DemoDecimal],[DemoDecimal_Nullable],[DemoFloat],[DemoFloat_Nullable],[DemoReal],[Demo_Nullable],[DemoGuid],[DemoGuid_Nullable],[DemoShort],[DemoShort_Nullable],[DemoInt],[DemoInt_Nullable],[DemoLong],[DemoLong_Nullable]) Values (N'001',N'001','KB',NULL,8,NULL,'01  1 2017 12:00AM',NULL,16.00,NULL,32,NULL,64,NULL,'9e110f40-a784-4dd1-bf30-e67f76ec6a78',NULL,128,NULL,256,NULL,512,NULL)
Insert Into [Sys_Demo] ([DemoCode],[DemoName],[DemoChar],[DemoChar_Nullable],[DemoByte],[DemoByte_Nullable],[DemoDateTime],[DemoDateTime_Nullable],[DemoDecimal],[DemoDecimal_Nullable],[DemoFloat],[DemoFloat_Nullable],[DemoReal],[Demo_Nullable],[DemoGuid],[DemoGuid_Nullable],[DemoShort],[DemoShort_Nullable],[DemoInt],[DemoInt_Nullable],[DemoLong],[DemoLong_Nullable]) Values (N'002',N'002','KB','KB',8,8,'01  1 2017 12:00AM','01  1 2017 12:00AM',16.00,16.00,32,32,64,64,'9e110f40-a784-4dd1-bf30-e67f76ec6a78','9e110f40-a784-4dd1-bf30-e67f76ec6a78',128,128,256,256,512,512)
Go

If Not Exists (Select Top 1 1  From sys.objects Where [object_id] = OBJECT_ID('Sys_DemoPerformance','U'))
CREATE TABLE [dbo].[Sys_DemoPerformance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[F_Byte] [tinyint] NULL,
	[F_Int16] [smallint] NULL,
	[F_Int32] [int] NULL,
	[F_Int64] [bigint] NULL,
	[F_Double] [float] NULL,
	[F_Float] [real] NULL,
	[F_Decimal] [decimal](18, 0) NULL,
	[F_Bool] [bit] NULL,
	[F_DateTime] [datetime] NULL,
	[F_Guid] [uniqueidentifier] NULL,
	[F_String] [nvarchar](100) NULL,
 CONSTRAINT [PK_Sys_DemoPerformance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

TRUNCATE TABLE [Sys_DemoPerformance]
GO
DECLARE @i INT = 0;
BEGIN TRAN;
WHILE(@i<=1000000)
BEGIN
INSERT INTO [dbo].[Sys_DemoPerformance]
           ([F_Byte]
           ,[F_Int16]
           ,[F_Int32]
           ,[F_Int64]
           ,[F_Double]
           ,[F_Float]
           ,[F_Decimal]
           ,[F_Bool]
           ,[F_DateTime]
           ,[F_Guid]
           ,[F_String])
     VALUES
           (1
           ,2
           ,@i
           ,@i
           ,@i
           ,@i
           ,@i
           ,@i%2
           ,GETDATE()
           ,NEWID()
           ,'Chloe' + CAST(@i AS NVARCHAR(1000))
		   )
set @i=@i+1;
END
COMMIT;


TRUNCATE TABLE [Bas_Client]
TRUNCATE TABLE [Bas_ClientAccount]
TRUNCATE TABLE [Bas_ClientAccountMarket]
SET @i = 1
BEGIN TRAN;
WHILE @i<=2000
BEGIN
	INSERT INTO [dbo].[Bas_Client]
           ([ClientId]
           ,[ClientCode]
           ,[ClientName]
           ,[CloudServerId]
           ,[ActiveDate]
           ,[State]
           ,[Remark])
     VALUES
           (@i
           ,'XFramework' + cast(@i as nvarchar)
           ,'XFramework' + cast(@i as nvarchar)
           ,8
           ,getdate()
           ,1
           ,'XFramework' + cast(@i as nvarchar))

	
	INSERT INTO [dbo].[Bas_ClientAccount]
           ([ClientId]
           ,[AccountId]
           ,[AccountCode]
           ,[AccountName])
     VALUES
           (@i
           ,1
           ,'XFrameworkAccount' + cast(@i as nvarchar)
           ,'XFrameworkAccount' + cast(@i as nvarchar))

		   	INSERT INTO [dbo].[Bas_ClientAccount]
           ([ClientId]
           ,[AccountId]
           ,[AccountCode]
           ,[AccountName])
     VALUES
           (@i
           ,2
           ,'XFrameworkAccount' + cast(@i as nvarchar)
           ,'XFrameworkAccount' + cast(@i as nvarchar))

		   
INSERT INTO [dbo].[Bas_ClientAccountMarket]
           ([ClientId]
           ,[AccountId]
           ,[MarketId]
           ,[MarketCode]
           ,[MarketName])
     VALUES
           (@i
           ,1
           ,1
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar)
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar))

		   		   
INSERT INTO [dbo].[Bas_ClientAccountMarket]
           ([ClientId]
           ,[AccountId]
           ,[MarketId]
           ,[MarketCode]
           ,[MarketName])
     VALUES
           (@i
           ,1
           ,2
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar)
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar))

		   INSERT INTO [dbo].[Bas_ClientAccountMarket]
           ([ClientId]
           ,[AccountId]
           ,[MarketId]
           ,[MarketCode]
           ,[MarketName])
     VALUES
           (@i
           ,2
           ,1
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar)
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar))

		   		   
INSERT INTO [dbo].[Bas_ClientAccountMarket]
           ([ClientId]
           ,[AccountId]
           ,[MarketId]
           ,[MarketCode]
           ,[MarketName])
     VALUES
           (@i
           ,2
           ,2
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar)
           ,'XFrameworkAccountMarket' + cast(@i as nvarchar))
		  
		   set @i=@i+1
END
COMMIT;