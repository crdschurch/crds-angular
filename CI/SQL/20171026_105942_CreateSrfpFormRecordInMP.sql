USE [MinistryPlatform]

DECLARE @srfp_form_id int = 57;

SET IDENTITY_INSERT forms ON

IF NOT EXISTS (SELECT 1 FROM [dbo].[forms] WHERE FORM_ID = @srfp_form_id)
BEGIN
	INSERT INTO [dbo].[forms]
	(Form_ID           ,Form_Title      ,Get_Contact_Info,Get_Address_Info,Domain_ID,Form_GUID                             ,Notify) VALUES
	(@srfp_form_id     ,'srfpassessment',0               ,0               ,1        ,'F8A21573-0B11-4154-8639-4F01F716397D',0     );
END

SET IDENTITY_INSERT forms OFF
