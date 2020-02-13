using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using AutoMapper;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using Common.Logging;
using RestSharp;

namespace crds_angular.Services
{
    public class PersonService : MinistryPlatformBaseService, IPersonService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(LoginService));
        private readonly MPServices.IContactRepository _contactRepository;
        private readonly IObjectAttributeService _objectAttributeService;
        private readonly IApiUserRepository _apiUserService;
        private readonly MPServices.IParticipantRepository _participantService;
        private readonly MPServices.IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationService;
        private readonly IAddressService _addressService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly string _identityServiceUrl;
        protected virtual HttpClient client { get { return _client; } }
        private static readonly HttpClient _client = new HttpClient();

        public PersonService(MPServices.IContactRepository contactService,
            IObjectAttributeService objectAttributeService,
            IApiUserRepository apiUserService,
            MPServices.IParticipantRepository participantService,
            MPServices.IUserRepository userService,
            IAuthenticationRepository authenticationService,
            IAddressService addressService,
            IAnalyticsService analyticsService,
            IConfigurationWrapper configurationWrapper)
        {
            _contactRepository = contactService;
            _objectAttributeService = objectAttributeService;
            _apiUserService = apiUserService;
            _participantService = participantService;
            _userRepository = userService;
            _authenticationService = authenticationService;
            _addressService = addressService;
            _analyticsService = analyticsService;
            _configurationWrapper = configurationWrapper;
            _identityServiceUrl = _configurationWrapper.GetEnvironmentVarAsString("IDENTITY_SERVICE_URL");
        }

        public void SetProfile(Person person, string userAccessToken)
        {
            var contactDictionary = getDictionary(person.GetContact());
            var householdDictionary = getDictionary(person.GetHousehold());
            var addressDictionary = getDictionary(person.GetAddress());
            addressDictionary.Add("State/Region", addressDictionary["State"]);


            // Some front-end consumers require an Address (e.g., /profile/personal), and
            // some do not (e.g., /undivided/facilitator).  Don't attempt to create/update
            // an Address record if we have no data.
            if (addressDictionary.Values.All(i => i == null))
            {
                addressDictionary = null;
            }
            else
            {
                //add the lat/long to the address 
                var address = new AddressDTO(addressDictionary["Address_Line_1"].ToString(), "", addressDictionary["City"].ToString(), addressDictionary["State"].ToString(), addressDictionary["Postal_Code"].ToString(), null, null);
                var coordinates = _addressService.GetGeoLocationCascading(address);
                addressDictionary.Add("Latitude", coordinates.Latitude);
                addressDictionary.Add("Longitude", coordinates.Longitude);
            }

            try
            {
                // update the user values if the email and/or password has changed
                UpdateUsernameOrPasswordIfNeeded(person, userAccessToken);

                _contactRepository.UpdateContact(person.ContactId, contactDictionary, householdDictionary, addressDictionary);
                var configuration = MpObjectAttributeConfigurationFactory.Contact();
                _objectAttributeService.SaveObjectAttributes(person.ContactId, person.AttributeTypes, person.SingleAttributes, configuration);

                var participant = _participantService.GetParticipant(person.ContactId);
                if (participant.AttendanceStart != person.AttendanceStartDate)
                {
                    participant.AttendanceStart = person.AttendanceStartDate;
                    _participantService.UpdateParticipant(participant);
                }

                // TODO: It appears we are updating the contact records email address above if the email address is changed
                // TODO: If the password is invalid we would not run the update on user, and therefore create a data integrity problem
                // TODO: See About moving the check for new password above or moving the update for user / person into an atomic operation
                // TODO: SEE IF THESE TODO's ARE RELEVANT 

                CaptureProfileAnalytics(person);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not complete updates : {e.Message}");
            }

        }

        public void CaptureProfileAnalytics(Person person)
        {
            var dateOfBirth = (String.IsNullOrWhiteSpace(person.DateOfBirth)) ? null : Convert.ToDateTime(person.DateOfBirth).ToUniversalTime().ToString("o");
            var props = new EventProperties
            {
                { "FirstName", person.NickName },
                { "LastName", person.LastName },
                { "Email", person.EmailAddress },
                { "Country", person.ForeignCountry },
                { "Zip", person.PostalCode },
                { "State", person.State },
                { "City", person.City },
                { "Employer", person.EmployerName },
                { "FirstAttendanceDate", person.AnniversaryDate },
                { "Congregation", person.CongregationId },
                { "DateOfBirth", dateOfBirth },
                { "Age", person.Age },
                { "Gender", person.GenderId },
                { "MaritalStatus", person.MaritalStatusId }
            };
            _analyticsService.IdentifyLoggedInUser(person.ContactId.ToString(), props);
        }

        public Person GetPerson(int contactId)
        {
            var contact = _contactRepository.GetContactById(contactId);
            var person = Mapper.Map<Person>(contact);

            var family = _contactRepository.GetHouseholdFamilyMembers(person.HouseholdId);
            person.HouseholdMembers = family;

            // TODO: Should this move to _contactService or should update move it's call out to this service?
            var apiUser = _apiUserService.GetDefaultApiClientToken();
            var configuration = MpObjectAttributeConfigurationFactory.Contact();
            var attributesTypes = _objectAttributeService.GetObjectAttributes(contactId, configuration);
            person.AttributeTypes = attributesTypes.MultiSelect;
            person.SingleAttributes = attributesTypes.SingleSelect;

            return person;
        }

        public List<MpRoleDto> GetLoggedInUserRoles(string token)
        {
            return GetMyRecordsRepository.GetMyRoles(token);
        }

        public Person GetLoggedInUserProfile(String token)
        {
            var contact = _contactRepository.GetMyProfile(token);
            var person = Mapper.Map<Person>(contact);

            var family = _contactRepository.GetHouseholdFamilyMembers(person.HouseholdId);
            person.HouseholdMembers = family;

            return person;
        }

        //Should not be called once cut over to Okta...
        protected virtual bool UpdateUsernameOrPasswordIfNeeded(Person person, string userAccessToken)
        {
            if (!(String.IsNullOrEmpty(person.NewPassword)) || (person.EmailAddress != person.OldEmail && person.OldEmail != null))
            {
                var authData = _authenticationService.AuthenticateUser(person.OldEmail, person.OldPassword);

                if (authData == null)
                {
                    throw new Exception("Could not authenticate user");
                }
                else
                {
                    var userUpdateValues = new Dictionary<string, object>();
                    userUpdateValues["User_ID"] = _userRepository.GetUserIdByUsername(person.OldEmail);
                    if (!string.IsNullOrEmpty(person.NewPassword))
                        userUpdateValues["Password"] = person.NewPassword;
                    userUpdateValues["Display_Name"] = $"{person.LastName}, {person.NickName}";
                    _userRepository.UpdateUser(userUpdateValues);

                    UpdateOktaEmailAddressIfNeeded(person, userAccessToken);
                    UpdateOktaPasswordIfNeeded(person, userAccessToken);
                }
            }
            return true;
        }

        private HttpResponseMessage PutToIdentityService(string apiEndpoint, string userAccessToken, JObject payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, _identityServiceUrl + apiEndpoint);
            request.Headers.Add("Authorization", userAccessToken);
            request.Headers.Add("Accept", "application/json");
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = client.SendAsync(request).Result;
            return response;
        }

        private Boolean UpdateOktaEmailAddressIfNeeded(Person person, string userAccessToken)
        {
            if (person.EmailAddress != person.OldEmail && person.OldEmail != null)
            {
                JObject payload = new JObject();
                payload.Add("newEmail", person.EmailAddress);
                var response = PutToIdentityService("/api/identities/" + person.OldEmail + "/email", userAccessToken, payload);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Could not update Okta email address for user {person.EmailAddress}");

                return true;
            }
            return false;
        }

        private Boolean UpdateOktaPasswordIfNeeded(Person person, string userAccessToken)
        {
            if (person.OldPassword != person.NewPassword && !string.IsNullOrEmpty(person.NewPassword))
            {
                OktaMigrationUser oktaMigrationUser = new OktaMigrationUser
                {
                    firstName = person.FirstName,
                    lastName = person.LastName,
                    email = person.EmailAddress,
                    login = person.EmailAddress,
                    password = person.NewPassword,
                    mpContactId = person.ContactId.ToString()
                };

                CreateOrUpdateOktaAccount(oktaMigrationUser);
                NotifyIdentityofPasswordUpdateIfNeeded(person, userAccessToken);
                return true;
            }
            return false;
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

        private Boolean NotifyIdentityofPasswordUpdateIfNeeded(Person person, string userAccessToken)
        {
            if (person.OldPassword != person.NewPassword && !string.IsNullOrEmpty(person.NewPassword))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _identityServiceUrl + $"/api/identities/{person.EmailAddress}/passwordupdated");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", userAccessToken);
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                    return true;

            }
            return false;
        }
    }
}