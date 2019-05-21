USE [MinistryPlatform]
GO

DECLARE @printerMapID as int
SET @printerMapID = (SELECT Printer_Map_ID FROM cr_Printer_Maps WHERE Printer_ID = 0
								AND Printer_Name = '(t+auto) Printer'
								AND Computer_ID = 0
								AND Computer_Name = '(t+auto) Computer'
		    )
IF @printerMapID IS NULL
BEGIN
	INSERT INTO cr_Printer_Maps (Printer_ID,Printer_Name       ,Computer_ID,Computer_Name      ,Domain_ID)
	VALUES                      (0         ,'(t+auto) Printer' ,0          ,'(t+auto) Computer',1)
END
