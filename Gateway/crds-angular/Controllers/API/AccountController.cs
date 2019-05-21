using System;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class AccountController : MPAuth
    {      
        private readonly IAccountService _accountService;

        public AccountController(IAuthTokenExpiryService authTokenExpiryService, 
                                 IAccountService accountService, 
                                 IUserImpersonationService userImpersonationService, 
                                 IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _accountService = accountService;
        }

        [ResponseType(typeof(AccountInfo))]
        [VersionedRoute(template: "account", minimumVersion: "1.0.0")]
        [Route("account")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            
            return Authorized( token =>
            {
                try
                {
                    var info = _accountService.getAccountInfo(token);                    
                    return Ok(info);
                }
                catch (Exception e)
                {
                    const string msg = "AccountController: GET account Info -- ";
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                
            });
            
        }

        [VersionedRoute(template: "account/password", minimumVersion: "1.0.0")]
        [Route("account/password")]
        [HttpPost]
        public IHttpActionResult UpdatePassword([FromBody] NewPassword password)
        {

            return Authorized(token =>
            {
     
                if (_accountService.ChangePassword(token, password.password))
                {
                    return Ok();
                }
                return BadRequest();
            });

        }

        [VersionedRoute(template: "account", minimumVersion: "1.0.0")]
        [Route("account")]
        [HttpPost]
        public IHttpActionResult Post([FromBody]AccountInfo accountInfo)
        {

            return Authorized(token =>
            {   
                _accountService.SaveCommunicationPrefs(token, accountInfo);
                return Ok();
            });
            
        }

    }
}
