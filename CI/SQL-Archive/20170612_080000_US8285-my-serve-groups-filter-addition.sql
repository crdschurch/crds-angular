----Insert new page view for My Serve Groups
USE [MinistryPlatform]


SET IDENTITY_INSERT dp_Page_Views ON

INSERT INTO [MinistryPlatform].[dbo].[dp_Page_Views] 
([Page_View_ID],[View_Title],[Page_ID],[Description],[Field_List],[View_Clause],[Order_By])
VALUES (1123, 'Current Groups', 536, 'Default filter to show all current "My Serve Groups" to exclude any past groups that have been end-dated'
,'Groups.Group_Name,Congregation_ID_Table.Congregation_Name,Ministry_ID_Table.Ministry_Name,Group_Type_ID_Table.Group_Type,Groups.Start_Date,
Groups.End_Date,Parent_Group_Table.Group_Name AS [Parent Group]',
'EXISTS (SELECT 1 FROM Group_Participants GP INNER JOIN Contacts C ON C.Participant_Record = GP.Participant_ID AND C.User_Account = @UserId WHERE GP.Group_ID = Groups.Group_ID AND GP.Group_Role_ID = 22 AND (GP.End_Date IS NULL OR GP.End_Date >= GetDate()))
AND Group_Type_ID_Table.[Group_Type_ID] = 9 
 AND (Groups.End_Date IS NULL OR Groups.End_Date >= GetDate())',
6);



GO

UPDATE [MinistryPlatform].[dbo].[dp_Pages]
SET [Default_View] = 1123 , [Pick_List_View] = 1123
WHERE Page_ID = 536 