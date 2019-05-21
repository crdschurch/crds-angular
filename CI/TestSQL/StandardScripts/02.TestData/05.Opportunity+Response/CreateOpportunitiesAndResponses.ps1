param (
    [string]$opportunityDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateOpportunities.csv"),
    [string]$responseDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateResponses.csv"),
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

#Create all opportunities in list
function CreateOpportunities($DBConnection){
	$opportunityDataList = import-csv $opportunityDataCSV
	$error_count = 0
	foreach($opportunityRow in $opportunityDataList)
	{
		if(![string]::IsNullOrEmpty($opportunityRow.R_Opportunity_Title))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Opportunity"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@opportunity_name" $opportunityRow.R_Opportunity_Title
			AddStringParameter $command "@contact_email" $opportunityRow.R_Contact_Email
			AddStringParameter $command "@program_name" $opportunityRow.R_Program_Name
			AddIntParameter $command "@role_id" $opportunityRow.R_Group_Role_ID
			AddDateParameter $command "@shift_start" $opportunityRow.Shift_Sart
			AddDateParameter $command "@shift_end" $opportunityRow.Shift_End
			AddIntParameter $command "@minimum_participants" $opportunityRow.Minimum_Needed
			AddIntParameter $command "@maximum_participants" $opportunityRow.Maximum_Needed
			AddStringParameter $command "@room_name" $opportunityRow.Room
			AddStringParameter $command "@group_name" $opportunityRow.Group_Name
			AddStringParameter $command "@event_type" $opportunityRow.Event_Type
			AddStringParameter $command "@description" $opportunityRow.Description
			AddDateParameter $command "@publish_date" $opportunityRow.Publish_Date
			AddIntParameter $command "@signup_deadline" $opportunityRow.Sign_Up_Deadline_ID
			AddStringParameter $command "@opportunity_subtitle" $opportunityRow.Opportunity_Subtitle
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@opportunity_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$opportunity_created = LogResult $command "@opportunity_id" "Opportunity created"
			
			if(!$opportunity_created){
				$error_count += 1
			}			
		}
	}
	return $error_count
}

#Create all responses in list
function CreateResponses($DBConnection){
	$responseDataList = import-csv $responseDataCSV
	$error_count = 0
	foreach($responseRow in $responseDataList)
	{
		if(![string]::IsNullOrEmpty($responseRow.R_User_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Response"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@participant_email" $responseRow.R_User_Email
			AddIntParameter $command "@opportunity_id" $responseRow.R_Opportunity_ID
			AddDateParameter $command "@response_date" $responseRow.R_Response_Date
			AddStringParameter $command "@comments" $responseRow.Comments
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@response_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$response_created = LogResult $command "@response_id" "Response created"
			
			if(!$response_created){
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
	$errors += CreateOpportunities $DBConnection
	$errors += CreateResponses $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}
