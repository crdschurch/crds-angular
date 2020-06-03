using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using log4net;
using MinistryPlatform.Translation.Repositories.Interfaces;
using IPersonService = crds_angular.Services.Interfaces.IPersonService;
using IDonorService = crds_angular.Services.Interfaces.IDonorService;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace crds_angular.Controllers.API
{
    public class ProfileController : ImpersonateAuthBaseController
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPersonService _personService;
        private readonly IServeService _serveService;
        private readonly IDonorService _donorService;
        private readonly IUserImpersonationService _impersonationService;
        private readonly IAuthenticationRepository _authenticationService ;
        private readonly IUserRepository _userService;
        private readonly IContactRelationshipRepository _contactRelationshipService;
        private readonly List<int> _allowedAdminGetProfileRoles;

        public ProfileController(IAuthTokenExpiryService authTokenExpiryService, 
                                 IPersonService personService, 
                                 IServeService serveService, 
                                 IUserImpersonationService impersonationService, 
                                 IDonorService donorService, 
                                 IAuthenticationRepository authenticationService, 
                                 IUserRepository userService, 
                                 IContactRelationshipRepository contactRelationshipService, 
                                 IConfigurationWrapper config, 
                                 IUserImpersonationService userImpersonationService) 
            : base(authTokenExpiryService, userImpersonationService, authenticationService)
        {
            _personService = personService;
            _serveService = serveService;
            _impersonationService = impersonationService;
            _donorService = donorService;
            _authenticationService = authenticationService;
            _userService = userService;
            _contactRelationshipService = contactRelationshipService;
            _allowedAdminGetProfileRoles = config.GetConfigValue("AdminGetProfileRoles").Split(',').Select(int.Parse).ToList();
        }

        [RequiresAuthorization]
        [ResponseType(typeof (Person))]
        [VersionedRoute(template: "profile", minimumVersion: "1.0.0")]
        [Route("profile")]
        [HttpGet]
        public IHttpActionResult GetProfile([FromUri(Name = "impersonateDonorId")]int? impersonateDonorId = null)
        {
            return Authorized(authDTO =>
            {
                var impersonateUserId = impersonateDonorId == null ? string.Empty : _donorService.GetContactDonorForDonorId(impersonateDonorId.Value).Email;
                try
                {
                    var person = (impersonateDonorId != null)
                        ? _impersonationService.WithImpersonation(authDTO, impersonateUserId, () => _personService.GetPerson(authDTO.UserInfo.Mp.ContactId))
                        : _personService.GetPerson(authDTO.UserInfo.Mp.ContactId);
                    if (person == null)
                    {
                        return Unauthorized();
                    }
                    return Ok(person);
                }
                catch (UserImpersonationException e)
                {
                    return (e.GetRestHttpActionResult());
                }
            });
        }

        [ResponseType(typeof(Person))]
        [VersionedRoute(template: "profile/{contactId}", minimumVersion: "1.0.0")]
        [Route("profile/{contactId}")]
        [HttpGet]
        public IHttpActionResult GetProfile(int contactId)
        {
            IEnumerable<string> accessTokens;
            Request.Headers.TryGetValues("Authorization", out accessTokens);
            string accessToken = accessTokens == null ? string.Empty : accessTokens.FirstOrDefault();
            _logger.Info($"Request received at Group-Leader/Leader-Status endpoint with Authorization Header {accessToken}");
            return Authorized(authDTO =>
            {
                try
                {
                    // does the logged in user have permission to view this contact?
                    //TODO: Move this security logic to MP, if for some reason we absulutly can't then centerlize all security logic that exists in the gateway
                    var family = _serveService.GetImmediateFamilyParticipants(authDTO.UserInfo.Mp.ContactId);
                    Person person = null;

                    bool canImpersonate = authDTO.UserInfo.Mp.CanImpersonate.HasValue ?
                      authDTO.UserInfo.Mp.CanImpersonate.Value :
                      false;

                    if (family.Where(f => f.ContactId == contactId).ToList().Count > 0) // This is not an impersonation case
                    {
                        person = _personService.GetPerson(contactId);
                    }
                    else if (canImpersonate)
                    {
                        person = _personService.GetPerson(contactId);
                    }
                    else
                    {
                        return Unauthorized();
                    }

                    if (person == null)
                    {
                        return Unauthorized();
                    }

                    return Ok(person);
                }
                catch (Exception e)
                {
                    _logger.Info($"Exception happened when trying to get profile for contact {contactId}");
                    var apiError = new ApiErrorDto("Get Profile Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
        
        [ResponseType(typeof(Person))]
        [VersionedRoute(template: "profile/{contactId}/admin", minimumVersion: "1.0.0")]
        [Route("profile/{contactId}/admin")]
        [HttpGet]
        public IHttpActionResult AdminGetProfile(int contactId)
        {
            return Authorized(token =>
            {
                var user = _userService.GetByUserId(token.UserInfo.Mp.UserId.ToString());
                var roles = _userService.GetUserRoles(user.UserRecordId);
                if (roles == null || !roles.Exists(r => _allowedAdminGetProfileRoles.Contains(r.Id)))
                {
                    return Unauthorized();
                }

                try
                {
                    var person = _personService.GetPerson(contactId);

                    if (person == null)
                    {
                        return NotFound();
                    }
                    return Ok(person);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Get Profile Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(FamilyMember))]
        [VersionedRoute(template: "profile/{contactId}/spouse", minimumVersion: "1.0.0")]
        [Route("profile/{contactId}/spouse")]
        [HttpGet]
        public IHttpActionResult GetMySpouse(int contactid)
        {
            return Authorized(token =>
            {
                try
                {
                    var family = _contactRelationshipService.GetMyImmediateFamilyRelationships(contactid);
                    var spouse = family.Where(f => f.Relationship_Id == 1).Select(s => new FamilyMember
                    {
                        Age = s.Age,
                        ContactId = s.Contact_Id,
                        Email = s.Email_Address,
                        LastName = s.Last_Name,
                        PreferredName = s.Preferred_Name,
                        RelationshipId = s.Relationship_Id,
                        ParticipantId = s.Participant_Id,
                        HighSchoolGraduationYear = s.HighSchoolGraduationYear
                    }).SingleOrDefault();
                    return Ok(spouse);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Spouse Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "profile", minimumVersion: "1.0.0")]
        [Route("profile")]
        public IHttpActionResult Post([FromBody] Person person)
        {
            if (ModelState.IsValid)
            {
                return Authorized(t =>
                {
                    // does the logged in user have permission to view this contact?
                    var family = _serveService.GetImmediateFamilyParticipants(t.UserInfo.Mp.ContactId);

                    if (family.Where(f => f.ContactId == person.ContactId).ToList().Count > 0)
                    {
                        try
                        {
                            var userAccessToken = this.Request.Headers.Authorization.ToString();
                            _personService.SetProfile(person, userAccessToken);
                            return this.Ok();
                        }
                        catch (Exception ex)
                        {
                            LogProfileError(ex, person);
                            var apiError = new ApiErrorDto("Profile update Failed", ex);
                            throw new HttpResponseException(apiError.HttpResponseMessage);
                        }
                    }
                    else
                    {
                        return this.Unauthorized();
                    }
                });
            }
            var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
            var dataError = new ApiErrorDto("Save Profile Data Invalid", new InvalidOperationException("Invalid Save Data" + errors));
            throw new HttpResponseException(dataError.HttpResponseMessage);
        }

        private void LogProfileError(Exception exception, Person person)
        {
            int contactId = person?.ContactId ?? 0;
            _logger.Error($"Save Profile exception (ContactId = {contactId})", exception);

            // include profile data in error log (serialized json); ignore exceptions during serialization
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Error = (serializer, err) => err.ErrorContext.Handled = true,
                ContractResolver = new IgnoreAttributesContractResolver()
            };
            string json = JsonConvert.SerializeObject(person, settings);
            _logger.Error($"Save Profile data {json}");
        }

        // Exclude "attributeTypes" property when logging errors because it adds ~1MB to the json!
        protected class IgnoreAttributesContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyName == "attributeTypes")
                {
                    property.ShouldSerialize = instance => false;
                }

                return property;
            }
        }
    }
}
