USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===============================================================
-- Author: Phil Lachmann	
-- Create date: 7/24/2016
-- Description:	Delete Request Dates for Childcare Request
-- ===============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_DeleteDatesForChildcareRequest]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_DeleteDatesForChildcareRequest] AS' 
END
GO


ALTER PROCEDURE [dbo].[api_crds_DeleteDatesForChildcareRequest]
	-- Add the parameters for the stored procedure here
	@ChildcareRequestID int
AS
BEGIN
	DELETE cr_Childcare_Request_Dates WHERE Childcare_Request_Id = @ChildcareRequestID
END