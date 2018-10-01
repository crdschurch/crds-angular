USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2018-04-11
-- Description:	Updates Inactive serve team report naming to remove
-- 90 day from description
-- =============================================
USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- id was generated using MP Identity Maintenance
DECLARE @ReportId INT = 326

IF EXISTS (SELECT * FROM [dbo].[dp_Reports] WHERE Report_ID = @ReportId AND Report_Name = '90 Day Inactive Serve Team Members')
BEGIN
UPDATE dp_Reports SET Report_Name = 'Inactive Serve Team Members', Description = 'Inactive Serve Team Members', Report_Path = '/Crossroads/CRDS Team Leader Inactive'
WHERE Report_ID = @ReportId AND Report_Name = '90 Day Inactive Serve Team Members'
END
GO
