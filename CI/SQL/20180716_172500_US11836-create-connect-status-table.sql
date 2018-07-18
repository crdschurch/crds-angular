USE MinistryPlatform
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cr_Connect_Status](
	[Connect_Status_ID] [int] IDENTITY(1,1) NOT NULL,
	[Connect_Status] [nvarchar](100) NOT NULL,
	[Domain_ID] [int] NOT NULL,
 CONSTRAINT [PK_ConnectStatus] PRIMARY KEY CLUSTERED 
(
	[Connect_Status_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[cr_Connect_Status]  WITH CHECK ADD  CONSTRAINT [FK_Connect_Status_Domains] FOREIGN KEY([Domain_ID])
REFERENCES [dbo].[dp_Domains] ([Domain_ID])
GO

ALTER TABLE [dbo].[cr_Connect_Status] CHECK CONSTRAINT [FK_Connect_Status_Domains]
GO

SET IDENTITY_INSERT cr_Connect_Status ON
INSERT INTO cr_Connect_Status(Connect_Status_ID,Connect_Status,Domain_ID) VALUES (1,'Added To the Map', 1)
INSERT INTO cr_Connect_Status(Connect_Status_ID,Connect_Status,Domain_ID) VALUES (2,'Removed From the Map', 1)
SET IDENTITY_INSERT cr_Connect_Status OFF
GO
