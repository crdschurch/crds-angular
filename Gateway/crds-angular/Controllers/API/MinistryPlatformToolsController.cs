using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Newtonsoft.Json;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class MinistryPlatformToolsController: MPAuth
    {
        private readonly ISelectionRepository _selectionService;

        public MinistryPlatformToolsController(IAuthTokenExpiryService authTokenExpiryService, 
                                               ISelectionRepository selectionService, 
                                               IUserImpersonationService userImpersonationService, 
                                               IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _selectionService = selectionService;
        }

        [HttpGet]
        [VersionedRoute(template: "mp-tools/selection/{selectionId}", minimumVersion: "1.0.0")]
        [Route("mptools/selection/{selectionId:regex(\\d+)}")]
        public IHttpActionResult GetPageSelectionRecordIds(int selectionId)
        {
            var response = Authorized(token =>
            {
                var selected = _selectionService.GetSelectionRecordIds(token, selectionId);
                if (selected == null || !selected.Any())
                {
                    return (RestHttpActionResult<SelectedRecords>.WithStatus(HttpStatusCode.NotFound, new SelectedRecords()));
                }
                var selectedRecords = new SelectedRecords
                {
                    RecordIds = selected
                };

                return (Ok(selectedRecords));
            });

            return (response);
        }
    }

    public class SelectedRecords
    {
        [JsonProperty("RecordIds", NullValueHandling = NullValueHandling.Ignore)]
        public IList<int> RecordIds { get; set; }
    }
}
