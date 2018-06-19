param (
    [string]$pledgeCampaignDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreatePledgeCampaigns.csv"),
    [string]$pledgeDataCSV = ((Split-Path $MyInvocation.MyCommand.Definition)+"\CreatePledges.csv"),
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

#Create all pledge campaigns in list
function CreatePledgeCampaigns($DBConnection){
    $pledgeCampaignDataList = import-csv $pledgeCampaignDataCSV
    $error_count = 0
    foreach($pledgeCampaignRow in $pledgeCampaignDataList)
    {
        if(![string]::IsNullOrEmpty($pledgeCampaignRow.R_Campaign_Name))
        {
            #Create command to be executed
            $command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Pledge_Campaign"
            
            #Add parameters to command - parameter names must match stored proc parameter names
            AddStringParameter $command "@campaign_name" $pledgeCampaignRow.R_Campaign_Name
            AddIntParameter $command "@campaign_type_id" $pledgeCampaignRow.R_Pledge_Campaign_Type            
            AddMoneyParameter $command "@goal" $pledgeCampaignRow.R_Campaign_Goal
            AddDateParameter $command "@start_date" $pledgeCampaignRow.R_Start_Date
            AddDateParameter $command "@end_date" $pledgeCampaignRow.End_Date            
            AddStringParameter $command "@description" $pledgeCampaignRow.Description            
            AddDateParameter $command "@registration_start" $pledgeCampaignRow.Registration_Start_Date
            AddDateParameter $command "@registration_end" $pledgeCampaignRow.Registration_End_Date
            AddMoneyParameter $command "@registration_deposit" $pledgeCampaignRow.Registration_Deposit
            AddIntParameter $command "@registration_form_id" $pledgeCampaignRow.Registration_Form_ID
            AddMoneyParameter $command "@fundraising_goal" $pledgeCampaignRow.Fundraising_Goal
            AddIntParameter $command "@destination_id" $pledgeCampaignRow.Destination_ID
            AddIntParameter $command "@youngest_age" $pledgeCampaignRow.Youngest_Age_Allowed
            AddStringParameter $command "@program_name" $pledgeCampaignRow.Program_Name
            AddStringParameter $command "@event_name" $pledgeCampaignRow.Event_Name
            AddStringParameter $command "@nickname" $pledgeCampaignRow.Campaign_Nickname
            AddOutputParameter $command "@error_message" "String"
            AddOutputParameter $command "@campaign_id" "Int32"
            
            #Execute and report results
            $result = $command.ExecuteNonQuery()
            $error_found = LogResult $command "@error_message" "ERROR"
            $pledge_campaign_created = LogResult $command "@campaign_id" "Pledge Campaign created"
            
            if(!$pledge_campaign_created){
                $error_count += 1
            }            
        }
    }
    return $error_count
}

#Create all pledges in list
function CreatePledges($DBConnection){
    $pledgeDataList = import-csv $pledgeDataCSV
    $error_count = 0
    foreach($pledgeRow in $pledgeDataList)
    {
        if(![string]::IsNullOrEmpty($pledgeRow.R_Donor_Email))
        {
            #Create command to be executed
            $command = CreateStoredProcCommand $DBConnection "cr_QA_Create_Pledge"
            
            #Add parameters to command - parameter names must match stored proc parameter names
            AddStringParameter $command "@donor_email" $pledgeRow.R_Donor_Email
            AddStringParameter $command "@campaign_name" $pledgeRow.R_Pledge_Campaign
            AddMoneyParameter $command "@amount_pledged" $pledgeRow.R_Total_Pledge
            AddDateParameter $command "@first_installment_date" $pledgeRow.R_First_Installment_Date
            AddIntParameter $command "@installments_planned" $pledgeRow.R_Installments_Planned
            AddOutputParameter $command "@error_message" "String"
            AddOutputParameter $command "@pledge_id" "Int32"
            
            #Execute and report results
            $result = $command.ExecuteNonQuery()
            $error_found = LogResult $command "@error_message" "ERROR"
            $pledge_created = LogResult $command "@pledge_id" "Pledge created"
            
            if(!$pledge_created){
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
    $errors += CreatePledgeCampaigns $DBConnection
    $errors += CreatePledges $DBConnection
} catch {
    write-host "Error encountered in $($MyInvocation.MyCommand.Name): "$_
    exit 1
} finally {
    $DBConnection.Close();
    if($errors -ne 0){
        exit 1
    }
}