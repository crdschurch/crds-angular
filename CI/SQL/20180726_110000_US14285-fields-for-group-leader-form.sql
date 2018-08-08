USE MinistryPlatform
GO

DECLARE @FORMID INTEGER = 29

SET IDENTITY_INSERT dbo.form_fields ON
IF NOT EXISTS(SELECT * FROM form_fields WHERE Form_ID = @FORMID and Form_Field_ID = 2000)
BEGIN
	INSERT INTO form_fields(Form_Field_ID, Field_Order, Field_Label, Field_Type_ID, Required, Form_ID, Domain_ID, Placement_Required)
	     VALUES(2000, 60, 'Open Response 3', 2, 1, @FORMID, 1, 0)
END

IF NOT EXISTS(SELECT * FROM form_fields WHERE Form_ID = @FORMID and Form_Field_ID = 2001)
BEGIN
	INSERT INTO form_fields(Form_Field_ID, Field_Order, Field_Label, Field_Type_ID, Required, Form_ID, Domain_ID, Placement_Required)
	     VALUES(2001, 70, 'Open Response 4', 2, 1, @FORMID, 1, 0)
END
SET IDENTITY_INSERT dbo.form_fields OFF

UPDATE form_fields SET Field_Label = 'Open Response 1' WHERE Form_Field_ID = 1522
UPDATE form_fields SET Field_Label = 'Open Response 2' WHERE Form_Field_ID = 1523

GO
