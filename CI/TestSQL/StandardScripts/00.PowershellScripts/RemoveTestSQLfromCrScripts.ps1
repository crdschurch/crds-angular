# Given a string of comma separated directories, parses files and removes meta data from the cr_Build_Scripts	
# table so that test SQL can be rerun on a database. 

param (
    [string]$path = $(throw "-path is required."), #Comma separated directories, no spaces
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

#get sql files and format for query
$path -split ',' | foreach { $child = (Get-ChildItem -path ($_).toString() -recurse -filter *.sql);
 if (![string]::IsNullOrEmpty($child)){ $sqlfiles += "'"+$child+"'"; } }
$sqlfiles = ([string]$sqlfiles).replace(" ", "', '").replace("''", "', '");

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