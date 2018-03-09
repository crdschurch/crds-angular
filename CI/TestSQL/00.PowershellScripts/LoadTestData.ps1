param (
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
)

#Call scripts in subfolders relative to current script
$root_path = (Split-Path (Split-Path ($MyInvocation.MyCommand.Definition)))

$errors = 0
#Load UserData folder
. "$($root_path)\01.TestUsers\01.Contact+Relationship\CreateContactsAndRelationships.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\01.TestUsers\02.Donor\CreateDonors.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\01.TestUsers\03.Participant\CreateParticipants.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\01.TestUsers\04.Household+Address\CreateHouseholdsAndAddresses.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE


#Load TestData folder
. "$($root_path)\02.TestData\01.Batch+Deposit\CreateDepositsAndBatches.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\02.Program\CreatePrograms.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\03.Group+Attribute\CreateGroupsAndAttributes.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\04.Event+EventType\CreateEventsAndEventTypes.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\05.Opportunity+Response\CreateOpportunitiesAndResponses.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\06.PledgeCampaign+Pledge\CreatePledgeCampaignsAndPledges.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\07.Donation+Distribution\CreateDonations.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\08.Invoice+Payment\CreateInvoicesAndPayments.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE
. "$($root_path)\02.TestData\09.EventParticipants+GroupParticipants\CreateEventAndGroupParticipants.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword
$errors += $LASTEXITCODE

if($errors -ne 0){
	exit 1
}