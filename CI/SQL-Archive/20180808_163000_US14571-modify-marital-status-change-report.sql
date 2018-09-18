USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[report_CRDS_Marital_Status_Change]    Script Date: 8/8/2018 4:39:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER  PROCEDURE [dbo].[report_CRDS_Marital_Status_Change] 
     @FromDate DATE = NULL,
     @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

	-- default to yesterday
    IF @FromDate IS NULL OR @ToDate IS NULL
    BEGIN
        SET @FromDate = DATEADD(DAY, -1, GETDATE())
        SET @ToDate = @FromDate;
    END

    -- end date is inclusive (e.g., 7/18 to 7/18 means 7/18 00:00 to 7/19 00:00)
    SET @ToDate = DATEADD(DAY, 1, @ToDate);

    DECLARE @log TABLE (
        Audit_ID INT IDENTITY(1,1) NOT NULL,
        Change_Date DATETIME NOT NULL,
        User_Name NVARCHAR(254) NULL,
        On_Behalf_Of_User_Name NVARCHAR(256) NULL,
        Contact_ID INT NOT NULL,
        Previous_Value NVARCHAR(MAX),
        New_Value NVARCHAR(MAX),
        Row_Num INT NOT NULL
    )

    INSERT INTO @log
        (Change_Date, User_Name, On_Behalf_Of_User_Name, Contact_ID, Previous_Value, New_Value, Row_Num)
    SELECT
        al.Date_Time,
        al.User_Name,
        al.On_Behalf_Of_User_Name,
        al.Record_ID,
        ad.Previous_Value,
        ad.New_Value,
        ROW_NUMBER() OVER (PARTITION BY al.Record_ID ORDER BY al.Audit_Item_ID)
    FROM
        dp_Audit_Log al
        INNER JOIN dp_Audit_Detail ad ON ad.Audit_Item_ID = al.Audit_Item_ID
    WHERE
        al.Table_Name = 'Contacts'
        AND ad.Field_Name = 'Marital_Status_ID'
        AND al.Date_Time >= @FromDate AND al.Date_Time < @ToDate
		AND (ad.Previous_ID = 2 OR (ad.New_ID = 2)) -- Only from Married or to Married
    ORDER BY
        al.Audit_Item_ID
    ;

    SELECT
        c.Contact_ID,
        c.First_Name,
        c.Last_Name,
        c.Email_Address,
        c.Mobile_Phone,
        l.Change_Date,
        l.Previous_Value,
        l.New_Value,
        l.User_Name,
        l.On_Behalf_Of_User_Name
    FROM
        @log as l
        INNER JOIN Contacts c ON c.Contact_ID = l.Contact_ID
    ORDER BY
        c.Contact_ID, l.Row_Num
    ;

END
