param (
    [string]$pledgeCampaignDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreatePledgeCampaigns.csv"),
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


#Deletes all pledge campaigns in the list
function DeletePledgeCampaigns($DBConnection){
	$pledgeCampaignList = import-csv $pledgeCampaignDataCSV
	
	foreach($pledgeCampaign in $pledgeCampaignList)
	{
		if(![string]::IsNullOrEmpty($pledgeCampaign.R_Campaign_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Pledge_Campaign_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@pledge_campaign_name" $pledgeCampaign.R_Campaign_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Pledge Campaign" $pledgeCampaign.R_Campaign_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to Pledge Campaign "$pledgeCampaign.R_Campaign_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeletePledgeCampaigns $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}