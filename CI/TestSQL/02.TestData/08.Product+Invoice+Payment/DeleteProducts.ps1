param (
    [string]$productDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateProducts.csv"),
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


#Deletes all products in the list
function DeleteProducts($DBConnection){
	$productList = import-csv $productDataCSV
	$error_count = 0
	foreach($product in $productList)
	{
		if(![string]::IsNullOrEmpty($product.R_Product_Name))
		{
			#Create command
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Delete_Product_By_Name"
			
			#Add variables for stored proc
			AddStringParameter $command "@product_name" $product.R_Product_Name
			
			#Execute command
			$adapter = new-object System.Data.SqlClient.SqlDataAdapter
			$adapter.SelectCommand = $command		
			$dataset = new-object System.Data.Dataset
			try { 
				write-host "Removing Product" $product.R_Product_Name
				$results = $adapter.Fill($dataset) 
			} catch {
				write-host "There was an error deleting data related to Product "$product.R_Product_Name
				write-host "Error: " $Error
				$error_count += 1
			}
		}
	}
	return $error_count
}

#Execute
try{
	$DBConnection = OpenConnection
	$errors = 0
	$errors += DeleteProducts $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}