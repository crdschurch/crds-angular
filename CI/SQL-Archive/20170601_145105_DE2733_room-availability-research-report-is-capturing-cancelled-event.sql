USE [MinistryPlatform]
GO

CREATE PROCEDURE [dbo].[report_CRDS_Rooms_Available]

	@DomainID varchar(40)
	,@UserID varchar(40)
	,@PageID Int
	,@StartDate DateTime
	,@EndDate DateTime
	,@Time Int
	,@EndTime Int
	,@BuildingID Int
	,@DayofWeek INT = NULL
	,@RoomsID varchar(MAX) = '0'

AS
BEGIN

DECLARE @Blocks Int, @Hours Int
SET @Hours = DATEDIFF(hh,DateAdd(hh, @Time, @StartDate),DateAdd(hh, @EndTime, @EndDate))

SET @Blocks = @Hours*2

DECLARE @Hrs Table (HrStart DateTime,HrEnd DateTime)

		WHILE @Blocks > 0
		BEGIN 
		INSERT INTO @Hrs VALUES (DateAdd(n,(@Blocks-1)*30,(DateAdd(hh, @Time, @StartDate))),DateAdd(n,@Blocks*30,(DateAdd(hh, @Time, @StartDate))))
		SET @Blocks = @Blocks - 1
		END

SELECT @StartDate AS StartDate
, Booked = ISNULL((
		SELECT SUM(Convert(Int, ER._Approved))
		FROM Event_Rooms ER
			INNER JOIN Events E
				ON E.Event_ID = ER.Event_ID
			WHERE ER.Room_ID = List.Room_ID
				AND ER.Cancelled = 0
			AND (
				List.HRStart BETWEEN E.__Reservation_Start AND E.__Reservation_End
					OR
				List.HREnd BETWEEN E.__Reservation_Start AND E.__Reservation_End
)), -1)
, List.HrStart
, List.HrEnd
, List.Room_Name
, List.Room_ID
, List.Room_Number
, List.RowNum
, B.Building_Name
, B.Building_ID
FROM (SELECT H.HrStart, H.HrEnd, R.Room_Name, Room_ID, Building_ID, Room_Number, Row_Number() OVER (PARTITION BY Room_ID ORDER BY HrStart) AS RowNum
		FROM @Hrs H CROSS JOIN Rooms R 
		WHERE R.Building_ID = @BuildingID
		AND Bookable = 1) List
 INNER JOIN Buildings B ON List.Building_ID = B.Building_ID
 INNER JOIN dp_Domains Dom ON Dom.Domain_ID = B.Domain_ID
WHERE Dom.Domain_GUID = @DomainID
AND DATEPART(DW,List.HrStart) = ISNULL(@DayofWeek,DATEPART(DW,List.HrStart))
AND (DATEPART(hh,List.HrStart) >= @Time 
		AND CASE WHEN DATEPART(hh,List.HrEnd) = 0 THEN 24 ELSE DATEPART(hh,List.HrEnd) END <= CASE WHEN @EndTime = 0 THEN 24 ELSE @EndTime END)
AND (List.Room_ID = @RoomsID OR @RoomsID = '0')
ORDER BY List.RowNum

END
GO
