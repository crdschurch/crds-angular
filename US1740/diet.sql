USE MinistryPlatform;

SELECT STUFF((
    SELECT '|'+attribute_name
    FROM dbo.Contact_Attributes AS ca
    INNER JOIN MinistryPlatform.dbo.Attributes AS a ON ca.attribute_id=a.attribute_id AND a.Attribute_Type_ID=65
    WHERE ca.contact_id=768379
    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '') AS Dietary_Restrictions;

SELECT STUFF((
    SELECT '|'+attribute_name
    FROM dbo.vw_crds_Contact_Single_Select_Attributes AS ca
    WHERE ca.contact_id=768379 and ca.Attribute_Type_ID = 65
    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '') AS Dietary_Restrictions;

