USE MinistryPlatform

DECLARE @GATHERINGPENDINGID INTEGER = 2030
DECLARE @BODY VARCHAR(2000) = '<div>Hi [Nick_Name],</div><div><br /></div><div>You have [Pending_Requests_Count] pending request(s) from people who want to join your &quot;[Group_Name]&quot; gathering.</div><div><br /></div><div>Go to https://[BaseUrl]/connect and look at My Connections to review them, and approve or deny the gathering participants.</div><div><br /></div><div><br /></div><div>Thanks!</div><div>Crossroads Anywhere Team</div><div><br /></div>'
DECLARE @SUBJECT VARCHAR(200) = 'Pending Request(s) to Join Gathering'


IF NOT EXISTS(SELECT * FROM dp_Communications WHERE Communication_ID = @GATHERINGPENDINGID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON;

	INSERT INTO [dbo].[dp_Communications](Communication_ID, Author_User_ID, Subject, Body, Domain_ID, Start_Date, 
				Communication_Status_ID, From_Contact, Reply_to_Contact, Template, Active)
	        VALUES (@GATHERINGPENDINGID,5,@SUBJECT,@BODY,1,GETDATE(),1,768371,768371,1,1);

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END

GO

SELECT * FROM dp_Communications WHERE Communication_ID in (2011,2030)