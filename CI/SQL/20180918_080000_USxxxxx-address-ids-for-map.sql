USE MinistryPlatform
GO

CREATE OR ALTER PROCEDURE [dbo].[crds_Get_Addressids_For_Map]
AS
BEGIN

-- get address ids missing lat/long information that are set to show on the connect map

SELECT a.Address_ID, a.Address_Line_1, a.Address_Line_2, a.City, a.[State/Region], a.Postal_Code, a.Foreign_Country, a.County, a.Longitude, a.Latitude FROM participants p 
  JOIN contacts c ON c.Contact_ID = p.Contact_ID
  LEFT JOIN Households H ON h.Household_ID = c.Household_ID
  LEFT JOIN Addresses A ON a.Address_ID = h.Address_ID
WHERE p.Show_On_Map = 1 AND a.Latitude IS NULL AND a.Address_ID IS NOT NULL

END
GO

DECLARE @PROCNAME VARCHAR(100) = 'crds_Get_Addressids_For_Map';

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
