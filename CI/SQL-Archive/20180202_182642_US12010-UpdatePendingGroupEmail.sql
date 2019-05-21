USE [MinistryPlatform]
GO

DECLARE @pendingGroupRequestTemplateId int = 2004;

UPDATE [dbo].dp_Communications SET
Subject='Action Required - Pending request(s) to try your group: [Group_Name]',
Body='<div>Hi [Nick_Name] [Last_Name],</div><div><br /></div><div>You have [Pending_Requests_Count] pending request(s) from people who want to try your "[Group_Name]" group:<br /></div><div><br /></div><div>[Pending_Requests]</div><div><br /></div><div><a href="https://[BaseUrl]/groups/search/small-group/[Group_ID]/my" target="_blank">Click here to open your group and from there you can accept or deny them. </a><br /></div><div><br /></div><div><a href="https://[BaseUrl]/groups/search/end-group/[Group_ID]" target="_blank">Click here if your group is no longer meeting to confirm the group has ended.</a> (And you will also stop getting these emails!)</div><div><br /></div><div>If either of the above links did not work go to the following url: </div><div>https://[BaseUrl]/groups/search/small-group/[Group_ID]/my<br /></div><div><br /></div><div>Thanks!</div><div>Crossroads Groups Team</div><div><br /></div><div><i>This email was sent to: [All_Leaders]</i></div>',
From_Contact=7675411,
Reply_to_Contact=7675411
WHERE Communication_Id = @pendingGroupRequestTemplateId;
GO

DECLARE @tryAGroupTemplate int = 2026;

UPDATE [dbo].dp_Communications SET
From_Contact=7675411,
Reply_to_Contact=7675411
WHERE Communication_Id = @tryAGroupTemplate;
GO
