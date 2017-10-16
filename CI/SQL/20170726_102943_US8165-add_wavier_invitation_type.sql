USE [MinistryPlatform]
GO

DECLARE @WaiverInvitationTypeId int = 4;

IF NOT EXISTS (SELECT 1 FROM [dbo].[cr_Invitation_Types] WHERE [Invitation_Type_ID] = @WaiverInvitationTypeId)
BEGIN
	SET IDENTITY_INSERT [dbo].[cr_Invitation_Types] ON;
	INSERT INTO [dbo].[cr_Invitation_Types]
			   ([Invitation_Type_ID]
			   ,[Invitation_Type]
			   ,[Description]
			   ,[Domain_ID])
		 VALUES
			   (@WaiverInvitationTypeId
			   ,'Waivers'
			   ,'Invitation to sign a waiver'
			   ,1)
	SET IDENTITY_INSERT [dbo].[cr_Invitation_Types] OFF;
END