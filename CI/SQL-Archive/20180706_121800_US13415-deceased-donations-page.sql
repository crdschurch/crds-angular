USE [MinistryPlatform]
GO

DECLARE @Page_ID INT;
SELECT @Page_ID = Page_ID FROM dbo.dp_Pages WHERE Display_Name = 'Deceased Donations';

IF @Page_ID IS NULL
BEGIN
      INSERT INTO dbo.dp_Pages
            ([Display_Name]
            ,[Singular_Name]
            ,[Description]
            ,[View_Order]
            ,[Table_Name]
            ,[Primary_Key]
            ,[Display_Search]
            ,[Default_Field_List]
            ,[Selected_Record_Expression]
            ,[Filter_Clause]
            ,[Start_Date_Field]
            ,[End_Date_Field]
            ,[Contact_ID_Field]
            ,[Default_View]
            ,[Pick_List_View]
            ,[Image_Name]
            ,[Direct_Delete_Only]
            ,[System_Name]
            ,[Date_Pivot_Field]
            ,[Custom_Form_Name]
            ,[Display_Copy])
      VALUES (
            'Deceased Donations',
            'Deceased Donation',
            'List of donations made after the donor was identified as deceased',
            28,
            'vw_crds_Deceased_Donations',
            NULL,
            0,
            'Donor_ID,Display_Name,Donor_Notes,[Co-Giver_Name],Donation_ID,Donation_Date,Donation_Amount,Distribution_Amount,Payment_Type,Is_Recurring_Gift',
            'Donation_ID',
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            1
      );
      SET @Page_ID = SCOPE_IDENTITY();

      INSERT INTO dp_Page_Section_Pages (Page_ID, Page_Section_ID) VALUES (@Page_ID, 9);
END

DECLARE @Roles TABLE (
      Role_ID INT NOT NULL
);

INSERT INTO @Roles (Role_ID) VALUES
      (2),		-- Administrators
      (5),		-- Stewardship Oversight
      (6),		-- Stewardship Manager
      (7),		-- Stewardship Donation Processor
      (104),	-- Finance Donation Entry - CRDS
      (105),	-- Finance Donation Mgr - CRDS
      (106),	-- Finance Management - CRDS
      (107)		-- System Administrator - CRDS
;

INSERT INTO [dbo].[dp_Role_Pages]
      ([Role_ID]
      ,[Page_ID]
      ,[Access_Level]
      ,[Scope_All]
      ,[Approver]
      ,[File_Attacher]
      ,[Data_Importer]
      ,[Data_Exporter]
      ,[Secure_Records]
      ,[Allow_Comments]
      ,[Quick_Add])
SELECT
      [Role_ID], @Page_ID, 0, 0, 0, 0, 0, 1, 0, 0, 0
FROM
      @Roles r
WHERE
      NOT EXISTS (SELECT 1 FROM dp_Role_Pages WHERE Page_ID = @Page_ID AND Role_ID = r.Role_ID)
;

GO
