. ((Split-Path $MyInvocation.MyCommand.Path)+"\DBFormat.ps1")

function CreateStoredProcCommand{
	param([System.Data.SqlClient.SqlConnection]$DBConnection,
		[String]$proc_name)
	$command = New-Object System.Data.SqlClient.SqlCommand
	$command.CommandType = [System.Data.CommandType]'StoredProcedure'
	$command.CommandText = $proc_name
	$command.Connection = $DBConnection
	$command.CommandTimeout = 900 #Set command timeout to 15 minutes. Teardown takes a while.
	return $command
}

function AddStringParameter{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$value)
	
	$db_value = CatchNullString($value)
	$command.Parameters.AddWithValue($parameter, $db_value) | Out-Null	
}

function AddIntParameter{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$value)
	
	$db_value = StringToInt($value)
	$command.Parameters.AddWithValue($parameter, $db_value) | Out-Null	
}

function AddBitParameter{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$value)
	
	$db_value = StringToBit($value)
	$command.Parameters.AddWithValue($parameter, $db_value) | Out-Null	
}

function AddDateParameter{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$value)
	
	$db_value = StringToDate($value)
	$command.Parameters.AddWithValue($parameter, $db_value) | Out-Null	
}

function AddMoneyParameter{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$value)
	
	$db_value = CatchNullString($value)
	$command.Parameters.AddWithValue($parameter, $db_value) | Out-Null	
}

function AddOutputParameter{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$type,
		[Int32]$size = 500
	)
	$output = new-object System.Data.SqlClient.SqlParameter
	$output.ParameterName = $parameter
	$output.Direction = [System.Data.ParameterDirection]'Output'
	$output.DBType = [System.Data.DbType]$type
	$output.Size = $size
	$command.Parameters.Add($output) | Out-Null
}

function LogResult{
	param([System.Data.SqlClient.SqlCommand]$command,
		[String]$parameter,
		[String]$message_intro)
	
	$message = $command.Parameters[$parameter].Value
	if(![string]::IsNullOrEmpty($message)){
		write-host $message_intro": "$message
		return $true
	}
	return $false
}