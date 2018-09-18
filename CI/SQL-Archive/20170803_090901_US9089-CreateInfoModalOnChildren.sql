USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_Get_Manage_Children_data]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_Get_Manage_Children_data] AS' 
END
GO
-- =============================================
-- Author:      Dustin Kocher
-- Create date: 2017-08-08
-- Description: Alter api_crds_Get_Manage_Children_data to more contact and household
-- fields
-- =============================================
ALTER PROCEDURE [dbo].[api_crds_Get_Manage_Children_data]
	@EventId INT = 4529783,
	@Search NVARCHAR(200) = NULL
AS
BEGIN
	
	-- Get the event data for the Event ID - either the parent or the ac event
	DECLARE @Events TABLE
	(
		Event_ID INT
	)

	INSERT INTO @Events (Event_ID)
		SELECT DISTINCT Event_ID
		FROM [dbo].[Events]
		WHERE Event_ID = @EventID OR Parent_Event_ID = @EventID
		
	DECLARE @Children TABLE
	(
		Event_ID INT,
		Event_Participant_ID INT,
		Participation_Status_ID INT,
		First_Name VARCHAR(50),
		Last_Name VARCHAR(50),
		Nickname VARCHAR(50),
		Call_Number INT,
		Room_ID INT,
		Room_Name NVARCHAR(50),
		Time_In DATETIME,
		Time_Confirmed DATETIME,
		Checkin_Household_ID INT,
		Household_ID INT,
		KC_Sort_Order INT
	)

	-- Set numeric search value for the case they are searching by call number
	Declare @CallNumberSearch as INT
	
	SELECT @CallNumberSearch =
		CASE
			WHEN ISNUMERIC(@Search) = 1 THEN CONVERT(INT, @Search)
			ELSE NULL
		END

	-- Get the children for this event
	-- If no Search parameters return them all
	-- Else return just the ones that work with the search
	INSERT INTO @Children ( Event_ID, Event_Participant_ID, Participation_Status_ID, First_Name, Last_Name, Nickname, Call_Number, Room_ID, Room_Name, Time_In, Time_Confirmed, Checkin_Household_ID, Household_ID, KC_Sort_Order)
		SELECT DISTINCT ep.Event_ID, ep.Event_Participant_ID, ep.Participation_Status_ID, c.First_Name, c.Last_Name, c.Nickname,
			ep.Call_Number, ep.Room_ID, r.Room_Name, ep.Time_In, ep.Time_Confirmed, ep.Checkin_Household_ID, c.Household_ID, ISNULL(KC_Sort_Order, 1000)
		FROM [dbo].[Event_Participants] ep
		INNER JOIN [dbo].[Participants] p ON p.Participant_ID = ep.Participant_ID
		INNER JOIN [dbo].[Contacts] c ON c.Contact_ID = p.Contact_ID
		INNER JOIN [dbo].[Rooms] r ON r.Room_ID = ep.Room_ID
		WHERE ep.Event_ID IN (SELECT EVENT_ID from @Events)
			AND ep.End_Date IS NULL
			AND ep.Call_Number IS NOT NULL
			AND ep.Checkin_Household_ID IS NOT NULL
			AND ep.Participation_Status_ID IN (3,4)
			AND (@Search IS NULL OR
					(c.First_name LIKE '%' + @Search + '%'
						OR c.Last_Name LIKE '%' + @Search + '%'
						OR c.Nickname LIKE '%' + @Search + '%'
						OR (@CallNumberSearch IS NOT NULL AND ep.Call_Number = @CallNumberSearch)))

	DECLARE @Household TABLE
	(
		Household_ID INT,
		First_Name VARCHAR(50),
		Last_Name VARCHAR(50),
		Nickname VARCHAR(50),
		Mobile_Phone NVARCHAR(25),
		Home_Phone NVARCHAR(25),
		Address_Line_1 NVARCHAR(75),
		Address_Line_2 NVARCHAR(75),
		City NVARCHAR(50),
		State NVARCHAR(50),
		Postal_Code NVARCHAR(15),
		Email_Address VARCHAR(255)
	)

	-- Return Head of Household that belong to the children we got above
	INSERT INTO @Household ( Household_ID, First_Name, Last_Name, Nickname, Mobile_Phone,
	 Home_Phone, Address_Line_1, Address_Line_2, City, State, Postal_Code, Email_Address)
		SELECT DISTINCT hc.Household_ID, hc.First_Name, hc.Last_Name, hc.Nickname,
			hc.Mobile_Phone, h.Home_Phone, a.Address_Line_1, a.Address_Line_2, a.City,
			a.[State/Region] as State, a.Postal_Code, Email_Address
		FROM [dbo].Contacts hc
		INNER JOIN Households h ON hc.Household_ID = h.Household_ID
		INNER JOIN Addresses a on h.Address_ID = a.Address_ID
		INNER JOIN @Children c ON hc.Household_ID = c.Checkin_Household_ID
		WHERE hc.Household_Position_ID IN (1, 7)

	SELECT * FROM @Children
	SELECT * FROM @Household

END
