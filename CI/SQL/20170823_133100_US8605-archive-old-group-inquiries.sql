IF EXISTS ( SELECT * FROM   sysobjects WHERE  id = 	  object_id(N'[dbo].[api_crds_Archive_Pending_Group_Inquiries_Older_Than_90_Days]') and OBJECTPROPERTY(id, N'IsProcedure') = 1 )

BEGIN
    DROP PROCEDURE [dbo].[api_crds_Archive_Pending_Group_Inquiries_Older_Than_90_Days]
END
GO

CREATE PROCEDURE [dbo].[api_crds_Archive_Pending_Group_Inquiries_Older_Than_90_Days] 

AS

BEGIN
	
	UPDATE [MinistryPlatform].[dbo].[Group_Inquiries]
	SET Archived_Date = CURRENT_TIMESTAMP
	WHERE Placed IS NULL 
	  AND Archived_Date IS NULL
	  AND DATEADD(dd,90,Inquiry_Date) < CURRENT_TIMESTAMP;

END