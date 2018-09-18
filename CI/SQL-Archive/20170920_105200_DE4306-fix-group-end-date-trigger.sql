USE [MinistryPlatform]
GO

-- Disable the existing Think Ministry trigger
IF OBJECTPROPERTY(OBJECT_ID('tr_End_Date_Group_Participant'), 'IsTrigger') = 1
	EXEC sp_executesql N'ALTER TABLE Groups DISABLE TRIGGER tr_End_Date_Group_Participant';
GO

-- Add a new trigger that accomplishes the same thing but avoids recursive calls
IF OBJECTPROPERTY(OBJECT_ID('crds_tr_End_Date_Group_Participant'), 'IsTrigger') = 1
	EXEC sp_executesql N'DROP TRIGGER crds_tr_End_Date_Group_Participant';
GO

CREATE TRIGGER [dbo].[crds_tr_End_Date_Group_Participant] 
   ON  [dbo].[Groups] 
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(End_Date) BEGIN
		DECLARE @change TABLE (Group_ID INT NOT NULL, End_Date DATETIME);

		-- list of groups whose end date has changed from a null end date to a non-null end date
		INSERT INTO @change (Group_ID, End_Date)
		SELECT DISTINCT I.Group_ID, I.End_Date
		FROM INSERTED I
		INNER JOIN DELETED D ON I.Group_ID = D.Group_ID
		WHERE I.End_Date IS NOT NULL AND D.End_Date IS NULL;

		IF EXISTS(SELECT 1 FROM @change) BEGIN
			DECLARE @GPUpdated TABLE (Group_Participant_ID INT, Old_End_Date DATETIME, New_End_Date DATETIME)
			DECLARE @AuditLogInserted TABLE (Audit_Item_ID INT, Record_ID INT)

			UPDATE Group_Participants 
			SET End_Date = (SELECT Top 1 End_Date FROM INSERTED WHERE INSERTED.Group_ID = Group_Participants.Group_ID)
			OUTPUT INSERTED.Group_Participant_ID ,DELETED.End_Date, INSERTED.End_Date
			INTO @GPUpdated 
			WHERE (End_Date > GetDate() OR End_Date IS NULL) AND Group_ID IN (SELECT Group_ID FROM @change);
			
			INSERT INTO dp_Audit_Log (Table_Name,Record_ID,Audit_Description,User_Name,User_ID,Date_Time)
			OUTPUT INSERTED.Audit_Item_ID, INSERTED.Record_ID
			INTO @AuditLogInserted   
			SELECT 'Group_Participants',g.Group_Participant_ID,'Updated','Svc Mngr',0,GETDATE() 
			FROM @GPUpdated g

			INSERT INTO dp_Audit_Detail (Audit_Item_ID, Field_Name, Field_Label, Previous_Value, New_Value, Previous_ID, New_ID)
			SELECT ALI.Audit_Item_ID,'End_Date','End Date',G.Old_End_Date,G.New_End_Date, NULL,NULL
			FROM @AuditLogInserted ALI
			INNER JOIN @GPUpdated G ON ALI.Record_ID = G.Group_Participant_ID
		END
	END
END
GO
