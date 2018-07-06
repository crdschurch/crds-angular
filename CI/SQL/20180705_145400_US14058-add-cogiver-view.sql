USE [MinistryPlatform];
GO

DECLARE @Page_View_ID INTEGER = 1136;

IF NOT EXISTS(SELECT 1 FROM dp_Page_Views WHERE Page_View_ID = @Page_View_ID )
BEGIN
  SET IDENTITY_INSERT dp_Page_Views ON
  INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
	  VALUES(@Page_View_ID,
	         'Co-givers with Contact Relationship ID', 
			  290,
			 'Contact_Relationships.Contact_Relationship_ID,Contact_ID_Table.Display_Name AS Person_One,Contact_ID_Table.Contact_ID AS Person_One_ID,Relationship_ID_Table.Relationship_Name AS Relationship,Related_Contact_ID_Table.Display_Name AS Person_Two,Related_Contact_ID_Table.Contact_ID AS Person_Two_ID,Contact_Relationships.Start_Date,Contact_Relationships.End_Date',
			 'Contact_Relationships.Relationship_ID = 42'
			);
  SET IDENTITY_INSERT dp_Page_Views OFF
END
GO
