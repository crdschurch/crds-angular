USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/26/2018
-- Description: Creates (if nonexistent) or Updates pledge campaign with given information
-- Output:      @campaign_id contains the pledge campaign id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Pledge_Campaign
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Pledge_Campaign')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Pledge_Campaign
	@campaign_name nvarchar(50),
	@campaign_type_id int,
	@goal money,
	@start_date datetime,
	@end_date datetime,
	@description nvarchar(500),
	@registration_start datetime,
	@registration_end datetime,
	@registration_deposit money,
	@registration_form_id int,
	@fundraising_goal money,
	@destination_id int,
	@youngest_age int,
	@program_name nvarchar(130),
	@event_name nvarchar(75),
	@nickname nvarchar(50),
	@maximum_registrants int,
	@error_message nvarchar(500) OUTPUT,
	@campaign_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Pledge_Campaign] 
	@campaign_name nvarchar(50),
	@campaign_type_id int,
	@goal money,
	@start_date datetime,
	@end_date datetime,
	@description nvarchar(500),
	@registration_start datetime,
	@registration_end datetime,
	@registration_deposit money,
	@registration_form_id int,
	@fundraising_goal money,
	@destination_id int,
	@youngest_age int,
	@program_name nvarchar(130),
	@event_name nvarchar(75),
	@nickname nvarchar(50),
	@maximum_registrants int,
	@error_message nvarchar(500) OUTPUT,
	@campaign_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @campaign_name is null
	BEGIN
		SET @error_message = 'Pledge Campaign name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @campaign_type_id = ISNULL(@campaign_type_id, 1); --Capital Campaign
	SET @goal = ISNULL(@goal, 1000);
	SET @start_date = ISNULL(@start_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]

	DECLARE @show_on_my_pledges bit = 1;
	IF @campaign_type_id <> 1 --Capital Campaign
		SET @show_on_my_pledges = 0;

	DECLARE @allow_online_pledge bit = 1;
	DECLARE @pledge_beyond_end_date bit = 1;
	IF @campaign_type_id = 2 --Trips
	BEGIN
		SET @allow_online_pledge = 0;
		SET @pledge_beyond_end_date = 0;
	END;
	

	--Optional fields
	SET @description = ISNULL(@description, @campaign_name);
	SET @nickname = ISNULL(@nickname, @campaign_name);

	DECLARE @external_trip_id int = 40; --No idea. 
	DECLARE @external_fund_id int = 99; --No idea.
	
	IF @event_name is not null
	BEGIN
		DECLARE @event_id int = (SELECT TOP 1 Event_ID FROM [dbo].Events WHERE Event_Title = @event_name ORDER BY Event_ID ASC);
		IF @event_id is null
			SET @error_message = 'Could not find event with name '+@event_name+'. Event not included in pledge campaign '+@campaign_name+CHAR(13);
	END
		
	IF @program_name is not null
	BEGIN
		DECLARE @program_id int = (SELECT TOP 1 Program_ID FROM [dbo].Programs WHERE Program_Name = @program_name ORDER BY Program_ID ASC);
		IF @program_id is null
			SET @error_message = 'Could not find program with name '+@program_name+'. Program not included in pledge campaign '+@campaign_name+CHAR(13);
	END
	
	--Create/Update Pledge Campaign
	SET @campaign_id = (SELECT TOP 1 Pledge_Campaign_ID FROM [dbo].Pledge_Campaigns WHERE Campaign_Name = @campaign_name);
	IF @campaign_id is null
	BEGIN
		INSERT INTO [dbo].Pledge_Campaigns 
		(Campaign_Name ,Pledge_Campaign_Type_ID,Campaign_Goal,Start_Date ,Domain_ID,Allow_Online_Pledge ,Pledge_Beyond_End_Date ,Show_On_My_Pledges ) VALUES
		(@campaign_name,@campaign_type_id      ,@goal        ,@start_date,1        ,@allow_online_pledge,@pledge_beyond_end_date,@show_on_my_pledges);

		SET @campaign_id = SCOPE_IDENTITY();
	END;
	
	IF @campaign_id is not null
	BEGIN
		UPDATE [dbo].Pledge_Campaigns
		SET Nickname = @nickname,
		Pledge_Campaign_Type_ID = @campaign_type_id,
		Description = @description,
		Campaign_Goal = @goal,
		Start_Date = @start_date,
		End_Date = @end_date,
		Event_ID = @event_id,
		Program_ID = @program_id,
		Destination_ID = @destination_id,
		Registration_Start = @registration_start,
		Registration_End = @registration_end,
		Youngest_Age_Allowed = @youngest_age,
		Registration_Deposit = @registration_deposit,
		Fundraising_Goal = @fundraising_goal,
		Registration_Form = @registration_form_id,
		Allow_Online_Pledge = @allow_online_pledge,
		Pledge_Beyond_End_Date = @pledge_beyond_end_date,
		Show_On_My_Pledges = @show_on_my_pledges,
		__ExternalTripID = @external_trip_id,
		__ExternalFundID = @external_fund_id,
		Maximum_Registrants = @maximum_registrants
		WHERE Pledge_Campaign_ID = @campaign_id;
	END
	ELSE
	BEGIN
		SET @error_message = @error_message+'Could not create Pledge Campaign '+@campaign_name+' for some reason'+CHAR(13);
		RETURN;
	END

	--Add pledge campaign to program
	IF @program_id is not null
	BEGIN
		UPDATE [dbo].Programs
		SET Pledge_Campaign_ID = @campaign_id
		WHERE Program_ID = @program_id;
	END;
END
GO