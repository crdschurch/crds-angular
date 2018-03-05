param (
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

#Create all invoices in list
function CreateInvoicesWithPayment($DBConnection){
	$invoiceAndPaymentDataList = import-csv $invoiceAndPaymentDataCSV
	
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
				throw
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
				throw
			}
		}
	}
}

#Execute all the update functions
try{
	$DBConnection = OpenConnection
	CreateInvoicesWithPayment $DBConnection
} catch {
	write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
	exit 1
} finally {
	$DBConnection.Close();
}