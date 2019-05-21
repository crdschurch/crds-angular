USE MinistryPlatform

DECLARE @JoinGroupEmailId INT = 2026;

IF NOT EXISTS(SELECT * FROM [dbo].[dp_Communications] WHERE communication_id =@JoinGroupEmailId)
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
				@JoinGroupEmailId,
				5,
				'[Nickname] [Last_Name] Wants to Try Your [Group_Name] Group - ACTION REQUIRED',
				'<blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div style="font-family: arial, sans-serif; font-size: 12px;">Hi [Primary_First_Name]!</div><div style="font-family: arial, sans-serif; font-size: 12px;"><br /></div><div style="font-family: arial, sans-serif; font-size: 12px;">[Nickname] [Last_Name] ([Email_Address]) is interested in your [Group_Name] group. </div><div style="font-family: arial, sans-serif; font-size: 12px;"><br /></div><div style="font-family: arial, sans-serif; font-size: 12px;">Let [Nickname] know if you want them to try out your group, or let [Nickname] know if now isn''t a good time for adding a new person. When you click one of the links below an automatic email will be sent to [Nickname]. If you approve her for your group then your full name, address, and contact information (email and phone number) will be sent in the email. </div><div style="font-family: arial, sans-serif; font-size: 12px;"><br /></div><div style=""><font face="arial, sans-serif"><span style="font-size: 12px;"><a href="[YesURL]">Yes - try my group</a> </span></font><span style="font-family: arial, sans-serif; font-size: 12px;">- </span><b style="font-family: arial, sans-serif; font-size: 12px;">The following will occur if you click this option:</b></div><div style="font-family: arial, sans-serif; font-size: 12px;"><br /></div><div style="font-family: arial, sans-serif; font-size: 12px;"><ul><li>An email will automatically be sent to [Nickname] [Last_Name] containing:</li></ul></div></blockquote><blockquote style="font-family: arial, sans-serif; font-size: 12px; margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Group Name: [Group_Name]</blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Group Leader: [Primary_First_Name] [Primary_Last_Name] </blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Group Leader Contact Info: [Primary_Email] [Primary_Phone]</blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Day: [Group_Meeting_Day] </blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Time: [Group_Meeting_Time] </blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Frequency: [Group_Meeting_Frequency] </blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">Location: </blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;">[Group_Meeting_Location] </blockquote></blockquote></blockquote><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div style="font-family: arial, sans-serif; font-size: 12px;"><ul><li><b>WHAT YOU NEED TO DO:</b></li><ul><li>Right now....yes now while you''re on your email - Reach out to [Nickname] at [Email_Address] or [Phone_Number] to let them know the date they should start attending your group.</li><li>In 60 days - let us know if [Nickname] has officially joined your group. And don''t worry - we''ll send you a reminder email since you likely have a ton of things going on in your life. </li></ul></ul></div><div style=""><font face="arial, sans-serif"><span style="font-size: 12px;"><a href="[NoURL]">No - not adding new members now</a></span></font><span style="font-family: arial, sans-serif; font-size: 12px;"> - Clicking this option will send [Nickname] [Last_Name] an automatic email to let them know that this group isn''t taking new members and will encourage [Nickname] to find another group. </span><b style="font-family: arial, sans-serif; font-size: 12px;">None</b><span style="font-family: arial, sans-serif; font-size: 12px;"> of your contact information will be shared with [Nickname].</span></div><div style="font-family: arial, sans-serif; font-size: 12px;"><br /></div><div style="font-family: arial, sans-serif; font-size: 12px;">Thanks! </div><div style="font-family: arial, sans-serif; font-size: 12px;">Crossroads Spiritual Growth Team </div></blockquote>',
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
