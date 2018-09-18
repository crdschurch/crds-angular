USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[crds_GroupCount](@ParticipantId INT)
RETURNS INT
AS
BEGIN
	DECLARE @GroupCount AS INT;
	SET @GroupCount = 	(SELECT count(*) FROM group_participants WHERE (end_date IS NULL OR end_date > GETDATE()) AND participant_id = @ParticipantId);
		
	RETURN @GroupCount;
END
GO


