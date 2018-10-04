USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Horner
-- Create date: 8/1/17
-- Description:	Get all the trip events for the Trip Waiver Report
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.report_filter_trip_events'))
	EXEC('CREATE PROCEDURE [dbo].[report_filter_trip_events] AS BEGIN SET NOCOUNT ON; END')
GO

ALTER PROCEDURE [dbo].[report_filter_trip_events]
	@DomainIdIn varchar(40)
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @DomainId int = (SELECT Domain_ID FROM dp_Domains WHERE CAST(Domain_GUID AS varchar(40)) = @DomainIdIn)
	DECLARE @TripEventType int = ISNULL((SELECT TOP 1 [Value] FROM dp_Configuration_Settings WHERE [Key_Name] = 'TripEventType' AND Application_Code = 'CRDS-COMMON' AND Domain_ID = @DomainId), 6)

	SELECT Event_ID, Event_Title
	FROM [Events]
	WHERE Event_Type_ID = @TripEventType
		AND NOT Cancelled = 1
		AND NOT (Event_Start_Date < GETDATE() OR Event_End_Date < GETDATE())
		AND Domain_ID = @DomainId
	ORDER BY Event_Title
END
GO
