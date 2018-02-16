param (
    [string]$userListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateUserList.csv"),
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

. ((Split-Path $MyInvocation.MyCommand.Definition)+"\..\00.ReloadControllers\DBCommand.ps1") #should avoid dot-source errors

function OpenConnection{
	$DBConnection = new-object System.Data.SqlClient.SqlConnection 
	$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
	$DBConnection.Open();
	return $DBConnection
}

#Deletes all contacts and their user account in the list
function DeleteContacts($DBConnection){
	$userList = import-csv $userListCSV
	
	foreach($user in $userList)
	{
		if(![string]::IsNullOrEmpty($user.email))
		{s
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Contact_Related_Data"
			
			#Add variables for stored proc
			AddStringParameter $command "@contact_email" $user.email
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing User" $user.first $user.last "with email" $user.email;
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to user "$user.email
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeleteContacts $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}