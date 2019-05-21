USE MinistryPlatform

DECLARE @AttributeTypeID INT = 1005
DECLARE @AttributeID_Gen1 INT = 10001
DECLARE @AttributeID_Gen2 INT = 10002
DECLARE @AttributeID_Gen3 INT = 10003
DECLARE @AttributeID_Gen4 INT = 10004
DECLARE @AttributeID_Gen5 INT = 10005
DECLARE @AttributeID_Gen6 INT = 10006
DECLARE @AttributeID_Gen7 INT = 10007
DECLARE @AttributeID_Gen8 INT = 10008
DECLARE @AttributeID_Gen9 INT = 10009

--create 'huddle' group type
SET IDENTITY_INSERT Group_Types ON 
IF NOT EXISTS(SELECT * FROM Group_Types WHERE Group_Type = 'Huddle')
BEGIN
  INSERT INTO Group_Types(Group_Type_ID, Group_Type, Description, Domain_ID, Default_Role, Show_On_Group_Finder, Show_On_Sign_Up_to_Serve, Show_On_MPMobile)
              Values (16, 'Huddle', 'Huddle',1,16,0,0,0)
END
SET IDENTITY_INSERT Group_Types OFF 


--create huddle attribute type
SET IDENTITY_INSERT Attribute_Types ON 
IF NOT EXISTS(SELECT * FROM Attribute_Types WHERE Attribute_Type = 'Huddle Generation')
BEGIN
  INSERT INTO Attribute_Types(Attribute_Type_ID, Attribute_Type, Description, Domain_ID, Prevent_Multiple_Selection)
              Values (@AttributeTypeID, 'Huddle Generation', 'Huddle Generation',1,1)
END
SET IDENTITY_INSERT Attribute_Types OFF 


--create attributes for huddle generations
SET IDENTITY_INSERT Attributes ON 
-- Gen 1
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen1)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen1, '1',@AttributeTypeID,1,0)
END
-- Gen 2
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen2)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen2, '2',@AttributeTypeID,1,0)
END
-- Gen 3
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen3)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen3, '3',@AttributeTypeID,1,0)
END
-- Gen 4
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen4)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen4, '4',@AttributeTypeID,1,0)
END
-- Gen 5
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen5)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen5, '5',@AttributeTypeID,1,0)
END
-- Gen 6
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen6)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen6, '6',@AttributeTypeID,1,0)
END
-- Gen 7
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen7)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen7, '7',@AttributeTypeID,1,0)
END
-- Gen 8
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen8)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen8, '8',@AttributeTypeID,1,0)
END
-- Gen 9
IF NOT EXISTS(SELECT * FROM Attributes WHERE Attribute_ID = @AttributeID_Gen9)
BEGIN
  INSERT INTO Attributes(Attribute_ID, Attribute_Name, Attribute_Type_ID, Domain_ID, Sort_Order)
              Values (@AttributeID_Gen9, '9',@AttributeTypeID,1,0)
END

SET IDENTITY_INSERT Attributes OFF 
GO
