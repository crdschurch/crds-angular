USE MinistryPlatform

DECLARE @TrialMemberId INT = 67;

IF NOT EXISTS (SELECT * FROM [dbo].[Group_Roles] WHERE GROUP_ROLE_ID = @TrialMemberId)
BEGIN
	SET IDENTITY_INSERT  [dbo].[Group_Roles] ON;

	INSERT INTO  [dbo].[Group_Roles](Group_Role_ID, Role_Title, Description, Group_Role_Type_ID, Group_Role_Intensity_ID, Domain_ID,Background_Check_Required)
	                         VALUES (@TrialMemberId, 'Trial Member', 'Trial Member', 3, 2, 1, 0);

	SET IDENTITY_INSERT  [dbo].[Group_Roles] OFF;
END

GO
