USE [master]
GO

IF NOT EXISTS 
    (SELECT name  
     FROM master.sys.server_principals
     WHERE name = 'AttendanceTrackerUser')
BEGIN
 CREATE LOGIN [AttendanceTrackerUser] WITH PASSWORD=N'9jTReUbz8Bpt', DEFAULT_DATABASE=[AttendanceTracker], CHECK_EXPIRATION=ON, CHECK_POLICY=ON
END
GO

USE [AttendanceTracker]
GO

IF NOT EXISTS
	(SELECT * 
	FROM sys.database_principals
	WHERE name = 'AttendanceTrackerUser')
BEGIN
CREATE USER [AttendanceTrackerUser] FOR LOGIN [AttendanceTrackerUser]

GRANT INSERT ON SCHEMA::[dbo] TO [AttendanceTrackerUser];
GRANT UPDATE ON SCHEMA::[dbo] TO [AttendanceTrackerUser];
GRANT DELETE ON SCHEMA::[dbo] TO [AttendanceTrackerUser];
GRANT SELECT ON SCHEMA::[dbo] TO [AttendanceTrackerUser];
GRANT EXECUTE ON SCHEMA::[dbo] TO [AttendanceTrackerUser];

END

GO