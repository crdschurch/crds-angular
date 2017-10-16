USE [MinistryPlatform]
GO

IF EXISTS(SELECT 1 FROM [dbo].[dp_Pages] WHERE Page_ID = 516)
BEGIN

UPDATE dp_Pages
SET Default_Field_List = 
'Donation_ID_Table.Donation_Date
,CASE WHEN (Donation_Distributions.Soft_Credit_Donor IS NULL) THEN ''False'' ELSE ''True'' END AS [Soft_Credit_Donation]
,ISNULL(Donation_ID_Table_Donor_ID_Table_Contact_ID_Table.Last_Name,Donation_ID_Table_Donor_ID_Table_Contact_ID_Table.Display_Name) AS [Last_Name]
,Donation_ID_Table_Donor_ID_Table_Contact_ID_Table.First_Name
,Donation_Distributions.Amount
,Donation_ID_Table_Payment_Type_ID_Table.Payment_Type
,Donation_ID_Table.Item_Number
,Program_ID_Table.Program_Name
,Program_ID_Table.Statement_Title
,Pledge_ID_Table_Pledge_Campaign_ID_Table.Campaign_Name
,Donation_Distributions.Donation_ID
,Donation_ID_Table.Donor_ID
,Donation_ID_Table_Batch_ID_Table.Batch_ID
,Pledge_ID_Table.Pledge_ID
,Target_Event_Table.Event_Title AS [Target_Event]
,Donation_ID_Table.Donation_Status_Date
,Donation_ID_Table.Donation_Status_ID
,Donation_ID_Table.Transaction_Code
,Donation_ID_Table.Payment_Type_ID
,Soft_Credit_Donor_Table.Donor_ID AS [Soft_Credit_Donor_ID]
,Donation_ID_Table_Donor_ID_Table_Contact_ID_Table.[Display_Name] AS [Donor_Display_Name]
,Donation_ID_Table.Is_Recurring_Gift
,Congregation_ID_Table_Accounting_Company_ID_Table.[Company_Name]
,Congregation_ID_Table_Accounting_Company_ID_Table.[Show_Online]
,Donation_ID_Table.Notes
,CASE WHEN (Donation_ID_Table.[__ExternalContributionID] IS NOT NULL) THEN ''True'' ELSE ''False'' END AS [Is_Legacy]'
WHERE page_id = 516

END