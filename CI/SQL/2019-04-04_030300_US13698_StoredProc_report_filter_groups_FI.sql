USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_filter_groups_FI]    Script Date: 4/1/2019 4:01:04 PM ******/
DROP PROCEDURE [dbo].[report_filter_groups_FI]
GO

/****** Object:  StoredProcedure [dbo].[report_filter_groups_FI]    Script Date: 4/1/2019 4:01:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
USE [MinistryPlatform]
GO

DECLARE	@return_value int

EXEC	@return_value = [dbo].[report_filter_groups_FI]
		@DomainID = N'0FDE7F32-37E3-4E0B-B020-622E0EBD6BF0',
		@UserID = N'DDADDBCB-8823-4F06-9250-6B245FA82755',
		@PageID = 322,
		@Congregation = N'1,6,7'

SELECT	'Return Value' = @return_value

GO

*/


CREATE PROCEDURE [dbo].[report_filter_groups_FI]

	@DomainID varchar(40)
	,@UserID varchar(40)
	,@PageID Int
	,@GroupTypeID INT = NULL
	,@Congregation VARCHAR(MAX) = '0'

AS
BEGIN

SELECT G.Group_Name, G.Group_ID
FROM Groups G
 INNER JOIN dp_Domains ON dp_Domains.Domain_ID = G.Domain_ID

WHERE dp_Domains.Domain_GUID = @DomainID
 AND G.Group_Type_ID = ISNULL(@GroupTypeID, G.Group_Type_ID)
 AND CAST(GETDATE() AS DATE) BETWEEN CAST(G.Start_Date AS DATE) AND CAST(ISNULL(G.End_Date,GETDATE()) AS DATE) 
 AND (G.Congregation_ID IN (SELECT * FROM dp_Split(@Congregation, ',')) OR ISNULL(@Congregation,'0') = '0')
 AND G.Group_Type_ID = 9 and Ministry_ID = 11


END

/****** Object:  StoredProcedure [dbo].[report_weekend_service_Crossroads]    Script Date: 9/16/2016 5:34:59 PM ******/
SET ANSI_NULLS ON
GO


