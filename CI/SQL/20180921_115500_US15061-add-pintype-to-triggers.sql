USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER trigger [dbo].[tr_ShowOnMapTrigger_Update] on [dbo].[Participants]
after update
as begin
	IF UPDATE(Show_On_Map)
	BEGIN
		INSERT INTO dbo.cr_MapAudit
			(Participant_ID, ShowOnMap, Processed, PinType )
		SELECT
			i.Participant_ID,
			i.Show_On_Map,
			0,
			'1'
		FROM
			INSERTED i
	END
end
GO

CREATE OR ALTER trigger [dbo].[tr_ShowOnMapTrigger_Insert] on [dbo].[Participants]
after insert
as begin
		INSERT INTO dbo.cr_MapAudit
			(Participant_ID, ShowOnMap, Processed, PinType )
		SELECT
			i.Participant_ID,
			i.Show_On_Map,
			0,
			'1'
		FROM
			INSERTED i
		WHERE
			i.Show_On_Map = 1;
end
GO