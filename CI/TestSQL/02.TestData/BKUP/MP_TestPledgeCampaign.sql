USE [MinistryPlatform]
GO

DECLARE @startDate as VARCHAR(19)
set @startDate = CONVERT(VARCHAR(4), datepart(year, getdate()))+'0101';
DECLARE @endDate as VARCHAR(19)
DECLARE @endDateTime as Date
set @endDateTime = dateadd(year, 2, getdate())
set @endDate = CONVERT(VARCHAR(4), datepart(year,@endDateTime) )+'1231';
DECLARE @curr_pledgeCampaign_id AS INT
DECLARE @pledgeCampaignId AS INT
DECLARE @programId AS INT
DECLARE @donorId AS INT


--Pledge Set up for Head of the household1
--Program for (t) Test Pledge Program1

INSERT INTO [dbo].programs 
(Program_Name              ,Congregation_ID,Ministry_ID,[Start_Date]                     ,End_Date                       ,Program_Type_ID,Leadership_Team,Primary_Contact,Priority_ID,On_Connection_Card,Stewardship_Information,Tax_Deductible_Donations,Statement_Title           ,Statement_Header_ID,Allow_Online_Giving,Online_Sort_Order,Pledge_Campaign_ID,Account_Number,Default_Target_Event,On_Donation_Batch_Tool,Domain_ID,Available_Online,__ExternalFundID,Communication_ID) VALUES
('(t) Test Pledge Program1',5              ,8          ,CAST(@startDate as smalldatetime),CAST(@endDate as smalldatetime),1              ,null           ,2562428        ,null       ,null              ,null                   ,1                       ,'(t) Test Pledge Program1',1                  ,1                  ,2                ,null              ,null          ,null                ,1                     ,1        ,1               ,null            ,11402            );

--Pledge Campaign for (t) Test Pledge Campaign1
SET IDENTITY_INSERT Pledge_Campaigns ON;

SET @curr_pledgeCampaign_id = IDENT_CURRENT('Pledge_Campaigns');
SET @pledgeCampaignId = 10000010;
SET @programId = (SELECT Program_ID FROM Programs WHERE Program_Name = '(t) Test Pledge Program1');

INSERT INTO Pledge_Campaigns 
(Pledge_Campaign_ID,Campaign_Name                 ,Nickname                   ,Pledge_Campaign_Type_ID,[Description]          ,Campaign_Goal,[Start_Date]                     ,End_Date ,Domain_ID,Event_ID,Program_ID,Registration_Details,Registration_Start ,Registration_End ,Maximum_Registrants,Youngest_Age_Allowed,Registration_Deposit,Fundraising_Goal,Registration_Form,Online_Pledge_Details,Allow_Online_Pledge,Online_Thank_You_Message,Pledge_Beyond_End_Date,Show_On_My_Pledges,__ExternalTripID,__ExternalFundID) VALUES
(@pledgeCampaignId ,'(t) Test Pledge Campaign1'   ,'(t) Test Pledge Campaign1',1                      ,'Test Pledge Campaign1',10000000.00  ,CAST(@startDate as smalldatetime),null     ,1        ,null    ,@programId,null                ,null               ,null             ,null               , null               ,null                ,null            ,null             ,null                 ,1                  ,null                    ,1                     ,1                 ,40              ,99);

SET IDENTITY_INSERT [dbo].[Pledge_Campaigns] OFF;

DBCC CHECKIDENT (Pledge_Campaigns, reseed, @curr_pledgeCampaign_id);

--Pledge for (t) Test Pledge

SET @donorId = (SELECT Donor_ID FROM Donors WHERE Contact_ID IN (SELECT Contact_ID FROM Contacts WHERE Email_Address = 'mpcrds+tremplay.richard@gmail.com'));

INSERT INTO [dbo].Pledges
(Donor_ID, Pledge_Campaign_ID, Pledge_Status_ID, Total_Pledge, Installments_Planned, Installments_Per_Year, First_Installment_Date      , Notes, Domain_ID, Beneficiary, Trip_Leader, Currency, __ExternalPersonID1, __ExternalPersonID2, __ExternalCommitmentID, __ExternalApplicationID, Trip_General_Fund) VALUES
(@donorId, @pledgeCampaignId , 1               , 1000.00     , 10                  , 1000.00              ,  DATEADD(DAY, -3, GETDATE()), null , 1        , null       , null       , null    , null               , null               , null                  , null                   , null );               

--Set the pledge campaign for the program
UPDATE [dbo].Programs SET Pledge_Campaign_ID = 10000010 where program_name = '(t) Test Pledge Program1';
---------------------------------------------------------------------------------------------------------------------------
--Pledge Set up for Head of the household2
--Program for (t) Test Pledge Program2

INSERT INTO [dbo].programs 
(Program_Name              ,Congregation_ID,Ministry_ID,[Start_Date]                     ,End_Date                       ,Program_Type_ID,Leadership_Team,Primary_Contact,Priority_ID,On_Connection_Card,Stewardship_Information,Tax_Deductible_Donations,Statement_Title           ,Statement_Header_ID,Allow_Online_Giving,Online_Sort_Order,Pledge_Campaign_ID,Account_Number,Default_Target_Event,On_Donation_Batch_Tool,Domain_ID,Available_Online,__ExternalFundID,Communication_ID, Allow_Recurring_Giving) VALUES
('(t) Test Pledge Program2',5              ,8          ,CAST(@startDate as smalldatetime),CAST(@endDate as smalldatetime),1              ,null           ,2562428        ,null       ,null              ,null                   ,1                       ,'(t) Test Pledge Program2',1                  ,1                  ,2                ,null              ,null          ,null                ,1                     ,1        ,1               ,null            ,11402           , 1 );

--Pledge Campaign for (t) Test Pledge Campaign
SET IDENTITY_INSERT Pledge_Campaigns ON;

SET @curr_pledgeCampaign_id = IDENT_CURRENT('Pledge_Campaigns');
SET @pledgeCampaignId = 10000011;
SET @programId = (SELECT Program_ID FROM Programs WHERE Program_Name = '(t) Test Pledge Program2');

INSERT INTO Pledge_Campaigns 
(Pledge_Campaign_ID,Campaign_Name                 ,Nickname                   ,Pledge_Campaign_Type_ID,[Description]  ,Campaign_Goal,[Start_Date]                     ,End_Date ,Domain_ID,Event_ID,Program_ID,Registration_Details,Registration_Start ,Registration_End ,Maximum_Registrants,Youngest_Age_Allowed,Registration_Deposit,Fundraising_Goal,Registration_Form,Online_Pledge_Details,Allow_Online_Pledge,Online_Thank_You_Message,Pledge_Beyond_End_Date,Show_On_My_Pledges,__ExternalTripID,__ExternalFundID) VALUES
(@pledgeCampaignId ,'(t) Test Pledge Campaign2'   ,'(t) Test Pledge Campaign2',1                      ,null           ,1000.00      ,CAST(@startDate as smalldatetime),null     ,1        ,null    ,@programId,null                ,null               ,null             ,null               , null               ,null                ,null            ,null             ,null                 ,1                  ,null                    ,1                     ,1                 ,40              ,99              );

SET IDENTITY_INSERT [dbo].[Pledge_Campaigns] OFF;

DBCC CHECKIDENT (Pledge_Campaigns, reseed, @curr_pledgeCampaign_id);

--Update Program to point at pledge campaign
UPDATE [dbo].Programs SET Pledge_Campaign_ID = 10000011 where program_name = '(t) Test Pledge Program2';

--Pledge for (t) Test Pledge

SET @donorId = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE Email_Address = 'mpcrds+tremplay.mary@gmail.com'));

INSERT INTO Pledges
(Donor_ID, Pledge_Campaign_ID, Pledge_Status_ID, Total_Pledge, Installments_Planned, Installments_Per_Year, First_Installment_Date      , Notes, Domain_ID, Beneficiary, Trip_Leader, Currency, __ExternalPersonID1, __ExternalPersonID2, __ExternalCommitmentID, __ExternalApplicationID, Trip_General_Fund) VALUES
(@donorId, @pledgeCampaignId , 1               , 1000.00     , 10                  , 1000.00              ,  DATEADD(DAY, -3, GETDATE()), null , 1        , null       , null       , null    , null               , null               , null                  , null                   , null );               
---------------------------------------------------------------------------------------------------------------------------
--Automation Pledge Campaigns
--Program for (t) Test Pledge Program+Auto1

INSERT INTO [dbo].programs 
(Program_Name                   ,Congregation_ID,Ministry_ID,[Start_Date]                     ,End_Date                       ,Program_Type_ID,Leadership_Team,Primary_Contact,Priority_ID,On_Connection_Card,Stewardship_Information,Tax_Deductible_Donations,Statement_Title                ,Statement_Header_ID,Allow_Online_Giving,Online_Sort_Order,Pledge_Campaign_ID,Account_Number,Default_Target_Event,On_Donation_Batch_Tool,Domain_ID,Available_Online,__ExternalFundID,Communication_ID, Allow_Recurring_Giving) VALUES
('(t) Test Pledge Program+Auto1',5              ,8          ,CAST(@startDate as smalldatetime),CAST(@endDate as smalldatetime),1              ,null           ,2562428        ,null       ,null              ,null                   ,1                       ,'(t) Test Pledge Program+Auto1',1                  ,1                  ,2                ,null              ,null          ,null                ,1                     ,1        ,1               ,null            ,11402           , 1 );

--Pledge Campaign for (t) Test Pledge Campaign+Auto1
SET IDENTITY_INSERT Pledge_Campaigns ON;

SET @curr_pledgeCampaign_id = IDENT_CURRENT('Pledge_Campaigns');
SET @pledgeCampaignId = 10000012;
SET @programId = (SELECT Program_ID FROM Programs WHERE Program_Name = '(t) Test Pledge Program+Auto1');

INSERT INTO Pledge_Campaigns 
(Pledge_Campaign_ID,Campaign_Name                      ,Nickname                        ,Pledge_Campaign_Type_ID,[Description]               ,Campaign_Goal,[Start_Date]                     ,End_Date ,Domain_ID,Event_ID,Program_ID,Registration_Details,Registration_Start ,Registration_End ,Maximum_Registrants,Youngest_Age_Allowed,Registration_Deposit,Fundraising_Goal,Registration_Form,Online_Pledge_Details,Allow_Online_Pledge,Online_Thank_You_Message,Pledge_Beyond_End_Date,Show_On_My_Pledges,__ExternalTripID,__ExternalFundID) VALUES
(@pledgeCampaignId ,'(t) Test Pledge Campaign+Auto1'   ,'(t) Test Pledge Campaign+Auto1',1                      ,'Test Pledge Campaign+Auto1',10000000.00  ,CAST(@startDate as smalldatetime),null     ,1        ,null    ,@programId,null                ,null               ,null             ,null               , null               ,null                ,null            ,null             ,null                 ,1                  ,null                    ,1                     ,1                 ,40              ,99);

SET IDENTITY_INSERT [dbo].[Pledge_Campaigns] OFF;

DBCC CHECKIDENT (Pledge_Campaigns, reseed, @curr_pledgeCampaign_id);

--Set the pledge campaign for the program
UPDATE [dbo].Programs SET Pledge_Campaign_ID = 10000012 where program_name = '(t) Test Pledge Program+Auto1';

--Pledge for (t) Test Pledge Program+Auto1
SET @donorId = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE Email_Address = 'mpcrds+auto+1@gmail.com' AND Last_Name = 'Karpyshyn'));

INSERT INTO Pledges
(Donor_ID, Pledge_Campaign_ID, Pledge_Status_ID, Total_Pledge, Installments_Planned, Installments_Per_Year, First_Installment_Date      , Notes, Domain_ID, Beneficiary, Trip_Leader, Currency, __ExternalPersonID1, __ExternalPersonID2, __ExternalCommitmentID, __ExternalApplicationID, Trip_General_Fund) VALUES
(@donorId, @pledgeCampaignId , 1               , 50000.00     , 0                  , 0                    ,  DATEADD(DAY, -3, GETDATE()), null , 1        , null       , null       , null    , null               , null               , null                  , null                   , null );               
---------------------------------------------------------------------------------------------------------------------------
--Program for (t) Test Pledge Program+Auto2

INSERT INTO [dbo].programs 
(Program_Name                   ,Congregation_ID,Ministry_ID,[Start_Date]                     ,End_Date                       ,Program_Type_ID,Leadership_Team,Primary_Contact,Priority_ID,On_Connection_Card,Stewardship_Information,Tax_Deductible_Donations,Statement_Title                ,Statement_Header_ID,Allow_Online_Giving,Online_Sort_Order,Pledge_Campaign_ID,Account_Number,Default_Target_Event,On_Donation_Batch_Tool,Domain_ID,Available_Online,__ExternalFundID,Communication_ID, Allow_Recurring_Giving) VALUES
('(t) Test Pledge Program+Auto2',5              ,8          ,CAST(@startDate as smalldatetime),CAST(@endDate as smalldatetime),1              ,null           ,2562428        ,null       ,null              ,null                   ,1                       ,'(t) Test Pledge Program+Auto2',1                  ,1                  ,2                ,null              ,null          ,null                ,1                     ,1        ,1               ,null            ,11402           , 1 );

--Pledge Campaign for (t) Test Pledge Campaign
SET IDENTITY_INSERT Pledge_Campaigns ON;

SET @curr_pledgeCampaign_id = IDENT_CURRENT('Pledge_Campaigns');
SET @pledgeCampaignId = 10000013;
SET @programId = (SELECT Program_ID FROM Programs WHERE Program_Name = '(t) Test Pledge Program+Auto2');

INSERT INTO Pledge_Campaigns 
(Pledge_Campaign_ID,Campaign_Name                      ,Nickname                        ,Pledge_Campaign_Type_ID,[Description]  ,Campaign_Goal,[Start_Date]                     ,End_Date ,Domain_ID,Event_ID,Program_ID,Registration_Details,Registration_Start ,Registration_End ,Maximum_Registrants,Youngest_Age_Allowed,Registration_Deposit,Fundraising_Goal,Registration_Form,Online_Pledge_Details,Allow_Online_Pledge,Online_Thank_You_Message,Pledge_Beyond_End_Date,Show_On_My_Pledges,__ExternalTripID,__ExternalFundID) VALUES
(@pledgeCampaignId ,'(t) Test Pledge Campaign+Auto2'   ,'(t) Test Pledge Campaign+Auto2',1                      ,null           ,10000000.00  ,CAST(@startDate as smalldatetime),null     ,1        ,null    ,@programId,null                ,null               ,null             ,null               , null               ,null                ,null            ,null             ,null                 ,1                  ,null                    ,1                     ,1                 ,40              ,99              );

SET IDENTITY_INSERT [dbo].[Pledge_Campaigns] OFF;

DBCC CHECKIDENT (Pledge_Campaigns, reseed, @curr_pledgeCampaign_id);

--Update Program to point at pledge campaign
UPDATE [dbo].Programs SET Pledge_Campaign_ID = 10000013 where program_name = '(t) Test Pledge Program+Auto2';

--Pledge for (t) Test Pledge Program+Auto2
SET @donorId = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE Email_Address = 'mpcrds+auto+2@gmail.com' AND Last_Name = 'Kenobi'));

INSERT INTO Pledges
(Donor_ID, Pledge_Campaign_ID, Pledge_Status_ID, Total_Pledge, Installments_Planned, Installments_Per_Year, First_Installment_Date      , Notes, Domain_ID, Beneficiary, Trip_Leader, Currency, __ExternalPersonID1, __ExternalPersonID2, __ExternalCommitmentID, __ExternalApplicationID, Trip_General_Fund) VALUES
(@donorId, @pledgeCampaignId , 1               , 50000.00    , 0                   , 0                    ,  DATEADD(DAY, -3, GETDATE()), null , 1        , null       , null       , null    , null               , null               , null                  , null                   , null );               
