Use MinistryPlatform 
GO

UPDATE dp_Tools 
SET Launch_Page = 'http://localhost:3000/mptools/requestchildcare'
WHERE Tool_ID = (SELECT Tool_ID FROM dp_Tools WHERE Tool_Name = 'Request Childcare Tool - Dev');