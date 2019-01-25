USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:      Phil Lachmann
-- Create date: 01/25/2019
-- Description: 
-- =============================================
CREATE OR ALTER  TRIGGER [dbo].[crds_tr_Small_Group_To_Firestore_GroupAttr]
   ON  [dbo].[Groups]
   AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @GroupAttributess_Cursor CURSOR;

	DECLARE @IN_PERSON_GROUP INTEGER = 2;
	DECLARE @ONLINE_GROUP INTEGER = 4;

    SET @GroupAttributes_Cursor = CURSOR FOR
        SELECT
            I.Group_Id,
            I.End_Date,
			G.Offsite_Meeting_Address
        FROM INSERTED I 
		JOIN Groups G ON G.Group_ID = I.Group_ID
		WHERE ISNULL(G.Group_Is_Full, 0) = 0 AND G.Available_Online = 1 AND (G.End_Date IS NULL OR G.End_Date > GETDATE())

    DECLARE @Group_Id INT;
    DECLARE @I_EndDate DateTime;
	DECLARE @I_OffsiteMeetingAddressId INT;
	
    OPEN @Groups_Cursor
    FETCH NEXT FROM @GroupAttributes_Cursor INTO @Group_Id, @I_EndDate, @I_OffsiteMeetingAddressId
    WHILE @@FETCH_STATUS = 0
    BEGIN
	    --add in person group pin to firestore
        IF (@I_OffsiteMeetingAddressId IS NOT NULL) 
            INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
			       VALUES(@Group_Id,1,0,@IN_PERSON_GROUP)

		--add online group pin to firestore
        IF (@I_OffsiteMeetingAddressId IS NULL) 
            INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
			       VALUES(@Group_Id,1,0,@ONLINE_GROUP)

        FETCH NEXT FROM @GroupAttributes_Cursor INTO @Group_Id, @I_EndDate, @I_OffsiteMeetingAddressId
    END
    CLOSE @GroupAttributes_Cursor
    DEALLOCATE @GroupAttributes_Cursor
END

GO

ALTER TABLE [dbo].[Groups_Attributes] ENABLE TRIGGER [crds_tr_Small_Group_To_Firestore_GroupAttr]
GO

