USE MinistryPlatform
GO

-- =============================================
-- Author:      Chris Andrews
-- Create date: 2018-11-29
-- Description:	Add Event Check In field
-- =============================================

IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = N'Event_CheckIn' AND Object_ID = Object_ID(N'dbo.[Groups]'))
BEGIN
    ALTER TABLE [dbo].Groups ADD Event_CheckIn BIT NOT NULL DEFAULT(0)
END
GO

DECLARE @Groups_Page_ID AS INT = 322

IF NOT EXISTS(SELECT * FROM dbo.dp_Pages 
    WHERE Default_Field_List LIKE '%Event_CheckIn%' 
    AND Page_ID = @Groups_Page_ID)
BEGIN
    UPDATE dbo.dp_Pages
    SET
        [Default_Field_List] = CONCAT([Default_Field_List], ', Groups.[Event_CheckIn]')
    WHERE
        Page_ID = @Groups_Page_ID
END
GO