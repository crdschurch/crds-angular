USE [MinistryPlatform]
GO

IF OBJECT_ID('dbo.api_crds_Get_Events_For_Group') IS NULL -- Check if SP Exists
        EXEC('CREATE PROCEDURE dbo.api_crds_Get_Events_For_Group AS SET NOCOUNT ON;') -- Create dummy/empty SP
GO

ALTER PROCEDURE api_crds_Get_Events_For_Group(
	@Group_ID INT,
	@Min_End_Date DATE = NULL,  -- NULL means no date restrictions
	@Cancelled BIT = NULL       -- NULL means both cancelled and active events 
) AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		e.Event_ID,
		e.Event_Title,
		c.Congregation_Name,
		e.Event_Start_Date,
		e.Event_End_Date,
		e.Event_Type_ID,
		et.Event_Type,
		e.Cancelled
	FROM
		Event_Groups eg
		INNER JOIN Events e ON eg.Event_ID = e.Event_ID
		INNER JOIN Event_Types et ON et.Event_Type_ID = e.Event_Type_ID
		INNER JOIN Congregations c ON c.Congregation_ID = e.Congregation_ID
	WHERE
		eg.Group_ID = @Group_ID AND
		eg.Domain_ID = 1 AND
		(@Min_End_Date IS NULL OR e.Event_End_Date >= @Min_End_Date) AND
		(@Cancelled IS NULL OR e.Cancelled = @Cancelled)
	ORDER BY
		e.Event_ID
	;
END


-- setup permissions for API User in MP

DECLARE @procName nvarchar(100) = N'api_crds_Get_Events_For_Group'
DECLARE @procDescription nvarchar(100) = N'Retrieves the list of events linked with a group'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName)
BEGIN
        INSERT INTO [dbo].[dp_API_Procedures] (
                 Procedure_Name
                ,Description
        ) VALUES (
                 @procName
                ,@procDescription
        )
END


DECLARE @API_ROLE_ID int = 62;
DECLARE @API_ID int;

SELECT @API_ID = API_Procedure_ID FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_API_Procedures] WHERE [Role_ID] = @API_ROLE_ID AND [API_Procedure_ID] = @API_ID)
BEGIN
        INSERT INTO [dbo].[dp_Role_API_Procedures] (
                 [Role_ID]
                ,[API_Procedure_ID]
                ,[Domain_ID]
        ) VALUES (
                 @API_ROLE_ID
                ,@API_ID
                ,1
        )
END
GO
