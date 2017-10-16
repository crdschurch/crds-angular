USE [MinistryPlatform]
GO

DECLARE @PageId int = 632;
DECLARE @PageSectionId int = 6; --People Lists

IF NOT EXISTS(SELECT 1 FROM dp_Pages WHERE Page_ID = @PageId)
BEGIN
	SET IDENTITY_INSERT dp_Pages ON
	INSERT INTO [dbo].[dp_Pages]
			   ([Page_ID]
			   ,[Display_Name]
			   ,[Singular_Name]
			   ,[Description]
			   ,[View_Order]
			   ,[Table_Name]
			   ,[Primary_Key]
			   ,[Default_Field_List]
			   ,[Selected_Record_Expression]
			   ,[Contact_ID_Field]
			   ,[Display_Copy])
		 VALUES
			   (@PageId
			   ,'Event Participant Documents'
			   ,'Event Participant Document'
			   ,'Documents to be collected from an Event Participant'
			   ,35
			   ,'cr_EventParticipant_Documents'
			   ,'EventParticipant_Document_ID'
			   ,'Event_Participant_ID_Table_Participant_ID_Table_Contact_ID_Table.[Display_Name]
	, Event_Participant_ID_Table_Participation_Status_ID_Table.[Participation_Status]
	, Event_Participant_ID_Table_Event_ID_Table.[Event_Title]
	, Document_ID_Table.[Document]
	, cr_EventParticipant_Documents.[Received]
	, cr_EventParticipant_Documents.[Notes]
	, [dp_Updated].[Date_Time] AS [Date Updated]'
			   ,'Event_Participant_ID_Table_Event_ID_Table.[Event_Title] + '': '' + Document_ID_Table.[Document]'
			   ,'Event_Participant_ID_Table_Participant_ID_Table_Contact_ID_Table.[Contact_ID]'
			   ,0)
	SET IDENTITY_INSERT dp_Pages OFF
END

IF NOT EXISTS(SELECT 1 FROM dp_Page_Section_Pages WHERE Page_ID = @PageId)
BEGIN
	INSERT INTO [dbo].[dp_Page_Section_Pages]
           ([Page_ID]
           ,[Page_Section_ID])
     VALUES
           (@PageId
           ,@PageSectionId)
END