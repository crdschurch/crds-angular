USE MinistryPlatform

DECLARE @COMMID INTEGER = 2011;

IF EXISTS(SELECT * FROM dp_Communications WHERE Communication_ID = @COMMID)
BEGIN
	UPDATE dp_Communications SET body = 'Hi [Recipient_Name],<br /><br />Somebody wants you to join them!  [Leader_Name] from [City], [State] would like you to join their gathering.  Here''s a description of what''s happening:<br />[Description]<div><br /><a href="[YesURL]" target="_self">Accept the invite</a> or <a href="[NoURL]" target="_self">decline the invite.</a></div>' WHERE Communication_ID = @COMMID
END

GO

