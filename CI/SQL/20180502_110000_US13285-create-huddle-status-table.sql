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

INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('Never Finished Huddle', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('Not Involved', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('Completed/Not interested', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('Completed/Hasn''t Led', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('Huddle Leader', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('Has Led/Not Currently', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('QTR 1', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('QTR 2', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('QTR 3', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('QTR 4', 1)
INSERT INTO cr_Huddle_Status(Huddle_Status,Domain_ID) VALUES ('TBD', 1)
GO
