USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cr_Coaches](
	[Coach_ID] [int] IDENTITY(1,1) NOT NULL,
	[Leader_Contact_ID] [int] NOT NULL,
	[Coach_Contact_ID] [int] NOT NULL,
	[Domain_ID] [int] NOT NULL,
	[Start_Date] [datetime] NOT NULL,
	[End_Date] [datetime] NULL,
 CONSTRAINT [PK_cr_Coaches] PRIMARY KEY CLUSTERED 
(
	[Coach_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[cr_Coaches]  WITH CHECK ADD  CONSTRAINT [FK_Coaches_Contact_Leader] FOREIGN KEY([Leader_Contact_ID])
	REFERENCES [dbo].[Contacts] ([Contact_ID])
ALTER TABLE [dbo].[cr_Coaches] CHECK CONSTRAINT [FK_Coaches_Contact_Leader]

ALTER TABLE [dbo].[cr_Coaches]  WITH CHECK ADD  CONSTRAINT [FK_Coaches_Contact_Coach] FOREIGN KEY([Coach_Contact_ID])
	REFERENCES [dbo].[Contacts] ([Contact_ID])
ALTER TABLE [dbo].[cr_Coaches] CHECK CONSTRAINT [FK_Coaches_Contact_Coach]

ALTER TABLE [dbo].[cr_Coaches]  WITH CHECK ADD  CONSTRAINT [FK_Coaches_Domain] FOREIGN KEY([Domain_ID])
	REFERENCES [dbo].[dp_Domains] ([Domain_ID])
ALTER TABLE [dbo].[cr_Coaches] CHECK CONSTRAINT [FK_Coaches_Domain]

GO


