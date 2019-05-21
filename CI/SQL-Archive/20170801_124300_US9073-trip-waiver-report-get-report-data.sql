USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Horner
-- Create date: 8/1/17
-- Description:	Get all the data for the Trip Waiver Report
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.report_trip_waiver'))
	EXEC('CREATE PROCEDURE [dbo].[report_trip_waiver] AS BEGIN SET NOCOUNT ON; END')
GO

ALTER PROCEDURE [dbo].[report_trip_waiver]
	@DomainIdIn varchar(40)
	, @EventIdIn int
	, @ParticipantIdIn int
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @DomainId int = (SELECT Domain_ID FROM dp_Domains WHERE CAST(Domain_GUID AS varchar(40)) = @DomainIdIn)

	SELECT p.Participant_ID
		, c.Display_Name
		, c.Mobile_Phone
		, a.[Address_Line_1]
		, a.[Address_Line_2]
		, a.[City]
		, a.[State/Region]
		, a.[Postal_Code]
		, e.Event_Title
		, w.Waiver_Name
		, w.Waiver_Text
		, cw.Display_Name as Signee_Name
		, pw.Waiver_Create_Date
	FROM cr_Event_Participant_Waivers pw
		JOIN Event_Participants ep ON pw.Event_Participant_ID = ep.Event_Participant_ID
		JOIN Participants p ON ep.Participant_ID = p.Participant_ID
		JOIN Contacts c ON p.Contact_ID = c.Contact_ID
		JOIN [Events] e ON ep.Event_ID = e.Event_ID
		JOIN cr_Waivers w ON pw.Waiver_ID = w.Waiver_ID
		LEFT JOIN Contacts cw ON pw.Signee_Contact_ID = cw.Contact_ID
		JOIN Households h ON h.Household_ID = c.Household_ID
		JOIN Addresses a ON a.Address_ID = h.Address_ID
	WHERE p.Domain_ID = @DomainId
		AND pw.Accepted = 1
		AND ep.Event_ID = @EventIdIn
		AND ep.Participation_Status_ID = 2 -- Registered
		AND (@ParticipantIdIn = 0 OR ep.Participant_ID = @ParticipantIdIn) -- 0 means <All Participants> was selected in the report
	ORDER BY Display_Name
END