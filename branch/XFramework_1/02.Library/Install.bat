@echo off
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe E:\HKTFS\数据提报系统\数据抽取平台\HKERP.ETL\Library\HKERP.ETL.Service.exe
Net Start HKETLService
sc config HKETLService start= auto
ECHO.
pause
