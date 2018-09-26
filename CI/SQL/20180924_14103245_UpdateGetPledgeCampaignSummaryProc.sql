USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[api_crds_Get_Pledge_Campaign_Summary]    Script Date: 9/23/2018 5:33:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 04/19/2017
-- This procedure provides data for an overview of total giving vs. commitments for
-- a pledge campaign.  This supports the /leaveyourmark page on CR.net.
--
-- Pledge_Campaign_Id (required) is the Id of the pledge campaign to query.
-- On_Pace_Min_Percent_Grace and On_Pace_Max_Percent_Grace control the window size
-- of the percentage range that is considered On Pace.  For example, if the campaign
-- is 42% complete (based on days elapsed), and the min/max Grace period is 10% for
-- both, then giving that falls between 32% - 52% is considered On Pace.
ALTER PROCEDURE [dbo].[api_crds_Get_Pledge_Campaign_Summary] (
	@Pledge_Campaign_Id INT,
	@On_Pace_Min_Percent_Grace FLOAT = 10.0,
	@On_Pace_Max_Percent_Grace FLOAT = 10.0
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Current_Date DATETIME = CONVERT(DATE, GETDATE())

	DECLARE @Start_Date DATE;
	DECLARE @End_Date DATE;
	DECLARE @Program_Id INT;

	-- 09/24/2018
	-- Added to handle very specific case of allowing I'm In campaign to have two different end dates
	DECLARE @Campaign_Part_1_Identifier varchar = 'I`m In'
	DECLARE @Campaign_Part_2_Identifier varchar = 'Obsessed'
	DECLARE @Campaign_Part_2_Start_Date DATE = '2018-03-18'
	DECLARE @Campaign_Part_2_End_Date DATE = '2019-12-31'		

	SELECT
		@Start_Date = pc.Start_Date,
		@End_Date = pc.End_Date,
		@Program_Id = pc.Program_Id
	FROM
		Pledge_Campaigns pc
	WHERE
		pc.Pledge_Campaign_ID = @Pledge_Campaign_Id
	;

	-- Return 0 rows if the campaign does not exist, or dates are invalid
	IF @End_Date >= @Start_Date 
	BEGIN
		-- On_Pace_Percent is the amount of time (based on days) elapsed in the
		-- campaign so far expressed as a percentage.  This is the ideal.
		DECLARE @On_Pace_Percent FLOAT =
				CASE
					WHEN @Current_Date > @End_Date THEN 100			-- past the campaign end date
					WHEN @Current_Date < @Start_Date THEN 0			-- before the campaign start date
					ELSE -- DaysUsed / DaysAvailable
						100.0 * DATEDIFF(DAY, @Start_Date, DATEADD(DAY, 1, @Current_Date)) / 
						DATEDIFF(DAY, @Start_Date, DATEADD(DAY, 1, @End_Date))
				END;
		DECLARE @On_Pace_Percent_Part_2 FLOAT =
				CASE
					WHEN @Current_Date > @Campaign_Part_2_End_Date THEN 100			-- past the campaign end date
					WHEN @Current_Date < @Start_Date THEN 0			-- before the campaign start date
					ELSE -- DaysUsed / DaysAvailable
						100.0 * DATEDIFF(DAY, @Start_Date, DATEADD(DAY, 1, @Current_Date)) / 
						DATEDIFF(DAY, @Start_Date, DATEADD(DAY, 1, @Campaign_Part_2_End_Date))
				END;
		-- On_Pace_Min_Percent and On_Pace_Max_Percent is a bounding range that
		-- surrounds On_Pace_Percent and represents an additional grace period that
		-- we will allow and still consider as On Pace.
		DECLARE @On_Pace_Min_Percent FLOAT = @On_Pace_Percent - @On_Pace_Min_Percent_Grace;
		DECLARE @On_Pace_Max_Percent FLOAT = @On_Pace_Percent + @On_Pace_Max_Percent_Grace;
		DECLARE @On_Pace_Min_Percent_Part_2 FLOAT = @On_Pace_Percent_Part_2 - @On_Pace_Min_Percent_Grace;
		DECLARE @On_Pace_Max_Percent_Part_2 FLOAT = @On_Pace_Percent_Part_2 + @On_Pace_Max_Percent_Grace;

		-- Count and total dollar amount of donations made to the campaign outside of a pledge
		DECLARE @No_Commitment_Count INT;
		DECLARE @No_Commitment_Amount MONEY;

		SELECT
			@No_Commitment_Count = COUNT(DISTINCT d.Donor_Id),
			@No_Commitment_Amount = SUM(dd.Amount)
		FROM
			Donations d
			INNER JOIN Donation_Distributions dd ON dd.Donation_Id = d.Donation_Id
		WHERE
			d.Donation_Status_Id = 2		-- Deposited
			AND dd.Program_Id = @Program_Id
			AND dd.Pledge_Id IS NULL
		;

		-- Get per pledge data
		; WITH PledgeData (Pledge_Id, Total_Committed, Total_Given, part, Percent_Money)
		AS
		(
			SELECT
				p.Pledge_Id,
				p.Total_Pledge,
				agg.Total_Given,
				Divide =
					CASE
						WHEN p.First_Installment_Date < @Campaign_Part_2_Start_Date THEN @Campaign_Part_1_Identifier
						ELSE @Campaign_Part_2_Identifier
					END,
				PercentMoney =		-- percent of commitment met so far
					CASE
						WHEN p.Total_Pledge > 0 AND agg.Total_Given IS NOT NULL THEN 100.0 * agg.Total_Given / p.Total_Pledge
						ELSE 0
					END
			FROM
				Pledges p
				INNER JOIN (
					SELECT
						p.Pledge_Id,
						Total_Given = SUM(CASE WHEN d.Donation_Status_Id = 2 THEN dd.Amount ELSE 0 END)
					FROM
						Pledges p
						LEFT JOIN Donation_Distributions dd ON dd.Pledge_Id = p.Pledge_Id
						LEFT JOIN Donations d ON d.Donation_Id = dd.Donation_Id
					WHERE
						p.Pledge_Campaign_Id = @Pledge_Campaign_Id
						AND p.Pledge_Status_Id IN (1, 2)	-- Active, Completed
					GROUP BY
						p.Pledge_Id
				) AS agg ON agg.Pledge_Id = p.Pledge_Id
		)
	
		SELECT
			Start_Date =
				CASE
					WHEN part = @Campaign_Part_2_Identifier THEN @Campaign_Part_2_Start_Date
					ELSE @Start_Date -- if there aren't parts this should work the way it did before
				END,
			End_Date =
				CASE
					WHEN part = @Campaign_Part_2_Identifier THEN @Campaign_Part_2_End_Date
					ELSE @End_Date -- if there aren't parts this should work the way it did before
				END,
			-- non-pledges
			No_Commitment_Count =
				CASE
					WHEN part = @Campaign_Part_2_Identifier THEN '0' -- Later update this with values after original end date?
					ELSE @No_Commitment_Count -- if there aren't parts this should work the way it did before
				END,
			No_Commitment_Amount =
				CASE
					WHEN part = @Campaign_Part_2_Identifier THEN '0' 
					ELSE @No_Commitment_Amount -- if there aren't parts this should work the way it did before
				END,
			-- pledge info below
			Total_Given = SUM(Total_Given),
			Total_Committed = SUM(Total_Committed),
			Not_Started_Count = SUM(CASE WHEN Bucket = 1 THEN 1 ELSE 0 END),
			Behind_Count = SUM(CASE WHEN Bucket = 2 THEN 1 ELSE 0 END),
			On_Pace_Count = SUM(CASE WHEN Bucket = 3 THEN 1 ELSE 0 END),
			Ahead_Count = SUM(CASE WHEN Bucket = 4 THEN 1 ELSE 0 END),
			Completed_Count = SUM(CASE WHEN Bucket = 5 THEN 1 ELSE 0 END),
			Total_Count = COUNT(*),
			part
		FROM
			(
				-- Assign a bucket for each pledge:
				--   1 = Not Started
				--   2 = Behind Pace
				--   3 = On Pace
				--   4 = Ahead Of Pace
				--   5 = Completed
				-- The case statement below is in order of priority so that each row
				-- is assigned to exactly one bucket that is the most appropriate.
				SELECT
					*,
					Bucket =
							CASE
								WHEN COALESCE(Total_Given, 0) >= Total_Committed THEN 5	-- completed
								WHEN COALESCE(Total_Given, 0) <= 0 THEN 1	-- not started
								WHEN (Percent_Money < @On_Pace_Min_Percent) AND part = @Campaign_Part_1_Identifier THEN 2	-- behind pace
								WHEN (Percent_Money > @On_Pace_Max_Percent) AND part = @Campaign_Part_1_Identifier THEN 4	-- ahead of pace
								WHEN (Percent_Money < @On_Pace_Min_Percent_Part_2) AND part = @Campaign_Part_2_Identifier THEN 2	-- behind pace
								WHEN (Percent_Money > @On_Pace_Max_Percent_Part_2) AND part = @Campaign_Part_2_Identifier THEN 4	-- ahead of pace
								ELSE 3	-- on pace
							END
				FROM
					PledgeData
			) AS PledgeDataWithBucket
		GROUP BY part
		ORDER BY part
		;
	END -- IF
END
