USE [MinistryPlatform]
GO

-- Add the user's email address to the body of the email that is sent for group signups

DECLARE @Template_ID int = 82;

UPDATE dp_Communications
SET Body = '[GP_Display_Name] has signed up online for [Group_Name]<br /><br />Please contact the user at [GP_Email_Address]'
WHERE Template = 1 AND Communication_ID = @Template_ID
;
