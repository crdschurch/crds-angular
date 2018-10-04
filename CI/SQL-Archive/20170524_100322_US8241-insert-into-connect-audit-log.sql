USE [MinistryPlatform]
GO

INSERT INTO cr_connect_communications_type (Communication_Type, Start_Date, Domain_ID)
VALUES  ('Request To Join Small Group', GETDATE(), 1)
      , ('Invite to Small Group', GETDATE(), 1)
	  , ('Email Small Group Leader', GETDATE(), 1)


UPDATE cr_connect_communications_type
SET Communication_Type = 'Request To Join Gathering'
WHERE Connect_Communications_Type_ID = 3
