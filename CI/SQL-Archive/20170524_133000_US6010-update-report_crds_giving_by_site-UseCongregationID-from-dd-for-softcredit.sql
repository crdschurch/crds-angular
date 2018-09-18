USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Giving_By_Site]    Script Date: 5/24/2017 1:30:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[report_CRDS_Giving_By_Site] 
     @startdate DATETIME ,
     @enddate DATETIME,
	 @programid AS VARCHAR(MAX) ,
	 @congregationid AS VARCHAR(MAX),
	 @donationstatusid as varchar(MAX)
AS 
BEGIN
SET nocount ON;


--SET DATEFIRST 1;
--SET @startdate = DATEADD(dd, DATEDIFF(dd, 0, @startdate),0);
--SET @enddate   =  DATEADD(mi,1439, (DATEADD(dd, DATEDIFF(dd, 0, @enddate),0)));

--Per Finance - change is needed to capture millisecond data on donations
SET DATEFIRST 1;
SET @startdate = DATEADD(dd, DATEDIFF(dd, 0, @startdate),0);
SET @enddate   =  DATEADD(ms,86399998, (DATEADD(dd, DATEDIFF(dd, 0, @enddate),0)))


IF OBJECT_ID('tempdb..#GIVING') IS NOT NULL   
	DROP TABLE #GIVING

CREATE TABLE #GIVING
(
	congregation_name			varchar(50),
	Total						money,
	MonthLabel					varchar(50),
	MonthNumber					int,
	YearNumber					int,
	program_name				varchar(50),
	soft_credit_donor			int
)

INSERT INTO #GIVING
	SELECT 
	  COALESCE(con.congregation_name,'Not Specified') AS congregation_name,
	  dd.amount AS Total,
	  DATENAME(mm,don.donation_date) MonthLabel,
	  DATEPART(mm,don.donation_date) MonthNumber,
	  DATEPART(YEAR,don.donation_date) YearNumber,
	  p.program_name as program_name,
	  dd.soft_credit_donor
	FROM donations don 
	JOIN donors d ON d.donor_id = don.donor_id
	JOIN Donation_Distributions dd on dd.donation_id= don.donation_id
	JOIN contacts c ON c.contact_id = d.contact_id
	JOIN programs p on p.program_id = dd.program_id
	LEFT JOIN congregations con ON con.congregation_id = dd.congregation_id
	WHERE don.donation_date BETWEEN @startdate AND @enddate
	AND dd.program_id IN (SELECT Item FROM dbo.dp_Split(@programid, ','))
	AND dd.congregation_id IN (SELECT Item FROM dbo.dp_Split(@congregationid, ','))
	AND don.donation_status_id IN (SELECT Item FROM dbo.dp_Split(@donationstatusid, ','))


	select congregation_name,
		SUM(Total) as Total,
		MonthLabel,
		MonthNumber,
		YearNumber,
		program_name 
		from #GIVING 
		GROUP BY congregation_name, MonthLabel,MonthNumber,YearNumber,program_name
		ORDER BY program_name,YearNumber, MonthNumber , congregation_name


DROP TABLE #GIVING
END


GO


