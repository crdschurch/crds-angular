USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[Participants]
ADD Approved_Host BIT NOT NULL DEFAULT(0)

GO