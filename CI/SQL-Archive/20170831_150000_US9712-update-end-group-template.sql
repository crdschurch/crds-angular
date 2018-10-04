USE MinistryPlatform

DECLARE @COMID INTEGER = 290789;

IF EXISTS(SELECT * FROM dp_Communications WHERE communication_id = @COMID)
BEGIN
	UPDATE dp_Communications SET
	  Subject = 'Group [Group_Name] has ended',
	  Body = 'Hi [Participant_Name],<br /><br />The [Group_Name] has ended and is no longer active. We hope it was a great experience and encourage you to <a href="[Group_Tool_Url]" target="_self">search for other group options</a>.<br /><br />Thanks!<br />Crossroads Spiritual Growth Team'
	WHERE communication_id = @COMID
END

GO
