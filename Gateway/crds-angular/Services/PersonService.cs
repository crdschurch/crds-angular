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


namespace crds_angular.Services
{
    public class PersonService : MinistryPlatformBaseService, IPersonService
    {
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

        public void SetProfile( Person person, string userAccessToken)
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
                var address = new AddressDTO(addressDictionary["Address_Line_1"].ToString(), "", addressDictionary["City"].ToString(),  addressDictionary["State"].ToString(),  addressDictionary["Postal_Code"].ToString(),null,null);
                var coordinates = _addressService.GetGeoLocationCascading(address);
                addressDictionary.Add("Latitude", coordinates.Latitude);
                addressDictionary.Add("Longitude", coordinates.Longitude);
            }

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
            //
            // update the user values if the email and/or password has changed
            if (!(String.IsNullOrEmpty(person.NewPassword)) || (person.EmailAddress != person.OldEmail && person.OldEmail != null))
            {
                var authData = _authenticationService.AuthenticateUser(person.OldEmail, person.OldPassword);

                if (authData == null)
                {
                    throw new Exception("Old password did not match profile");
                }
                else
                {                     
                    var userUpdateValues = new Dictionary<string, object>();
                    userUpdateValues["Display_Name"] = $"{person.LastName}, {person.NickName}";
                    _userRepository.UpdateUser(userUpdateValues);

                    UpdateOktaEmailAddressIfNeeded(person, userAccessToken);                    
                    UpdateOktaPasswordIfNeeded(person, userAccessToken);
                }
            }
            CaptureProfileAnalytics(person);
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

        private HttpResponseMessage PutToIdentityService(string apiEndpoint, string userAccessToken, JObject payload)
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, _identityServiceUrl + apiEndpoint);
            request.Headers.Add("Authorization", userAccessToken);
            request.Headers.Add("Accept", "application/json");            
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");            
            var response = client.SendAsync(request).Result;
            return response;            
        }

        private Boolean UpdateOktaEmailAddressIfNeeded(Person person, string userAccessToken)
        {
            if(person.EmailAddress != person.OldEmail && person.OldEmail != null)
            {                                                
                JObject payload = new JObject();
                payload.Add("newEmail", person.EmailAddress);
                PutToIdentityService(person.OldEmail + "/email", userAccessToken, payload);
                return true;
            }
            return false;
        }

        private Boolean UpdateOktaPasswordIfNeeded(Person person, string userAccessToken)
        {
            if(!(String.IsNullOrEmpty(person.NewPassword)))
            {                                                                        
                JObject payload = new JObject();
                payload.Add("newPassword", person.NewPassword);
                PutToIdentityService(person.EmailAddress + "/password", userAccessToken, payload);
                return true;
            }
            return false;
        }
    }
}
