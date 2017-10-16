USE [MinistryPlatform]
GO
DECLARE @CommunicationID    AS INT
SET     @CommunicationID = 2025
IF EXISTS (SELECT * 
		   FROM [dbo].[dp_Communications]
		   WHERE Communication_ID = @CommunicationID)

BEGIN
DELETE  
FROM [dbo].[dp_Communications]
WHERE Communication_ID = @CommunicationID
END

SET IDENTITY_INSERT [dbo].[dp_Communications] ON
INSERT INTO [dbo].[dp_Communications]
           ([Communication_ID]
           ,[Author_User_ID]
           ,[Subject]
           ,[Body]
           ,[Domain_ID]
           ,[Start_Date]
		   ,[Communication_Status_ID]
           ,[From_Contact]
           ,[Reply_to_Contact]
           ,[Template]
           ,[Active])
     VALUES
           (@CommunicationID
		   ,5
		   ,'You''ve Been Added to the [Group_Name] group!'
		   ,'<div class="gmail_default rally-rte-class-03755033359875" style="font-family: arial, sans-serif; font-size: 12px;">Hi <span font-size:="" medium;"="" new="" roman";="" style="font-family: " times="">[Participant_Name]!</span><br font-size:="" medium;"="" new="" roman";="" style="font-family: " times="" /><br /></div><div class="gmail_default rally-rte-class-03755033359875" style="font-family: arial, sans-serif; font-size: 12px;">[Leader_Full_Name] just added you to the [Group_Name] group. Here''s the group info:<br /></div><div class="gmail_default rally-rte-class-03755033359875" style="font-family: arial, sans-serif; font-size: 12px;"><br /></div><div class="gmail_default rally-rte-class-03755033359875" style="font-family: arial, sans-serif; font-size: 12px;"><blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><span style="font-weight: bold;">Group</span>: [Group_Name] </div></div></div><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><span style="font-weight: bold;">Group Leader: </span>[Leader_Full_Name] </div></div></div><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><span style="font-weight: bold;">Day</span>: [Group_Meeting_Day] </div></div></div><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><span style="font-weight: bold;">Time</span>: [Group_Meeting_Time] </div></div></div><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><span style="font-weight: bold;">Frequency</span>: [Group_Meeting_Frequency] </div></div></div><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><span style="font-weight: bold";>Location</span>: </style="font-weight:>[Group_Meeting_Location]</div></div></div></blockquote><div><div class="gmail_default rally-rte-class-0d816df0803d4"><div class="gmail_default rally-rte-class-03755033359875"><br />If you have any questions about the group, please contact [Leader_Full_Name] at [Leader_Phone] or [Leader_Email]. <br /><br />Thanks! <br />Crossroads Spiritual Growth Team  </div><div><br /></div></div></div></div><div class="gmail_default rally-rte-class-03755033359875" style="font-family: arial, sans-serif; font-size: 12px;"><br /></div>'
		   ,1
		   ,'2017-06-23 00:00:00.000'	   
		   ,1
		   ,7675411
		   ,7675411		   
		   ,0
		   ,0)  

SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
GO
