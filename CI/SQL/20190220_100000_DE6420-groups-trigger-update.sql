USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Phil Lachmann
-- Create date: 12/04/2018
-- Description: 
-- =============================================
CREATE OR ALTER TRIGGER [dbo].[crds_tr_Small_Group_To_Firestore]
   ON  [dbo].[Groups]
   AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Groups_Cursor CURSOR;

	DECLARE @SMALL_GROUP_GROUP_TYPE INTEGER = 1;

	DECLARE @IN_PERSON_GROUP INTEGER = 2;
	DECLARE @ONLINE_GROUP INTEGER = 4;

    SET @Groups_Cursor = CURSOR FOR
        SELECT
            C.Group_Id,
            ISNULL(C.Group_Is_Full, 0),
            C.End_Date,
			C.Available_Online,
			C.Offsite_Meeting_Address
			/****************************************************************/
			/* Added this EXCEPT block to remove unchanged rows from cursor */
			/****************************************************************/
        FROM 
		(
		   SELECT * FROM Inserted
		   EXCEPT
		   SELECT * FROM Deleted
		) C 
		WHERE C.Group_Type_ID = @SMALL_GROUP_GROUP_TYPE

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
