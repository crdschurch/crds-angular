USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_filter_camp_events_crossroads]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_filter_camp_events_crossroads] AS' 
END
GO

ALTER PROCEDURE [dbo].[report_filter_camp_events_crossroads]
  @DomainID varchar(40)
 ,@UserID varchar(40)
 ,@PageID int
AS
    
	DECLARE @Domain_ID int = (SELECT Domain_ID FROM dp_Domains WHERE CAST(Domain_GUID as varchar(40)) = @DomainID)

	SELECT DISTINCT E.Event_ID, E.Event_Title
	FROM Events AS E
	LEFT JOIN cr_Event_Waivers AS W
	ON E.Event_ID = W.Event_ID
	WHERE W.Event_Waiver_ID IS NOT NULL AND E.Domain_ID = @Domain_ID
	ORDER BY Event_Title;
	
GO