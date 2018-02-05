USE [MinistryPlatform]
GO

DECLARE @pendingGroupRequestTemplateId int = 2004;

UPDATE [dbo].dp_Communications SET
Subject='Action Required - Pending request(s) to try your group: [Group_Name]',
Body='<div>Hi [Nick_Name] [Last_Name],</div><div><br /></div><div>You have [Pending_Requests_Count] pending request(s) from people who want to join your "[Group_Name]" group:<br /></div><div><br /></div><div>[Pending_Requests]</div><div><br /></div><div>Click on the link below to open your group and from there you can accept or deny them. <br /></div><div><br /></div><div>https://[BaseUrl]/groups/search/small-group/[Group_ID]/my</div><div><br /></div><div>If your group is no longer meeting click on the following link to confirm the group has ended (And you will also stop getting these emails!)</div><div><br /></div><div><div>https://[BaseUrl]/groups/search/end-group/[Group_ID]</div></div><div><br /></div><div>Thanks!</div><div>Crossroads Groups Team</div><div><br /></div><div><i>This email was sent to: [All_Leaders]</i></div>',
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