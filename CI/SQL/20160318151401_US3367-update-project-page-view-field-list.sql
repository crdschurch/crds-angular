USE [MinistryPlatform]
GO

UPDATE [MinistryPlatform].[dbo].[dp_Pages]
SET [Default_Field_List] = 'Project_Name, Location_ID_Table.Location_Name, Project_Type_ID_Table.Description AS [Project_Type], Project_Status_ID_Table.Description AS [Project_Status], Organization_ID_Table.Name AS [Organization], Initiative_ID_Table.Initiative_Name, Minimum_Volunteers, Maximum_Volunteers, _Volunteer_Count'
WHERE PAGE_ID = 14

GO