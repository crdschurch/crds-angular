USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Group_Participants_By_Date_Congregation]    Script Date: 4/23/2018 8:35:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[report_CRDS_Group_Participants_By_Date_Congregation] 
     @reportdate DATE
AS

BEGIN
SET nocount ON;

DECLARE @grouptype INT = 1

select Count(Group_ID) as Active_Groups, Sum(Participant_Count) as Participant_Count, Congregation_Name from
(
select G.Group_ID, count(GP.Participant_ID) As Participant_Count, C.Congregation_Name from Group_Participants GP 
	JOIN GROUPS G ON G.Group_ID = GP.Group_ID
	JOIN Congregations C ON C.Congregation_ID = G.Congregation_ID
    where
	((@reportdate >= GP.Start_Date) AND
	(GP.End_Date is null OR @reportdate <= GP.End_Date))
	AND
	((@reportdate >= G.Start_Date) AND
	(G.End_Date is null OR @reportdate <= G.End_Date))
	AND
	G.Group_Type_ID = @grouptype
	GROUP BY G.Group_ID, C.Congregation_Name
) a
group by Congregation_Name
order by Congregation_Name
END

GO


