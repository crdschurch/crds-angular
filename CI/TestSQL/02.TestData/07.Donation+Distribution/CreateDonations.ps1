param (
    [string]$donationDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonations.csv"),
	[string]$donationToPledgeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonationToPledges.csv"),
	[string]$softCreditDonationDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateSoftCreditDonations.csv"),
	[string]$donationTwoDistributionsDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonationsWithTwoDistributions.csv"),
    [string]$DBServer = "mp-demo-db.centralus.cloudapp.azure.com",
    [string]$DBUser = 'MigrateUser', #$(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = 'Aw@!ted2014' #$(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
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
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_With_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@donor_email" $donationRow.R_Donor_Email
			AddIntParameter $command "@donation_amount" $donationRow.R_Donation_Amount
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
			AddOutputParameter $command "@error_message" "String" 1000
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
function CreateDonationToPledges($DBConnection){
	$donationToPledgeDataList = import-csv $donationToPledgeDataCSV
	
	foreach($donationRow in $donationToPledgeDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_With_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@donor_email" $donationRow.R_Donor_Email
			AddIntParameter $command "@donation_amount" $donationRow.R_Donation_Amount
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
			AddOutputParameter $command "@error_message" "String" 1000
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

#Create all soft credit donations in list
function OLDCreateSoftCreditDonation($DBConnection){
	$softCreditDonationDataList = import-csv $softCreditDonationDataCSV
	
	foreach($donationRow in $softCreditDonationDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Soft_Credit_Donor_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_With_Distribution_No_User"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@soft_credit_donor_email" $donationRow.R_Soft_Credit_Donor_Email
			AddStringParameter $command "@display_name" $donationRow.R_Hard_Credit_Donor_Display_Name
			AddBitParameter $command "@is_company" $donationRow.R_Is_Hard_Credit_Donor_Company
			AddStringParameter $command "@contact_email" ""			
			AddIntParameter $command "@donation_amount" $donationRow.R_Donation_Amount
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
			AddStringParameter $command "@program_name" ""
			AddStringParameter $command "@pledge_user_email" ""
			AddOutputParameter $command "@error_message" "String" 1000
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

#Create all soft credit donations in list
function CreateSoftCreditDonation($DBConnection){
	$softCreditDonationDataList = import-csv $softCreditDonationDataCSV
	
	foreach($donationRow in $softCreditDonationDataList)
	{
		if(![string]::IsNullOrEmpty($donationRow.R_Donor_Display_Name))
		{		
			#Pick Create Donation stored proc based on how we're finding the donor
			if($donationRow.R_Donor_Has_User_Account -eq '0')
			{
				#Get contact without user account
				$c_command = CreateStoredProcCommand $DBConnection "cr_QA_Get_Contact_No_User_Acount"
				AddStringParameter $c_command "@display_name" $donationRow.R_Donor_Display_Name
				AddStringParameter $c_command "@contact_email" $donationRow.Donor_Email
				AddOutputParameter $c_command "@error_message" "String" 500
				AddOutputParameter $c_command "@contact_id" "Int32"
				
				$result = $c_command.ExecuteNonQuery()
				$error_found = LogResult $c_command "@error_message" "ERROR"
				$contact_id = $c_command.Parameters["@contact_id"].Value
				
				if([string]::IsNullOrEmpty($contact_id)){
					throw
				}
				
				#Create command to create donation
				$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_By_Contact_Id"
			}
			else
			{
				#Create command to create donation
				$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation"
			}
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddIntParameter $command "@donation_amount" $donationRow.R_Donation_Amount
			AddDateParameter $command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $command "@payment_type_id" $donationRow.R_Payment_Type_ID
			AddIntParameter $command "@donation_status" $donationRow.R_Donation_Status_ID
			AddBitParameter $command "@receipted" $donationRow.R_Receipted
			AddBitParameter $command "@anonymous" $donationRow.Anonymous
			AddDateParameter $command "@status_date" $donationRow.Status_Date
			AddStringParameter $command "@status_notes" $donationRow.Status_Notes
			AddBitParameter $command "@processed" $donationRow.Processed
			AddStringParameter $command "@batch_name" $donationRow.Batch_Name
			AddStringParameter $command "@item_number" $donationRow.Item_Number
			AddStringParameter $command "@donation_notes" $donationRow.Donation_Notes
			AddStringParameter $command "@processor_id" $donationRow.Processor_ID
			AddStringParameter $command "@transaction_code" $donationRow.Transaction_Code
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@donation_id" "Int32"
			
			#Execute and report results for donation creation
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$donation_created = LogResult $command "@donation_id" "Donation created"
			
			if(!$donation_created){
				throw
			}
			
			$donation_id = $command.Parameters["@donation_id"].Value
			
			
			#Create command to create donation distribution
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_Distribution"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddIntParameter $command "@donation_id" $donation_id
			AddIntParameter $command "@distribution_amount" $donationRow.R_Donation_Amount
			AddStringParameter $command "@program_name" $donationRow.R_Program_Name
			AddStringParameter $command "@pledge_donor_email" ""			
			AddStringParameter $command "@soft_credit_donor_email" $donationRow.R_Soft_Credit_Donor_Email			
			AddIntParameter $command "@congregation_id" $donationRow.Congregation_ID
			AddStringParameter $command "@notes" ""			
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@distribution_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$distribution_created = LogResult $command "@distribution_id" "        with Donation Distribution"
			
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
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Donation_With_Two_Distributions"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@donor_email" $donationRow.R_Donor_Email
			AddIntParameter $command "@donation_amount" $donationRow.R_Total_Donation_Amount
			AddDateParameter $command "@donation_date" $donationRow.R_Donation_Date
			AddIntParameter $command "@payment_type_id" $donationRow.R_Payment_Type_ID
			AddIntParameter $command "@donation_status" $donationRow.R_Donation_Status_ID
			AddBitParameter $command "@receipted" $donationRow.R_Receipted
			AddBitParameter $command "@anonymous" $donationRow.Anonymous
			
			AddIntParameter $command "@distribution1_amount" $donationRow.R_Distribution1_Amount
			AddStringParameter $command "@distribution1_program_name" $donationRow.R_Distribution1_Program_Name
			AddIntParameter $command "@distribution2_amount" $donationRow.R_Distribution2_Amount
			AddStringParameter $command "@distribution2_program_name" $donationRow.R_Distribution2_Program_Name
			
			AddDateParameter $command "@status_date" $donationRow.Status_Date
			AddStringParameter $command "@status_notes" $donationRow.Status_Notes
			AddBitParameter $command "@processed" $donationRow.Processed
			AddStringParameter $command "@batch_name" $donationRow.Batch_Name
			AddIntParameter $command "@congregation_id" $donationRow.Congregation_ID
			AddStringParameter $command "@item_number" $donationRow.Item_Number
			AddStringParameter $command "@donation_notes" $donationRow.Donation_Notes
			AddStringParameter $command "@processor_id" $donationRow.Processor_ID
			AddStringParameter $command "@transaction_code" $donationRow.Transaction_Code
			AddStringParameter $command "@pledge_user_email" ""
			AddOutputParameter $command "@error_message" "String" 1500
			AddOutputParameter $command "@donation_id" "Int32"
			AddOutputParameter $command "@distribution1_id" "Int32"
			AddOutputParameter $command "@distribution2_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$donation_created = LogResult $command "@donation_id" "Donation created"
			$distribution_created = LogResult $command "@distribution1_id" "        with Donation Distribution 1"
			$distribution_created = LogResult $command "@distribution2_id" "        and Donation Distribution 2"
			
			if(!$donation_created){
				throw
			}			
		}
	}
}




#Execute all the update functions
try{
	$DBConnection = OpenConnection
	#CreateDonations $DBConnection
	#CreateDonationToPledges $DBConnection
	CreateSoftCreditDonation $DBConnection #TODO working on splitting this up to create donation then distribution, accounting for donor without user account
	#CreateDonationsWithTwoDistributions $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}