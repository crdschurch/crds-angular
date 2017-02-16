U S E   [ M i n i s t r y P l a t f o r m ]  G O 
 
-- if the table doesn't exists, we need to do the work
I F   N O T   E X I S T S   ( S E L E C T   *   
      F R O M   InformationSchema.Tables
     WHERE Table_Schema = 'dbo'
     AND Table_Name='Host_Statuses')
BEGIN
  -- create new lookup table
  CREATE TABLE [dbo].[Host_Statuses](
    [Host_Status_ID] [int] IDENTITY(1,1) NOT NULL,
    [TextValue] [nvarchar](32),
  	[Domain_ID] [int] NOT NULL,
    [SortValue] [int] NOT NULL
  )

  -- add the lookup values
  INSERT INTO [dbo].[Host_Statuses](Host_Status_ID, TextValue, Domain_ID, SortValue)
    VALUES(0, 'Not Applied',  1, 100),
    VALUES(1, 'Pending',      1, 200),
    VALUES(2, 'Unapproved',   1, 300),
    VALUES(3, 'Approved',     1, 400);

  -- get rid of old column
  ALTER TABLE [dbo].[Participants] DROP [Approved_Host];

  -- add new column, with 'Not Applied' as the default
  ALTER TABLE [dbo].[Participants] ADD [Host_Status_ID] [int] DEFAULT 0;
END
