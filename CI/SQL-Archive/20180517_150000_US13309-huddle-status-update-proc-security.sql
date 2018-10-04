USE MinistryPlatform
GO

DECLARE @PROCNAME VARCHAR(100) = 'crds_Huddle_Participant_Status_Refresh';

-- add to dp_api_procedures
IF NOT EXISTS(SELECT * FROM dp_API_Procedures WHERE Procedure_Name = @PROCNAME)
BEGIN
   INSERT INTO dp_API_Procedures(Procedure_Name) VALUES(@PROCNAME)
END

DECLARE @APIPROCID INTEGER;
SELECT @APIPROCID = API_Procedure_ID FROM dp_API_Procedures WHERE Procedure_Name = @PROCNAME


-- add to Administrators and unauthenticatedCreate
DECLARE @ROLEIDADMIN INTEGER = 2;
DECLARE @ROLEIDUNAUTHCREATE INTEGER = 62;

IF NOT EXISTS(SELECT * FROM dp_Role_API_Procedures WHERE API_Procedure_ID = @APIPROCID AND Role_ID = @ROLEIDADMIN)
BEGIN
   INSERT INTO dp_Role_API_Procedures(Role_ID, API_Procedure_ID, Domain_ID) VALUES(@ROLEIDADMIN, @APIPROCID, 1)
END

IF NOT EXISTS(SELECT * FROM dp_Role_API_Procedures WHERE API_Procedure_ID = @APIPROCID AND Role_ID = @ROLEIDUNAUTHCREATE)
BEGIN
   INSERT INTO dp_Role_API_Procedures(Role_ID, API_Procedure_ID, Domain_ID) VALUES(@ROLEIDUNAUTHCREATE, @APIPROCID, 1)
END
GO
