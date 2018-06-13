param (
    [string]$productDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateProducts.csv"),
    [string]$invoiceAndPaymentDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateInvoicesAndPayments.csv"),
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

#Create all invoices in list
function CreateInvoicesWithPayment($DBConnection){
	$invoiceAndPaymentDataList = import-csv $invoiceAndPaymentDataCSV
	$error_count = 0
	foreach($invoiceRow in $invoiceAndPaymentDataList)
	{
		if(![string]::IsNullOrEmpty($invoiceRow.R_Purchaser_Email))
		{
			#Create command to create invoice
			$i_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Invoice"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $i_command "@purchaser_email" $invoiceRow.R_Purchaser_Email
			AddMoneyParameter $i_command "@invoice_total" $invoiceRow.R_Invoice_Total
			AddDateParameter $i_command "@invoice_date" $invoiceRow.R_Invoice_Date
			AddStringParameter $i_command "@product_name" $invoiceRow.R_Product_Name
			AddOutputParameter $i_command "@error_message" "String"
			AddOutputParameter $i_command "@invoice_id" "Int32"
			AddOutputParameter $i_command "@invoice_detail_id" "Int32"
			
			#Execute and report invoice creation results
			$result = $i_command.ExecuteNonQuery()
			$error_found = LogResult $i_command "@error_message" "ERROR"
			$invoice_created = LogResult $i_command "@invoice_id" "Invoice created"
			$invoice_detail_created = LogResult $i_command "@invoice_detail_id" "        with Invoice Detail"
			
			if(!$invoice_created){
				$error_count += 1
				continue
			}
			
			$invoice_id = $i_command.Parameters["@invoice_id"].Value
		
			#Create command to create payment
			$p_command = CreateStoredProcCommand $DBConnection "cr_QA_New_Payment"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $p_command "@contact_email" $invoiceRow.R_Purchaser_Email
			AddMoneyParameter $p_command "@payment_total" $invoiceRow.R_Payment_Total
			AddDateParameter $p_command "@payment_date" $invoiceRow.R_Payment_Date
			AddIntParameter $p_command "@congregation_id" $invoiceRow.R_Congregation_Id
			AddStringParameter $p_command "@batch_name" $invoiceRow.Batch_Name
			AddIntParameter $p_command "@invoice_id" $invoice_id
			AddIntParameter $p_command "@payment_type_id" $invoiceRow.Payment_Type_Id
			AddStringParameter $p_command "transaction_code" $invoiceRow.Transaction_Code
			AddOutputParameter $p_command "@error_message" "String"
			AddOutputParameter $p_command "@payment_id" "Int32"
			AddOutputParameter $p_command "@payment_detail_id" "Int32"
				
			#Execute and report results
			$result = $p_command.ExecuteNonQuery()
			$error_found = LogResult $p_command "@error_message" "ERROR"
			$payment_created = LogResult $p_command "@payment_id" "Payment created"
			$payment_detail_created = LogResult $p_command "@payment_detail_id" "        with Payment Detail"
			
			if(!$payment_created){
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
    $errors += CreateProducts $DBConnection
	$errors += CreateInvoicesWithPayment $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
	if($errors -ne 0){
		exit 1
	}
}