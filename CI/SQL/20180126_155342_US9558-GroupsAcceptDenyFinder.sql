USE [MinistryPlatform]
GO

UPDATE [dbo].dp_Communications SET 
Body='[Nickname],<p><span style="font-family: Arial; font-size: 13px; white-space: pre-wrap; background-color: rgb(255, 255, 255);">You''re in! </span><font face="Arial"><span style="font-size: 13px; white-space: pre-wrap;">[Leader_Name]</span></font><span style="background-color: rgb(255, 255, 255); font-family: Arial; font-size: 13px; white-space: pre-wrap;"> has approved your request to join their gathering in [City], [State].   </span></p><p><span style="font-family: Arial; font-size: 13px; white-space: pre-wrap; background-color: rgb(255, 255, 255);">If you haven''t yet, reach out to </span><font face="Arial"><span style="font-size: 13px; white-space: pre-wrap;">[Leader_Name]</span></font><span style="background-color: rgb(255, 255, 255); font-family: Arial; font-size: 13px; white-space: pre-wrap;"> and find out when they are hosting service. For bonus points, tell them your favorite, non-email way to connect with people (text, Facebook Messenger, video chat), ''Cause let''s face it, no one likes email.</span></p>'
WHERE Communication_ID = 2009;

UPDATE [dbo].dp_Communications SET 
Body='<p>Hi [Nickname],</p><p>Rats. Your request to join [Leader_Name]''s gathering wasn''t accepted. No worries, there''s still a ton of ways you can connect with other Crossroads people. Here''s some things to consider:</p><ul><li>Sign up to host</li><li>Look for a different host</li><li>Say ''Hi'' to other community members in your area</li></ul>If you have any questions, we''re here to help! Email anywhere@crossroads.net and we''ll get back to you in a jiffy.'
WHERE Communication_ID = 2010;
GO