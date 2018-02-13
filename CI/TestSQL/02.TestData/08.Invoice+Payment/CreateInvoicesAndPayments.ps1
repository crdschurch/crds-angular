param (
    [string]$invoiceAndPaymentDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreateInvoiceAndPayment.csv"),
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

#Create all invoices in list
function CreateInvoiceWithPayment($DBConnection){
	$invoiceAndPaymentDataList = import-csv $invoiceAndPaymentDataCSV
	
	foreach($invoiceRow in $invoiceAndPaymentDataList)
	{
		if(![string]::IsNullOrEmpty($invoiceRow.R_Purchaser_Email))
		{
			#Create command to be executed
			$command = CreateStoredProcCommand $DBConnection "cr_QA_New_Invoice_With_Payment"
			
			#Add parameters to command - parameter names must match stored proc parameter names
			AddStringParameter $command "@user_email" $invoiceRow.R_Purchaser_Email			
			AddStringParameter $command "@invoice_total" $invoiceRow.R_Invoice_Total
			AddDateParameter $command "@invoice_date" $invoiceRow.R_Invoice_Date
			AddStringParameter $command "@product_name" $invoiceRow.R_Product_Name
			AddStringParameter $command "@batch_name" $invoiceRow.Batch_Name
			AddStringParameter $command "@payment_total" $invoiceRow.R_Payment_Total
			AddDateParameter $command "@payment_date" $invoiceRow.R_Payment_Date
			AddIntParameter $command "@payment_type_id" $invoiceRow.Payment_Type_ID
			AddStringParameter $command "@payment_transaction_code" $invoiceRow.Transaction_Code
			AddIntParameter $command "@congregation_id" $invoiceRow.R_Congregation_ID
			AddOutputParameter $command "@error_message" "String"
			AddOutputParameter $command "@invoice_id" "Int32"
			AddOutputParameter $command "@invoice_detail_id" "Int32"
			AddOutputParameter $command "@payment_id" "Int32"
			AddOutputParameter $command "@payment_detail_id" "Int32"
				
			#Execute and report results
			$result = $command.ExecuteNonQuery()
			$error_found = LogResult $command "@error_message" "ERROR"
			$invoice_created = LogResult $command "@invoice_id" "Invoice created"
			$invoice_detail_created = LogResult $command "@invoice_detail_id" "        with Invoice Detail"
			$payment_created = LogResult $command "@payment_id" "Payment created"
			$payment_detail_created = LogResult $command "@payment_detail_id" "        with Payment Detail"
			
			if(!$invoice_created -or !$payment_created){
				throw
			}
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateInvoiceWithPayment $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}