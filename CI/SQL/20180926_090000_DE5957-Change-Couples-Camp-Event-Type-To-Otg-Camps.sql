USE MinistryPlatform
GO

DECLARE @OTG_CAMP_EVENT_TYPE_ID int = 9;

UPDATE [MinistryPlatform].[dbo].[Events]
SET Event_Type_ID = @OTG_CAMP_EVENT_TYPE_ID
WHERE Event_Title = 'Couples Camp - Spring 2019';