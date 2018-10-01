USE [MinistryPlatform]
GO

DECLARE @pendingGroupRequestTemplateId int = 2004;

UPDATE [dbo].dp_Communications SET
Subject='Action Required - Pending request(s) to try your group: [Group_Name]',
Body='<div>Hi [Primary_First_Name]!</div><div><br /></div><div>[Nickname] [Last_Name] ([Email_Address]) is interested in your [Group_Name] group. </div><div><br /></div><div>Let [Nickname] know whether you want them to try out [Group_Name] by <a href="https://[Base_URL]/groups/search/small-group/[Group_ID]/my" target="_blank">going to its listing on the Crossroads group tool.</a></div><div><br /></div><div>If for some reason the above link doesn''t work, you can: </div><div><ul><li>Go to https://[Base_URL]/groups/search/my</li><li>Login (if not already logged in)</li><li>Click Accept or Deny within your [Group_Name] detail.</li></ul></div><div><br /></div><div>Thanks for being a leader!</div><div>Crossroads Groups Team</div>',
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