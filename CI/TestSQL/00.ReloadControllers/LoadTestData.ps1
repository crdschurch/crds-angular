param (
    [string]$contactDataCSV,
    [string]$contactRelationshipsDataCSV,
    [string]$donorDataCSV,
    [string]$particpantDataCSV,
    [string]$householdDataCSV,
    [string]$householdAddressDataCSV,
    [string]$contactsInHouseholdDataCSV,
	[string]$depositDataCSV,
    [string]$batchDataCSV,
	[string]$depositBatchLinkDataCSV,
	[string]$programDataCSV,
	[string]$groupDataCSV,
    [string]$addChildGroupDataCSV,
	[string]$attributeDataCSV,
    [string]$groupAttributeDataCSV,
	[string]$eventTypeDataCSV,
    [string]$eventDataCSV,
	[string]$opportunityDataCSV,
	[string]$responseDataCSV,
	[string]$pledgeCampaignDataCSV,
	[string]$pledgeDataCSV,
	[string]$donationDataCSV,
	[string]$donationToPledgeDataCSV,
	[string]$invoiceAndPaymentDataCSV,
    [string]$DBServer = "mp-demo-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
)

#Call scripts in subfolders relative to current script
$root_path = (Split-Path (Split-Path ($MyInvocation.MyCommand.Definition)))


#Load UserData folder
<#. "$($root_path)\01.TestUsers\01.Contact+Relationship\CreateContactsAndRelationships.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -contactDataCSV $contactDataCSV -contactRelationshipsDataCSV $contactRelationshipsDataCSV
. "$($root_path)\01.TestUsers\02.Donor\CreateDonors.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword  -donorDataCSV $donorDataCSV
. "$($root_path)\01.TestUsers\03.Participant\CreateParticipants.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword  -particpantDataCSV $particpantDataCSV
. "$($root_path)\01.TestUsers\04.Household+Address\CreateHouseholdsAndAddresses.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -householdDataCSV $householdDataCSV -householdAddressDataCSV $householdAddressDataCSV -contactsInHouseholdDataCSV $contactsInHouseholdDataCSV
#>

#Load TestData folder
TODO test all below
#. "$($root_path)\02.TestData\01.Batch+Deposit\CreateDepositsAndBatches.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -depositDataCSV $depositDataCSV -batchDataCSV $batchDataCSV -depositBatchLinkDataCSV $depositBatchLinkDataCSV
. "$($root_path)\02.TestData\02.Program\CreatePrograms.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword# -programDataCSV $programDataCSV
#. "$($root_path)\02.TestData\03.Group+Attribute\CreateGroupsAndAttributes.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -groupDataCSV $groupDataCSV -addChildGroupDataCSV $addChildGroupDataCSV -attributeDataCSV $attributeDataCSV -groupAttributeDataCSV $groupAttributeDataCSV
#. "$($root_path)\02.TestData\04.Event+EventType\CreateEventsAndEventTypes.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -eventTypeDataCSV $eventTypeDataCSV -eventDataCSV $eventDataCSV
#. "$($root_path)\02.TestData\05.Opportunity+Response\CreateOpportunitiesAndResponses.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -opportunityDataCSV $opportunityDataCSV -responseDataCSV $responseDataCSV
#. "$($root_path)\02.TestData\06.PledgeCampaign+Pledge\CreatePledgeCampaignsAndPledges.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -pledgeCampaignDataCSV $pledgeCampaignDataCSV -pledgeDataCSV $pledgeDataCSV
#. "$($root_path)\02.TestData\07.Donation+Distribution\CreateDonations.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -donationDataCSV $donationDataCSV -donationToPledgeDataCSV $donationToPledgeDataCSV
#. "$($root_path)\02.TestData\08.Invoice+Payment\CreateInvoicesAndPayments.ps1" -DBServer $DBServer -DBUser $DBUser -DBPassword $DBPassword -invoiceAndPaymentDataCSV $invoiceAndPaymentDataCSV


#Load TestConfigData
#TODO