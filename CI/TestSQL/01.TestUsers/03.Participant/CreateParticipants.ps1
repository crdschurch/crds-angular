param (
    [string]$particpantDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateParticipants.csv"),
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

#Create all participants in list
function CreateParticipants($DBConnection){
	$participantDataList = import-csv $particpantDataCSV
	
	foreach($userRow in $participantDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Participant_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Participant"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@participant_email" $userRow.R_Participant_Email
			AddDateParameter $command "@start_date" $userRow.R_Start_Date
			AddBitParameter $command "@show_on_map" $userRow.R_Show_On_Map
			AddIntParameter $command "@host_status" $userRow.R_Host_Status_ID
			AddIntParameter $command "@group_leader_status" $userRow.R_Group_Leader_Status_ID
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@participant_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$participant_created = LogResult $command "@participant_id" "Participant created"
			
			if(!$participant_created){
				throw
			}			
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateParticipants $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}