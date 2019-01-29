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
CREATE OR ALTER   TRIGGER [dbo].[crds_tr_Small_Group_To_Firestore]
   ON  [dbo].[Groups]
   AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Groups_Cursor CURSOR;

    SET @Groups_Cursor = CURSOR FOR
        SELECT
            I.Group_Id,
            ISNULL(I.Group_Is_Full, 0),
            I.End_Date,
			I.Available_Online,
			I.Offsite_Meeting_Address
        FROM INSERTED I WHERE I.Group_Type_ID = 1

    DECLARE @Group_Id INT;
    DECLARE @I_Full BIT;
    DECLARE @I_EndDate DateTime;
	DECLARE @I_AvailOnline BIT;
	DECLARE @I_OffsiteMeetingAddressId INT;
    OPEN @Groups_Cursor
    FETCH NEXT FROM @Groups_Cursor INTO @Group_Id, @I_Full, @I_EndDate, @I_AvailOnline, @I_OffsiteMeetingAddressId
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (@I_Full = 0) AND (@I_AvailOnline = 1) AND (@I_OffsiteMeetingAddressId IS NOT NULL) AND (@I_EndDate IS NULL OR @I_EndDate > GETDATE())
            INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
			       VALUES(@Group_Id,1,0,2)
        FETCH NEXT FROM @Groups_Cursor INTO @Group_Id, @I_Full, @I_EndDate, @I_AvailOnline, @I_OffsiteMeetingAddressId
    END
    CLOSE @Groups_Cursor
    DEALLOCATE @Groups_Cursor
END


GO


