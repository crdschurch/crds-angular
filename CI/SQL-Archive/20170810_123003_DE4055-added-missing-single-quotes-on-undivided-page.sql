USE [MinistryPlatform]
GO

UPDATE [dbo].[dp_Pages]
   SET [Default_Field_List] = 'Participant_ID_Table_Contact_ID_Table.[First_Name]
		, Participant_ID_Table_Contact_ID_Table.[Last_Name]
		, Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [Email]
		, Group_ID_Table.[Group_Name]
		, Group_ID_Table.[Description]
		, Group_Role_ID_Table.Role_Title
		, Group_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Site]
		, (SELECT TOP 1 Attribute_Name FROM Group_Participant_Attributes GPA, Attributes A WHERE GPA.Attribute_ID = A.Attribute_ID AND GPA.Group_Participant_ID = Group_Participants.Group_Participant_ID AND GETDATE() BETWEEN GPA.Start_Date AND ISNULL(GPA.End_Date,GETDATE()) AND Attribute_Type_ID = 85) AS [Facilitator_Training]
		, (STUFF((SELECT '' ''+ c.First_Name + '' '' + c.Last_Name + '' ('' + c.email_address + '') '' FROM Group_Participants gp INNER JOIN Contacts c ON c.Participant_record = gp.Participant_Id WHERE group_id = Group_ID_Table.[Group_ID] AND Group_Role_ID = 22 FOR XML PATH('''')), 1, 1, '''')) AS [Facilitators]
		, Group_Participants.[Notes]'
      ,[Selected_Record_Expression] = 'Participant_ID_Table_Contact_ID_Table.Display_Name'
      ,[Filter_Clause] = 'Group_ID_Table_Group_Type_ID_Table.Group_Type_ID = 26 AND (Group_Participants.End_Date > GetDate() OR Group_Participants.End_Date IS NULL) AND (Group_ID_Table.End_Date > GetDate() OR Group_ID_Table.End_Date IS NULL) AND Group_ID_Table_Parent_Group_Table.Group_ID IS NOT NULL'
 WHERE Page_ID = 556 AND Table_Name = 'Group_Participants'
GO


