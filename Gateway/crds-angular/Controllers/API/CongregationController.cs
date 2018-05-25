using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class CongregationController : MPAuth
    {
        private readonly ICongregationService _congregationService;

        public CongregationController(IAuthTokenExpiryService authTokenExpiryService, 
                                      ICongregationService congregationService, 
                                      IUserImpersonationService userImpersonationService, 
                                      IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _congregationService = congregationService;
        }

        [ResponseType(typeof (Congregation))]
        [VersionedRoute(template: "congregation/{congregationId}", minimumVersion: "1.0.0")]
        [Route("congregation/{congregationId}")]
        public IHttpActionResult Get(int congregationId)
        {
            return Authorized(t =>
            {
                try
                {
                    var congregation = _congregationService.GetCongregationById(congregationId);

                    return Ok(congregation);
                }
                catch (Exception e)
                {
                    var msg = "Error getting Congregation by congregationId " + congregationId;
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof (List<Room>))]
        [VersionedRoute(template: "congregation/{congregationId}/rooms", minimumVersion: "1.0.0")]
        [Route("congregation/{congregationId}/rooms")]
        public IHttpActionResult GetRooms(int congregationId, 
                                            [FromUri(Name="startDate")] DateTime startDate, 
                                            [FromUri(Name="endDate")]DateTime endDate)
        {
            return Authorized(t =>
            {
                try
                {

                    var rooms = _congregationService.GetRooms(congregationId, startDate, endDate);
                    return Ok(rooms);
                }
                catch (Exception e)
                {
                    var msg = "Error getting Rooms by congregationId " + congregationId;
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof (List<RoomEquipment>))]
        [VersionedRoute(template: "congregation/{congregationId}/equipment", minimumVersion: "1.0.0")]
        [Route("congregation/{congregationId}/equipment")]
        public IHttpActionResult GetEquipment(int congregationId)
        {
            return Authorized(t =>
            {
                try
                {
                    var equipment = _congregationService.GetEquipment(congregationId);
                    return Ok(equipment);
                }
                catch (Exception e)
                {
                    var msg = "Error getting Equipment by congregatioinId " + congregationId;
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}