USE [MinistryPlatform]
GO

/****** Object:  Table [dbo].[Contacts]    Script Date: 7/5/2018 11:14:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

ALTER TABLE dbo.Contacts
ADD Date_Of_Death date null;
GO