param (
    [string]$createGroupDataCSV = "CreateGroup.csv",
	[string]$updateGroupDataCSV = "UpdateGroup.csv",
	[string]$addChildGroupDataCSV = "AddChildGroup.csv",
	[string]$updateGroupAddressDataCSV = "UpdateGroupAddressFromHost.csv",
	[string]$updateOpportunityDataCSV = "UpdateOpportunities.csv",
	[string]$createProgramDataCSV = "CreateProgram.csv",
	[string]$createEventDataCSV = "CreateEvent.csv",
    [string]$DBServer = "mp-demo-db.centralus.cloudapp.azure.com",
    [string]$DBUser = 'MigrateUser', #$(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
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

function StringToBit([string]$s){	
	$s = StringToInt($s)
	if ($s -lt 1){
		return 0
	} else {
		return 1
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
	try { return $adapter.Fill($dataset) }
	catch {
		#Catches issues when running query
		write-host "Error: " $Error
		return 1
	}
}


#Create Groups in list
function CreateGroup($DBConnection){
	$dataList = import-csv $createGroupDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Create_Group" #Set name of stored procedure
			
			#Get data in correct format
			$name = $row.Group_Name
			$type = StringToInt($row.Group_Type_ID)
			$ministry = StringToInt($row.Ministry_ID)
			$congregation = StringToInt($row.Congregation_ID)
			$contact_email = CatchNullString($row.Primary_Contact)
			$start_date = StringToDate($row.Start_Date)
			$size = StringToInt($row.Target_Size)
			$description = CatchNullString($row.Description)
			
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
	$dataList = import-csv $updateGroupDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Group" #Set name of stored procedure
			
			#Get data in correct format
			$name = $row.Group_Name
			$waiting_list = StringToBit($row. Enable_Waiting_List)			
			$childcare = StringToBit($row.Child_Care_Available )
			$is_public = CatchNullString($row.__IsPublic)
			$is_blog_enabled = CatchNullString($row.__ISBlogEnabled)
			$is_web_enabled = CatchNullString($row.__ISWebEnabled)
			$message_id = StringToInt($row.Deadline_Passed_Message_ID)
			$meeting_time = CatchNullString($row.Meeting_Time) #TODO verify time format?
			
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
	$dataList = import-csv $addChildGroupDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Parent_Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Add_Child_Group" #Set name of stored procedure
			
			#Get data in correct format
			$parent_group = $row.Parent_Group_Name
			$child_group = CatchNullString($row.Child_Group_Name)
			
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
	$dataList = import-csv $updateGroupAddressDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Group_Address_From_Host" #Set name of stored procedure
			
			#Get data in correct format
			$group_name = $row.Group_Name
			$contact_email = CatchNullString($row.Host_Email)
			
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

#Updates opportunities in list
function UpdateOpportunities($DBConnection){
	$dataList = import-csv $updateOpportunityDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Group_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Update_Opportunity" #Set name of stored procedure
			
			#Get data in correct format
			$name = $row.Group_Name
			$opportunity_name = CatchNullString($row.Opportunity_Title)
			$contact_email = CatchNullString($row.Contact_Email)
			$group_role_id = StringToInt($row.Group_Role_ID)
			$program_id = StringToInt($row.Program_ID)
			$description = CatchNullString($row.Description)
			$publish_date = StringToDate($row.Publish_Date)
			$min_participants = StringToInt($row.Minimum_Needed)
			$max_participants = StringToInt($row.Maximum_Needed)			
			$shift_start = CatchNullString($row.Shift_Sart) #TODO verify time format?
			$shift_end = CatchNullString($row.Shift_End) #TODO verify time format?
			$event_type = CatchNullString($row.Event_Type)
			$signup_deadline = StringToInt($row.Sign_Up_Deadline_ID)			
			$room_name = CatchNullString($row.Room)
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@group_name", $name) | Out-Null
			$command.Parameters.AddWithValue("@opportunity_name", $opportunity_name) | Out-Null
			$command.Parameters.AddWithValue("@contact_email", $contact_email) | Out-Null
			$command.Parameters.AddWithValue("@role_id", $group_role_id) | Out-Null
			$command.Parameters.AddWithValue("@program_id", $program_id) | Out-Null
			$command.Parameters.AddWithValue("@description", $description) | Out-Null
			$command.Parameters.AddWithValue("@publish_date", $publish_date) | Out-Null
			$command.Parameters.AddWithValue("@minimum_participants", $min_participants) | Out-Null
			$command.Parameters.AddWithValue("@maximum_participants", $max_participants) | Out-Null
			$command.Parameters.AddWithValue("@shift_start", $shift_start) | Out-Null
			$command.Parameters.AddWithValue("@shift_end", $shift_end) | Out-Null
			$command.Parameters.AddWithValue("@event_type", $event_type) | Out-Null
			$command.Parameters.AddWithValue("@signup_deadline", $signup_deadline) | Out-Null
			$command.Parameters.AddWithValue("@room_name", $room_name) | Out-Null
			
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error updating opportunity $opportunity_name in group $name"
				$error_count += 1
			}			
		}
	}
	return $errorCount
}

#Creates programs in list
function CreatePrograms($DBConnection){
	$dataList = import-csv $createProgramDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Program_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Create_Program" #Set name of stored procedure
			
			#Get data in correct format
			$name = $row.Program_Name
			$congregation_id = StringToInt($row.Congregation_ID)
			$ministry_id = StringToInt($row.Ministry_ID)
			$start_date = StringToDate($row.Start_Date)
			$end_date = StringToDate($row.End_Date)
			$program_type_id = StringToInt($row.Program_Type_ID)
			$contact_email = CatchNullString($row.Contact_Email)
			$available_online = StringToBit($row.Available_Online)
			$communication_id = StringToInt($row.Communication_ID)
			$allow_recurring_giving = StringToBit($row.Allow_Recurring_Giving)
						
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@program_name", $name) | Out-Null
			$command.Parameters.AddWithValue("@contact_email", $contact_email) | Out-Null
			$command.Parameters.AddWithValue("@start_date", $start_date) | Out-Null
			$command.Parameters.AddWithValue("@end_date", $end_date) | Out-Null
			$command.Parameters.AddWithValue("@congregation_id", $congregation_id) | Out-Null
			$command.Parameters.AddWithValue("@ministry_id", $ministry_id) | Out-Null
			$command.Parameters.AddWithValue("@program_type_id", $program_type_id) | Out-Null
			$command.Parameters.AddWithValue("@available_online", $available_online) | Out-Null
			$command.Parameters.AddWithValue("@communication_id", $communication_id) | Out-Null
			$command.Parameters.AddWithValue("@allow_recurring_giving", $allow_recurring_giving) | Out-Null
			
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error creating program "$name
				$error_count += 1
			}			
		}
	}
	return $errorCount
}

#Creates events in list
#Run this after Groups and Programs are created since Events can be dependent on this data
function CreateEvents($DBConnection){
	$dataList = import-csv $createEventDataCSV
	$errorCount = 0

	foreach($row in $dataList)
	{
		if(![string]::IsNullOrEmpty($row.Event_Name))
		{
			#Create command to be executed
			$command = CreateCommand($DBConnection)
			$command.CommandText = "cr_QA_Create_Event" #Set name of stored procedure
			
			#Get data in correct format
			$name = $row.Event_Name
			$event_type = CatchNullString($row.Event_Type)
			$program_name = CatchNullString($row.Program_Name)
			$contact_email = CatchNullString($row.Contact_Email)
			$start_date = StringToDate($row.Start_Date)
			$end_date = StringToDate($row.End_Date)			
			$congregation_id = StringToInt($row.Congregation_ID)
			$location_id = StringToInt($row.Location_ID)			
			$group_name = CatchNullString($row.Group_Name)
			
			Write-host "$name $event_type $program_name $contact_email $start_date $end_date $congregation_id $location_id $group_name"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			$command.Parameters.AddWithValue("@event_name", $name) | Out-Null
			$command.Parameters.AddWithValue("@event_type", $event_type) | Out-Null
			$command.Parameters.AddWithValue("@program_name", $program_name) | Out-Null
			$command.Parameters.AddWithValue("@contact_email", $contact_email) | Out-Null
			$command.Parameters.AddWithValue("@start_date", $start_date) | Out-Null
			$command.Parameters.AddWithValue("@end_date", $end_date) | Out-Null
			$command.Parameters.AddWithValue("@congregation_id", $congregation_id) | Out-Null
			$command.Parameters.AddWithValue("@location_id", $location_id) | Out-Null
			$command.Parameters.AddWithValue("@group_name", $group_name) | Out-Null
			
			#Execute query
			$result = ExecuteCommand($command)
			if($result -ne 0){
				write-host "There was an error creating event "$name
				$error_count += 1
			}			
		}
	}
	return $errorCount
}

#Execute all the update functions
$DBConnection = OpenConnection
$errorCount = 0
#$error_count += CreateGroup($DBConnection)
#$error_count += UpdateGroup($DBConnection)
#$error_count += AddChildGroup($DBConnection)
#$error_count += UpdateGroupFromHost($DBConnection)
#$error_count += UpdateOpportunities($DBConnection)
#$error_count += CreatePrograms($DBConnection)
$error_count += CreateEvents($DBConnection)
exit $errorCount