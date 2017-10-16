use [ministryplatform]
go

WITH CTE AS(
   SELECT gp.participant_id as particpant_id, gp.group_id, gp.Group_Role_ID, g.group_name, gp.group_participant_id,
       RN = ROW_NUMBER()OVER(PARTITION BY gp.participant_id, gp.group_id, gp.Group_Role_ID ORDER BY gp.start_date)
   FROM group_Participants gp
   join groups g on g.group_id = gp.group_id
   where g.End_Date IS NULL and gp.end_date is null
)

UPDATE Group_Participants 
SET End_Date = GETDATE()
--select gp.participant_id as particpant_id, gp.group_id, gp.Group_Role_ID, c.group_name, gp.group_participant_id, c.RN
FROM Group_Participants gp
JOIN CTE c on c.group_participant_id = gp.group_participant_id
WHERE c.RN>1  --keep the earlier start date, delete the later one


--SELECT * FROM CTE WHERE RN > 1

