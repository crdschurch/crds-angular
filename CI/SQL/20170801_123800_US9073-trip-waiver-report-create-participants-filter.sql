USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Horner
-- Create date: 8/1/17
-- Description:	Get all the trip event participants for the Trip Waiver Report
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.report_filter_trip_participants'))
	EXEC('CREATE PROCEDURE [dbo].[report_filter_trip_participants] AS BEGIN SET NOCOUNT ON; END')
GO

ALTER PROCEDURE [dbo].[report_filter_trip_participants]
	@DomainIdIn varchar(40)
	, @EventIdIn int
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @DomainId int = (SELECT Domain_ID FROM dp_Domains WHERE CAST(Domain_GUID AS varchar(40)) = @DomainIdIn)

	SELECT 0 AS Participant_ID, '<All Participants>' AS Display_Name
	UNION
	SELECT p.Participant_ID
		, c.Display_Name
	FROM Event_Participants ep
		, Participants p
		, Contacts c
	WHERE ep.Event_ID = @EventIdIn
		AND p.Participant_ID = ep.Participant_ID
		AND ep.Participation_Status_ID = 2 -- Registered
		AND c.Contact_ID = p.Contact_ID
		AND p.Domain_ID = @DomainId
	ORDER BY Display_Name
END