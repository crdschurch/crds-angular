USE [MinistryPlatform]
GO

-- ===================================================================================================
-- Author:      Shakila Rajaiah
-- Create date: 4/8/19
-- Description: This stored proc returns the groups for First Impressions (based on the congregations)
-- ===================================================================================================

CREATE OR ALTER PROCEDURE [dbo].[report_filter_groups_FI]
	@DomainID varchar(40)
	,@UserID varchar(40)
	,@PageID Int
	,@Congregation VARCHAR(MAX) = '0'

AS
BEGIN

SELECT G.Group_Name, G.Group_ID
FROM Groups G
 INNER JOIN dp_Domains ON dp_Domains.Domain_ID = G.Domain_ID

WHERE dp_Domains.Domain_GUID = @DomainID
 AND CAST(GETDATE() AS DATE) BETWEEN CAST(G.Start_Date AS DATE) AND CAST(ISNULL(G.End_Date,GETDATE()) AS DATE) 
 AND (G.Congregation_ID IN (SELECT * FROM dp_Split(@Congregation, ',')) OR ISNULL(@Congregation,'0') = '0')
 AND G.Group_Type_ID = 9 
 AND Ministry_ID = 11

END



