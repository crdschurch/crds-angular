using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Util;
using log4net;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.ApiVersioning;
using crds_angular.Services.Interfaces;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.PlatformService;

namespace crds_angular.Controllers.API
{
    public class ImageController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (ImageController));
        private readonly MPInterfaces.IMinistryPlatformService _mpService;
        private readonly IApiUserRepository _apiUserService;
        private readonly int _defaultContactId;

        public ImageController(IAuthTokenExpiryService authTokenExpiryService, 
                               MPInterfaces.IMinistryPlatformService mpService, 
                               IAuthenticationRepository authenticationService,
                               IApiUserRepository apiUserService, 
                               IUserImpersonationService userImpersonationService, 
                               IConfigurationWrapper configurationWrapper)
            : base(authTokenExpiryService, userImpersonationService, authenticationService)
        {
            _apiUserService = apiUserService;
            _mpService = mpService;

            _defaultContactId = configurationWrapper.GetConfigIntValue("DefaultProfileImageContactId");
        }

        private IHttpActionResult GetImage(int fileId, string fileName, string token)
        {
            var imageStream = _mpService.GetFile(fileId, token);
            if (imageStream == null)
            {
                return (RestHttpActionResult<ApiErrorDto>.WithStatus(HttpStatusCode.NotFound, new ApiErrorDto("No matching image found")));
            }

            HttpContext.Current.Response.Buffer = true;
            return (new FileResult(imageStream, fileName, null, false));
        }

        /// <summary>
        /// Retrieves an image given a file ID.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns>A byte stream?</returns>
        [VersionedRoute(template: "image/{fileId}", minimumVersion: "1.0.0")]
        [Route("image/{fileId:int}")]
        [HttpGet]
        [IgnoreClientApiKey]
        public IHttpActionResult GetImage(int fileId)
        {
            try
            {
                return (Authorized(token =>
                {
                    var imageDescription = _mpService.GetFileDescription(fileId, token);
                    return GetImage(fileId, imageDescription.FileName, token);
                }));
            }
            catch (Exception e)
            {
                _logger.Error("Error getting profile image", e);
                return (BadRequest());
            }
        }

        /// <summary>
        /// Retrieves a profile image given a contact ID. If the contact ID does not have an associated
        /// profile image, return the profile image for the Default contact.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="defaultIfMissing"></param>
        /// <returns>A byte stream?</returns>
        [VersionedRoute(template: "image/profile/{contactId}", minimumVersion: "1.0.0")]
        [Route("image/profile/{contactId:int}")]
        [HttpGet]
        [IgnoreClientApiKey]
        public IHttpActionResult GetProfileImage(int contactId, bool defaultIfMissing = true)
        {
            var apiToken = _apiUserService.GetDefaultApiClientToken();

            IHttpActionResult result = GetContactImage(contactId, apiToken);
            if (result is NotFoundResult && defaultIfMissing)
                result = GetContactImage(_defaultContactId, apiToken);

            return result;
        }

        private IHttpActionResult GetContactImage(int contactId, string token)
        {
            IHttpActionResult result = null;

            try
            {
                var files = _mpService.GetFileDescriptions("Contacts", contactId, token);
                var file = files.FirstOrDefault(f => f.IsDefaultImage);
                if (file != null)
                {
                    result = GetImage(file.FileId, file.FileName, token);
                }
            }
            catch (Exception)
            {
                // If the file is not present on the file system, GetImage() will throw an exception
                // but we want to treat that as "not found" instead of an exception
            }

            return result ?? NotFound();
        }

        /// <summary>
        /// Retrieves an image for a pledge campaign given a record ID.
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns>A byte stream?</returns>
        [VersionedRoute(template: "image/pledge-campaign/{recordId}", minimumVersion: "1.0.0")]
        [Route("image/pledgecampaign/{recordId:int}")]
        [HttpGet]
        [IgnoreClientApiKey]
        public IHttpActionResult GetCampaignImage(int recordId)
        {
            var token = _apiUserService.GetToken();
            var files = _mpService.GetFileDescriptions("Pledge_Campaigns", recordId, token);
            var file = files.FirstOrDefault(f => f.IsDefaultImage);
            return file != null ?
                GetImage(file.FileId, file.FileName, token) :
                (RestHttpActionResult<ApiErrorDto>.WithStatus(HttpStatusCode.NotFound, new ApiErrorDto("No campaign image found")));
        }

        [VersionedRoute(template: "image/profile", minimumVersion: "1.0.0")]
        [Route("image/profile")]
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            // this needs to happen prior to Authorized() because ReadAsStringAsync() will
            // sometimes deadlock if it's called syncronously
            var base64String = await Request.Content.ReadAsStringAsync();
            if (base64String.Length == 0)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, "Request did not specify a \"file\" for the profile image."));
            }

            return (Authorized(token =>
            {
                const string fileName = "profile.png";

                var contactId = _mpService.GetContactInfo(token).ContactId;
                var files = _mpService.GetFileDescriptions("MyContact", contactId, token);
                var file = files.FirstOrDefault(f => f.IsDefaultImage);

                var imageBytes = Convert.FromBase64String(base64String.Split(',')[1]);

                if (file!=null)
                {
                    _mpService.UpdateFile(
                        file.FileId,
                        fileName,
                        "Profile Image",
                        true,
                        -1,
                        imageBytes,
                        token
                        );
                }
                else
                {
                    _mpService.CreateFile(
                        "MyContact",
                        contactId,
                        fileName,
                        "Profile Image",
                        true,
                        -1,
                        imageBytes,
                        token
                        );
                }
                return (Ok());
            }));
        }
    }
}
