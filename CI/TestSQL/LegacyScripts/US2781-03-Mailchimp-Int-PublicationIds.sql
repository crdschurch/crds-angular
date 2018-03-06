-- Set List ID values for Int account for Mailchimp
USE MinistryPlatform
GO

IF EXISTS (SELECT * FROM [dbo].[dp_Publications] WHERE [Publication_ID] = 1)
BEGIN

UPDATE [dbo].[dp_Publications]

SET
	[Third_Party_Publication_ID] = '864ac15bc2'
WHERE
	[Publication_ID] = 1
END

IF EXISTS (SELECT * FROM [dbo].[dp_Publications] WHERE [Publication_ID] = 2)
BEGIN

UPDATE [dbo].[dp_Publications]

SET
	[Third_Party_Publication_ID] = 'ff1d70b3e6'
WHERE
	[Publication_ID] = 2
END

IF EXISTS (SELECT * FROM [dbo].[dp_Publications] WHERE [Publication_ID] = 3)
BEGIN

UPDATE [dbo].[dp_Publications]

SET
	[Third_Party_Publication_ID] = 'fcf72bfb27'
WHERE
	[Publication_ID] = 3
END
