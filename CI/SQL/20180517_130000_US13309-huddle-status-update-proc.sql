USE MinistryPlatform
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
  huddleStatusId int
)

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

--select * from group_types
-- huddle = 31
INSERT INTO @Huddlers
select G.Group_ID, 
       G.Start_Date, 
	   G.End_Date, 
	   GP.Participant_ID, 
	   GP.Start_Date, 
	   GP.End_Date,  
	   GP.Group_Role_ID, 
	   0 
from groups G
join group_participants GP on GP.group_id = G.group_id
where group_type_id = 31

--Huddle Leader
UPDATE @Huddlers SET huddleStatusId = 5 WHERE 
                               (huddlerole = 22 and huddleEnd is null) and
							   (huddlestatusid = 0)

-- Has Led/Not Currently
UPDATE @Huddlers SET huddleStatusId = 6 WHERE 
                               (huddleEnd < GETDATE() AND huddleRole=22) AND
							   (huddlestatusid = 0)

-- Completed/Hasn't Led
UPDATE @Huddlers SET huddleStatusId = 4 WHERE 
                               (huddleEnd < GETDATE()) AND
							   (huddlestatusid = 0)

-- QTR 1
UPDATE @Huddlers SET huddleStatusId = 7 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(month, 3, huddleStart)) AND
								(huddlestatusid = 0)

-- QTR 2
UPDATE @Huddlers SET huddleStatusId = 8 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(month, 6, huddleStart)) AND
								(huddlestatusid = 0)

-- QTR 3
UPDATE @Huddlers SET huddleStatusId = 9 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(month, 9, huddleStart)) AND
								(huddlestatusid = 0)

-- QTR 4
UPDATE @Huddlers SET huddleStatusId = 10 WHERE 
								(huddleEnd is null) AND
								(GETDATE() < DATEADD(year, 1, huddleStart)) AND
								(huddlestatusid = 0)

--Never Finished
UPDATE @Huddlers SET huddleStatusId = 2 WHERE 
                               (participantEnd < huddleEnd) OR 
							   (huddleEnd is null AND participantEnd < GETDATE()) and 
							   huddlestatusid = 0;

-- TBD
UPDATE @Huddlers SET huddleStatusId = 11 WHERE huddlestatusid = 0;

-- UPDATE the participant records with the 

UPDATE P
SET P.Huddle_Status_ID = H.huddleStatusId
FROM Participants P
JOIN @Huddlers H ON H.participantID = P.Participant_ID

END
GO


