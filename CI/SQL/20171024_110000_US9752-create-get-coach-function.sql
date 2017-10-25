USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[crds_GetCoach]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[crds_GetCoach]
GO 


CREATE FUNCTION [dbo].[crds_GetCoach](@ContactID INT)
RETURNS NVARCHAR(MAX)
AS
BEGIN

	DECLARE @CoachName AS NVARCHAR(MAX);
	SET @CoachName = (select C.Display_Name
			from cr_Coaches COACH
			left join Contacts C ON C.Contact_ID = COACH.Coach_Contact_ID
			where COACH.Leader_Contact_ID  = @CONTACTID)

	RETURN @CoachName;
END

GO
