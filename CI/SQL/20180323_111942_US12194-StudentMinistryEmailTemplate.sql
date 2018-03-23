USE [MinistryPlatform]
GO

DECLARE @emailTemplate INTEGER = 280854;

UPDATE [dbo].dp_Communications 
SET Body = 'The following group leaders are posting a group for students Grades (either 6-8, or 9-12) in the group tool<br />Please check to ensure this person is known and approved by SM <br />If not, please proceed with your normal process for approval/background check with this person.  <br />If you do not know them, ask them to set their group to "private" until you have approved them.<br /><br />[Leaders]<ul>Group: [GroupName]</ul><ul>Group Description: [GroupDescription]</ul>'
WHERE Communication_ID = @emailTemplate;
GO
