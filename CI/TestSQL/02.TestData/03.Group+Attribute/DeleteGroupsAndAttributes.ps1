param (
    [string]$groupListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateGroups.csv"),
    [string]$attributeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateAttributes.csv"),
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

#Deletes all groups in the list
function DeleteGroups($DBConnection){
	$groupList = import-csv $groupListCSV
	
	foreach($group in $groupList)
	{
		if(![string]::IsNullOrEmpty($group.R_Group_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Group_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@group_name" $group.R_Group_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Group" $group.R_Group_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to group "$group.R_Group_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Deletes all attributes in the list
function DeleteAttributes($DBConnection){
	$attributeList = import-csv $attributeDataCSV
	
	foreach($attribute in $attributeList)
	{
		if(![string]::IsNullOrEmpty($attribute.R_Attribute_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Attribute_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@attribute_name" $attribute.R_Attribute_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Attribute" $attribute.R_Attribute_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to attribute "$attribute.R_Attribute_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeleteGroups $DBConnection
	DeleteAttributes $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}