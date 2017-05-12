﻿param (
    [string]$BackupDBServer = "mp-demo-db.centralus.cloudapp.azure.com",
    [string]$RestoreDBServer = "mp-demo-db.centralus.cloudapp.azure.com\TestDBScripts,2433",
    [string]$DBName = "MinistryPlatform", # default to MinistryPlatform
    [string]$ScriptPath = $(throw "-ScriptPath is required."),
    [string]$BackupPath = "F:\Backups\FromProduction",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value, # Default to environment variable
    [string]$SQLcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\110\Tools\Binn\sqlcmd.exe",
    [switch]$ForceBackup = $FALSE, # Default to use existing backup file,
    [switch]$RunIfNoScriptChanges = $FALSE, # Default to not running if changes to CI/SQL folder
    [string]$ChangeLogFile # Use teamcity's list of changes log to determine if we need to run
)

try
{
    $SQLChanges = @(Get-Content $changeLogFile | Where-Object {$_.StartsWith("CI/SQL")}).Count
} 
catch [System.Exception] 
{
    echo "ERROR - Looking for changed scripts: " + $_.Exception.Message;
    exit 1
}

if(($SQLChanges -eq 0) -and ($RunIfNoScriptChanges -eq $FALSE))
{
    echo "No database changes found. Skipping backup, restore, and running scripts"
    exit 0	
}

echo "Found $SQLChanges sql files changed"

#Use mutex to ensure only 1 process executing against DBServer / DB at a time
$uniqueName = "MPDemoDatabaseRestoreAndRunScripts$DBServer$DBName" 
$singleInstanceMutex = New-Object System.Threading.Mutex($false, $uniqueName)

try
{   
	$singleInstanceMutex.WaitOne()
    echo "Aquired Mutex at $(Get-Date)"

	.\CI\MPDemoDatabaseBackup.ps1 -DBServer $BackupDBServer -DBName $DBName -BackupPath $BackupPath -DBUser $DBUser -DBPassword $DBPassword -ForceBackup $ForceBackup

    if($LASTEXITCODE -eq 0) 
    {
        .\CI\MPTestDatabasePrepForQuickRestores.ps1 -DBServer $RestoreDBServer -DBName $DBName -BackupPath $BackupPath -DBUser $DBUser -DBPassword $DBPassword -ForceBackup $ForceBackup
    }

    if($LASTEXITCODE -eq 0) 
    {
        .\CI\MPTestDatabaseRestore.ps1 -DBServer $RestoreDBServer -DBName $DBName -BackupPath $BackupPath -DBUser $DBUser -DBPassword $DBPassword
    }

    if($LASTEXITCODE -eq 0)
    {
        .\CI\ScriptProcessing.ps1 -DBServer $RestoreDBServer -path $ScriptPath -SQLcmd $SQLcmd -DBUser $DBUser -DBPassword $DBPassword
    }

    exit $LASTEXITCODE
}
finally
{
	$singleInstanceMutex.ReleaseMutex()
	$singleInstanceMutex.Dispose()
}