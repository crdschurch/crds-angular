USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[crds_StartedASmallGroup](@ParticipantId INT)
RETURNS NVARCHAR(100)
AS
BEGIN

	DECLARE @JourneyStart AS DATETIME;
	DECLARE @JourneyEnd AS DATETIME;
	DECLARE @GroupsCreatedCount AS INT;
	DECLARE @Answer AS NVARCHAR(100);

	SELECT TOP 1 @JourneyStart=Start_Date,@JourneyEnd=End_Date FROM attributes WHERE attribute_category_id = 51 ORDER BY start_date DESC
   
	SET @GroupsCreatedCount = (SELECT count(*)
		FROM GROUPS G
		JOIN Group_Participants GP ON G.Group_ID = GP.Group_ID
		JOIN Participants P ON P.Participant_Id  = GP.Participant_Id
		WHERE P.Participant_ID = @ParticipantId
		AND P.Contact_id = G.Primary_Contact
		AND G.Start_Date BETWEEN @JourneyStart AND @JourneyEnd)

	IF @GroupsCreatedCount > 0
       SET @Answer = 'YES';
	ELSE 
       SET @Answer = 'NO';

	RETURN @Answer;
END
GO


