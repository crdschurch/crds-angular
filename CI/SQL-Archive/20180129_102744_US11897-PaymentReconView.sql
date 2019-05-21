USE [MinistryPlatform]
GO

DECLARE @PageViewID int = 1131

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewID)
BEGIN
	SET IDENTITY_INSERT dp_Page_Views ON
	INSERT INTO [dbo].[dp_Page_Views]
           ([Page_View_ID]
		       ,[View_Title]
           ,[Page_ID]
           ,[Description]
           ,[Field_List]
           ,[View_Clause])
     VALUES
           (@PageViewID
		      ,'Payment Reconciliation'
          ,273
          ,'Payment reconciliation view for finance'
          ,'Invoice_Detail_ID_Table_Product_ID_Table.[Product_Name]
          ,Invoice_Detail_ID_Table_Event_Participant_ID_Table_Event_ID_Table.[Event_Title]
          ,Payment_ID_Table_Contact_ID_Table.[Display_Name]
          ,Payment_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name]
          ,Payment_ID_Table.[Payment_Date] 
          ,Payment_ID_Table_Payment_Type_ID_Table.[Payment_Type]
          ,Payment_Detail.[Payment_Amount] 
          ,Payment_ID_Table_Payment_Status_ID_Table.[Donation_Status] AS [Payment Status]'
          ,'Payment_ID_Table_Payment_Status_ID_Table.[Donation_Status] IS NOT NULL')
	SET IDENTITY_INSERT dp_Page_Views OFF
END