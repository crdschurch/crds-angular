USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[cr_GetRejectedRoomReservations]    Script Date: 12/05/2017 11:27:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.cr_GetRejectedRoomReservations') IS NULL -- Check if SP Exists
        EXEC('CREATE PROCEDURE dbo.cr_GetRejectedRoomReservations AS SET NOCOUNT ON;') -- Create dummy/empty SP
GO

-- =============================================
-- Author:		Ken Baum
-- Create date: 10/11/2017
-- Description:	Return RejectedRoomReservations to send an email to the requestor
-- Modified: 12/05/2017
-- Description: Add code to set approval flag for room reservation to false when
--              room is rejected
-- =============================================
ALTER PROCEDURE [dbo].[cr_GetRejectedRoomReservations] 
AS
BEGIN

	SET NOCOUNT ON;
	IF OBJECT_ID('tempdb..#Temp') IS NOT NULL DROP Table #Temp

	-- Store tasks that are not completed and are rejected into a temp table
    SELECT t.Task_ID,
			e.Primary_Contact as [Requestor_Contact_ID],
		   r.Room_Name,
		   e.Event_Start_Date,
		   er.Event_Room_ID,
		   er._Approved,
           e.Event_Title,
		   t.Description as [Task_Rejection_Reason]
	INTO #Temp
	FROM
        dp_Tasks t
        INNER JOIN Event_Rooms AS er ON er.Event_Room_ID = t._Record_ID AND t._Page_ID = 384
        INNER JOIN Events AS e ON e.Event_ID = er.Event_ID
		INNER JOIN Rooms AS r ON er.Room_ID = r.Room_ID
    WHERE
        t.Completed = 0 
        AND t._Rejected = 1 
		AND e.Cancelled = 0
        AND e.Event_End_Date > GETDATE();

	-- Mark the tasks as completed
	UPDATE [dbo].[dp_Tasks] 
	SET Completed = 1
    WHERE
        Task_ID in (SELECT Task_ID from #Temp)

    --Set rejected rooms Approval = false
	update dbo.Event_Rooms
	set _Approved = 0
	from dbo.Event_Rooms
		where Event_Room_ID in (
			select Event_Room_ID from #Temp
		)

	-- Return the data for the emails
	SELECT * from #Temp

END
GO
-- setup permissions for API User in MP
DECLARE @procName nvarchar(100) = N'cr_GetRejectedRoomReservations'
DECLARE @procDescription nvarchar(100) = N'Retrieves the list of room reservations that have been rejected for events that have not been cancelled.'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName)
BEGIN
        INSERT INTO [dbo].[dp_API_Procedures] (
                 Procedure_Name
                ,Description
        ) VALUES (
                 @procName
                ,@procDescription
        )
END

DECLARE @API_ROLE_ID int = 62;
DECLARE @API_ID int;

SELECT @API_ID = API_Procedure_ID FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_API_Procedures] WHERE [Role_ID] = @API_ROLE_ID AND [API_Procedure_ID] = @API_ID)
BEGIN
        INSERT INTO [dbo].[dp_Role_API_Procedures] (
                 [Role_ID]
                ,[API_Procedure_ID]
                ,[Domain_ID]
        ) VALUES (
                 @API_ROLE_ID
                ,@API_ID
                ,1
        )
END
GO
