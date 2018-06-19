param (
    [string]$userDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateUserList.csv"),
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

. ((Split-Path $MyInvocation.MyCommand.Definition)+"\..\00.PowershellScripts\DBCommand.ps1") #should avoid dot-source errors

function OpenConnection{
	$DBConnection = new-object System.Data.SqlClient.SqlConnection 
	$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
	$DBConnection.Open();
	return $DBConnection
}

#Deletes all contacts and their user account in the list
function DeleteContacts($DBConnection){
	$userList = import-csv $userDataCSV
	$error_count = 0
	foreach($user in $userList)
	{
		if(![string]::IsNullOrEmpty($user.email))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Accounts_With_Email"
			
			#Add variables for stored proc
			AddStringParameter $command "@contact_email" $user.email
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
            
            $retries = 0
            $complete = false
            while(-not $complete){
                try { 
                    write-host "Removing User" $user.first $user.last "with email" $user.email;
                    $results = $adapter.Fill($dataset)
                    $complete = true
                } catch [System.Data.SqlClient.SqlException]{
                    if ($_.Exception.Number -eq 1205 -and $retries -le 2) #deadlock issue
                    {
                        Start-Sleep -s 3
                        $retries += 1
                    } else {
                        write-host "There was an error after $retries attempts deleting data related to user "$user.email
                        write-host "Error: " $Error
                        $error_count += 1
                        $complete = true
                    }
                } catch {
                    write-host "There was an error deleting data related to user "$user.email
                    write-host "Error: " $Error
                    $error_count += 1
                    $complete = true
                }
            }
		}
	}
	return $error_count
}

#Execute
try{
	$DBConnection = OpenConnection
	$errors = 0
	$errors += DeleteContacts $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}