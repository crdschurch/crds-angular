USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[tr_Households_Update] ON [dbo].[Households]
AFTER UPDATE
AS BEGIN
  IF UPDATE(Congregation_ID)
   BEGIN
	INSERT INTO dbo.cr_MapAudit
				(Participant_ID, ShowOnMap, Processed, PinType)
		SELECT P.Participant_ID,1,0,'1'
			FROM Participants P
			JOIN CONTACTS C ON C.Contact_id = P.Contact_id
			JOIN Inserted I ON I.Household_id = C.Household_ID
			JOIN Addresses A ON A.Address_ID = I.Address_ID
			WHERE P.Show_On_Map = 1
   END	
END
GO

ALTER TABLE [dbo].[Households] ENABLE TRIGGER [tr_Address_Update]
GO


