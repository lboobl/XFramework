@echo off
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe E:\HKTFS\�����ᱨϵͳ\���ݳ�ȡƽ̨\HKERP.ETL\Library\HKERP.ETL.Service.exe
Net Start HKETLService
sc config HKETLService start= auto
ECHO.
pause
