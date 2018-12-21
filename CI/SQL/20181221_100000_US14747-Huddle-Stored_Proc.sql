USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[crds_Huddle_Participant_Status_Refresh]
AS
BEGIN

DECLARE @Huddlers TABLE
(
  huddleID int, 
  huddleStart date,
  huddleEnd date,
  participantID int,
  participantStart date,
  participantEnd date,
  huddlerole int, -- leader=22
  currentHuddleStatusId int,
  newHuddleStatusId int
)

DECLARE @HuddleGroupTypeID INTEGER = 16;

-- 2 Never Finished Huddle
-- 3 Completed/Not interested
-- 4 Completed/Hasn't Led
-- 5 Huddle Leader
-- 6 Has Led/Not Currently
-- 7 QTR 1
-- 8 QTR 2
-- 9 QTR 3
-- 10 QTR 4
-- 11 TBD

-- First let's end date any huddle groups that are older than 1 year
UPDATE Groups SET End_Date = GETDATE() WHERE Group_Type_ID = @HuddleGroupTypeID AND End_Date IS NULL AND Start_Date < DATEADD(year, -1, GETDATE());

--select * from group_types
-- huddle = 16
INSERT INTO @Huddlers
select G.Group_ID, 
       G.Start_Date, 
	   G.End_Date, 
	   GP.Participant_ID, 
	   GP.Start_Date, 
	   GP.End_Date,  
	   GP.Group_Role_ID, 
	   P.Huddle_Status_ID,
	   0 
from groups G
JOIN group_participants GP on GP.group_id = G.group_id
JOIN participants P ON P.Participant_ID = GP.Participant_ID
where group_type_id = @HuddleGroupTypeID

-- QTR 1
UPDATE @Huddlers SET newHuddleStatusId = 7 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(month, 3, huddleStart)) AND
								(newHuddleStatusId = 0) AND (currentHuddleStatusId != 5)

-- QTR 2
UPDATE @Huddlers SET newHuddleStatusId = 8 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(month, 6, huddleStart)) AND
								(newHuddleStatusId = 0) AND (currentHuddleStatusId != 5)

-- QTR 3
UPDATE @Huddlers SET newHuddleStatusId = 9 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(month, 9, huddleStart)) AND
								(newHuddleStatusId = 0) AND (currentHuddleStatusId != 5)

-- QTR 4
UPDATE @Huddlers SET newHuddleStatusId = 10 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(year, 1, huddleStart)) AND
								(newHuddleStatusId = 0) AND (currentHuddleStatusId != 5)

-- Completed/Hasn't Led
UPDATE @Huddlers SET newHuddleStatusId = 4 WHERE 
                               (participantEnd > DATEADD(year, 1, huddleStart)) AND
							   (newHuddleStatusId = 0) AND (currentHuddleStatusId = 10)

--Huddle Leader
UPDATE @Huddlers SET newHuddleStatusId = 5 WHERE 
							   (huddleEnd is null) AND
                               (huddlerole = 22 ) and
							   (newHuddleStatusId = 0)

-- Has Led/Not Currently
UPDATE @Huddlers SET newHuddleStatusId = 6 WHERE 
							   (participantEnd > DATEADD(year, 1, huddleStart)) AND
                               (currentHuddleStatusId = 5) AND
							   (newHuddleStatusId = 0)

-- 2, 3 and 11 are full manual.


-- UPDATE the participant records with the 

UPDATE P
SET P.Huddle_Status_ID = H.newHuddleStatusId
FROM Participants P
JOIN @Huddlers H ON H.participantID = P.Participant_ID
WHERE newHuddleStatusId != 0

END

GO


