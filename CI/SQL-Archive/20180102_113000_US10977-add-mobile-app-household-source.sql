USE MinistryPlatform
GO

-- =================================
-- Author: Jon Horner
-- Date: 1/2/2018
-- Description: Add a Household Source for the mobile
-- 	app and stop sending the default welcome email to
--	to users who sign up from it.
-- =================================

DECLARE @mobileAppHouseholdSourceId INT = 51;									-- Not in MPIdentityManagement Range
DECLARE @domainId INT = 1;
DECLARE @defaultWelcomeEmailProcessId INT = 23;									-- The 'Send Welcome Email' Process
DECLARE @newConditionClause nvarchar(50) = ' AND h.Household_Source_ID != 51';	-- This will exclude the new mobile app household source

-- Create the new household source if it doesn't already exist
IF NOT EXISTS (SELECT * FROM Household_Sources WHERE Household_Source_ID = @mobileAppHouseholdSourceId)
BEGIN
	SET IDENTITY_INSERT Household_Sources ON;

	INSERT INTO Household_Sources
	(
		Household_Source_ID
		, Household_Source
		, [Description]
		, Domain_ID
	)
	VALUES
	(
		@mobileAppHouseholdSourceId
		, 'Mobile App'
		, 'Differential Mobile App Registration'
		, @domainId
	)

	SET IDENTITY_INSERT Household_Sources OFF;
END

-- Modify Dependent_Condition of email trigger to include the new clause if it hasn't already been added
DECLARE @previousDependentCondition nvarchar(4000);
DECLARE @newDependentCondition nvarchar(4000);

IF EXISTS (SELECT * FROM dp_Processes WHERE Process_ID = @defaultWelcomeEmailProcessId AND Dependent_Condition NOT LIKE '%' + @newConditionClause + '%')
BEGIN
	-- Grab the old trigger condition on the process
	SET @previousDependentCondition = (
		SELECT Dependent_Condition
		FROM dp_Processes
		WHERE Process_ID = @defaultWelcomeEmailProcessId
	);

	-- Add to the old trigger condition a clause that excludes the mobile app registration household source
	SET @newDependentCondition = (
		SELECT SUBSTRING(@previousDependentCondition, 1, LEN(@previousDependentCondition)-1)	-- Gets rid of the trailing closing paren
			+ @newConditionClause + ')'															-- add new clause and closing paren
	);

	UPDATE dp_Processes
	SET Dependent_Condition = @newDependentCondition
	WHERE Process_ID = @defaultWelcomeEmailProcessId;
END