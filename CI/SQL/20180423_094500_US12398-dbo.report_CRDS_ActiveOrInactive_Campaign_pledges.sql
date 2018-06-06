USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_ActiveOrInactive_Campaign_Pledges]    Script Date: 4/17/2018 1:16:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- This procedure is intended to fulfill User Story 12398 
--
-- The user should receive the email list of the giver and the co-giver 
-- and he should not see the giving amounts. 
--
-- The email list is based on who has either given to the I'm in campaign, or who has a pledge, 
-- a list of all the pledges whether they have given into it or not.
--
-- This list should also have the list of people who have given into the I'm in campaign, 
-- whether they have made a pledge or not. 

CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_ActiveOrInactive_Campaign_Pledges] 
	@pledge_campaign_id AS VARCHAR(MAX),
	@pledge_status_id as varchar(MAX)

AS
BEGIN
	SET NOCOUNT ON;


IF OBJECT_ID('tempdb..#REPORT') IS NOT NULL
/*Then it exists*/
   DROP TABLE #REPORT

IF OBJECT_ID('tempdb..#DUPHH') IS NOT NULL
/*Then it exists*/
   DROP TABLE #DUPHH

IF OBJECT_ID('tempdb..#SPOUSE') IS NOT NULL
/*Then it exists*/
   DROP TABLE #SPOUSE


-- I'm IN campaign donor list
DECLARE @IminCommitment TABLE
(
 donor_id int,
 totalpledge money
);


Insert into @IminCommitment
	select d.donor_id,sum(p.total_pledge)
	from donors d 
	join pledges p on p.donor_id = d.donor_id
	join pledge_campaigns pc on pc.Pledge_Campaign_ID = p.pledge_campaign_id
	-- where pc.pledge_campaign_id = @pledge_campaign_id           --1103 -- 'I''m in'
	where pc.pledge_campaign_id in (SELECT Item FROM dbo.dp_Split(@pledge_campaign_id, ','))
	--and p.pledge_status_id in (1,2,3) -- active, completed, or discontinued
	and p.pledge_status_id in (SELECT Item FROM dbo.dp_Split(@pledge_status_id, ','))
	group by d.donor_id

	--select * from @IminCommitment

-- I'm In Donations
DECLARE @IminDonations TABLE
(
 donor_id int,
 donation money
);
INSERT INTO @IminDonations
	select d.donor_id,sum(dd.amount)
	from donation_distributions dd
	join donations d on dd.donation_id = d.donation_id
	where dd.program_id IN (select Program_ID from pledge_campaigns where Pledge_campaign_ID IN (SELECT Item FROM dbo.dp_Split(@pledge_campaign_id, ','))) --- 146 --I'm In
	and d.donation_status_id in (2)
	and soft_credit_donor is null
	group by d.donor_id
	UNION
	select dd.soft_credit_donor,sum(dd.amount)
	from donation_distributions dd
	join donations d on dd.donation_id = d.donation_id
	where dd.program_id IN (select Program_ID from pledge_campaigns where Pledge_campaign_ID IN (SELECT Item FROM dbo.dp_Split(@pledge_campaign_id, ',')))   --- 146 --I'm In
	and d.donation_status_id in (2)
	and soft_credit_donor is not null
	group by dd.soft_credit_donor


-- put all of the data together in this big temp table
CREATE TABLE #REPORT
(
	donor_id int,
	contact_id int,
	displayname nvarchar(150),
	Imingiving money,
	IminPledge money,
	firstname varchar(50),
	lastname varchar(50),
	email varchar(50),
	address1 varchar(50),
	city varchar(50),
	stte varchar(50),
	zip varchar(50),
	householdid int,
	cogiver int,
	householdposition int,
	cogivercontactid int,
	cogivername varchar(100),
	cogiveremail varchar(50),
	congregation_id int,
	congregation varchar(50),
	hascogiver int
);

CREATE INDEX IX_CONTACT_ID ON #REPORT(donor_id,contact_id);

-- make sure all of the unique donors are included in the report table


Insert INTO #REPORT(donor_id) 
	select distinct donor_id from @IminCommitment ic 
		where 
		NOT EXISTS (select * from #REPORT r WHERE ic.donor_id = r.donor_id)

--select TOP (5) '@IminCommitment', * from #REPORT --where donor_id in (7600953,7632234, 7555609)


Insert INTO #REPORT(donor_id) 
	select distinct donor_id from @IminDonations ic 
		where 
		NOT EXISTS (select * from #REPORT r WHERE ic.donor_id = r.donor_id)

--select TOP (5) '@IminDonations', *  from #REPORT --where donor_id in (7600953,7632234, 7555609)


-- add Im In donations to table
UPDATE r
SET r.Imingiving = g.donationsum 
FROM #REPORT r
JOIN  
  (select donor_id, sum(donation) donationsum FROM @IminDonations GROUP BY donor_id ) g ON r.donor_id = g.donor_id

--select TOP (5) ' Im In donations' , * from #REPORT 

-- add Im In commitment to table
UPDATE r
SET r.IminPledge = g.totalpledgesum 
FROM #REPORT r
JOIN 
  (select donor_id, sum(totalpledge) totalpledgesum FROM @IminCommitment GROUP BY donor_id ) g ON r.donor_id = g.donor_id
--select TOP (5) ' Im In committment' , * from #REPORT 

-- add contact info to table
UPDATE r
SET r.contact_id = c.contact_id, r.displayname = c.display_name,
	r.firstname = c.first_name, r.lastname = c.last_name, r.email = c.email_address, 
    r.householdid = c.Household_ID, r.householdposition = c.household_position_id
FROM #REPORT r
JOIN donors d ON d.donor_id = r.donor_id
JOIN contacts c on d.contact_id = c.contact_id
--select TOP (5) 'Contact Info' , * from #REPORT 

-- add address info to table
UPDATE r
SET r.address1 = a.address_line_1, 
    r.city = a.city, 
	r.stte = a.[state/region], 
	r.zip = a.postal_code,
    r.congregation_id = h.congregation_id
FROM #REPORT r
JOIN Households h ON r.householdid = h.household_id
JOIN Addresses a on h.address_id = a.address_id
 --select TOP (5) 'Address' , * from #REPORT

-- add congregation info to table
UPDATE r 
SET r.congregation = c.congregation_name
FROM #REPORT r
JOIN Congregations c on c.congregation_id = r.congregation_id
--select TOP (5) 'congregation' , * from #REPORT

-- set donors statement type (used as cogiver indicator)
UPDATE r
SET r.cogiver = d.statement_type_id
FROM #REPORT r
JOIN donors d ON d.donor_id = r.donor_id

--select TOP (5) 'donors statement type' , * from #REPORT

-- find co-giver if statement type is family (2)
update r 
SET r.cogivercontactid = c.contact_id, cogivername = c.first_name + ' ' + c.last_name, r.cogiveremail = c.email_address,r.hascogiver = 1
FROM #REPORT r
JOIN Contacts c on r.householdid = c.household_id
where r.cogiver = 2 and householdposition = 1 and c.contact_id <> r.contact_id and c.household_position_id = 1

--select TOP (5) 'find co-giver' , * from #REPORT
 
-- at this point we have all of the giving at the individual level
-- combine the household with cogiving
select * into #SPOUSE FROM #REPORT where hascogiver = 1

CREATE INDEX IX_HH_DONOR_ID ON #SPOUSE(householdid,donor_id);

-- work the housholds with a cursor
DECLARE @householdid int
DECLARE @previoushouseholdid int = 0
DECLARE @donorid int
DECLARE @TestTable TABLE
(
 householdid int,
 donorid int
);

DECLARE db_cursor CURSOR FAST_FORWARD FOR  
select householdid, donor_id from #SPOUSE order by householdid

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @householdid, @donorid
-- compare this record to the last one
WHILE @@FETCH_STATUS = 0   
BEGIN   
		IF (@householdid <> @previoushouseholdid)
		BEGIN
         update #REPORT 
			set Imingiving     = (select sum(Imingiving)     from #REPORT where householdid = @householdid and hascogiver = 1),
				IminPledge         = (select sum(IminPledge)         from #REPORT where householdid = @householdid and hascogiver = 1)
			where householdid = @householdid and donor_id = @donorid
		 delete #REPORT where householdid = @householdid and hascogiver = 1 and donor_id <> @donorid
		END
		 
		SET @previoushouseholdid =  @householdid	   

       FETCH NEXT FROM db_cursor INTO @householdid, @donorid
END   

CLOSE db_cursor   
DEALLOCATE db_cursor

-- produce the display table showing information requested
select 
firstname, lastname, email, address1,city, stte as state, zip, cogivername, cogiveremail, congregation,
case when Imingiving is null then 0 else Imingiving end as ImIn_Giving, 
case when IminPledge is null then 0 else IminPledge end as Im_In_Pledge
 from #REPORT 
order by householdid,lastname,firstname;

END
GO

