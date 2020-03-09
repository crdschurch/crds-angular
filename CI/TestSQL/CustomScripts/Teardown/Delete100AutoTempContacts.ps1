param (
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )
 
$DBConnection = new-object System.Data.SqlClient.SqlConnection 
$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
$DBConnection.Open();
 
try {
	$command = New-Object System.Data.SqlClient.SqlCommand
	$command.CommandType = [System.Data.CommandType]'StoredProcedure'
	$command.CommandText = "cr_QA_Delete_Temp_Auto_Users"
	$command.Connection = $DBConnection
	$command.CommandTimeout = 900 #Set command timeout to 15 minutes. Deletion takes 4-5sec per user.
	$command.Parameters.AddWithValue("@count_to_delete", 100) | Out-Null
	
	#Execute command
	$adapter = new-object System.Data.SqlClient.SqlDataAdapter
	$adapter.SelectCommand = $command        
	$dataset = new-object System.Data.Dataset
	$results = $adapter.Fill($dataset)	
}
catch {
	Write-Output "Something went wrong. Running cr_QA_Delete_Temp_Auto_Users directly in the DB may give more valuable debugging info."
    Write-Output "File: $_"
    Write-Output "Error: $_.Exception.Message"
    $exitCode = 8
}
finally {
    $DBConnection.close()
	exit $exitCode
}
