USE [MinistryPlatform]
GO
/****** Object:  UserDefinedFunction [dbo].[crds_GetGroupCategoryStringForAWS]    Script Date: 11/8/2017 11:26:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Phil Lachmann
-- Create date: 05/31/2017
-- Description: Get Group Categories formatted for Cloudsearch
-- =============================================
ALTER FUNCTION [dbo].[crds_GetGroupCategoryStringForAWS](@GroupId INT)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @Categories AS NVARCHAR(MAX);
	SELECT @Categories = STUFF((
		SELECT 
			   ', ' + AC.Attribute_Category + ':' + A.Attribute_Name
		FROM  
			   Group_Attributes GA, Attributes A 
			      LEFT OUTER JOIN Attribute_Categories AC ON A.Attribute_Category_ID = AC.Attribute_Category_ID
				  LEFT OUTER JOIN Attribute_Types ATT ON ATT.Attribute_Type_ID = A.Attribute_Type_ID
		WHERE  
			   GA.Attribute_ID = A.Attribute_ID
			   AND GA.Group_ID = @GroupId
			   AND GETDATE() BETWEEN GA.Start_Date AND ISNULL(GA.End_Date,GETDATE())
			   AND AC.Attribute_Category_ID IN (
				   17, -- Life Stages
				   18, -- Neighborhoods
				   19, -- Spiritual Growth
				   20, -- Interest
				   21, -- Healing
				   51  -- Journey
			   )
        FOR XML PATH('')), 1, 2, '');
	
	RETURN @Categories ;
END

