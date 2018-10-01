USE MinistryPlatform
GO

-- table to help handle syncing map elements to firestore db
CREATE TABLE dbo.cr_MapAudit 
(
  AuditID BIGINT IDENTITY(1,1) PRIMARY KEY,
  Participant_ID [int],
  ShowOnMap [bit],
  Processed [bit],
  DateProcessed [datetime],
  PinType [nvarchar](50)
); 
GO