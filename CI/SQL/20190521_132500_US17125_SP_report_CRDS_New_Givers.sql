USE [MinistryPlatform]
GO

-- =======================================================================================================================
-- Author:      Shakila Rajaiah / Adam Bartoszek
-- Modified Date: 5/21/19
-- Description: This stored procedure generates a list of new givers, for a program for a given date range. 
-- Modifications : US17125 & US 17
-- Modifications made in April : April included adding the email fields for the giver and co-giver.
-- Modifications made in May: The New givers were not included in the result set if a donor's first donation date was
--	made prior to the date range (even if it was for someone's trip few years ago).
--	The current logic searches if the first donation date is in the date range for the program/programID that
--	was entered for the filter. If more than one program id was chosen (IM IN and Ministry Fund), 
--  it will pick the first donation date for both programs, and if the it is earlier than teh date range, it will not be displayed.
--  Deprecated: dbo.report_CRDS_New_Givers_NoAmount. There is a second report CRDS New Givers Email report that displays the
--  same fields without the donation amount. Any changes made to this Stored Proc, needs to work with the otehr report as well.
--  Field display Changes: There are changes to the last seven fields.
--	They were changed to New:
--	Hard Credit Donor ID    Hard Credit Display Name    Hard Credit Address1    Hard Credit Address2    Hard Credit City    Hard Credit State    Hard Credit Zip
--	From Old:
--	Soft Credit Donor ID    Soft Credit Display Name    Soft Credit Address1    Soft Credit Address2    Soft Credit City    Soft Credit State    Soft Credit Zip
-- ======================================================================================================================================

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER  PROCEDURE [dbo].[report_CRDS_New_Givers] 
     @startdate DATETIME,
     @enddate DATETIME,
     @programid as varchar(MAX),
     @congregationid as varchar(MAX),
     @donationstatusid as varchar(MAX),
     @accountingcompanyid as varchar(MAX)
AS

BEGIN
SET nocount ON;

-- Normalize dates to remove time
SET @startdate = CONVERT(DATE, @startdate)
SET @enddate = CONVERT(DATE, @enddate)


	select
    h1.household_id as HHID,
    d.donation_id as 'Donation ID',
    T1.donation_date as 'Donation Date',
    dd.amount as 'Donation Amount',
    p.program_name as Program,
    c1.contact_id as 'Contact Id',
    c1.date_of_birth as 'Birth Day',
    c1.display_name as 'Display Name',
    coalesce(c1.nickname, c1.first_name) as 'First Name',
    c1.last_name as 'Last Name',
	c1.email_address as 'Email',
    c2.contact_id as 'Spouse Id',
    c2.display_name as 'Spouse Display Name',
    coalesce(c2.nickname,c2.first_name) as 'Spouse FName',
    c2.Last_name as 'Spouse LName',
	c2.email_address as 'Spouse Email',
    a1.address_line_1 as 'Address1',
    a1.address_line_2 as 'Address2',
    a1.city as 'City',
    a1.[State/Region] as 'State',
    a1.Postal_Code as 'Zip',
    cn.Congregation_Name as 'Site',
    T1.statement_type as 'Statement Type',
    ds.donation_status as 'Donation Status',
    dn2.Donor_ID as 'Hard Credit Donor ID',
    c3.display_name as 'Hard Credit Display Name',
    a3.address_line_1 as 'Hard Credit Address1',
    a3.address_line_2 as 'Hard Credit Address2',
    a3.city as 'Hard Credit City',
    a3.[State/Region] as 'Hard Credit State',
    a3.Postal_Code as 'Hard Credit Zip'
	from [dbo].[Donation_Distributions] as dd (nolock)
	join [dbo].[Donations] as d
	on d.donation_ID = dd.donation_id
	and dd.program_ID in (SELECT Item FROM dbo.dp_Split(@programid, ','))
	and d.donation_status_ID in (SELECT Item FROM dbo.dp_Split(@donationstatusid, ','))
	join [dbo].[Donation_Statuses] as ds
	on ds.donation_status_id = d.donation_Status_id
	join
	(select 
	dn.Contact_ID,
	st.Statement_Type,
	dn.donor_ID,
	min(d.Donation_Date) as Donation_Date
	from [dbo].[Donation_Distributions] as dd (nolock)
	join [dbo].[Donations] as d
	on d.donation_ID = dd.donation_id
	and dd.program_ID in (SELECT Item FROM dbo.dp_Split(@programid, ','))
	and d.donation_status_ID in (SELECT Item FROM dbo.dp_Split(@donationstatusid, ','))
	join [dbo].[Donors] as dn
	on dn.donor_id = coalesce(dd.soft_credit_donor, d.donor_id)
	join [dbo].[Donation_Statuses] as ds
	on ds.donation_status_id = d.donation_Status_id
	join [dbo].[Statement_Types] as st
	on st.statement_type_id = dn.statement_type_id
	group by 
	dn.Contact_ID,
	st.Statement_Type,
	dn.donor_ID
	) as T1
	on t1.Donor_ID = coalesce(dd.soft_credit_donor, d.donor_ID) -- use donor id if soft credit id is not available
	and T1.Donation_Date = d.Donation_Date 
	join [dbo].[Programs] as p
	on p.Program_ID = dd.Program_ID
	join [dbo].[Contacts] as c1
	on c1.contact_id = T1.contact_ID
	left join [dbo].[Households] as h1
	on h1.household_ID = c1.household_id
	left join [dbo].[Congregations] as cn
	on cn.congregation_ID = coalesce(h1.congregation_ID,5)
	left join [dbo].[Addresses] as a1
	on a1.address_id = h1.address_id
	left join [dbo].[Contact_Relationships] as cr
	on cr.contact_ID = c1.contact_ID
	and cr.relationship_id = 42
	and cr.end_date is null
	left join [dbo].[Contacts] as c2
	on c2.contact_ID = cr.related_contact_ID
	--hard credit, if there was a soft credit donor
	left join [dbo].[Donors] as dn2  
	on dn2.Donor_ID = d.Donor_ID
	and dd.Soft_Credit_Donor is not null
	left join [dbo].[Contacts] as c3
	on c3.Contact_ID = dn2.Contact_ID
	left join [dbo].[Households] as h3
	on h3.household_ID = c3.household_id
	left join [dbo].[Addresses] as a3
	on a3.address_id = h3.address_id

	where cast(T1.donation_date as date) between @startdate and @enddate
	--add in other parameters
	and coalesce(h1.congregation_ID,5) in (SELECT Item FROM dbo.dp_Split(@congregationid, ','))
	and cn.Accounting_Company_Id IN (SELECT Item FROM dbo.dp_Split(@accountingcompanyid, ','))
	order by
    c1.display_name, c1.contact_id, p.program_name, T1.donation_date 


	END
GO


