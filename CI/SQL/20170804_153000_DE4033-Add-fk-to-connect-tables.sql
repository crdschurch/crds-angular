USE [MinistryPlatform]


IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_cr_Connect_Communications_Status_dp_Domains')
   AND parent_object_id = OBJECT_ID(N'dbo.cr_Connect_Communications_Status'))
BEGIN
	ALTER TABLE [dbo].[cr_Connect_Communications_Status]  WITH CHECK ADD  CONSTRAINT [FK_cr_Connect_Communications_Status_dp_Domains] FOREIGN KEY([Domain_ID])
	REFERENCES [dbo].[dp_Domains] ([Domain_ID])

	ALTER TABLE [dbo].[cr_Connect_Communications_Status] CHECK CONSTRAINT [FK_cr_Connect_Communications_Status_dp_Domains]
END
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_cr_Connect_Communications_Type_dp_Domains')
   AND parent_object_id = OBJECT_ID(N'dbo.cr_Connect_Communications_Type'))
BEGIN
	ALTER TABLE [dbo].[cr_Connect_Communications_Type]  WITH CHECK ADD  CONSTRAINT [FK_cr_Connect_Communications_Type_dp_Domains] FOREIGN KEY([Domain_ID])
	REFERENCES [dbo].[dp_Domains] ([Domain_ID])

	ALTER TABLE [dbo].[cr_Connect_Communications_Type] CHECK CONSTRAINT [FK_cr_Connect_Communications_Type_dp_Domains]
END
GO
