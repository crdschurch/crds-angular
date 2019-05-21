USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS(SELECT name FROM sys.objects WHERE name = N'crds_GetAtrributeStringForAWS')
  DROP FUNCTION dbo.crds_GetAtrributeStringForAWS;
GO

-- =============================================
-- Author:      Phil Lachmann
-- Create date: 05/31/2017
-- Description: Get Group Attributes formatted for Cloudsearch
-- =============================================
CREATE FUNCTION [dbo].[crds_GetAtrributeStringForAWS](@GroupId INT, @AttributeTypeId INT, @collectionFormat bit)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @Categories AS NVARCHAR(MAX);
	SELECT @Categories = STUFF((
		SELECT 
			   ', '  + A.Attribute_Name 
		FROM  
			   Group_Attributes GA, Attributes A 
			      LEFT OUTER JOIN Attribute_Categories AC ON A.Attribute_Category_ID = AC.Attribute_Category_ID
				  LEFT OUTER JOIN Attribute_Types ATT ON ATT.Attribute_Type_ID = A.Attribute_Type_ID
		WHERE  
			   GA.Attribute_ID = A.Attribute_ID
			   AND GA.Group_ID = @GroupId
			   AND GETDATE() BETWEEN GA.Start_Date AND ISNULL(GA.End_Date,GETDATE())
			   AND A.Attribute_Type_ID = @AttributeTypeId
        FOR XML PATH('')), 1, 2, '');

	RETURN @Categories;
END

GO
