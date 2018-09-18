USE [MinistryPlatform]
GO

-- disable existing MP index
ALTER INDEX IX_Selected_Records_Record_ID ON dp_Selected_Records DISABLE;
GO

-- add new index that covers more queries
CREATE INDEX IX_dp_Selected_Records__SelectionID_RecordID ON dp_Selected_Records(Selection_ID, Record_ID);
GO
