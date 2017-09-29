# Executes database restore of the MinistryPlatform database
# Parameters:
#   -DBServer servername_or_ip   The database server, defaults to mp-int-db (optional)
#   -DBName databaseName         The database to backup (optional, defaults to MinistryPlatform)
#   -BackupPath path_on_server   The backup file path on the DBServer (required)
#   -DBUser user                 The SQLServer user to login to the DBServer (optional, defaults to environment variable MP_TARGET_DB_USER)
#   -DBPassword password         The SQLServer password to login to the DBServer (optional, defaults to environment variable MP_TARGET_DB_PASSWORD)

Param (
  [Parameter(Mandatory=$true)]
  [string]$DBServer,
  [string]$DBName = "MinistryPlatform", # default to MinistryPlatform
  [Parameter(Mandatory=$true)]
  [string]$BackupUrl,
  [string]$DBUser = $(Get-ChildItem Env:MP_TARGET_DB_USER).Value, # Default to environment variable
  [string]$DBPassword = $(Get-ChildItem Env:MP_TARGET_DB_PASSWORD).Value, # Default to environment variable
  [Parameter(Mandatory=$true)]
  [string]$StorageCred,
  [Parameter(Mandatory=$true)]
  [string] $InternalServerName,
  [Parameter(Mandatory=$true)]
  [string] $ExternalServerName,
  [Parameter(Mandatory=$true)]
  [string] $ApplicationTitle,
  [Parameter(Mandatory=$true)]
  [string] $ApiPassword,
  [Parameter(Mandatory=$true)]
  [string] $InternalDBServerName,
  [Parameter(Mandatory=$true)]
  [string] $DomainGuid,
  [Parameter(Mandatory=$true)]
  [string] $EmailServer,
  [Parameter(Mandatory=$true)]
  [string] $EmailUserName,
  [Parameter(Mandatory=$true)]
  [string] $RegisterApiPasswordHash,
  [Parameter(Mandatory=$true)]
  [string] $BaseUri,
  [Parameter(Mandatory=$true)]
  [string] $ReportingServerAddress,
  [Parameter(Mandatory=$true)]
  [string] $SMTPServerName,
  [Parameter(Mandatory=$true)]
  [string] $SMTPServerPort,
  [Parameter(Mandatory=$true)]
  [string]$DpToolUriToBeReplaced,
  [Parameter(Mandatory=$true)]
  [string]$DpToolNewUri
)

$SourceDBName = "MinistryPlatform";

$backupDateStamp = Get-Date -format 'yyyyMMdd';
$BackupUrl = "$BackupUrl/$SourceDBName-Backup-$backupDateStamp.trn";

$connectionString = "Server=$DBServer;uid=$DBUser;pwd=$DBPassword;Database=master;Integrated Security=False;";

$connection = New-Object System.Data.SqlClient.SqlConnection;
$connection.ConnectionString = $connectionString;
$connection.Open();

# Determine the current log and data file locations, so we can relocate from the backup.
# This is needed because the servers are not setup with identical drives and paths.
$sql = @"
SELECT type, name, physical_name
FROM sys.master_files
WHERE [database_id] = DB_ID('$DBName')
ORDER BY type, name;
"@;

$command = $connection.CreateCommand();
$command.CommandText = "$sql";

$reader = $command.ExecuteReader();

$table = New-Object System.Data.DataTable;
$table.Load($reader);

$dataFile = $table | Where-Object {$_.type -eq 0};
$logFile = $table | Where-Object {$_.type -eq 1};

$dataFileName = $dataFile.name;
$dataFilePhysicalName = $dataFile.physical_name;

$logFileName = $logFile.name;
$logFilePhysicalName = $logFile.physical_name;

# Restore the database - need to take it offline, restore, then bring back online
$restoreSql = @"
USE [master];

ALTER DATABASE [$DBName] SET OFFLINE WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [$DBName]
FROM URL = N'$backupUrl' 
WITH CREDENTIAL = N'$StorageCred', FILE = 1, NOUNLOAD, REPLACE, STATS = 5,
MOVE N'$logFileName' TO N'$logFilePhysicalName',
MOVE N'$dataFileName' TO N'$dataFilePhysicalName';

ALTER DATABASE [$DBName] SET ONLINE;
"@;

$command = $connection.CreateCommand();
$command.CommandText = "$restoreSql";
$command.CommandTimeout = 600000;

$exitCode = 0;
$exitMessage = "Success";

echo "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Beginning restore from file $backupFileName on server $DBServer"
try {
  $command.ExecuteNonQuery();
} catch [System.Exception] {
  $exitCode = 1;
  $exitMessage = "ERROR - Restore failed: " + $_.Exception.Message;
}
echo "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Finished restore from file $backupFileName on server $DBServer"

# If the restore failed, exit now
if($exitCode -ne 0) {
  echo "Status: $exitMessage";
  exit $exitCode;
}

# Now update some environment-specific data in the database
$updateSql = @"
-- The API password
DECLARE @apiPassword varchar(30) = '$ApiPassword';

-- The internal server name, if accessible at a different URL internally versus externally
DECLARE @internalServerName varchar(75) = '$InternalServerName';

-- The external server name
DECLARE @externalServerName varchar(75) = '$ExternalServerName';

-- The name of the site
DECLARE @applicationTitle varchar(30) = '$ApplicationTitle';

-- The user which will be logging in to the MP database from the ministryplatform and ministryplatformapi apps
DECLARE @dbLoginUser varchar(50) = '$InternalServerName\MPUser';

-- The user which will be running the Windows scheduled tasks from the WEB server
DECLARE @scheduledTasksUser varchar(50) = '$InternalServerName\MPAdmin';

-- The domain GUID - set this to NEWID() when setting up a new domain, but use a previous value for an existing domain
-- DECLARE @domainGuid = NEWID();
DECLARE @domainGuid UNIQUEIDENTIFIER = CAST('$DomainGuid' AS UNIQUEIDENTIFIER);

DECLARE @baseUri nvarchar(128) = '$BaseUri';

-- Reporting Server Address
DECLARE @reportingServerAddress nvarchar(128) = '$ReportingServerAddress';

-- SMTP Server Name
DECLARE @SMTPServerName nvarchar(50) = '$SMTPServerName';

DECLARE @SMTPServerPort int = $SMTPServerPort;


USE [$DBName];

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
      ,[Max_Secured_Users] = null
	  ,[Base_Uri] = @baseUri
	  ,[Database_Server_Name] = null
	  ,[Reporting_Server_Address] = @reportingServerAddress
	  ,[SMTP_Server_Name] = @SMTPServerName
	  ,[SMTP_Server_Port] = @SMTPServerPort
      ,[SMTP_Server_Username] = null
      ,[SMTP_Server_Password] = null
      ,[SMTP_Enable_SSL] = 0;

SELECT * FROM dp_Domains;

-- Update register_api users password hash
UPDATE dp_Users SET Password = $RegisterApiPasswordHash WHERE User_Name = 'register_api'

-- Update Mobile Tools settings
USE [$DBName]

CREATE TABLE #NewConfigSettings (Application_Code VARCHAR(500), Key_Name VARCHAR(500), [Value] VARCHAR(500))

INSERT INTO #NewConfigSettings 
	(Application_Code, Key_Name, Value) 
	VALUES
	('MOBILETOOLS', 'EmailServer', '$EmailServer'),
	('MOBILETOOLS', 'EmailMode', 'SMTP'),
	('MOBILETOOLS', 'EmailServerPort', '3000'),
	('MOBILETOOLS', 'EmailUserName', '$EmailUserName'),
	('MOBILETOOLS', 'EmailPassword', 'ShouldNotMatter')

UPDATE s
		SET s.Value = new.Value
	FROM dp_Configuration_Settings s
		INNER JOIN #NewConfigSettings new ON 
			new.Application_Code = s.Application_Code AND
			new.Key_Name = s.Key_Name
	WHERE s.Value != new.Value

DROP TABLE #NewConfigSettings;

-- Delete the processor id for all donors
DISABLE TRIGGER Donors.tr_DisallowDefaultDonorUpdate ON dbo.Donors;

UPDATE Donors SET Processor_ID = '';

ENABLE TRIGGER Donors.tr_DisallowDefaultDonorUpdate ON dbo.Donors;

-- Update dp_Tools Launch_Page value
UPDATE dp_Tools
    SET Launch_Page = REPLACE(Launch_Page, '$DpToolUriToBeReplaced', '$DpToolNewUri')

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

	-- Grant service broker permissins to Network Service
	GRANT CREATE PROCEDURE TO [$InternalDBServerName\MPUser]
	GRANT CREATE SERVICE TO [$InternalDBServerName\MPUser]
	GRANT CREATE QUEUE TO [$InternalDBServerName\MPUser]
	GRANT CONTROL ON SCHEMA::[dbo] TO [$InternalDBServerName\MPUser]
	GRANT IMPERSONATE ON USER::[dbo] TO [$InternalDBServerName\MPUser]
	GRANT REFERENCES ON CONTRACT::[http://schemas.microsoft.com/SQL/Notifications/PostQueryNotification] TO [$InternalDBServerName\MPUser]
	GRANT SUBSCRIBE QUERY NOTIFICATIONS TO [$InternalDBServerName\MPUser]
END;

USE [$DBName];

CREATE USER [$InternalDBServerName\MPUser] FOR LOGIN [$InternalDBServerName\MPUser];
ALTER ROLE [db_accessadmin] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_backupoperator] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_datareader] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_datawriter] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_ddladmin] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_executor] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_owner] ADD MEMBER [$InternalDBServerName\MPUser];
ALTER ROLE [db_securityadmin] ADD MEMBER [$InternalDBServerName\MPUser];


-- TODO: Verify that mapped users works
exec sp_change_users_login @Action='update_one', @UserNamePattern='ApiUser', @LoginName='ApiUser'
exec sp_change_users_login @Action='update_one', @UserNamePattern='EcheckAgent', @LoginName='EcheckAgent'
exec sp_change_users_login @Action='update_one', @UserNamePattern='MigrateUser', @LoginName='MigrateUser'
exec sp_change_users_login @Action='update_one', @UserNamePattern='NewRelic', @LoginName='NewRelic'

-- TODO: Review, Rework, and determine plan for mapping users
USE [$DBName]

CREATE USER [$InternalDBServerName\CRDSAdmin] FOR LOGIN [$InternalDBServerName\CRDSAdmin];

ALTER ROLE [db_accessadmin] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_backupoperator] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_datareader] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_datawriter] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_ddladmin] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_executor] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_owner] ADD MEMBER [$InternalDBServerName\CRDSAdmin];
ALTER ROLE [db_securityadmin] ADD MEMBER [$InternalDBServerName\CRDSAdmin];

CREATE USER [$InternalDBServerName\MPAdmin] FOR LOGIN [$InternalDBServerName\MPAdmin];

ALTER ROLE [db_accessadmin] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_backupoperator] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_datareader] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_datawriter] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_ddladmin] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_executor] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_owner] ADD MEMBER [$InternalDBServerName\MPAdmin];
ALTER ROLE [db_securityadmin] ADD MEMBER [$InternalDBServerName\MPAdmin];

ALTER AUTHORIZATION ON DATABASE::$DBName to sa;
"@;

$command = $connection.CreateCommand();
$command.CommandText = "$updateSql";
$command.CommandTimeout = 600000;

echo "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Beginning update of database $DBName on server $DBServer"
try {
  $command.ExecuteNonQuery();
} catch [System.Exception] {
  $exitCode = 1;
  $exitMessage = "ERROR - Update failed: " + $_.Exception.Message;
}
echo "$(Get-Date -format 'yyyy-MM-dd HH:mm:ss') Finished update of database $DBName on server $DBServer"

echo "Status: $exitMessage"
exit $exitCode
