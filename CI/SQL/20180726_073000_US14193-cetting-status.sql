USE MinistryPlatform
GO

IF NOT EXISTS(SELECT * FROM cr_group_leader_statuses where Group_Leader_Status = 'Vetting') 
BEGIN
	SET IDENTITY_INSERT cr_Group_Leader_Statuses ON;
	INSERT INTO cr_Group_Leader_Statuses(Group_Leader_Status_ID,Group_Leader_Status, Sort_Order) VALUES(7,'Vetting', 250);
    SET IDENTITY_INSERT cr_Group_Leader_Statuses OFF;
	UPDATE cr_Group_Leader_Statuses SET Sort_Order = 100 WHERE Group_Leader_Status_ID = 2;
	UPDATE cr_Group_Leader_Statuses SET Sort_Order = 200 WHERE Group_Leader_Status_ID = 3;
	UPDATE cr_Group_Leader_Statuses SET Sort_Order = 300 WHERE Group_Leader_Status_ID = 4;
	UPDATE cr_Group_Leader_Statuses SET Sort_Order = 400 WHERE Group_Leader_Status_ID = 5;
	UPDATE cr_Group_Leader_Statuses SET Sort_Order = 500 WHERE Group_Leader_Status_ID = 6;
END
GO
