USE [master]
GO

IF NOT EXISTS 
    (SELECT name  
     FROM master.sys.server_principals
     WHERE name = 'AttendanceTrackerAdmin')
BEGIN
 CREATE LOGIN [AttendanceTrackerAdmin] WITH PASSWORD=N'QujM9nXRutum', DEFAULT_DATABASE=[AttendanceTracker], CHECK_EXPIRATION=ON, CHECK_POLICY=ON
END
GO


USE [AttendanceTracker]
GO


IF NOT EXISTS
	(SELECT * 
	FROM sys.database_principals
	WHERE name = 'AttendanceTrackerAdmin')
BEGIN
	CREATE USER [AttendanceTrackerAdmin] FOR LOGIN [AttendanceTrackerAdmin]	
	GRANT CONTROL ON SCHEMA::[dbo] TO [AttendanceTrackerAdmin];
	GRANT ALTER ON SCHEMA::[dbo] TO [AttendanceTrackerAdmin];
	GRANT CREATE TABLE TO [dbo];
END

GO