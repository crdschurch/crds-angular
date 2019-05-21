USE [MinistryPlatform]
GO


UPDATE Group_Participants
SET End_Date = g.End_Date
--select gp.Group_Participant_ID, g.Group_ID, gp.End_Date as 'Participant End', g.End_Date as 'Group End'
FROM Group_Participants gp
JOIN Groups g on gp.Group_ID = g.Group_ID
WHERE 
gp.End_Date IS NULL
AND g.End_Date IS NOT NULL
--AND g.End_Date < GETDATE() --Uncomment to filter out future end dated groups