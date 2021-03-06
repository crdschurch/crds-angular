USE [MinistryPlatform]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'_Volunteer_Count' AND Object_ID = Object_ID(N'cr_Projects'))
BEGIN
ALTER TABLE dbo.cr_Projects DROP COLUMN _Volunteer_Count
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[crds_GoVolunteerProjectMemberCount]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'


ALTER FUNCTION [dbo].[crds_GoVolunteerProjectMemberCount](@ProjectId INT)
RETURNS INT
AS
BEGIN
    RETURN(
	   select SUM(r._Contributor_Count)
	   from dbo.cr_Group_Connectors gc
	   inner join dbo.cr_Group_Connector_Registrations gcr on gc.Group_Connector_ID = gcr.Group_Connector_ID
	   inner join dbo.cr_Registrations r on gcr.Registration_ID = r.Registration_ID
	   where gc.Project_ID = @ProjectId and ISNULL(r.Cancelled, 0) != 1
    )
END


' 
END

ALTER TABLE cr_Projects
    ADD [_Volunteer_Count]  AS ([dbo].[crds_GoVolunteerProjectMemberCount](Project_ID));
GO