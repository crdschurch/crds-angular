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
   AFTER INSERT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    DECLARE @Groups_Cursor CURSOR;

    SET @Groups_Cursor = CURSOR FOR
        SELECT
            I.Group_Id,
            ISNULL(I.Group_Is_Full, 0),
            I.End_Date
        FROM INSERTED I

    DECLARE @Group_Id INT;
    DECLARE @I_Full BIT;
    DECLARE @I_EndDate DateTime;
    OPEN @Groups_Cursor
    FETCH NEXT FROM @Groups_Cursor INTO @Group_Id, @I_Full, @I_EndDate
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (@I_Full = 0) AND (@I_EndDate IS NULL OR @I_EndDate > GETDATE())
            INSERT INTO cr_mapaudit(Participant_ID,ShowOnMap,Processed,PinType)
			       VALUES(@Group_Id,1,0,2)
        FETCH NEXT FROM @Groups_Cursor INTO @Group_Id, @I_Full, @I_EndDate
    END
    CLOSE @Groups_Cursor
    DEALLOCATE @Groups_Cursor
END

GO

ALTER TABLE [dbo].[Groups] ENABLE TRIGGER [crds_tr_Small_Group_To_Firestore]
GO


