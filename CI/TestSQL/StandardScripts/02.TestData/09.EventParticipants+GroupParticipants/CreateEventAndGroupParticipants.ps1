param (
    [string]$eventParticipantDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateEventParticipants.csv"),
    [string]$groupParticipantDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateGroupParticipants.csv"),
    [string]$DBServer = "mp-int-db.centralus.cloudapp.azure.com",
    [string]$DBUser = $(Get-ChildItem Env:MP_SOURCE_DB_USER).Value, # Default to environment variable
    [string]$DBPassword = $(Get-ChildItem Env:MP_SOURCE_DB_PASSWORD).Value # Default to environment variable
 )

. ((Split-Path $MyInvocation.MyCommand.Definition)+"\..\..\00.PowershellScripts\DBCommand.ps1") #should avoid dot-source errors

function OpenConnection{
	$DBConnection = new-object System.Data.SqlClient.SqlConnection 
	$DBConnection.ConnectionString = "Server=$DBServer;Database=MinistryPlatform;User Id=$DBUser;Password=$DBPassword"
	$DBConnection.Open();
	return $DBConnection
}

#Create all event participants in list
function CreateEventParticipants($DBConnection){
	$eventParticipantDataList = import-csv $eventParticipantDataCSV
	$error_count = 0
	foreach($participantRow in $eventParticipantDataList)
	{
		if(![string]::IsNullOrEmpty($participantRow.R_Event_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Event_Participant"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@event_name" $participantRow.R_Event_Name			
			AddStringParameter $command "@participant_email" $participantRow.R_Participant_Email			
			AddDateParameter $command "@event_start_date" $participantRow.R_Event_Start_Date
			AddIntParameter $command "@participant_status_id" $participantRow.R_Participation_Status_ID
			AddDateParameter $command "@time_in" $participantRow.Time_In
			AddDateParameter $command "@time_confirmed" $participantRow.Time_Confirmed
			AddDateParameter $command "@setup_date" $participantRow.Setup_Date
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@event_participant_id" "Int32"
				
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$event_participant_created = LogResult $command "@event_participant_id" "Event Participant created"
			
			if(!$event_participant_created){
				$error_count += 1
			}
		}
	}
	return $error_count
}

#Create all group participants in list
function CreateGroupParticipants($DBConnection){
	$groupParticipantDataList = import-csv $groupParticipantDataCSV
	$error_count = 0
	foreach($participantRow in $groupParticipantDataList)
	{
		if(![string]::IsNullOrEmpty($participantRow.R_Group_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Group_Participant"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@group_name" $participantRow.R_Group_Name			
			AddStringParameter $command "@participant_email" $participantRow.R_Participant_Email
			AddIntParameter $command "@group_role_id" $participantRow.R_Group_Role_ID
			AddDateParameter $command "@start_date" $participantRow.R_Start_Date
            AddIntParameter $command "@preferred_serve_event_type_id" $participantRow.Preferred_Serving_Event_Type_ID
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@group_participant_id" "Int32"
				
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$group_participant_created = LogResult $command "@group_participant_id" "Group Participant created"
			
			if(!$group_participant_created){
				$error_count += 1
			}
		}
	}
	return $error_count
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	$errors = 0
	$errors += CreateEventParticipants $DBConnection
	$errors += CreateGroupParticipants $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}