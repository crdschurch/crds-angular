param (
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
)

#Call scripts in subfolders relative to current script
$root_path = (Split-Path (Split-Path ($MyInvocation.MyCommand.Definition)))

$errors = 0
#Delete from UserData folder
. "$($root_path)\01.TestUsers\DeleteTestUsers.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE

#Delete from TestData folder
. "$($root_path)\02.TestData\01.Batch+Deposit\DeleteDepositsAndBatches.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE
. "$($root_path)\02.TestData\02.Program\DeletePrograms.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE
. "$($root_path)\02.TestData\03.Group+Attribute\DeleteGroupsAndAttributes.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE
. "$($root_path)\02.TestData\04.Event+EventType\DeleteEventsAndEventTypes.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE
. "$($root_path)\02.TestData\05.Opportunity+Response\DeleteOpportunities.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE
. "$($root_path)\02.TestData\06.PledgeCampaign+Pledge\DeletePledgeCampaigns.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTERRORCODE

if($errors -ne 0){
	exit 1
}