--select * from dp_Communications where template = 1 and body like '%frequency%'
USE MinistryPlatform

DECLARE @COMM_ID INT = 2029;
DECLARE @SUBJECT NVARCHAR(256) = '[Nickname] [Lastname] has Been Added to the [Group_Name] Group';
DECLARE @BODY NVARCHAR(2000) = '<div>Hi [Recipient_First_Name]!<br /><br /></div><div>[Nickname] has been added to your [Group_Name] Group. Here''s the group info:<br /></div><div><br /></div><div><blockquote style="color: rgb(34, 34, 34); margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div><div><div><span style="font-weight: bold;">Group</span>: [Group_Name]</div></div></div><div><div><div><span style="color: rgb(0, 0, 0); font-weight: bold;">Day</span><span style="color: rgb(0, 0, 0);">: [Group_Meeting_Day] </span><br style="color: rgb(0, 0, 0);" /><span style="color: rgb(0, 0, 0); font-weight: bold;">Time</span><span style="color: rgb(0, 0, 0);">: [Group_Meeting_Time] </span><br style="color: rgb(0, 0, 0);" /><span style="color: rgb(0, 0, 0); font-weight: bold;">Frequency</span><span style="color: rgb(0, 0, 0);">: [Group_Meeting_Frequency] </span><br style="color: rgb(0, 0, 0);" /><span style="color: rgb(0, 0, 0); font-weight: bold;">Location: </span>[Group_Meeting_Location] </span><br /></div></div></div></blockquote><div><div><div><br />If you want to reach out to [Nickname] directly, you can reach them at [Participant_Email].  </div><div><br />Thanks for growing your group!<br />Crossroads Spiritual Growth Team </div></div></div></div>';

IF NOT EXISTS(SELECT * FROM dp_Communications WHERE communication_id = 2029)
BEGIN
	SET IDENTITY_INSERT dp_Communications ON;  

	INSERT INTO dp_Communications(Communication_ID,Author_User_ID,Subject,Body,Domain_ID,Start_Date,Communication_Status_ID,From_Contact,Reply_to_Contact,Template,Active)
	       VALUES(@COMM_ID,5,@SUBJECT,@BODY,1,GETDATE(),1,7676252,7676252,1,1);

	SET IDENTITY_INSERT dp_Communications OFF; 
END

GO
