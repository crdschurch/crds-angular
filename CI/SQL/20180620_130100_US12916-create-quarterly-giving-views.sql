USE [MinistryPlatform]
GO

/****** Object:  View [dbo].[vw_crds_Q1_Giving_Statements]    Script Date: 6/20/2018 11:13:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vw_crds_Q1_Giving_Statements]
AS
select ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation 
from crds_QuarterlyGivingStatementDonors(1)
GO


CREATE VIEW [dbo].[vw_crds_Q2_Giving_Statements]
AS
select ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation 
from crds_QuarterlyGivingStatementDonors(2)
GO

CREATE VIEW [dbo].[vw_crds_Q3_Giving_Statements]
AS
select ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation 
from crds_QuarterlyGivingStatementDonors(3)
GO


CREATE VIEW [dbo].[vw_crds_Q4_Giving_Statements]
AS
select ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation 
from crds_QuarterlyGivingStatementDonors(4)
GO