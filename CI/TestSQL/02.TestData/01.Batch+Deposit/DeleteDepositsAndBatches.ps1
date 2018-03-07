param (
    [string]$batchListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateBatches.csv"),
	[string]$depositListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDeposits.csv"),
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

#Deletes all batches in the list
function DeleteBatches($DBConnection){
	$batchList = import-csv $batchListCSV
	
	foreach($batch in $batchList)
	{
		if(![string]::IsNullOrEmpty($batch.R_Batch_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Batch_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@batch_name" $batch.R_Batch_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Batch" $batch.R_Batch_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to batch "$batch.R_Batch_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Deletes all deposits in the list
function DeleteDeposites($DBConnection){
	$depositList = import-csv $depositListCSV
	
	foreach($deposit in $depositList)
	{
		if(![string]::IsNullOrEmpty($deposit.R_Deposit_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Deposit_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@deposit_name" $deposit.R_Deposit_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Deposit" $deposit.R_Deposit_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to deposit "$deposit.R_Deposit_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeleteBatches $DBConnection
	DeleteDeposites $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}