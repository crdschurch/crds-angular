param (
	[string]$eventTypeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateEventTypes.csv"),
    [string]$eventDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateEvents.csv"),
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

#Create all event types in list
function CreateEventTypes($DBConnection){
	$eventTypeDataList = import-csv $eventTypeDataCSV
	
	foreach($eventTypeRow in $eventTypeDataList)
	{
		if(![string]::IsNullOrEmpty($eventTypeRow.R_Event_Type))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Event_Type"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@event_type_name" $eventTypeRow.R_Event_Type			
			AddBitParameter $command "@allow_multiday_event" $eventTypeRow.R_Allow_Multiday_Event			
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@event_type_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$event_type_created = LogResult $command "@event_type_id" "Event Type created"
			
			if(!$event_type_created){
				throw
			}			
		}
	}
}

#Create all event types in list
function CreateEvents($DBConnection){
	$eventDataList = import-csv $eventDataCSV
	
	foreach($eventRow in $eventDataList)
	{
		if(![string]::IsNullOrEmpty($eventRow.R_Event_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Event"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@event_name" $eventRow.R_Event_Name
			AddDateParameter $command "@start_date" $eventRow.R_Start_Date
			AddDateParameter $command "@end_date" $eventRow.R_End_Date
			AddStringParameter $command "@event_type_name" $eventRow.R_Event_Type
			AddIntParameter $command "@congregation_id" $eventRow.R_Congregation_ID
			AddStringParameter $command "@primary_contact_email" $eventRow.R_Contact_Email
			AddStringParameter $command "@program_name" $eventRow.R_Program_Name
			AddIntParameter $command "@location_id" $eventRow.Location_ID
			AddStringParameter $command "@group_name" $eventRow.Group_Name
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@event_id" "Int32"
			AddOutputParameter $command "@event_group_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$event_created = LogResult $command "@event_id" "Event created"
			$event_group_created = LogResult $command "@event_group_id" "        and added to Event Group"
			
			#Event group is not required, so don't error out if not created
			if(!$event_created){
				throw
			}
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateEventTypes $DBConnection
	CreateEvents $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}