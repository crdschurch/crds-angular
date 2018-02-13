param (
    [string]$householdDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateHouseholds.csv"),
    [string]$householdAddressDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateHouseholdAddresses.csv"),
    [string]$contactsInHouseholdDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\AddHouseholdMember.csv"),
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

#Create all households in list
function CreateHouseholds($DBConnection){
	$householdDataList = import-csv $householdDataCSV

	foreach($userRow in $householdDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Household_Member_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Household"
						
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@household_member_email" $userRow.R_Household_Member_Email
			AddStringParameter $command "@home_phone_number" $userRow.Home_Phone
			AddIntParameter $command "@congregation_id" $userRow.Congregation_ID
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@household_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$household_created = LogResult $command "@household_id" "Household created"
			
			if(!$household_created){
				throw
			}	
		}
	}
}

#Create all household addresses in list
function CreateHouseholdAddresses($DBConnection){
	$addressDataList = import-csv $householdAddressDataCSV

	foreach($userRow in $addressDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Household_Member_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Household_Address"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@household_member_email" $userRow.R_Household_Member_Email			
			AddStringParameter $command "@line_1" $userRow.R_Address_Line_1
			AddStringParameter $command "@line_2" $userRow.Address_Line_2			
			AddStringParameter $command "@city" $userRow.City
			AddStringParameter $command "@state" $userRow.State
			AddStringParameter $command "@zip" $userRow.Zip_Code
			AddStringParameter $command "@country" $userRow.Country
			AddStringParameter $command "@country_code" $userRow.Country_Code
			AddStringParameter $command "@county" $userRow.County
			AddStringParameter $command "@longitude" $userRow.Longitude
			AddStringParameter $command "@latitude" $userRow.Latitude			
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@address_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$address_created = LogResult $command "@address_id" "Address created"
			
			if(!$address_created){
				throw
			}
		}
	}
}

#Add contacts to households
function AddHouseholdMember($DBConnection){
	$contactHouseholdsDataList = import-csv $contactsInHouseholdDataCSV

	foreach($userRow in $contactHouseholdsDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Household_Member_Email))
		{
		#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Add_Household_Member"
						
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@household_member_email" $userRow.R_Household_Member_Email	
			AddStringParameter $command "@new_member_email" $userRow.R_New_Member_Email	
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@household_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$household_created = LogResult $command "@household_id" "$($userRow.R_New_Member_Email) added to household"
			
			if(!$household_created){
				throw
			}
		}
	}
}

#Execute all the Create functions
try{
	$DBConnection = OpenConnection
	CreateHouseholds $DBConnection
	CreateHouseholdAddresses $DBConnection
	AddHouseholdMember $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}