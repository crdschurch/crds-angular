USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:      Phil Lachmann
-- 
-- Description: 
-- =============================================
CREATE OR ALTER TRIGGER [dbo].[crds_tr_Small_Group_To_Firestore_UPDATE]
   ON  [dbo].[Groups]
   AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @SMALL_GROUP_GROUP_TYPE INTEGER = 1;
	DECLARE @IN_PERSON_GROUP INTEGER = 2;
	DECLARE @ONLINE_GROUP INTEGER = 4;

	INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
		SELECT I.GROUP_ID, 1,0, IIF (I.Offsite_Meeting_Address IS NOT NULL, @IN_PERSON_GROUP , @ONLINE_GROUP)
		FROM INSERTED I
		JOIN DELETED D ON I.Group_ID = D.Group_ID
		WHERE I.Group_Type_ID = @SMALL_GROUP_GROUP_TYPE AND
		      (I.End_Date IS NULL OR I.End_Date > GETDATE()) AND
			  I.Group_Is_Full = 0 AND
			  (I.Group_Name <> D.Group_Name OR I.Description <> D.Description OR I.End_Date <> D.End_Date OR I.Available_Online <> D.Available_Online)

	--set the record to remove if its end dated or available_online is changed to false
	INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
		SELECT I.GROUP_ID, 0,0, IIF (I.Offsite_Meeting_Address IS NOT NULL, @IN_PERSON_GROUP , @ONLINE_GROUP)
		FROM INSERTED I
		JOIN DELETED D ON I.Group_ID = D.Group_ID
		WHERE I.Group_Type_ID = @SMALL_GROUP_GROUP_TYPE AND
		      ((I.End_Date <= GETDATE()));

END

GO

ALTER TABLE [dbo].[Groups] ENABLE TRIGGER [crds_tr_Small_Group_To_Firestore_UPDATE]
GO


