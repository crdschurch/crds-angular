using System;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class DonorStatementController : ImpersonateAuthBaseController
    {
        private readonly IDonorStatementService _donorStatementService;

        public DonorStatementController(IAuthTokenExpiryService authTokenExpiryService, 
                                        IDonorStatementService donorStatementService, 
                                        IUserImpersonationService userImpersonationService, 
                                        IAuthenticationRepository authenticationRepository) 
          : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _donorStatementService = donorStatementService;
        }


        [ResponseType(typeof(DonorStatementDTO))]
        [VersionedRoute(template: "donor-statement", minimumVersion: "1.0.0")]
        [Route("donor-statement")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            return (Authorized(authDto =>
            {
                try
                {
                    var donorStatement = _donorStatementService.GetDonorStatement(authDto.UserInfo.Mp.ContactId);
                    return Ok(donorStatement);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Donor Statement", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            }));
        }

        [VersionedRoute(template: "donor-statement", minimumVersion: "1.0.0")]
        [Route("donor-statement")]
        [HttpPost]
        public IHttpActionResult Post(DonorStatementDTO donorStatement)
        {
            return (Authorized(authDto =>
            {
                try
                {
                    _donorStatementService.SaveDonorStatement(donorStatement);
                    return this.Ok();
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Save Donor Statement", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            }));
        }
    }
}
