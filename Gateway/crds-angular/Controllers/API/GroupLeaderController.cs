using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.GroupLeader;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class GroupLeaderController : MPAuth
    {
        private readonly IAuthTokenExpiryService _authTokenExpiryService;
        private readonly IGroupLeaderService _groupLeaderService;

        public GroupLeaderController(IAuthTokenExpiryService authTokenExpiryService,
                                     IGroupLeaderService groupLeaderService, 
                                     IUserImpersonationService userImpersonationService, 
                                     IAuthenticationRepository authenticationRepository) 
          : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _groupLeaderService = groupLeaderService;
        }

        [VersionedRoute(template: "group-leader/interested", minimumVersion: "1.0.0")]
        [HttpPost]
        public async Task<IHttpActionResult> InterestedInGroupLeadership()
        {
            return Authorized(token =>
            {
                try
                {
                    _groupLeaderService.SetInterested(token);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failed to start the application", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "group-leader/profile", minimumVersion: "1.0.0")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveProfile([FromBody] GroupLeaderProfileDTO profile)
        {
            if (ModelState.IsValid)
            {
                return Authorized(token =>
                {
                    try
                    {                                                
                        _groupLeaderService.SaveReferences(profile).Zip<int, IList<Unit>, int>(_groupLeaderService.SaveProfile(token, profile),
                                                     (int first, IList<Unit> second) => first).Wait();
                        
                        return Ok();
                    }
                    catch (Exception e)
                    {
                        var apiError = new ApiErrorDto("Saving Leader Profile failed:", e);
                        throw new HttpResponseException(apiError.HttpResponseMessage);
                    }
                });
            }
            var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.ErrorMessage);
            var dataError = new ApiErrorDto("Registration Data Invalid", new InvalidOperationException("Invalid Registration Data" + errors));
            throw new HttpResponseException(dataError.HttpResponseMessage);
        }

        [ResponseType(typeof(object))]
        [VersionedRoute(template: "group-leader/url-segment", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<IHttpActionResult> GetURLSegment()
        {
                try
                {
                    var urlSegment = _groupLeaderService.GetUrlSegment().Wait();
                    return Ok(new { url = urlSegment });
            }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Getting Url Segment Failed: ", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
        }

        [VersionedRoute(template: "group-leader/spiritual-growth", minimumVersion: "1.0.0")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveSpiritualGrowth([FromBody] SpiritualGrowthDTO spiritualGrowth)
        {
            if (ModelState.IsValid)
            {
                return Authorized(token =>
                {
                    try
                    {
                        _groupLeaderService.SaveSpiritualGrowth(spiritualGrowth)
                            .Concat(_groupLeaderService.SetApplied(token)).Wait();

                        _groupLeaderService.GetApplicationData(spiritualGrowth.ContactId).Subscribe((res) =>
                        {
                            if (((string)res["studentLeaderRequest"]).ToUpper() == "TRUE")
                            {
                                _groupLeaderService.SendStudentMinistryRequestEmail(res).Subscribe(CancellationToken.None);
                            }
                        });
                        return Ok();
                    }
                    catch (Exception e)
                    {
                        var apiError = new ApiErrorDto("Saving SpiritualGrowth failed:", e);
                        throw new HttpResponseException(apiError.HttpResponseMessage);
                    }
                });
            }
            var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.ErrorMessage);
            var dataError = new ApiErrorDto("Spiritual Growth Data Invalid", new InvalidOperationException("Invalid Spiritual Growth Data" + errors));
            throw new HttpResponseException(dataError.HttpResponseMessage);
        }

        [VersionedRoute(template: "group-leader/leader-status", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<IHttpActionResult> GetLeaderStatus()
        {
            return Authorized(token =>
            {
                try
                {
                    var status = _groupLeaderService.GetGroupLeaderStatus(token).Wait();
                    return Ok(new GroupLeaderStatusDTO
                    {
                        Status = status
                    });
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Getting group leader status failed: ", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}
