USE [MinistryPlatform]
GO

/****** Object:  View [dbo].[vw_crds_Cogiver_Exceptions]    Script Date: 6/25/2018 7:01:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[vw_crds_Cogiver_Exceptions]
AS
-- Overlapping co-giver relationships
	select cr1.Contact_ID as ContactId, c.Display_Name as DisplayName, 'Start and end dates overlap' as Exception
	from dbo.contact_relationships cr1
		inner join dbo.contact_relationships cr2 on cr1.contact_id = cr2.contact_id 
		inner join dbo.Contacts c on c.Contact_ID = cr1.Contact_ID
	where 
		cr1.relationship_id = 42
		and 
		cr2.relationship_id = 42
		and
		cr1.Contact_Relationship_ID <> cr2.contact_relationship_id
		and
		(
			(convert(date,cr2.start_date,101) >= convert(date,cr1.start_date,101) and 
			convert(date,cr2.start_date,101) <= convert(date,cr1.end_date,101))
			or
			(convert(date,cr2.end_date,101) <= convert(date,cr1.end_date,101) and 
			convert(date,cr2.end_date,101) >= convert(date,cr1.start_date,101))
			or
			(cr2.end_date is null and convert(date,cr1.end_date,101) > convert(date,cr2.start_date,101))
			or
			(cr1.end_date is null and cr2.end_date is null)
			or
			(cr1.end_date is null and cr2.start_date > cr1.start_date)
		)
	UNION
	--script to identify co-giver relationships where start and end dates are not matched in two related records
	select cr1.contact_id as ContactId, c.Display_Name as DisplayName, 'Start and end dates do not match' as Exception
	from dbo.contact_relationships cr1
		inner join dbo.contact_relationships cr2 on cr1.contact_id = cr2.related_contact_id
		inner join dbo.Contacts c on c.Contact_ID = cr1.Contact_ID
	where cr1.relationship_id = 42
		and cr2.relationship_id = 42
		and cr2.contact_id = cr1.related_contact_id
		and (CONVERT(DATE,cr1.start_date) <> CONVERT(DATE,cr2.start_date) or CONVERT(DATE,cr1.end_date) <> CONVERT(DATE, cr2.end_date))
	UNION
	--Script to identify co-giver contact relationships that have null start dates
	select c1.Contact_ID as ContactId, c1.display_name as DisplayName, 'Cogiver has no start date' as Exception
	from dbo.contact_relationships cr
		join dbo.contacts c1 on cr.contact_id = c1.contact_id
		join dbo.contacts c2 on cr.related_contact_id = c2.contact_id
	where cr.relationship_id = 42 and cr.start_date is null 
	UNION
	--Script to identify co-giver contact relationships that have end dates before start dates
	select c1.Contact_ID as ContactId, c1.display_name as DisplayName, 'End date prior to start date' as Exception
	from dbo.contact_relationships cr
		join dbo.contacts c1 on cr.contact_id = c1.contact_id
		join dbo.contacts c2 on cr.related_contact_id = c2.contact_id
	where cr.relationship_id = 42 and cr.start_date > cr.end_date 
	UNION
	--script to identify unmatched co-giver records
	select cr.Contact_ID as ContactId, c.Display_Name as DisplayName, 'No reciprocal cogiver relationship' as Exception
	from dbo.contact_relationships cr
		inner join dbo.Contacts c on c.Contact_ID = cr.Contact_ID
	where cr.relationship_id = 42
		and cr.End_Date is null
		and cr.Contact_ID not in 
			(	select Related_Contact_ID 
				from dbo.Contact_Relationships 
				where Relationship_ID = 42 
					AND End_Date is NULL
					--AND CONVERT(DATE, cr.Start_Date) = CONVERT(DATE, Start_Date) Don't think we care about Start_Date
			)
GO


