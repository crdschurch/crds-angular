USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_New_Givers]    Script Date: 6/28/2017 2:30:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[report_CRDS_New_Givers] 
     @startdate DATETIME,
     @enddate DATETIME,
	 @programid as varchar(MAX),
	 @congregationid as varchar(MAX),
	 @donationstatusid as varchar(MAX),
	 @accountingcompanyid as varchar(MAX)
AS

BEGIN
SET nocount ON;

-- Normalize dates to remove time and ensure end date is inclusive
SET @startdate = CONVERT(DATE, @startdate)
SET @enddate = CONVERT(DATE, @enddate + 1)

IF OBJECT_ID('tempdb..#cogivers') IS NOT NULL   
	DROP TABLE #cogivers
IF OBJECT_ID('tempdb..#ftdonormaster') IS NOT NULL   
	DROP TABLE #ftdonormaster
IF OBJECT_ID('tempdb..#ftdonordetail') IS NOT NULL   
	DROP TABLE #ftdonordetail
IF OBJECT_ID('tempdb..#cogivercleanup') IS NOT NULL   
	DROP TABLE #cogivercleanup
IF OBJECT_ID('tempdb..#scfirstdon') IS NOT NULL
	DROP TABLE #scfirstdon
IF OBJECT_ID('tempdb..#scdonorcleanup') IS NOT NULL   
DROP TABLE #scdonorcleanup
IF OBJECT_ID('tempdb..#nonstandardhh') IS NOT NULL   
	DROP TABLE #nonstandardhh

--build cogiver/spouse model
CREATE TABLE #cogivers
(
	cid1	int,
	cbday   date,
	cstmttype int,
	cid2	int,
	cgbday  date,
	cgstmttype int,
	hhid	int
)


insert into #cogivers
	select c1.contact_id,c1.Date_of_Birth,d1.statement_type_id, c2.contact_id, c2.Date_of_Birth,d2.statement_type_id,c1.household_id
	FROM contacts c1 
	JOIN contacts c2 on c1.household_id = c2.household_id
	join donors d1 on d1.contact_id = c1.contact_id
	join donors d2 on d2.contact_id = c2.contact_id
	where c1.household_position_id = 1
	and c2.household_position_id =1 --head of household
	and (d1.statement_type_id = 2 -- family
	or d2.Statement_Type_ID = 2)
	and c1.contact_id <> c2.contact_id

--store donor information	
CREATE TABLE #ftdonormaster
(
	donor_id int,
	doncontact_id int,
	donation_id int,
	donation_amount money,
	donation_date datetime,
	firstdondate datetime,
	program_id int,
	statementtype varchar(25),
	donationstatus varchar(25),
	household_pos_id int,
	soft_credit_donor int,
	congregation_id int
)

--first select is to grab regular donor information, second select is to also grab Soft Credit information
insert into #ftdonormaster(donor_id,doncontact_id,donation_id,donation_amount,program_id,donation_date,firstdondate,statementtype,donationstatus,household_pos_id,soft_credit_donor, congregation_id)
select distinct d.donor_id,d.contact_id, don.donation_id, don.donation_amount,dd.program_id,don.donation_date,d._first_donation_date as firstdonationdate, st.statement_type,ds.donation_status,c.Household_Position_ID,dd.soft_credit_donor,dd.congregation_id
	from dbo.donors d 
	left join dbo.donations don on don.donor_id = d.donor_id and don.donation_date = d._first_donation_date
	inner join dbo.donation_distributions dd on dd.donation_id = don.donation_id
	join dbo.Donation_Statuses ds on ds.donation_status_id = don.donation_status_id
	join dbo.Statement_Types st on st.Statement_Type_ID = d.Statement_Type_ID
	join dbo.Contacts c on c.Contact_ID = d.Contact_ID
	left join congregations AS con ON con.congregation_id = dd.Congregation_ID
where d._first_donation_date >= @startdate AND d._first_donation_date < @enddate
	and don.Donation_Date >= @startdate AND don.Donation_Date < @enddate
	and dd.Soft_Credit_Donor is null
	and don.Donation_Amount > 0
	AND dd.Congregation_ID IN (SELECT Item FROM dbo.dp_Split(@congregationid, ','))
	and dd.program_id IN (SELECT Item FROM dbo.dp_Split(@programid, ','))
	and don.donation_status_id IN (SELECT Item FROM dbo.dp_Split(@donationstatusid, ','))
	AND con.Accounting_Company_Id IN (SELECT Item FROM dbo.dp_Split(@accountingcompanyid, ','))
group by d.donor_id, d.contact_id,don.donation_id, don.donation_amount,dd.program_id,don.donation_date,d._First_Donation_Date,st.statement_type, ds.donation_status,c.Household_Position_ID,dd.Soft_Credit_Donor, dd.congregation_id
union all
select distinct d.donor_id,d.contact_id, don.donation_id, don.donation_amount,dd.program_id,don.donation_date,null as firstdonationdate, st.statement_type,ds.donation_status,c.Household_Position_ID,dd.soft_credit_donor, dd.congregation_id
	from dbo.donors d 
	left join dbo.donations don on don.donor_id = d.donor_id
	inner join dbo.donation_distributions dd on dd.donation_id = don.donation_id
	join dbo.Donation_Statuses ds on ds.donation_status_id = don.donation_status_id
	join dbo.Statement_Types st on st.Statement_Type_ID = d.Statement_Type_ID
	join dbo.Contacts c on c.Contact_ID = d.Contact_ID
	left join congregations AS con ON con.congregation_id = dd.Congregation_ID
where don.Donation_Date >= @startdate AND don.Donation_Date < @enddate  --we can't put d._first_donation_date >= @startdate and d._first_donation_date < @enddate here because the Donor is going to be the bank, and we'd need the soft credit donor
	and dd.Soft_Credit_Donor is not null
	and don.Donation_Amount > 0
	AND dd.Congregation_ID IN (SELECT Item FROM dbo.dp_Split(@congregationid, ','))
	and dd.program_id IN (SELECT Item FROM dbo.dp_Split(@programid, ','))
	and don.donation_status_id IN (SELECT Item FROM dbo.dp_Split(@donationstatusid, ','))
	AND con.Accounting_Company_Id IN (SELECT Item FROM dbo.dp_Split(@accountingcompanyid, ','))
group by d.donor_id, d.contact_id,don.donation_id, don.donation_amount,program_id,don.donation_date,st.statement_type, ds.donation_status,c.Household_Position_ID,dd.Soft_Credit_Donor, dd.congregation_id

--store Soft Credit Donor's first donation
Create Table #scfirstdon
(
	scdonor_id int,
	scfirstdondate datetime
)

insert into #scfirstdon(scdonor_id,scfirstdondate)
select dd.soft_credit_donor,min(don.Donation_Date) as first_don_date
from donations don 
	join donors d on d.Donor_ID = don.Donor_ID
	join dbo.donation_distributions dd on dd.donation_id = don.Donation_ID
	where dd.Soft_Credit_Donor in (select distinct ftm1.soft_credit_donor from #ftdonormaster ftm1)
	group by dd.soft_credit_donor

--We dont' want the first_don_date to just be the Soft Credit donors first soft credit date, it should be the first any donation date
update #scfirstdon
set scfirstdondate = d._First_Donation_Date
FROM #scfirstdon sf
inner join donors d on d.donor_ID = sf.scdonor_id
where d._First_Donation_Date < sf.scfirstdondate


--populate the Soft Credit donors first donation into the master table
update ftm
set firstdondate = sf.scfirstdondate
from #ftdonormaster ftm
join #scfirstdon sf on sf.scdonor_id = ftm.soft_credit_donor

--store/build all the additional detail about the donor
CREATE TABLE #ftdonordetail
(
	hhid int,
	donation_id int,
	donation_date datetime,
	firstdononationdate datetime,
	donation_amount money,
	programid int,
	programname varchar(150),
	cid1 int,
	cbday date,
	cdisplayname varchar(150),
	cfname varchar(50),
	clname varchar(50),
	cid2 int,
	cgbday date,
	cgdisplayname varchar(150),
	cgfname varchar(50),
	cglname varchar(50),
	addr1 varchar(150),
	addr2 varchar(150),
	city varchar(150),
	st varchar(150),
	zip varchar(150),
	site varchar(150),
	to_remove int,
	statementtype varchar(25),
	donationstatus varchar(25),
	sddonid int,
	sddisplayname varchar(50),
	sdaddr1 varchar(150),
	sdaddr2 varchar(150),
	sdcity varchar(150),
	sdst varchar(150),
	sdzip varchar(150)
	)
insert into #ftdonordetail(hhid,donation_id,donation_date,firstdononationdate,donation_amount,programid, programname,cid1,cbday,cdisplayname,cfname,clname,cid2,cgbday,cgdisplayname,cgfname,cglname,addr1,addr2,city,st,zip,site,statementtype,donationstatus,sddonid)
select h.Household_ID,
ftdm.donation_id,
ftdm.donation_date,
ftdm.firstdondate,
ftdm.donation_amount,
ftdm.program_id,
(select distinct p.program_name 
	from dbo.programs p 
		join #ftdonormaster on p.program_id = ftdm.program_id) as programname,
c.Contact_ID,c.Date_of_Birth,c.Display_Name,c.First_Name,c.Last_Name,cg.cid2,cg.cgbday,c2.display_name as cogivername,c2.First_Name,c2.Last_Name,
Address_Line_1,Address_Line_2,City,[State/Region],Postal_Code,con.congregation_name, ftdm.statementtype,ftdm.donationstatus,ftdm.soft_credit_donor
	from #ftdonormaster ftdm 
	left join contacts c on c.Contact_ID = ftdm.doncontact_id
	left join #cogivers cg on cg.cid1 = ftdm.doncontact_id
	left join contacts c2 on c2.contact_id = cg.cid2
	left join households h on h.household_id = c.Household_ID
	left join congregations con on con.congregation_id = ftdm.congregation_id
	left join Programs p on p.Program_ID = ftdm.program_id
	left join addresses a on a.address_id = h.address_id
	


update #ftdonordetail set cbday='' where cbday is null
update #ftdonordetail set cgbday='' where cgbday is null
update #ftdonordetail set cdisplayname='' where cdisplayname is null
update #ftdonordetail set cfname ='' where cfname  is null
update #ftdonordetail set clname ='' where clname  is null
update #ftdonordetail set cid2 ='' where cid2  is null
update #ftdonordetail set cgdisplayname ='' where cgdisplayname  is null
update #ftdonordetail set cgfname ='' where cgfname  is null
update #ftdonordetail set cglname ='' where cglname  is null
update #ftdonordetail set addr1='' where addr1 is null
update #ftdonordetail set addr2='' where addr2 is null
update #ftdonordetail set city='' where city is null
update #ftdonordetail set st='' where st is null
update #ftdonordetail set zip='' where zip is null

--add all the Soft Credit contact information
update ftd
set sddisplayname = c.display_name, 
sdaddr1=a.Address_Line_1,
sdaddr2=a.Address_line_2,
	sdcity = a.City,
	sdst = a.[State/Region],
	sdzip = a.Postal_Code
from #ftdonordetail ftd
join donors d on d.donor_id = ftd.sddonid
join contacts c on c.contact_id = d.contact_id
left join households h on h.household_id = c.household_id
left join addresses a on a.address_id = h.address_id
where ftd.sddonid is not null

update #ftdonordetail set sddonid ='' where sddonid  is null
update #ftdonordetail set sddisplayname ='' where sddisplayname  is null
update #ftdonordetail set sdaddr1='' where sdaddr1 is null
update #ftdonordetail set sdaddr2= '' where sdaddr2 is null
update #ftdonordetail set sdcity='' where sdcity is null
update #ftdonordetail set sdst='' where sdst is null
update #ftdonordetail set sdzip='' where sdzip is null

--Grab GoGivers that have previously donated for a particular program
Create Table #cogivercleanup
(
	hhid int,
	contact_id int,
	donation_id int
)

insert into #cogivercleanup(hhid,contact_id,donation_id)
select distinct f.hhid,f.cid2,f.donation_id from #ftdonordetail f 
join dbo.contacts c on c.Contact_ID = f.cid2
join dbo.Donors d on d.Contact_ID = c.Contact_ID
where f.cid2 is not null
and d._First_Donation_Date < @startdate
and f.programid in (SELECT Item FROM dbo.dp_Split(@programid, ','))

--remove the records where an associated CoGiver has previously donated
delete fd from #ftdonordetail fd join #cogivercleanup cgc on cgc.hhid = fd.hhid and cgc.donation_id = fd.donation_id and cgc.contact_id = fd.cid2

--Grab Soft Credit donors that have previously donated for a particular program
Create Table #scdonorcleanup
(
	hhid int,
	sccontact_id int,
	donation_id int
)

insert into #scdonorcleanup(hhid,sccontact_id,donation_id)
select distinct f.hhid,f.sddonid,f.donation_id from #ftdonordetail f 
join dbo.donors d on d.Donor_ID = f.sddonid
where f.firstdononationdate < @startdate
and f.sddonid is not null
and f.programid in (SELECT Item FROM dbo.dp_Split(@programid, ','))

--remove the records where an associated Soft Credit donor has previously donated
delete fd from #ftdonordetail fd join #scdonorcleanup scd on scd.hhid = fd.hhid and scd.donation_id = fd.donation_id and scd.sccontact_id = fd.sddonid

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

select
	f.hhid as HHID,
	f.donation_id as 'Donation ID',
	f.donation_date as 'Donation Date',
	f.donation_amount as 'Donation Amount',
	f.programname as Program,
	f.cid1 as 'Contact Id',
	f.cbday as 'Birth Day',
	f.cdisplayname as 'Display Name',
	f.cfname as 'First Name',
	f.clname as 'Last Name',
	f.cid2 as 'Spouse Id',
	f.cgdisplayname as 'Spouse Display Name',
	f.cgfname as 'Spouse FName',
	f.cglname as 'Spouse LName',
	f.addr1 as 'Address1',
	f.addr2 as 'Address2',
	f.city as 'City',
	f.st as 'State',
	f.zip as 'Zip',
	f.site as 'Site',
	f.statementtype as 'Statement Type',
	f.donationstatus as 'Donation Status',
	f.sddonid as 'Soft Credit Donor ID',
	f.sddisplayname as 'Soft Credit Display Name',
	f.sdaddr1 as 'Soft Credit Address1',
	f.sdaddr2 as 'Soft Credit Address2',
	f.sdcity as 'Soft Credit City',
	f.sdst as 'Soft Credit State',
	f.sdzip as 'Soft Credit Zip'
from
	#ftdonordetail f 
	left join #nonstandardhh nshh on nshh.hhid = f.hhid
where
	nshh.hhid is null
order by
	f.cdisplayname,f.cid1,f.programname,f.donation_date asc

END

GO


