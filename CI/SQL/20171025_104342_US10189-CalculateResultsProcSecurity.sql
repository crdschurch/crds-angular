USE MinistryPlatform;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Roles] 
			   WHERE ROLE_NAME = 'SRFP - CRDS')
BEGIN
	INSERT INTO DP_ROLES
(Role_Name    ,Description                                   ,Domain_ID,Mass_Email_Quota,_AdminRole) VALUES
('SRFP - CRDS','Role to hold permissions for srfp assessment',1        ,0               ,0     );
END


IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_user_roles]
			   WHERE USER_ID = (SELECT USER_ID FROM dp_users WHERE user_name = 'crds_srfp')
			   AND ROLE_ID = (SELECT ROLE_ID FROM dp_roles WHERE ROLE_NAME = 'SRFP - CRDS'))
BEGIN
	INSERT INTO dp_user_roles
	(User_ID                                                     , Role_ID                                                      , DOMAIN_ID) VALUES
	((SELECT USER_ID from dp_users where user_name = 'crds_srfp'), (select role_id from dp_roles where role_name = 'SRFP - CRDS'), 1        );
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_API_Procedures] where Procedure_Name = 'api_crds_Calculate_Current_SRFP_Results')
BEGIN
		INSERT INTO dp_API_Procedures (Procedure_Name,Description) VALUES ('api_crds_Calculate_Current_SRFP_Results', 'Gets latest SRFP scores for user');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_API_Procedures] 
			   WHERE ROLE_ID = (SELECT ROLE_ID from dp_roles where role_name = 'SRFP - CRDS') 
			   AND API_PROCEDURE_ID = (SELECT API_PROCEDURE_ID from dp_API_Procedures where procedure_name = 'api_crds_Calculate_Current_SRFP_Results'))
BEGIN
	INSERT INTO dp_Role_API_Procedures 
	(Role_ID                                                       ,API_Procedure_ID                                                                                                 ,Domain_ID) VALUES
	((SELECT ROLE_ID from dp_roles where role_name = 'SRFP - CRDS'),(SELECT API_PROCEDURE_ID from dp_API_Procedures where procedure_name = 'api_crds_Calculate_Current_SRFP_Results'),1        )
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[forms] WHERE form_title = 'srfpassessment')
BEGIN
	INSERT INTO forms 
	(Form_Title      ,Instructions,Standalone_Form_Use_Only,Get_Contact_Info,Get_Address_Info,Domain_ID,Form_GUID                             ,End_Date,Complete_Message,Primary_Contact,Notify) VALUES
	('srfpassessment',null        ,null                    ,0           ,0           ,1        ,'F8A21573-0B11-4154-8639-4F01F716397D',null    ,null            ,null           ,0 )
GO
END