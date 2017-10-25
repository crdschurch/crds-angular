USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[crds_GetMentor]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[crds_GetMentor]
GO 


CREATE FUNCTION [dbo].[crds_GetMentor](@ContactID INT)
RETURNS NVARCHAR(MAX)
AS
BEGIN

	DECLARE @MentorName AS NVARCHAR(MAX);
	SET @MentorName = (select C.Display_Name
			from cr_Coaches COACH
			left join cr_Mentors M ON M.Coach_Contact_ID = COACH.Coach_Contact_ID
			left join Contacts C ON C.Contact_ID = M.Mentor_Contact_ID
			where COACH.Leader_Contact_ID  = @CONTACTID)

	RETURN @MentorName;
END

GO
