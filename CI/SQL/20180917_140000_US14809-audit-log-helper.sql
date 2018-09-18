USE [MinistryPlatform]
GO

CREATE TYPE crds_Audit_Item AS TABLE (
    Row_Number INT IDENTITY(1,1) NOT NULL,
    Table_Name VARCHAR(50) NOT NULL,
    Record_ID INT NOT NULL,
    Audit_Description VARCHAR(50) NOT NULL,

    Field_Name NVARCHAR(50),
    Field_Label NVARCHAR(50),
    Previous_Value NVARCHAR(max),
    New_Value NVARCHAR(max),
    Previous_ID INT,
    New_ID INT,

    PRIMARY KEY(Row_Number)
);
GO

-- Creates a series of audit log records.  The first parameter is a user-defined table type
-- and may contain multiple records that are added all at once.  The procedure automatically
-- creates the appropriate rows in both dp_Audit_Log and dp_Audit_Detail (with appropriate
-- foreign keys) based on the data supplied.  Rows in Record_List that have the same 
-- Table_Name, Record_ID, and Audit_Description are combined into a single master record
-- in dp_Audit_Log, and multiple detail records are created in dp_Audit_Detail and linked
-- with the master record.  Example:
-- Record_List:
--     (Table_Name = 'Donors', Record = 123, Audit_Description = 'Updated', Field_Name = 'Statement_Method_ID', ... )
--     (Table_Name = 'Donors', Record = 123, Audit_Description = 'Updated', Field_Name = 'Statement_Frequency_ID', ... )
--     (Table_Name = 'Donors', Record = 456, Audit_Description = 'Updated', Field_Name = 'Statement_Method_ID', ... )
--     (Table_Name = 'Donors', Record = 456, Audit_Description = 'Updated', Field_Name = 'Statement_Frequency_ID', ... )
-- creates two rows in dp_Audit_Log:
--     (Table_Name = 'Donors', Record = 123, Audit_Description = 'Updated')
--     (Table_Name = 'Donors', Record = 456, Audit_Description = 'Updated')
-- and four rows in dp_Audit_Detail
--     (Audit_Item_ID = 1, Field_Name = 'Statement_Method_ID', ...)
--     (Audit_Item_ID = 1, Field_Name = 'Statement_Frequency_ID', ...)
--     (Audit_Item_ID = 2, Field_Name = 'Statement_Method_ID', ...)
--     (Audit_Item_ID = 2, Field_Name = 'Statement_Frequency_ID', ...)
CREATE PROCEDURE crds_Add_Audit_Items(
    @Record_List crds_Audit_Item READONLY,
	@Date_Time DATETIME,
	@User_Name NVARCHAR(254),
	@User_ID INT = NULL,
	@On_Behalf_Of_User_ID INT = NULL,
	@On_Behalf_Of_User_Name VARCHAR(256) = NULL,
	@Impersonated_By_User_ID INT = NULL,
	@Impersonated_By_User_Name VARCHAR(256) = NULL
) AS
BEGIN
    -- Capture auto-generated Audit_Item_IDs
    DECLARE @Audit_ID_List TABLE (
        Audit_Item_ID INT NOT NULL,
        Table_Name VARCHAR(50) NOT NULL,
        Record_ID INT NOT NULL,
        Audit_Description VARCHAR(50) NOT NULL
    );

    -- Create one dp_Audit_Log row per distinct Table_Name, Record_ID, Audit_Description
    WITH Audit_Items_CTE (Row_Number, Row_Number_By_Partition)
    AS (
        SELECT
            Row_Number,
            Row_Number_By_Partition = ROW_NUMBER() OVER (PARTITION BY Table_Name, Record_ID, Audit_Description ORDER BY Row_Number)
        FROM
            @Record_List
    )
    INSERT INTO dp_Audit_Log (
        Table_Name,
        Record_ID,
        Audit_Description,
        User_Name,
        User_ID,
        Date_Time,
        On_Behalf_Of_User_ID,
        On_Behalf_Of_User_Name,
        Impersonated_By_User_ID,
        Impersonated_By_User_Name
    )
    OUTPUT
        INSERTED.Audit_Item_ID,
        INSERTED.Table_Name,
        INSERTED.Record_ID,
        INSERTED.Audit_Description
    INTO @Audit_ID_List
    SELECT
        Table_Name,
        Record_ID,
        Audit_Description,
        @User_Name,
        @User_ID,
        @Date_Time,
        @On_Behalf_Of_User_ID,
        @On_Behalf_Of_User_Name,
        @Impersonated_By_User_ID,
        @Impersonated_By_User_Name
    FROM
        @Record_List
    WHERE
        Row_Number IN (SELECT Row_Number FROM Audit_Items_CTE WHERE Row_Number_By_Partition = 1)
    ORDER BY
        Row_Number
    ;

    -- Create dp_Audit_Detail rows linked with appropriate dp_Audit_Log row created earlier
    INSERT INTO dp_Audit_Detail (
        Audit_Item_ID,
        Field_Name,
        Field_Label,
        Previous_Value,
        New_Value,
        Previous_ID,
        New_ID
    )
    SELECT
        i.Audit_Item_ID,
        r.Field_Name,
        r.Field_Label,
        r.Previous_Value,
        r.New_Value,
        r.Previous_ID,
        r.New_ID
    FROM
        @Audit_ID_List i
        INNER JOIN @Record_List r ON
            r.Table_Name = i.Table_Name
            AND r.Record_ID = i.Record_ID
            AND r.Audit_Description = i.Audit_Description
    WHERE
        r.Field_Name IS NOT NULL
    ORDER BY
        r.Row_Number
    ;
END
GO
