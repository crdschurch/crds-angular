#Updated
param (
    [string]$path = $(throw "-path is required."), #Comma separated paths
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

#get sql files and format for query
$path -split ',' | foreach { $sqlfiles += (Get-ChildItem -path ($_) -recurse -filter *.sql)}
$sqlfiles = "'"+([string]$sqlfiles).replace(" ", "', '")+"'"

#open connection
$DBConnection = new-object System.Data.SqlClient.SqlConnection 
$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
$DBConnection.Open();

try {
	$command = New-Object System.Data.SQLClient.SQLCommand
	$command.Connection = $DBConnection
	$command.CommandText = "DELETE FROM [MinistryPlatform].[dbo].[cr_Scripts] WHERE Name in ($sqlfiles)";

	$command.ExecuteNonQuery() | Out-Null
}
catch {         
	Write-Output "File: $_"
	Write-Output "Error: $_.Exception.Message"
	$exitCode = 8
}
finally {
	$DBConnection.close()
}