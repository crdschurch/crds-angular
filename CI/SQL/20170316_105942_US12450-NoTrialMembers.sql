USE MinistryPlatform
GO

UPDATE [dbo].Group_Participants SET Group_Role_ID = 16 WHERE Group_Role_ID = 67;

DELETE from [dbo].Group_roles where group_role_ID = 67;
GO