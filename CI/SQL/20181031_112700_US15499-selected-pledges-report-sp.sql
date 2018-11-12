USE [MinistryPlatform]
GO

-- 10/31/2018
-- This is a replacement/enhancement for report_Pledges_Selected_Letter supplied by Think Ministry.
-- This version uses Contact_Relationships to find co-givers instead of Statement Type = Family,
-- and outputs different columns.
CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_Pledges_Selected_Letter]
	@DomainID varchar(40)
	,@UserID varchar(40)
	,@PageID Int
	,@SelectionID Int
AS
BEGIN

    SELECT
        PL.Pledge_ID 
        , PC.Campaign_Name 
        , Pl.Total_Pledge
        , Given.Total_Given 
        , Balance = CASE WHEN PL.Total_Pledge - ISNULL(Given.Total_Given,0) < 0 THEN 0 ELSE PL.Total_Pledge - ISNULL(Total_Given,0) END
        , Do.Donor_ID
        , Co.Congregation_Name AS [Site]
        , Pre.Prefix
        , C.Last_Name
        , First_Name = CASE WHEN C.Company = 1 THEN '' ELSE C.First_Name + ISNULL(' & ' + SP.First_Name,'') END
        , Nickname = CASE WHEN C.Company = 1 THEN '' ELSE C.Nickname + ISNULL(' & ' + SP.Nickname,'') END
        , Suf.Suffix
        , Full_Name = CASE WHEN C.Company = 1 THEN Display_Name WHEN SP.Nickname IS NOT NULL THEN C.Nickname + ISNULL(' & ' + SP.Nickname,'') + Space(1) + C.Last_Name ELSE ISNULL(Pre.Prefix + ' ', '') + C.First_Name + ' ' + C.Last_Name + ISNULL(' ' + Suf.Suffix,'') END
        , A.Address_Line_1
        , A.Address_Line_2
        , A.City
        , A.[State/Region]
        , A.Postal_Code
        , A.Foreign_Country 
        , C.Email_Address
        , First_Installment_Date 
        , PL._Last_Installment_Date 
        , CASE	WHEN SP.Contact_ID IS NULL THEN ISNULL(C.Nickname,C.Display_Name)
                ELSE ISNULL(C.Nickname,C.Display_Name) + ISNULL(' & ' + SP.Nickname,'') 
                END AS Combine_Nicknames
        , CASE	WHEN SP.Contact_ID IS NULL THEN CASE WHEN C.Company = 1 THEN Display_Name ELSE ISNULL(ISNULL(Pre.Prefix + ' ', '') + C.First_Name + ' ' + C.Last_Name + ISNULL(' ' + Suf.Suffix,''),C.Display_Name) END
                WHEN SP.Last_Name <> C.Last_Name THEN ISNULL(ISNULL(Pre.Prefix + ' ', '') + C.First_Name + ' ' + C.Last_Name + ISNULL(' ' + Suf.Suffix,''),C.Display_Name) + ISNULL(' & ' + ISNULL(SP.Prefix + ' ', '') + SP.First_Name + ' ' + SP.Last_Name,'') 
                WHEN C.Gender_ID = 1 AND SP.Prefix IS NOT NULL AND Pre.Prefix IS NOT NULL THEN ISNULL(Pre.Prefix + ' & ' + SP.Prefix + SPACE(1) + C.First_Name + Space(1) + C.Last_Name,C.Display_Name)
                WHEN SP.Gender_ID = 1 AND SP.Prefix IS NOT NULL AND Pre.Prefix IS NOT NULL THEN ISNULL(SP.Prefix + ' & ' + Pre.Prefix + SPACE(1) + SP.First_Name + Space(1) + C.Last_Name,C.Display_Name)
                ELSE ISNULL(C.First_Name + ISNULL(' & ' + SP.First_Name,'') + ' ' + C.Last_Name,C.Display_Name)
                END AS Combine_Full_Name
        , SP.First_Name AS Spouse_First
        , SP.Last_Name AS Spouse_Last
        , SP.Nickname AS Spouse_Nickname
        , SP.Email_Address AS Spouse_Email
        , SP.Contact_ID AS Spouse_Contact_ID
        , SP.Prefix AS Spouse_Prefix		
            
    FROM Pledges Pl
    INNER JOIN Pledge_Campaigns PC ON PC.Pledge_Campaign_ID = Pl.Pledge_Campaign_ID
    INNER JOIN Donors Do ON Do.Donor_ID = Pl.Donor_ID
    INNER JOIN dp_Domains Dom on Dom.Domain_ID = Do.Domain_ID
    INNER JOIN Contacts C ON C.Contact_ID = Do.Contact_ID
    LEFT OUTER JOIN Programs Prog ON Prog.Program_ID = PC.Program_ID
    LEFT OUTER JOIN Prefixes Pre ON Pre.Prefix_ID = C.Prefix_ID
    LEFT OUTER JOIN Suffixes Suf ON Suf.Suffix_ID = C.Suffix_ID
    LEFT OUTER JOIN Households H ON H.Household_ID = C.Household_ID
    LEFT OUTER JOIN Addresses A ON A.Address_ID = H.Address_ID
    LEFT OUTER JOIN Congregations Co on Co.Congregation_ID = H.Congregation_ID
    OUTER APPLY (
        SELECT TOP 1
            S.Contact_ID,
            S.Email_Address,
            S.Nickname,
            S.First_Name,
            S.Last_Name,
            Pre.Prefix,
            S.Prefix_ID,
            S.Gender_ID
        FROM
            Contact_Relationships cr
            INNER JOIN Contacts S ON s.Contact_ID = cr.Related_Contact_ID
            LEFT OUTER JOIN Prefixes Pre ON Pre.Prefix_ID = S.Prefix_ID
        WHERE
            cr.Contact_ID = c.Contact_ID
            AND cr.Relationship_ID = 42  -- co-giver with
            AND cr.End_Date IS NULL
    ) SP
    OUTER APPLY (
        SELECT
            SUM(Amount) AS Total_Given
        FROM
            Donation_Distributions DD
            INNER JOIN Donations D ON D.Donation_ID = DD.Donation_ID AND D.Batch_ID IS NOT NULL
        WHERE
            DD.Pledge_ID = PL.Pledge_ID
    ) AS Given

    WHERE Dom.Domain_GUID = @DomainID 
    AND PL.Pledge_ID IN (
        SELECT
            Record_ID
        FROM
            dp_Selected_Records SR
            INNER JOIN dp_Selections S ON S.Selection_ID = SR.Selection_ID
            INNER JOIN dbo.dp_Users U ON U.[User_ID] = S.[User_ID] AND U.User_GUID = @UserID
        WHERE
            S.Page_ID = @PageID
            AND ((S.Selection_ID = @SelectionID AND @SelectionID > 0) OR (S.Selection_Name = 'dp_DEFAULT' AND @SelectionID < 1 AND S.Sub_Page_ID IS NULL))
    )
END
GO
