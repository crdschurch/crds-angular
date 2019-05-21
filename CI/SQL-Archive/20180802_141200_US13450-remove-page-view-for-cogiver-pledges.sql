USE [MinistryPlatform]
GO

-- Remove a view that is no longer needed
-- Page View ID: 1127 "Co-Giver with Multiple IM IN Pledges"

DECLARE @PageViewID int = 1127;

DELETE FROM dp_Notification_Page_Views WHERE Page_View_ID = @PageViewID;

DELETE FROM Publication_Page_Views WHERE Page_View_ID = @PageViewID;

DELETE FROM dp_Page_Views WHERE Page_View_ID = @PageViewID;
