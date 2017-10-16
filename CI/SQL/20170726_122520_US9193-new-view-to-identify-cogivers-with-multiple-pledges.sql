USE [MinistryPlatform]
GO


DECLARE @PageViewID int = 1127 

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewID)
BEGIN
	SET IDENTITY_INSERT dp_Page_Views ON

	INSERT INTO [dbo].[dp_Page_Views]
			   ([Page_View_ID]
			   ,[View_Title]
			   ,[Page_ID]
			   ,[Description]
			   ,[Field_List]
			   ,[View_Clause]
			   ,[Order_By]
			   ,[User_ID]
			   ,[User_Group_ID])
		 VALUES
			   (@PageViewID
			   ,'Co-Giver with Multiple Pledges'
			   ,363
			   ,'Finance Team wanted Page View to identify active/completed co-givers for the I''m In campaign under Stewardship - Pledges'
			   ,'Donor_ID_Table_Contact_ID_Table.[Household_ID] AS [Household], Pledge_Campaign_ID_Table.[Pledge_Campaign_ID] AS [Pledge Campaign], (SELECT COUNT(*) FROM Pledges p1 JOIN Donors d ON d.Donor_ID = p1.Donor_ID JOIN Contacts c ON c.donor_record = d.donor_id WHERE c.Household_ID = Donor_ID_Table_Contact_ID_Table.[Household_ID] AND d.[Statement_Type_ID] = 2 AND p1.[Pledge_Status_ID] in(1,2) AND p1.[Pledge_Campaign_ID] = 1103) AS [Pledge Count for Household], (SELECT MIN(c.Display_Name) FROM Pledges p1 INNER JOIN Donors d ON d.Donor_ID = p1.Donor_ID INNER JOIN Contacts c ON c.donor_record = d.donor_id WHERE c.Household_ID = Donor_ID_Table_Contact_ID_Table.[Household_ID] AND d.[Statement_Type_ID] = 2 AND p1.[Pledge_Status_ID] in(1,2) AND p1.[Pledge_Campaign_ID] = 1103) AS [Donor], (SELECT MAX(c.Display_Name) FROM Pledges p1 INNER JOIN Donors d ON d.Donor_ID = p1.Donor_ID INNER JOIN Contacts c ON c.donor_record = d.donor_id WHERE c.Household_ID = Donor_ID_Table_Contact_ID_Table.[Household_ID] AND d.[Statement_Type_ID] = 2 AND p1.[Pledge_Status_ID] in(1,2) AND p1.[Pledge_Campaign_ID] = 1103) AS [Donor 2], Donor_ID_Table_Contact_ID_Table.[Email_Address] AS [Email Address], Donor_ID_Table_Contact_ID_Table.[Mobile_Phone] AS [Phone Number]'
			   ,'Donor_ID_Table.[Statement_Type_ID] = 2 AND Pledge_Status_ID_Table.[Pledge_Status_ID] in(1,2) AND Pledge_Campaign_ID_Table.[Pledge_Campaign_ID] = 1103 AND (SELECT COUNT(*) FROM Pledges p1 JOIN Donors d ON d.Donor_ID = p1.Donor_ID JOIN Contacts c ON c.donor_record = d.donor_id WHERE c.Household_ID = Donor_ID_Table_Contact_ID_Table.[Household_ID] AND d.[Statement_Type_ID] = 2 AND p1.[Pledge_Status_ID] in(1,2) AND p1.[Pledge_Campaign_ID] = 1103 GROUP BY c.Household_ID) > 1'
			   ,'Donor_ID_Table_Contact_ID_Table.[Household_ID]'
			   ,NULL
			   ,NULL)
		SET IDENTITY_INSERT dp_Page_Views OFF
END

