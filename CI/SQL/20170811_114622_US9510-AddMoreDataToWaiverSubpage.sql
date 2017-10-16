USE [MinistryPlatform]
GO

DECLARE @WaiverSubPage int = 601

IF EXISTS(SELECT 1 FROM [dbo].[dp_Sub_Pages] WHERE Sub_Page_ID = @WaiverSubPage AND Display_Name = 'Waivers')
BEGIN
	UPDATE [dbo].[dp_Sub_Pages]
	SET [Default_Field_List] = 'Waiver_ID_Table.[Waiver_Name]
		, cr_Event_Participant_Waivers.[Accepted]
		, Signee_Contact_ID_Table.[Display_Name] as [Signee Contact]
		, cr_Event_Participant_Waivers.[Waiver_Create_Date]'
 WHERE Sub_Page_ID = @WaiverSubPage
END
