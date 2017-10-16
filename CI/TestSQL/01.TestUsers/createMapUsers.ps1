param (
    [string]$userEndpoint = 'https://gatewayint.crossroads.net/gateway/api/User',
	[string]$loginEndpoint = 'https://gatewayint.crossroads.net/gateway/api/Login',
	[string]$participantEndpoint = 'https://gatewayint.crossroads.net/gateway/api/Participant',
	[string]$profileEndpoint = 'https://gatewayint.crossroads.net/gateway/api/Profile',
	[string]$pinEndpoint = 'https://gatewayint.crossroads.net/gateway/api/finder/pin',
    [string]$deleteAwsEndpoint = 'https://gatewayint.crossroads.net/gateway/api/finder/deleteallcloudsearchrecords',
    [string]$uploadAwsEndpoint = 'https://gatewayint.crossroads.net/gateway/api/finder/uploadallcloudsearchrecords'
 )

$userList = import-csv .\mapUserList.csv

foreach($user in $userList)
{

	if(![string]::IsNullOrEmpty($user.first))
	{
		write-host "Adding User" $user.first $user.last "with email" $user.email;
		
		#Build the user request
		$person = @{
			firstName= $user.first
			lastName= $user.last
			email= $user.email
			password= 'welcome'
		};
		$personJson = $person | ConvertTo-Json;
		
		#Create the user
		try {
			$createResponse = Invoke-RestMethod $userEndpoint -Method Post -Body $personJson -ContentType 'application/json'
			write-host "Successfully added user account" $user.email"!" -foregroundcolor green
		}		
		catch{
			write-host "An error occurred adding "$user.first $user.last "with email" $user.email"!" -foregroundcolor red
		}
		
		#Build the login request
		$login = @{
			username= $user.email
			password= 'welcome'
		};
		$loginJson = $login | ConvertTo-Json;
		
		#Login as the user
		try {
			$loginResponse = Invoke-RestMethod $loginEndpoint -Method Post -Body $loginJson -ContentType 'application/json'
			write-host "Successfully logged in as " $user.email"!" -foregroundcolor green
		}
		catch{
			write-host "An error occurred logging in as "%user.first $user.last "with email" %user.email"!" -foregroundcolor red
		}
		
		#Add the userToken to a header
		$header = New-Object "System.Collections.Generic.Dictionary[[String], [String]]"
		$header.Add("Authorization", $loginResponse.userToken)
				
		#Get the user's participant record
		try {
			$participantRecord = Invoke-RestMethod $participantEndpoint -Method Get -Headers $header
			write-host "Successfully retrieved the users's participant record. ParticipantID: "$participantRecord.ParticipantID -foregroundcolor green
		}
		catch{
			write-host "An error occurred retrieving the user's participant record" -foregroundcolor red
		}

        #Get the user's profile
        try {
            $profile = Invoke-RestMethod $profileEndpoint -Method Get -Headers $header
            write-host "Successfully retrieved the user's profile"  -foregroundcolor green
        }
        catch{
            write-host "An error occurred retrieving the user's participant record" -foregroundcolor red
        }

        #Build the pin request
        $pin = @{
            updateHomeAddress = "false"
            firstName = $user.first
            lastName = $user.last
            #siteName = [NullString]::Value
            emailAddress = $user.email
            contactId = $participantRecord.ContactId
            participantId = $participantRecord.ParticipantId
            address = @{
                #addressId = [NullString]::Value
                addressLine1 = $user.address1
                addressLine2 = $user.address2
                city = $user.city
                state = $user.state
                zip = $user.zip
                foreignCountry = $user.country
                #county = [NullString]::Value
                #longitude = [NullString]::Value
                #latitude = [NullString]::Value
            }
            hostStatus = 0
            #gathering = [NullString]::Value
            pinType = 1
            #proximity = [NullString]::Value
            householdId = $profile.householdId
            iconUrl = ""
            title = ""                                 
        };
        $pinJson = $pin | ConvertTo-Json -Depth 5 -Compress;

		#Add the user to the map
        try {
        	$pinResponse = Invoke-RestMethod $pinEndpoint -Method Post -Headers $header -Body $pinJson -ContentType 'application/json'
            write-host "Successfully added the user with email " $user.email " to the map" -ForegroundColor Green
        }
        catch{
            write-host "An error occurred trying to add the user with email " $user.email " to the map" -ForegroundColor Red
        }
	}
}

#Refresh AWS
#Delete cloudsearchrecords
try {
    $deleteAwsResponse = Invoke-RestMethod $deleteAwsEndpoint
    write-host "Deleted cloud search records" -ForegroundColor Green
}
catch{
    write-host "An error occurred trying to delete cloud search records" -ForegroundColor Red
}

#Upload cloudsearchrecords
try {
    $uploadAwsResponse = Invoke-RestMethod $uploadAwsEndpoint
    write-host "Uploaded cloud search records" -ForegroundColor Green
}
catch{
    write-host "An error occurred trying to upload cloud search records" -ForegroundColor Red
} 