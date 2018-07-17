USE [MinistryPlatform]
GO

DECLARE @printerMapID as int
SET @printerMapID = (SELECT Printer_Map_ID FROM cr_Printer_Maps WHERE Printer_ID = 0
								AND Printer_Name = '(t+auto) Printer'
								AND Computer_ID = 0
								AND Computer_Name = '(t+auto) Computer'
		    )

DELETE FROM cr_Kiosk_Configs WHERE Printer_Map_ID = @printerMapID
DELETE FROM cr_Printer_Maps WHERE Printer_Map_ID = @printerMapID
