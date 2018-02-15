param (
    [string]$donationDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonations_DonorsHaveUserAccounts.csv"),
	[string]$donationNoUserDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonations_NoUserAccounts.csv"),
	[string]$donationTwoDistributionsDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonations_TwoDistributions.csv"),
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
function CreateDonations($DBConnection){
	$donationDataList = import-csv $donationDataCSV
	
	foreach($donationRow in $donationDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Email))
		{
			#Create command to create donation
			$d_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $d_command "@donor_email" $donationRow.R_Donor_Email
			AddMoneyParameter $d_command "@donation_amount" $donationRow.R_Donation_Amount
			AddDateParameter $d_command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $d_command "@payment_type_id" $donationRow.R_Payment_Type_Id
			AddIntParameter $d_command "@donation_status" $donationRow.R_Donation_Status_Id
			AddBitParameter $d_command "@receipted" $donationRow.R_Receipted
			AddBitParameter $d_command "@anonymous" $donationRow.Anonymous
			AddDateParameter $d_command "@status_date" $donationRow.Status_Date
			AddStringParameter $d_command "@status_notes" $donationRow.Status_Notes
			AddBitParameter $d_command "@processed" $donationRow.Processed
			AddStringParameter $d_command "@batch_name" $donationRow.Batch_Name
			AddStringParameter $d_command "@item_number" $donationRow.Item_Number
			AddStringParameter $d_command "@donation_notes" $donationRow.Donation_Notes
			AddStringParameter $d_command "@processor_id" $donationRow.Processor_Id
			AddStringParameter $d_command "@transaction_code" $donationRow.Transaction_Code
			AddIntParameter $d_command "@non_cash_asset_type_id" $donationRow.Non-Cash_Asset_Type_Id
			AddOutputParameter $d_command "@error_message" "String"
			AddOutputParameter $d_command "@donation_id" "Int32"
		
			#Execute and report results for donation creation
			$result = $d_command.ExecuteNonQuery()
			$error_found = LogResult $d_command "@error_message" "ERROR"
			$donation_created = LogResult $d_command "@donation_id" "Donation created"
			
			if(!$donation_created){
				throw
			}
			
			$donation_id = $d_command.Parameters["@donation_id"].Value
		
		
			#Create command to create donation distribution
			$dd_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddIntParameter $dd_command "@donation_id" $donation_id
			AddMoneyParameter $dd_command "@distribution_amount" $donationRow.R_Donation_Amount
			AddStringParameter $dd_command "@program_name" $donationRow.R_Program_Name
			AddStringParameter $dd_command "@pledge_donor_email" $donationRow.Pledge_Owner_Email
			AddStringParameter $dd_command "@soft_credit_donor_email" $donationRow.Soft_Credit_Donor_Email
			AddIntParameter $dd_command "@congregation_id" $donationRow.Congregation_Id
			AddStringParameter $dd_command "@notes" ""
			AddOutputParameter $dd_command "@error_message" "String"
			AddOutputParameter $dd_command "@distribution_id" "Int32"
			
			#Execute and report results for donation distribution creation
			$result = $dd_command.ExecuteNonQuery()
			$error_found = LogResult $dd_command "@error_message" "ERROR"
			$distribution_created = LogResult $dd_command "@distribution_id" "        with Donation Distribution"
			
			if(!$distribution_created){
				throw
			}
		}
	}
}

#Create all donations in the list
function CreateDonationNoUserAccount($DBConnection){
	$donationNoUserDataList = import-csv $donationNoUserDataCSV
	
	foreach($donationRow in $donationNoUserDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Display_Name))
		{	
			#Get contact without user account
			$c_command = CreateStoredProcCommand $DBConnection "cr_QA_Get_Contact_No_User_Acount"
			AddStringParameter $c_command "@display_name" $donationRow.R_Donor_Display_Name
			AddStringParameter $c_command "@contact_email" $donationRow.Donor_Email
			AddOutputParameter $c_command "@error_message" "String"
			AddOutputParameter $c_command "@contact_id" "Int32"
			
			$result = $c_command.ExecuteNonQuery()
			$error_found = LogResult $c_command "@error_message" "ERROR"
			$contact_found = LogResult $c_command "@contact_id" "Contact making donation"
			
			if(!$contact_found){
				throw
			}
			
			$contact_id = $c_command.Parameters["@contact_id"].Value
			
			
			#Create command to create donation
			$d_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_By_Contact_Id"
			AddIntParameter $d_command "@contact_id" $contact_id
									
			#Add parameters to command - parameter names must match stored proc parameter names
			AddMoneyParameter $d_command "@donation_amount" $donationRow.R_Donation_Amount
			AddDateParameter $d_command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $d_command "@payment_type_id" $donationRow.R_Payment_Type_Id
			AddIntParameter $d_command "@donation_status" $donationRow.R_Donation_Status_Id
			AddBitParameter $d_command "@receipted" $donationRow.R_Receipted
			AddBitParameter $d_command "@anonymous" $donationRow.Anonymous
			AddDateParameter $d_command "@status_date" $donationRow.Status_Date
			AddStringParameter $d_command "@status_notes" $donationRow.Status_Notes
			AddBitParameter $d_command "@processed" $donationRow.Processed
			AddStringParameter $d_command "@batch_name" $donationRow.Batch_Name
			AddStringParameter $d_command "@item_number" $donationRow.Item_Number
			AddStringParameter $d_command "@donation_notes" $donationRow.Donation_Notes
			AddStringParameter $d_command "@processor_id" $donationRow.Processor_Id
			AddStringParameter $d_command "@transaction_code" $donationRow.Transaction_Code
			AddIntParameter $d_command "@non_cash_asset_type_id" $donationRow.Non-Cash_Asset_Type_Id
			AddOutputParameter $d_command "@error_message" "String"
			AddOutputParameter $d_command "@donation_id" "Int32"
			
			#Execute and report results for donation creation
			$result = $d_command.ExecuteNonQuery()
			$error_found = LogResult $d_command "@error_message" "ERROR"
			$donation_created = LogResult $d_command "@donation_id" "Donation created"
			
			if(!$donation_created){
				throw
			}
			
			$donation_id = $d_command.Parameters["@donation_id"].Value
			
			
			#Create command to create donation distribution
			$dd_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddIntParameter $dd_command "@donation_id" $donation_id
			AddMoneyParameter $dd_command "@distribution_amount" $donationRow.R_Donation_Amount
			AddStringParameter $dd_command "@program_name" $donationRow.R_Program_Name
			AddStringParameter $dd_command "@pledge_donor_email" $donationRow.Pledge_Owner_Email
			AddStringParameter $dd_command "@soft_credit_donor_email" $donationRow.Soft_Credit_Donor_Email			
			AddIntParameter $dd_command "@congregation_id" $donationRow.Congregation_Id
			AddStringParameter $dd_command "@notes" ""			
			AddOutputParameter $dd_command "@error_message" "String"
			AddOutputParameter $dd_command "@distribution_id" "Int32"
			
			#Execute and report results for donation distribution creation
			$result = $dd_command.ExecuteNonQuery()
			$error_found = LogResult $dd_command "@error_message" "ERROR"
			$distribution_created = LogResult $dd_command "@distribution_id" "        with Donation Distribution"
			
			if(!$distribution_created){
				throw
			}			
		}
	}
}


#Create all donations in list
function CreateDonationsWithTwoDistributions($DBConnection){
	$donationTwoDistributionsDataList = import-csv $donationTwoDistributionsDataCSV
	
	foreach($donationRow in $donationTwoDistributionsDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Email))
		{
			#Create command to create donation
			$d_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $d_command "@donor_email" $donationRow.R_Donor_Email
			AddMoneyParameter $d_command "@donation_amount" $donationRow.R_Donation_Amount
			AddDateParameter $d_command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $d_command "@payment_type_id" $donationRow.R_Payment_Type_Id
			AddIntParameter $d_command "@donation_status" $donationRow.R_Donation_Status_Id
			AddBitParameter $d_command "@receipted" $donationRow.R_Receipted
			AddBitParameter $d_command "@anonymous" $donationRow.Anonymous
			AddDateParameter $d_command "@status_date" $donationRow.Status_Date
			AddStringParameter $d_command "@status_notes" $donationRow.Status_Notes
			AddBitParameter $d_command "@processed" $donationRow.Processed
			AddStringParameter $d_command "@batch_name" $donationRow.Batch_Name
			AddStringParameter $d_command "@item_number" $donationRow.Item_Number
			AddStringParameter $d_command "@donation_notes" $donationRow.Donation_Notes
			AddStringParameter $d_command "@processor_id" $donationRow.Processor_Id
			AddStringParameter $d_command "@transaction_code" $donationRow.Transaction_Code
			AddIntParameter $d_command "@non_cash_asset_type_id" $donationRow.Non-Cash_Asset_Type_Id
			AddOutputParameter $d_command "@error_message" "String"
			AddOutputParameter $d_command "@donation_id" "Int32"
		
			#Execute and report results for donation creation
			$result = $d_command.ExecuteNonQuery()
			$error_found = LogResult $d_command "@error_message" "ERROR"
			$donation_created = LogResult $d_command "@donation_id" "Donation created"
			
			if(!$donation_created){
				throw
			}
			
			$donation_id = $d_command.Parameters["@donation_id"].Value
		
		
			#Create command to create first donation distribution
			$dd1_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddIntParameter $dd1_command "@donation_id" $donation_id
			AddMoneyParameter $dd1_command "@distribution_amount" $donationRow.R_Distribution1_Amount
			AddStringParameter $dd1_command "@program_name" $donationRow.R_Distribution1_Program_Name
			AddStringParameter $dd1_command "@pledge_donor_email" ""
			AddStringParameter $dd1_command "@soft_credit_donor_email" ""
			AddIntParameter $dd1_command "@congregation_id" $donationRow.Congregation_Id
			AddStringParameter $dd1_command "@notes" ""
			AddOutputParameter $dd1_command "@error_message" "String"
			AddOutputParameter $dd1_command "@distribution_id" "Int32"
			
			#Execute and report results for donation distribution creation
			$result = $dd1_command.ExecuteNonQuery()
			$error_found = LogResult $dd1_command "@error_message" "ERROR"
			$distribution_created = LogResult $dd1_command "@distribution_id" "        with first Distribution"
			
			if(!$distribution_created){
				throw
			}
			
			
			#Create command to create second donation distribution
			$dd2_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddIntParameter $dd2_command "@donation_id" $donation_id
			AddMoneyParameter $dd2_command "@distribution_amount" $donationRow.R_Distribution2_Amount
			AddStringParameter $dd2_command "@program_name" $donationRow.R_Distribution2_Program_Name
			AddStringParameter $dd2_command "@pledge_donor_email" ""
			AddStringParameter $dd2_command "@soft_credit_donor_email" ""
			AddIntParameter $dd2_command "@congregation_id" $donationRow.Congregation_Id
			AddStringParameter $dd2_command "@notes" ""
			AddOutputParameter $dd2_command "@error_message" "String"
			AddOutputParameter $dd2_command "@distribution_id" "Int32"
			
			#Execute and report results for donation distribution creation
			$result = $dd2_command.ExecuteNonQuery()
			$error_found = LogResult $dd2_command "@error_message" "ERROR"
			$distribution_created = LogResult $dd2_command "@distribution_id" "        and second Distribution"
			
			if(!$distribution_created){
				throw
			}
		}
	}
}




#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateDonations $DBConnection
	CreateDonationNoUserAccount $DBConnection
	CreateDonationsWithTwoDistributions $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}