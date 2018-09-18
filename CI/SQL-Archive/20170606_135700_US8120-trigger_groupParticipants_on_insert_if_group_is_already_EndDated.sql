USE [MinistryPlatform]
GO

USE [MinistryPlatform]
GO
/****** Object:  Trigger [dbo].[tr_End_Date_Group_Participant_If_Group_Is_End_Dated]    Script Date: 6/6/2017 1:52:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Katie Dwyer
-- Create date: 2017-06-06
-- Description: end dates group participants when they are created as part of a group that has an end date
-- =============================================
CREATE TRIGGER [dbo].[crds_tr_End_Date_Group_Participant_If_Group_Is_End_Dated] 
   ON  [dbo].[Group_Participants] 
   AFTER INSERT
AS 
BEGIN

	 -- SET NOCOUNT ON added to prevent extra result sets from
	 -- interfering with SELECT statements.
	 SET NOCOUNT ON;
	 UPDATE Group_Participants 
	 SET End_Date = g.End_Date
	 FROM Group_Participants gp
	 JOIN Groups g on gp.group_id = g.group_id
	 JOIN  INSERTED ON INSERTED.Group_Participant_ID = gp.Group_Participant_ID
	 WHERE g.End_Date IS NOT NULL AND gp.End_Date IS NULL

 END