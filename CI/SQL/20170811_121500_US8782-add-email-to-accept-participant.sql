USE MinistryPlatform

DECLARE @GroupUserAcceptedEmailId INT = 2027;

IF NOT EXISTS(SELECT * FROM [dbo].[dp_Communications] WHERE communication_id =@GroupUserAcceptedEmailId)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON;

	INSERT INTO [dbo].[dp_Communications](
				Communication_ID, 
				Author_User_ID, 
				Subject, 
				Body, 
				Domain_ID, 
				Start_Date, 
				Communication_Status_ID, 
				From_Contact, 
				Reply_to_Contact, 
				Template, 
				Active)
	        VALUES (
				@GroupUserAcceptedEmailId,
				5,
				'You''re now in the [Group_Name] group! ',
				'<blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div style="font-family: arial, sans-serif; font-size: 12px;">Hi [Nickname], <br /><br />Great news! [Primary_First_Name] is excited to have you attend the [Group_Name] group! Here are the details: <br /><br /><span style="font-weight: bold;">Group</span>: [Group_Name] <br /><span style="font-weight: bold;">Group Leader:</span> [Primary_First_Name] [Primary_Last_Name] <br /><span style="font-weight: bold;">Day</span>: [Group_Meeting_Day] <br /><span style="font-weight: bold;">Time</span>: [Group_Meeting_Time] <br /><span style="font-weight: bold;">Frequency</span>: [Group_Meeting_Frequency] <br /><span style="font-weight: bold;">Location</span>: <br />[Group_Meeting_Location] <br /><br />If you have any questions about the group, please contact [Primary_First_Name] [Primary_Last_Name]  at [Primary_Phone] or [Primary_Email]. <br /><br />Thanks! <br />Crossroads Spiritual Growth Team  <br /></div></blockquote>',
				1,
				GETDATE(),
				1,
				7676252,
				7676252,
				1,
				1);

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END

GO
