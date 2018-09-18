USE [MinistryPlatform]
GO

-- =====================================
-- Author:		Jon Horner
-- Create Date:	5/25/17
-- Description:	Add Reference name field
--		to the group leader form.
-- =====================================

DECLARE @referenceNameFieldId INT = 1524;
DECLARE @groupLeaderForm INT = 29;
DECLARE @fieldOrder INT = 15; --After the reference contact id
DECLARE @fieldType INT = 1; --Text box type

IF NOT EXISTS (SELECT 1 FROM Form_Fields WHERE Form_Field_ID = @referenceNameFieldId)
BEGIN
	SET IDENTITY_INSERT Form_Fields ON

	INSERT INTO Form_Fields
	(
		Form_Field_ID
		, Field_Order
		, Field_Label
		, Field_Type_ID
		, [Required]
		, Form_ID
		, Domain_ID
		, Placement_Required
	)
	VALUES
	(
		@referenceNameFieldId
		, @fieldOrder
		, N'Staff Reference Name'
		, @fieldType
		, 1
		, @groupLeaderForm
		, 1
		, 0
	);

	SET IDENTITY_INSERT Form_Fields OFF
END