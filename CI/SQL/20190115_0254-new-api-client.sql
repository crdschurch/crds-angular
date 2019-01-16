USE MinistryPlatform
GO

IF NOT EXISTS(SELECT * FROM [dbo].[dp_API_Clients] WHERE [Client_ID] = 'CRDS.Service.SignCheckIn')

BEGIN
	-- get SECURITY ROLE
	DECLARE @role_id int;
	SET @role_id = 112 ;
	
	-- insert CONTACT
	DECLARE @tableContact table (Contact_ID int);
	INSERT INTO Contacts (Display_Name,Contact_Status_ID,Email_Address,Company,Domain_ID)
	OUTPUT INSERTED.Contact_ID INTO @tableContact
	VALUES ('Kids Club Checkin',1,'webteam+KidsClubCheckin@crossroads.net',0,1);

	DECLARE @contact_id int;
	SELECT @contact_id = Contact_ID from @tableContact;

	-- insert USER
	DECLARE @tableUser table (User_ID int);
	INSERT INTO dp_Users (User_Name,User_Email,Admin,Domain_ID,Contact_ID)
	OUTPUT INSERTED.User_ID INTO @tableUser
	VALUES ('webteam+KidsClubCheckin@crossroads.net','webteam+KidsClubCheckin@crossroads.net',0,1,@contact_id);

	DECLARE @user_id int;
	SELECT @user_id = User_ID from @tableUser;

	-- insert USER SECURITY ROLE
	INSERT INTO dp_User_Roles (User_ID,Role_ID,Domain_ID)
	VALUES (@user_id,@role_id,1);

	-- insert API CLIENT
	INSERT INTO dp_API_Clients (Display_Name,Client_ID,Access_Token_Lifetime,Client_User_ID,Domain_ID)
	VALUES ('CRDS Kids Club CheckIn Microservice','CRDS.Service.SignCheckIn',30,@user_id,1);
END
