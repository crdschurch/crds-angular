using System;
using System.Web.Http;
using crds_angular.Exceptions.Models;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class RoomController : MPAuth
    {
        private readonly IRoomService _roomService;

        public RoomController(IAuthTokenExpiryService authTokenExpiryService, 
                              IRoomService roomService, 
                              IUserImpersonationService userImpersonationService, 
                              IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _roomService = roomService;
        }

        [VersionedRoute(template: "room/layouts", minimumVersion: "1.0.0")]
        [Route("room/layouts")]
        public IHttpActionResult GetLayouts()
        {
            return Authorized(t =>
            {
                try
                {
                    var rooms = _roomService.GetRoomLayouts();

                    return Ok(rooms);
                }
                catch (Exception e)
                {
                    var msg = "Error getting Layouts ";
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}