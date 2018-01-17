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
 
 foreach($user in $contactDataList)
{
	if(![string]::IsNullOrEmpty($user.User_Email))
	{
        $email = $user.User_Email
		$middle = $user.Middle_Name
		$dob = $user.Date_of_Birth
		$gender = $user.Gender_ID
		$marital_status = $user.Marital_Status_ID
		$household_position = $user.Household_Position_ID
		$mobile_phone = $user.Mobile_Phone
		$company_phone = $user.Company_Phone
		$prefix = $user.Prefix_ID
		$output = & $SQLcmd @SQLCommonParams -Q """EXEC [MinistryPlatform].[dbo].[cr_QA_Update_Contact] '$email', '$middle', $dob, $gender, 
			$marital_status, $household_position, '$mobile_phone', '$company_phone', $prefix"""
		
		if($LASTEXITCODE -ne 0){
				write-host "User: "$email
				write-host "Error: "$output
				$exitCode = $LASTEXITCODE
			}
	}
}
exit $exitCode