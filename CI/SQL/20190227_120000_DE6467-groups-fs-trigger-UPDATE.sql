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
    DECLARE @Groups_Cursor CURSOR;

	DECLARE @SMALL_GROUP_GROUP_TYPE INTEGER = 1;

	DECLARE @IN_PERSON_GROUP INTEGER = 2;
	DECLARE @ONLINE_GROUP INTEGER = 4;

    SET @Groups_Cursor = CURSOR FOR
        SELECT
            I.Group_Id,
            ISNULL(I.Group_Is_Full, 0),
            I.End_Date,
			I.Available_Online,
			I.Offsite_Meeting_Address
        FROM INSERTED I
		JOIN DELETED D ON I.Group_ID = D.Group_ID
		WHERE I.Group_Type_ID = @SMALL_GROUP_GROUP_TYPE
		AND (I.Group_Name <> D.Group_Name OR I.Description <> D.Description
		     OR I.End_Date <> D.End_Date OR I.Available_Online <> D.Available_Online)

    DECLARE @Group_Id INT;
    DECLARE @I_Full BIT;
    DECLARE @I_EndDate DateTime;
	DECLARE @I_AvailOnline BIT;
	DECLARE @I_OffsiteMeetingAddressId INT;
    OPEN @Groups_Cursor
    FETCH NEXT FROM @Groups_Cursor INTO @Group_Id, @I_Full, @I_EndDate, @I_AvailOnline, @I_OffsiteMeetingAddressId
    WHILE @@FETCH_STATUS = 0
    BEGIN
	    --add in person group pin to firestore
        IF (@I_Full = 0) AND (@I_AvailOnline = 1) AND (@I_OffsiteMeetingAddressId IS NOT NULL) AND (@I_EndDate IS NULL OR @I_EndDate > GETDATE())
            INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
			       VALUES(@Group_Id,1,0,@IN_PERSON_GROUP)

		--add online group pin to firestore
        IF (@I_Full = 0) AND (@I_AvailOnline = 1) AND (@I_OffsiteMeetingAddressId IS NULL) AND (@I_EndDate IS NULL OR @I_EndDate > GETDATE())
            INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
			       VALUES(@Group_Id,1,0,@ONLINE_GROUP)

        FETCH NEXT FROM @Groups_Cursor INTO @Group_Id, @I_Full, @I_EndDate, @I_AvailOnline, @I_OffsiteMeetingAddressId
    END
    CLOSE @Groups_Cursor
    DEALLOCATE @Groups_Cursor
END


GO

ALTER TABLE [dbo].[Groups] ENABLE TRIGGER [crds_tr_Small_Group_To_Firestore]
GO


