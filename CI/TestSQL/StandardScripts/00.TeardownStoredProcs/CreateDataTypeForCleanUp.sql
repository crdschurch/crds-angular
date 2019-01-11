-- ================================
-- Create User-defined Table Type 
-- for automation clean up
-- ================================
USE [MinistryPlatform]
GO

-- Create the data type
CREATE TYPE dbo.cr_QA_List_Of_Ids AS TABLE 
(
	ID INT NOT NULL
)
GO
