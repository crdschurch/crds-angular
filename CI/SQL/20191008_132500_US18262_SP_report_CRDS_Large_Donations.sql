USE [MinistryPlatform]
GO

-- =======================================================================================================================
-- Author:      Shakila Rajaiah 
-- Modified Date: 5/21/19
-- Description:   This stored procedure generates a list of large donors for a program (IM IN or Ministry Fund) for a given date range. 
-- Modifications: US17253
-- Description:   Additional fields were added to display the Giver Name, Giver Email, 
--                Spouse Email and Soft Donor Email.
-- Modifications: By Shakila Rajaiah ; 10/07/19; US 18268; Added fields Payment Types and Description
-- ======================================================================================================================================


CREATE OR ALTER  PROCEDURE [dbo].[report_CRDS_Large_Donations] 
     @startdate DATETIME,
     @enddate DATETIME,
     @programid as varchar(MAX),
     @congregationid as varchar(MAX),
     @donationstatusid as varchar(MAX),
     @mindonation as money,
     @accountingcompanyid as varchar(MAX)
AS 

BEGIN
SET nocount ON;


IF OBJECT_ID('tempdb..#largedonation') IS NOT NULL   
    DROP TABLE #largedonation

--create a temp table to hold giver, spouse and soft credit donor information.
CREATE TABLE #largedonation
(
    hhid int,
    amount	money,
    donationid int,
    dondate	datetime,
    donamount money,
    donationstatus varchar(25),
    paymenttype varchar (25),
	progname varchar(150),
    cid1	int,
    cdisplayname varchar(150),
    clname varchar(50),
    cfname varchar(50),
    cemail varchar(100),
    cid2	int,
    cgdisplayname varchar(150),
    cglname varchar(50),
    cgfname varchar(50),
	cgemail varchar(100),
    addr1 varchar(150),
    addr2 varchar(150),
    city varchar(150),
    st varchar(150),
    zip varchar(150),
    conname varchar(150),
    programid int,
    sddonid int,
    sddisplayname varchar(50),
    sdlname varchar(50),
    sdfname varchar(50),
	sdemail varchar(100),
    sdaddr1 varchar(50),
    sdaddr2 varchar(50),
    sdcity varchar(50),
    sdst varchar(50),
    sdzip varchar(50),
	notes varchar (2000)
)

insert into #largedonation
select 
	h.household_id
	,dd.Amount
	, don.donation_id
	, don.Donation_date
	, don.donation_amount
	, ds.Donation_Status
	, pt.Payment_Type 
	, p.[program_name]
	, c.contact_id as contact1
	, c.display_name
	, c.last_name
	, c.first_name
	, c.Email_Address
	, c2.contact_id as contact2
	, c2.display_name as cogiver
	, c2.last_name
	, c2.First_Name
	, c2.Email_Address
	, a.Address_Line_1
	, a.Address_Line_2
	, a.City
	, a.[State/Region]
	, a.Postal_Code
	,con.Congregation_Name
	, dd.Program_ID
	,dd.Soft_Credit_Donor
	,null,null,null,null,null,null,null,null,null
	, don.Notes
from donations don
	join donation_distributions dd on dd.donation_id = don.donation_id
	left join programs p on p.program_id = dd.program_id
	left join Payment_Types pt on pt.Payment_Type_ID = don.Payment_Type_ID
	left join donors d on d.donor_id = don.donor_id
	left join contacts c on c.contact_id=d.contact_id
	left join contact_relationships cr on cr.Contact_ID = c.Contact_ID
		and relationship_id = 42
		and don.donation_date between cr.start_date and coalesce(cr.end_date, '9999-12-31')
	left join contacts c2 on c2.contact_id = cr.related_contact_id
	left join households h on h.household_id = c.household_id
	left join congregations AS con ON con.congregation_id = dd.Congregation_ID
	left join addresses a on a.address_id = h.address_id
	left join Donation_Statuses ds on ds.Donation_Status_ID = don.Donation_Status_ID
WHERE don.donation_date between @startdate and @enddate+1
    AND don.donation_status_id IN (SELECT Item FROM dbo.dp_Split(@donationstatusid, ','))
    AND dd.Amount >= @mindonation
    AND dd.Congregation_ID IN (SELECT Item FROM dbo.dp_Split(@congregationid, ','))
    AND dd.program_id IN (SELECT Item FROM dbo.dp_Split(@programid, ','))
    AND con.Accounting_Company_Id IN (SELECT Item FROM dbo.dp_Split(@accountingcompanyid, ','))

update ld
	set sddisplayname = c.display_name, 
	sdlname = c.last_name,
	sdfname = c.first_name,
	sdemail = c.Email_Address,
	sdaddr1=a.Address_Line_1,
	sdaddr2=a.Address_Line_2,
	sdcity = a.City,
	sdst = a.[State/Region],
	sdzip = a.Postal_Code
from #largedonation ld
	join donors d on d.donor_id = ld.sddonid
	join contacts c on c.contact_id = d.contact_id
	left join households h on h.household_id = c.household_id
	left join addresses a on a.address_id = h.address_id
where ld.sddonid is not null

update #largedonation set cdisplayname='' where cdisplayname is null
update #largedonation set clname='' where clname is null
update #largedonation set cfname='' where cfname is null
update #largedonation set cemail='' where cemail is null
update #largedonation set cgdisplayname ='' where cgdisplayname  is null
update #largedonation set cglname ='' where cglname  is null
update #largedonation set cgfname ='' where cgfname  is null
update #largedonation set cgemail='' where cgemail is null
update #largedonation set addr1='' where addr1 is null
update #largedonation set addr2='' where addr2 is null
update #largedonation set city='' where city is null
update #largedonation set st='' where st is null
update #largedonation set zip='' where zip is null
update #largedonation set notes='' where notes is null

update #largedonation set sddonid ='' where sddonid  is null
update #largedonation set sddisplayname ='' where sddisplayname  is null
update #largedonation set sdlname ='' where sdlname  is null
update #largedonation set sdfname ='' where sdfname  is null
update #largedonation set sdemail='' where sdemail is null
update #largedonation set sdaddr1='' where sdaddr1 is null
update #largedonation set sdaddr2= '' where sdaddr2 is null
update #largedonation set sdcity='' where sdcity is null
update #largedonation set sdst='' where sdst is null
update #largedonation set sdzip='' where sdzip is null

select distinct 
	ld.amount as 'Distribution Amount', 
	ld.donationid as 'Donation ID',
	ld.dondate as 'Donation Date',
	ld.donamount as 'Donation Amount', 
	ld.donationstatus as 'Donation Status',
	ld.paymenttype as 'Payment Type',
	ld.progname as 'Fund',
	ld.cdisplayname as 'Giver Name',
	ld.clname as 'Giver Last Name',
	ld.cfname as 'Giver First Name',
	ld.cemail as 'Giver Email',
	ld.cgdisplayname as 'Spouse Name',
	ld.cglname as 'Spouse Last Name',
	ld.cgfname as 'Spouse First Name', 
	ld.cgemail as 'Spouse Email',
	ld.addr1 as 'Address1', 
	ld.addr2 as 'Address2',
	ld.city as 'City',
	ld.st as 'State',
	ld.zip as 'Zip',
	ld.conname as 'Site Name',
	ld.sddisplayname as 'Soft Credit Donor Name',
	ld.sdlname as 'Soft Credit Donor Last Name',
	ld.sdfname as 'Soft Credit Donor First Name', 
	ld.sdemail as 'Soft Credit Donor Email', 
	ld.sdaddr1 as 'Soft Credit Address1',
	ld.sdaddr2 as 'Soft Credit Address2',
	ld.sdcity as 'Soft Credit City', 
	ld.sdst as 'Soft Credit State',
	ld.sdzip as 'Soft Credit Zip',
	ld.notes as 'Description'
from #largedonation ld
where ld.cdisplayname not like 'Offering%'
order by ld.cdisplayname,ld.progname,ld.dondate

END

GO


