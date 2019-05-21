USE MINISTRYPLATFORM
GO

IF EXISTS (SELECT * FROM [dbo].[dp_Communications] WHERE [template] = 1 AND [communication_id] = 2010)
BEGIN
	UPDATE [dbo].[dp_Communications]
	SET [body] = '<p>Hi [NickName],</p><p>Rats. Your request to join [Leader_Name]''s gathering wasn''t accepted. No worries, there''s still a ton of ways you can connect with other Crossroads people. Here''s some things to consider:<ul><li>Sign up to host</li><li>Look for a different host</li><li>Say ''Hi'' to other community members in your area</li></ul>If you have any questions, we''re here to help! Email anywhere@crossroads.net and we''ll get back to you in a jiffy.</p>'
	WHERE [communication_id] = 2010
END
