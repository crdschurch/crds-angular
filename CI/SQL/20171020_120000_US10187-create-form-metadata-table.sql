USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'cr_Form_Metadata')
BEGIN
	CREATE TABLE [dbo].[cr_Form_Metadata](
		[Form_Metadata_ID] [int] IDENTITY(1,1) NOT NULL,
		[Form_ID] [int] NOT NULL,
		[Form_Field_Name] [nvarchar](max) NOT NULL,
		[Metadata_Label] [nvarchar](200) NOT NULL,
		[Metadata_Value] [nvarchar](200) NOT NULL,
		[Start_Date] [datetime] NOT NULL,
		[End_Date] [datetime] NULL,
	 CONSTRAINT [PK_Form_Metadata] PRIMARY KEY CLUSTERED 
	(
		[Form_Metadata_ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	

	ALTER TABLE [dbo].[cr_Form_Metadata]  WITH CHECK ADD  CONSTRAINT [FK_Form_Metadata_Form] FOREIGN KEY([Form_ID])
	REFERENCES [dbo].[Forms] ([Form_ID])
	

	ALTER TABLE [dbo].[cr_Form_Metadata] CHECK CONSTRAINT [FK_Form_Metadata_Form]

END
GO
