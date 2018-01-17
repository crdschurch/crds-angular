param (
    [string]$contactDataCSV = "UpdateContact.csv",
	[string]$donorDataCSV = "UpdateDonor.csv",
    [string]$DBServer = "mp-demo-db.centralus.cloudapp.azure.com",
    [string]$DBUser = 'MigrateUser',  #$(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = 'Aw@!ted2014' #$(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
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
		$s = $s.replace('/', '-') #parseexact is really picky
		return [datetime]::parseexact($s, 'd-M-yyyy', $null)
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
	$adapter.Fill($dataset)
}


#Update all contacts in list
function UpdateContact($DBConnection){
	$contactDataList = import-csv $contactDataCSV
	$exitCode = 0

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
			ExecuteCommand($command)
					
			if($LASTEXITCODE -ne 0){
					write-host "There was an error updating user: "$email
					$exitCode = $LASTEXITCODE
			}
		}
	}
	exit $exitCode
}

#Update all donors in list
function UpdateDonor($DBConnection){
	$donorDataList = import-csv $donorDataCSV
	$exitCode = 0

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
				$stripe_pid = StringToInt($userRow.DEMO_Stripe_Processor_ID)
			} else {
				$stripe_pid = StringToInt($userRow.INT_Stripe_Processor_ID)
			}
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@contact_email", $email) | Out-Null
			$command.Parameters.AddWithValue("@setup_date", $setup_date) | Out-Null
			$command.Parameters.AddWithValue("@statement_type_id", $statement_type) | Out-Null
			$command.Parameters.AddWithValue("@processor_id", $stripe_pid) | Out-Null
				
			#Execute query
			ExecuteCommand($command)
					
			if($LASTEXITCODE -ne 0){
					write-host "There was an error updating donor on : "$email
					$exitCode = $LASTEXITCODE
			}
		}
	}
	exit $exitCode
}



#Execute all the update functions
$DBConnection = OpenConnection
#UpdateContact($DBConnection)
UpdateDonor($DBConnection)