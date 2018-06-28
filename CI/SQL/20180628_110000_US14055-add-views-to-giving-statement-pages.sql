USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Insert Email Statements view
INSERT INTO [dbo].[dp_Page_Views]
           ([View_Title]
           ,[Page_ID]
           ,[Description]
           ,[Field_List]
           ,[View_Clause])
     VALUES
           ('Email Statements'
           ,1018
           ,'List of contacts that gave in the first quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'DisplayName not like ''Cash Offering%'' AND DisplayName not like ''Guest Giver%'' AND DisplayName not like ''Anyonymous%'' AND DisplayName not like ''Non Cash%'' AND DisplayName not like ''Stripe%'''),
           ('Email Statements'
           ,1020
           ,'List of contacts that gave in thesSecond quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'DisplayName not like ''Cash Offering%'' AND DisplayName not like ''Guest Giver%'' AND DisplayName not like ''Anyonymous%'' AND DisplayName not like ''Non Cash%'' AND DisplayName not like ''Stripe%'''),
           ('Email Statements'
           ,1021
           ,'List of contacts that gave in the third quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'DisplayName not like ''Cash Offering%'' AND DisplayName not like ''Guest Giver%'' AND DisplayName not like ''Anyonymous%'' AND DisplayName not like ''Non Cash%'' AND DisplayName not like ''Stripe%'''),
           ('Email Statements'
           ,1022
           ,'List of contacts that gave in the fourth quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'DisplayName not like ''Cash Offering%'' AND DisplayName not like ''Guest Giver%'' AND DisplayName not like ''Anyonymous%'' AND DisplayName not like ''Non Cash%'' AND DisplayName not like ''Stripe%''')

GO

INSERT INTO [dbo].[dp_Page_Views]
           ([View_Title]
           ,[Page_ID]
           ,[Description]
           ,[Field_List]
           ,[View_Clause])
     VALUES
           ('No Statement/Exceptions'
           ,1018
           ,'List of contacts that gave in the first quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod like ''No Statement%''  OR EmailAddress is null'),
           ('No Statement/Exceptions'
           ,1020
           ,'List of contacts that gave in the second quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod like ''No Statement%''  OR EmailAddress is null'),
           ('No Statement/Exceptions'
           ,1021
           ,'List of contacts that gave in the second quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod like ''No Statement%''  OR EmailAddress is null'),
           ('No Statement/Exceptions'
           ,1022
           ,'List of contacts that gave in the second quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod like ''No Statement%''  OR EmailAddress is null')

GO