USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[crds_OnSiteAndOffSite](@ParticipantId INT)
RETURNS NVARCHAR(100)
AS
BEGIN
	DECLARE @OffsiteSmallGroupCount AS INT;
	DECLARE @OnsiteGroupCount AS INT;
	DECLARE @Answer AS NVARCHAR(100);

	SET @OffsiteSmallGroupCount = (SELECT COUNT(*) 
		FROM group_participants GP
		JOIN GROUPS G ON G.Group_ID = GP.Group_ID
		WHERE (GP.end_date IS NULL OR GP.end_date > GETDATE()) AND GP.participant_id = @ParticipantId
		AND G.Group_Type_ID = 1); -- small group

    SET @OnsiteGroupCount = (select COUNT(*) 
		FROM group_participants GP
		JOIN GROUPS G ON G.Group_ID = GP.Group_ID
		WHERE (GP.end_date IS NULL OR GP.end_date > GETDATE()) AND GP.participant_id = @ParticipantId
		AND G.Group_Type_ID = 8); -- onsite group

	IF @OffsiteSmallGroupCount > 0 AND @OnsiteGroupCount > 0
       SET @Answer = 'YES';
	ELSE 
       SET @Answer = 'NO';

	RETURN @Answer;
END
GO


