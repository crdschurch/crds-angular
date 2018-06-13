param (
    [string]$programDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreatePrograms.csv"),
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

#Create all programs in list
function CreatePrograms($DBConnection){
	$programDataList = import-csv $programDataCSV
	$error_count = 0
	foreach($programRow in $programDataList)
	{
		if(![string]::IsNullOrEmpty($programRow.R_Program_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Program"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@program_name" $programRow.R_Program_Name			
			AddStringParameter $command "@primary_contact_email" $programRow.R_Contact_Email			
			AddDateParameter $command "@start_date" $programRow.R_Start_Date
			AddDateParameter $command "@end_date" $programRow.End_Date
			AddIntParameter $command "@congregation_id" $programRow.R_Congregation_ID
			AddIntParameter $command "@ministry_id" $programRow.R_Ministry_ID
			AddIntParameter $command "@program_type_id" $programRow.Program_Type_ID
			AddIntParameter $command "@communication_id" $programRow.Communication_ID
			AddStringParameter $command "@pledge_campaign_name" "" #We're not using this
			AddBitParameter $command "@available_online" $programRow.Available_Online
			AddBitParameter $command "@allow_recurring_giving" $programRow.R_Allow_Recurring_Giving
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@program_id" "Int32"
				
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$program_created = LogResult $command "@program_id" "Program created"
			
			if(!$program_created){
				$error_count += 1
			}
		}
	}
	return $error_count
}

#Create all products in list
function CreateProducts($DBConnection){
	$productDataList = import-csv $productDataCSV
	$error_count = 0
	foreach($productRow in $productDataList)
	{
		if(![string]::IsNullOrEmpty($productRow.R_Product_Name))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Product"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@product_name" $productRow.R_Product_Name			
			AddMoneyParameter $command "@base_price" $productRow.R_Base_Price
            AddMoneyParameter $command "@deposit_price" $productRow.Deposit_Price
            AddStringParameter $command "@program_name" $productRow.ProgramName
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@product_id" "Int32"
				
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$product_created = LogResult $command "@product_id" "Product created"
			
			if(!$product_created){
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
	$errors += CreatePrograms $DBConnection
    $errors += CreateProducts $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}