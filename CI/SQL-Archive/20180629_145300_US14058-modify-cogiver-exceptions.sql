USE [MinistryPlatform]
GO

CREATE OR ALTER VIEW [dbo].[vw_crds_Cogiver_Exceptions]
AS
	-- Overlapping co-giver relationships
    select distinct
        cr1.Contact_Relationship_ID as Contact_Relationship_ID,
        cr1.Contact_ID as Contact_ID,
        c.Display_Name as Display_Name,
        'Start and end dates overlap' as Exception,
		1 AS Domain_ID
    from
        dbo.Contacts c
        inner join dbo.Contact_Relationships cr1 on cr1.Contact_ID = c.Contact_ID and cr1.Relationship_ID = 42
        inner join dbo.Contact_Relationships cr2 on cr2.Contact_ID = c.Contact_ID and cr2.Relationship_ID = 42
    where 
        cr1.Contact_Relationship_ID <> cr2.contact_relationship_id
        and CONVERT(DATE, COALESCE(cr1.Start_Date, '1900-01-01')) < CONVERT(DATE,COALESCE(cr2.End_Date, '9999-12-31'))
        and CONVERT(DATE, COALESCE(cr1.End_Date, '9999-12-31')) > CONVERT(DATE, COALESCE(cr2.Start_Date, '1900-01-01'))
	UNION
	--script to identify co-giver relationships where start and end dates are not matched in two related records
	select
        cr1.Contact_Relationship_ID as Contact_Relationship_ID,
        cr1.contact_id as Contact_ID,
        c.Display_Name as Display_Name,
        'Start and end dates do not match' as Exception,
        1 AS Domain_ID
	from dbo.contact_relationships cr1
		inner join dbo.contact_relationships cr2 on cr1.contact_id = cr2.related_contact_id
		inner join dbo.Contacts c on c.Contact_ID = cr1.Contact_ID
	where cr1.relationship_id = 42
		and cr2.relationship_id = 42
		and cr2.contact_id = cr1.related_contact_id
		and (CONVERT(DATE, coalesce(cr1.Start_Date, '1900-01-01')) <> CONVERT(DATE, coalesce(cr2.Start_Date, '1900-01-01'))
             or CONVERT(DATE, coalesce(cr1.End_Date, '9999-12-31')) <> CONVERT(DATE, coalesce(cr2.End_Date, '9999-12-31')))
	UNION
	--Script to identify co-giver contact relationships that have null start dates
	select
        cr.Contact_Relationship_ID as Contact_Relationship_ID,
        c1.Contact_ID as Contact_ID,
        c1.display_name as Display_Name,
        'Cogiver has no start date' as Exception,
        1 AS Domain_ID
	from dbo.contact_relationships cr
		join dbo.contacts c1 on cr.contact_id = c1.contact_id
	where cr.relationship_id = 42 and cr.start_date is null 
	UNION
	--Script to identify co-giver contact relationships that have end dates before start dates
	select
        cr.Contact_Relationship_ID as Contact_Relationship_ID,
        c1.Contact_ID as Contact_ID,
        c1.display_name as Display_Name,
        'End date prior to start date' as Exception,
        1 AS Domain_ID
	from dbo.contact_relationships cr
		join dbo.contacts c1 on cr.contact_id = c1.contact_id
	where cr.relationship_id = 42 and CONVERT(DATE, coalesce(cr.Start_Date, '1900-01-01')) > CONVERT(DATE, coalesce(cr.End_Date, '9999-12-31'))
	UNION
	--script to identify unmatched co-giver records
	select
        cr1.Contact_Relationship_ID as Contact_Relationship_ID,
		c1.Contact_ID as Contact_ID,
		c1.Display_Name as Display_Name,
		'No reciprocal cogiver relationship' as Exception,
		1 AS Domain_ID
	from
		dbo.Contacts c1
			inner join dbo.Contact_Relationships cr1 on cr1.Contact_ID = c1.Contact_ID and cr1.Relationship_ID = 42
			inner join dbo.Contacts c2 on c2.Contact_ID = cr1.Related_Contact_ID
			left join dbo.Contact_Relationships cr2 on cr2.Contact_ID = c2.Contact_ID and cr2.Relationship_ID = 42
                and cr2.Related_Contact_ID = c1.Contact_ID
                and CONVERT(DATE, coalesce(cr1.Start_Date, '1900-01-01')) = CONVERT(DATE, coalesce(cr2.Start_Date, '1900-01-01'))
	where
		cr2.Contact_ID IS NULL

GO


UPDATE dp_Pages SET
	Default_Field_List = 'Contact_Relationship_ID, Contact_ID, Display_Name, Exception',
	Selected_Record_Expression = 'Contact_ID',
	Contact_ID_Field = 'Contact_ID'
WHERE
	Display_Name = 'Co-giver Exceptions'
;
GO
