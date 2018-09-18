USE [MinistryPlatform]
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[[crds_GetGroupParticipantCount]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[[crds_GetGroupParticipantCount]

GO 


CREATE FUNCTION [dbo].[crds_GetGroupParticipantCount](@GroupId INT)
RETURNS INT
AS
BEGIN
	DECLARE @ParticipantCount AS INT;
	SET @ParticipantCount = 	(SELECT count(*) FROM group_participants WHERE (end_date IS NULL OR end_date > GETDATE()) AND group_id = @GroupId);
		
	RETURN @ParticipantCount;
END

GO

