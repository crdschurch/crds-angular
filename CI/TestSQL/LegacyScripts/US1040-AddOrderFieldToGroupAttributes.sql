USE [MinistryPlatform]
GO

ALTER TABLE [dbo].[Group_Attributes] DROP CONSTRAINT [FK_Group_Attributes_Groups]
GO

ALTER TABLE [dbo].[Group_Attributes] DROP CONSTRAINT [FK_Group_Attributes_dp_Domains]
GO

ALTER TABLE [dbo].[Group_Attributes] DROP CONSTRAINT [FK_Group_Attributes_Attributes]
GO

/****** Object:  Table [dbo].[Group_Attributes]    Script Date: 6/5/2015 4:04:39 PM ******/
DROP TABLE [dbo].[Group_Attributes]
GO

/****** Object:  Table [dbo].[Group_Attributes]    Script Date: 6/5/2015 4:04:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Group_Attributes](
  [Group_Attribute_ID] [int] IDENTITY(1,1) NOT NULL,
  [Attribute_ID] [int] NOT NULL,
  [Group_ID] [int] NOT NULL,
  [Domain_ID] [int] NOT NULL,
  [Start_Date] [datetime] NOT NULL,
  [End_Date] [datetime] NULL,
  [Notes] [nvarchar](255) NULL,
  [Order] [int] NULL
 CONSTRAINT [PK_Group_Attributes] PRIMARY KEY CLUSTERED 
(
  [Group_Attribute_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Group_Attributes]  WITH CHECK ADD  CONSTRAINT [FK_Group_Attributes_Attributes] FOREIGN KEY([Attribute_ID])
REFERENCES [dbo].[Attributes] ([Attribute_ID])
GO

ALTER TABLE [dbo].[Group_Attributes] CHECK CONSTRAINT [FK_Group_Attributes_Attributes]
GO

ALTER TABLE [dbo].[Group_Attributes]  WITH CHECK ADD  CONSTRAINT [FK_Group_Attributes_dp_Domains] FOREIGN KEY([Domain_ID])
REFERENCES [dbo].[dp_Domains] ([Domain_ID])
GO

ALTER TABLE [dbo].[Group_Attributes] CHECK CONSTRAINT [FK_Group_Attributes_dp_Domains]
GO

ALTER TABLE [dbo].[Group_Attributes]  WITH CHECK ADD  CONSTRAINT [FK_Group_Attributes_Groups] FOREIGN KEY([Group_ID])
REFERENCES [dbo].[Groups] ([Group_ID])
GO

ALTER TABLE [dbo].[Group_Attributes] CHECK CONSTRAINT [FK_Group_Attributes_Groups]
GO

UPDATE [dbo].[dp_Sub_Pages]
SET [Default_Field_List] = 'Attribute_ID_Table_Attribute_Type_ID_Table.Attribute_Type,Attribute_ID_Table.Attribute_Name,Group_Attributes.Start_Date,Group_Attributes.End_Date, Group_Attributes.[Order]'
WHERE [Sub_Page_ID] = 303
GO
