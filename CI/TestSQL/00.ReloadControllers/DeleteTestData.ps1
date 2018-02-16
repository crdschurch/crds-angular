param (
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
)

#Call scripts in subfolders relative to current script
$root_path = (Split-Path (Split-Path ($MyInvocation.MyCommand.Definition)))


#Delete from UserData folder
. "$($root_path)\01.TestUsers\DeleteTestUsers.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
