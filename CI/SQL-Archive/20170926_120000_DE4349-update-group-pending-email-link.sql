USE MinistryPlatform

DECLARE @GROUPPENDINGID INTEGER = 2004
DECLARE @BODY VARCHAR(2000) = '<div>Hi [Nick_Name] [Last_Name],</div><div><br /></div><div>You have [Pending_Requests_Count] pending request(s) from people who want to join your &quot;[Group_Name]&quot; group. Log into your crossroads.net account, and go to your groups dashboard to view them, and approve or deny the group participants.</div><div><br /></div><div>https://[BaseUrl]/groups/search/my</div><div><br /></div><div><br /></div><div>Thanks!</div><div>Crossroads Groups Team</div><div><br /></div><div><i>This email was sent to: [All_Leaders]</i></div>'

IF EXISTS(SELECT * FROM dp_Communications WHERE Communication_ID = @GROUPPENDINGID)
BEGIN
	UPDATE dp_Communications SET Body = @BODY WHERE Communication_ID = @GROUPPENDINGID
END
