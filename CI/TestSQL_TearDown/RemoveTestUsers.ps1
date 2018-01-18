param (
    [string]$userListCSV = "CreateUserList.csv",
    [string]$DBServer = "mp-int-db.cloudapp.net",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

#Create connection
$DBConnection = new-object System.Data.SqlClient.SqlConnection 
$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
 
$userList = import-csv $userListCSV
$exitCode = 0
 
foreach($user in $userList)
{
	if(![string]::IsNullOrEmpty($user.email))
	{
        $email = $user.email
		
		#Create command
		$command = New-Object System.Data.SqlClient.SqlCommand
		$command.CommandType = [System.Data.CommandType]'StoredProcedure'
		$command.Connection = $DBConnection
		$command.CommandText = "cr_QADeleteData" #Set name of stored procedure
		
		#Add variables for stored proc
		$command.Parameters.AddWithValue("@Email_Address", $email) | Out-Null
		
		write-host "Removing User" $user.first $user.last "with email" $email;
		
		#Execute command
		$adapter = new-object System.Data.SqlClient.SqlDataAdapter
		$adapter.SelectCommand = $command		
		$dataset = new-object System.Data.Dataset
		$adapter.Fill($dataset)
		
		if($LASTEXITCODE -ne 0){
				write-host "There was an error deleting data related to user "$email
				write-host $LASTEXITCODE #debug 
				$exitCode = $LASTEXITCODE
			}
	}
}
exit $exitCode