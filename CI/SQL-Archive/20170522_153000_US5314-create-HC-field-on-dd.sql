USE [MinistryPlatform]
GO

IF NOT EXISTS (
  SELECT 1 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Donation_Distributions]')
         AND name = 'HC_Donor_Congregation_ID')
	ALTER TABLE [dbo].[Donation_Distributions] ADD HC_Donor_Congregation_ID INT
ELSE
	ALTER TABLE [dbo].[Donation_Distributions] DROP CONSTRAINT [FK_Donation_Distributions_HC_Congregations]

GO

ALTER TABLE [dbo].[Donation_Distributions]  WITH CHECK ADD  CONSTRAINT [FK_Donation_Distributions_HC_Congregations] FOREIGN KEY([HC_Donor_Congregation_ID])
REFERENCES [dbo].[Congregations] ([Congregation_ID])
GO

ALTER TABLE [dbo].[Donation_Distributions] CHECK CONSTRAINT [FK_Donation_Distributions_HC_Congregations]
GO



