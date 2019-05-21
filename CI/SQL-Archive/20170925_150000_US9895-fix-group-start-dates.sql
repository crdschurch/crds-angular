USE MinistryPlatform

DECLARE @SMALLGROUPTYPEID INTEGER = 1;
DECLARE @GROUPSTOUPDATE TABLE ( GroupID int, StartDate datetime, AuditLogStartDate datetime);
DECLARE @GROUPPARTICIPANTSTOUPDATE TABLE ( GroupParticipantID int, StartDate datetime, AuditLogStartDate datetime);
DECLARE @BOUNDRYDATE DATETIME = Cast('9/6/2017' as datetime);

INSERT INTO @GROUPSTOUPDATE(GroupID, StartDate, AuditLogStartDate)
SELECT G.Group_ID, G.Start_Date, min(AD.Previous_Value)
 FROM Groups G 
 INNER JOIN dp_Audit_Log AL ON AL.Record_ID = G.Group_ID
 INNER JOIN dp_Audit_Detail AD ON AD.Audit_Item_ID = AL.Audit_Item_ID
 WHERE G.Start_Date > @BOUNDRYDATE AND G.Group_Type_ID = @SMALLGROUPTYPEID AND AL.Table_Name = 'Groups' AND AD.Field_Name = 'Start_Date'
 GROUP BY G.Group_ID, G.Start_Date
SELECT count(*) FROM @GROUPSTOUPDATE


INSERT INTO @GROUPPARTICIPANTSTOUPDATE(GroupParticipantID, StartDate, AuditLogStartDate)
SELECT GP.Group_Participant_ID, GP.Start_Date , min(AD.Previous_Value)
  FROM Group_Participants GP
  INNER JOIN @GROUPSTOUPDATE GTU ON GTU.GroupID = GP.Group_ID
  INNER JOIN dp_Audit_Log AL ON AL.Record_ID = GP.Group_Participant_ID
  INNER JOIN dp_Audit_Detail AD ON AD.Audit_Item_ID = AL.Audit_Item_ID
  WHERE GP.Start_Date > @BOUNDRYDATE AND AL.Table_Name = 'Group_Participants' AND AD.Field_Name = 'Start_Date'
  GROUP BY GP.Group_Participant_ID, GP.Start_Date
SELECT count(*) FROM @GROUPPARTICIPANTSTOUPDATE

BEGIN TRAN
	UPDATE Groups 
	   SET Start_Date = G2.AuditLogStartDate
	   FROM Groups G
	   INNER JOIN @GROUPSTOUPDATE G2 ON G2.GroupID = G.Group_ID

	SELECT G.Group_ID, G.Start_Date, min(AD.Previous_Value)
	 FROM Groups G 
	 INNER JOIN dp_Audit_Log AL ON AL.Record_ID = G.Group_ID
	 INNER JOIN dp_Audit_Detail AD ON AD.Audit_Item_ID = AL.Audit_Item_ID
	 WHERE G.Start_Date > @BOUNDRYDATE AND G.Group_Type_ID = @SMALLGROUPTYPEID AND AL.Table_Name = 'Groups' AND AD.Field_Name = 'Start_Date'
	 GROUP BY G.Group_ID, G.Start_Date

	UPDATE Group_Participants
	   SET Start_Date = G2.AuditLogStartDate
	   FROM Group_Participants GP
	   INNER JOIN @GROUPPARTICIPANTSTOUPDATE G2 ON G2.GroupParticipantID = GP.Group_Participant_ID

	SELECT GP.Group_Participant_ID, GP.Start_Date , min(AD.Previous_Value)
	  FROM Group_Participants GP
	  INNER JOIN @GROUPSTOUPDATE GTU ON GTU.GroupID = GP.Group_ID
	  INNER JOIN dp_Audit_Log AL ON AL.Record_ID = GP.Group_Participant_ID
	  INNER JOIN dp_Audit_Detail AD ON AD.Audit_Item_ID = AL.Audit_Item_ID
	  WHERE GP.Start_Date > @BOUNDRYDATE AND AL.Table_Name = 'Group_Participants' AND AD.Field_Name = 'Start_Date'
	  GROUP BY GP.Group_Participant_ID, GP.Start_Date
ROLLBACK TRAN