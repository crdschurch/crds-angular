param (
    [string]$opportunityDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateOpportunities.csv"),
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


#Deletes all opportunities in the list
function DeleteOpportunities($DBConnection){
	$opportunityList = import-csv $opportunityDataCSV
	
	foreach($opportunity in $opportunityList)
	{
		if(![string]::IsNullOrEmpty($opportunity.R_Opportunity_Title))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Opportunity_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@opportunity_name" $opportunity.R_Opportunity_Title
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Opportunity" $opportunity.R_Opportunity_Title
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to opportunity "$opportunity.R_Opportunity_Title
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeleteOpportunities $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}