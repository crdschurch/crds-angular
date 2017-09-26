USE [MinistryPlatform]
GO

DECLARE @subpageID int = 618;
DECLARE @pageID int = 635;
DECLARE @pageSectionID int = 9;

-------  Add Trip Pledges Page   -----------

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Pages] WHERE Page_ID = @PageId)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Pages] ON
	INSERT INTO [dbo].[dp_Pages]
			   ([Page_ID]
			   ,[Display_Name]
			   ,[Singular_Name]
			   ,[Description]
			   ,[View_Order]
			   ,[Table_Name]
			   ,[Primary_Key]
			   ,[Display_Search]
			   ,[Default_Field_List]
			   ,[Selected_Record_Expression]
			   ,[Filter_Clause]
			   ,[Display_Copy])
		 VALUES
           (@pageID
		   ,'Trip Pledges'
           ,'Trip Pledge'
		   ,'Trip pledge page for volunteers'
		   ,60
		   ,'Pledges'
		   ,NULL
		   ,NULL
           ,'Donor_ID_Table_Contact_ID_Table.Display_Name ,Donor_ID_Table_Contact_ID_Table.Nickname ,Donor_ID_Table_Contact_ID_Table.First_Name ,Pledges.Total_Pledge ,Pledge_Campaign_ID_Table.Campaign_Name ,Pledges.Beneficiary ,Pledge_Status_ID_Table.Pledge_Status ,Pledges.First_Installment_Date'
           ,'Donor_ID_Table_Contact_ID_Table.Display_Name'
           ,'Pledge_Campaign_ID_Table.Pledge_Campaign_Type_ID = 2'
           ,1)
	SET IDENTITY_INSERT [dbo].[dp_Pages] OFF
END

-------------  Add Donations subpage to Trip Pledges page   ----------------------

IF NOT EXISTS (SELECT 1 FROM dp_Sub_Pages WHERE Sub_Page_ID = @subpageID) 
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Sub_Pages] ON;
	INSERT INTO dp_Sub_Pages (
		 [Sub_Page_ID]
		,[Display_Name]
		,[Singular_Name]
		,[Page_ID]
		,[View_Order]
		,[Primary_Table]
		,[Default_Field_List]
		,[Selected_Record_Expression]
		,[Filter_Key]
		,[Relation_Type_ID]
		,[Display_Copy]
	) VALUES (
		 @subpageID
		,'Donations'
		,'Donation'
		,@pageID
		,1
		,'Donation_Distributions'
		,'Donation_ID_Table.Donation_Date ,Donation_Distributions.Amount ,Donation_ID_Table.Item_Number ,Program_ID_Table.Program_Name ,Donation_ID_Table_Donor_ID_Table_Contact_ID_Table.Display_Name AS [Given By] ,Donation_ID_Table_Donation_Status_ID_Table.Donation_Status'
		,'Donation_Distributions.Amount'
		,'Pledge_ID'
		,1
		,1
	)

	SET IDENTITY_INSERT [dbo].[dp_Sub_Pages] OFF;
END

------- Add Trip Pledge Page to Stewardship Section ------------

INSERT INTO [dbo].[dp_Page_Section_Pages]
           ([Page_ID]
           ,[Page_Section_ID]
           ,[User_ID])
     VALUES
           (@pageID
           ,@pageSectionID
           ,NULL)
GO


