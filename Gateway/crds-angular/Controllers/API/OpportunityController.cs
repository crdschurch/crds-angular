using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Opportunity;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class OpportunityController : ImpersonateAuthBaseController
    {
        private readonly IOpportunityRepository _opportunityService;

        public OpportunityController(IAuthTokenExpiryService authTokenExpiryService, 
                                     IOpportunityRepository opportunityService, 
                                     IUserImpersonationService userImpersonationService, 
                                     IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _opportunityService = opportunityService;
        }

        [ResponseType(typeof (int))]
        [VersionedRoute(template: "opportunity/{opportunityId}", minimumVersion: "1.0.0")]
        [Route("opportunity/{opportunityId}")]
        public IHttpActionResult Post(int opportunityId, [FromBody] string stuff)
        {
            var comments = string.Format("Request on {0}", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            return Authorized(authDto =>
            {
                try
                {
                    var id = _opportunityService.RespondToOpportunity( opportunityId, comments, authDto.UserInfo.Mp.ContactId);
                    return Ok(id);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            });
        }

        [ResponseType(typeof (int))]
        [VersionedRoute(template: "opportunity/save-qualified-server", minimumVersion: "1.0.0")]
        [Route("opportunity/save-qualified-server")]
        public IHttpActionResult Post([FromBody] MpRespondToOpportunityDto opportunityResponse)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors)
                    .Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("POST Data Invalid",
                    new InvalidOperationException("Invalid POST Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            try
            {
                if (opportunityResponse.Participants.Count > 0)
                {
                    _opportunityService.RespondToOpportunity(opportunityResponse);
                }
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Opportunity POST failed", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
            return Ok();
        }

        [ResponseType(typeof (List<long>))]
        [VersionedRoute(template: "opportunity/get-all-opportunity-dates/{opportunityId}", minimumVersion: "1.0.0")]
        [Route("opportunity/getAllOpportunityDates/{opportunityId}")]
        public IHttpActionResult GetAllOpportunityDates(int opportunityId)
        {
            var oppDates = new List<long>();
            return Authorized(authDto =>
            { 
			
                var opportunities = _opportunityService.GetAllOpportunityDates(opportunityId);
                oppDates.AddRange(opportunities.Select(opp => opp.ToUnixTime()));
                return Ok(oppDates);
            });
        }

        [ResponseType(typeof (Dictionary<string, long>))]
        [VersionedRoute(template: "opportunity/get-last-opportunity-date/{opportunityId}", minimumVersion: "1.0.0")]
        [Route("opportunity/getLastOpportunityDate/{opportunityId}")]
        public IHttpActionResult GetLastOpportunityDate(int opportunityId)
        {
            return
                Authorized(
                   authDto =>
                        Ok(new Dictionary<string, long>
                        {
                            {"date", _opportunityService.GetLastOpportunityDate(opportunityId).ToUnixTime()}
                        }));
        }

        [ResponseType(typeof (OpportunityGroup))]
        [VersionedRoute(template: "opportunity/getGroupParticipantsForOpportunity/{opportunityId}", minimumVersion: "1.0.0")]
        [Route("opportunity/getGroupParticipantsForOpportunity/{opportunityId}")]
        public IHttpActionResult GetGroupParticipantsForOpportunity(int opportunityId)
        {
            return Authorized(authDto =>
            {
                var group = _opportunityService.GetGroupParticipantsForOpportunity(opportunityId);
                var oppGrp = Mapper.Map<MpGroup, OpportunityGroup>(group);
                return Ok(oppGrp);
            });
        }

        [ResponseType(typeof (OpportunityResponseDto))]
        [VersionedRoute(template: "opportunity/get-response-for-opportunity/{opportunityId}/{contactId}", minimumVersion: "1.0.0")]
        [Route("opportunity/getResponseForOpportunity/{opportunityId}/{contactId}")]
        public IHttpActionResult GetResponseForOpportunity(int opportunityId, int contactId)
        {
            try
            {
                var response = _opportunityService.GetOpportunityResponse(contactId, opportunityId);
                var mapped = Mapper.Map<MpResponse, OpportunityResponseDto>((MpResponse)response);
                return Ok(mapped);
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Get Response For Opportunity", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}
