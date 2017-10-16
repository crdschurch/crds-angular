USE MinistryPlatform
GO

DECLARE @groupLeaderStatusViewId INT = 1121;

UPDATE dp_Page_Views
SET View_Title = N'Group Leader Status'
	, [Description] = N'Group Leader Status and when the status was last set'
WHERE Page_View_ID = @groupLeaderStatusViewId;