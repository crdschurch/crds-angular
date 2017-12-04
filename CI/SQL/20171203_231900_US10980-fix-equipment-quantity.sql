USE [MinistryPlatform]
GO

-- The Create/Edit Event tool writes to Quantity_Requested.  The Facilities App reports Quantity.
-- This trigger copies changes made to Quantity_Requested to Quantity so the Facilities App can
-- show the quantity of equipment that has been requested.

CREATE TRIGGER [dbo].[crds_tr_event_equipment_quantity] 
   ON [dbo].[Event_Equipment]
   AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF (UPDATE(Quantity_Requested))
	BEGIN
		UPDATE Event_Equipment
		SET Quantity = Quantity_Requested
		WHERE Event_Equipment_ID IN (SELECT Event_Equipment_ID FROM INSERTED)
	END
END
GO


-- one-time update for existing data before the trigger was added
UPDATE Event_Equipment
SET Quantity = Quantity_Requested
GO
