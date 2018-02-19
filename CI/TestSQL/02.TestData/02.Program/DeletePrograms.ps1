param (
    [string]$programListCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreatePrograms.csv"),
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

#Deletes all programs in the list
function DeletePrograms($DBConnection){
	$programList = import-csv $programListCSV
	
	foreach($program in $programList)
	{
		if(![string]::IsNullOrEmpty($program.R_Program_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Program_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@program_name" $program.R_Program_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Program" $program.R_Program_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to program "$program.R_Program_Name
				write-host "Error: " $Error
			}
		}
	}
}

#Execute
try{
	$DBConnection = OpenConnection
	DeletePrograms $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}