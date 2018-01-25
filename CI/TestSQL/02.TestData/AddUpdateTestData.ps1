param (
    [string]$createGroupDataCSV = "CreateGroup.csv",
	[string]$updateGroupDataCSV = "UpdateGroup.csv",
	[string]$addChildGroupDataCSV = "AddChildGroup.csv",
	[string]$updateGroupAddressDataCSV = "UpdateGroupAddressFromHost.csv",
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

function StringToBit([string]$s){	
	$s = CatchNullString($s)
	if ([string]::IsNullOrEmpty($s)){
		return 0
	} else { #guarantee bit returned
		$b = [int]$s
		if ($b -lt 1){
			return 0
		} else {
			return 1
		}
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


#Create Groups in list
function CreateGroup($DBConnection){
	$groupDataList = import-csv $createGroupDataCSV
	$errorCount = 0

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
			$contact_email = CatchNullString($groupRow.Primary_Contact)
			$start_date = StringToDate($groupRow.Start_Date)
			$size = StringToInt($groupRow.Target_Size)
			$description = CatchNullString($groupRow.Description)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@group_name", $name) | Out-Null
			$command.Parameters.AddWithValue("@group_type_id", $type) | Out-Null
			$command.Parameters.AddWithValue("@ministry_id", $ministry) | Out-Null
			$command.Parameters.AddWithValue("@congregation_id", $congregation) | Out-Null
			$command.Parameters.AddWithValue("@primary_contact_email", $contact_email) | Out-Null
			$command.Parameters.AddWithValue("@start_date", $start_date) | Out-Null
			$command.Parameters.AddWithValue("@target_size", $size) | Out-Null
			$command.Parameters.AddWithValue("@description", $description) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error creating group "$name
				$error_count += 1
			}			
		}
	}
	return $errorCount
}

#Updates groups in list
function UpdateGroup($DBConnection){
	$groupDataList = import-csv $updateGroupDataCSV
	$errorCount = 0

	foreach($groupRow in $groupDataList)
	{
		if(![string]::IsNullOrEmpty($groupRow.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Group" #Set name of stored procedure
			
			#Get data in correct format
			$name = $groupRow.Group_Name
			$waiting_list = StringToBit($groupRow. Enable_Waiting_List)			
			$childcare = StringToBit($groupRow.Child_Care_Available )
			$is_public = CatchNullString($groupRow.__IsPublic)
			$is_blog_enabled = CatchNullString($groupRow.__ISBlogEnabled)
			$is_web_enabled = CatchNullString($groupRow.__ISWebEnabled)
			$message_id = StringToInt($groupRow.Deadline_Passed_Message_ID)
			$meeting_time = CatchNullString($groupRow.Meeting_Time) #TODO probs need to convert to time
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@group_name", $name) | Out-Null
			$command.Parameters.AddWithValue("@enable_waiting_list", $waiting_list) | Out-Null
			$command.Parameters.AddWithValue("@child_care_available", $childcare) | Out-Null
			$command.Parameters.AddWithValue("@is_public", $is_public) | Out-Null
			$command.Parameters.AddWithValue("@is_blog_enabled", $is_blog_enabled) | Out-Null
			$command.Parameters.AddWithValue("@is_web_enabled", $is_web_enabled) | Out-Null
			$command.Parameters.AddWithValue("@deadline_passed_message_id", $message_id) | Out-Null
			$command.Parameters.AddWithValue("@meeting_time", $meeting_time) | Out-Null
			
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error updating group "$name
				$error_count += 1
			}			
		}
	}
	return $errorCount
}

#Add child groups
function AddChildGroup($DBConnection){
	$groupDataList = import-csv $addChildGroupDataCSV
	$errorCount = 0

	foreach($groupRow in $groupDataList)
	{
		if(![string]::IsNullOrEmpty($groupRow.Parent_Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Add_Child_Group" #Set name of stored procedure
			
			#Get data in correct format
			$parent_group = $groupRow.Parent_Group_Name
			$child_group = CatchNullString($groupRow.Child_Group_Name)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@parent_group_name", $parent_group) | Out-Null
			$command.Parameters.AddWithValue("@child_group_name", $child_group) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error adding $child_group to $parent_group"
				$error_count += 1
			}			
		}
	}
	return $errorCount
}

#Set Group address to host's address
function UpdateGroupFromHost($DBConnection){
	$groupDataList = import-csv $updateGroupAddressDataCSV
	$errorCount = 0

	foreach($groupRow in $groupDataList)
	{
		if(![string]::IsNullOrEmpty($groupRow.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Group_Address_From_Host" #Set name of stored procedure
			
			#Get data in correct format
			$group_name = $groupRow.Group_Name
			$contact_email = CatchNullString($groupRow.Host_Email)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@group_name", $group_name) | Out-Null
			$command.Parameters.AddWithValue("@contact_email", $contact_email) | Out-Null
				
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error adding $child_group to $parent_group"
				$error_count += 1
			}			
		}
	}
	return $errorCount
}


#Execute all the update functions
$DBConnection = OpenConnection
$errorCount = 0
$error_count += CreateGroup($DBConnection)
$error_count += UpdateGroup($DBConnection)
$error_count += AddChildGroup($DBConnection)
$error_count += UpdateGroupFromHost($DBConnection)
exit $errorCount