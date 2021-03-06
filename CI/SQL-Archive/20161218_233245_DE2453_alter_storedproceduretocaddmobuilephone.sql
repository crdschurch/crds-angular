USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[api_crds_CreateContact]    Script Date: 12/18/2016 11:15:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_CreateContact]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
    ALTER PROCEDURE [dbo].[api_crds_CreateContact]
        @FirstName VARCHAR(100),
	    @LastName VARCHAR(100),
	    @MiddleName VARCHAR(100),
        @PreferredName VARCHAR(100),
	    @NickName VARCHAR(100),
        @Birthdate DATE,
	    @Gender INT,
		@SchoolAttending VARCHAR(100),
    	@HouseholdId INT ,
    	@HouseholdPosition INT,
    	@MobilePhone VARCHAR(25)
    	
    AS 
	BEGIN
        DECLARE @RecordID INT;  
        INSERT INTO [dbo].[Contacts] (
	                [Company],
	                [Display_Name],
					[First_Name],
					[Middle_Name],
					[Last_Name],
					[Nickname],
					[Date_of_Birth],
					[Gender_ID],
					[Household_ID],
					[Household_Position_ID],
					[Mobile_Phone],
					[Current_School],
					[Domain_ID]

				) VALUES (
				    0,
					@PreferredName,
					@FirstName,
					@MiddleName,
					@LastName,
					@NickName,
					@Birthdate,
					@Gender,
					@HouseholdId,
					@HouseholdPosition,
					@MobilePhone,
					@SchoolAttending,
					1)

	    SELECT @RecordID = SCOPE_IDENTITY()

	    IF NOT EXISTS ( SELECT 1 FROM [dbo].[Participants] 
						WHERE [Contact_ID] = @RecordID)
		    BEGIN
			  INSERT INTO [dbo].[Participants](
	                  [Contact_ID],
					  [Participant_Type_ID],
					  [Participant_Start_Date],
					  [Domain_ID])
			   VALUES (@RecordID,
			           1,
					   GetDate(),
					   1)
		    END
	
	    SELECT @RecordID AS RecordID
    END'
END
GO