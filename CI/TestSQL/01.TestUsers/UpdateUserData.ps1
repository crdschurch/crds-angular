param (
    [string]$contactDataCSV = "UpdateContact.csv",
    [string]$donorDataCSV = "UpdateDonor.csv",
    [string]$householdDataCSV = "UpdateHousehold.csv",
    [string]$householdAddressDataCSV = "UpdateHouseholdAddress.csv",
    [string]$contactsInHouseholdDataCSV = "UpdateContactsInHousehold.csv",
    [string]$contactRelationshipsDataCSV = "UpdateContactRelationship.csv",
    [string]$responseDataCSV = "UpdateResponse.csv",
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )
 
 #Helpers to reformat/convert from csv input 
 function CatchNullString([string]$s){
	$s = $s.replace('null', '').replace('Null', '')
	if ([string]::IsNullOrEmpty($s)){
		return [DBNull]::Value
	} else {
		return $s
	}
}

function StringToInt([string]$s){	
	$s = CatchNullString($s)
	if ([string]::IsNullOrEmpty($s)){
		return [DBNull]::Value
	} else {
		return [int]$s
	}
}

function StringToDate([string]$s){
	$s = CatchNullString($s)
	if ([string]::IsNullOrEmpty($s)){
		return [DBNull]::Value
	} else {
		return [datetime]$s
	}
}

#Helper functions for DB and queries
function OpenConnection(){
	$DBConnection = new-object System.Data.SqlClient.SqlConnection 
	$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
	return $DBConnection
}

function CreateCommand($DBConnection){
	$command = New-Object System.Data.SqlClient.SqlCommand
	$command.CommandType = [System.Data.CommandType]'StoredProcedure'
	$command.Connection = $DBConnection
	return $command
}

function ExecuteCommand($command){
	$adapter = new-object System.Data.SqlClient.SqlDataAdapter
	$adapter.SelectCommand = $command
	
	$dataset = new-object System.Data.Dataset
	try { $adapter.Fill($dataset) }
	catch {
		#Catches issues when running query
		write-host "Error: " $Error
		return 1
	}
	return 0
}


#Update all contacts in list
function UpdateContact($DBConnection){
	$contactDataList = import-csv $contactDataCSV
	$errorCount = 0
	
	foreach($userRow in $contactDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.User_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Contact" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.User_Email
			$middle = CatchNullString($userRow.Middle_Name)
			$dob = StringToDate($userRow.Date_of_Birth)
			$gender = StringToInt($userRow.Gender_ID)
			$marital_status = StringToInt($userRow.Marital_Status_ID)
			$household_position = StringToInt($userRow.Household_Position_ID)
			$mobile_phone = CatchNullString($userRow.Mobile_Phone)
			$company_phone = CatchNullString($userRow.Company_Phone)
			$prefix = StringToInt($userRow.Prefix_ID)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@middle_name", $middle) | Out-Null
			$command.Parameters.AddWithValue("@birthdate", $dob) | Out-Null
			$command.Parameters.AddWithValue("@gender_id", $gender) | Out-Null
			$command.Parameters.AddWithValue("@marital_status_id", $marital_status) | Out-Null
			$command.Parameters.AddWithValue("@prefix_id", $prefix) | Out-Null
			$command.Parameters.AddWithValue("@household_position_id", $household_position) | Out-Null
			$command.Parameters.AddWithValue("@mobile_phone_number", $mobile_phone) | Out-Null
			$command.Parameters.AddWithValue("@company_phone_number", $company_phone) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error updating contact info for user "$email
				$errorCount += $result
			}			
		}
	}
	return $errorCount
}

#Update all donors in list
function UpdateDonor($DBConnection){
	$donorDataList = import-csv $donorDataCSV
	$errorCount = 0
	
	foreach($userRow in $donorDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.User_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Donor_on_Contact" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.User_Email
			$setup_date = StringToDate($userRow.Setup_Date)
			$statement_type = StringToInt($userRow.Statement_Type_ID)
			
			#Pick processor ID by environment
			if ($DBServer -match 'demo') {
				$demo_pid = CatchNullString($userRow.DEMO_Stripe_Processor_ID)
				$command.Parameters.AddWithValue("@processor_id", $demo_pid) | Out-Null
			} else {
				$int_pid = CatchNullString($userRow.INT_Stripe_Processor_ID)
				$command.Parameters.AddWithValue("@processor_id", $int_pid) | Out-Null
			}
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@setup_date", $setup_date) | Out-Null
			$command.Parameters.AddWithValue("@statement_type_id", $statement_type) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error updating donor account related to user "$email
				$errorCount += $result
			}
		}
	}
	return $errorCount
}

#Update all households in list
function UpdateHousehold($DBConnection){
	$householdDataList = import-csv $householdDataCSV
	$errorCount = 0

	foreach($userRow in $householdDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.User_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Household" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.User_Email
			$home_phone = CatchNullString($userRow.Home_Phone)
			$congregation = StringToInt($userRow.Congregation_ID)
						
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@home_phone_number", $home_phone) | Out-Null
			$command.Parameters.AddWithValue("@congregation_id", $congregation) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error updating household for to user "$email
				$errorCount += $result
			}
		}
	}
	return $errorCount
}

#Update all household addresses in list
function UpdateHouseholdAddress($DBConnection){
	$addressDataList = import-csv $householdAddressDataCSV
	$errorCount = 0

	foreach($userRow in $addressDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.User_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Household_Address" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.User_Email
			$address_1 = CatchNullString($userRow.Address_Line_1)
			$address_2 = CatchNullString($userRow.Address_Line_2)
			$city = CatchNullString($userRow.City)
			$state = CatchNullString($userRow.State)
			$zip = StringToInt($userRow.Zip_Code)
			$country = CatchNullString($userRow.Country)
			$country_code = CatchNullString($userRow.Country_Code)
			$county = CatchNullString($userRow.County)
			$latitude = CatchNullString($userRow.Latitude)
			$longitude = CatchNullString($userRow.Longitude)			
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@line_1", $address_1) | Out-Null
			$command.Parameters.AddWithValue("@line_2", $address_2) | Out-Null
			$command.Parameters.AddWithValue("@city", $city) | Out-Null
			$command.Parameters.AddWithValue("@state", $state) | Out-Null
			$command.Parameters.AddWithValue("@zip", $zip) | Out-Null
			$command.Parameters.AddWithValue("@country", $country) | Out-Null
			$command.Parameters.AddWithValue("@country_code", $country_code) | Out-Null
			$command.Parameters.AddWithValue("@county", $county) | Out-Null
			$command.Parameters.AddWithValue("@latitude", $latitude) | Out-Null
			$command.Parameters.AddWithValue("@longitude", $longitude) | Out-Null
			
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error updating household address of user "$email
				$errorCount += $result
			}
		}
	}
	return $errorCount
}

#Add contacts to households
function AddContactsToHouseholds($DBConnection){
	$contactHouseholdsDataList = import-csv $contactsInHouseholdDataCSV
	$errorCount = 0

	foreach($userRow in $contactHouseholdsDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.Member_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Add_Contact_to_Household" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.Member_Email
			$new_member_email = $userRow.New_Member_Email
						
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@household_contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@new_contact_email", $new_member_email) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error adding $new_member_email to $email household"
				$errorCount += $result
			}
		}
	}
	return $errorCount
}

#Add contact relationships
function AddContactRelationships($DBConnection){
	$contactRelationshipsDataList = import-csv $contactRelationshipsDataCSV
	$errorCount = 0

	foreach($userRow in $contactRelationshipsDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.User_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Add_Contact_Relationship" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.User_Email
			$related_user_email = $userRow.Related_User_Email
			$relationship = StringToInt($userRow.Relationship_ID)
			$start_date = StringToDate($userRow.Start_Date)
								
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@related_contact_email", $related_user_email) | Out-Null
			$command.Parameters.AddWithValue("@relationship_id", $relationship) | Out-Null
			$command.Parameters.AddWithValue("@start_date", $start_date) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error adding contact relationships for user "$email
			}
		}
	}
	return $errorCount
}

#Add contacts to households
function AddResponses($DBConnection){
	$responsesDataList = import-csv $responseDataCSV
	$errorCount = 0

	foreach($userRow in $responsesDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.User_Email))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Add_Response_to_Opportunity" #Set name of stored procedure
			
			#Get data in correct format
			$email = $userRow.User_Email
			$opportunity = StringToInt($userRow.Opportunity_ID)
			$response_date = StringToDate($userRow.Response_Date)
			$comments = CatchNullString($userRow.Comments)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@opportunity_id", $opportunity) | Out-Null
			$command.Parameters.AddWithValue("@response_date", $response_date) | Out-Null
			$command.Parameters.AddWithValue("@comments", $comments) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error adding a response to user "$email
				$errorCount += $result
			}
		}
	}
	return $errorCount
}

#Execute all the update functions
$DBConnection = OpenConnection
$errorCount = 0
$errorCount += UpdateContact($DBConnection)
$errorCount += UpdateDonor($DBConnection)
$errorCount += UpdateHousehold($DBConnection)
$errorCount += UpdateHouseholdAddress($DBConnection)
$errorCount += AddContactsToHouseholds($DBConnection)
$errorCount += AddContactRelationships($DBConnection)
$errorCount += AddResponses($DBConnection)
exit $errorCount