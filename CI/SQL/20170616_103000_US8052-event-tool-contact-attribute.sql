USE [MinistryPlatform]
GO

DECLARE @ATTRIBUTE_ID int = 9048;
DECLARE @ATTRIBUTE_TYPE_ID int = 89;	-- Contact Segmentation

IF NOT EXISTS (SELECT 1 FROM Attributes WHERE Attribute_ID = @ATTRIBUTE_ID)
BEGIN
    SET IDENTITY_INSERT Attributes ON;

    INSERT INTO Attributes
        (Attribute_ID, Attribute_Name, Description, Domain_ID, Attribute_Type_ID)
    VALUES
        (@ATTRIBUTE_ID, N'Event Tool Contact', N'Include in Primary Contact list in Create/Edit Event Tool', 1, @ATTRIBUTE_TYPE_ID)
    ;

    SET IDENTITY_INSERT Attributes OFF;
END
