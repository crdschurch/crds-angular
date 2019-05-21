USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[cr_MapAudit]
  ADD ProcessStatus NVARCHAR(50);

GO


