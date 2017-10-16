USE [MinistryPlatform]
GO

--this one is accessed by id in the code, trying to be consistent
update dp_page_views
set field_list = 'Contacts.Contact_ID, Email_Address, Donor_Record, Donor_Record_Table.Processor_ID, Contacts.First_Name, Contacts.Last_Name'
where page_view_id = 1098

--this one is accessed by view title in the code, trying to be consistent with code
update dp_page_views
set Field_List = 'Contact_ID_Table.[Contact_ID], Donors.[Donor_ID], Donors.[Processor_ID], Statement_Frequency_ID_Table.[Statement_Frequency], Statement_Type_ID_Table.[Statement_Type], Statement_Method_ID_Table.[Statement_Method], Donors.[Setup_Date], Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_ID], Contact_ID_Table.[Email_Address] AS [Email], Contact_ID_Table_Household_ID_Table.[Household_ID] AS [Household_ID]
, Statement_Type_ID_Table.[Statement_Type_ID] AS [Statement_Type_ID], Contact_ID_Table.[First_Name], Contact_ID_Table.[Last_Name]'
where view_title = 'DonorByContactId'

--and update the email template
update dp_communications
set subject = 'You''ve Received A Note from Donor [Donor_Name] for Your GO Trip!'
where communication_id = 12530