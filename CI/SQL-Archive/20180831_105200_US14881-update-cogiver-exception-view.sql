USE [MinistryPlatform]
GO

/****** Object:  View [dbo].[vw_crds_Cogiver_Exceptions]    Script Date: 8/31/2018 10:46:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER VIEW [dbo].[vw_crds_Cogiver_Exceptions]
AS
    -- Overlapping co-giver relationships
    select
        x.Contact_Relationship_ID as Contact_Relationship_ID,
        x.Contact_ID as Contact_ID,
        x.Display_Name as Display_Name,
        'Start and end dates overlap' as Exception,
        1 AS Domain_ID
    from
        (
            select
                cr1.Contact_Relationship_ID as Contact_Relationship_ID,
                cr1.Contact_ID as Contact_ID,
                c.Display_Name as Display_Name,
                Row_Num = ROW_NUMBER() OVER(PARTITION BY cr1.Contact_ID ORDER BY cr1.Contact_Relationship_ID)
            from
                dbo.Contacts c
                inner join dbo.Contact_Relationships cr1 on cr1.Contact_ID = c.Contact_ID and cr1.Relationship_ID = 42
                inner join dbo.Contact_Relationships cr2 on cr2.Contact_ID = c.Contact_ID and cr2.Relationship_ID = 42
            where 
                cr1.Contact_Relationship_ID <> cr2.contact_relationship_id
                and CONVERT(DATE, COALESCE(cr1.Start_Date, '1900-01-01')) < CONVERT(DATE,COALESCE(cr2.End_Date, '9999-12-31'))
                and CONVERT(DATE, COALESCE(cr1.End_Date, '9999-12-31')) > CONVERT(DATE, COALESCE(cr2.Start_Date, '1900-01-01'))
        ) as x
    where
        x.Row_Num = 1
    UNION
    -- unpaired co-giver records (based on Triggered_By per Mike F.)
    select
        cr.Contact_Relationship_ID as Contact_Relationship_ID,
        cr.Contact_ID as Contact_ID,
        c.Display_Name as Display_Name,
        'No reciprocal co-giver relationship' as Exception,
        1 AS Domain_ID
    from
        dbo.Contacts c
        inner join dbo.Contact_Relationships cr on cr.Contact_ID = c.Contact_ID and cr.Relationship_ID = 42
        inner join
        (
            -- if Triggered_By is not null, it must reference an existing row
            select
                cr1.Contact_Relationship_ID
            from
                contact_relationships cr1
                left join contact_relationships cr2 on cr2.Contact_Relationship_ID = cr1._Triggered_By and cr2.Relationship_ID = 42
            where
                cr1.Relationship_ID = 42
                and cr1._Triggered_By is not null
                and cr2.Contact_Relationship_ID is null
            union
            -- if Triggered_By is null, there must be another row whose Triggered_By field references this row
            select
                cr1.Contact_Relationship_ID
            from
                contact_relationships cr1
                left join contact_relationships cr2 on cr2._Triggered_By = cr1.Contact_Relationship_ID and cr2.Relationship_ID = 42
            where
                cr1.Relationship_ID = 42
                and cr1._Triggered_By is null
                and cr2.Contact_Relationship_ID is null
        ) as problem on problem.Contact_Relationship_ID = cr.Contact_Relationship_ID
    UNION
    -- co-giver relationships where contact id and related contact id do not match
    select
        pairs.Contact_Relationship_ID as Contact_Relationship_ID,
        pairs.contact_id as Contact_ID,
        c.Display_Name as Display_Name,
        'Contact ID and Related Contact ID do not match' as Exception,
        1 AS Domain_ID
    from
        dbo.Contacts c
        inner join (
            -- if Triggered_By is not null, the 2nd half of the pair is the row referenced by Triggered_By
            select
                cr1.Contact_Relationship_ID,
                cr1.Contact_ID
            from
                contact_relationships cr1
                inner join contact_relationships cr2 on cr2.Contact_Relationship_ID = cr1._Triggered_By and cr2.Relationship_ID = 42
            where
                cr1.Relationship_ID = 42
                and (cr1.Contact_ID <> cr2.Related_Contact_ID or cr1.Related_Contact_ID <> cr2.Contact_ID)
            union
            -- if Triggered_By is null, the 2nd half of the pair is the row whose Triggered_By field references this row
            select
                cr1.Contact_Relationship_ID,
                cr1.Contact_ID
            from
                contact_relationships cr1
                inner join contact_relationships cr2 on cr2._Triggered_By = cr1.Contact_Relationship_ID and cr2.Relationship_ID = 42
            where
                cr1.Relationship_ID = 42
                and cr1._Triggered_By is null
                and (cr1.Contact_ID <> cr2.Related_Contact_ID or cr1.Related_Contact_ID <> cr2.Contact_ID)
        ) as pairs on pairs.Contact_ID = c.Contact_ID
    UNION
    -- co-giver relationships where start and end dates are not matched in two related records
    select
        pairs.Contact_Relationship_ID as Contact_Relationship_ID,
        pairs.contact_id as Contact_ID,
        c.Display_Name as Display_Name,
        'Start and end dates do not match' as Exception,
        1 AS Domain_ID
    from
        dbo.Contacts c
        inner join (
            -- if Triggered_By is not null, the 2nd half of the pair is the row referenced by Triggered_By
            select
                cr1.Contact_Relationship_ID,
                cr1.Contact_ID
            from
                contact_relationships cr1
                inner join contact_relationships cr2 on cr2.Contact_Relationship_ID = cr1._Triggered_By and cr2.Relationship_ID = 42
            where
                cr1.Relationship_ID = 42
                and (
                    CONVERT(DATE, coalesce(cr1.Start_Date, '1900-01-01')) <> CONVERT(DATE, coalesce(cr2.Start_Date, '1900-01-01'))
                    or CONVERT(DATE, coalesce(cr1.End_Date, '9999-12-31')) <> CONVERT(DATE, coalesce(cr2.End_Date, '9999-12-31'))
                )
            union
            -- if Triggered_By is null, the 2nd half of the pair is the row whose Triggered_By field references this row
            select
                cr1.Contact_Relationship_ID,
                cr1.Contact_ID
            from
                contact_relationships cr1
                inner join contact_relationships cr2 on cr2._Triggered_By = cr1.Contact_Relationship_ID and cr2.Relationship_ID = 42
            where
                cr1.Relationship_ID = 42
                and cr1._Triggered_By is null
                and (
                    CONVERT(DATE, coalesce(cr1.Start_Date, '1900-01-01')) <> CONVERT(DATE, coalesce(cr2.Start_Date, '1900-01-01'))
                    or CONVERT(DATE, coalesce(cr1.End_Date, '9999-12-31')) <> CONVERT(DATE, coalesce(cr2.End_Date, '9999-12-31'))
                )
        ) as pairs on pairs.Contact_ID = c.Contact_ID
    UNION
    -- co-giver contact relationships that have null start dates
    select
        cr.Contact_Relationship_ID as Contact_Relationship_ID,
        c1.Contact_ID as Contact_ID,
        c1.display_name as Display_Name,
        'Co-giver has no start date' as Exception,
        1 AS Domain_ID
    from dbo.contact_relationships cr
        join dbo.contacts c1 on cr.contact_id = c1.contact_id
    where cr.relationship_id = 42 and cr.start_date is null 
    UNION
    -- co-giver contact relationships that have end dates before start dates
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
    -- co-giving relationship exists, but contacts are not in the same household
    select
        cr.Contact_Relationship_ID as Contact_Relationship_ID,
        c1.Contact_ID as Contact_ID,
        c1.Display_Name as Display_Name,
        'Co-givers have different households' as Exception,
        1 AS Domain_ID
    from
        Contact_Relationships cr
        inner join Contacts c1 on c1.Contact_ID = cr.Contact_ID
        inner join Contacts c2 on c2.Contact_ID = cr.Related_Contact_ID
    where
        cr.Relationship_ID = 42
        and cr.End_Date is null
        and c1.Household_ID <> c2.Household_ID
    ;
GO


