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
	SET @MentorName = (select top 1 C.Display_Name
			FROM cr_Coaches COACH
			LEFT JOIN cr_Mentors M ON M.Coach_Contact_ID = COACH.Coach_Contact_ID
			LEFT JOIN Contacts C ON C.Contact_ID = M.Mentor_Contact_ID
			WHERE COACH.Leader_Contact_ID  = @CONTACTID
			AND GetDate() > COACH.Start_Date 
			AND (COACH.End_Date IS NULL OR COACH.End_Date > GetDate())
			AND GetDate() > M.Start_Date 
			AND (M.End_Date IS NULL OR M.End_Date > GetDate()))

	RETURN @MentorName;
END

GO
