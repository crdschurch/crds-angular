USE MinistryPlatform
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cr_Connect_History](
	[Connect_History_ID] [int] IDENTITY(1,1) NOT NULL,
	[Participant_ID] [int] NOT NULL,
	[Connect_Status_ID] [int] NOT NULL,
	[Transaction_Date] [datetime] NOT NULL default GETDATE(),
	[Domain_ID] [int] NOT NULL,
 CONSTRAINT [PK_ConnectHistory] PRIMARY KEY CLUSTERED 
(
	[Connect_History_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

-- FK Domain
ALTER TABLE [dbo].[cr_Connect_History]  WITH CHECK ADD  CONSTRAINT [FK_Connect_History_Domains] FOREIGN KEY([Domain_ID])
REFERENCES [dbo].[dp_Domains] ([Domain_ID])
GO

ALTER TABLE [dbo].[cr_Connect_History] CHECK CONSTRAINT [FK_Connect_History_Domains]
GO

-- FK Connect Status
ALTER TABLE [dbo].[cr_Connect_History]  WITH CHECK ADD  CONSTRAINT [FK_Connect_History_Connect_Status] FOREIGN KEY([Connect_Status_ID])
REFERENCES [dbo].[cr_Connect_Status] ([Connect_Status_ID])
GO

ALTER TABLE [dbo].[cr_Connect_History] CHECK CONSTRAINT [FK_Connect_History_Connect_Status]
GO

-- FK Participant
ALTER TABLE [dbo].[cr_Connect_History]  WITH CHECK ADD  CONSTRAINT [FK_Connect_History_Participant] FOREIGN KEY([Participant_ID])
REFERENCES [dbo].[Participants] ([Participant_ID])
GO

ALTER TABLE [dbo].[cr_Connect_History] CHECK CONSTRAINT [FK_Connect_History_Participant]
GO
