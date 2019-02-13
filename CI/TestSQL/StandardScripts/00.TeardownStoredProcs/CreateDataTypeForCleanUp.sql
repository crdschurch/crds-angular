-- ================================
-- Create User-defined Table Type 
-- for automation clean up
-- ================================
USE [MinistryPlatform]
GO

-- Create the data type
IF NOT EXISTS(SELECT 1 FROM sys.types WHERE name = 'cr_QA_List_Of_Ids' AND is_table_type = 1)
BEGIN
	CREATE TYPE dbo.cr_QA_List_Of_Ids AS TABLE 
	(
		ID INT NOT NULL
	)
END
