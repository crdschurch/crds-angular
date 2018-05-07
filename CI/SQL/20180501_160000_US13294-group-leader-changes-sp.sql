USE MinistryPlatform

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_GetGroupLeaderEmailUpdates]
	@FROMDATE DATE,
	@TODATE DATE
AS
BEGIN

DECLARE @FromSelection TABLE(Firstname VARCHAR(64), Lastname VARCHAR(64), Email VARCHAR(64))
DECLARE @ToSelection TABLE(Firstname VARCHAR(64), Lastname VARCHAR(64), Email VARCHAR(64))

INSERT INTO @FromSelection
	select distinct C.First_Name, C.Last_Name, C.Email_Address from groups G
	join group_participants GP on GP.group_id = G.group_id
	join group_roles GR on GR.Group_Role_ID = GP.Group_Role_ID
	join participants P on P.Participant_ID = GP.Participant_ID
	join Contacts C on C.contact_id = P.Contact_ID
	where GR.Group_Role_ID = 22
	AND ((G.End_Date is null AND CAST(G.Start_Date AS DATE) <= @FROMDATE) OR (@FROMDATE between CAST(G.Start_Date AS DATE) and CAST(G.End_Date AS DATE)))
	AND ((GP.End_Date is null AND CAST(GP.Start_Date AS DATE) <= @FROMDATE) OR (@FROMDATE between CAST(GP.Start_Date AS DATE) and CAST(GP.End_Date AS DATE)))
	AND G.Group_Type_ID = 1


INSERT INTO @ToSelection
	select distinct C.First_Name, C.Last_Name, C.Email_Address from groups G
	join group_participants GP on GP.group_id = G.group_id
	join group_roles GR on GR.Group_Role_ID = GP.Group_Role_ID
	join participants P on P.Participant_ID = GP.Participant_ID
	join Contacts C on C.contact_id = P.Contact_ID
	where GR.Group_Role_ID = 22
	AND ((G.End_Date is null AND CAST(G.Start_Date AS DATE) <= @TODATE) OR (@TODATE between CAST(G.Start_Date AS DATE) and CAST(G.End_Date AS DATE)))
	AND ((GP.End_Date is null AND CAST(GP.Start_Date AS DATE) <= @TODATE) OR (@TODATE between CAST(GP.Start_Date AS DATE) and CAST(GP.End_Date AS DATE)))
	AND G.Group_Type_ID = 1

--EXCEPT
--Returns any distinct values from the query to the left of the EXCEPT operator that are not also returned from the right query.

--Removed - No longer leaders 
select Firstname, LastName, Email, 'Removed' As Action from @FromSelection
EXCEPT
select Firstname, LastName, Email, 'Removed' As Action from @ToSelection
	UNION
--Added -- became leaders
select Firstname, LastName, Email, 'Added' As Action from @ToSelection
EXCEPT
select Firstname, LastName, Email, 'Added' As Action from @FromSelection

END
GO
