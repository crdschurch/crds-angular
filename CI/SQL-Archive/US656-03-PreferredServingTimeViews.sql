USE [MinistryPlatform]

UPDATE [dbo].[dp_Sub_Pages]
SET Default_Field_List = 'Participant_ID_Table_Contact_ID_Table.Display_Name
    ,Group_Role_ID_Table.Role_Title
    ,(SELECT MAX(E.Event_Start_Date) FROM Event_Participants EP
        INNER JOIN Events E ON E.Event_ID = EP.Event_ID
        WHERE EP.Group_Participant_ID = Group_Participants.Group_Participant_ID
        AND EP.Participation_Status_ID IN (3,4)) AS Last_Attended
    ,Group_Participants.Start_Date
    ,Group_Participants.End_Date
    ,Group_Participants.Employee_Role
    ,Group_Participants.Hours_Per_Week
    ,Group_Participants.Participant_ID
    ,Group_Participants.[Child_Care_Requested]
	,Preferred_Serving_Time_ID_Table.[Preferred_Serve_Time]'
WHERE Sub_Page_ID = 298

UPDATE [dbo].[dp_Pages]
SET Default_Field_List = 'Participant_ID_Table_Contact_ID_Table.Display_Name AS GP_Display_Name
,Participant_ID_Table_Contact_ID_Table.Nickname
,Participant_ID_Table_Contact_ID_Table.First_Name
,Group_ID_Table.Group_Name
,Group_Role_ID_Table.Role_Title
,Group_ID_Table_Ministry_ID_Table.Ministry_Name
,Group_ID_Table_Congregation_ID_Table.Congregation_Name
,Group_ID_Table_Parent_Group_Table.Group_Name AS [Parent Group]
,Group_Participants.Start_Date
,Participant_ID_Table_Contact_ID_Table.Email_Address AS GP_Email_Address
,Participant_ID_Table_Contact_ID_Table.Mobile_Phone AS GP_Mobile_Phone
,Group_ID_Table.[Group_ID]
,Preferred_Serving_Time_ID_Table.[Preferred_Serve_Time]'
WHERE Page_ID = 316

UPDATE [dbo].[dp_Pages]
SET Default_Field_List = 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name]
, Participant_ID_Table_Contact_ID_Table.[Nickname] 
, Participant_ID_Table_Contact_ID_Table.[Last_Name] 
, Group_ID_Table.[Group_Name] 
, Group_Role_ID_Table.[Role_Title] 
, Participant_ID_Table_Contact_ID_Table.[Email_Address] 
, Participant_ID_Table_Contact_ID_Table.[Date_of_Birth] 
, Group_Participants.[Start_Date], Group_Participants.[End_Date]
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] 
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table.[Home_Phone] 
, Participant_ID_Table_Contact_ID_Table.[Mobile_Phone] 
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Address_Line_1] 
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Address_Line_2] 
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[City] 
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[State/Region] 
, Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Postal_Code] 
, Group_ID_Table_Ministry_ID_Table.[Ministry_Name]
, Preferred_Serving_Time_ID_Table.[Preferred_Serve_Time]'
WHERE Page_ID = 537