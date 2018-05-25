using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Controllers.API
{
    public class LocationController : MPAuth
    {
        private readonly ILocationService _locationService;

        public LocationController(IAuthTokenExpiryService authTokenExpiryService, 
                                  ILocationService locationService,
                                  IUserImpersonationService userImpersonationService,
                                  IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _locationService = locationService;
        }

        [ResponseType(typeof(IList<OrgLocation>))]
        [VersionedRoute(template: "locations", minimumVersion: "1.0.0")]
        [Route("locations")]
        public IHttpActionResult Get()
        {
            try
            {
                var locations = _locationService.GetAllCrossroadsLocations();
                return Ok(locations);
            }
            catch (Exception e)
            {
                const string msg = "LocationController: GET locations -- ";
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(IList<LocationProximityDto>))]
        [VersionedRoute(template: "locations/proximities", minimumVersion: "1.0.0")]
        [Route("locations/proximities")]
        public IHttpActionResult GetProximities(string origin)
        {
            try
            {
                var locationProximities = _locationService.GetDistanceToCrossroadsLocations(origin);
                return Ok(locationProximities);
            }
            catch (Exception e)
            {
                const string msg = "LocationController: GET locations proximities -- ";
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}
