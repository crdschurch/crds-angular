USE [MinistryPlatform]

DECLARE @ChildcareEventType INT = 243

UPDATE [dbo].[Events]
SET [Primary_Contact] = [Congregations].[Childcare_Contact]
FROM [dbo].[Congregations]
WHERE [Events].[Congregation_ID] = [Congregations].[Congregation_ID] AND [Events].[Event_Type_ID] = @ChildcareEventType