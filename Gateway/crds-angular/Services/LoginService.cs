using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Newtonsoft.Json;
using RestSharp;

//using WebMatrix.WebData;

namespace crds_angular.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(LoginService));

        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IContactRepository _contactService;
        private readonly IEmailCommunication _emailCommunication;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly string _identityServiceUrl;
        private readonly IContactRepository _contactRepository;
        protected virtual HttpClient client { get { return _client; } }
        private static readonly HttpClient _client = new HttpClient();

        public LoginService(IAuthenticationRepository authenticationRepository, IConfigurationWrapper configurationWrapper, IContactRepository contactService, IEmailCommunication emailCommunication, IUserRepository userRepository, IContactRepository contactRepository)
        {
            _configurationWrapper = configurationWrapper;
            _contactService = contactService;
            _emailCommunication = emailCommunication;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
            _identityServiceUrl = _configurationWrapper.GetEnvironmentVarAsString("IDENTITY_SERVICE_URL");
            _contactRepository = contactRepository;
        }

        public bool PasswordResetRequest(string username, bool isMobile)
        {
            int user_ID = 0;
            int contact_Id = 0;

            // validate the email on the server side to avoid erroneous or malicious requests
            try
            {
                user_ID = _userRepository.GetUserIdByUsername(username);
                contact_Id = _userRepository.GetContactIdByUserId(user_ID);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Could not find email {0} for password reset", JsonConvert.SerializeObject(username, Formatting.Indented)), ex);
                return false;
            }

            // create a token -- see http://stackoverflow.com/questions/664673/how-to-implement-password-resets
            var resetArray = Encoding.UTF8.GetBytes(Guid.NewGuid() + username + System.DateTime.Now);
            RNGCryptoServiceProvider prov = new RNGCryptoServiceProvider();
            prov.GetBytes(resetArray);
            var resetToken = Encoding.UTF8.GetString(resetArray);
            string cleanToken = Regex.Replace(resetToken, "[^A-Za-z0-9]", "");

            Dictionary<string, object> userUpdateValues = new Dictionary<string, object>();
            userUpdateValues["User_ID"] = user_ID;
            userUpdateValues["PasswordResetToken"] = cleanToken; // swap out for real implementation
            _userRepository.UpdateUser(userUpdateValues);

            string baseURL = _configurationWrapper.GetConfigValue("BaseURL");
            string resetLink = $"https://{baseURL}/reset-password?token={cleanToken}";
            if (isMobile)
            {
                resetLink += $"&email={username}";
            }

            // add the email here...
            var emailCommunication = new EmailCommunicationDTO
            {
                FromContactId = 7, // church admin contact id
                FromUserId = 5, // church admin user id
                ToContactId = contact_Id,
                TemplateId = isMobile?2034:13356,
                MergeData = new Dictionary<string, object>
                    {   
                        { "resetlink", resetLink },
                        { "baseURL", baseURL},
                        { "token",  cleanToken },
                        { "email", username }
                    }
            };

            try
            {
                _emailCommunication.SendEmail(emailCommunication);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Could not send email {0} for password reset", JsonConvert.SerializeObject(username, Formatting.Indented)), ex);
                return false;
            }
        }

        public bool ResetPassword(string password, string token)
        {
            var user = _userRepository.GetUserByResetToken(token);

            Dictionary<string, object> userUpdateValues = new Dictionary<string, object>();
            userUpdateValues["User_ID"] = user.UserRecordId;
            userUpdateValues["PasswordResetToken"] = null;
            userUpdateValues["Password"] = password;
            _userRepository.UpdateUser(userUpdateValues);
            var contact = _contactRepository.GetContactByUserRecordId(user.UserRecordId, _userRepository.HelperApiLogin());
            OktaMigrationUser oktaMigrationUser = new OktaMigrationUser
            {
                firstName = contact.First_Name,
                lastName = contact.Last_Name,
                email = contact.Email_Address,
                login = user.UserId,
                password = password,
                mpContactId = contact.Contact_ID.ToString()
            };
            CreateOrUpdateOktaAccount(oktaMigrationUser);
            NotifyIdentityofPasswordUpdate(user.UserEmail, _userRepository.HelperApiLogin());
         
            return true;
        }

        public bool ClearResetToken(string username)
        {
            int user_ID = _userRepository.GetUserIdByUsername(username);

            Dictionary<string, object> userUpdateValues = new Dictionary<string, object>();
            userUpdateValues["User_ID"] = user_ID;
            userUpdateValues["ResetToken"] = null; // swap out for real implementation
            _userRepository.UpdateUser(userUpdateValues);

            return true;
        }

        public bool ClearResetToken(int userId) //KD use if we already have the id to save time
        {
            Dictionary<string, object> userUpdateValues = new Dictionary<string, object>()
            {
                {"User_ID",userId},
                {"ResetToken",null }
            };

            _userRepository.UpdateUserRest(userUpdateValues);

            return true;
        }

        public bool VerifyResetToken(string token)
        {
            var user = _userRepository.GetUserByResetToken(token);

            if (user != null)
            {
                return true;
            }

            return false;
        }

        public bool IsValidPassword(string token, string password)
        {
            AuthToken authData = null;

            var user = _userRepository.GetByAuthenticationToken(token);
            if (user != null)
            {
                authData = _authenticationRepository.AuthenticateUser(user.UserId, password);
            }

            return authData != null ? true : false;
        }

        public void CreateOrUpdateOktaAccount(OktaMigrationUser oktaMigrationUser)
        {
            string migrationBaseUrl = Environment.GetEnvironmentVariable("OKTA_MIGRATION_BASE_URL");
            if (String.IsNullOrEmpty(migrationBaseUrl))
            {
                _logger.Error("OKTA_MIGRATION_BASE_URL environment variable is null or an empty string");
                return;
            }

            string azureFunctionApiCode = Environment.GetEnvironmentVariable("OKTA_MIGRATION_AZURE_API_KEY");
            if (String.IsNullOrEmpty(azureFunctionApiCode))
            {
                _logger.Error("OKTA_MIGRATION_AZURE_API_KEY environment variable is null or an empty string");
                return;
            }

            var client = new RestClient(migrationBaseUrl);
            var request = new RestRequest("api/migrate", Method.POST);

            request.AddQueryParameter("code", azureFunctionApiCode);

            request.AddHeader("FirstName", oktaMigrationUser.firstName);
            request.AddHeader("LastName", oktaMigrationUser.lastName);
            request.AddHeader("Email", oktaMigrationUser.email);
            request.AddHeader("Login", oktaMigrationUser.login);
            request.AddHeader("Password", oktaMigrationUser.password);
            request.AddHeader("MpContactID", oktaMigrationUser.mpContactId);

            client.ExecuteAsync(request, (response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.Info("Okta Migration Request Sent for MP Contact: " + oktaMigrationUser.mpContactId);
                }
                else
                {
                    _logger.Error("Okta Migration Request Failed for MP Contact: " + oktaMigrationUser.mpContactId);
                    _logger.Error("Response Code: " + response.StatusCode.ToString());
                }
            });
        }

        private Boolean NotifyIdentityofPasswordUpdate(string emailAddress, string userAccessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _identityServiceUrl + $"/api/identities/{emailAddress}/passwordupdated");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", userAccessToken);
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
            catch
            {
                _logger.Info($"Could not notify Identity Service of Password Update for user {emailAddress}.");
                return false;
            }
            
        }

        private Boolean ForceOktaPasswordReset(string emailAddress, string newPassword, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _identityServiceUrl + $"/api/identities/{emailAddress}/password");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", accessToken);
            var body = new
            {
                NewPassword = newPassword
            };
            var json = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                    return true;
                return false;
            }
            catch
            {
                _logger.Info($"Could not update password for user {emailAddress} in Okta.");
                return false;
            }
        }

    }
}