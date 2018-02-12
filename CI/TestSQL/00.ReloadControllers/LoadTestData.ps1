param (
    [string]$contactDataCSV,
    [string]$contactRelationshipsDataCSV,
    [string]$donorDataCSV,
    [string]$particpantDataCSV,
    [string]$householdDataCSV,
    [string]$householdAddressDataCSV,
    [string]$contactsInHouseholdDataCSV,
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
)

#Call scripts in subfolders relative to current script
$root_path = (Split-Path (Split-Path ($MyInvocation.MyCommand.Definition)))


#Load User data
. "$($root_path)\01.TestUsers\01.Contact+Relationship\CreateContactsAndRelationships.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -contactDataCSV $contactDataCSV -contactRelationshipsDataCSV $contactRelationshipsDataCSV
. "$($root_path)\01.TestUsers\02.Donor\CreateDonors.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword  -donorDataCSV $donorDataCSV
. "$($root_path)\01.TestUsers\03.Participant\CreateParticipants.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword  -particpantDataCSV $particpantDataCSV
. "$($root_path)\01.TestUsers\04.Household+Address\CreateHouseholdsAndAddresses.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -householdDataCSV $householdDataCSV -householdAddressDataCSV $householdAddressDataCSV -contactsInHouseholdDataCSV $contactsInHouseholdDataCSV
