USE [MinistryPlatform]
GO

CREATE UNIQUE INDEX [IX_GL_Account_Mapping_ProgramID_CongregationID_CustomerID] ON [dbo].[GL_Account_Mapping]
(
	[Program_ID] ASC,
	[Congregation_ID] ASC,
	[Customer_ID] ASC
)
GO


