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
           ,'StatementMethod not like ''No Statement%'' AND EmailAddress is not null'),
           ('Email Statements'
           ,1020
           ,'List of contacts that gave in thesSecond quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod not like ''No Statement%'' AND EmailAddress is not null'),
           ('Email Statements'
           ,1021
           ,'List of contacts that gave in the third quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod not like ''No Statement%'' AND EmailAddress is not null'),
           ('Email Statements'
           ,1022
           ,'List of contacts that gave in the fourth quarter of this year. Filter out Guest Giver, Cash Offerings, Stripe Finance, Anonymous, and Non Cash.'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'StatementMethod not like ''No Statement%'' AND EmailAddress is not null'),

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
           ,'(StatementMethod like ''No Statement%'' OR (EmailAddress is null AND StatementMethod = ''Email/Online'')) AND NOT (DisplayName like ''Cash Offer%''  OR DisplayName like ''Guest Giver%'')'),
           ('No Statement/Exceptions'
           ,1020
           ,'List of contacts that gave in the second quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'(StatementMethod like ''No Statement%'' OR (EmailAddress is null AND StatementMethod = ''Email/Online'')) AND NOT (DisplayName like ''Cash Offer%''  OR DisplayName like ''Guest Giver%'')'),
           ('No Statement/Exceptions'
           ,1021
           ,'List of contacts that gave in the second quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, Notes'
           ,'(StatementMethod like ''No Statement%'' OR (EmailAddress is null AND StatementMethod = ''Email/Online'')) AND NOT (DisplayName like ''Cash Offer%''  OR DisplayName like ''Guest Giver%'')'),
           ('No Statement/Exceptions'
           ,1022
           ,'List of contacts that gave in the second quarter of this year and have a statement method of No Statement Required or that do not have an email address'
           ,'(StatementMethod like ''No Statement%'' OR (EmailAddress is null AND StatementMethod = ''Email/Online'')) AND NOT (DisplayName like ''Cash Offer%''  OR DisplayName like ''Guest Giver%'')'),
           ,'StatementMethod like ''No Statement%''  OR EmailAddress is null')

GO