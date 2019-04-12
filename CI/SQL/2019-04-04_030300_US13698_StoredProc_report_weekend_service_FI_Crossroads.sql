USE [MinistryPlatform]
GO

-- =======================================================================================
-- Author:      Shakila Rajaiah
-- Create date: 4/5/19
-- Description: This stored proc generates a list of volunteers to serve with the FI teams
-- =======================================================================================

CREATE OR ALTER PROCEDURE [dbo].[report_weekend_service_FI_Crossroads](
	@DomainID VARCHAR(40)
	,@UserID VARCHAR(40)
	,@PageID INT
	,@FromDate DATETIME
	,@ToDate DATETIME
	,@Congregation NVARCHAR(MAX)='0'
	,@NumOfColumns INT = 6
	,@NumOfRows INT = 26
	,@Groups NVARCHAR(MAX)='0'
	)

AS 
BEGIN

/**** Get Congregations ****/
CREATE TABLE #Congregations (Congregation_ID INT, Congregation_Name NVARCHAR(75))
INSERT  #Congregations (Congregation_ID, Congregation_Name)
        SELECT CAST(Item AS INT), Con.Congregation_Name FROM dp_Split(@Congregation, ',') SC INNER JOIN Congregations Con ON CAST(Item AS INT) = Con.Congregation_ID
		UNION
		SELECT Congregation_ID, Congregation_Name FROM Congregations WHERE ISNULL(@Congregation,'0') = '0'

/**** Get Groups for First Impressions ****/
CREATE TABLE #GroupsFI (Group_ID INT, Group_Name NVARCHAR(75),Group_Sort_Order INT)
INSERT  #GroupsFI (Group_ID,Group_Name,Group_Sort_Order)
        SELECT CAST(Item AS INT),G.Group_Name,G.KC_Sort_Order FROM dp_Split(@Groups, ',') SG INNER JOIN Groups G ON CAST(Item AS INT) = G.Group_ID
	UNION
	SELECT Group_ID,Group_Name, KC_Sort_Order FROM Groups WHERE ISNULL(@Groups,'0') = '0' 

SELECT E.Event_ID
	, E.Event_Title
	, E.Event_Start_Date
	, ET.Event_Type_Id
	, ET.Event_Type
	, G.Group_Name
	, O.Opportunity_Title
	, O.Program_ID
	, C.Contact_ID
	, RTRIM(COALESCE(C.Nickname + ' ','') +  COALESCE(C.Last_Name + ' ', '')) + CASE WHEN (SELECT DATEDIFF(HOUR,  C.Date_of_Birth, GETDATE())/8766) < 18 THEN ' (SV)' ELSE '' END AS FULLNAME
	, C.Nickname
	, C.Last_Name
	, C.Date_of_Birth
	, O.Opportunity_ID
    , ISNULL(O.Maximum_Needed,0) Max_Needed
    , ISNULL(O.Minimum_Needed,0) Min_Needed
    , O.Shift_Start
    , O.Shift_End
    , O.Room
    , G.Group_ID
    , Cong.Congregation_Name
	, R.Response_ID
	, R.Response_Date
	, RowNum = ROW_NUMBER() OVER (PARTITION BY O.Opportunity_ID, E.Event_ID ORDER BY E.Event_ID,R.Response_Date)
	, Position = 'MAX'
	, DisplayColumn = CAST(0 AS INT)
	, GroupRow = CAST (0 AS INT)
	, RowGroupMax = CAST (0 AS INT)
	, ISNULL(G.Group_Sort_Order,999) AS Sort_Order
INTO #ReportData
  FROM #GroupsFI G
  INNER JOIN Opportunities O ON G.Group_ID = O.Add_to_Group AND O.Group_Role_ID IN (16,22)
  INNER JOIN Event_Types ET ON O.Event_Type_ID = ET.Event_Type_ID
  INNER JOIN Events E ON E.Event_Type_ID = ET.Event_Type_ID
  INNER JOIN #Congregations Cong ON Cong.Congregation_ID = E.Congregation_ID
  LEFT JOIN Responses R ON R.Opportunity_ID = O.Opportunity_ID AND R.Event_ID = E.Event_ID AND R.response_result_id=1
  LEFT JOIN Participants P ON P.Participant_ID = R.Participant_ID
  LEFT JOIN Contacts C ON C.Contact_ID = P.Contact_ID  
WHERE CAST(Event_Start_Date AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)			
  ORDER BY E.Event_Start_Date, O.Opportunity_Title, C.Last_Name 


--ADD PLACEHOLDER IF THERE ARE NO VOLUNTEERS 
SELECT NumPosToAdd = Max_Needed - ISNULL((SELECT MAX(RowNum) FROM #ReportData RD WHERE #ReportData.Opportunity_ID = RD.Opportunity_ID AND #ReportData.Event_ID = RD.Event_ID),0)
	, Event_ID
	, Event_Title
	, Event_Start_Date
	, Event_Type_Id
	, Event_Type
	, Group_Name
	, Opportunity_Title
	, Program_ID
	, Opportunity_ID
    	, Max_Needed
    	, Min_Needed
    	, Shift_Start
    	, Shift_End
    	, Room
    	, Group_ID
    	, Congregation_Name
	, Sort_Order
INTO #AddPos
FROM #ReportData 
WHERE Max_Needed > ISNULL((SELECT MAX(RowNum) FROM #ReportData RD WHERE #ReportData.Opportunity_ID = RD.Opportunity_ID AND #ReportData.Event_ID = RD.Event_ID),0)
GROUP BY  Event_ID
	, Event_Title
	, Event_Start_Date
	, Event_Type_Id
	, Event_Type
	, Group_Name
	, Opportunity_Title
	, Program_ID
	, Opportunity_ID
        , Max_Needed
        , Min_Needed
        , Shift_Start
        , Shift_End
        , Room
        , Group_ID
        , Congregation_Name
	, Sort_Order

DECLARE @CurrPos INT = 1
		,@MaxPos INT = (SELECT MAX(NumPosToAdd) from #AddPos)+1

WHILE   @CurrPos < @MaxPos
BEGIN
 INSERT INTO #ReportData
			(Event_ID
			, Event_Title
			, Event_Start_Date
			, Event_Type_Id
			, Event_Type
			, Group_Name
			, Opportunity_Title
			, Program_ID
			, Contact_ID
			, FULLNAME
			, Nickname
			, Last_Name
			, Date_of_Birth
			, Opportunity_ID
			, Max_Needed
			, Min_Needed
			, Shift_Start
			, Shift_End
			, Room
			, Group_ID
			, Congregation_Name
			, Response_ID
			, Response_Date
			, RowNum
			, Position
			, DisplayColumn
			, Sort_Order
			)
		SELECT Event_ID
			, Event_Title
			, Event_Start_Date
			, Event_Type_Id
			, Event_Type
			, Group_Name
			, Opportunity_Title
			, Program_ID
			, 0
			, ''
			, ''
			, ''
			, NULL
			, Opportunity_ID
			, Max_Needed
			, Min_Needed
			, Shift_Start
			, Shift_End
			, Room
			, Group_ID
			, Congregation_Name
			, 0
			, NULL
			, (Max_Needed + 1) - @CurrPos
			, 'MAX'
			, 0
			, Sort_Order
		FROM #AddPos
		WHERE NumPosToAdd >= @CurrPos

 SET @CurrPos= @CurrPos + 1

END	

  UPDATE RD
  SET Position = 'MIN'
  FROM #ReportData RD
  WHERE RowNum <= Min_Needed

  UPDATE #ReportData
  SET DisplayColumn =  RD.DC, GroupRow = RD.GR
  FROM #ReportData
  INNER JOIN (SELECT Event_Type_ID,Event_ID, Sort_Order
							,DC = (ROW_NUMBER() OVER (PARTITION BY Event_Type_ID,Event_ID ORDER BY Event_Type_ID,Event_ID,Sort_Order) + @NumOfColumns - 1) % @NumOfColumns + 1 
							,GR = FLOOR ((ROW_NUMBER() OVER (PARTITION BY Event_Type_ID,Event_ID ORDER BY Event_Type_ID,Event_ID,Sort_Order) + @NumOfColumns - 1) / @NumOfColumns  )
							
						FROM #ReportData 
						GROUP BY Event_Type_ID,Event_ID,Sort_Order
						) RD ON #ReportData.Event_Type_ID = RD.Event_Type_ID 
							AND #ReportData.Event_ID = RD.Event_ID
							AND #ReportData.Sort_Order = RD.Sort_Order

--get the total rows for the group
  SELECT *
  INTO #TotalRowsForGroup
  FROM (
			SELECT Congregation_Name
			  ,Event_Type_ID
			  ,Event_Type
			  ,Event_Title
			  ,Event_Start_Date
			  ,Event_ID		
			  ,GroupRow
			  ,Group_Name
			  ,(COUNT(*) + COUNT(DISTINCT Opportunity_Title)) AS TotalRowsForGroup
			  ,Sort_Order
			  ,DisplayColumn
			  ,MAX(Opportunity_Title) AS Opportunity_Title 
			  FROM #ReportData 
			  GROUP BY Congregation_Name, Event_Type_ID, Event_Type, Event_Title, Event_Start_Date, Event_ID, GroupRow, Group_Name, Sort_Order, DisplayColumn	 
       ) AS TRG

  SELECT *
  INTO #GroupRowMax
  FROM (
			SELECT Congregation_Name, Event_Start_Date, GroupRow
			  ,MAX(TotalRowsForGroup) AS GroupRowMax --+1 is for the group name
			  FROM #TotalRowsForGroup
			  GROUP BY Congregation_Name, Event_Start_Date, GroupRow		 
       ) AS GRM

  SELECT *
  INTO #FillerCount
  FROM (
			SELECT TD.Congregation_Name, TD.Event_Type_ID, TD.Event_Type, TD.Event_Start_Date, TD.Event_ID, TD.Event_Title
			  ,TD.GroupRow, TD.Sort_Order, TD.DisplayColumn, TD.Group_Name, TD.Opportunity_Title 			 
			  ,(TD2.GroupRowMax- TD.TotalRowsForGroup) AS NumPosToAdd
			  ,TD2.GroupRowMax
	    	  FROM #TotalRowsForGroup TD
			  JOIN #GroupRowMax TD2 ON TD.Event_Start_Date = TD2.Event_Start_Date AND TD.GroupRow = TD2.GroupRow AND TD.Congregation_Name = TD2.Congregation_Name	 
       ) AS FC


 --Add the filler to line it up
 DECLARE @FilCount INT = 1
		,@MaxPosFiller INT = (SELECT MAX(NumPosToAdd) from #FillerCount)


WHILE @FilCount <= @MaxPosFiller
BEGIN
 INSERT INTO #ReportData
			( Congregation_Name
			, Program_ID
			, Event_ID
			, Event_Start_Date
			, Event_Type_ID
			, Event_Type
			, Event_Title
			, Group_Name
			, Opportunity_Title
			, RowNum
			, Position
			, DisplayColumn
			, Sort_Order
			, Opportunity_ID
			, Max_needed
			, Min_Needed
			)
		SELECT Congregation_Name 
		    , 0
		    , Event_ID  
			, Event_Start_Date
			, Event_Type_ID
			, Event_Type
			, Event_Title
			, Group_Name
			, Opportunity_Title			
			, (GroupRowMax - NumPosToAdd + @FilCount)
			, 'FIL'
			, DisplayColumn
			, Sort_Order
			, 0
			, 0
			, 0
		FROM #FillerCount
		WHERE NumPosToAdd >= @FilCount

 SET @FilCount= @FilCount + 1
END

  
  SELECT Event_ID
			, Event_Title
			, Event_Start_Date
			, Event_Type_Id
			, Event_Type
			, Group_Name
			, Opportunity_Title
			, Program_ID
			, Contact_ID
			, FULLNAME
			, Nickname
			, Last_Name
			, Date_of_Birth
			, Opportunity_ID
			, Max_Needed
			, Min_Needed
			, Shift_Start
			, Shift_End
			, Room
			, Group_ID
			, Congregation_Name
			, Response_ID
			, Response_Date
			, RowNum
			, Position
			, DisplayColumn
			, DisplayRow = (ROW_NUMBER() OVER (PARTITION BY Event_Type ORDER BY Event_Type,Sort_Order,DisplayColumn,Opportunity_Title, RowNum))-- + @NumOfRows - 1) % @NumOfRows + 1 
			, Sort_Order
			, PageName = REPLACE(RIGHT(convert(varchar, Event_Start_Date, 100),8),':',' ')
	FROM #ReportData RD
	ORDER BY Event_Type, Event_Start_Date, Sort_Order, DisplayColumn, Group_Name, Opportunity_Title, RowNum

    DROP TABLE #Congregations
	DROP TABLE #GroupsFI
	DROP TABLE #AddPos
	DROP TABLE #ReportData
	DROP TABLE #TotalRowsForGroup
    DROP TABLE #GroupRowMax
	DROP TABLE #FillerCount
END



GO


