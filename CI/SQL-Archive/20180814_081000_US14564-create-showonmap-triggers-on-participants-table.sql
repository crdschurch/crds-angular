USE MinistryPlatform
GO

--triggers on Participants table to capture changes/additions to showonmap
CREATE OR ALTER TRIGGER tr_ShowOnMapTrigger_Insert on dbo.Participants
AFTER INSERT
AS BEGIN
		INSERT INTO dbo.cr_MapAudit
			(Participant_ID, ShowOnMap, Processed)
		SELECT
			i.Participant_ID,
			i.Show_On_Map,
			0
		FROM
			INSERTED i
		WHERE
			i.Show_On_Map = 1;
END
GO

CREATE OR ALTER TRIGGER tr_ShowOnMapTrigger_Update on dbo.Participants
AFTER UPDATE
AS BEGIN
	IF UPDATE(Show_On_Map)
	BEGIN
		INSERT INTO dbo.cr_MapAudit
			(Participant_ID, ShowOnMap, Processed )
		SELECT
			i.Participant_ID,
			i.Show_On_Map,
			0
		FROM
			INSERTED i
	END
END
GO
