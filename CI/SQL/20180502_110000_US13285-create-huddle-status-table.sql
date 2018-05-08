USE MinistryPlatform
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cr_Huddle_Status](
	[Huddle_Status_ID] [int] IDENTITY(1,1) NOT NULL,
	[Huddle_Status] [nvarchar](100) NOT NULL,
	[Domain_ID] [int] NOT NULL,
 CONSTRAINT [PK_HuddleStatus] PRIMARY KEY CLUSTERED 
(
	[Huddle_Status_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[cr_Huddle_Status]  WITH CHECK ADD  CONSTRAINT [FK_Huddle_Status_Domains] FOREIGN KEY([Domain_ID])
REFERENCES [dbo].[dp_Domains] ([Domain_ID])
GO

ALTER TABLE [dbo].[cr_Huddle_Status] CHECK CONSTRAINT [FK_Huddle_Status_Domains]
GO

SET IDENTITY_INSERT cr_Huddle_Status ON
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (1,'Not Involved', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (2,'Never Finished Huddle', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (3,'Completed/Not interested', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (4,'Completed/Hasn''t Led', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (5,'Huddle Leader', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (6,'Has Led/Not Currently', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (7,'QTR 1', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (8,'QTR 2', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (9,'QTR 3', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (10,'QTR 4', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status_ID,Huddle_Status,Domain_ID) VALUES (11,'TBD', 1)
SET IDENTITY_INSERT cr_Huddle_Status OFF
GO
