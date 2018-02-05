param (
	[parameter(Mandatory=$true)] [string]$userCredentialsCSV,
	[parameter(Mandatory=$true)] [string]$userListCSV,
	[string]$loginEndpoint = 'https://gatewayint.crossroads.net/gateway/api/Login',
	[string]$participantEndpoint = 'https://gatewayint.crossroads.net/gateway/api/Participant',
	[string]$profileEndpoint = 'https://gatewayint.crossroads.net/gateway/api/Profile',
	[string]$pinEndpoint = 'https://gatewayint.crossroads.net/gateway/api/finder/pin',
	[string]$deleteAwsEndpoint = 'https://gatewayint.crossroads.net/gateway/api/finder/deleteallcloudsearchrecords',
	[string]$uploadAwsEndpoint = 'https://gatewayint.crossroads.net/gateway/api/finder/uploadallcloudsearchrecords'
 )

function addUsersToMap
{
	$mapUserList = import-csv $userListCSV
	$logFile = "log_creating_map_data.txt"
	
	foreach($mapUser in $mapUserList)
	{
		if(!$mapUser.email.IsNullOrEmpty)
		{
			#find their password
			$password = getPassword $mapUser.email
			if($password.IsNullOrEmpty)
			{
				Add-Content $logFile "User $($mapUser.email) could not be added to the map because their password could not be found"
				break
			}
			
			#Build the login request
			$login = @{
				username= $mapUser.email
				password= $password
			};
			$loginJson = $login | ConvertTo-Json;
			
			#Login as the user
			try {
				$loginResponse = Invoke-RestMethod $loginEndpoint -Method Post -Body $loginJson -ContentType 'application/json'
				Add-Content $logFile "Logged in as $($mapUser.email)"
			}
			catch{
				Add-Content $logFile "An error occurred logging in as $($mapUser.email)" 
				break
			}
			
			#Add the userToken to a header
			$header = New-Object "System.Collections.Generic.Dictionary[[String], [String]]"
			$header.Add("Authorization", $loginResponse.userToken)
			
			#Get the user's participant record
			try {
				$participantRecord = Invoke-RestMethod $participantEndpoint -Method Get -Headers $header
				Add-Content $logFile "Retrieved participant record. ParticipantID: $($participantRecord.ParticipantID)"
			}
			catch{
				Add-Content $logFile "An error occurred retrieving the user's participant record"
				break
			}

			#Get the user's profile
			try {
				$profile = Invoke-RestMethod $profileEndpoint -Method Get -Headers $header
			}
			catch{
				Add-Content $logFile "An error occurred retrieving the user's profile"
				break
			}
			
			#Build the pin request
			$pin = @{
				updateHomeAddress = "false"
				firstName = $mapUser.first
				lastName = $mapUser.last
				#siteName = [NullString]::Value
				emailAddress = $mapUser.email
				contactId = $participantRecord.ContactId
				participantId = $participantRecord.ParticipantId
				address = @{
					#addressId = [NullString]::Value
					addressLine1 = $mapUser.address1
					addressLine2 = $mapUser.address2
					city = $mapUser.city
					state = $mapUser.state
					zip = $mapUser.zip
					foreignCountry = $mapUser.country
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
				Add-Content $logFile "Added $($mapUser.email) to the map"
			}
			catch{
				Add-Content $logFile "An error occurred trying to add the user $($mapUser.email) to the map"
			}
		}
	}
}

function getPassword($email){
	$userList = import-csv $userCredentialsCSV
	foreach($user in $userList)
	{
		if($($user.email).equals($email))
		{
			return $user.password
		}
	}
}

addUsersToMap #run this function automatically