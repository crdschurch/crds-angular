# Executes database restore of the MinistryPlatform database
# Parameters:
#   -DBServer servername_or_ip   The database server, defaults to mp-int-db (optional)
#   -DBName databaseName         The database to backup (optional, defaults to MinistryPlatform)
#   -BackupPath path_on_server   The backup file path on the DBServer (required)
#   -DBUser user                 The SQLServer user to login to the DBServer (optional, defaults to environment variable MP_TARGET_DB_USER)
#   -DBPassword password         The SQLServer password to login to the DBServer (optional, defaults to environment variable MP_TARGET_DB_PASSWORD)

Param (
  [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com", # default to mp-int-db
  [string]$DBName = "MinistryPlatform", # default to MinistryPlatform
  [string]$BackupPath = $(throw "-BackupPath (backup file path on the DBServer) is required."),
  [string]$DBUser = $(Get-ChildItem Env:MP_TARGET_DB_USER).Value, # Default to environment variable
  [string]$DBPassword = $(Get-ChildItem Env:MP_TARGET_DB_PASSWORD).Value # Default to environment variable
)
Write-Output "Starting database restore script at $(Get-Date)"

$connectionString = "Server=$DBServer;uid=$DBUser;pwd=$DBPassword;Database=master;Integrated Security=False;";

$connection = New-Object System.Data.SqlClient.SqlConnection;
$connection.ConnectionString = $connectionString;
$connection.Open();

# Add SQL Message output to the console
$sqlMessageHandler = [System.Data.SqlClient.SqlInfoMessageEventHandler] {param($sender, $event) Write-Host $event.Message };
$connection.add_InfoMessage($sqlMessageHandler);

# Restore the database - need to take it offline, restore, then bring back online
$snapshotDBName = $DBName + "_Snapshot";

$restoreSql = @"
USE [master];

ALTER DATABASE [$DBName] SET OFFLINE WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [$DBName] 
FROM DATABASE_SNAPSHOT = '$snapshotDBName';

ALTER DATABASE [$DBName] SET ONLINE;
"@;

$command = $connection.CreateCommand();
$command.CommandText = "$restoreSql";
$command.CommandTimeout = 600000;

$exitCode = 0;
$exitMessage = "Success";

Write-Output "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Beginning restore from snapshot $snapshotDBName on server $DBServer"
try {
  $command.ExecuteNonQuery();
} catch [System.Exception] {
  $exitCode = 1;
  $exitMessage = "ERROR - Restore failed: " + $_.Exception.Message;
}
Write-Output "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Finished restore from snapshot $snapshotDBName on server $DBServer"

# If the restore failed, exit now
if($exitCode -ne 0) {
  Write-Output "Status: $exitMessage";
  exit $exitCode;
}

# Now update some environment-specific data in the database
$updateSql = @"
-- The API password
DECLARE @apiPassword varchar(30) = 'xrds8253%%(';
-- The internal server name, if accessible at a different URL internally versus externally
DECLARE @internalServerName varchar(75) = 'MP-INT-WEB';
-- The external server name
DECLARE @externalServerName varchar(75) = 'adminint.crossroads.net';
-- The name of the site
DECLARE @applicationTitle varchar(30) = 'Crossroads Integration';
-- The user which will be logging in to the MP database from the ministryplatform and ministryplatformapi apps
--DECLARE @dbLoginUser varchar(50) = '[MP-INT-DB\MPUser]';
-- The user which will be running the Windows scheduled tasks from the WEB server
DECLARE @scheduledTasksUser varchar(50) = 'MP-INT-WEB\MPAdmin';
-- The domain GUID - set this to NEWID() when setting up a new domain, but use a previous value for an existing domain
--DECLARE @domainGuid = NEWID();
DECLARE @domainGuid UNIQUEIDENTIFIER = CAST('0FDE7F32-37E3-4E0B-B020-622E0EBD6BF0' AS UNIQUEIDENTIFIER);

USE $DBName;

SELECT * FROM dp_Domains;

UPDATE [dbo].[dp_Domains]
   SET [Internal_Server_Name] = @internalServerName
      ,[External_Server_Name] = @externalServerName
      ,[Application_Title] = @applicationTitle
      ,[Domain_GUID] = @domainGuid
      --,[API_Service_Password] = @apiPassword -- Commented by John Cleaver 4/5/17 pending info from TM
      --,[GMT_Offset] = -5 Removed by Andy Canterbury on 7/29/2016 to fix TeamCity build.
      ,[Company_Contact] = 5
      ,[Database_Name] = null
      ,[Max_Secured_Users] = null;

SELECT * FROM dp_Domains;

-- The following Scripts are necessary to enable the application to work with the database.
-- Please don't adjust anything by the Database Name in these scripts.

USE master;

-- Create login for Network Service
IF NOT EXISTS
(
	SELECT * FROM syslogins	WHERE [loginname] = 'NT AUTHORITY\NETWORK SERVICE'
)
BEGIN
	CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS
		WITH DEFAULT_DATABASE = [$DBName], DEFAULT_LANGUAGE = [us_english];
END;

-- Execute in $DBName database
USE [$DBName];

-- Update the user identity for mpadmin with the proper mpadmin user
UPDATE [dbo].[dp_User_Identities]
   SET [Value] = @scheduledTasksUser
WHERE
   [User_Identity_ID] = 1;

-- Create role
IF NOT EXISTS
(
	SELECT * FROM sys.database_principals WHERE name = 'db_executor' and [type] = 'R'
)
BEGIN
	CREATE ROLE db_executor;
	GRANT EXECUTE TO db_executor;
END;

-- Create database user for Network Service
IF NOT EXISTS
(
	SELECT * FROM sys.database_principals WHERE name = 'NT AUTHORITY\NETWORK SERVICE'
)
BEGIN
	CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE]	WITH DEFAULT_SCHEMA = dbo;

	EXEC sp_addrolemember 'db_datawriter', 'NT AUTHORITY\NETWORK SERVICE';
	EXEC sp_addrolemember 'db_datareader', 'NT AUTHORITY\NETWORK SERVICE';
	EXEC sp_addrolemember 'db_executor', 'NT AUTHORITY\NETWORK SERVICE';
END;

-- Enable service broker
ALTER DATABASE $DBName SET TRUSTWORTHY ON;

IF NOT EXISTS
(
	SELECT is_broker_enabled FROM sys.databases WHERE name = '$DBName' AND is_broker_enabled = 1
)
BEGIN
	ALTER DATABASE $DBName SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	ALTER DATABASE $DBName SET MULTI_USER;

	ALTER DATABASE $DBName SET NEW_BROKER;
	ALTER DATABASE $DBName SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;

	-- Grant service broker permissins to Network Service
	GRANT CREATE PROCEDURE TO [NT AUTHORITY\NETWORK SERVICE];
	GRANT CREATE SERVICE TO [NT AUTHORITY\NETWORK SERVICE];
	GRANT CREATE QUEUE TO [NT AUTHORITY\NETWORK SERVICE];
	GRANT CONTROL ON SCHEMA::[dbo] TO [NT AUTHORITY\NETWORK SERVICE];
	GRANT IMPERSONATE ON USER::[dbo] TO [NT AUTHORITY\NETWORK SERVICE];
	GRANT REFERENCES ON CONTRACT::[http://schemas.microsoft.com/SQL/Notifications/PostQueryNotification] TO [NT AUTHORITY\NETWORK SERVICE];
	GRANT SUBSCRIBE QUERY NOTIFICATIONS TO [NT AUTHORITY\NETWORK SERVICE];
END;

ALTER AUTHORIZATION ON DATABASE::$DBName to sa;
"@;

$command = $connection.CreateCommand();
$command.CommandText = "$updateSql";
$command.CommandTimeout = 600000;

Write-Output "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Beginning update of database $DBName on server $DBServer"
try {
  $command.ExecuteNonQuery();
} catch [System.Exception] {
  $exitCode = 1;
  $exitMessage = "ERROR - Update failed: " + $_.Exception.Message;
}
Write-Output "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Finished update of database $DBName on server $DBServer"

Write-Output "Status: $exitMessage"
exit $exitCode
