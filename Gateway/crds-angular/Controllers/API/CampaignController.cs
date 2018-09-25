using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using crds_angular.Models.Crossroads.Campaign;
using crds_angular.Security;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Exceptions;
using log4net;

namespace crds_angular.Controllers.API
{
    public class CampaignController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(CampaignController));

        private readonly ICampaignService _campaignService;

        public CampaignController(IAuthTokenExpiryService authTokenExpiryService, 
                                  ICampaignService campaignService,
                                  IUserImpersonationService userImpersonationService,
                                  IAuthenticationRepository authenticationRepository)
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _campaignService = campaignService;
        }

        /// <summary>
        /// Return summary information for the given Pledge Campaign.
        /// </summary>
        /// <param name="pledgeCampaignId">A pledge campaign Id</param>
        /// <returns>A <see cref="PledgeCampaignSummaryDto">PledgeCampaignSummaryDto</see> with summary information for the campaign. This will return a 404/Not Found if the pledge campaign Id could not be located.</returns>
        [ResponseType(typeof(List<PledgeCampaignSummaryDto>))]
        [VersionedRoute(template: "campaign/summary/{pledgeCampaignId}", minimumVersion: "1.0.0")]
        [Route("campaign/summary/{pledgeCampaignId}")]
        [HttpGet]
        public IHttpActionResult GetSummary(int pledgeCampaignId)
        {
            try
            {
                return Ok(_campaignService.GetSummary(pledgeCampaignId));
            }
            catch (PledgeCampaignNotFoundException e)
            {
                _logger.Error($"GetSummary: pledge campaign id not found: {pledgeCampaignId}");
                var apiError = new ApiErrorDto(e.Message, code: HttpStatusCode.NotFound);
                return ResponseMessage(apiError.HttpResponseMessage);
            }
            catch (Exception e)
            {
                _logger.Error($"GetSummary: failed to load pledge campaign id {pledgeCampaignId}", e);
                var apiError = new ApiErrorDto($"Failed to load data for pledge campaign {pledgeCampaignId}", code: HttpStatusCode.InternalServerError);
                return ResponseMessage(apiError.HttpResponseMessage);
            }
        }
    }
}