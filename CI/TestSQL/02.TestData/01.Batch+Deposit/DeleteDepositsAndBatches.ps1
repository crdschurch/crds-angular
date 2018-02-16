param (
    [string]$batchListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateBatches.csv"),
	[string]$depositListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateDeposits.csv"),
    [string]$DBServer = "mp-demo-db.centralus.cloudapp.azure.com",
    [string]$DBBatch = 'MigrateUser', #$(Get-ChildItem Env:MP_SOURCE_DB_Batch).Value, # Default to environment variable
    [string]$DBPassword = 'Aw@!ted2014' #$(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

. ((Split-Path $MyInvocation.MyCommand.Definition)+"\..\..\00.ReloadControllers\DBCommand.ps1") #should avoid dot-source errors

function OpenConnection{
	$DBConnection = new-object System.Data.SqlClient.SqlConnection 
	$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;Batch Id=$DBBatch;Password=$DBPassword"
	$DBConnection.Open();
	return $DBConnection
}

#Deletes all batches in the list
function DeleteBatches($DBConnection){
	$batchList = import-csv $batchListCSV
	
	foreach($batch in $batchList)
	{
		if(![string]::IsNullOrEmpty($batch.email))
		{s
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Batch"
			
			#Add variables for stored proc
			AddIntParameter $command "@batch_id" ""
			AddStringParameter $command "@batch_name" $batch.R_Batch_Name
			write-host $command.Parameters
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
		if(![string]::IsNullOrEmpty($deposit.email))
		{s
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Deposit"
			
			#Add variables for stored proc
			AddIntParameter $command "@deposit_id" ""
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