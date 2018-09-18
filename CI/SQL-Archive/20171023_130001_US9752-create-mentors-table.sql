USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cr_Mentors](
	[Mentor_ID] [int] IDENTITY(1,1) NOT NULL,
	[Coach_Contact_ID] [int] NOT NULL,
	[Mentor_Contact_ID] [int] NOT NULL,
	[Domain_ID] [int] NOT NULL,
	[Start_Date] [datetime] NOT NULL,
	[End_Date] [datetime] NULL,
 CONSTRAINT [PK_cr_Mentors] PRIMARY KEY CLUSTERED 
(
	[Mentor_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[cr_Mentors]  WITH CHECK ADD  CONSTRAINT [FK_Mentors_Contact_Mentor] FOREIGN KEY([Mentor_Contact_ID])
	REFERENCES [dbo].[Contacts] ([Contact_ID])
ALTER TABLE [dbo].[cr_Mentors] CHECK CONSTRAINT [FK_Mentors_Contact_Mentor]

ALTER TABLE [dbo].[cr_Mentors]  WITH CHECK ADD  CONSTRAINT [FK_Mentors_Contact_Coach] FOREIGN KEY([Coach_Contact_ID])
	REFERENCES [dbo].[Contacts] ([Contact_ID])
ALTER TABLE [dbo].[cr_Mentors] CHECK CONSTRAINT [FK_Mentors_Contact_Coach]

ALTER TABLE [dbo].[cr_Mentors]  WITH CHECK ADD  CONSTRAINT [FK_Mentors_Domain] FOREIGN KEY([Domain_ID])
	REFERENCES [dbo].[dp_Domains] ([Domain_ID])
ALTER TABLE [dbo].[cr_Mentors] CHECK CONSTRAINT [FK_Mentors_Domain]

GO


