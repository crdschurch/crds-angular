param (
    [string]$eventTypeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateEventTypes.csv"),
    [string]$eventDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateEvents.csv"),
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

#Deletes all event types in the list
function DeleteEventTypes($DBConnection){
	$eventTypeDataList = import-csv $eventTypeDataCSV
	
	foreach($eventtype in $eventTypeDataList)
	{
		if(![string]::IsNullOrEmpty($eventtype.R_Event_Type))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Event_Type_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@event_type_name" $eventtype.R_Event_Type
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Event Type" $eventtype.R_Event_Type
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to event type "$eventtype.R_Event_Type
				write-host "Error: " $Error
			}
		}
	}
}

#Deletes all events in the list
function DeleteEvents($DBConnection){
	$eventDataList = import-csv $eventDataCSV
	
	foreach($event in $eventDataList)
	{
		if(![string]::IsNullOrEmpty($event.R_Event_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Event_By_Name_And_Date"
			
			#Add variables for stored proc
			AddStringParameter $command "@event_name" $event.R_Event_Name
			AddDateParameter $command "@start_date" $event.R_Start_Date
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Event" $event.R_Event_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to event "$event.R_Event_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeleteEventTypes $DBConnection
	DeleteEvents $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}