USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Horner
-- Create date: 8/1/17
-- Description:	Script location of Trip Waiver Report in MP
-- =============================================

DECLARE @ReportId INT = 325			-- From Identity
DECLARE @TripPledgeCampaignPageId INT = 514
DECLARE @TripReportsRoleId INT = 108

IF NOT EXISTS (SELECT 1 FROM dp_Reports WHERE Report_ID = @ReportId)
BEGIN
	SET IDENTITY_INSERT dp_Reports ON;

	INSERT INTO dp_Reports
	(
		Report_ID
		, Report_Name
		, [Description]
		, Report_Path
	)
	VALUES
	(
		@ReportId
		, 'Trip Waiver'
		, 'Gets the waivers for participants on a trip'
		, '/MPReports/Crossroads/CRDS Trip Waiver'
	)

	SET IDENTITY_INSERT dp_Reports OFF;
END

IF NOT EXISTS (SELECT 1 FROM dp_Report_Pages WHERE Report_ID = @ReportId AND Page_ID = @TripPledgeCampaignPageId)
BEGIN
	INSERT INTO dp_Report_Pages
	(
		Report_ID
		, Page_ID
	)
	VALUES
	(
		@ReportId
		, @TripPledgeCampaignPageId
	)
END

IF NOT EXISTS (SELECT 1 FROM dp_Role_Reports WHERE Role_ID = @TripReportsRoleId AND Report_ID = @ReportId)
BEGIN
	INSERT INTO dp_Role_Reports
	(
		Role_ID
		, Report_ID
		, Domain_ID
	)
	VALUES
	(
		@TripReportsRoleId
		, @ReportId
		, 1
	)
END