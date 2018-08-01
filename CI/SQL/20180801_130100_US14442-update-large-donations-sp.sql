USE [MinistryPlatform]
GO

CREATE PROCEDURE [dbo].[report_CRDS_Large_Donations] 
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


IF OBJECT_ID('tempdb..#ldcogivers') IS NOT NULL   
    DROP TABLE #ldcogivers
IF OBJECT_ID('tempdb..#largedonation') IS NOT NULL   
    DROP TABLE #largedonation
IF OBJECT_ID('tempdb..#nonstandardhh') IS NOT NULL
    DROP TABLE #nonstandardhh


--build cogiver/spouse model
CREATE TABLE #ldcogivers
(
    cid1	int,
    cbday   date,
    cstmttype int,
    cid2	int,
    cgbday  date,
    cgstmttype int,
    hhid	int
)


insert into #ldcogivers
    select distinct c1.contact_id,c1.Date_of_Birth,d1.statement_type_id, c2.contact_id, c2.Date_of_Birth,d2.statement_type_id,c1.household_id
    FROM contacts c1 
    JOIN contacts c2 on c1.household_id = c2.household_id
    join donors d1 on d1.contact_id = c1.contact_id
    join donors d2 on d2.contact_id = c2.contact_id
    where c1.household_position_id = 1
    and c2.household_position_id =1 --head of household
    and (d1.statement_type_id = 2 -- family
    or d2.Statement_Type_ID = 2)
    and c1.contact_id <> c2.contact_id
    and c1.Display_Name <> c2.Display_Name


CREATE TABLE #largedonation
(
    hhid int,
    amount	money,
    donationid int,
    dondate	datetime,
    donamount money,
    donationstatus varchar(25),
    progname varchar(150),
    cid1	int,
    cdisplayname varchar(150),
    clname varchar(50),
    cfname varchar(50),
    cid2	int,
    cgdisplayname varchar(150),
    cglname varchar(50),
    cgfname varchar(50),
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
    sdaddr1 varchar(50),
    sdaddr2 varchar(50),
    sdcity varchar(50),
    sdst varchar(50),
    sdzip varchar(50)
)

insert into #largedonation
select 
h.household_id
,dd.Amount
, don.donation_id
, don.Donation_date
, don.donation_amount
, ds.Donation_Status
, p.[program_name]
, c.contact_id as contact1
, c.display_name
, c.last_name
, c.first_name
, cg.cid2 as contact2
, c2.display_name as cogiver
, c2.last_name
, c2.First_Name
, a.Address_Line_1
, a.Address_Line_2
, a.City
, a.[State/Region]
, a.Postal_Code
,con.Congregation_Name
, dd.Program_ID
,dd.Soft_Credit_Donor
,null,null,null,null,null,null,null,null
from donations don
join donation_distributions dd on dd.donation_id = don.donation_id
left join programs p on p.program_id = dd.program_id
left join donors d on d.donor_id = don.donor_id
left join contacts c on c.contact_id=d.contact_id
left join #ldcogivers cg on cg.cid1 = c.Contact_ID
left join contacts c2 on c2.contact_id = cg.cid2
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
update #largedonation set cgdisplayname ='' where cgdisplayname  is null
update #largedonation set cglname ='' where cglname  is null
update #largedonation set cgfname ='' where cgfname  is null
update #largedonation set addr1='' where addr1 is null
update #largedonation set addr2='' where addr2 is null
update #largedonation set city='' where city is null
update #largedonation set st='' where st is null
update #largedonation set zip='' where zip is null

update #largedonation set sddonid ='' where sddonid  is null
update #largedonation set sddisplayname ='' where sddisplayname  is null
update #largedonation set sdlname ='' where sdlname  is null
update #largedonation set sdfname ='' where sdfname  is null
update #largedonation set sdaddr1='' where sdaddr1 is null
update #largedonation set sdaddr2= '' where sdaddr2 is null
update #largedonation set sdcity='' where sdcity is null
update #largedonation set sdst='' where sdst is null
update #largedonation set sdzip='' where sdzip is null

CREATE TABLE #nonstandardhh
(
    hhid int
)

--take out households with greater than 2 Heads of Households that are active
insert into #nonstandardhh(hhid)
    select c.household_id from contacts c 
        where c.household_position_id = 1
        and c.Contact_Status_ID = 1
        group by c.household_id
        having count(c.contact_id) > 2


select distinct ld.amount as 'Distribution Amount', ld.donationid as 'Donation ID',ld.dondate as 'Donation Date',ld.donamount as 'Donation Amount', ld.donationstatus as 'Donation Status',ld.progname as 'Fund',ld.cdisplayname as 'Giver Name',
ld.clname as 'Giver Last Name',ld.cfname as 'Giver First Name',ld.cgdisplayname as 'Spouse Name',ld.cglname as 'Spouse Last Name',ld.cgfname as 'Spouse First Name', ld.addr1 as 'Address1', ld.addr2 as 'Address2',ld.city as 'City',ld.st as 'State',ld.zip as 'Zip',ld.conname as 'Site Name',
ld.sddisplayname as 'Soft Credit Donor Name',ld.sdlname as 'Soft Credit Donor Last Name',ld.sdfname as 'Soft Credit Donor First Name', ld.sdaddr1 as 'Soft Credit Address1',ld.sdaddr2 as 'Soft Credit Address2', ld.sdcity as 'Soft Credit City', ld.sdst as 'Soft Credit State',
ld.sdzip as 'Soft Credit Zip'
from #largedonation ld
left join #nonstandardhh nshh on nshh.hhid = ld.hhid
where nshh.hhid is null
and ld.cdisplayname not like 'Offering%'
order by ld.cdisplayname,ld.progname,ld.dondate

END
GO
