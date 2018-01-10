using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.MP;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using ILookupRepository = MinistryPlatform.Translation.Repositories.Interfaces.ILookupRepository;

namespace crds_angular.Services
{
    public class AccountService : MinistryPlatformBaseService, IAccountService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (AccountService));

        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly ICommunicationRepository _communicationService;
        private readonly IAuthenticationRepository _authenticationService;
        private readonly ISubscriptionsService _subscriptionsService;
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly ILookupRepository _lookupService;
        private readonly IApiUserRepository _apiUserService;
        private readonly IParticipantRepository _participantService;
        private readonly IContactRepository _contactRepository;

        public AccountService(IConfigurationWrapper configurationWrapper, 
            ICommunicationRepository communicationService, 
            IAuthenticationRepository authenticationService, 
            ISubscriptionsService subscriptionService, 
            IMinistryPlatformService ministryPlatformService, 
            ILookupRepository lookupService,
            IApiUserRepository apiUserService,
            IParticipantRepository participantService,
            IContactRepository contactRepository)
        {
            _configurationWrapper = configurationWrapper;
            _communicationService = communicationService;
            _authenticationService = authenticationService;
            _subscriptionsService = subscriptionService;
            _ministryPlatformService = ministryPlatformService;
            _lookupService = lookupService;
            _apiUserService = apiUserService;
            _participantService = participantService;
            _contactRepository = contactRepository;
        }
        public bool ChangePassword(string token, string newPassword)
        {
            return _authenticationService.ChangePassword(token, newPassword);
        }

        //TODO: Put this logic in the Translation Layer?
        public bool SaveCommunicationPrefs(string token, AccountInfo accountInfo)
        {
            var contactId = _ministryPlatformService.GetContactInfo(token).ContactId;
            var contact = _ministryPlatformService.GetRecordDict(_configurationWrapper.GetConfigIntValue("MyContact"), contactId, token);
            try
            {                
                var emailsmsDict = getDictionary(new EmailSMSOptOut
                    {
                        Contact_ID = contactId,
                        Bulk_Email_Opt_Out = accountInfo.EmailNotifications,
                        Bulk_SMS_Opt_Out = accountInfo.TextNotifications
                    });
                var mailDict = getDictionary(new MailOptOut 
                {
                    Household_ID = (int)contact["Household_ID"],
                    Bulk_Mail_Opt_Out = accountInfo.PaperlessStatements 
                });
                _communicationService.SetEmailSMSPreferences(token, emailsmsDict);
                _communicationService.SetMailPreferences(token, mailDict);
                return true;
            }
            catch (Exception e)
            {
                _logger.Warn(string.Format("Could not set communication preferences for contact {0}", contactId), e);
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
        }

        public AccountInfo getAccountInfo(string token)
        {
            var contactId = _ministryPlatformService.GetContactInfo(token).ContactId;
            var contact = _communicationService.GetPreferences(token, contactId);
            var accountInfo = new AccountInfo
            {
                EmailNotifications = contact.Bulk_Email_Opt_Out,
                TextNotifications = contact.Bulk_SMS_Opt_Out,
                PaperlessStatements = contact.Bulk_Mail_Opt_Out
            };
            return accountInfo;


       }

        // TODO  Create a PageIdManager that wraps ConfigurationManager and does the convert for us.

        private int CreateHouseholdRecord(User newUserData, string token, int? householdSourceId = null)
        {
            var householdDictionary = new Dictionary<string, object>();
            householdDictionary["Household_Name"] = newUserData.lastName;
            householdDictionary["Congregation_ID"] = _configurationWrapper.GetConfigIntValue("Congregation_Default_ID");
            householdDictionary["Household_Source_ID"] = householdSourceId ?? _configurationWrapper.GetConfigIntValue("Household_Default_Source_ID");

            var recordId = _ministryPlatformService.CreateRecord(_configurationWrapper.GetConfigIntValue("Households"), householdDictionary, token);

            return recordId;
        }

        private int CreateContactRecord(User newUserData, string token, int householdRecordId)
        {
            var contactDictionary = new Dictionary<string, object>();
            contactDictionary["First_Name"] = newUserData.firstName;
            contactDictionary["Last_Name"] = newUserData.lastName;
            contactDictionary["Email_Address"] = newUserData.email;
            contactDictionary["Company"] = false; // default
            contactDictionary["Display_Name"] = newUserData.lastName + ", " + newUserData.firstName;
            contactDictionary["Nickname"] = newUserData.firstName;
            contactDictionary["Household_Position_ID"] = _configurationWrapper.GetConfigIntValue("Household_Position_Default_ID");
            contactDictionary["Household_ID"] = householdRecordId;

            var recordId = _ministryPlatformService.CreateRecord(_configurationWrapper.GetConfigIntValue("Contacts"), contactDictionary, token);

            return recordId;
        }

        private int CreateUserRecord(User newUserData, string token, int contactRecordId)
        {
            var userDictionary = new Dictionary<string, object>();
            userDictionary["First_Name"] = newUserData.firstName;
            userDictionary["Last_Name"] = newUserData.lastName;
            userDictionary["User_Email"] = newUserData.email;
            userDictionary["Password"] = newUserData.password;
            userDictionary["Company"] = false; // default
            userDictionary["Display_Name"] = userDictionary["First_Name"];
            userDictionary["Domain_ID"] = 1;
            userDictionary["User_Name"] = userDictionary["User_Email"];
            userDictionary["Contact_ID"] = contactRecordId;

            var recordId = _ministryPlatformService.CreateRecord(_configurationWrapper.GetConfigIntValue("Users"), userDictionary, token, true);

            return recordId;
        }

        private void CreateUserRoleSubRecord(string token, int userRecordId)
        {
            var userRoleDictionary = new Dictionary<string, object>();
            userRoleDictionary["Role_ID"] = _configurationWrapper.GetConfigIntValue("Role_Default_ID");
            _ministryPlatformService.CreateSubRecord(_configurationWrapper.GetConfigIntValue("Users_Roles"),userRecordId, userRoleDictionary, token);
        }

        /*
         * Not needed as long as the triggers are in place on Ministry Platform
         */
        // ReSharper disable once UnusedMember.Local
        private void UpdateContactRecord(int contactRecordId, int userRecordId, int participantRecordId, string token)
        {
            var contactDictionary = new Dictionary<string, object>();
            contactDictionary["Contact_ID"] = contactRecordId;
            contactDictionary["User_account"] = userRecordId;
            contactDictionary["Participant_Record"] = participantRecordId;

            _ministryPlatformService.UpdateRecord(_configurationWrapper.GetConfigIntValue("Contacts"), contactDictionary, token);
        }

        public User RegisterPerson(User newUserData, int? householdSourceId = null)
        {
            var token = _apiUserService.GetToken();
            var exists = _lookupService.EmailSearch(newUserData.email, token);
            if (exists != null && exists.Any())
            {
                throw (new DuplicateUserException(newUserData.email));
            }
            var householdRecordId = CreateHouseholdRecord(newUserData, token, householdSourceId);
            var contactRecordId = CreateContactRecord(newUserData, token, householdRecordId);
            var userRecordId = CreateUserRecord(newUserData, token, contactRecordId);
            CreateUserRoleSubRecord(token, userRecordId);
            _participantService.CreateParticipantRecord(contactRecordId);
            CreateNewUserSubscriptions(contactRecordId, token);

            //TODO Contingent on cascading delete via contact
            //TODO Consider encrypting the password on the user model
            return newUserData;
        }

        public int RegisterPersonWithoutUserAccount(User newUserData)
        {
            var token = _apiUserService.GetToken();
            var householdRecordId = CreateHouseholdRecord(newUserData, token, null);
            var contactRecordId = CreateContactRecord(newUserData, token, householdRecordId);
           
            _participantService.CreateParticipantRecord(contactRecordId);

            return contactRecordId;
        }

        private void CreateNewUserSubscriptions(int contactRecordId, string token)
        {
            var newSubscription = new Dictionary<string, object>();
            newSubscription["Publication_ID"] = _configurationWrapper.GetConfigValue("KidsClubPublication");
            newSubscription["Unsubscribed"] = false;
            _subscriptionsService.SetSubscriptions(newSubscription, contactRecordId, token);
            newSubscription["Publication_ID"] = _configurationWrapper.GetConfigValue("CrossroadsPublication");
            newSubscription["Unsubscribed"] = false;
            _subscriptionsService.SetSubscriptions(newSubscription, contactRecordId, token);
        }

    }

    
}