USE [MinistryPlatform]
GO

DECLARE @Template_ID int = 2018;

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Communications] WHERE Communication_ID = @Template_ID) 
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	INSERT INTO [dbo].[dp_Communications]
			   (Communication_ID
			   ,[Author_User_ID]
			   ,[Subject]
			   ,[Body]
			   ,[Domain_ID]
			   ,[Start_Date]
			   ,[From_Contact]
			   ,[Reply_to_Contact]
			   ,[Template]
			   ,[Active])
		 VALUES
			   (@Template_ID
			   ,1
			   ,N'Action Required: Group Leader Application Reference Request'
			   ,N'<div>Hello [Recipient_First_Name], </div>

<div><br /></div>

<div>[First_Name] [Last_Name] has applied to be a small group leader and selected you as the person who knows them best at Crossroads.</div><div><br /></div><div>Please go the the <a href=''https://[Base_Url]/ministryplatform#/355/[Participant_ID]''> Participants page </a> in Ministry Platform.  </div><div><br /></div><div>For accepting applications to be a small group leader, please change their Small Group Leader Status to Approved and click the Submit button.  Once you have done so, the applicant will receive an email stating they have been approved.  Someone from Spiritual Growth will follow up to get a coach for them.</div><div><br /></div><div>If you have chosen to deny this request, you will need to reach out to the individual to inform them why their request has been denied.  This will be the only communication they receive.  You will then need to change their Small Group Leader Status to Denied.  Also, update the feedback in Ministry Platform as to why you have denied the request and the date you had the conversation with the individual who was denied.</div><div><br /></div><div>If you do not know the individual who has asked for a reference, please forward this email to groups@crossroads.net.</div>'
			   ,1
			   ,'2017-05-10'
			   ,7675411
			   ,7675411			   
			   ,1
			   ,1)
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END

