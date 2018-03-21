param (
    [string]$contactDataCSV =((Split-Path $MyInvocation.MyCommand.Definition)+"\UpdateContacts.csv"),
    [string]$contactRelationshipsDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateContactRelationships.csv"),
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


#Update all contacts in list
function UpdateContacts($DBConnection){
	$contactDataList = import-csv $contactDataCSV
	$error_count = 0
	foreach($userRow in $contactDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Contact_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Update_Contact"
						
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@contact_email" $userRow.R_Contact_Email
			AddStringParameter $command "@middle_name" $userRow.Middle_Name
			AddDateParameter $command "@birthdate" $userRow.Date_of_Birth
			AddIntParameter $command "@gender_id" $userRow.Gender_ID
			AddIntParameter $command "@marital_status_id" $userRow.Marital_Status_ID
			AddIntParameter $command "@household_position_id" $userRow.Household_Position_ID
			AddStringParameter $command "@mobile_phone_number" $userRow.Mobile_Phone
			AddStringParameter $command "@company_phone_number" $userRow.Company_Phone
			AddIntParameter $command "@prefix_id" $userRow.Prefix_ID
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@contact_id" "Int32"

			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$contact_created = LogResult $command "@contact_id" "Contact updated"
			
			if(!$contact_created){
				$error_count += 1
			}
		}
	}
	return $error_count
}

#Create contact relationships
function CreateContactRelationships($DBConnection){
	$contactRelationshipsDataList = import-csv $contactRelationshipsDataCSV
	$error_count = 0
	foreach($userRow in $contactRelationshipsDataList)
	{
		if(![string]::IsNullOrEmpty($userRow.R_Contact_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Contact_Relationship"

			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@contact_email" $userRow.R_Contact_Email
			AddStringParameter $command "@related_contact_email" $userRow.R_Related_Contact_Email
			AddIntParameter $command "@relationship_id" $userRow.R_Relationship_ID
			AddDateParameter $command "@start_date" $userRow.Start_Date
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@contact_relationship_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$relationship_created = LogResult $command "@contact_relationship_id" "Contact Relationship"
			
			if(!$relationship_created){
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
	$errors += UpdateContacts $DBConnection
	$errors += CreateContactRelationships $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}