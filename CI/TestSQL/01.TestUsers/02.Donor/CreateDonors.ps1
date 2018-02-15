param (
    [string]$donorDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDonors.csv"),
	[string]$guestGiverDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateGuestGivers.csv"),
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

#Create all donors in list
function CreateDonors($DBConnection){
	$donorDataList = import-csv $donorDataCSV
	
	foreach($userRow in $donorDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Donor_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Donor"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@donor_email" $userRow.R_Donor_Email
			AddDateParameter $command "@setup_date" $userRow.R_Setup_Date
			AddIntParameter $command "@statement_type_id" $userRow.R_Statement_Type_ID
			AddIntParameter $command "@statement_frequency_id" $userRow.R_Statement_Frequency_ID
			AddIntParameter $command "@statement_method_id" $userRow.R_Statement_Method_ID
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@donor_id" "Int32"
			
			#Pick processor ID by environment
			if ($DBServer -match 'demo') {
				AddStringParameter $command "@processor_id" $userRow.DEMO_Stripe_Processor_ID
			} else {
				AddStringParameter $command "@processor_id" $userRow.INT_Stripe_Processor_ID
			}
							
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$donor_created = LogResult $command "@donor_id" "Donor created"
			
			if(!$donor_created){
				throw
			}
		}
	}
}

#Create all guest givers in list
function CreateGuestGivers($DBConnection){
	$guestGiverDataList = import-csv $guestGiverDataCSV
	
	foreach($userRow in $guestGiverDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Donor_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Guest_Giver"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@contact_email" $userRow.R_Donor_Email
			AddDateParameter $command "@setup_date" $userRow.R_Setup_Date
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@donor_id" "Int32"
			
			#Pick processor ID by environment
			if ($DBServer -match 'demo') {
				AddStringParameter $command "@processor_id" $userRow.DEMO_Stripe_Processor_ID
			} else {
				AddStringParameter $command "@processor_id" $userRow.INT_Stripe_Processor_ID
			}
							
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$donor_created = LogResult $command "@donor_id" "Guest Giver created"
			
			if(!$donor_created){
				throw
			}
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateDonors $DBConnection
	CreateGuestGivers $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}