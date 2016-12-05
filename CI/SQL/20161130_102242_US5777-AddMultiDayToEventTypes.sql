USE [MinistryPlatform]
GO

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Event_Types ADD
	Allow_Multiday_Event bit NOT NULL CONSTRAINT DF_Event_Types_Allow_Multiday_Event DEFAULT 0
GO
ALTER TABLE dbo.Event_Types SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
GO
UPDATE dbo.Event_Types SET Allow_Multiday_Event = 1 WHERE EVENT_TYPE IN ('GO Trips', 'Camp');
GO
COMMIT