USE MinistryPlatform
GO

-- table to help handle syncing map elements to firestore db
CREATE TABLE dbo.cr_MapAudit 
(
  AuditID BIGINT IDENTITY(1,1) NOT NULL,
  Participant_ID [int],
  ShowOnMap [bit],
  Processed [bit],
  DateProcessed [datetime]
); 
GO