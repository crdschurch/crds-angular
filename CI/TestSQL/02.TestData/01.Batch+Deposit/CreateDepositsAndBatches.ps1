param (
    [string]$depositDataCSV =((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDeposits.csv"),
    [string]$batchDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateBatches.csv"),
	[string]$depositBatchLinkDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\AddBatchToDeposit.csv"),
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

#Create empty deposit
function CreateDeposits($DBConnection){
	$depositDataList = import-csv $depositDataCSV

	foreach($depositRow in $depositDataList)
	{
		if(![string]::IsNullOrEmpty($depositRow.R_Deposit_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Empty_Deposit"

			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@deposit_name" $depositRow.R_Deposit_Name
			AddDateParameter $command "@deposit_date" $depositRow.R_Deposit_Date			
			AddStringParameter $command "@account_number" $depositRow.R_Account_Number
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@deposit_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$deposit_created = LogResult $command "@deposit_id" "Empty Deposit created"
			
			if(!$deposit_created){
				throw
			}
		}
	}
}

#Create empty batch
function CreateBatches($DBConnection){
	$batchDataList = import-csv $batchDataCSV

	foreach($batchRow in $batchDataList)
	{
		if(![string]::IsNullOrEmpty($batchRow.R_Batch_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Empty_Batch"

			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@batch_name" $batchRow.R_Batch_Name
			AddDateParameter $command "@setup_date" $batchRow.R_Setup_Date
			AddDateParameter $command "@finalize_date" $batchRow.Finalize_Date
			AddStringParameter $command "@user_email" $batchRow.Operator_User_Email			
			AddIntParameter  $command "@congregation_id" $batchRow.Congregation_ID
			AddIntParameter  $command "@deposit_id" "" #We're not using this
			AddStringParameter $command "@default_program_id_list" $batchRow.Default_Program_ID_List
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@batch_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$batch_created = LogResult $command "@batch_id" "Empty Batch created"
			
			if(!$batch_created){
				throw
			}
		}
	}
}

#Add batch to deposit
function AddBatchToDeposit($DBConnection){
	$depositBatchLinkDataList = import-csv $depositBatchLinkDataCSV

	foreach($depositRow in $depositBatchLinkDataList)
	{
		if(![string]::IsNullOrEmpty($depositRow.R_Deposit_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Add_Batch_To_Deposit"

			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@deposit_name" $depositRow.R_Deposit_Name
			AddStringParameter $command "@batch_name" $depositRow.R_Batch_Name
			AddOutputParameter $command "@error_message" "String" 1000
			AddOutputParameter $command "@deposit_id" "Int32"
			AddOutputParameter $command "@batch_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$batch_found = LogResult $command "@batch_id" "Batch"
			$deposit_found = LogResult $command "@deposit_id" "        added to Deposit"
			
			if(!$batch_found -or !$deposit_found){
				throw
			}
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateDeposits $DBConnection
	CreateBatches $DBConnection
	AddBatchToDeposit $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}