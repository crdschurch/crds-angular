param (
    [string]$groupDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateGroups.csv"),
    [string]$addChildGroupDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\AddChildGroup.csv"),
    [string]$attributeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateAttributes.csv"),
    [string]$groupAttributeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateGroupAttributes.csv"),
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

#Create all groups in list
function CreateGroups($DBConnection){
	$groupDataList = import-csv $groupDataCSV

	foreach($groupRow in $groupDataList)
	{
		if(![string]::IsNullOrEmpty($groupRow.R_Group_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Group"
						
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@group_name" $groupRow.R_Group_Name
			AddStringParameter $command "@primary_contact_email" $groupRow.R_Primary_Contact
			AddIntParameter $command "@group_type_id" $groupRow.R_Group_Type_ID
			AddIntParameter $command "@ministry_id" $groupRow.R_Ministry_ID
			AddIntParameter $command "@congregation_id" $groupRow.R_Congregation_ID
			AddDateParameter $command "@start_date" $userRow.R_Start_Date
			AddBitParameter $command "@child_care_available" $userRow.R_Child_Care_Available
			AddBitParameter $command "@primary_contact_is_host" $userRow.Is_Primary_Contact_Host
			AddBitParameter $command "@enable_waiting_list" $userRow.Enable_Waiting_List
			AddIntParameter $command "@target_size" $groupRow.Target_Size
			AddStringParameter $command "@description" $groupRow.Description
			AddStringParameter $command "@is_public" $groupRow.IsPublic
			AddStringParameter $command "@is_blog_enabled" $groupRow.IsBlogEnabled
			AddStringParameter $command "@is_web_enabled" $groupRow.IsWebEnabled
			AddIntParameter $command "@deadline_passed_message_id" $groupRow.Deadline_Passed_Message_ID
			AddDateParameter $command "@meeting_time" $userRow.Meeting_Time
			AddStringParameter $command "@meeting_day" $groupRow.Meeting_Day
			AddBitParameter $command "@available_online" $groupRow.Available_Online
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@group_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$group_created = LogResult $command "@group_id" "Group created"
			
			if(!$group_created){
				throw
			}	
		}
	}
}

#Add child group to parent group
function AddChildGroup($DBConnection){
	$addChildGroupDataList = import-csv $addChildGroupDataCSV

	foreach($groupRow in $addChildGroupDataList)
	{
		if(![string]::IsNullOrEmpty($groupRow.R_Parent_Group_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Add_Child_Group"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@parent_group_name" $groupRow.R_Parent_Group_Name			
			AddStringParameter $command "@child_group_name" $groupRow.R_Child_Group_Name		
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@parent_group_id" "Int32"
			AddOutputParameter $command "@child_group_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$child_found = LogResult $command "@child_group_id" "Added Group"
			$parent_found = LogResult $command "@parent_group_id" "        to parent Group"
			
			if(!$child_found -or !$parent_found){
				throw
			}
		}
	}
}

#Creates all attributes in list
function CreateAttributes($DBConnection){
	$attributeDataList = import-csv $attributeDataCSV

	foreach($attributeRow in $attributeDataList)
	{
		if(![string]::IsNullOrEmpty($attributeRow.R_Attribute_Name))
		{
		#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Attribute"
						
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@attribute_name" $attributeRow.R_Attribute_Name	
			AddIntParameter $command "@attribute_type_id" $attributeRow.Attribute_Type	
			AddIntParameter $command "@attribute_category_id" $attributeRow.Attribute_Category	
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@attribute_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$attribute_created = LogResult $command "@attribute_id" "Attribute created"
			
			if(!$attribute_created){
				throw
			}
		}
	}
}

#Creates group attributes
function CreateGroupAttributes($DBConnection){
	$groupAttributeDataList = import-csv $groupAttributeDataCSV

	foreach($attributeRow in $groupAttributeDataList)
	{
		if(![string]::IsNullOrEmpty($attributeRow.R_Attribute_Name))
		{
		#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Group_Attribute"
						
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@attribute_name" $attributeRow.R_Attribute_Name
			AddStringParameter $command "@group_name" $attributeRow.R_Group_Name
			AddDateParameter $command "@start_date" $attributeRow.R_Start_Date
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@group_attribute_id" "Int32"
			
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$attribute_created = LogResult $command "@group_attribute_id" "Group Attribute created"
			
			if(!$attribute_created){
				throw
			}
		}
	}
}

#Execute all the Create functions
try{
	$DBConnection = OpenConnection
	CreateGroups $DBConnection
	AddChildGroup $DBConnection
	CreateAttributes $DBConnection
	CreateGroupAttributes $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}