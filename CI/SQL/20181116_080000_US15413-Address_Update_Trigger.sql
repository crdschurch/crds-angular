USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[tr_Address_Update] ON [dbo].[Addresses]
AFTER UPDATE
AS BEGIN

	INSERT INTO dbo.cr_MapAudit
				(Participant_ID, ShowOnMap, Processed, PinType)
		SELECT P.Participant_ID,1,0,'1'
			FROM Participants P
			JOIN CONTACTS C ON C.Contact_id = P.Contact_id
			JOIN Households H ON H.Household_id = C.Household_ID
			JOIN Inserted I ON I.Address_ID = H.Address_ID
			WHERE P.Show_On_Map = 1
		
END
GO

ALTER TABLE [dbo].[Addresses] ENABLE TRIGGER [tr_Address_Update]
GO


