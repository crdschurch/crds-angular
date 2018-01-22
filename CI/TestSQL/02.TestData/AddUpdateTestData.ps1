param (
    [string]$createGroupDataCSV = "CreateGroup.csv",
    [string]$DBServer = "mp-demo-db.centralus.cloudapp.azure.com",
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
	$exitCode = 0
	$adapter = new-object System.Data.SqlClient.SqlDataAdapter
	$adapter.SelectCommand = $command
	
	$dataset = new-object System.Data.Dataset
	try { $adapter.Fill($dataset) }
	catch {
		#Catches issues when running query
		write-host "Error: " $Error
		$exitCode = 1
	}
	return $exitCode
}


#Update all contacts in list
function CreateGroup($DBConnection){
	$groupDataList = import-csv $createGroupDataCSV
	$exitCode = 0

	foreach($groupRow in $groupDataList)
	{
		if(![string]::IsNullOrEmpty($groupRow.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Create_Group" #Set name of stored procedure
			
			#Get data in correct format
			$name = $groupRow.Group_Name
			$type = StringToInt($groupRow.Group_Type_ID)
			$ministry = StringToInt($groupRow.Ministry_ID)
			$congregation = StringToInt($groupRow.Congregation_ID)
			$contact_id = StringToInt($groupRow.Primary_Contact)
			$start_date = StringToDate($groupRow.Start_Date)
			$size = StringToInt($groupRow.Target_Size)
			$description = CatchNullString($groupRow.Description)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@group_name", $name) | Out-Null
			$command.Parameters.AddWithValue("@group_type_id", $type) | Out-Null
			$command.Parameters.AddWithValue("@ministry_id", $ministry) | Out-Null
			$command.Parameters.AddWithValue("@congregation_id", $congregation) | Out-Null
			$command.Parameters.AddWithValue("@primary_contact_id", $contact_id) | Out-Null
			$command.Parameters.AddWithValue("@start_date", $start_date) | Out-Null
			$command.Parameters.AddWithValue("@target_size", $size) | Out-Null
			$command.Parameters.AddWithValue("@description", $description) | Out-Null
				
			#Execute query
			$exitCode = ExecuteCommand($command)
			if($exitCode -ne 0){
				write-host "There was an error creating group "$name
			}			
		}
	}
}


#Execute all the update functions
$DBConnection = OpenConnection
CreateGroup($DBConnection)