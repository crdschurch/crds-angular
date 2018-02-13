param (
    [string]$donationDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonation.csv"),
	[string]$donationToPledgeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonationToPledge.csv"),
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

. ((Split-Path $MyInvocation.MyCommand.Definition)+"\..\..\00.ReloadControllers\DBCommand.ps1") #should avoid dot-source errors

function OpenConnection{
	$DBConnection = new-object System.Data.SqlClient.SqlConnection 
	$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
	$DBConnection.Open();
	return $DBConnection
}

#Create all donations in list
function CreateDonation($DBConnection){
	$donationDataList = import-csv $donationDataCSV
	
	foreach($donationRow in $donationDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_DonationWithDistribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@donor_email" $donationRow.R_Donor_Email
			AddStringParameter $command "@donation_amount" $donationRow.R_Donation_Amount
			AddDateParameter $command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $command "@payment_type_id" $donationRow.R_Payment_Type_ID
			AddIntParameter $command "@donation_status" $donationRow.R_Donation_Status_ID
			AddBitParameter $command "@receipted" $donationRow.R_Receipted
			AddBitParameter $command "@anonymous" $donationRow.Anonymous
			AddDateParameter $command "@status_date" $donationRow.Status_Date
			AddStringParameter $command "@status_notes" $donationRow.Status_Notes
			AddBitParameter $command "@processed" $donationRow.Processed
			AddStringParameter $command "@batch_name" $donationRow.Batch_Name
			AddIntParameter $command "@congregation_id" $donationRow.Congregation_ID
			AddStringParameter $command "@item_number" $donationRow.Item_Number
			AddStringParameter $command "@donation_notes" $donationRow.Donation_Notes
			AddStringParameter $command "@processor_id" $donationRow.Processor_ID
			AddStringParameter $command "@transaction_code" $donationRow.Transaction_Code
			AddStringParameter $command "@program_name" $donationRow.R_Program_Name
			AddStringParameter $command "@pledge_user_email" ""
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@donation_id" "Int32"
			AddOutputParameter $command "@distribution_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$donation_created = LogResult $command "@donation_id" "Donation created"
			$distribution_created = LogResult $command "@distribution_id" "        with Donation Distribution"
			
			if(!$donation_created){
				throw
			}			
		}
	}
}

#Create all donations towards pledges in list
function CreateDonationToPledge($DBConnection){
	$donationToPledgeDataList = import-csv $donationToPledgeDataCSV
	
	foreach($donationRow in $donationToPledgeDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_DonationWithDistribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@donor_email" $donationRow.R_Donor_Email
			AddStringParameter $command "@donation_amount" $donationRow.R_Donation_Amount
			AddDateParameter $command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $command "@payment_type_id" $donationRow.R_Payment_Type_ID
			AddIntParameter $command "@donation_status" $donationRow.R_Donation_Status_ID
			AddBitParameter $command "@receipted" $donationRow.R_Receipted
			AddBitParameter $command "@anonymous" $donationRow.Anonymous
			AddDateParameter $command "@status_date" ""
			AddStringParameter $command "@status_notes" ""
			AddBitParameter $command "@processed" ""
			AddStringParameter $command "@batch_name" ""
			AddIntParameter $command "@congregation_id" $donationRow.Congregation_ID
			AddStringParameter $command "@item_number" ""
			AddStringParameter $command "@donation_notes" ""
			AddStringParameter $command "@processor_id" ""
			AddStringParameter $command "@transaction_code" ""
			AddStringParameter $command "@program_name" $donationRow.R_Program_Name
			AddStringParameter $command "@pledge_user_email" $donationRow.R_Pledge_Owner_Email
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@donation_id" "Int32"
			AddOutputParameter $command "@distribution_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$donation_created = LogResult $command "@donation_id" "Donation created"
			$distribution_created = LogResult $command "@distribution_id" "        with Donation Distribution"
			
			if(!$donation_created){
				throw
			}			
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateDonation $DBConnection
	CreateDonationToPledge $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}