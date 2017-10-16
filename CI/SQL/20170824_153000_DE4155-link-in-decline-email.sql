USE MinistryPlatform

IF (EXISTS(SELECT * FROM dp_Communications WHERE Communication_ID = 2028))
BEGIN

	UPDATE dp_Communications
		SET Body = '<blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div style="font-family: arial, sans-serif; font-size: 12px;">Hi [Nickname], <br /><br />Thanks for reaching out to try the [Group_Name] group. Unfortunately the group isn''t adding new members at this time. How about trying a different group? You can <a href="[SearchURL]">search for a new one</a>. And if you don''t see one that quite fits your needs, you can always <a href="[StartURL]">start your own group</a>!<br /><br />Thanks again, <br />Crossroads Spiritual Growth Team <br /></div></blockquote>'
		WHERE Communication_ID = 2028;

END



GO
