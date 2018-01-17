param (
    [string]$contactDataCSV = "..\TestSQL\01.TestUsers\contactData.csv",
    [string]$DBServer = "mp-int-db.cloudapp.net",
    [string]$SQLcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\110\Tools\Binn\sqlcmd.exe",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )
 
 $contactDataList = import-csv $contactDataCSV
 $exitCode = 0
 $SQLCommonParams = @("-U", $DBUser, "-P", $DBPassword, "-S", $DBServer, "-b")
 
 $connection = new-object System.Data.SqlClient.SqlConnection
 $connection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
 
 #this may need to be done in the loop
 $command = $connection.CreateCommand()
 $command.CommandText = "EXEC [MinistryPlatform].[dbo].[cr_QA_Update_Contact] @email, @middle_name, @dob, @gender, @marital_status, @household_position, @mobile_phone, @company_phone, @prefix"
 
 foreach($user in $contactDataList)
{
	if(![string]::IsNullOrEmpty($user.User_Email))
	{
        $email = $user.User_Email
		
		$command.Parameters.AddWithValue("@email", $email) | Out-Null
		$command.Parameters.AddWithValue("@middle_name", $user.Middle_Name) | Out-Null
		$command.Parameters.AddWithValue("@dob", $user.Date_of_Birth) | Out-Null
		$command.Parameters.AddWithValue("@gender", $user.Gender_ID) | Out-Null
		$command.Parameters.AddWithValue("@marital_status", $user.Marital_Status_ID) | Out-Null
		$command.Parameters.AddWithValue("@household_position", $user.Household_Position_ID) | Out-Null
		$command.Parameters.AddWithValue("@mobile_phone", $user.Mobile_Phone) | Out-Null
		$command.Parameters.AddWithValue("@company_phone", $user.Company_Phone) | Out-Null
		$command.Parameters.AddWithValue("@prefix", $user.Prefix_ID) | Out-Null
		
		$adapter = new-object System.Data.SqlClient.SqlDataAdapter $command
		$dataset = new-object System.Data.Dataset
		
		Write-Host $adapter.Fill($dataset) ' contact data has been added.'#debug
		
		#$output = & $SQLcmd @SQLCommonParams -Q """EXEC [MinistryPlatform].[dbo].[cr_QA_Update_Contact] '$email', '$middle', $dob, $gender, $marital_status, $household_position, '$mobile_phone', '$company_phone', $prefix"""
		
		
		if($LASTEXITCODE -ne 0){
				write-host "There was an error updating user: "$email
				#write-host "Error: "$output
				$exitCode = $LASTEXITCODE
			}
	}
}
exit $exitCode