USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[api_crds_GetReservedAndAvailableRoomsByLocation]    Script Date: 12/07/2016 09:05:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_GetReservedAndAvailableRoomsByLocation]') AND type in (N'P', N'PC'))
BEGIN
  EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_GetReservedAndAvailableRoomsByLocation] AS'
END
GO

-- =============================================
-- Author:       Ken Baum
-- Create date:  2016-12-07
-- Description:  Returns all bookable rooms by 
--               location id, plus whether they 
--               are currently reserved for a 
--               particular time or not
-- =============================================
ALTER PROCEDURE [dbo].[api_crds_GetReservedAndAvailableRoomsByLocation]
	@StartDate DateTime,
	@EndDate DateTime,
	@LocationId int = 3
AS
BEGIN

	WITH Reserved_Rooms (Room_ID, RoomStatus)
	AS
	(
		SELECT DISTINCT r.Room_ID, 
						er._Approved AS RoomStatus					
		FROM dbo.Events e
			inner join dbo.Event_Rooms er on e.Event_ID = er.Event_ID
			inner join dbo.Rooms r on er.Room_ID = r.Room_ID
		WHERE ( (e.Event_Start_Date >= @StartDate AND e.Event_Start_Date < @EndDate)
			OR (e.Event_End_Date > @StartDate AND e.Event_End_Date <= @EndDate) )
			AND e.Location_ID = @LocationId
	)
	SELECT DISTINCT r.Room_ID AS RoomId, 
					r.Room_Name AS RoomName, 
					r.Room_Number AS RoomNumber, 
					b.Building_ID AS BuildingId, 
					b.Building_Name AS BuildingName, 
					b.Location_ID AS LocationId, 
					r.Description, 
					ISNULL(r.Theater_Capacity, 0) AS TheaterCapacity, 
					ISNULL(r.Banquet_Capacity,0) AS BanquetCapacity, 
					CASE
						WHEN (rr.RoomStatus IS NOT NULL)
							THEN CASE
								WHEN rr.RoomStatus = 0 THEN '0'
								WHEN rr.RoomStatus = 1 THEN '1'
						    END
						ELSE rr.RoomStatus)
					END AS RoomStatus
	FROM dbo.Rooms r
		INNER JOIN dbo.Buildings b on b.Building_ID = r.Building_ID
		LEFT OUTER JOIN Reserved_Rooms rr on rr.Room_ID = r.Room_ID
	WHERE r.Bookable = 1 
			and b.Location_ID = @LocationId
	ORDER BY r.Room_ID

END
